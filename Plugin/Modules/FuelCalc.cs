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

        private float _fuelReserve = 0.5f;
        private string _fuelReserveString = "0.5";
        private bool _fuelReserveValid = true;

        public float FuelReserve
        {
            get { return _fuelReserve; }
        }
        public string FuelReserveString
        {
            get => _fuelReserveString;
            set
            {
                if (_fuelReserveString != value)
                {
                    _fuelReserveString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _fuelReserve = result;
                        FuelReserveValid = true;
                    }
                    else
                    {
                        _fuelReserve = 0;
                        FuelReserveValid = false;
                    }

                    OnPropertyChanged(nameof(FuelReserve));
                    OnPropertyChanged(nameof(FuelReserveString));
                }
            }
        }
        public bool FuelReserveValid
        {
            get => _fuelReserveValid;
            private set
            {
                if (_fuelReserveValid != value)
                {
                    _fuelReserveValid = value;
                    OnPropertyChanged(nameof(FuelReserveValid));
                }
            }
        }

        private float _extralLaps = 0.0f;
        private string _extraLapsString = "0";
        private bool _extraLapsValid = true;

        public float ExtraLaps
        {
            get { return _extralLaps; }
        }

        public string ExtraLapsString
        {
            get => _extraLapsString;
            set
            {
                if (_extraLapsString != value)
                {
                    _extraLapsString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _extralLaps = result;
                        ExtraLapsValid = true;
                    }
                    else
                    {
                        _extralLaps = 0;
                        ExtraLapsValid = false;
                    }

                    OnPropertyChanged(nameof(ExtraLaps));
                    OnPropertyChanged(nameof(ExtraLapsString));
                }
            }
        }
        public bool ExtraLapsValid
        {
            get => _extraLapsValid;
            private set
            {
                if (_extraLapsValid != value)
                {
                    _extraLapsValid = value;
                    OnPropertyChanged(nameof(ExtraLapsValid));
                }
            }
        }

        private float _extraConsumptionPct = 1.0f;
        private string _extraConsumptionPctString = "1.0";
        private bool _extraConsumptionPctValid = true;

        public float ExtraConsumptionPct
        {
            get { return _extraConsumptionPct; }
        }

        public string ExtraConsumptionPctString
        {
            get => _extraConsumptionPctString;
            set
            {
                if (_extraConsumptionPctString != value)
                {
                    _extraConsumptionPctString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _extraConsumptionPct = result;
                        ExtraConsumptionPctValid = true;
                    }
                    else
                    {
                        _extraConsumptionPct = 0;
                        ExtraConsumptionPctValid = false;
                    }

                    OnPropertyChanged(nameof(ExtraConsumptionPct));
                    OnPropertyChanged(nameof(ExtraConsumptionPctString));
                }
            }
        }
        public bool ExtraConsumptionPctValid
        {
            get => _extraConsumptionPctValid;
            private set
            {
                if (_extraConsumptionPctValid != value)
                {
                    _extraConsumptionPctValid = value;
                    OnPropertyChanged(nameof(ExtraConsumptionPctValid));
                }
            }
        }
        public bool EnablePreRaceWarning { get; set; } = true;
    }

    public class FuelCalcModule : IPluginModule
    {
        public FuelCalcSettings Settings { get; set; }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Settings = plugin.ReadCommonSettings<FuelCalcSettings>("FuelCalcSettings", () => new FuelCalcSettings());
            plugin.AttachDelegate(name: "FuelCalc.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);
            plugin.AttachDelegate(name: "FuelCalc.FuelReserve", valueProvider: () => Settings.FuelReserve);
            plugin.AttachDelegate(name: "FuelCalc.ExtraLaps", valueProvider: () => Settings.ExtraLaps);
            plugin.AttachDelegate(name: "FuelCalc.ExtraConsumptionPct", valueProvider: () => Settings.ExtraConsumptionPct);
            plugin.AttachDelegate(name: "FuelCalc.EnablePreRaceWarning", valueProvider: () => Settings.EnablePreRaceWarning);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {

        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("FuelCalcSettings", Settings);
        }
    }
}
