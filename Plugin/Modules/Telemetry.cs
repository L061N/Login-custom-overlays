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

namespace benofficial2.Plugin
{
    public class TelemetrySettings : INotifyPropertyChanged
    {
        private bool _tracesVisible = true;

        public bool TracesVisible
        {
            get { return _tracesVisible; }
            set
            {
                if (_tracesVisible != value)
                {
                    _tracesVisible = value;
                    OnPropertyChanged(nameof(TracesVisible));
                }
            }
        }

        private int _tracesWidth = 500;

        public int TracesWidth
        {
            get { return _tracesWidth; }
            set
            {
                if (_tracesWidth != value)
                {
                    _tracesWidth = value;
                    OnPropertyChanged(nameof(TracesWidth));
                }
            }
        }

        private bool _handbrakeTraceVisible = true;

        public bool HandbrakeTraceVisible
        {
            get { return _handbrakeTraceVisible; }
            set
            {
                if (_handbrakeTraceVisible != value)
                {
                    _handbrakeTraceVisible = value;
                    OnPropertyChanged(nameof(HandbrakeTraceVisible));
                }
            }
        }

        private bool _steeringTraceVisible = true;

        public bool SteeringTraceVisible
        {
            get { return _steeringTraceVisible; }
            set
            {
                if (_steeringTraceVisible != value)
                {
                    _steeringTraceVisible = value;
                    OnPropertyChanged(nameof(SteeringTraceVisible));
                }
            }
        }

        private bool _pedalsVisible = true;

        public bool PedalsVisible
        {
            get { return _pedalsVisible; }
            set
            {
                if (_pedalsVisible != value)
                {
                    _pedalsVisible = value;
                    OnPropertyChanged(nameof(PedalsVisible));
                }
            }
        }

        private bool _steeringVisible = true;

        public bool SteeringVisible
        {
            get { return _steeringVisible; }
            set
            {
                if (_steeringVisible != value)
                {
                    _steeringVisible = value;
                    OnPropertyChanged(nameof(SteeringVisible));
                }
            }
        }

        private bool _gearAndSpeedVisible = true;

        public bool GearAndSpeedVisible
        {
            get { return _gearAndSpeedVisible; }
            set
            {
                if (_gearAndSpeedVisible != value)
                {
                    _gearAndSpeedVisible = value;
                    OnPropertyChanged(nameof(GearAndSpeedVisible));
                }
            }
        }

        private int _backgroundOpacity = 60;

        public int BackgroundOpacity
        {
            get { return _backgroundOpacity; }
            set
            {
                if (_backgroundOpacity != value)
                {
                    _backgroundOpacity = value;
                    OnPropertyChanged(nameof(BackgroundOpacity));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Telemetry : IPluginModule
    {
        private const int _tracesWidth = 500;
        private const int _tracesLeft = 17;
        private const int _pedalsLeft = 492;
        private const int _steeringLeft = 650;
        private const int _gearAndSpeedLeft = 573;
        private const int _backgroundWidth = 760;

        public TelemetrySettings Settings { get; set; }

        public int PedalsLeft { get; set; } = _pedalsLeft;

        public int SteeringLeft { get; set; } = _steeringLeft;

        public int GearAndSpeedLeft { get; set; } = _gearAndSpeedLeft;

        public int BackgroundWidth { get; set; } = _backgroundWidth;

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Settings = plugin.ReadCommonSettings<TelemetrySettings>("TelemetrySettings", () => new TelemetrySettings());
            plugin.AttachDelegate(name: "Telemetry.TracesVisible", valueProvider: () => Settings.TracesVisible);
            plugin.AttachDelegate(name: "Telemetry.TracesWidth", valueProvider: () => Settings.TracesWidth);
            plugin.AttachDelegate(name: "Telemetry.HandbrakeTraceVisible", valueProvider: () => Settings.HandbrakeTraceVisible);
            plugin.AttachDelegate(name: "Telemetry.SteeringTraceVisible", valueProvider: () => Settings.SteeringTraceVisible);
            plugin.AttachDelegate(name: "Telemetry.PedalsVisible", valueProvider: () => Settings.PedalsVisible);
            plugin.AttachDelegate(name: "Telemetry.PedalsLeft", valueProvider: () => PedalsLeft);
            plugin.AttachDelegate(name: "Telemetry.SteeringVisible", valueProvider: () => Settings.SteeringVisible);
            plugin.AttachDelegate(name: "Telemetry.SteeringLeft", valueProvider: () => SteeringLeft);
            plugin.AttachDelegate(name: "Telemetry.GearAndSpeedVisible", valueProvider: () => Settings.GearAndSpeedVisible);
            plugin.AttachDelegate(name: "Telemetry.GearAndSpeedLeft", valueProvider: () => GearAndSpeedLeft);
            plugin.AttachDelegate(name: "Telemetry.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);
            plugin.AttachDelegate(name: "Telemetry.BackgroundWidth", valueProvider: () => BackgroundWidth);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            int tracesOffset = 0;
            if (!Settings.TracesVisible)
            {
                tracesOffset = -_tracesWidth + _tracesLeft;
            }
            else
            {
                tracesOffset = -(_tracesWidth - Settings.TracesWidth);
            }

            int pedalsOffset = 0;
            if (!Settings.PedalsVisible)
            {
                pedalsOffset = -(_gearAndSpeedLeft - _pedalsLeft);
            }

            int gearAndSpeedOffset = 0;
            if (!Settings.GearAndSpeedVisible)
            {
                gearAndSpeedOffset = -(_steeringLeft - _gearAndSpeedLeft);
            }

            int steeringOffset = 0;
            if (!Settings.SteeringVisible)
            {
                steeringOffset = -(_backgroundWidth - _steeringLeft);
            }

            PedalsLeft = _pedalsLeft + tracesOffset;
            GearAndSpeedLeft = _gearAndSpeedLeft + tracesOffset + pedalsOffset;
            SteeringLeft = _steeringLeft + tracesOffset + pedalsOffset + gearAndSpeedOffset;
            BackgroundWidth = _backgroundWidth + tracesOffset + pedalsOffset + gearAndSpeedOffset + steeringOffset;
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("TelemetrySettings", Settings);
        }
    }
}
