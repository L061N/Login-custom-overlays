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

const g_literToKg = 0.74380169246343626270662456402943;
const g_DebugLitersPerLap = -1;
const g_DebugBestLapTime = ""

function getSessionDetails(sessionIdx)
{
    const info = getFuelInfo(sessionIdx);

    if (info.sessionType == "Invalid")
    {
        return '';
    }

    if (info.limitedBy == "laps")
    {
        let laps = " laps:";
        if (info.sessionLaps == 1)
        {
            laps = " lap:";
        }
        return info.sessionType + ' ' + info.sessionLaps + laps;
    }
    
    if (info.sessionTime < 0)
    {
        return info.sessionType + ' ∞:';
    }

    return info.sessionType + ' ' + info.sessionTime / 60 + ' min:'
}

function getFuelNeeded(sessionIdx)
{
    const info = getFuelInfo(sessionIdx);

    if (info.sessionType == "Invalid")
    {
        return '';
    }

    let stops = '';
    if (info.stops == 1)
    {
        stops = '(' + info.stops + ' stop)';
    }
    else if (info.stops > 1)
    {
        stops = '(' + info.stops + ' stops)';
    }

    const setupFuelInfo = getSetupFuelInfo();
    return (info.fuelNeeded * setupFuelInfo.convert).toFixed(1) + ' ' + setupFuelInfo.unit + ' ' + stops;
}

function getFuelInfo(sessionIdx)
{
    let info = {
            fuelPerLap: 0,              // Avg fuel consumed per lap (must drive at least a lap)
            fuelNeeded: 0,              // Fuel needed at the start of the session
            fuelUnits: "L",             // Always "L"
            limitedBy: "time",          // "laps" or "time"
            stops: 0,                   // Number of stops required in session
            sessionType: "Invalid",     // Session type string
            sessionLaps: 0,             // -1 is unlimited
            sessionTime: 0              // Session time limit in seconds
        };

    if (NewRawData() == null)
    {
        return info;
    }

    const numSession = NewRawData().AllSessionData["SessionInfo"]["Sessions"].length;
    if (numSession < sessionIdx + 1)
    {
        return info;
    }

    const session = NewRawData().AllSessionData["SessionInfo"]["Sessions"][sessionIdx];
    info.sessionType = String(session["SessionType"]);
    info.fuelPerLap = $prop('variable.fuelLitersPerLap');

    info.sessionLaps = Number(session["SessionLaps"]);
    if (String(info.sessionLaps).indexOf('unlimited') != -1)
    {
        info.sessionLaps = -1;
    }

    const time = session["SessionTime"];
    if (time == "unlimited" || String(time).length <= 4)
    {
        info.sessionTime = -1;
    }
    else
    {
        info.sessionTime = Number(String(time).slice(0, -4));
    }

    if (info.sessionLaps < 0 && info.sessionTime < 0)
    {
        return info;
    }

    const bestLapTimeSecs = timespantoseconds($prop('benofficial2.FuelCalc.BestLapTime'));

    let minTimeForLaps = -1;
    if (info.sessionLaps > 0 && bestLapTimeSecs > 0)
    {
        minTimeForLaps = bestLapTimeSecs * info.sessionLaps;
    }

    if (info.sessionLaps > 0 && (info.sessionTime < 0 || minTimeForLaps < info.sessionTime))
    {
        info.limitedBy = "laps";
        info.fuelNeeded = info.fuelPerLap * info.sessionLaps;
    }
    else
    {
        info.limitedBy = "time"
        if (bestLapTimeSecs <= 0)
        {
            // Can't compute fuel without a best lap time.
            return info;
        }
        
        let lapsEstimate = Math.ceil(info.sessionTime / bestLapTimeSecs);

        // Add an extra lap if we would cross the line with more than X% of a lap remaining
        // It is unknown what is the exact rule used by iRacing. Could be 60% of avg from last 3 race laps.
        const remainingTime = info.sessionTime % bestLapTimeSecs;
        if (remainingTime > g_WhiteFlagRuleLapPct * bestLapTimeSecs)
        {
            lapsEstimate++;
        }

        info.fuelNeeded = info.fuelPerLap * lapsEstimate;
    }

    const trackInfo = $prop('variable.trackInfo');
    let outLaps = 0;
    if (String(info.sessionType).indexOf('Qual') != -1)
    {
        outLaps = 1 - trackInfo.qualStartTrackPct;
    }
    else if (String(info.sessionType).indexOf('Race') != -1)
    {
        const standingStart = NewRawData().AllSessionData["WeekendInfo"]["WeekendOptions"]["StandingStart"];
        if (standingStart == 0)
        {    
            if (isOvalCategory())
            {
                const shortParadeLap = $prop('GameRawData.SessionData.WeekendInfo.WeekendOptions.ShortParadeLap');
                const trackType = $prop('GameRawData.SessionData.WeekendInfo.TrackType');
                if (trackInfo.raceStartTrackPct > 0)
                {
                    outLaps = 1 - trackInfo.raceStartTrackPct;
                }
                else if (shortParadeLap == 1)
                {
                    outLaps = g_ShortParadeLapPct;
                }
                else if(String(trackType) == 'super speedway')            
                {
                    outLaps = 1;
                }
                else
                {
                    outLaps = 2;
                }
            }
            else
            {
                outLaps = 1 - trackInfo.raceStartTrackPct;
            }
        }
    }

    info.fuelNeeded += info.fuelPerLap * outLaps;

    if (info.fuelNeeded > 0 && info.fuelPerLap > 0)
    {
        const fuelReserve = isnull($prop('benofficial2.FuelCalc.FuelReserve'), 0.5);
        info.fuelNeeded += fuelReserve;

        if (String(info.sessionType).indexOf('Race') != -1)
        {
            const extraLaps = isnull($prop('benofficial2.FuelCalc.ExtraLaps'), 0.0);
            info.fuelNeeded += info.fuelPerLap * extraLaps;
        }
    }

    const maxFuelTank = $prop('MaxFuel');
    const maxFuelPct = $prop('DataCorePlugin.GameRawData.SessionData.DriverInfo.DriverCarMaxFuelPct');

    const maxFuel = maxFuelTank * maxFuelPct;

    info.stops = Math.floor(info.fuelNeeded / maxFuel);
    if (info.stops >= 1)
    {
        info.fuelNeeded = maxFuel;
    }

    return info;
}

