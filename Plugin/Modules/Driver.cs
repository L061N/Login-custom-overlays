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
    public class Driver
    {
        public int EnterPitLapUnconfirmed { get; set; } = 0;
        public int EnterPitLap { get; set; } = 0;
        public DateTime InPitSince { get; set; } = DateTime.MinValue;
    }

    public class DriverModule : IPluginModule
    {
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(500);
        private TimeSpan _minTimeInPit = TimeSpan.FromMilliseconds(4000);
        private double _lastSessionTime = 0;
        private Session _sessionModule;

        public const int MaxDrivers = 64;
        public Dictionary<string, Driver> Drivers { get; private set; } = null;

        public DriverModule()
        {

        }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            _sessionModule = plugin.GetModule<Session>();
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
                _lastSessionTime = sessionTime;
                return;
            }

            _lastSessionTime = sessionTime;

            // Initialize
            if (Drivers == null)
            {
                Drivers = new Dictionary<string, Driver>();
            }

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
                        driver.EnterPitLapUnconfirmed = opponent.CurrentLap ?? 0;
                    }

                    // If they are in the pit for a very short time then we consider that a glitch in telemetry and ignore it.
                    if (driver.InPitSince > DateTime.MinValue &&
                        driver.InPitSince + _minTimeInPit < DateTime.Now)
                    {
                        driver.EnterPitLap = driver.EnterPitLapUnconfirmed;
                    }
                }
                else
                {
                    driver.InPitSince = DateTime.MinValue;
                }

                Drivers[opponent.CarNumber] = driver;
            }
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {

        }
    }
}
