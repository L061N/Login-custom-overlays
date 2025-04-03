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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace benofficial2.Plugin
{
    public class FuelCalcSettings : ModuleSettings
    {
        public int BackgroundOpacity { get; set; } = 60;
        public ModuleSettingFloat FuelReserveLiters { get; set; } = new ModuleSettingFloat(0.5f);
        public ModuleSettingFloat ExtraRaceLaps { get; set; } = new ModuleSettingFloat(0.0f);
        public ModuleSettingFloat ExtraConsumption { get; set; } = new ModuleSettingFloat(1.0f);
        public bool EnablePreRaceWarning { get; set; } = true;

        // Legacy properties for backwards compatibility (saved pre 3.0)
        public string FuelReserveString { get => FuelReserveLiters.ValueString; set => FuelReserveLiters.ValueString = value; }
        public string ExtraLapsString { get => ExtraRaceLaps.ValueString; set => ExtraRaceLaps.ValueString = value; }
        public string ExtraConsumptionPctString { get => ExtraConsumption.ValueString; set => ExtraConsumption.ValueString = value; }
    }

    public class FuelCalcModule : PluginModuleBase
    {
        private DriverModule _driverModule = null;

        private DateTime _lastUpdateTime = DateTime.MinValue;
        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(1000);

        public FuelCalcSettings Settings { get; set; }
        public bool Visible { get; internal set; } = true;
        public TimeSpan BestLapTime { get; internal set; } = TimeSpan.Zero;

        public override void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            _driverModule = plugin.GetModule<DriverModule>();

            Settings = plugin.ReadCommonSettings<FuelCalcSettings>("FuelCalcSettings", () => new FuelCalcSettings());
            plugin.AttachDelegate(name: "FuelCalc.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);
            plugin.AttachDelegate(name: "FuelCalc.FuelReserve", valueProvider: () => Settings.FuelReserveLiters.Value);
            plugin.AttachDelegate(name: "FuelCalc.ExtraLaps", valueProvider: () => Settings.ExtraRaceLaps.Value);
            plugin.AttachDelegate(name: "FuelCalc.ExtraConsumptionPct", valueProvider: () => Settings.ExtraConsumption.Value);
            plugin.AttachDelegate(name: "FuelCalc.EnablePreRaceWarning", valueProvider: () => Settings.EnablePreRaceWarning);
            plugin.AttachDelegate(name: "FuelCalc.Visible", valueProvider: () => Visible);
            plugin.AttachDelegate(name: "FuelCalc.BestLapTime", valueProvider: () => BestLapTime);
        }

        public override void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (data.FrameTime - _lastUpdateTime < _updateInterval) return;
            _lastUpdateTime = data.FrameTime;
            
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            bool isGarageVisible = false;
            try { isGarageVisible = (bool)raw.Telemetry["IsGarageVisible"]; } catch { }

            Visible = isGarageVisible;

            UpdateBestLapTime(ref data);
        }

        public override void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("FuelCalcSettings", Settings);
        }

        private void UpdateBestLapTime(ref GameData data)
        {
            BestLapTime = TimeSpan.Zero;

            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            int driverCount = 0;
            try { driverCount = (int)raw.AllSessionData["DriverInfo"]["Drivers"].Count; } catch { Debug.Assert(false); }

            int playerCarIdx = -1;
            try { playerCarIdx = int.Parse(raw.AllSessionData["DriverInfo"]["DriverCarIdx"]); } catch { Debug.Assert(false); }

            if (playerCarIdx < 0 || playerCarIdx >= driverCount) return;

            string playerClassId = data.NewData.CarClass;
            try { playerClassId = raw.AllSessionData["DriverInfo"]["Drivers"][playerCarIdx]["CarClassID"]; } catch { Debug.Assert(false); }

            // Try to find the fastest time of any session
            int sessionCount = 0;
            try { sessionCount = (int)raw.AllSessionData["SessionInfo"]["Sessions"].Count; } catch { Debug.Assert(false); }

            double fastestTime = 0;
            for (int sessionIdx = 0; sessionIdx < sessionCount; sessionIdx++)
            {
                List<object> positions = null;
                try { positions = raw.AllSessionData["SessionInfo"]["Sessions"][sessionIdx]["ResultsPositions"]; } catch { Debug.Assert(false); }
                if (positions == null) continue;

                for (int posIdx = 0; posIdx < positions.Count; posIdx++)
                {
                    int carIdx = -1;
                    try { carIdx = int.Parse(raw.AllSessionData["SessionInfo"]["Sessions"][sessionIdx]["ResultsPositions"][posIdx]["CarIdx"]); } catch { Debug.Assert(false); }

                    if (!_driverModule.DriversByCarIdx.TryGetValue(carIdx, out Driver driver))
                    {
                        continue;
                    }

                    string classId = string.Empty;
                    try { classId = raw.AllSessionData["DriverInfo"]["Drivers"][driver.DriverInfoIdx]["CarClassID"]; } catch { Debug.Assert(false); }

                    // Must be in same class as player
                    if (playerClassId == null || playerClassId == classId)
                    {
                        double timeSecs = 0;
                        try { timeSecs = double.Parse(raw.AllSessionData["SessionInfo"]["Sessions"][sessionIdx]["ResultsPositions"][posIdx]["FastestTime"]); } catch { Debug.Assert(false); }

                        if (timeSecs > 0 && (timeSecs < fastestTime || fastestTime == 0))
                        {
                            fastestTime = timeSecs;
                        }
                    }
                }
            }

            BestLapTime = TimeSpan.FromSeconds(fastestTime);
        }
    }
}
