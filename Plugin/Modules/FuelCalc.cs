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
using SimHub.Plugins.DataPlugins.DataCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime;

namespace benofficial2.Plugin
{
    public class FuelCalcSettings : ModuleSettings
    {
        public int BackgroundOpacity { get; set; } = 60;
        public ModuleSettingFloat FuelReserveLiters { get; set; } = new ModuleSettingFloat(0.4f);
        public ModuleSettingFloat ExtraRaceLaps { get; set; } = new ModuleSettingFloat(0.0f);
        public ModuleSettingFloat ExtraRaceLapsOval { get; set; } = new ModuleSettingFloat(2.0f);
        public ModuleSettingFloat ExtraConsumption { get; set; } = new ModuleSettingFloat(1.0f);
        public bool FuelRemainingInfoVisible { get; set; } = true;
        public bool PitStopInfoVisible { get; set; } = true;
        public bool ConsumptionInfoVisible { get; set; } = true;
        public bool EnablePreRaceWarning { get; set; } = true;
        public bool EvenFuelStints { get; set; } = false;

        // Legacy properties for backwards compatibility (saved pre 3.0)
        public string FuelReserveString { get => FuelReserveLiters.ValueString; set => FuelReserveLiters.ValueString = value; }
        public string ExtraLapsString { get => ExtraRaceLaps.ValueString; set => ExtraRaceLaps.ValueString = value; }
        public string ExtraConsumptionPctString { get => ExtraConsumption.ValueString; set => ExtraConsumption.ValueString = value; }
    }

    public class FuelCalcSession
    {
        public string TypeName { get; set; } = string.Empty;
        public string SubTypeName { get; set; } = string.Empty;
        public int Laps { get; set; } = 0;
        public TimeSpan Time { get; set; } = TimeSpan.Zero;
        public bool LimitedByTime { get; set; } = false;
        public double FuelNeeded { get; set; } = 0.0;
        public int StopsNeeded { get; set; } = 0;
    }

    public class FuelCalcModule : PluginModuleBase
    {
        private DriverModule _driverModule = null;
        private SessionModule _sessionModule = null;
        private TrackModule _trackModule = null;
        private StandingsModule _standingsModule = null;
        private CarModule _carModule = null;

        private DateTime _lastUpdateTime = DateTime.MinValue;
        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(1000);

        private string _lastCarTrackCombo = string.Empty;

        // Maximum amount of time remaining (in percentage of best lap) for the white flag to be shown.
        // It is unknown what is the exact rule used by iRacing. Could be 60% of avg from last 3 race laps.
        private const double WhiteFlagRuleLapPct = 0.65;

        // How long is the short parade lap at supported tracks (as a percentage of a lap).
        // This is the default values for tracks we don't have in the database.
        private const double ShortParadeLapPct = 0.50;

        public FuelCalcSettings Settings { get; set; }
        public bool StartFuelCalculatorVisible { get; internal set; } = true;
        public TimeSpan BestLapTime { get; internal set; } = TimeSpan.Zero;
        public double SetupFuelLevel { get; internal set; } = 0.0;
        public double Fuel { get; internal set; } = 0.0;
        public string Units { get; internal set; } = "L";
        public double ConvertFromLiters { get; internal set; } = 1.0;
        public double ConsumptionPerLapSafe { get; internal set; } = 0.0;
        public double ConsumptionPerLapAvg { get; internal set; } = 0.0;
        public double ConsumptionLastLap { get; internal set; } = 0.0;
        public double ConsumptionTargetForExtraLap { get; internal set; } = 0.0;
        public double RemainingLaps { get; internal set; } = 0.0;
        public int EstimatedTotalLaps { get; internal set; } = 0;
        public int PitLap { get; internal set; } = 0;
        public bool PitIndicatorOn { get; internal set; } = false;
        public int PitWindowLap { get; internal set; } = 0;
        public bool PitWindowIndicatorOn { get; internal set; } = false;
        public int PitStopsNeeded { get; internal set; } = 0;
        public double RefuelNeeded { get; internal set; } = 0.0;
        public double ExtraFuelAtFinish { get; internal set; } = 0.0;
        public bool WarningVisible { get; internal set; } = false;

        public const int MaxSessions = 6;
        public List<FuelCalcSession> Sessions { get; internal set; }

