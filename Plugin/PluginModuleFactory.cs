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
using System.Linq;
using System.Reflection;

namespace benofficial2.Plugin
{
    public interface IPluginModule
    {
        void Init(PluginManager pluginManager, benofficial2 plugin);
        void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data);
        void End(PluginManager pluginManager, benofficial2 plugin);
    }

    public static class PluginModuleFactory
    {
        public static Dictionary<string, IPluginModule> CreateAllPluginModules()
        {
            // Get all types that implement IPluginModule
            var moduleTypes = Assembly.GetExecutingAssembly()
                                      .GetTypes()
                                      .Where(t => typeof(IPluginModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                                      .ToList();

            // Instantiate each module type
            var modules = new Dictionary<string, IPluginModule>();
            foreach (var type in moduleTypes)
            {
                var instance = Activator.CreateInstance(type) as IPluginModule;
                if (instance != null)
                {
                    modules[type.Name] = instance;
                }
            }

            return modules;
        }
    }
}