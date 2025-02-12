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

    public class RejoinHelper : IPluginModule
    {
        public RejoinHelperSettings Settings { get; set; }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Settings = plugin.ReadCommonSettings<RejoinHelperSettings>("RejoinHelperSettings", () => new RejoinHelperSettings());
            plugin.AttachDelegate(name: "RejoinHelper.Enabled", valueProvider: () => Settings.Enabled);
            plugin.AttachDelegate(name: "RejoinHelper.MinClearGap", valueProvider: () => Settings.MinClearGap);
            plugin.AttachDelegate(name: "RejoinHelper.MinCareGap", valueProvider: () => Settings.MinCareGap);
            plugin.AttachDelegate(name: "RejoinHelper.MinSpeed", valueProvider: () => Settings.MinSpeed);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {

        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("RejoinHelperSettings", Settings);
        }
    }
}
