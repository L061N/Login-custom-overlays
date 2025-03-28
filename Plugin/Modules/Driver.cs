﻿/*
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
using System.Linq;

namespace benofficial2.Plugin
{
    using OpponentsWithDrivers = List<(Opponent, Driver)>;

    public class Driver
    {
        public int CarIdx { get; set; } = -1;
        public string CarId { get; set; } = "";
        public int EnterPitLapUnconfirmed { get; set; } = -1;
        public int EnterPitLap { get; set; } = -1;
        public int ExitPitLap { get; set; } = -1;
        public bool OutLap { get; set; } = false;
        public DateTime InPitSince { get; set; } = DateTime.MinValue;
        public int QualPositionInClass { get; set; } = 0;
        public double QualFastestTime { get; set; } = 0;
        public int LivePositionInClass { get; set; } = 0;
        public double LastCurrentLapHighPrecision { get; set; } = -1;
        public double CurrentLapHighPrecision { get; set; } = -1;
        public bool Towing { get; set; } = false;
        public DateTime TowingEndTime { get; set; } = DateTime.MinValue;
        public bool FinishedRace { get; set; } = false;
    }

    public class ClassLeaderboard
    {
        public LeaderboardCarClassDescription CarClassDescription { get; set; } = null;
        public OpponentsWithDrivers Drivers { get; set; } = new OpponentsWithDrivers();
    }

    public class DriverModule : PluginModuleBase
    {
        private double _lastSessionTime = double.MaxValue;
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(500);
        private TimeSpan _minTimeInPit = TimeSpan.FromMilliseconds(2500);
        private SessionModule _sessionModule;
        private CarModule _carModule = null;

        public const int MaxDrivers = 64;

        // Key is car number
        public Dictionary<string, Driver> Drivers { get; private set; } = new Dictionary<string, Driver>();

        // Dictionary to cache the driverInfo index for each carIdx. Because they might not always match.
        // Key is carIdx, value is index in the raw table AllSessionData["DriverInfo"]["Drivers"]
        public Dictionary<int, int> DriverInfoIndexes { get; private set; } = new Dictionary<int, int>();

        public bool PlayerOutLap { get; internal set; } = false;
        public string PlayerNumber { get; internal set; } = "";
        public string PlayerCarBrand { get; internal set; } = "";
        public int PlayerPositionInClass { get; internal set; } = 0;
        public int PlayerLivePositionInClass { get; internal set; } = 0;
        public bool PlayerHadWhiteFlag { get; internal set; } = false;

        public List<ClassLeaderboard> LiveClassLeaderboards { get; private set; } = new List<ClassLeaderboard>();
        public override int UpdatePriority => 30;
        public override void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            _sessionModule = plugin.GetModule<SessionModule>();
            _carModule = plugin.GetModule<CarModule>();

            plugin.AttachDelegate(name: "Player.OutLap", valueProvider: () => PlayerOutLap);
            plugin.AttachDelegate(name: "Player.Number", valueProvider: () => PlayerNumber);
            plugin.AttachDelegate(name: "Player.CarBrand", valueProvider: () => PlayerCarBrand);
            plugin.AttachDelegate(name: "Player.PositionInClass", valueProvider: () => PlayerPositionInClass);
            plugin.AttachDelegate(name: "Player.LivePositionInClass", valueProvider: () => PlayerLivePositionInClass);
        }

        public override void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (data.FrameTime - _lastUpdateTime < _updateInterval) return;
            _lastUpdateTime = data.FrameTime;

            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            bool sessionChanged = (_sessionModule.SessionTime == 0 || _sessionModule.SessionTime < _lastSessionTime);
            double deltaTime = Math.Max(_sessionModule.SessionTime - _lastSessionTime, 0);
            _lastSessionTime = _sessionModule.SessionTime;

            // Reset when changing/restarting session
            if (sessionChanged)
            {
                Drivers = new Dictionary<string, Driver>();
                DriverInfoIndexes = new Dictionary<int, int>();
                PlayerOutLap = false;
                PlayerNumber = "";
                PlayerCarBrand = "";
                PlayerPositionInClass = 0;
                PlayerLivePositionInClass = 0;
                PlayerHadWhiteFlag = false;
                LiveClassLeaderboards = new List<ClassLeaderboard>();
            }

            UpdateRawDriverInfo(ref data);

            for (int i = 0; i < data.NewData.Opponents.Count; i++)
            {
                Opponent opponent = data.NewData.Opponents[i];
                if (!Drivers.TryGetValue(opponent.CarNumber, out Driver driver))
                {
                    driver = new Driver();
                    Drivers[opponent.CarNumber] = driver;
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
                }
                else
                {
                    // If they are in the pit for a very short time then we consider that a glitch in telemetry and ignore it.
                    // Ignore pit exit before the race start.
                    if (driver.InPitSince > DateTime.MinValue &&
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

                    driver.OutLap = driver.ExitPitLap >= opponent.CurrentLap;
                    driver.InPitSince = DateTime.MinValue;
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
                            double avgSpeedKph = ComputeAvgSpeedKph(data.NewData.TrackLength, driver.CurrentLapHighPrecision, opponent.CurrentLapHighPrecision.Value, deltaTime);
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

                if (opponent.IsPlayer)
                {
                    PlayerOutLap = driver.OutLap;
                    PlayerNumber = opponent.CarNumber;
                    PlayerCarBrand = _carModule.GetCarBrand(driver.CarId, opponent.CarName);
                    PlayerPositionInClass = opponent.Position > 0 ? opponent.PositionInClass : 0;

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

        private double ComputeAvgSpeedKph(double trackLength, double fromPos, double toPos, double deltaTime)
        {
            if (deltaTime <= 0) return 0;
            double deltaPos = Math.Abs(toPos - fromPos);
            double length = deltaPos * trackLength;
            return (length / 1000) / (deltaTime / 3600);
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

                int positionInClass = 0;
                try { positionInClass = int.Parse(raw.AllSessionData["QualifyResultsInfo"]["Results"][i]["ClassPosition"]) + 1; } catch { }

                double fastestTime = 0;
                try { fastestTime = double.Parse(raw.AllSessionData["QualifyResultsInfo"]["Results"][i]["FastestTime"]); } catch { }

                if (!DriverInfoIndexes.TryGetValue(carIdx, out int driverInfoIdx))
                {
                    continue;
                }

                string carNumber = string.Empty;
                try { carNumber = raw.AllSessionData["DriverInfo"]["Drivers"][driverInfoIdx]["CarNumber"]; } catch { }
                if (carNumber == string.Empty) continue;

                if (!Drivers.TryGetValue(carNumber, out Driver driver))
                {
                    driver = new Driver();
                    Drivers[carNumber] = driver;
                }

                driver.QualPositionInClass = positionInClass;
                driver.QualFastestTime = fastestTime;
            }
        }

        private void UpdateRawDriverInfo(ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;
            
            int driverCount = 0;
            try { driverCount = (int)raw.AllSessionData["DriverInfo"]["Drivers"].Count; } catch { }

            for (int i = 0; i < driverCount; i++)
            {
                int carIdx = -1;
                try { carIdx = int.Parse(raw.AllSessionData["DriverInfo"]["Drivers"][i]["CarIdx"]); } catch { }

                string carNumber = string.Empty;
                try { carNumber = raw.AllSessionData["DriverInfo"]["Drivers"][i]["CarNumber"]; } catch { }

                string carPath = string.Empty;
                try { carPath = raw.AllSessionData["DriverInfo"]["Drivers"][i]["CarPath"]; } catch { }

                if (carIdx >= 0 && carNumber.Length > 0)
                {
                    if (!Drivers.TryGetValue(carNumber, out Driver driver))
                    {
                        driver = new Driver();
                        Drivers[carNumber] = driver;
                    }

                    driver.CarIdx = carIdx;
                    driver.CarId = carPath;
                    DriverInfoIndexes[carIdx] = i;
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
                        driver = new Driver();
                        Drivers[opponent.CarNumber] = driver;
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
    }
}
