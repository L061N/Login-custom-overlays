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
using System.Drawing;

namespace benofficial2.Plugin
{
    using OpponentsWithDrivers = List<(Opponent, Driver)>;

    public class StandingsSettings : ModuleSettings
    {
        public bool HideInReplay { get; set; } = true;
        public bool HeaderVisible { get; set; } = true;
        public bool CarClassHeaderVisible { get; set; } = true;
        public int LeadFocusedRows { get; set; } = 3;
        public int MaxRowsOtherClasses { get; set; } = 3;
        public int MaxRowsPlayerClass { get; set; } = 10;
        public bool CarLogoVisible { get; set; } = true;
        public bool CarLogoVisibleInRace { get; set; } = true;
        public bool GapVisibleInRace { get; set; } = true;
        public bool BestVisibleInRace { get; set; } = true;
        public bool LastVisibleInRace { get; set; } = true;
        public bool BestVisible { get; set; } = true;
        public bool LastVisible { get; set; } = true;
        public int AlternateRowBackgroundColor { get; set; } = 5;
        public bool HighlightPlayerRow { get; set; } = true;
        public int HeaderOpacity { get; set; } = 90;
        public int BackgroundOpacity { get; set; } = 7;
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
        public string CarId { get; set; } = string.Empty;
        public string CarBrand { get; set; } = string.Empty;
        public bool InPitLane { get; set; } = false;
        public bool Towing { get; set; } = false;
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
        public TimeSpan LastLapTime { get; set; } = TimeSpan.Zero;
    }

    public class StandingCarClass
    {
        public const int MaxRows = 25;
        public string Name { get; set; } = string.Empty;
        public float NameSize { get; set; } = 0;
        public int VisibleRowCount { get; set; } = 0;
        public List<StandingRow> Rows { get; internal set; }
        public bool LeadFocusedDividerVisible { get; set; } = false;
        public string Color { get; set; } = string.Empty;
        public string TextColor { get; set; } = string.Empty;
        public int Sof { get; set; } = 0;
        public int DriverCount { get; set; } = 0;
        public TimeSpan BestLapTime { get; set; } = TimeSpan.Zero;

        public StandingCarClass()
        {
            Rows = new List<StandingRow>(Enumerable.Range(0, MaxRows).Select(x => new StandingRow()));
        }
    }

    public class StandingsModule : PluginModuleBase
    {
        private DriverModule _driverModule = null;
        private CarModule _carModule = null;
        private SessionModule _sessionModule = null;

        private DateTime _lastUpdateTime = DateTime.MinValue;
        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(500);

        public StandingsSettings Settings { get; set; }

        public const int MaxCarClasses = 4;
        public List<StandingCarClass> CarClasses { get; internal set; }
        public int VisibleClassCount { get; internal set; } = 0;
        public int PlayerCarClassIdx { get; internal set; } = 0;
        public bool CarLogoVisible { get; internal set; } = true;
        public bool GapVisible { get; internal set; } = true;
        public bool BestVisible { get; internal set; } = true;
        public bool LastVisible { get; internal set; } = true;

        public StandingsModule()
        {
            CarClasses = new List<StandingCarClass>(Enumerable.Range(0, MaxCarClasses).Select(x => new StandingCarClass()));
        }
        public override int UpdatePriority => 80;
        public override void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            _driverModule = plugin.GetModule<DriverModule>();
            _carModule = plugin.GetModule<CarModule>();
            _sessionModule = plugin.GetModule<SessionModule>();

