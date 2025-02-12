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
        public PluginSettings Settings;
        public static Dictionary<string, IPluginModule> Modules { get; private set; }

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

            // Load settings
            Settings = this.ReadCommonSettings<PluginSettings>("GeneralSettings", () => new PluginSettings());

            // Check for latest plugin version
            if (Settings.CheckForUpdates)
            {
                Task.Run(() =>
                {
                    VersionChecker versionChecker = new VersionChecker();
                    versionChecker.CheckForUpdateAsync().Wait();
                });
            }

            // Create all the modules
            Modules = PluginModuleFactory.CreateAllPluginModules();

            // Init each module
            foreach (var module in Modules.Values)
            {
                module.Init(pluginManager, this);
            }

            // Declare a property available in the property list, this gets evaluated "on demand" (when shown or used in formulas)
            this.AttachDelegate(name: "CheckForUpdates", valueProvider: () => Settings.CheckForUpdates);

            this.AttachDelegate(name: "Standings.BackgroundOpacity", valueProvider: () => Settings.Standings.BackgroundOpacity);
            this.AttachDelegate(name: "Relative.BackgroundOpacity", valueProvider: () => Settings.Relative.BackgroundOpacity);
            this.AttachDelegate(name: "TrackMap.BackgroundOpacity", valueProvider: () => Settings.TrackMap.BackgroundOpacity);
            this.AttachDelegate(name: "Delta.BackgroundOpacity", valueProvider: () => Settings.Delta.BackgroundOpacity);
            this.AttachDelegate(name: "Telemetry.BackgroundOpacity", valueProvider: () => Settings.Telemetry.BackgroundOpacity);
            this.AttachDelegate(name: "Dash.BackgroundOpacity", valueProvider: () => Settings.Dash.BackgroundOpacity);
            this.AttachDelegate(name: "LaunchAssist.BackgroundOpacity", valueProvider: () => Settings.LaunchAssist.BackgroundOpacity);

            this.AttachDelegate(name: "Spotter.Enabled", valueProvider: () => Settings.Spotter.Enabled);
            this.AttachDelegate(name: "Spotter.Threshold", valueProvider: () => Settings.Spotter.Threshold);
            this.AttachDelegate(name: "Spotter.Height", valueProvider: () => Settings.Spotter.Height);
            this.AttachDelegate(name: "Spotter.MinHeight", valueProvider: () => Settings.Spotter.MinHeight);
            this.AttachDelegate(name: "Spotter.Width", valueProvider: () => Settings.Spotter.Width);
            this.AttachDelegate(name: "Spotter.Border", valueProvider: () => Settings.Spotter.Border);

            this.AttachDelegate(name: "RejoinHelper.Enabled", valueProvider: () => Settings.RejoinHelper.Enabled);
            this.AttachDelegate(name: "RejoinHelper.MinClearGap", valueProvider: () => Settings.RejoinHelper.MinClearGap);
            this.AttachDelegate(name: "RejoinHelper.MinCareGap", valueProvider: () => Settings.RejoinHelper.MinCareGap);
            this.AttachDelegate(name: "RejoinHelper.MinSpeed", valueProvider: () => Settings.RejoinHelper.MinSpeed);

            this.AttachDelegate(name: "BlindSpotMonitor.Enabled", valueProvider: () => Settings.BlindSpotMonitor.Enabled);

            this.AttachDelegate(name: "FuelCalc.BackgroundOpacity", valueProvider: () => Settings.FuelCalc.BackgroundOpacity);
            this.AttachDelegate(name: "FuelCalc.FuelReserve", valueProvider: () => Settings.FuelCalc.FuelReserve);
            this.AttachDelegate(name: "FuelCalc.ExtraLaps", valueProvider: () => Settings.FuelCalc.ExtraLaps);
            this.AttachDelegate(name: "FuelCalc.ExtraConsumptionPct", valueProvider: () => Settings.FuelCalc.ExtraConsumptionPct);
            this.AttachDelegate(name: "FuelCalc.EnablePreRaceWarning", valueProvider: () => Settings.FuelCalc.EnablePreRaceWarning);

            this.AttachDelegate(name: "TwitchChat.URL", valueProvider: () => Settings.TwitchChat.URL);
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

            // Save settings
            this.SaveCommonSettings("GeneralSettings", Settings);
        }
    }
}