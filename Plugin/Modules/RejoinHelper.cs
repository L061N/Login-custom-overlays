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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace benofficial2.Plugin
{
    public class RejoinHelperSettings : INotifyPropertyChanged
    {
        private bool _enabled = true;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }

        private float _minClearGap = 3.5f;
        private string _minClearGapString = "3.5";
        private bool _minClearGapValid = true;

        public float MinClearGap
        {
            get { return _minClearGap; }
        }
        public string MinClearGapString
        {
            get => _minClearGapString;
            set
            {
                if (_minClearGapString != value)
                {
                    _minClearGapString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _minClearGap = result;
                        MinClearGapValid = true;
                    }
                    else
                    {
                        _minClearGap = 0;
                        MinClearGapValid = false;
                    }

                    OnPropertyChanged(nameof(MinClearGap));
                    OnPropertyChanged(nameof(MinClearGapString));
                }
            }
        }
        public bool MinClearGapValid
        {
            get => _minClearGapValid;
            private set
            {
                if (_minClearGapValid != value)
                {
                    _minClearGapValid = value;
                    OnPropertyChanged(nameof(MinClearGapValid));
                }
            }
        }

        private float _minCareGap = 1.5f;
        private string _minCareGapString = "1.5";
        private bool _minCareGapValid = true;

        public float MinCareGap
        {
            get { return _minCareGap; }
        }
        public string MinCareGapString
        {
            get => _minCareGapString;
            set
            {
                if (_minCareGapString != value)
                {
                    _minCareGapString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _minCareGap = result;
                        MinCareGapValid = true;
                    }
                    else
                    {
                        _minCareGap = 0;
                        MinCareGapValid = false;
                    }

                    OnPropertyChanged(nameof(MinCareGap));
                    OnPropertyChanged(nameof(MinCareGapString));
                }
            }
        }
        public bool MinCareGapValid
        {
            get => _minCareGapValid;
            private set
            {
                if (_minCareGapValid != value)
                {
                    _minCareGapValid = value;
                    OnPropertyChanged(nameof(MinCareGapValid));
                }
            }
        }

        private int _minSpeed = 35;

        public int MinSpeed
        {
            get { return _minSpeed; }
            set
            {
                if (_minSpeed != value)
                {
                    _minSpeed = value;
                    OnPropertyChanged(nameof(MinSpeed));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RejoinHelperModule : IPluginModule
    {
        private SessionModule _sessionModule = null;

        public RejoinHelperSettings Settings { get; set; }
        public bool Visible { get; set; } = false;
        public double Gap { get; set; } = 0;
        public string State { get; set; } = string.Empty;
        public double ColorPct { get; set; } = 0;

        public const string StateClear = "Clear";
        public const string StateCare = "Care";
        public const string StateYield = "Yield";

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            _sessionModule = plugin.GetModule<SessionModule>();

            Settings = plugin.ReadCommonSettings<RejoinHelperSettings>("RejoinHelperSettings", () => new RejoinHelperSettings());
            plugin.AttachDelegate(name: "RejoinHelper.Enabled", valueProvider: () => Settings.Enabled);
            plugin.AttachDelegate(name: "RejoinHelper.MinClearGap", valueProvider: () => Settings.MinClearGap);
            plugin.AttachDelegate(name: "RejoinHelper.MinCareGap", valueProvider: () => Settings.MinCareGap);
            plugin.AttachDelegate(name: "RejoinHelper.MinSpeed", valueProvider: () => Settings.MinSpeed);
            plugin.AttachDelegate(name: "RejoinHelper.Visible", valueProvider: () => Visible);
            plugin.AttachDelegate(name: "RejoinHelper.Gap", valueProvider: () => Gap);
            plugin.AttachDelegate(name: "RejoinHelper.State", valueProvider: () => State);
            plugin.AttachDelegate(name: "RejoinHelper.ColorPct", valueProvider: () => ColorPct);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            // Wait for race to be started for a few seconds not to trigger on a standing start
            if (!Settings.Enabled || (_sessionModule.Race && (_sessionModule.RaceFinished || _sessionModule.RaceTimer < 3)))
            {
                Visible = false;
                Gap = 0;
                State = StateClear;
                ColorPct = 100;
            }
            else
            {
                int trackSurface = 0;
                try { trackSurface = (int)raw.Telemetry["PlayerTrackSurface"]; } catch { }

                bool isSlow = data.NewData.SpeedKmh < Settings.MinSpeed;
                Visible = isSlow || trackSurface == 0;

                List<Opponent> opponents = data.NewData.OpponentsBehindOnTrack;
                if (opponents.Count > 0)
                {
                    Gap = opponents[0].RelativeGapToPlayer ?? 0;
                }
                else
                {
                    Gap = 0;
                }

                if (Gap <= 0)
                {
                    State = StateClear;
                    ColorPct = 100;
                }
                else
                {
                    if (Gap >= Settings.MinClearGap)
                    {
                        State = StateClear;
                        ColorPct = 100;
                    }
                    else if (Gap >= Settings.MinCareGap)
                    {
                        State = StateCare;
                        double ratio = (Gap - Settings.MinCareGap) / (Settings.MinClearGap - Settings.MinCareGap);
                        ColorPct = ((100 - 50) * ratio) + 50;
                    }
                    else
                    {
                        State = StateYield;
                        double ratio = Gap / Settings.MinClearGap;
                        ColorPct = 50 * ratio;
                    }
                }
            }
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("RejoinHelperSettings", Settings);
        }
    }
}