function getFuelLitersPerLap()
{
    const trackId = $prop('TrackId');
    const carId = $prop('CarId');
    const combo = String(trackId) + String(carId);
    const comboValid = trackId != null && carId != null;

    if (comboValid && combo != root['lastCombo'])
    {
        root['fuelLitersPerLap'] = null;
        root['lastCombo'] = combo;
    }

    const fuelLitersPerLap = g_DebugLitersPerLap >= 0 ? g_DebugLitersPerLap : Number($prop('DataCorePlugin.Computed.Fuel_LitersPerLap'));
    const extraConsumptionPct = isnull($prop('benofficial2.FuelCalc.ExtraConsumptionPct'), 0) / 100;

    if (fuelLitersPerLap > 0)
    {
        // Remember the last valid consumption for this combo
        // Because sometimes when advancing session we lose the computed value
        root['fuelLitersPerLap'] = fuelLitersPerLap * (1 + extraConsumptionPct);
    }

    if(root['fuelLitersPerLap'] > 0)
    {
        return root['fuelLitersPerLap'];
    }

    return 0;
}

function getSetupFuelInfo()
{
    let info = {
        fuel: 0,    // Amount of fuel specified in setup, 0 if not found
        unit: "L",  // Display units, "L" or "Kg"
        convert: 1  // Multiplier to convert from liters, 1 if already in liters
    };

    // Must handle all the different ways fuel is defined in car setups
    // Because the telemetry fuel value is 0 when not in-car
    let fuelLevel = null;
    fuelLevelProperties = ['GameRawData.SessionData.CarSetup.Suspension.Rear.FuelLevel',
        'GameRawData.SessionData.CarSetup.Chassis.Rear.FuelLevel',
        'GameRawData.SessionData.CarSetup.Chassis.Front.FuelLevel',
        'GameRawData.SessionData.CarSetup.BrakesDriveUnit.Fuel.FuelLevel',
        'GameRawData.SessionData.CarSetup.Chassis.BrakesInCarMisc.FuelLevel',
        'GameRawData.SessionData.CarSetup.InCarSystems.Fuel.FuelLevel',
        'GameRawData.SessionData.CarSetup.Systems.Fuel.FuelLevel',
        'GameRawData.SessionData.CarSetup.TiresFuel.Fuel.FuelLevel',
        'GameRawData.SessionData.CarSetup.VehicleSystems.Fuel.FuelLevel'
    ];

    for (let i = 0; i < fuelLevelProperties.length; i++)
    {
        fuelLevel = $prop(fuelLevelProperties[i]);
        if (fuelLevel != null)
        {
            break;
        }
    }
    
    if (fuelLevel == null)
    {
        return info;
    }

    if (fuelLevel.indexOf("L") != -1)
    {
        info.fuel = Number(String(fuelLevel).slice(0, -2));
        info.unit = "L";
        info.convert = 1;
    }
    else if (fuelLevel.indexOf("Kg") != -1)
    {
        info.fuel = Number(String(fuelLevel).slice(0, -3));
        info.unit = "Kg";
        
        // The value iRacing give is imprecise
        //info.convert = $prop('GameRawData.SessionData.DriverInfo.DriverCarFuelKgPerLtr');
        info.convert = g_literToKg;
    }

    return info;
}