        public override void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            _driverModule = plugin.GetModule<DriverModule>();
            _sessionModule = plugin.GetModule<SessionModule>();
            _trackModule = plugin.GetModule<TrackModule>();
            _standingsModule = plugin.GetModule<StandingsModule>();
            _carModule = plugin.GetModule<CarModule>();

            Settings = plugin.ReadCommonSettings<FuelCalcSettings>("FuelCalcSettings", () => new FuelCalcSettings());
            plugin.AttachDelegate(name: "FuelCalc.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);
            plugin.AttachDelegate(name: "FuelCalc.FuelReserve", valueProvider: () => Settings.FuelReserveLiters.Value);
            plugin.AttachDelegate(name: "FuelCalc.ExtraLaps", valueProvider: () => Settings.ExtraRaceLaps.Value);
            plugin.AttachDelegate(name: "FuelCalc.ExtraLapsOval", valueProvider: () => Settings.ExtraRaceLapsOval.Value);
            plugin.AttachDelegate(name: "FuelCalc.ExtraConsumptionPct", valueProvider: () => Settings.ExtraConsumption.Value);
            plugin.AttachDelegate(name: "FuelCalc.FuelRemainingInfoVisible", valueProvider: () => Settings.FuelRemainingInfoVisible);
            plugin.AttachDelegate(name: "FuelCalc.PitStopInfoVisible", valueProvider: () => Settings.PitStopInfoVisible);
            plugin.AttachDelegate(name: "FuelCalc.ConsumptionInfoVisible", valueProvider: () => Settings.ConsumptionInfoVisible);
            plugin.AttachDelegate(name: "FuelCalc.EnablePreRaceWarning", valueProvider: () => Settings.EnablePreRaceWarning);
            plugin.AttachDelegate(name: "FuelCalc.EvenFuelStints", valueProvider: () => Settings.EvenFuelStints);
            plugin.AttachDelegate(name: "FuelCalc.Visible", valueProvider: () => StartFuelCalculatorVisible);
            plugin.AttachDelegate(name: "FuelCalc.BestLapTime", valueProvider: () => BestLapTime);
            plugin.AttachDelegate(name: "FuelCalc.SetupFuelLevel", valueProvider: () => SetupFuelLevel);
            plugin.AttachDelegate(name: "FuelCalc.Fuel", valueProvider: () => Fuel);
            plugin.AttachDelegate(name: "FuelCalc.Units", valueProvider: () => Units);
            plugin.AttachDelegate(name: "FuelCalc.ConsumptionPerLap", valueProvider: () => ConsumptionPerLapSafe);
            plugin.AttachDelegate(name: "FuelCalc.ConsumptionLastLap", valueProvider: () => ConsumptionLastLap);
            plugin.AttachDelegate(name: "FuelCalc.ConsumptionTargetForExtraLap", valueProvider: () => ConsumptionTargetForExtraLap);
            plugin.AttachDelegate(name: "FuelCalc.RemainingLaps", valueProvider: () => RemainingLaps);
            plugin.AttachDelegate(name: "FuelCalc.EstimatedTotalLaps", valueProvider: () => EstimatedTotalLaps);
            plugin.AttachDelegate(name: "FuelCalc.PitLap", valueProvider: () => PitLap);
            plugin.AttachDelegate(name: "FuelCalc.PitIndicatorOn", valueProvider: () => PitIndicatorOn);
            plugin.AttachDelegate(name: "FuelCalc.PitWindowLap", valueProvider: () => PitWindowLap);
            plugin.AttachDelegate(name: "FuelCalc.PitWindowIndicatorOn", valueProvider: () => PitWindowIndicatorOn);
            plugin.AttachDelegate(name: "FuelCalc.PitStopsNeeded", valueProvider: () => PitStopsNeeded);
            plugin.AttachDelegate(name: "FuelCalc.RefuelNeeded", valueProvider: () => RefuelNeeded);
            plugin.AttachDelegate(name: "FuelCalc.ExtraFuelAtFinish", valueProvider: () => ExtraFuelAtFinish);
            plugin.AttachDelegate(name: "FuelCalc.WarningVisible", valueProvider: () => WarningVisible);

            Sessions = new List<FuelCalcSession>(Enumerable.Range(0, MaxSessions).Select(x => new FuelCalcSession()));
            for (int sessionIdx = 0; sessionIdx < MaxSessions; sessionIdx++)
            {
                FuelCalcSession session = Sessions[sessionIdx];
                plugin.AttachDelegate(name: $"FuelCalc.Session{sessionIdx:00}.TypeName", valueProvider: () => session.TypeName);
                plugin.AttachDelegate(name: $"FuelCalc.Session{sessionIdx:00}.SubTypeName", valueProvider: () => session.SubTypeName);
                plugin.AttachDelegate(name: $"FuelCalc.Session{sessionIdx:00}.Laps", valueProvider: () => session.Laps);
                plugin.AttachDelegate(name: $"FuelCalc.Session{sessionIdx:00}.Time", valueProvider: () => session.Time);
                plugin.AttachDelegate(name: $"FuelCalc.Session{sessionIdx:00}.LimitedByTime", valueProvider: () => session.LimitedByTime);
                plugin.AttachDelegate(name: $"FuelCalc.Session{sessionIdx:00}.FuelNeeded", valueProvider: () => session.FuelNeeded);
                plugin.AttachDelegate(name: $"FuelCalc.Session{sessionIdx:00}.StopsNeeded", valueProvider: () => session.StopsNeeded);
            }
        }

