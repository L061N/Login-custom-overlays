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
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace benofficial2.Plugin
{
    [PluginDescription("Enable this plugin to use benofficial's Official Overlay Collection for iRacing. Enable Show in Left Menu to access configuration options.")]
    [PluginAuthor("benofficial2")]
    [PluginName("benofficial2 Plugin")]
    public class benofficial2 : IPlugin, IDataPlugin, IWPFSettingsV2
    {
        public Dictionary<string, IPluginModule> Modules { get; private set; }

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
        /// </summary>
        public ImageSource PictureIcon => this.ToIcon(Properties.Resources.sdkmenuicon);

        /// <summary>
        /// Gets a short plugin title to show in left menu. Return null if you want to use the title as defined in PluginName attribute.
        /// </summary>
        public string LeftMenuTitle => "benofficial2";

        /// <summary>
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return new SettingsControl(this);
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            SimHub.Logging.Current.Info($"Starting benofficial2 plugin version {VersionChecker.CurrentVersion}");

            // Create all the modules
            Modules = PluginModuleFactory.CreateAllPluginModules();

            // Init each module
            foreach (var module in Modules.Values)
            {
                module.Init(pluginManager, this);
            }

            // Check for latest plugin version
            if (GetModule<General>().Settings.CheckForUpdates)
            {
                Task.Run(() =>
                {
                    VersionChecker versionChecker = new VersionChecker();
                    versionChecker.CheckForUpdateAsync().Wait();
                });
            }
        }

        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        ///
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
        ///
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data">Current game data, including current and previous data frame.</param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            if (!data.GameRunning) return;
            if (data.GameName != "IRacing") return;
            if (data.OldData == null || data.NewData == null) return;

            // Update each module
            foreach (var module in Modules.Values)
            {
                module.DataUpdate(pluginManager, this, ref data);
            }

            // Define the value of our property (declared in init)
            if (data.GameRunning)
            {
                if (data.OldData != null && data.NewData != null)
                {
                    dynamic raw = data.NewData.GetRawDataObject();
                    string playerName = raw.SessionData.DriverInfo.CompetingDrivers[0].UserName;
                    float oilLevel = raw.Telemetry.OilLevel;
                }
            }
        }

        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here !
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {
            // End each module
            foreach (var module in Modules.Values)
            {
                module.End(pluginManager, this);
            }
        }

        public T GetModule<T>() where T : class, IPluginModule
        {
            var key = typeof(T).Name;
            if (Modules.TryGetValue(key, out var module))
            {
                return module as T;
            }
            SimHub.Logging.Current.Error($"Missing Plugin Module {typeof(T).Name}");
            return null;
        }
    }
}