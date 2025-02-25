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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace benofficial2.Plugin
{
    public class RelativeSettings : INotifyPropertyChanged
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

    public class RelativeRow
    {
        public bool RowVisible { get; set; } = false;
        public int PositionInClass { get; set; } = 0;
        public string ClassColor { get; set; } = string.Empty;
        public string ClassTextColor { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool OutLap { get; set; } = false;
        public int iRating { get; set; } = 0;
        public string License { get; set; } = string.Empty;
        public double SafetyRating { get; set; } = 0;
        public double GapToPlayer { get; set; } = 0;
        public string GapToPlayerCombined { get; set; } = string.Empty;
    }

    public class RelativeAhead
    {
        public const int MaxRows = 3;
        public List<RelativeRow> Rows { get; internal set; }

        public RelativeAhead()
        {
            Rows = new List<RelativeRow>(Enumerable.Range(0, MaxRows).Select(x => new RelativeRow()));
        }
    }
    public class RelativeBehind
    {
        public const int MaxRows = 3;
        public List<RelativeRow> Rows { get; internal set; }

        public RelativeBehind()
        {
            Rows = new List<RelativeRow>(Enumerable.Range(0, MaxRows).Select(x => new RelativeRow()));
        }
    }

    public class Relative : IPluginModule
    {
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(500);
        private DriverModule _driverModule = null;

        public RelativeSettings Settings { get; set; }

        public RelativeAhead Ahead;
        public RelativeBehind Behind;

        public Relative()
        {
            Ahead = new RelativeAhead();
            Behind = new RelativeBehind();
        }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            _driverModule = plugin.GetModule<DriverModule>();

            Settings = plugin.ReadCommonSettings<RelativeSettings>("RelativeSettings", () => new RelativeSettings());
            plugin.AttachDelegate(name: "Relative.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);

            InitRelative(plugin, "Ahead", Ahead.Rows, RelativeAhead.MaxRows);
            InitRelative(plugin, "Behind", Behind.Rows, RelativeBehind.MaxRows);
        }

        private void InitRelative(benofficial2 plugin, string aheadBehind, List<RelativeRow> rows, int maxRows)
        {
            for (int rowIdx = 0; rowIdx < maxRows; rowIdx++)
            {
                RelativeRow row = rows[rowIdx];
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.RowVisible", valueProvider: () => row.RowVisible);
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.PositionInClass", valueProvider: () => row.PositionInClass);
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.ClassColor", valueProvider: () => row.ClassColor);
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.ClassTextColor", valueProvider: () => row.ClassTextColor);
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.Number", valueProvider: () => row.Number);
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.Name", valueProvider: () => row.Name);
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.OutLap", valueProvider: () => row.OutLap);
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.iRating", valueProvider: () => row.iRating);
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.License", valueProvider: () => row.License);
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.SafetyRating", valueProvider: () => row.SafetyRating);
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.GapToPlayer", valueProvider: () => row.GapToPlayer);
                plugin.AttachDelegate(name: $"Relative.{aheadBehind}{rowIdx:00}.GapToPlayerCombined", valueProvider: () => row.GapToPlayerCombined);
            }
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (DateTime.Now - _lastUpdateTime < _updateInterval) return;
            _lastUpdateTime = DateTime.Now;

            UpdateRelative(ref data, Ahead.Rows, RelativeAhead.MaxRows, data.NewData.OpponentsAheadOnTrack);
            UpdateRelative(ref data, Behind.Rows, RelativeBehind.MaxRows, data.NewData.OpponentsBehindOnTrack);
        }

        public void UpdateRelative(ref GameData data, List<RelativeRow> rows, int maxRows, List<Opponent> opponents)
        {
            for (int rowIdx = 0; rowIdx < maxRows; rowIdx++)
            {
                RelativeRow row = rows[rowIdx];

                if (rowIdx >= opponents.Count)
                {
                    BlankRow(row);
                    continue;
                }

                Opponent opponent = opponents[rowIdx];
                if (!IsValidRow(opponent))
                {
                    BlankRow(row);
                    continue;
                }

                row.RowVisible = true;
                row.PositionInClass = opponent.Position > 0 ? opponent.PositionInClass : 0;
                row.ClassColor = opponent.CarClassColor;
                row.ClassTextColor = opponent.CarClassTextColor;
                row.Number = opponent.CarNumber;
                row.Name = opponent.Name;
                try { row.OutLap = _driverModule.Drivers?[opponent.CarNumber]?.OutLap ?? false; }
                catch { row.OutLap = false; }
                row.iRating = (int)opponent.IRacing_IRating;
                (row.License, row.SafetyRating) = DriverModule.ParseLicenseString(opponent.LicenceString);
                row.GapToPlayer = opponent.RelativeGapToPlayer ?? 0;
                row.GapToPlayerCombined = opponent.GapToPlayerCombined;
            }
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("RelativeSettings", Settings);
        }

        public void BlankRow(RelativeRow row)
        {
            row.RowVisible = false;
            row.PositionInClass = 0;
            row.ClassColor = string.Empty;
            row.ClassTextColor = string.Empty;
            row.Number = string.Empty;
            row.Name = string.Empty;
            row.OutLap = false;
            row.iRating = 0;
            row.License = string.Empty;
            row.SafetyRating = 0;
            row.GapToPlayer = 0;
            row.GapToPlayerCombined = string.Empty;
        }
        public bool IsValidRow(Opponent opponent)
        {
            return true;
        }
    }
}
