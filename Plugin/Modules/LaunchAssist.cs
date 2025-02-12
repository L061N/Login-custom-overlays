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
    public class LaunchAssistSettings : INotifyPropertyChanged
    {
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

    public class LaunchAssist : IPluginModule
    {
        public LaunchAssistSettings Settings { get; set; }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Settings = plugin.ReadCommonSettings<LaunchAssistSettings>("LaunchAssistSettings", () => new LaunchAssistSettings());
            plugin.AttachDelegate(name: "LaunchAssist.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {

        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("LaunchAssistSettings", Settings);
        }
    }
}