        public override void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (data.FrameTime - _lastUpdateTime < _updateInterval) return;
            _lastUpdateTime = data.FrameTime;

            RawDataHelper.TryGetTelemetryData<bool>(ref data, out bool isGarageVisible, "IsGarageVisible");

            StartFuelCalculatorVisible = isGarageVisible;
            UpdateWarningVisible(ref data);

            // Remember the last valid consumption & time for this car/track combo
            // So if we immediately come back to the same car/track combo, we can reuse the values
            string carTrackCombo = data.NewData.CarId + "_" + data.NewData.TrackId;
            bool carTrackComboValid = data.NewData.CarId.Length > 0 && data.NewData.TrackId.Length > 0;
            if (carTrackComboValid && carTrackCombo != _lastCarTrackCombo)
            {
                ConsumptionPerLapAvg = 0.0;
                ConsumptionPerLapSafe = 0.0;
                BestLapTime = TimeSpan.Zero;
                _lastCarTrackCombo = carTrackCombo;
            }

            UpdateSetupFuelLevel(ref data);
            UpdateConsumptionPerLap(pluginManager, ref data);

            bool beforeRaceStart = _sessionModule.Race && !_sessionModule.RaceStarted;
            if (StartFuelCalculatorVisible || beforeRaceStart)
            {
                UpdateBestLapTime(ref data);
                UpdateAllSessionFuel(ref data);
            }

            UpdateFuelCalculations(ref data);
        }

