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

namespace benofficial2.Plugin
{
    public class PushToPass : IPluginModule
    {
        public bool Enabled { get; } = false;
        public bool Activated { get; } = false;
        public int Count { get; } = 0;
        public float TimeLeft { get; } = 0.0f;
        public float TimeUsed { get; } = 0.0f;
        public float Cooldown { get; } = 0.0f;
        
        public PushToPass()
        {
            
        }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            /*plugin.AttachDelegate(name: "PushToPass.Enabled", valueProvider: () => Enabled);
            plugin.AttachDelegate(name: "PushToPass.Activated", valueProvider: () => Activated);
            plugin.AttachDelegate(name: "PushToPass.Count", valueProvider: () => Count);
            plugin.AttachDelegate(name: "PushToPass.TimeLeft", valueProvider: () => TimeLeft);
            plugin.AttachDelegate(name: "PushToPass.TimeUsed", valueProvider: () => TimeUsed);
            plugin.AttachDelegate(name: "PushToPass.Cooldown", valueProvider: () => Cooldown);*/
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {

        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {

        }
    }
}
