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
    public class TelemetrySettings : ModuleSettings
    {
        public bool TracesVisible { get; set; } = true;
        public int TracesWidth { get; set; } = 500;
        public int TracesSpeed { get; set; } = 75;
        public bool HandbrakeTraceVisible { get; set; } = true;
        public bool SteeringTraceVisible { get; set; } = true;
        public bool GuideLinesVisible { get; set; } = true;
        public bool PedalsVisible { get; set; } = true;
        public bool SteeringVisible { get; set; } = true;
        public bool GearAndSpeedVisible { get; set; } = true;
        public bool ShiftLightsVisible { get; set; } = true;
        public int BackgroundOpacity { get; set; } = 60;
    }

    public class TelemetryModule : PluginModuleBase
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

        public override void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Settings = plugin.ReadCommonSettings<TelemetrySettings>("TelemetrySettings", () => new TelemetrySettings());
            plugin.AttachDelegate(name: "Telemetry.TracesVisible", valueProvider: () => Settings.TracesVisible);
            plugin.AttachDelegate(name: "Telemetry.TracesWidth", valueProvider: () => Settings.TracesWidth);
            plugin.AttachDelegate(name: "Telemetry.TracesSpeed", valueProvider: () => Settings.TracesSpeed);
            plugin.AttachDelegate(name: "Telemetry.HandbrakeTraceVisible", valueProvider: () => Settings.HandbrakeTraceVisible);
            plugin.AttachDelegate(name: "Telemetry.SteeringTraceVisible", valueProvider: () => Settings.SteeringTraceVisible);
            plugin.AttachDelegate(name: "Telemetry.GuideLinesVisible", valueProvider: () => Settings.GuideLinesVisible);
            plugin.AttachDelegate(name: "Telemetry.PedalsVisible", valueProvider: () => Settings.PedalsVisible);
            plugin.AttachDelegate(name: "Telemetry.PedalsLeft", valueProvider: () => PedalsLeft);
            plugin.AttachDelegate(name: "Telemetry.SteeringVisible", valueProvider: () => Settings.SteeringVisible);
            plugin.AttachDelegate(name: "Telemetry.SteeringLeft", valueProvider: () => SteeringLeft);
            plugin.AttachDelegate(name: "Telemetry.GearAndSpeedVisible", valueProvider: () => Settings.GearAndSpeedVisible);
            plugin.AttachDelegate(name: "Telemetry.GearAndSpeedLeft", valueProvider: () => GearAndSpeedLeft);
            plugin.AttachDelegate(name: "Telemetry.ShiftLightsVisible", valueProvider: () => Settings.ShiftLightsVisible);
            plugin.AttachDelegate(name: "Telemetry.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);
            plugin.AttachDelegate(name: "Telemetry.BackgroundWidth", valueProvider: () => BackgroundWidth);
        }

        public override void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
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

        public override void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("TelemetrySettings", Settings);
        }
    }
}
