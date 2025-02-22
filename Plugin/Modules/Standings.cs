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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System;

namespace benofficial2.Plugin
{
    public class StandingsSettings : INotifyPropertyChanged
    {
        private int _leadFocusedRows = 3;

        public int LeadFocusedRows
        {
            get { return _leadFocusedRows; }
            set
            {
                if (_leadFocusedRows != value)
                {
                    _leadFocusedRows = value;
                    OnPropertyChanged(nameof(LeadFocusedRows));
                }
            }
        }

        private int _leadFocusedRowsOtherClasses = 3;

        public int MaxRowsOtherClasses
        {
            get { return _leadFocusedRowsOtherClasses; }
            set
            {
                if (_leadFocusedRowsOtherClasses != value)
                {
                    _leadFocusedRowsOtherClasses = value;
                    OnPropertyChanged(nameof(MaxRowsOtherClasses));
                }
            }
        }

        private int _maxRows = 8;

        public int MaxRowsPlayerClass
        {
            get { return _maxRows; }
            set
            {
                if (_maxRows != value)
                {
                    _maxRows = value;
                    OnPropertyChanged(nameof(MaxRowsPlayerClass));
                }
            }
        }

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

    public class StandingRow
    {
        public bool RowVisible { get; set; } = false;
        public string PlayerID { get; set; } = "";
        public int Position { get; set; } = 0;
        public string CarNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsCarInPitLane { get; set; } = false;
        public bool OutLap { get; set; } = false;
        public int PitEnterAtLap { get; set; } = 0;
        public int iRating { get; set; } = 0;
        public string iRatingText { get; set; } = string.Empty;
        public int CurrentLap {  get; set; } = 0;
        public int LapsToClassLeader { get; set; } = 0;
        public double GaptoClassLeader { get; set; } = 0;
        public string TireCompound {  get; set; } = string.Empty;
        public bool TireCompoundVisible { get; set; } = false;
        public TimeSpan BestLapTime { get; set; } = TimeSpan.Zero;
    }

    public class StandingCarClass
    {
        public const int MaxRows = 64;
        public List<StandingRow> Rows;

        public StandingCarClass()
        {
            Rows = new List<StandingRow>(Enumerable.Range(0, MaxRows).Select(x => new StandingRow()));
        }
    }

    public class Standings : IPluginModule
    {
        private DateTime _lastPacketTime = DateTime.MinValue;
        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(500);

        public StandingsSettings Settings { get; set; }

        public const int MaxCarClasses = 4;
        public List<StandingCarClass> CarClasses;

        public int PlayerCarClassIdx { get; internal set; } = 0;

