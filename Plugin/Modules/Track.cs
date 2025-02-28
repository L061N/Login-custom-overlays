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
using System.Runtime;
using System.Threading.Tasks;

namespace benofficial2.Plugin
{
    public class TrackModule : IPluginModule
    {
        private const string TrackInfoUrl = "https://raw.githubusercontent.com/fixfactory/bo2-official-overlays/main/Data/TrackInfo.json";

        private JObject _trackInfoJson = null;

        private string _lastTrackId = string.Empty;

        public int PushToPassCooldown { get; set; } = 0;

        public float QualStartTrackPct { get; set; } = 0.0f;

        public float RaceStartTrackPct { get; set; } = 0.0f;

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Task.Run(() =>
            {
                LoadTrackInfoAsync().Wait();
            });

            plugin.AttachDelegate(name: "Track.QualStartTrackPct", valueProvider: () => QualStartTrackPct);
            plugin.AttachDelegate(name: "Track.RaceStartTrackPct", valueProvider: () => RaceStartTrackPct);
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (_trackInfoJson == null) return;
            if (data.NewData.TrackId == _lastTrackId) return;
            _lastTrackId = data.NewData.TrackId;

            if (data.NewData.TrackId.Length == 0)
            {
                PushToPassCooldown = 0;
                QualStartTrackPct = 0.0f;
                RaceStartTrackPct = 0.0f;
                return;
            }

            JToken track = _trackInfoJson[data.NewData.TrackId];

            if (data.NewData.CarId == "superformulasf23 toyota" || data.NewData.CarId == "superformulasf23 honda")
            {
                PushToPassCooldown = track?["pushToPassCooldown_SF23"]?.Value<int>() ?? 100;
            }
            else
            {
                PushToPassCooldown = 0;
            }

            QualStartTrackPct = track?["qualStartTrackPct"]?.Value<float>() ?? 0.0f;
            RaceStartTrackPct = track?["raceStartTrackPct"]?.Value<float>() ?? 0.0f;
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
        }

        public async Task LoadTrackInfoAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(TrackInfoUrl);
                    _trackInfoJson = JObject.Parse(json);
                }
            }
            catch (Exception ex)
            {
                SimHub.Logging.Current.Error($"An error occurred while downloading {TrackInfoUrl}\n{ex.Message}");
            }
        }
    }
}
