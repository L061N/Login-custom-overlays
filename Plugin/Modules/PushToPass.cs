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

namespace benofficial2.Plugin
{
    public class PushToPass : IPluginModule
    {
        public bool Enabled { get; set; } = false;
        public bool Activated { get; set; } = false;
        public int Count { get; set; } = 0;
        public float TimeLeft { get; set; } = 0.0f;
        public float Cooldown { get; set; } = 0.0f;
        public float TotalCooldown { get; set; } = 0.0f;

        private bool _wasActivated = false;
        private DateTime _activatedTime = DateTime.MinValue;
        private DateTime _deactivatedTime = DateTime.MinValue;

        public PushToPass()
        {
            
        }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.AttachDelegate(name: "PushToPass.Enabled", valueProvider: () => Enabled);
            plugin.AttachDelegate(name: "PushToPass.Activated", valueProvider: () => Activated);
            plugin.AttachDelegate(name: "PushToPass.Count", valueProvider: () => Count);
            plugin.AttachDelegate(name: "PushToPass.TimeLeft", valueProvider: () => TimeLeft);
            plugin.AttachDelegate(name: "PushToPass.Cooldown", valueProvider: () => Cooldown);
            plugin.AttachDelegate(name: "PushToPass.TotalCooldown", valueProvider: () => TotalCooldown);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return;

            if (data.NewData.CarId == "dallarair18")
            {
                Enabled = true;
                Activated = (bool)data.NewData.PushToPassActive;
                try { Count = raw.Telemetry["P2P_Count"]; }
                catch { Count = 0; }
                TimeLeft = 0;
                Cooldown = 0;
                TotalCooldown = 0;
            }
            else if (data.NewData.CarId == "superformulasf23 toyota" || data.NewData.CarId == "superformulasf23 honda")
            {
                // For Super Formula, the telemetry only exposes 'activated' and 'timeLeft'.
                // We have to generate the other values.
                Enabled = true;
                Activated = (bool)data.NewData.PushToPassActive;
                try { TimeLeft = raw.Telemetry["P2P_Count"]; }
                catch { TimeLeft = 0; }

                // Total cooldown time at that track in milliseconds
                TotalCooldown = getTotalCooldown(ref data);

                // Check if Push-to-Pass was toggled
                if (Activated != _wasActivated)
                {
                    _wasActivated = Activated;

                    if (Activated)
                    {
                        _activatedTime = DateTime.Now;
                        _deactivatedTime = DateTime.MinValue;
                    }
                    else
                    {
                        _activatedTime = DateTime.MinValue;
                        _deactivatedTime = DateTime.Now;
                    }
                }

                // Update Cooldown timer
                if (!Activated)
                {
                    // Cooldown is only used in race
                    bool isRace = data.NewData.SessionTypeName.IndexOf("Race") != -1;
                    if (isRace)
                    {
                        int enterExitReset;
                        try { enterExitReset = raw.Telemetry["EnterExitReset"]; }
                        catch { enterExitReset = 0; }

                        int sessionState;
                        try { sessionState = raw.Telemetry["SessionState"]; }
                        catch { sessionState = 0; }

                        bool cancelCooldown = enterExitReset != 2 && sessionState == 4;
                        if (cancelCooldown)
                        {
                            _deactivatedTime = DateTime.MinValue;
                            Cooldown = 0;
                        }

                        if (_deactivatedTime != DateTime.MinValue)
                        {
                            TimeSpan deactivatedDuration = DateTime.Now - _deactivatedTime;
                            if (deactivatedDuration.TotalSeconds < TotalCooldown)
                            {
                                Cooldown = (float)(TotalCooldown - deactivatedDuration.TotalSeconds);
                            }
                            else
                            {
                                Cooldown = 0;
                                _deactivatedTime = DateTime.MinValue;
                            }
                        }
                    }
                    else
                    {
                        Cooldown = 0;
                    }
                }
            }
            else
            {
                // Car does not support PushToPass
                Enabled = false;
                Activated = false;
                Count = 0;
                TimeLeft = 0;
                Cooldown = 0;
                TotalCooldown = 0;
            }
        }


        public void End(PluginManager pluginManager, benofficial2 plugin)
        {

        }

        // Returns the P2P Cooldown Time (aka ReTime) in seconds based on the current track.
        public float getTotalCooldown(ref GameData data)
        {
            string trackIds120 = "twinring fullrc, fuji gp";
            if (trackIds120.IndexOf(data.NewData.TrackId) != -1) return 120;
            return 100;
        }
    }
}
