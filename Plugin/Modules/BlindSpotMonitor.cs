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
using System.ComponentModel;

namespace benofficial2.Plugin
{
    public class BlindSpotMonitorSettings : INotifyPropertyChanged
    {
        private bool _enabled = false;

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BlindSpotMonitor : IPluginModule
    {
        private Spotter _spotterModule = null;

        public BlindSpotMonitorSettings Settings { get; set; }
        public bool Visible { get; set; } = false;

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            _spotterModule = plugin.GetModule<Spotter>();

            Settings = plugin.ReadCommonSettings<BlindSpotMonitorSettings>("BlindSpotMonitorSettings", () => new BlindSpotMonitorSettings());
            plugin.AttachDelegate(name: "BlindSpotMonitor.Enabled", valueProvider: () => Settings.Enabled);
            plugin.AttachDelegate(name: "BlindSpotMonitor.Visible", valueProvider: () => Visible);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (!Settings.Enabled)
            {
                Visible = false;
            }
            else
            {
                Visible = _spotterModule.OverlapAhead < 0 || _spotterModule.OverlapBehind > 0;
            }
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("BlindSpotMonitorSettings", Settings);
        }
    }
}
