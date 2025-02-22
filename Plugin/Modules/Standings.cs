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

    public class Driver
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

    public class CarClass
    {
        public const int MaxDrivers = 64;
        public List<Driver> Drivers;

        public CarClass()
        {
            Drivers = new List<Driver>(Enumerable.Range(0, MaxDrivers).Select(x => new Driver()));
        }
    }

    public class Standings : IPluginModule
    {
        private DateTime _lastPacketTime = DateTime.MinValue;
        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(500);

        public StandingsSettings Settings { get; set; }

        public const int MaxCarClasses = 4;
        public List<CarClass> CarClasses;

        public int PlayerCarClassIdx { get; internal set; } = 0;

        public Standings()
        {
            CarClasses = new List<CarClass>(Enumerable.Range(0, MaxCarClasses).Select(x => new CarClass()));
        }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Settings = plugin.ReadCommonSettings<StandingsSettings>("StandingsSettings", () => new StandingsSettings());
            plugin.AttachDelegate(name: $"Standings.PlayerCarClassIdx", valueProvider: () => PlayerCarClassIdx);
            plugin.AttachDelegate(name: "Standings.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);

            for (int carClassIdx = 0; carClassIdx < MaxCarClasses; carClassIdx++)
            {
                CarClass carClass = CarClasses[carClassIdx];
                for (int driverIdx = 0; driverIdx < CarClass.MaxDrivers; driverIdx++)
                {
                    Driver driver = carClass.Drivers[driverIdx];
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.RowVisible", valueProvider: () => driver.RowVisible);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.PlayerID", valueProvider: () => driver.PlayerID);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.Position", valueProvider: () => driver.Position);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.CarNumber", valueProvider: () => driver.CarNumber);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.Name", valueProvider: () => driver.Name);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.IsCarInPitLane", valueProvider: () => driver.IsCarInPitLane);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.OutLap", valueProvider: () => driver.OutLap);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.PitEnterAtLap", valueProvider: () => driver.PitEnterAtLap);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.iRating", valueProvider: () => driver.iRating);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.iRatingText", valueProvider: () => driver.iRatingText);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.CurrentLap", valueProvider: () => driver.CurrentLap);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.LapsToClassLeader", valueProvider: () => driver.LapsToClassLeader);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.GaptoClassLeader", valueProvider: () => driver.GaptoClassLeader);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.TireCompound", valueProvider: () => driver.TireCompound);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.TireCompoundVisible", valueProvider: () => driver.TireCompoundVisible);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Driver{driverIdx:00}.BestLapTime", valueProvider: () => driver.BestLapTime);
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
                CarClass carClass = CarClasses[carClassIdx];
                if (carClassIdx < data.NewData.OpponentsClassses.Count)
                {
                    List<Opponent> opponents = data.NewData.OpponentsClassses[carClassIdx].Opponents;

                    // Find how many rows to skip to have a lead-focused leaderboard
                    int maxRowCount = 0;
                    int skipRowCount = 0;
                    int playerDriverIdx = -1;
                    if (PlayerCarClassIdx == carClassIdx)
                    {
                        maxRowCount = Settings.MaxRowsPlayerClass;

                        // Find the player's driverIdx in the class
                        for (int driverIdx = 0; driverIdx < opponents.Count; driverIdx++)
                        {
                            if (opponents[driverIdx].IsPlayer)
                            {
                                playerDriverIdx = driverIdx;
                                break;
                            }
                        }

                        // Too many rows to be shown
                        if (playerDriverIdx > Settings.MaxRowsPlayerClass - 1)
                        {
                            int shown = Math.Max(0, opponents.Count - Settings.LeadFocusedRows);
                            int before = Math.Max(0, playerDriverIdx - Settings.LeadFocusedRows);
                            int after = Math.Max(0, opponents.Count - playerDriverIdx);

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

                    int rowCount = 0;
                    for (int driverIdx = 0; driverIdx < CarClass.MaxDrivers; driverIdx++)
                    {
                        rowCount++;
                        Driver driver = carClass.Drivers[driverIdx];
                        if (rowCount > maxRowCount)
                        {
                            ResetDriver(driver);
                            continue;
                        }

                        if (driverIdx >= opponents.Count)
                        {
                            ResetDriver(driver);
                            continue;
                        }

                        int actualDriverIdx = driverIdx;
                        if (driverIdx > Settings.LeadFocusedRows)
                        {
                            actualDriverIdx += skipRowCount;
                        }

                        if (actualDriverIdx >= opponents.Count)
                        {
                            ResetDriver(driver);
                            continue;
                        }

                        Opponent opponent = opponents[actualDriverIdx];
                        if (opponent.Position <= 0)
                        {
                            ResetDriver(driver);
                            continue;
                        }
                        
                        driver.RowVisible = true;
                        driver.PlayerID = opponent.Id;
                        driver.Position = opponent.Position;
                        driver.CarNumber = opponent.CarNumber;
                        driver.Name = opponent.Name;
                        driver.IsCarInPitLane = opponent.IsCarInPitLane;
                        driver.OutLap = opponent.IsOutLap;
                        driver.PitEnterAtLap = (int)(opponent.PitEnterAtLap ?? 0);
                        driver.iRating = (int)opponent.IRacing_IRating;
                        driver.iRatingText = FormatIRating(driver.iRating);
                        driver.CurrentLap = opponent.CurrentLap ?? 0;
                        driver.LapsToClassLeader = opponent.LapsToClassLeader ?? 0;
                        driver.GaptoClassLeader = opponent.GaptoClassLeader ?? 0;
                        driver.TireCompound = GetTireCompound(opponent);
                        driver.TireCompoundVisible = GetTireCompoundVisible(opponent);
                        driver.BestLapTime = opponent.BestLapTime;
                    }
                }
                else
                {
                    for (int driverIdx = 0; driverIdx < CarClass.MaxDrivers; driverIdx++)
                    {
                        Driver driver = carClass.Drivers[driverIdx];
                        ResetDriver(driver);
                    }
                }
            }
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("StandingsSettings", Settings);
        }

        public void ResetDriver(Driver driver)
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
