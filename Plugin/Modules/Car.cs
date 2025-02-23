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
using Newtonsoft.Json.Linq;
using SimHub.Plugins;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace benofficial2.Plugin
{
    public class Car : IPluginModule
    {
        private const string CarInfoUrl = "https://raw.githubusercontent.com/fixfactory/bo2-official-overlays/main/Data/CarInfo.json";

        private JObject _carInfoJson = null;

        private string _lastCarId = string.Empty;

        public bool IsGT3 { get; set; } = false;
        public bool IsGT4 { get; set; } = false;
        public bool IsGTE { get; set; } = false;
        public bool IsGTP { get; set; } = false;
        public bool HasPushToPass { get; set; } = false;
        public bool HasPushToPassCount { get; set; } = false;
        public bool HasPushToPassTimer { get; set; } = false;
        public bool HasPushToPassCooldown { get; set; } = false;
        public bool HasDrsDetection { get; set; } = false;
        public bool HasDrsCount { get; set; } = false;
        public bool HasErs { get; set; } = false;
        public bool HasBoost { get; set; } = false;
        public bool HasEnginePowerMode { get; set; } = false;
        public bool HasDeployMode { get; set; } = false;
        public bool HasDeployModeType1 { get; set; } = false;
        public bool HasDeployModeType2 { get; set; } = false;
        public bool HasThrottleShaping { get; set; } = false;
        public bool HasFuelMix { get; set; } = false;
        public bool HasARBModeP { get; set; } = false;
        public bool HasFrontARB { get; set; } = false;
        public bool HasExitDiff { get; set; } = false;
        public bool HasEntryDiffPreload { get; set; } = false;
        public bool HasTC2 { get; set; } = false;
        public bool HasRearARB { get; set; } = false;
        public bool HasEntryDiff { get; set; } = false;
        public bool HasTC { get; set; } = false;
        public bool HasTwoPartBrakeBias { get; set; } = false;
        public bool HasWeightJacker { get; set; } = false;
        public bool HasTwoPartPeakBrakeBias { get; set; } = false;
        public bool HasABS { get; set; } = false;
        public bool HasFineBrakeBias { get; set; } = false;
        public bool HasBrakeBiasMigration { get; set; } = false;
        public bool HasDryTireCompounds { get; set; } = false;

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Task.Run(() =>
            {
                LoadCarInfoAsync().Wait();
            });

            plugin.AttachDelegate(name: "Car.IsGT3", valueProvider: () => IsGT3);
            plugin.AttachDelegate(name: "Car.IsGT4", valueProvider: () => IsGT4);
            plugin.AttachDelegate(name: "Car.IsGTE", valueProvider: () => IsGTE);
            plugin.AttachDelegate(name: "Car.IsGTP", valueProvider: () => IsGTP);
            plugin.AttachDelegate(name: "Car.HasPushToPass", valueProvider: () => HasPushToPass);
            plugin.AttachDelegate(name: "Car.HasPushToPassCount", valueProvider: () => HasPushToPassCount);
            plugin.AttachDelegate(name: "Car.HasPushToPassTimer", valueProvider: () => HasPushToPassTimer);
            plugin.AttachDelegate(name: "Car.HasPushToPassCooldown", valueProvider: () => HasPushToPassCooldown);
            plugin.AttachDelegate(name: "Car.HasDrsDetection", valueProvider: () => HasDrsDetection);
            plugin.AttachDelegate(name: "Car.HasDrsCount", valueProvider: () => HasDrsCount);
            plugin.AttachDelegate(name: "Car.HasErs", valueProvider: () => HasErs);
            plugin.AttachDelegate(name: "Car.HasBoost", valueProvider: () => HasBoost);
            plugin.AttachDelegate(name: "Car.HasEnginePowerMode", valueProvider: () => HasEnginePowerMode);
            plugin.AttachDelegate(name: "Car.HasDeployMode", valueProvider: () => HasDeployMode);
            plugin.AttachDelegate(name: "Car.HasDeployModeType1", valueProvider: () => HasDeployModeType1);
            plugin.AttachDelegate(name: "Car.HasDeployModeType2", valueProvider: () => HasDeployModeType2);
            plugin.AttachDelegate(name: "Car.HasThrottleShaping", valueProvider: () => HasThrottleShaping);
            plugin.AttachDelegate(name: "Car.HasFuelMix", valueProvider: () => HasFuelMix);
            plugin.AttachDelegate(name: "Car.HasARBModeP", valueProvider: () => HasARBModeP);
            plugin.AttachDelegate(name: "Car.HasFrontARB", valueProvider: () => HasFrontARB);
            plugin.AttachDelegate(name: "Car.HasExitDiff", valueProvider: () => HasExitDiff);
            plugin.AttachDelegate(name: "Car.HasEntryDiffPreload", valueProvider: () => HasEntryDiffPreload);
            plugin.AttachDelegate(name: "Car.HasTC2", valueProvider: () => HasTC2);
            plugin.AttachDelegate(name: "Car.HasRearARB", valueProvider: () => HasRearARB);
            plugin.AttachDelegate(name: "Car.HasEntryDiff", valueProvider: () => HasEntryDiff);
            plugin.AttachDelegate(name: "Car.HasTC", valueProvider: () => HasTC);
            plugin.AttachDelegate(name: "Car.HasTwoPartBrakeBias", valueProvider: () => HasTwoPartBrakeBias);
            plugin.AttachDelegate(name: "Car.HasWeightJacker", valueProvider: () => HasWeightJacker);
            plugin.AttachDelegate(name: "Car.HasTwoPartPeakBrakeBias", valueProvider: () => HasTwoPartPeakBrakeBias);
            plugin.AttachDelegate(name: "Car.HasABS", valueProvider: () => HasABS);
            plugin.AttachDelegate(name: "Car.HasFineBrakeBias", valueProvider: () => HasFineBrakeBias);
            plugin.AttachDelegate(name: "Car.HasBrakeBiasMigration", valueProvider: () => HasBrakeBiasMigration);
            plugin.AttachDelegate(name: "Car.HasDryTireCompounds", valueProvider: () => HasDryTireCompounds);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (_carInfoJson == null) return;
            if (data.NewData.CarId == _lastCarId) return;
            _lastCarId = data.NewData.CarId;
            if (data.NewData.CarId.Length == 0) return;

            JToken car = _carInfoJson[data.NewData.CarId];
            if (car == null)
            {
                IsGT3 = false;
                IsGT4 = false;
                IsGTE = false;
                IsGTP = false;
                HasPushToPass = false;
                HasPushToPassCount = false;
                HasPushToPassTimer = false;
                HasPushToPassCooldown = false;
                HasDrsDetection = false;
                HasDrsCount = false;
                HasErs = false;
                HasBoost = false;
                HasEnginePowerMode = false;
                HasDeployMode = false;
                HasDeployModeType1 = false;
                HasDeployModeType2 = false;
                HasThrottleShaping = false;
                HasFuelMix = false;
                HasARBModeP = false;
                HasFrontARB = false;
                HasExitDiff = false;
                HasEntryDiffPreload = false;
                HasTC2 = false;
                HasRearARB = false;
                HasEntryDiff = false;
                HasTC = false;
                HasTwoPartBrakeBias = false;
                HasWeightJacker = false;
                HasTwoPartPeakBrakeBias = false;
                HasABS = false;
                HasFineBrakeBias = false;
                HasBrakeBiasMigration = false;
                HasDryTireCompounds = false;
                return;
            }

            IsGT3 = car["isGT3"]?.Value<bool>() ?? false;
            IsGT4 = car["isGT4"]?.Value<bool>() ?? false;
            IsGTE = car["isGTE"]?.Value<bool>() ?? false;
            IsGTP = car["isGTP"]?.Value<bool>() ?? false;
            HasPushToPass = car["hasPushToPass"]?.Value<bool>() ?? false;
            HasPushToPassCount = car["hasPushToPassCount"]?.Value<bool>() ?? false;
            HasPushToPassTimer = car["hasPushToPassTimer"]?.Value<bool>() ?? false;
            HasPushToPassCooldown = car["hasPushToPassCooldown"]?.Value<bool>() ?? false;
            HasDrsDetection = car["hasDrsDetection"]?.Value<bool>() ?? false;
            HasDrsCount = car["hasDrsCount"]?.Value<bool>() ?? false;
            HasErs = car["hasErs"]?.Value<bool>() ?? false;
            HasBoost = car["hasBoost"]?.Value<bool>() ?? false;
            HasEnginePowerMode = car["hasEnginePowerMode"]?.Value<bool>() ?? false;
            HasDeployMode = car["hasDeployMode"]?.Value<bool>() ?? false;
            HasDeployModeType1 = car["hasDeployModeType1"]?.Value<bool>() ?? false;
            HasDeployModeType2 = car["hasDeployModeType2"]?.Value<bool>() ?? false;
            HasThrottleShaping = car["hasThrottleShaping"]?.Value<bool>() ?? false;
            HasFuelMix = car["hasFuelMix"]?.Value<bool>() ?? false;
            HasARBModeP = car["hasARBModeP"]?.Value<bool>() ?? false;
            HasFrontARB = car["hasFrontARB"]?.Value<bool>() ?? false;
            HasExitDiff = car["hasExitDiff"]?.Value<bool>() ?? false;
            HasEntryDiffPreload = car["hasEntryDiffPreload"]?.Value<bool>() ?? false;
            HasTC2 = car["hasTC2"]?.Value<bool>() ?? false;
            HasRearARB = car["hasRearARB"]?.Value<bool>() ?? false;
            HasEntryDiff = car["hasEntryDiff"]?.Value<bool>() ?? false;
            HasTC = car["hasTC"]?.Value<bool>() ?? false;
            HasTwoPartBrakeBias = car["hasTwoPartBrakeBias"]?.Value<bool>() ?? false;
            HasWeightJacker = car["hasWeightJacker"]?.Value<bool>() ?? false;
            HasTwoPartPeakBrakeBias = car["hasTwoPartPeakBrakeBias"]?.Value<bool>() ?? false;
            HasABS = car["hasABS"]?.Value<bool>() ?? false;
            HasFineBrakeBias = car["hasFineBrakeBias"]?.Value<bool>() ?? false;
            HasBrakeBiasMigration = car["hasBrakeBiasMigration"]?.Value<bool>() ?? false;
            HasDryTireCompounds = car["hasDryTireCompounds"]?.Value<bool>() ?? false;
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
        }

        public async Task LoadCarInfoAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(CarInfoUrl);
                    _carInfoJson = JObject.Parse(json);
                }
            }
            catch (Exception ex)
            {
                SimHub.Logging.Current.Error($"An error occurred while downloading {CarInfoUrl}\n{ex.Message}");
            }
        }
    }
}
