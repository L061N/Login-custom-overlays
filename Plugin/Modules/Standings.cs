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
        public bool IsPlayer { get; set; } = false;
        public string PlayerID { get; set; } = string.Empty;
        public bool Connected { get; set; } = false;
        public int LivePositionInClass { get; set; } = 0;
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool InPitLane { get; set; } = false;
        public bool OutLap { get; set; } = false;
        public int EnterPitLap { get; set; } = 0;
        public int iRating { get; set; } = 0;
        public string License {  get; set; } = string.Empty;
        public double SafetyRating { get; set; } = 0;
        public int CurrentLap {  get; set; } = 0;
        public int LapsToClassLeader { get; set; } = 0;
        public double GapToClassLeader { get; set; } = 0;
        public string TireCompound {  get; set; } = string.Empty;
        public bool TireCompoundVisible { get; set; } = false;
        public TimeSpan BestLapTime { get; set; } = TimeSpan.Zero;
    }

    public class StandingCarClass
    {
        public const int MaxRows = 64;
        public List<StandingRow> Rows { get; internal set; }
        public bool LeadFocusedDividerVisible { get; set; } = false;
        public string Color { get; set; } = string.Empty;
        public string TextColor { get; set; } = string.Empty;
        public int Sof { get; set; } = 0;

        public StandingCarClass()
        {
            Rows = new List<StandingRow>(Enumerable.Range(0, MaxRows).Select(x => new StandingRow()));
        }
    }

    public class Standings : IPluginModule
    {
        private DriverModule _driverModule = null;
        private CarModule _carModule = null;
        private SessionModule _sessionModule = null;

        private DateTime _lastUpdateTime = DateTime.MinValue;
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
            _driverModule = plugin.GetModule<DriverModule>();
            _carModule = plugin.GetModule<CarModule>();
            _sessionModule = plugin.GetModule<SessionModule>();

            Settings = plugin.ReadCommonSettings<StandingsSettings>("StandingsSettings", () => new StandingsSettings());
            plugin.AttachDelegate(name: $"Standings.PlayerCarClassIdx", valueProvider: () => PlayerCarClassIdx);
            plugin.AttachDelegate(name: $"Standings.LeadFocusedRows", valueProvider: () => Settings.LeadFocusedRows);
            plugin.AttachDelegate(name: "Standings.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);

            for (int carClassIdx = 0; carClassIdx < MaxCarClasses; carClassIdx++)
            {
                StandingCarClass carClass = CarClasses[carClassIdx];
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.LeadFocusedDividerVisible", valueProvider: () => carClass.LeadFocusedDividerVisible);
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Color", valueProvider: () => carClass.Color);
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.TextColor", valueProvider: () => carClass.TextColor);
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Sof", valueProvider: () => carClass.Sof);

                for (int rowIdx = 0; rowIdx < StandingCarClass.MaxRows; rowIdx++)
                {
                    StandingRow row = carClass.Rows[rowIdx];
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.RowVisible", valueProvider: () => row.RowVisible);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.IsPlayer", valueProvider: () => row.IsPlayer);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.PlayerID", valueProvider: () => row.PlayerID);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.Connected", valueProvider: () => row.Connected);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.LivePositionInClass", valueProvider: () => row.LivePositionInClass);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.Number", valueProvider: () => row.Number);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.Name", valueProvider: () => row.Name);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.InPitLane", valueProvider: () => row.InPitLane);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.OutLap", valueProvider: () => row.OutLap);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.EnterPitLap", valueProvider: () => row.EnterPitLap);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.iRating", valueProvider: () => row.iRating);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.License", valueProvider: () => row.License);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.SafetyRating", valueProvider: () => row.SafetyRating);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.CurrentLap", valueProvider: () => row.CurrentLap);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.LapsToClassLeader", valueProvider: () => row.LapsToClassLeader);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.GapToClassLeader", valueProvider: () => row.GapToClassLeader);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.TireCompound", valueProvider: () => row.TireCompound);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.TireCompoundVisible", valueProvider: () => row.TireCompoundVisible);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.BestLapTime", valueProvider: () => row.BestLapTime);
                }
            }
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (DateTime.Now - _lastUpdateTime < _updateInterval) return;
            _lastUpdateTime = DateTime.Now;

            PlayerCarClassIdx = FindPlayerCarClassIdx(ref data);

            for (int carClassIdx = 0; carClassIdx < MaxCarClasses; carClassIdx++)
            {
                StandingCarClass carClass = CarClasses[carClassIdx];
                if (carClassIdx < data.NewData.OpponentsClassses.Count)
                {
                    LeaderboardCarClassDescription opponentClass = data.NewData.OpponentsClassses[carClassIdx];
                    List<Opponent> opponents = new List<Opponent>(opponentClass.Opponents);

                    // In a race, get a live leaderboard by sorting on track position
                    // TODO: Don't sort before race: show qualification result
                    // Don't sort after race: keep race result
                    if (_sessionModule.Race && !_sessionModule.RaceFinished)
                    {
                        opponents = opponents.OrderByDescending(p => p.CurrentLapHighPrecision).ToList();
                    }

                    carClass.Color = opponentClass.ClassColor;
                    carClass.TextColor = opponentClass.ClassTextColor;
                    carClass.Sof = CalculateSof(opponents);

                    int maxRowCount = 0;
                    int skipRowCount = 0;
                    if (PlayerCarClassIdx == carClassIdx)
                    {
                        maxRowCount = Settings.MaxRowsPlayerClass;

                        // How many rows to skip to have a lead-focused leaderboard
                        skipRowCount = GetLeadFocusedSkipRowCount(opponents);
                    }
                    else
                    {
                        maxRowCount = Settings.MaxRowsOtherClasses;
                    }

                    carClass.LeadFocusedDividerVisible = skipRowCount > 0;

                    int visibleRowCount = 0;
                    for (int rowIdx = 0; rowIdx < StandingCarClass.MaxRows; rowIdx++)
                    {
                        StandingRow row = carClass.Rows[rowIdx];
                        if (visibleRowCount >= maxRowCount)
                        {
                            // Reached row count limit
                            BlankRow(row);
                            continue;
                        }

                        int actualDriverIdx = rowIdx;
                        if (rowIdx > Settings.LeadFocusedRows - 1)
                        {
                            actualDriverIdx += skipRowCount;
                        }

                        if (actualDriverIdx >= opponents.Count)
                        {
                            BlankRow(row);
                            continue;
                        }

                        Opponent opponent = opponents[actualDriverIdx];
                        if (!IsValidRow(opponent))
                        {
                            BlankRow(row);
                            continue;
                        }
                        
                        row.RowVisible = true;
                        row.IsPlayer = opponent.IsPlayer;
                        row.PlayerID = opponent.Id;
                        row.Connected = opponent.IsConnected;

                        if (_sessionModule.Race)
                        {
                            row.LivePositionInClass = actualDriverIdx + 1;
                        }
                        else
                        {
                            row.LivePositionInClass = opponent.Position > 0 ? actualDriverIdx + 1 : 0;
                        }

                        row.Number = opponent.CarNumber;
                        row.Name = opponent.Name;
                        row.InPitLane = opponent.IsCarInPitLane;
                        row.OutLap = opponent.IsOutLap;
                        try { row.EnterPitLap = _driverModule.Drivers?[opponent.CarNumber]?.EnterPitLap ?? 0; }
                        catch { row.EnterPitLap = 0; }
                        row.iRating = (int)(opponent.IRacing_IRating ?? 0);
                        (row.License, row.SafetyRating) = DriverModule.ParseLicenseString(opponent.LicenceString);
                        row.CurrentLap = opponent.CurrentLap ?? 0;
                        row.LapsToClassLeader = opponent.LapsToClassLeader ?? 0;
                        row.GapToClassLeader = opponent.GaptoClassLeader ?? 0;
                        (row.TireCompound, row.TireCompoundVisible) = GetTireCompound(ref data, opponent);
                        row.BestLapTime = opponent.BestLapTime;
                        visibleRowCount++;

                        if (row.IsPlayer)
                        {
                            _driverModule.PlayerLivePositionInClass = row.LivePositionInClass;
                        }
                    }
                }
                else
                {
                    for (int driverIdx = 0; driverIdx < StandingCarClass.MaxRows; driverIdx++)
                    {
                        StandingRow driver = carClass.Rows[driverIdx];
                        BlankRow(driver);
                    }
                }
            }
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("StandingsSettings", Settings);
        }

        public void BlankRow(StandingRow row)
        {
            row.RowVisible = false;
            row.IsPlayer = false;
            row.PlayerID = string.Empty;
            row.Connected = false;
            row.LivePositionInClass = 0;
            row.Number = string.Empty;
            row.Name = string.Empty;
            row.InPitLane = false;
            row.OutLap = false;
            row.EnterPitLap = 0;
            row.iRating = 0;
            row.License = string.Empty;
            row.SafetyRating = 0;
            row.CurrentLap = 0;
            row.LapsToClassLeader = 0;
            row.GapToClassLeader = 0;
            row.TireCompound = string.Empty;
            row.TireCompoundVisible = false;
            row.BestLapTime = TimeSpan.Zero;
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

        public (string compound, bool visible) GetTireCompound(ref GameData data, Opponent opponent)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) return ("", false);

            // TODO Revisit this logic when these cars get wet tires
            if (_carModule.HasDryTireCompounds)
            {
                return (opponent.FrontTyreCompound, opponent.FrontTyreCompound.Length > 0);
            }

            List<int> rawCompounds = null;
            try { rawCompounds = new List<int>(raw.Telemetry["CarIdxTireCompound"]); } catch { }
            if (rawCompounds != null && opponent.FrontTyreCompoundGameCode == "1")
            {
                // Wet enabled car with wet tires
                return ("W", true);
            }

            if (opponent.FrontTyreCompoundGameCode == "-1")
            {
                // Hidden
                return ("", false);
            }

            // Override SimHub's tire compound for cars that don't have dry compounds
            // It should be 'H' by default not 'S'
            // TODO: This logic won't work for multi-class with a mix of cars with/without compounds
            // because Car module is currently only looking at the player car.
            return ("H", true);
        }

        public bool IsValidRow(Opponent opponent)
        {
            return opponent.PositionInClass > 0;
        }

        public int GetValidRowCount(List<Opponent> opponents)
        {
            int validRowCount = 0;
            for (int opponentIdx = 0; opponentIdx < opponents.Count; opponentIdx++)
            {
                if (IsValidRow(opponents[opponentIdx])) { validRowCount++; }               
            }
            return validRowCount;
        }

        public int GetLeadFocusedSkipRowCount(List<Opponent> opponents)
        {
            // Find the player in the opponent list
            int playerOpponentIdx = -1;
            for (int opponentIdx = 0; opponentIdx < opponents.Count; opponentIdx++)
            {
                if (opponents[opponentIdx].IsPlayer)
                {
                    playerOpponentIdx = opponentIdx;
                    break;
                }
            }

            if (playerOpponentIdx < 0 || !IsValidRow(opponents[playerOpponentIdx]))
            {
                // Player not in opponent list
                return 0;
            }

            int validRowCount = GetValidRowCount(opponents);
            if (validRowCount <= Settings.MaxRowsPlayerClass)
            {
                // They all fit in
                return 0;
            }

            int nonLeadFocusedRowCount = Math.Max(0, Settings.MaxRowsPlayerClass - Settings.LeadFocusedRows);
            if (playerOpponentIdx <= Settings.LeadFocusedRows + nonLeadFocusedRowCount / 2)
            {
                // Player already centered in top rows
                return 0;
            }

            // Center the player in the view by trying to keep an equal amount of rows before as after 
            int shown = Math.Max(0, validRowCount - Settings.LeadFocusedRows);
            int before = Math.Max(0, playerOpponentIdx - Settings.LeadFocusedRows);
            int after = Math.Max(0, validRowCount - playerOpponentIdx - 1);         
            int skipRowCount = 0;

            // TODO make this O(1)
            while (shown > nonLeadFocusedRowCount)
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

            return skipRowCount;
        }

        public int CalculateSof(List<Opponent> opponents)
        {
            if (opponents.Count <= 0) return 0;
            double totalSof = 0;
            for (int opponentIdx = 0; opponentIdx < opponents.Count; opponentIdx++)
            {
                totalSof += opponents[opponentIdx].IRacing_IRating ?? 0;
            }
            return (int)(totalSof / opponents.Count);
        }
    }
}