        public Standings()
        {
            CarClasses = new List<StandingCarClass>(Enumerable.Range(0, MaxCarClasses).Select(x => new StandingCarClass()));
        }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Settings = plugin.ReadCommonSettings<StandingsSettings>("StandingsSettings", () => new StandingsSettings());
            plugin.AttachDelegate(name: $"Standings.PlayerCarClassIdx", valueProvider: () => PlayerCarClassIdx);
            plugin.AttachDelegate(name: "Standings.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);

            for (int carClassIdx = 0; carClassIdx < MaxCarClasses; carClassIdx++)
            {
                StandingCarClass carClass = CarClasses[carClassIdx];
                for (int rowIdx = 0; rowIdx < StandingCarClass.MaxRows; rowIdx++)
                {
                    StandingRow driver = carClass.Rows[rowIdx];
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.RowVisible", valueProvider: () => driver.RowVisible);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.PlayerID", valueProvider: () => driver.PlayerID);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.Position", valueProvider: () => driver.Position);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.CarNumber", valueProvider: () => driver.CarNumber);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.Name", valueProvider: () => driver.Name);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.IsCarInPitLane", valueProvider: () => driver.IsCarInPitLane);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.OutLap", valueProvider: () => driver.OutLap);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.PitEnterAtLap", valueProvider: () => driver.PitEnterAtLap);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.iRating", valueProvider: () => driver.iRating);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.iRatingText", valueProvider: () => driver.iRatingText);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.CurrentLap", valueProvider: () => driver.CurrentLap);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.LapsToClassLeader", valueProvider: () => driver.LapsToClassLeader);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.GaptoClassLeader", valueProvider: () => driver.GaptoClassLeader);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.TireCompound", valueProvider: () => driver.TireCompound);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.TireCompoundVisible", valueProvider: () => driver.TireCompoundVisible);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.BestLapTime", valueProvider: () => driver.BestLapTime);
                }
            }
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (data.NewData.PacketTime - _lastPacketTime < _updateInterval) return;
            _lastPacketTime = data.NewData.PacketTime;

            PlayerCarClassIdx = FindPlayerCarClassIdx(ref data);

            for (int carClassIdx = 0; carClassIdx < MaxCarClasses; carClassIdx++)
            {
                StandingCarClass carClass = CarClasses[carClassIdx];
                if (carClassIdx < data.NewData.OpponentsClassses.Count)
                {
                    List<Opponent> opponents = data.NewData.OpponentsClassses[carClassIdx].Opponents;

                    // Find how many rows to skip to have a lead-focused leaderboard
                    int maxRowCount = 0;
                    int skipRowCount = 0;
                    int playerOpponentIdx = -1;
                    if (PlayerCarClassIdx == carClassIdx)
                    {
                        maxRowCount = Settings.MaxRowsPlayerClass;

                        // Find the player's driverIdx in the class
                        for (int opponentIdx = 0; opponentIdx < opponents.Count; opponentIdx++)
                        {
                            if (opponents[opponentIdx].IsPlayer)
                            {
                                playerOpponentIdx = opponentIdx;
                                break;
                            }
                        }

                        // Too many rows to be shown
                        if (playerOpponentIdx > Settings.MaxRowsPlayerClass - 1)
                        {
                            int shown = Math.Max(0, opponents.Count - Settings.LeadFocusedRows);
                            int before = Math.Max(0, playerOpponentIdx - Settings.LeadFocusedRows);
                            int after = Math.Max(0, opponents.Count - playerOpponentIdx);

                            // Center the player in the view by trying to keep as many rows before as after
                            while (shown > Math.Max(0, Settings.MaxRowsPlayerClass - Settings.LeadFocusedRows)) 
                            {
                                if (before > after)
                                {
                                    before--;
                                    shown--;
                                    skipRowCount++;
                                }
                                else
                                {
                                    after--;
                                    shown--;
                                }
                            }
                        }
                    }
                    else
                    {
                        maxRowCount = Settings.MaxRowsOtherClasses;
                    }

                    int visibleRowCount = 0;
                    for (int rowIdx = 0; rowIdx < StandingCarClass.MaxRows; rowIdx++)
                    {
                        StandingRow row = carClass.Rows[rowIdx];
                        if (visibleRowCount >= maxRowCount)
                        {
                            ResetRow(row);
                            continue;
                        }

                        if (rowIdx >= opponents.Count)
                        {
                            ResetRow(row);
                            continue;
                        }

                        int actualDriverIdx = rowIdx;
                        if (rowIdx > Settings.LeadFocusedRows)
                        {
                            actualDriverIdx += skipRowCount;
                        }

                        if (actualDriverIdx >= opponents.Count)
                        {
                            ResetRow(row);
                            continue;
                        }

                        Opponent opponent = opponents[actualDriverIdx];
                        if (opponent.Position <= 0)
                        {
                            ResetRow(row);
                            continue;
                        }
                        
                        row.RowVisible = true;
                        row.PlayerID = opponent.Id;
                        row.Position = opponent.Position;
                        row.CarNumber = opponent.CarNumber;
                        row.Name = opponent.Name;
                        row.IsCarInPitLane = opponent.IsCarInPitLane;
                        row.OutLap = opponent.IsOutLap;
                        row.PitEnterAtLap = (int)(opponent.PitEnterAtLap ?? 0);
                        row.iRating = (int)opponent.IRacing_IRating;
                        row.iRatingText = FormatIRating(row.iRating);
                        row.CurrentLap = opponent.CurrentLap ?? 0;
                        row.LapsToClassLeader = opponent.LapsToClassLeader ?? 0;
                        row.GaptoClassLeader = opponent.GaptoClassLeader ?? 0;
                        row.TireCompound = GetTireCompound(opponent);
                        row.TireCompoundVisible = GetTireCompoundVisible(opponent);
                        row.BestLapTime = opponent.BestLapTime;
                        visibleRowCount++;
                    }
                }
                else
                {
                    for (int driverIdx = 0; driverIdx < StandingCarClass.MaxRows; driverIdx++)
                    {
                        StandingRow driver = carClass.Rows[driverIdx];
                        ResetRow(driver);
                    }
                }
            }
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("StandingsSettings", Settings);
        }

        public void ResetRow(StandingRow driver)
        {
            driver.RowVisible = false;
            driver.PlayerID = "";
            driver.Position = 0;
            driver.CarNumber = "";
            driver.Name = "";
            driver.IsCarInPitLane = false;
            driver.OutLap = false;
            driver.PitEnterAtLap = 0;
            driver.iRating = 0;
            driver.iRatingText = "0.0k";
            driver.CurrentLap = 0;
            driver.LapsToClassLeader = 0;
            driver.GaptoClassLeader = 0;
            driver.TireCompound = string.Empty;
            driver.TireCompoundVisible = false;
            driver.BestLapTime = TimeSpan.Zero;
        }

        public int FindPlayerCarClassIdx(ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return -1;

            int playerCarClassID = 0;
            try { playerCarClassID = (int)raw.Telemetry["PlayerCarClass"]; } catch { }

            for (int carClassIdx = 0; carClassIdx < data.NewData.OpponentsClassses.Count; carClassIdx++)
            {
                var carClass = data.NewData.OpponentsClassses[carClassIdx];
                List<Opponent> opponents = carClass.Opponents;
                if (opponents.Count > 0)
                {
                    if (opponents[0].CarClassID == playerCarClassID.ToString())
                    {
                        return carClassIdx;
                    }
                }
            }

            return -1;
        }

        public int FindPlayerPositionInClass()
        {

            return -1;
        }

        public string FormatIRating(int iRating)
        {
            return (iRating / 1000).ToString("0.0") + "k";
        }

        public string GetTireCompound(Opponent opponent)
        {
            if (opponent.FrontTyreCompoundGameCode == "1")
            {
                return "W";
            }
            return opponent.FrontTyreCompound;
        }

        public bool GetTireCompoundVisible(Opponent opponent)
        {
            if (opponent.FrontTyreCompound.Length == 0) return false;
            if (opponent.FrontTyreCompoundGameCode.Length == 0) return false;
            if (opponent.FrontTyreCompoundGameCode == "-1") return false;
            return true;
        }
    }
}
