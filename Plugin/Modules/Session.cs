﻿/*
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
using System.Runtime;

namespace benofficial2.Plugin
{
    public class Session : IPluginModule
    {
        private string _lastSessionTypeName = string.Empty;

        public bool Race { get; internal set; } = false;
        public bool Qual { get; internal set; } = false;
        public bool Practice { get; internal set; } = false;
        public bool Offline { get; internal set; } = false;
        public bool ReplayPlaying { get; internal set; } = false;
        public bool RaceFinished { get; internal set; } = false;

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.AttachDelegate(name: "Session.Race", valueProvider: () => Race);
            plugin.AttachDelegate(name: "Session.Qual", valueProvider: () => Qual);
            plugin.AttachDelegate(name: "Session.Practice", valueProvider: () => Practice);
            plugin.AttachDelegate(name: "Session.Offline", valueProvider: () => Offline);
            plugin.AttachDelegate(name: "Session.ReplayPlaying", valueProvider: () => ReplayPlaying);
            //plugin.AttachDelegate(name: "Session.RaceFinished", valueProvider: () => RaceFinished);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            if (data.NewData.SessionTypeName != _lastSessionTypeName)
            {
                Race = data.NewData.SessionTypeName.IndexOf("Race") != -1;
                Qual = data.NewData.SessionTypeName.IndexOf("Qual") != -1;

                Practice = data.NewData.SessionTypeName.IndexOf("Practice") != -1 ||
                    data.NewData.SessionTypeName.IndexOf("Warmup") != -1 ||
                    data.NewData.SessionTypeName.IndexOf("Testing") != -1;
                
                Offline = data.NewData.SessionTypeName.IndexOf("Offline") != -1;

                _lastSessionTypeName = data.NewData.SessionTypeName;
            }

            // Determine if replay is playing.
            // There's a short moment when loading into a session when isReplayPlaying is false
            // but position or trackSurface is -1.
            bool isReplayPLaying = false;
            try { isReplayPLaying = (bool)raw.Telemetry["IsReplayPlaying"]; } catch { }

            int position = -1;
            try { position = (int)raw.Telemetry["PlayerCarPosition"]; } catch { }

            int trackSurface = -1;
            try { trackSurface = (int)raw.Telemetry["PlayerTrackSurface"]; } catch { }
            
            ReplayPlaying = isReplayPLaying || position < 0 || trackSurface < 0;
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
        }
    }
}
