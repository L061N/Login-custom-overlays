/*
    benofficial2's Official Overlays
    Copyright (C) 2025 benofficial2

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using GameReaderCommon;
using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace benofficial2.Plugin
{
    using OpponentsWithDrivers = List<(Opponent, Driver)>;

    public class AverageLapTime
    {
        private readonly Queue<TimeSpan> _lapTimes = new Queue<TimeSpan>();
        private readonly int _maxLapCount;
        private int _currentLap = -1;

        public AverageLapTime(int lapCount)
        {
            if (lapCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lapCount), "Lap count must be greater than zero.");
            }
            _maxLapCount = lapCount;
        }

        public void AddLapTime(int currentLap, TimeSpan lapTime)
        {
            if (currentLap == _currentLap)
                return;

            if (lapTime <= TimeSpan.Zero)
                return;

            if (_lapTimes.Count == _maxLapCount)
            {
                _lapTimes.Dequeue();
            }
            _lapTimes.Enqueue(lapTime);
            _currentLap = currentLap;
        }

        public TimeSpan GetAverageLapTime()
        {
            if (_lapTimes.Count == 0)
            {
                return TimeSpan.Zero;
            }

            long averageTicks = (long)_lapTimes.Average(ts => ts.Ticks);
            return new TimeSpan(averageTicks);
        }
    }

    public class Driver
    {
        // Index in the array AllSessionData["DriverInfo"]["Drivers"]
        public int DriverInfoIdx { get; set; } = -1;
        public int CarIdx { get; set; } = -1;
        public string CarId { get; set; } = "";
        public int FlairId { get; set; } = 0;
        public int EnterPitLapUnconfirmed { get; set; } = -1;
        public int EnterPitLap { get; set; } = -1;
        public int ExitPitLap { get; set; } = -1;
        public bool OutLap { get; set; } = false;
        public DateTime InPitSince { get; set; } = DateTime.MinValue;
        public int StintLap { get; set; } = 0;
        public int QualPositionInClass { get; set; } = 0;
        public double QualFastestTime { get; set; } = 0;
        public int LivePositionInClass { get; set; } = 0;
        public double LastCurrentLapHighPrecision { get; set; } = -1;
        public double CurrentLapHighPrecision { get; set; } = -1;
        public bool Towing { get; set; } = false;
        public DateTime TowingEndTime { get; set; } = DateTime.MinValue;
        public bool FinishedRace { get; set; } = false;
        public TimeSpan LastLapTime { get; set; } = TimeSpan.Zero;
        public TimeSpan BestLapTime { get; set; } = TimeSpan.Zero;
        public AverageLapTime AvgLapTime { get; set; } = new AverageLapTime(3);
        public int JokerLapsComplete { get; set; } = 0;
        public int SessionFlags { get; set; } = 0;
    }

    public class ClassLeaderboard
    {
        public LeaderboardCarClassDescription CarClassDescription { get; set; } = null;
        public OpponentsWithDrivers Drivers { get; set; } = new OpponentsWithDrivers();
    }

    public class DriverModule : PluginModuleBase
    {
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(500);
        private TimeSpan _minTimeInPit = TimeSpan.FromMilliseconds(2500);

        private SessionModule _sessionModule = null;
        private CarModule _carModule = null;
        private FlairModule _flairModule = null;

        private SessionState _sessionState = new SessionState();

        public const int MaxDrivers = 64;

        // Key is car number
        public Dictionary<string, Driver> Drivers { get; private set; } = new Dictionary<string, Driver>();

        // Key is CarIdx
        public Dictionary<int, Driver> DriversByCarIdx { get; private set; } = new Dictionary<int, Driver>();

        public bool PlayerOutLap { get; internal set; } = false;
        public string PlayerNumber { get; internal set; } = "";
        public string PlayerCarBrand { get; internal set; } = "";
        public string PlayerCountryCode { get; internal set; } = "";
        public int PlayerPositionInClass { get; internal set; } = 0;
        public int PlayerLivePositionInClass { get; internal set; } = 0;
        public bool PlayerHadWhiteFlag { get; internal set; } = false;
        public TimeSpan PlayerLastLapTime { get; internal set; } = TimeSpan.Zero;

        public List<ClassLeaderboard> LiveClassLeaderboards { get; private set; } = new List<ClassLeaderboard>();
        public override int UpdatePriority => 30;
        public override void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            _sessionModule = plugin.GetModule<SessionModule>();
            _carModule = plugin.GetModule<CarModule>();
            _flairModule = plugin.GetModule<FlairModule>();

            plugin.AttachDelegate(name: "Player.OutLap", valueProvider: () => PlayerOutLap);
            plugin.AttachDelegate(name: "Player.Number", valueProvider: () => PlayerNumber);
            plugin.AttachDelegate(name: "Player.CarBrand", valueProvider: () => PlayerCarBrand);
            plugin.AttachDelegate(name: "Player.CountryCode", valueProvider: () => PlayerCountryCode);
            plugin.AttachDelegate(name: "Player.PositionInClass", valueProvider: () => PlayerPositionInClass);
            plugin.AttachDelegate(name: "Player.LivePositionInClass", valueProvider: () => PlayerLivePositionInClass);
            plugin.AttachDelegate(name: "Player.LastLapTime", valueProvider: () => PlayerLastLapTime);
        }

        public override void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (data.FrameTime - _lastUpdateTime < _updateInterval) return;
            _lastUpdateTime = data.FrameTime;

            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            _sessionState.Update(ref data);

            // Reset when changing/restarting session
            if (_sessionState.SessionChanged)
            {
                Drivers = new Dictionary<string, Driver>();
                DriversByCarIdx = new Dictionary<int, Driver>();
                PlayerOutLap = false;
                PlayerNumber = "";
                PlayerCarBrand = "";
                PlayerCountryCode = "";
                PlayerPositionInClass = 0;
                PlayerLivePositionInClass = 0;
                PlayerHadWhiteFlag = false;
                LiveClassLeaderboards = new List<ClassLeaderboard>();
                PlayerLastLapTime = TimeSpan.Zero;
            }

            UpdateDrivers(ref data);

            // Update lap times for all drivers based on the session results.
            // Do this after first trying to get the times from telemetry. 
            // Because lap times will be invalid in telemetry after the driver diconnected or exited the car.
            UpdateLapTimesFromSessionResults(ref data);

            for (int i = 0; i < data.NewData.Opponents.Count; i++)
            {
                Opponent opponent = data.NewData.Opponents[i];
                if (!Drivers.TryGetValue(opponent.CarNumber, out Driver driver))
                {
                    Debug.Assert(false);
                    continue;
                }

                // Evaluate the lap when they entered the pit lane
                if (opponent.IsCarInPitLane)
                {
                    // Remember when they entered the pit.
                    if (driver.InPitSince == DateTime.MinValue)
                    {
                        driver.InPitSince = DateTime.Now;
                        driver.EnterPitLapUnconfirmed = opponent.CurrentLap ?? -1;
                    }

                    // If they are in the pit for a very short time then we consider that a glitch in telemetry and ignore it.
                    if (driver.InPitSince > DateTime.MinValue &&
                        driver.InPitSince + _minTimeInPit < DateTime.Now)
                    {
                        driver.EnterPitLap = driver.EnterPitLapUnconfirmed;
                    }

                    driver.OutLap = false;
                    driver.ExitPitLap = -1;
                    driver.StintLap = 0;
                }
                else
                {
                    // If they are in the pit for a very short time then we consider that a glitch in telemetry and ignore it.
                    // Ignore pit exit before the race start.
                    if (opponent.IsConnected &&
                        driver.InPitSince > DateTime.MinValue &&
                        !(_sessionModule.Race && !_sessionModule.RaceStarted) &&
                        driver.InPitSince + _minTimeInPit < DateTime.Now)
                    {
                        driver.ExitPitLap = opponent.CurrentLap ?? -1;

                        // Edge case when the pit exit is before the finish line.
                        // The currentLap will increment, so consider the next lap an out lap too.
                        if (opponent.TrackPositionPercent > 0.5)
                        {
                            driver.ExitPitLap++;
                        }
                    }

                    driver.OutLap = opponent.IsConnected && driver.ExitPitLap >= opponent.CurrentLap;
                    driver.InPitSince = DateTime.MinValue;

                    if (driver.ExitPitLap >= 0)
                    {
                        driver.StintLap = (opponent.CurrentLap ?? 0) - driver.ExitPitLap + 1;
                    }
                    else if (_sessionModule.Race && !_sessionModule.JoinedRaceInProgress)
                    {
                        // When we join a race session in progress, we cannot know when the driver exited the pit, so StintLap should stay 0.
                        driver.StintLap = opponent.CurrentLap ?? 0;
                    }
                }

                if (_sessionModule.Race)
                {
                    double playerCarTowTime = 0;
                    try { playerCarTowTime = (double)raw.Telemetry["PlayerCarTowTime"]; } catch { }

                    if (!driver.Towing)
                    {
                        // Check for a jump in continuity, this means the driver teleported (towed) back to the pit.
                        if (driver.CurrentLapHighPrecision > -1 && 
                            opponent.CurrentLapHighPrecision.HasValue && opponent.CurrentLapHighPrecision.Value > -1)
                        {
                            // Use avg speed because in SimHub we can step forward in time in a recorded replay.
                            double avgSpeedKph = ComputeAvgSpeedKph(data.NewData.TrackLength, driver.CurrentLapHighPrecision, opponent.CurrentLapHighPrecision.Value, _sessionState.DeltaTime);
                            bool teleportingToPit = avgSpeedKph > 500 && opponent.IsCarInPit;
                            bool playerTowing = opponent.IsPlayer && playerCarTowTime > 0;

                            if (playerTowing || teleportingToPit)
                            {
                                driver.Towing = true;

                                if (opponent.IsPlayer)
                                {
                                    driver.TowingEndTime = DateTime.Now + TimeSpan.FromSeconds(playerCarTowTime);
                                }
                                else
                                {
                                    (double towLength, TimeSpan towTime) = ComputeTowLengthAndTime(data.NewData.TrackLength, driver.CurrentLapHighPrecision, opponent.CurrentLapHighPrecision.Value);
                                    driver.TowingEndTime = DateTime.Now + towTime;
                                }
                            }
                        }
                    }
                    else
                    {
                        // iRacing doesn't provide a tow time for other drivers, so we have to estimate it.
                        // Consider towing done if the car starts moving forward from a valid position
                        double smallDistancePct = 0.05 / data.NewData.TrackLength; // 0.05m is roughly the distance you cover at 10km/h in 16ms.

                        bool movingForward = opponent.CurrentLapHighPrecision.HasValue &&
                            opponent.CurrentLapHighPrecision.Value > -1 &&
                            driver.LastCurrentLapHighPrecision > -1 &&
                            opponent.CurrentLapHighPrecision > driver.LastCurrentLapHighPrecision + smallDistancePct;

                        bool done = opponent.CurrentLapHighPrecision == -1;
                        bool towEnded = !opponent.IsPlayer && DateTime.Now > driver.TowingEndTime;
                        bool playerNotTowing = opponent.IsPlayer && playerCarTowTime <= 0;
                        if (playerNotTowing || towEnded || movingForward || done)
                        {
                            driver.Towing = false;
                            driver.TowingEndTime = DateTime.MinValue;
                        }
                    }

                    // Pause updating the current lap if the driver is towing, so they stay at their last "on-track" position in the live standings.
                    // Otherwide they would leapfrog the leaders as they teleport in the pit.
                    if (!driver.Towing)
                    {
                        // Stop updating the current lap if the driver is done (-1), so they stay at their last known position in the live standings.
                        // Happens at the end of the race when they get out of the car.
                        if (opponent.CurrentLapHighPrecision.HasValue && opponent.CurrentLapHighPrecision.Value > -1)
                        {
                            driver.CurrentLapHighPrecision = opponent.CurrentLapHighPrecision.Value;
                        }
                    }

                    driver.LastCurrentLapHighPrecision = opponent.CurrentLapHighPrecision ?? -1;
                }

                // Update the average lap time for the driver
                int currentLap = opponent.CurrentLap ?? -1;
                driver.AvgLapTime.AddLapTime(currentLap, driver.LastLapTime);

                if (opponent.IsPlayer)
                {
                    PlayerOutLap = driver.OutLap;
                    PlayerNumber = opponent.CarNumber;
                    PlayerCarBrand = _carModule.GetCarBrand(driver.CarId, opponent.CarName);
                    PlayerCountryCode = _flairModule.GetCountryCode(driver.FlairId);
                    PlayerPositionInClass = opponent.Position > 0 ? opponent.PositionInClass : 0;
                    PlayerLastLapTime = driver.LastLapTime;

                    if (_sessionModule.Race)
                    {
                        PlayerHadWhiteFlag = PlayerHadWhiteFlag || data.NewData.Flag_White == 1;
                    }
                }
            }

            UpdateQualResult(ref data);
            UpdateLivePositionInClass(ref data);
        }

        public override void End(PluginManager pluginManager, benofficial2 plugin)
        {

        }

        private double ComputeAvgSpeedKph(double trackLength, double fromPos, double toPos, TimeSpan deltaTime)
        {
            if (deltaTime <= TimeSpan.Zero) return 0;
            double deltaPos = Math.Abs(toPos - fromPos);
            double length = deltaPos * trackLength;
            return (length / 1000) / (deltaTime.TotalSeconds / 3600);
        }

        private (double, TimeSpan) ComputeTowLengthAndTime(double trackLength, double fromPos, double toPos)
        {
            double deltaPos;
            if (toPos < fromPos)
            {
                // Must drive around the track
                deltaPos = 1.0 - fromPos + toPos;
            }
            else
            {
                deltaPos = toPos - fromPos;
            }
                
            double length = deltaPos * trackLength;
            const double towSpeedMs = 30;
            const double towTimeFixed = 50;
            return (length, TimeSpan.FromSeconds(length / towSpeedMs + towTimeFixed));
        }

        public static (string license, double rating) ParseLicenseString(string licenseString)
        {
            var parts = licenseString.Split(' ');
            return (parts[0], double.Parse(parts[1]));
        }

        private void UpdateQualResult(ref GameData data)
        {
            // Only needed before the race start to show qual position
            if (!(_sessionModule.Race && !_sessionModule.RaceStarted)) return;

            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            int resultCount = 0;
            try { resultCount = (int)raw.AllSessionData["QualifyResultsInfo"]["Results"].Count; } catch { }

            for (int i = 0; i < resultCount; i++)
            {
                int carIdx = 0;
                try { carIdx = int.Parse(raw.AllSessionData["QualifyResultsInfo"]["Results"][i]["CarIdx"]); } catch { }

                if (!DriversByCarIdx.TryGetValue(carIdx, out Driver driver))
                {
                    Debug.Assert(false);
                    continue;
                }

                int positionInClass = 0;
                try { positionInClass = int.Parse(raw.AllSessionData["QualifyResultsInfo"]["Results"][i]["ClassPosition"]) + 1; } catch { }

                double fastestTime = 0;
                try { fastestTime = double.Parse(raw.AllSessionData["QualifyResultsInfo"]["Results"][i]["FastestTime"]); } catch { }

                driver.QualPositionInClass = positionInClass;
                driver.QualFastestTime = fastestTime;
            }
        }

        private void UpdateDrivers(ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;
            
            int driverCount = 0;
            try { driverCount = (int)raw.AllSessionData["DriverInfo"]["Drivers"].Count; } catch { Debug.Assert(false); }

            for (int i = 0; i < driverCount; i++)
            {
                int carIdx = -1;
                try { carIdx = int.Parse(raw.AllSessionData["DriverInfo"]["Drivers"][i]["CarIdx"]); } catch { Debug.Assert(false); }

                string carNumber = string.Empty;
                try { carNumber = raw.AllSessionData["DriverInfo"]["Drivers"][i]["CarNumber"]; } catch { Debug.Assert(false); }

                string carPath = string.Empty;
                try { carPath = raw.AllSessionData["DriverInfo"]["Drivers"][i]["CarPath"]; } catch { Debug.Assert(false); }

                RawDataHelper.TryGetSessionData<int>(ref data, out int flairId, "DriverInfo", "Drivers", i, "FlairID");

                double lastLapTime = 0;
                try { lastLapTime = Math.Max(0, (double)raw.Telemetry["CarIdxLastLapTime"][carIdx]); } catch { Debug.Assert(false); }

                double bestLapTime = 0;
                try { bestLapTime = Math.Max(0, (double)raw.Telemetry["CarIdxBestLapTime"][carIdx]); } catch { Debug.Assert(false); }

                RawDataHelper.TryGetTelemetryData<int>(ref data, out int sessionFlags, "CarIdxSessionFlags", carIdx);

                if (carIdx >= 0 && carNumber.Length > 0)
                {
                    if (!Drivers.TryGetValue(carNumber, out Driver driver))
                    {
                        driver = new Driver();
                        Drivers[carNumber] = driver;
                        DriversByCarIdx[carIdx] = driver;
                    }

                    driver.DriverInfoIdx = i;
                    driver.CarIdx = carIdx;
                    driver.CarId = carPath;
                    driver.FlairId = flairId;
                    driver.LastLapTime = lastLapTime > 0 ? TimeSpan.FromSeconds(lastLapTime) : TimeSpan.Zero;
                    driver.BestLapTime = bestLapTime > 0 ? TimeSpan.FromSeconds(bestLapTime) : TimeSpan.Zero;
                    driver.SessionFlags = sessionFlags;
                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }

        private void UpdateLivePositionInClass(ref GameData data)
        {
            LiveClassLeaderboards = new List<ClassLeaderboard>();

            for (int carClassIdx = 0; carClassIdx < data.NewData.OpponentsClassses.Count; carClassIdx++)
            {
                ClassLeaderboard leaderboard = new ClassLeaderboard();
                LiveClassLeaderboards.Add(leaderboard);

                leaderboard.CarClassDescription = data.NewData.OpponentsClassses[carClassIdx];
                List<Opponent> opponents = leaderboard.CarClassDescription.Opponents;
                for (int i = 0; i < opponents.Count; i++)
                {
                    Opponent opponent = opponents[i];
                    if (!Drivers.TryGetValue(opponent.CarNumber, out Driver driver))
                    {
                        Debug.Assert(false);
                        continue;
                    }

                    leaderboard.Drivers.Add((opponent, driver));
                }

                if (_sessionModule.Race)
                {
                    if (!_sessionModule.RaceStarted)
                    {
                        // Before the start keep the leaderboard sorted by qual position
                        leaderboard.Drivers = leaderboard.Drivers.OrderBy(p => p.Item2.QualPositionInClass).ToList();
                    }
                    else if (!_sessionModule.RaceFinished)
                    {
                        // During the race sort on position on track for a live leaderboard.
                        // Except for ovals under caution, show the official position.
                        if (!(_sessionModule.Oval && data.NewData.Flag_Yellow == 1))
                        {
                            leaderboard.Drivers = leaderboard.Drivers.OrderByDescending(p => p.Item2.CurrentLapHighPrecision).ToList();
                        }
                    }
                    else
                    {
                        // After the race don't sort to show the official race result
                    }
                }

                for (int i = 0; i < leaderboard.Drivers.Count; i++)
                {
                    Opponent opponent = leaderboard.Drivers[i].Item1;
                    Driver driver = leaderboard.Drivers[i].Item2;

                    if (_sessionModule.Race)
                    {
                        if (!_sessionModule.RaceStarted)
                        {
                            driver.LivePositionInClass = driver.QualPositionInClass;
                        }
                        else
                        {
                            driver.LivePositionInClass = i + 1;
                        }
                    }
                    else
                    {
                        driver.LivePositionInClass = opponent.Position > 0 ? i + 1 : 0;
                    }

                    if (opponent.IsPlayer)
                    {
                        PlayerLivePositionInClass = driver.LivePositionInClass;
                    }
                }
            }
        }

        private void UpdateLapTimesFromSessionResults(ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            // It can happen that CurrentSessionNum is missing on SessionInfo. We can't tell which session to use in that case.
            int sessionInfoCount = -1;
            try { sessionInfoCount = raw.AllSessionData["SessionInfo"].Count; } catch { Debug.Assert(false); }
            if (sessionInfoCount <= 1) return;

            int sessionIdx = -1;
            try { sessionIdx = int.Parse(raw.AllSessionData["SessionInfo"]["CurrentSessionNum"]); } catch { Debug.Assert(false); }
            if (sessionIdx < 0) return;

            List<object> positions = null;
            try { positions = raw.AllSessionData["SessionInfo"]["Sessions"][sessionIdx]["ResultsPositions"]; } catch { Debug.Assert(false); }
            if (positions == null) return;

            for (int posIdx = 0; posIdx < positions.Count; posIdx++)
            {
                int carIdx = -1;
                try { carIdx = int.Parse(raw.AllSessionData["SessionInfo"]["Sessions"][sessionIdx]["ResultsPositions"][posIdx]["CarIdx"]); } catch { Debug.Assert(false); }
                if (carIdx < 0) continue;

                if (!DriversByCarIdx.TryGetValue(carIdx, out Driver driver))
                {
                    Debug.Assert(false);
                    continue;
                }

                double bestLapTime = 0;
                try { bestLapTime = double.Parse(raw.AllSessionData["SessionInfo"]["Sessions"][sessionIdx]["ResultsPositions"][posIdx]["FastestTime"]); } catch { Debug.Assert(false); }

                if (driver.BestLapTime == TimeSpan.Zero && bestLapTime > 0)
                {
                    driver.BestLapTime = TimeSpan.FromSeconds(bestLapTime);
                }

                double lastLapTime = 0;
                try { lastLapTime = double.Parse(raw.AllSessionData["SessionInfo"]["Sessions"][sessionIdx]["ResultsPositions"][posIdx]["LastTime"]); } catch { Debug.Assert(false); }

                if (driver.LastLapTime == TimeSpan.Zero && lastLapTime > 0)
                {
                    driver.LastLapTime = TimeSpan.FromSeconds(lastLapTime);
                }

                int jokerLapsComplete = 0;
                try { jokerLapsComplete = int.Parse(raw.AllSessionData["SessionInfo"]["Sessions"][sessionIdx]["ResultsPositions"][posIdx]["JokerLapsComplete"]); } catch { Debug.Assert(false); }
                driver.JokerLapsComplete = jokerLapsComplete;
            }
        }
    }
}