function getTrackInfo()
{
    let info = {
        qualStartTrackPct: 0.00, // Default starting from pit, must do full out lap.
        raceStartTrackPct: 0.00  // Default starting from pit or finish line, must do full parade lap.
    };

    if (root['cache'])
    {
        const trackId = String($prop('TrackId'));
        const trackInfo = root['cache'][trackId];
        if (trackInfo != null) 
        {
            if (trackInfo['qualStartTrackPct'] != null)
            {
                info.qualStartTrackPct = trackInfo['qualStartTrackPct']
            }

            if (trackInfo['raceStartTrackPct'] != null)
            {
                info.raceStartTrackPct = trackInfo['raceStartTrackPct']
            }
        }
        return info;
    }

    const url = 'https://raw.githubusercontent.com/fixfactory/bo2-official-overlays/main/Data/TrackInfo.json';
    const jsonStr = downloadstringasync(500, url);
    if (jsonStr) 
    {
        root['cache'] = JSON.parse(jsonStr);;
    }

    return info;
}

function showPreRaceFuelWarning()
{
    const garageVisible = $prop('GameRawData.Telemetry.IsGarageVisible');
    const sessionState = $prop('DataCorePlugin.GameRawData.Telemetry.SessionState');
    const raceStarted = sessionState >= 4;
    const enablePreRaceFuelWarning = isnull($prop('benofficial2.FuelCalc.EnablePreRaceWarning'), true);

    if (!isGameIRacing() || !isGameRunning() || !isRace() || !enablePreRaceFuelWarning || garageVisible || raceStarted)
    {
        return false;
    }

    const setupFuelInfo = getSetupFuelInfo();
    const sessionNum = NewRawData().Telemetry["SessionNum"];
    const fuelInfo = getFuelInfo(sessionNum);

    if (fuelInfo.fuelNeeded <= 0 || setupFuelInfo.fuel <= 0)
    {
        return false;
    }

    const errorMargin = 0.05;
    if ((setupFuelInfo.fuel / setupFuelInfo.convert + errorMargin) >= fuelInfo.fuelNeeded)
    {
        return false;
    }

    return true;
}