        public override void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("FuelCalcSettings", Settings);
        }

        private void UpdateBestLapTime(ref GameData data)
        {
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null) 
                return;

            int driverCount = 0;
            try { driverCount = (int)raw.AllSessionData["DriverInfo"]["Drivers"].Count; } catch { Debug.Assert(false); }

            int playerCarIdx = -1;
            try { playerCarIdx = int.Parse(raw.AllSessionData["DriverInfo"]["DriverCarIdx"]); } catch { Debug.Assert(false); }

            if (playerCarIdx < 0 || playerCarIdx >= driverCount) 
                return;

            string playerClassId = data.NewData.CarClass;
            try { playerClassId = raw.AllSessionData["DriverInfo"]["Drivers"][playerCarIdx]["CarClassID"]; } catch { Debug.Assert(false); }

            // Try to find the fastest time of any session
            int sessionCount = 0;
            try { sessionCount = (int)raw.AllSessionData["SessionInfo"]["Sessions"].Count; } catch { Debug.Assert(false); }

            double fastestTime = 0;
            for (int sessionIdx = 0; sessionIdx < sessionCount; sessionIdx++)
            {
                List<object> positions = null;
                try { positions = raw.AllSessionData["SessionInfo"]["Sessions"][sessionIdx]["ResultsPositions"]; } catch { Debug.Assert(false); }
                if (positions == null) 
                    continue;

                for (int posIdx = 0; posIdx < positions.Count; posIdx++)
                {
                    int carIdx = -1;
                    try { carIdx = int.Parse(raw.AllSessionData["SessionInfo"]["Sessions"][sessionIdx]["ResultsPositions"][posIdx]["CarIdx"]); } catch { Debug.Assert(false); }

                    if (!_driverModule.DriversByCarIdx.TryGetValue(carIdx, out Driver driver))
                    {
                        continue;
                    }

                    string classId = string.Empty;
                    try { classId = raw.AllSessionData["DriverInfo"]["Drivers"][driver.DriverInfoIdx]["CarClassID"]; } catch { Debug.Assert(false); }

                    // Must be in same class as player
                    if (playerClassId == null || playerClassId == classId)
                    {
                        double timeSecs = 0;
                        try { timeSecs = double.Parse(raw.AllSessionData["SessionInfo"]["Sessions"][sessionIdx]["ResultsPositions"][posIdx]["FastestTime"]); } catch { Debug.Assert(false); }

                        if (timeSecs > 0 && (timeSecs < fastestTime || fastestTime == 0))
                        {
                            fastestTime = timeSecs;
                        }
                    }
                }
            }

            if (fastestTime > 0)
            {
                BestLapTime = TimeSpan.FromSeconds(fastestTime);
            }
        }

        private void UpdateSetupFuelLevel(ref GameData data)
        {
            string fuelLevel = GetSetupFuelLevel(ref data);

            if (fuelLevel.IndexOf("L") != -1)
            {
                Units = "L";
                ConvertFromLiters = 1.0;
                SetupFuelLevel = double.Parse(fuelLevel.Substring(0, fuelLevel.Length - 2), CultureInfo.InvariantCulture);
            }
            else if (fuelLevel.IndexOf("Kg") != -1)
            {
                Units = "Kg";

                double driverCarFuelKgPerLtr;
                RawDataHelper.TryGetSessionData<double>(ref data, out driverCarFuelKgPerLtr, "DriverInfo", "DriverCarFuelKgPerLtr");
                
                ConvertFromLiters = driverCarFuelKgPerLtr > 0 ? driverCarFuelKgPerLtr : 0.7438;
                SetupFuelLevel = double.Parse(fuelLevel.Substring(0, fuelLevel.Length - 3), CultureInfo.InvariantCulture);
            }
            else
            {
                Units = "L";
                ConvertFromLiters = 1.0;
                SetupFuelLevel = 0.0;
            }
        }

        private string GetSetupFuelLevel(ref GameData data)
        {
            // Search for fuel level in various places in the car setup.
            string fuelLevel;
            if (RawDataHelper.TryGetSessionData<string>(ref data, out fuelLevel, "CarSetup", "Suspension", "Rear", "FuelLevel"))
                return fuelLevel;

            if (RawDataHelper.TryGetSessionData<string>(ref data, out fuelLevel, "CarSetup", "Chassis", "Rear", "FuelLevel"))
                return fuelLevel;

            if (RawDataHelper.TryGetSessionData<string>(ref data, out fuelLevel, "CarSetup", "Chassis", "Front", "FuelLevel"))
                return fuelLevel;

            if (RawDataHelper.TryGetSessionData<string>(ref data, out fuelLevel, "CarSetup", "Chassis", "BrakesInCarMisc", "FuelLevel"))
                return fuelLevel;

            if (RawDataHelper.TryGetSessionData<string>(ref data, out fuelLevel, "CarSetup", "BrakesDriveUnit", "Fuel", "FuelLevel"))
                return fuelLevel;

            if (RawDataHelper.TryGetSessionData<string>(ref data, out fuelLevel, "CarSetup", "InCarSystems", "Fuel", "FuelLevel"))
                return fuelLevel;

            if (RawDataHelper.TryGetSessionData<string>(ref data, out fuelLevel, "CarSetup", "Systems", "Fuel", "FuelLevel"))
                return fuelLevel;

            if (RawDataHelper.TryGetSessionData<string>(ref data, out fuelLevel, "CarSetup", "VehicleSystems", "Fuel", "FuelLevel"))
                return fuelLevel;

            if (RawDataHelper.TryGetSessionData<string>(ref data, out fuelLevel, "CarSetup", "TiresFuel", "Fuel", "FuelLevel"))
                return fuelLevel;

            return string.Empty;
        }

        private void UpdateConsumptionPerLap(PluginManager pluginManager, ref GameData data)
        {
            var dataCorePlugin = pluginManager.GetPlugin<DataCorePlugin>();
            double fuelLitersPerLap = dataCorePlugin.properties.Computed_Fuel_LitersPerLap.Value;
            double fuelLitersLastLap = dataCorePlugin.properties.Computed_Fuel_LastLapConsumption.Value;
                        
            ConsumptionLastLap = fuelLitersLastLap * ConvertFromLiters;

            if (fuelLitersPerLap > 0)
            {
                ConsumptionPerLapAvg = fuelLitersPerLap * ConvertFromLiters;
                ConsumptionPerLapSafe = fuelLitersPerLap * (1 + Settings.ExtraConsumption / 100.0) * ConvertFromLiters;
            }
        }

        private void UpdateAllSessionFuel(ref GameData data)
        {
            for (int sessionIdx = 0; sessionIdx < MaxSessions; sessionIdx++)
            {
                UpdateSessionFuel(ref data, sessionIdx);
            }
        }

        private void UpdateSessionFuel(ref GameData data, int sessionIdx)
        {
            if (sessionIdx < 0 || sessionIdx >= MaxSessions)
                return;

            FuelCalcSession session = Sessions[sessionIdx];

            RawDataHelper.TryGetSessionData<List<object>>(ref data, out List<object> sessions, "SessionInfo", "Sessions");           
            if (sessions == null || sessionIdx >= sessions.Count)
            {
                BlankSession(session);
                return;
            }

            RawDataHelper.TryGetSessionData<string>(ref data, out string sessionType, "SessionInfo", "Sessions", sessionIdx, "SessionType");
            session.TypeName = sessionType;

            RawDataHelper.TryGetSessionData<string>(ref data, out string sessionSubType, "SessionInfo", "Sessions", sessionIdx, "SessionSubType");
            session.SubTypeName = sessionSubType;

            RawDataHelper.TryGetSessionData<string>(ref data, out string sessionLaps, "SessionInfo", "Sessions", sessionIdx, "SessionLaps");
            if (sessionLaps.IndexOf("unlimited") != -1)
            {
                session.Laps = -1;
            }
            else
            {
                session.Laps = int.Parse(sessionLaps);
            }

            RawDataHelper.TryGetSessionData<string>(ref data, out string sessionTime, "SessionInfo", "Sessions", sessionIdx, "SessionTime");
            if (sessionTime.IndexOf("unlimited") != -1 || sessionTime.Length <= 4)
            {
                session.Time = TimeSpan.Zero;
            }
            else
            {
                double sessionTimeSecs = double.Parse(sessionTime.Substring(0, sessionTime.Length - 4));
                session.Time = TimeSpan.FromSeconds(sessionTimeSecs);
            }

            if (session.Laps < 0 && session.Time == TimeSpan.MinValue)
            {
                session.FuelNeeded = 0.0;
                session.StopsNeeded = 0;
                return;
            }

            TimeSpan minTimeForLaps = TimeSpan.Zero;
            if (session.Laps > 0 && BestLapTime > TimeSpan.Zero)
            {
                minTimeForLaps = TimeSpan.FromSeconds(session.Laps * BestLapTime.TotalSeconds);
            }

            if (session.Laps > 0 && (session.Time <= TimeSpan.Zero || minTimeForLaps < session.Time))
            {
                session.LimitedByTime = false;
                session.FuelNeeded = ConsumptionPerLapSafe * session.Laps;
            }
            else
            {
                session.LimitedByTime = true;
                if (BestLapTime <= TimeSpan.Zero)
                {
                    // Can't compute fuel without a best lap time.
                    session.FuelNeeded = 0.0;
                    session.StopsNeeded = 0;
                    return;
                }

                double lapsEstimate = Math.Ceiling(session.Time.TotalSeconds / BestLapTime.TotalSeconds);

                // Add an extra lap if we would cross the line with more than X% of a lap remaining
                // It is unknown what is the exact rule used by iRacing. Could be 60% of avg from last 3 race laps.
                double remainingTimeSecs = session.Time.TotalSeconds % BestLapTime.TotalSeconds;
                if (remainingTimeSecs > WhiteFlagRuleLapPct * BestLapTime.TotalSeconds)
                {
                    lapsEstimate++;
                }

                session.FuelNeeded = ConsumptionPerLapSafe * lapsEstimate;
            }

            double outLaps = 0;
            if (session.TypeName.IndexOf("Qual") != -1)
            {
                // Add an outlap for qualifying.
                // At some tracks (e.g. Nurburgring) the outlap is shorter.
                outLaps = 1 - _trackModule.QualStartTrackPct;
            }
            else if (session.TypeName.IndexOf("Race") != -1)
            {
                if (!_sessionModule.StandingStart)
                {
                    if (_sessionModule.Oval)
                    {
                        if (_trackModule.RaceStartTrackPct > 0.0f)
                        {
                            outLaps = 1 - _trackModule.RaceStartTrackPct;
                        }
                        else if (_sessionModule.ShortParadeLap)
                        {
                            outLaps = ShortParadeLapPct;
                        }
                        else if (_trackModule.TrackType == "super speedway")
                        {
                            outLaps = 1.0;
                        }
                        else
                        {
                            outLaps = 2.0;
                        }
                    }
                    else
                    {
                        // Add an outlap for the formation lap on road courses.
                        // At some tracks (e.g. Nurburgring) the formation lap is shorter.
                        outLaps = 1 - _trackModule.RaceStartTrackPct;
                    }
                }
            }

            session.FuelNeeded += ConsumptionPerLapSafe * outLaps;

            if (session.FuelNeeded > 0 && ConsumptionPerLapSafe > 0)
            {
                session.FuelNeeded += Settings.FuelReserveLiters * ConvertFromLiters;

                if (session.TypeName.IndexOf("Race") != -1)
                {
                    if (_sessionModule.Oval)
                    {
                        session.FuelNeeded += ConsumptionPerLapSafe * Settings.ExtraRaceLapsOval;
                    }
                    else
                    {
                        session.FuelNeeded += ConsumptionPerLapSafe * Settings.ExtraRaceLaps;
                    }                        
                }
            }

            double maxFuelTank = data.NewData.MaxFuel * ConvertFromLiters;
            double maxFuel = maxFuelTank * _sessionModule.MaxFuelPct;

            session.StopsNeeded = (int)Math.Floor(session.FuelNeeded / maxFuel);
            if (session.StopsNeeded >= 1)
            {
                if (Settings.EvenFuelStints)
                {
                    // Don't fill up the tank to the maximum so that each stint is evenly fueled.
                    int stints = Math.Max(1, session.StopsNeeded + 1);
                    session.FuelNeeded = Math.Min(session.FuelNeeded / stints, maxFuel);
                }
                else
                {
                    session.FuelNeeded = maxFuel;
                }
            }
        }

        private void UpdateFuelCalculations(ref GameData data)
        {
            Fuel = data.NewData.Fuel * ConvertFromLiters;

            if (ConsumptionPerLapAvg <= 0.0 || _standingsModule.HighlightedCarClassIdx < 0 || _standingsModule.HighlightedCarClassIdx >= StandingsModule.MaxCarClasses)
            {
                BlankFuelCalculations();
                return;
            }            
            
            double fuelReserve = Settings.FuelReserveLiters.Value * ConvertFromLiters;
            double fuelLeftSafe = Math.Max(0.0, Fuel - fuelReserve);
            RemainingLaps = fuelLeftSafe / ConsumptionPerLapAvg;

            // Determine the lap when we run out of fuel, and pit the lap before that.
            double currentLapHighPrecision = _driverModule.HighlightedDriver.CurrentLapHighPrecision;
            PitLap = Math.Max(1, (int)Math.Floor(currentLapHighPrecision + RemainingLaps));
            PitIndicatorOn = _carModule.HasRefueling ? currentLapHighPrecision >= (PitLap - 1) : false;

            EstimatedTotalLaps = _standingsModule.CarClasses[_standingsModule.HighlightedCarClassIdx].EstimatedTotalLaps;
            int lapsToFinishAfterNextStop = Math.Max(0, EstimatedTotalLaps - PitLap);
            if (lapsToFinishAfterNextStop > 0)
            {
                ExtraFuelAtFinish = 0.0;

                double lapsToNextStop = Math.Max(0.0, PitLap - currentLapHighPrecision);
                double fuelToNextStop = lapsToNextStop * ConsumptionPerLapAvg;
                double fuelLeftAtStop = Math.Max(0.0, Fuel - fuelToNextStop);

                double fuelToFinishAfterNextStop = (lapsToFinishAfterNextStop * ConsumptionPerLapAvg) + fuelReserve;

                if (_sessionModule.Race)
                {
                    if (_sessionModule.Oval)
                        fuelToFinishAfterNextStop += (Settings.ExtraRaceLapsOval.Value * ConsumptionPerLapAvg);
                    else
                        fuelToFinishAfterNextStop += (Settings.ExtraRaceLaps.Value * ConsumptionPerLapAvg);
                }

                double maxFuelTank = data.NewData.MaxFuel * ConvertFromLiters;
                double maxFuel = maxFuelTank * _sessionModule.MaxFuelPct;
                PitStopsNeeded = (int)Math.Floor(fuelToFinishAfterNextStop / maxFuel) + 1;

                if (Settings.EvenFuelStints)
                {
                    // Don't fill up the tank to the maximum so that each stint is evenly fueled.
                    RefuelNeeded = Math.Min(Math.Max(0.0, (fuelToFinishAfterNextStop / PitStopsNeeded) - fuelLeftAtStop), maxFuel);
                }
                else
                {
                    RefuelNeeded = Math.Min(Math.Max(0.0, fuelToFinishAfterNextStop - fuelLeftAtStop), maxFuel);
                }

                double maxRefuel = maxFuel * PitStopsNeeded;
                double maxLapsAfterRefuel = Math.Max(0.0, maxRefuel / ConsumptionPerLapAvg);
                PitWindowLap = Math.Max(1, EstimatedTotalLaps - (int)Math.Floor(maxLapsAfterRefuel));
                PitWindowIndicatorOn = _carModule.HasRefueling ? currentLapHighPrecision >= (PitWindowLap - 1) : false;
            }
            else
            {
                PitStopsNeeded = 0;
                RefuelNeeded = 0.0;
                PitWindowLap = 0;
                PitWindowIndicatorOn = false;

                if (EstimatedTotalLaps > 0)
                {
                    double lapsToFinish = Math.Max(0.0, EstimatedTotalLaps - currentLapHighPrecision);
                    double fuelToFinish = lapsToFinish * ConsumptionPerLapAvg;

                    ExtraFuelAtFinish = Math.Max(0.0, Fuel - fuelToFinish);
                }
                else
                {
                    ExtraFuelAtFinish = 0.0;
                }
            }

            // Calculate the consumption target for an extra lap.
            double lapsToNextStopExtra = Math.Max(0.0, (PitLap + 1) - currentLapHighPrecision);
            ConsumptionTargetForExtraLap = (lapsToNextStopExtra > 0) ? (fuelLeftSafe / lapsToNextStopExtra) : 0.0;
        }

        private void BlankFuelCalculations()
        {
            EstimatedTotalLaps = 0;
            ConsumptionTargetForExtraLap = 0.0;
            RemainingLaps = 0.0;
            PitLap = 0;
            PitIndicatorOn = false;
            PitWindowLap = 0;
            PitWindowIndicatorOn = false;
            PitStopsNeeded = 0;
            RefuelNeeded = 0.0;
            ExtraFuelAtFinish = 0.0;
        }

        private void BlankSession(FuelCalcSession session)
        {
            session.TypeName = string.Empty;
            session.SubTypeName = string.Empty;
            session.Laps = 0;
            session.Time = TimeSpan.Zero;
            session.LimitedByTime = false;
            session.FuelNeeded = 0.0;
            session.StopsNeeded = 0;
        }

        private void UpdateWarningVisible(ref GameData data)
        {
            if (!_sessionModule.Race || !Settings.EnablePreRaceWarning || StartFuelCalculatorVisible || _sessionModule.RaceStarted)
            {
                WarningVisible = false;
                return;
            }

            RawDataHelper.TryGetTelemetryData<int>(ref data, out int sessionNum, "SessionNum");

            if (sessionNum < 0 || sessionNum >= MaxSessions)
            {
                WarningVisible = false;
                return;
            }

            FuelCalcSession session = Sessions[sessionNum];
            if (session.FuelNeeded <= 0 || SetupFuelLevel <= 0)
            {
                WarningVisible = false;
                return;
            }

            double errorMarginLiters = 0.05;
            double errorMargin = errorMarginLiters * ConvertFromLiters;
            if (SetupFuelLevel + errorMargin >= session.FuelNeeded)
            {
                WarningVisible = false;
                return;
            }

            WarningVisible = true;
        }
    }
}
