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
using System.Linq;

namespace benofficial2.Plugin
{
    public class RawDriverInfo
    {
        // Index in the raw table AllSessionData["DriverInfo"]["Drivers"]
        public int driverInfoIdx = 0;
    }

    public class Driver
    {
        public int EnterPitLapUnconfirmed { get; set; } = -1;
        public int EnterPitLap { get; set; } = -1;
        public int ExitPitLap { get; set; } = -1;
        public bool OutLap { get; set; } = false;
        public DateTime InPitSince { get; set; } = DateTime.MinValue;
        public int QualPositionInClass { get; set; } = 0;
        public double QualFastestTime { get; set; } = 0;
        
    }

    public class DriverModule : IPluginModule
    {
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(500);
        private TimeSpan _minTimeInPit = TimeSpan.FromMilliseconds(2500);
        private double _lastSessionTime = 0;
        private Session _sessionModule;

        public const int MaxDrivers = 64;

        // Key is car number
        public Dictionary<string, Driver> Drivers { get; private set; } = null;

        // Key is carIdx
        public Dictionary<int, RawDriverInfo> DriverInfos { get; private set; } = null;

        public bool PlayerOutLap { get; internal set; } = false;
        public string PlayerNumber { get; internal set; } = "";
        public int PlayerPositionInClass { get; internal set; } = 0;
        public int PlayerLivePositionInClass { get; internal set; } = 0;

        public DriverModule()
        {

        }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            _sessionModule = plugin.GetModule<Session>();

            plugin.AttachDelegate(name: "Player.OutLap", valueProvider: () => PlayerOutLap);
            plugin.AttachDelegate(name: "Player.Number", valueProvider: () => PlayerNumber);
            plugin.AttachDelegate(name: "Player.PositionInClass", valueProvider: () => PlayerPositionInClass);
            plugin.AttachDelegate(name: "Player.LivePositionInClass", valueProvider: () => PlayerLivePositionInClass);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (DateTime.Now - _lastUpdateTime < _updateInterval) return;
            _lastUpdateTime = DateTime.Now;

            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            // Reset when changing/restarting session
            double sessionTime = 0;
            try { sessionTime = (double)raw.Telemetry["SessionTime"]; } catch { }
            if (sessionTime == 0 || sessionTime < _lastSessionTime)
            {
                Drivers = null;
                PlayerLivePositionInClass = 0;
                _lastSessionTime = sessionTime;
                return;
            }

            _lastSessionTime = sessionTime;

            // Initialize
            if (Drivers == null)
            {
                Drivers = new Dictionary<string, Driver>();
            }

            // TODO enable this if we want to show qual result before race start.
            //UpdateQualResult(ref data);

            for (int i = 0; i < data.NewData.Opponents.Count; i++)
            {
                Opponent opponent = data.NewData.Opponents[i];
                if (!Drivers.TryGetValue(opponent.CarNumber, out Driver driver))
                {
                    driver = new Driver();
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

                Drivers[opponent.CarNumber] = driver;

                if (opponent.IsPlayer)
                {
                    PlayerOutLap = driver.OutLap;
                    PlayerNumber = opponent.CarNumber;
                    PlayerPositionInClass = opponent.Position > 0 ? opponent.PositionInClass : 0;
                }
            }
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {

        }
        public static string FormatIRating(int iRating)
        {
            return ((double)iRating / 1000.0).ToString("0.0") + "k";
        }

        public static (string license, double rating) ParseLicenseString(string licenseString)
        {
            var parts = licenseString.Split(' ');
            return (parts[0], double.Parse(parts[1]));
        }
        public void UpdateQualResult(ref GameData data)
        {
            // Only needed before the race start to show qual position
            if (!(_sessionModule.Race && !_sessionModule.RaceStarted)) return;

            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            UpdateRawDriverInfo(ref data);

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

                if (!DriverInfos.TryGetValue(carIdx, out RawDriverInfo driverInfo))
                {
                    continue;
                }

                string carNumber = string.Empty;
                try { carNumber = raw.AllSessionData["DriverInfo"]["Drivers"][driverInfo.driverInfoIdx]["CarNumber"]; } catch { }
                if (carNumber == string.Empty) continue;

                if (!Drivers.TryGetValue(carNumber, out Driver driver))
                {
                    driver = new Driver();
                }

                driver.QualPositionInClass = positionInClass;
                driver.QualFastestTime = fastestTime;
                Drivers[carNumber] = driver;
            }
        }

        public void UpdateRawDriverInfo(ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            // Create a dictionary to cache the driverInfo index for each carIdx.
            // Because they will not always match; they can be different when server slots
            // are reclaimed by another driver.
            DriverInfos = new Dictionary<int, RawDriverInfo>();
            int driverCount = 0;
            try { driverCount = (int)raw.AllSessionData["DriverInfo"]["Drivers"].Count; } catch { }

            for (int i = 0; i < driverCount; i++)
            {
                int carIdx = -1;
                try { carIdx = int.Parse(raw.AllSessionData["DriverInfo"]["Drivers"][i]["CarIdx"]); } catch { }
                
                if (carIdx >= 0)
                {
                    RawDriverInfo driverInfo = new RawDriverInfo();
                    driverInfo.driverInfoIdx = i;
                    DriverInfos[carIdx] = driverInfo;
                }
            }
        }
    }
}
