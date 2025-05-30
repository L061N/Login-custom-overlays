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
using System;
using System.Diagnostics;

namespace benofficial2.Plugin
{
    public class SessionState
    {
        private double _lastSessionTime = double.MaxValue;
        private Guid _lastSessionId = Guid.Empty;

        public TimeSpan SessionTime { get; private set; } = TimeSpan.Zero;
        public TimeSpan DeltaTime { get; private set; } = TimeSpan.Zero;
        public bool SessionChanged { get; private set; } = true;
        
        public void Update(ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            double sessionTime = 0;
            try { sessionTime = (double)raw.Telemetry["SessionTime"]; } catch { Debug.Assert(false); }

            SessionTime = TimeSpan.FromSeconds(sessionTime);
            DeltaTime = TimeSpan.FromSeconds(Math.Max(sessionTime - _lastSessionTime, 0));

            // Also consider the session changed when time flows backward.
            // Because many checks are based on time flowing forward and would break otherwise.
            // Only happens when manually moving the time backwards in a SimHub replay.
            SessionChanged = (sessionTime < _lastSessionTime || data.SessionId != _lastSessionId);
            
            _lastSessionTime = sessionTime;
            _lastSessionId = data.SessionId;
        }
    }

    public class SessionModule : PluginModuleBase
    {
        private string _lastSessionTypeName = string.Empty;
        private bool _raceFinishedForPlayer = false;
        private double? _lastTrackPct = null;
        private DateTime _raceStartedTime = DateTime.MinValue;

        public SessionState State { get; internal set; } = new SessionState();
        public bool Race { get; internal set; } = false;
        public bool Qual { get; internal set; } = false;
        public bool Practice { get; internal set; } = false;
        public bool Offline { get; internal set; } = false;
        public bool ReplayPlaying { get; internal set; } = false;
        public bool RaceStarted { get; internal set; } = false;
        public bool RaceFinished { get; internal set; } = false;
        public double RaceTimer { get; internal set; } = 0;
        public bool Oval { get; internal set; } = false;

        public override int UpdatePriority => 10;

        public override void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.AttachDelegate(name: "Session.Race", valueProvider: () => Race);
            plugin.AttachDelegate(name: "Session.Qual", valueProvider: () => Qual);
            plugin.AttachDelegate(name: "Session.Practice", valueProvider: () => Practice);
            plugin.AttachDelegate(name: "Session.Offline", valueProvider: () => Offline);
            plugin.AttachDelegate(name: "Session.ReplayPlaying", valueProvider: () => ReplayPlaying);
            plugin.AttachDelegate(name: "Session.RaceStarted", valueProvider: () => RaceStarted);
            plugin.AttachDelegate(name: "Session.RaceFinished", valueProvider: () => RaceFinished);
            plugin.AttachDelegate(name: "Session.RaceTimer", valueProvider: () => RaceTimer);
            plugin.AttachDelegate(name: "Session.Oval", valueProvider: () => Oval);
        }

        public override void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            State.Update(ref data);

            if (State.SessionChanged || data.NewData.SessionTypeName != _lastSessionTypeName)
            {
                Race = data.NewData.SessionTypeName.IndexOf("Race") != -1;
                Qual = data.NewData.SessionTypeName.IndexOf("Qual") != -1;

                Practice = data.NewData.SessionTypeName.IndexOf("Practice") != -1 ||
                    data.NewData.SessionTypeName.IndexOf("Warmup") != -1 ||
                    data.NewData.SessionTypeName.IndexOf("Testing") != -1;

                Offline = data.NewData.SessionTypeName.IndexOf("Offline") != -1;

                string category = string.Empty;
                try { category = raw.AllSessionData["WeekendInfo"]["Category"]; } catch { }
                Oval = category == "Oval" || category == "DirtOval";

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

            // Determine if race started
            int sessionState = 0;
            try { sessionState = (int)raw.Telemetry["SessionState"]; } catch { }
            RaceStarted = Race && sessionState >= 4;

            // Determine if race finished for the player
            if (!Race || State.SessionChanged)
            {
                // Reset when changing/restarting session
                _lastTrackPct = null;
                _raceFinishedForPlayer = false;
                RaceFinished = false;
            }
            else
            {
                if (_raceFinishedForPlayer)
                {
                    // Race finished
                    RaceFinished = true;
                }
                else if (data.NewData.Flag_Checkered != 1)
                {
                    // Checkered flag is not shown
                    RaceFinished = false;
                }
                else if (!_lastTrackPct.HasValue || _lastTrackPct.Value <= data.NewData.TrackPositionPercent)
                {
                    // Heading toward the checkered flag
                    _lastTrackPct = data.NewData.TrackPositionPercent;
                    RaceFinished = false;
                }
                else
                {
                    // Crossed the line with the checkered flag
                    _raceFinishedForPlayer = true;
                    RaceFinished = true;
                }
            }

            // Update race timer
            if (RaceStarted)
            {
                // Freeze timer when race is finished
                if (!RaceFinished)
                {
                    if (_raceStartedTime == DateTime.MinValue)
                    {
                        _raceStartedTime = DateTime.Now;
                    }

                    RaceTimer = (DateTime.Now - _raceStartedTime).TotalSeconds;
                }
            }
            else
            {
                RaceTimer = 0;
                _raceStartedTime = DateTime.MinValue;
            }
        }

        public override void End(PluginManager pluginManager, benofficial2 plugin)
        {
        }
    }
}