            Settings = plugin.ReadCommonSettings<StandingsSettings>("StandingsSettings", () => new StandingsSettings());
            plugin.AttachDelegate(name: $"Standings.HeaderVisible", valueProvider: () => Settings.HeaderVisible);
            plugin.AttachDelegate(name: $"Standings.CarClassHeaderVisible", valueProvider: () => Settings.CarClassHeaderVisible);
            plugin.AttachDelegate(name: $"Standings.VisibleClassCount", valueProvider: () => VisibleClassCount);
            plugin.AttachDelegate(name: $"Standings.PlayerCarClassIdx", valueProvider: () => PlayerCarClassIdx);
            plugin.AttachDelegate(name: $"Standings.HideInReplay", valueProvider: () => Settings.HideInReplay);
            plugin.AttachDelegate(name: $"Standings.LeadFocusedRows", valueProvider: () => Settings.LeadFocusedRows);
            plugin.AttachDelegate(name: $"Standings.MaxRowsPlayerClass", valueProvider: () => Settings.MaxRowsPlayerClass);
            plugin.AttachDelegate(name: $"Standings.MaxRowsOtherClasses", valueProvider: () => Settings.MaxRowsOtherClasses);
            plugin.AttachDelegate(name: $"Standings.CarLogoVisible", valueProvider: () => CarLogoVisible);
            plugin.AttachDelegate(name: $"Standings.GapVisible", valueProvider: () => GapVisible);
            plugin.AttachDelegate(name: $"Standings.BestVisible", valueProvider: () => BestVisible);
            plugin.AttachDelegate(name: $"Standings.LastVisible", valueProvider: () => LastVisible);
            plugin.AttachDelegate(name: "Standings.AlternateRowBackgroundColor", valueProvider: () => Settings.AlternateRowBackgroundColor);
            plugin.AttachDelegate(name: "Standings.HighlightPlayerRow", valueProvider: () => Settings.HighlightPlayerRow);
            plugin.AttachDelegate(name: "Standings.HeaderOpacity", valueProvider: () => Settings.HeaderOpacity);
            plugin.AttachDelegate(name: "Standings.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);

            for (int carClassIdx = 0; carClassIdx < MaxCarClasses; carClassIdx++)
            {
                StandingCarClass carClass = CarClasses[carClassIdx];
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Name", valueProvider: () => carClass.Name);
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.NameSize", valueProvider: () => carClass.NameSize);
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.VisibleRowCount", valueProvider: () => carClass.VisibleRowCount);
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.LeadFocusedDividerVisible", valueProvider: () => carClass.LeadFocusedDividerVisible);
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Color", valueProvider: () => carClass.Color);
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.TextColor", valueProvider: () => carClass.TextColor);
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Sof", valueProvider: () => carClass.Sof);
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.DriverCount", valueProvider: () => carClass.DriverCount);
                plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.BestLapTime", valueProvider: () => carClass.BestLapTime);

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
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.CarId", valueProvider: () => row.CarId);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.CarBrand", valueProvider: () => row.CarBrand);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.InPitLane", valueProvider: () => row.InPitLane);
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.Towing", valueProvider: () => row.Towing);
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
                    plugin.AttachDelegate(name: $"Standings.Class{carClassIdx:00}.Row{rowIdx:00}.LastLapTime", valueProvider: () => row.LastLapTime);
                }
            }
        }

        public override void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (data.FrameTime - _lastUpdateTime < _updateInterval) return;
            _lastUpdateTime = data.FrameTime;

            PlayerCarClassIdx = FindPlayerCarClassIdx(ref data);

            if (_sessionModule.Race)
            {
                CarLogoVisible = Settings.CarLogoVisibleInRace;
                GapVisible = Settings.GapVisibleInRace;
                BestVisible = Settings.BestVisibleInRace;
                LastVisible = Settings.LastVisibleInRace;
            }
            else
            {
                CarLogoVisible = Settings.CarLogoVisible;
                GapVisible = false;
                BestVisible = Settings.BestVisible;
                LastVisible = Settings.LastVisible;
            }

            int visibleClassCount = 0;

            for (int carClassIdx = 0; carClassIdx < MaxCarClasses; carClassIdx++)
            {
                StandingCarClass carClass = CarClasses[carClassIdx];
                if (carClassIdx < _driverModule.LiveClassLeaderboards.Count)
                {
                    LeaderboardCarClassDescription opponentClass = _driverModule.LiveClassLeaderboards[carClassIdx].CarClassDescription;
                    OpponentsWithDrivers opponentsWithDrivers = _driverModule.LiveClassLeaderboards[carClassIdx].Drivers;

                    TimeSpan bestLapTime = TimeSpan.MaxValue;

                    if ((opponentClass.ClassName == null || opponentClass.ClassName.Length == 0 || opponentClass.ClassName == "Hosted All Cars")                            
                        && opponentClass.CarModels.Count == 1)
                    {
                        // Fallback to the car model name when we don't have a class name and there's only 1 car model.
                        carClass.Name = opponentClass.CarModels[0];
                    }
                    else
                    {
                        carClass.Name = _carModule.GetCarClassName(opponentClass.ClassName);
                    }

                    carClass.NameSize = MeasureTextInPixels(carClass.Name);
                    carClass.Color = opponentClass.ClassColor;
                    carClass.TextColor = opponentClass.ClassTextColor;
                    
                    
                    carClass.Sof = CalculateSof(opponentsWithDrivers);
                    carClass.DriverCount = opponentsWithDrivers.Count;

                    int skipRowCount = 0;
                    int maxRowCount;
                    if (PlayerCarClassIdx == carClassIdx)
                    {
                        maxRowCount = Settings.MaxRowsPlayerClass;

                        // How many rows to skip to have a lead-focused leaderboard
                        skipRowCount = GetLeadFocusedSkipRowCount(opponentsWithDrivers);
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

                        if (actualDriverIdx >= opponentsWithDrivers.Count)
                        {
                            BlankRow(row);
                            continue;
                        }

                        Opponent opponent = opponentsWithDrivers[actualDriverIdx].Item1;
                        Driver driver = opponentsWithDrivers[actualDriverIdx].Item2;
                        if (!IsValidRow(opponent))
                        {
                            BlankRow(row);
                            continue;
                        }

                        row.RowVisible = true;
                        row.IsPlayer = opponent.IsPlayer;
                        row.PlayerID = opponent.Id;
                        row.Connected = opponent.IsConnected;
                        row.LivePositionInClass = driver.LivePositionInClass;
                        row.Number = opponent.CarNumber;
                        row.Name = opponent.Name;
                        row.CarId = driver.CarId;
                        row.CarBrand = _carModule.GetCarBrand(driver.CarId, opponent.CarName);
                        row.InPitLane = opponent.IsCarInPitLane;
                        row.Towing = driver.Towing;
                        row.OutLap = driver.OutLap;
                        row.EnterPitLap = driver.EnterPitLap;
                        row.iRating = (int)(opponent.IRacing_IRating ?? 0);
                        (row.License, row.SafetyRating) = DriverModule.ParseLicenseString(opponent.LicenceString);
                        row.CurrentLap = opponent.CurrentLap ?? 0;
                        row.LapsToClassLeader = opponent.LapsToClassLeader ?? 0;
                        row.GapToClassLeader = opponent.GaptoClassLeader ?? 0;
                        (row.TireCompound, row.TireCompoundVisible) = GetTireCompound(ref data, opponent);
                        row.BestLapTime = driver.BestLapTime;
                        row.LastLapTime = driver.LastLapTime;

                        // Make sure we have a best lap time for the first lap of a race
                        // iRacing often doesn't provide a valid best lap time for lap 1
                        if (_sessionModule.Race && row.BestLapTime <= TimeSpan.Zero && row.LastLapTime > TimeSpan.Zero)
                        {
                            row.BestLapTime = row.LastLapTime;
                        }

                        // Determine the class best lap time
                        if (row.BestLapTime > TimeSpan.Zero && row.BestLapTime < bestLapTime)
                        {
                            bestLapTime = row.BestLapTime;
                        }

                        visibleRowCount++;
                    }

                    carClass.VisibleRowCount = visibleRowCount;
                    carClass.BestLapTime = bestLapTime < TimeSpan.MaxValue ? bestLapTime : TimeSpan.Zero;
                    visibleClassCount++;
                }
                else
                {
                    BlankCarClass(carClass);
                }
            }

            VisibleClassCount = visibleClassCount;
        }

        public override void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("StandingsSettings", Settings);
        }

        public void BlankCarClass(StandingCarClass carClass)
        {
            carClass.Name = string.Empty;
            carClass.NameSize = 0;
            carClass.VisibleRowCount = 0;
            carClass.LeadFocusedDividerVisible = false;
            carClass.Color = string.Empty;
            carClass.TextColor = string.Empty;
            carClass.Sof = 0;
            carClass.DriverCount = 0;
            carClass.BestLapTime = TimeSpan.Zero;

            for (int driverIdx = 0; driverIdx < StandingCarClass.MaxRows; driverIdx++)
            {
                StandingRow driver = carClass.Rows[driverIdx];
                BlankRow(driver);
            }
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
            row.CarId = string.Empty;
            row.CarBrand = string.Empty;
            row.InPitLane = false;
            row.Towing = false;
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
            row.LastLapTime = TimeSpan.Zero;
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

        public int GetValidRowCount(OpponentsWithDrivers opponentsWithDrivers)
        {
            int validRowCount = 0;
            for (int opponentIdx = 0; opponentIdx < opponentsWithDrivers.Count; opponentIdx++)
            {
                if (IsValidRow(opponentsWithDrivers[opponentIdx].Item1)) { validRowCount++; }               
            }
            return validRowCount;
        }

        public int GetLeadFocusedSkipRowCount(OpponentsWithDrivers opponentsWithDrivers)
        {
            // Find the player in the opponent list
            int playerOpponentIdx = -1;
            for (int opponentIdx = 0; opponentIdx < opponentsWithDrivers.Count; opponentIdx++)
            {
                if (opponentsWithDrivers[opponentIdx].Item1.IsPlayer)
                {
                    playerOpponentIdx = opponentIdx;
                    break;
                }
            }

            if (playerOpponentIdx < 0 || !IsValidRow(opponentsWithDrivers[playerOpponentIdx].Item1))
            {
                // Player not in opponent list
                return 0;
            }

            int validRowCount = GetValidRowCount(opponentsWithDrivers);
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

        public int CalculateSof(OpponentsWithDrivers opponentsWithDrivers)
        {
            if (opponentsWithDrivers.Count <= 0) return 0;
            double totalSof = 0;
            for (int opponentIdx = 0; opponentIdx < opponentsWithDrivers.Count; opponentIdx++)
            {
                totalSof += opponentsWithDrivers[opponentIdx].Item1.IRacing_IRating ?? 0;
            }
            return (int)(totalSof / opponentsWithDrivers.Count);
        }

        public float MeasureTextInPixels(string text)
        {
            SizeF textSize;
            Font font = new Font("Roboto", 16);

            using (Bitmap bitmap = new Bitmap(1, 1))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                textSize = graphics.MeasureString(text, font);
            }

            font.Dispose();
            return textSize.Width;
        }
    }
}
