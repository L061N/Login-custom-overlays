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
    public class DeltaSettings : INotifyPropertyChanged
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

    public class Delta : IPluginModule
    {
        private Session _sessionModule = null;

        public float Speed { get; internal set; } = 0.0f;

        public DeltaSettings Settings { get; set; }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            _sessionModule = plugin.GetModule<Session>();

            Settings = plugin.ReadCommonSettings<DeltaSettings>("DeltaSettings", () => new DeltaSettings());
            plugin.AttachDelegate(name: "Delta.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);
            plugin.AttachDelegate(name: "Delta.Speed", valueProvider: () => Speed);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;
            if (_sessionModule == null) return;

            float delta = 0.0f;
            if (_sessionModule.Practice)
            {
                try { delta = (float)raw.Telemetry["LapDeltaToSessionBestLap_DD"]; } catch { }
            }
            else if (_sessionModule.Race)
            {
                try { delta = (float)raw.Telemetry["LapDeltaToSessionBestLap_DD"]; } catch { }
            }
            else if (_sessionModule.Qual)
            {
                try { delta = (float)raw.Telemetry["LapDeltaToBestLap_DD"]; } catch { }
            }

            Speed = Math.Min((float)data.NewData.SpeedLocal, (float)data.NewData.SpeedLocal * -delta);
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("DeltaSettings", Settings);
        }
    }
}
