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
using System.ComponentModel;
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

        // Legacy properties for backwards compatibility (saved pre 2.2)
        public string FuelReserveString { get => FuelReserveLiters.ValueString; set => FuelReserveLiters.ValueString = value; }
        public string ExtraLapsString { get => ExtraRaceLaps.ValueString; set => ExtraRaceLaps.ValueString = value; }
        public string ExtraConsumptionPctString { get => ExtraConsumption.ValueString; set => ExtraConsumption.ValueString = value; }
    }

    public class FuelCalcModule : PluginModuleBase
    {
        public FuelCalcSettings Settings { get; set; }

        public override void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Settings = plugin.ReadCommonSettings<FuelCalcSettings>("FuelCalcSettings", () => new FuelCalcSettings());
            plugin.AttachDelegate(name: "FuelCalc.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);
            plugin.AttachDelegate(name: "FuelCalc.FuelReserve", valueProvider: () => Settings.FuelReserveLiters.Value);
            plugin.AttachDelegate(name: "FuelCalc.ExtraLaps", valueProvider: () => Settings.ExtraRaceLaps.Value);
            plugin.AttachDelegate(name: "FuelCalc.ExtraConsumptionPct", valueProvider: () => Settings.ExtraConsumption.Value);
            plugin.AttachDelegate(name: "FuelCalc.EnablePreRaceWarning", valueProvider: () => Settings.EnablePreRaceWarning);
        }

        public override void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {

        }

        public override void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("FuelCalcSettings", Settings);
        }
    }
}
