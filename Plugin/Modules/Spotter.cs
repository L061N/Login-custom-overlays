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
using System.Globalization;

namespace benofficial2.Plugin
{
    public class SpotterSettings : ModuleSettings
    {
        public bool Enabled { get; set; } = true;

        private float _threshold = 5.5f;
        private string _thresholdString = "5.5";
        private bool _thresholdValid = true;

        public float Threshold
        {
            get { return _threshold; }
        }
        public string ThresholdString
        {
            get => _thresholdString;
            set
            {
                if (_thresholdString != value)
                {
                    _thresholdString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _threshold = result;
                        ThresholdValid = true;
                    }
                    else
                    {
                        _threshold = 0;
                        ThresholdValid = false;
                    }

                    OnPropertyChanged(nameof(Threshold));
                    OnPropertyChanged(nameof(ThresholdString));
                }
            }
        }
        public bool ThresholdValid
        {
            get => _thresholdValid;
            private set
            {
                if (_thresholdValid != value)
                {
                    _thresholdValid = value;
                    OnPropertyChanged(nameof(ThresholdValid));
                }
            }
        }

        public int Height { get; set; } = 129;
        public int MinHeight { get; set; } = 15;
        public int Width { get; set; } = 12;
        public int Border { get; set; } = 3;
    }

    public class SpotterModule : IPluginModule
    {
        public SpotterSettings Settings { get; set; }

        public double OverlapAhead { get; internal set; } = 0;
        public double OverlapBehind { get; internal set; } = 0;

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Settings = plugin.ReadCommonSettings<SpotterSettings>("SpotterSettings", () => new SpotterSettings());
            plugin.AttachDelegate(name: "Spotter.Enabled", valueProvider: () => Settings.Enabled);
            plugin.AttachDelegate(name: "Spotter.Threshold", valueProvider: () => Settings.Threshold);
            plugin.AttachDelegate(name: "Spotter.Height", valueProvider: () => Settings.Height);
            plugin.AttachDelegate(name: "Spotter.MinHeight", valueProvider: () => Settings.MinHeight);
            plugin.AttachDelegate(name: "Spotter.Width", valueProvider: () => Settings.Width);
            plugin.AttachDelegate(name: "Spotter.Border", valueProvider: () => Settings.Border);
            plugin.AttachDelegate(name: "Spotter.OverlapAhead", valueProvider: () => OverlapAhead);
            plugin.AttachDelegate(name: "Spotter.OverlapBehind", valueProvider: () => OverlapBehind);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            UpdateOverlapAhead(ref data);
            UpdateOverlapBehind(ref data);
        }

        public void UpdateOverlapAhead(ref GameData data)
        {
            (double dist0, double dist1) = GetNearestDistances(data.NewData.OpponentsAheadOnTrack);
            double overlap = 0;
            if (dist0 < 0 && dist0 >= -Settings.Threshold)
            {
                overlap = dist0;
            }

            if (dist1 < 0 && dist1 >= -Settings.Threshold)
            {
                overlap = Math.Min(overlap, dist1);
            }

            OverlapAhead = overlap;
        }

        public void UpdateOverlapBehind(ref GameData data)
        {
            (double dist0, double dist1) = GetNearestDistances(data.NewData.OpponentsBehindOnTrack);
            double overlap = 0;
            if (dist0 > 0 && dist0 <= Settings.Threshold)
            {
                overlap = dist0;
            }

            if (dist1 > 0 && dist1 <= Settings.Threshold)
            {
                overlap = Math.Max(overlap, dist1);
            }

            OverlapBehind = overlap;
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("SpotterSettings", Settings);
        }

        public (double dist0, double dist1) GetNearestDistances(List<Opponent> opponents)
        {
            double dist0 = 0, dist1 = 0;
            if (opponents.Count > 0) dist0 = opponents[0].RelativeDistanceToPlayer ?? 0;
            if (opponents.Count > 1) dist1 = opponents[1].RelativeDistanceToPlayer ?? 0;
            return (dist0, dist1);
        }
    }
}
