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

    const displayInfo = getFuelDisplayInfo();
    return (info.fuelNeeded * displayInfo.convert).toFixed(1) + ' ' + displayInfo.unit + ' ' + stops;
}

function getBestLapTime()
{
    if (!isGameIRacing() || !isGameRunning() || NewRawData() == null)
    {
        return "00:00.000";
    }

    // Return the player's best lap time in the current session.
    if (g_UsePlayersFastestTime)
    {
        let bestLapTime = $prop('BestLapTime');
        if (!isInvalidTime(bestLapTime))
        {
            return String(bestLapTime).slice(3, -4);
        }
    }

    // Try to find the fastest time of any session.
    const numSession = NewRawData().AllSessionData["SessionInfo"]["Sessions"].length;
    let fastestTime = 0;
    for (let sessionIdx = 0; sessionIdx < numSession; sessionIdx++)
    {
        const session = NewRawData().AllSessionData["SessionInfo"]["Sessions"][sessionIdx];
        const lapNum = session["ResultsFastestLap"].length;
        if (lapNum >= 0)
        {
            const timeSecs = Number(session["ResultsFastestLap"][0]["FastestTime"]);
            if (timeSecs > 0 && (timeSecs < fastestTime || fastestTime == 0))
            {
                fastestTime = timeSecs;
            }
        }
    }

    return convertToTimestamp(fastestTime);
}

function getBestLapTimeInClass()
{
    if (!isGameIRacing() || !isGameRunning() || NewRawData() == null)
    {
        return "00:00.000";
    }

    // Return the player's best lap time in the current session
    if (g_UsePlayersFastestTime)
    {
        let bestLapTime = $prop('BestLapTime');
        if (!isInvalidTime(bestLapTime))
        {
            return String(bestLapTime).slice(3, -4);
        }
    }

    // Get the player's class
    const data = NewRawData().AllSessionData;
    const playerCarIdx = data["DriverInfo"]["DriverCarIdx"];
    const playerClassId = data["DriverInfo"]["Drivers"][playerCarIdx]["CarClassID"];

    // Try to find the fastest time of any session
    const numSession = data["SessionInfo"]["Sessions"].length;
    let fastestTime = 0;
    for (let sessionIdx = 0; sessionIdx < numSession; sessionIdx++)
    {
        const session = data["SessionInfo"]["Sessions"][sessionIdx];
        const posCount = session["ResultsPositions"].length;
        for (let posIdx = 0; posIdx < posCount; posIdx++)
        {
            // Must be in same class as player
            const carIdx = session["ResultsPositions"][posIdx]["CarIdx"];
            if (playerClassId == data["DriverInfo"]["Drivers"][carIdx]["CarClassID"])
            {
                const timeSecs = Number(session["ResultsPositions"][posIdx]["FastestTime"]);
                if (timeSecs > 0 && (timeSecs < fastestTime || fastestTime == 0))
                {
                    fastestTime = timeSecs;
                }
            }
        }
    }

    return convertToTimestamp(fastestTime);
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
            sessionTime: 0,             // Session time limit in seconds
            sessionBestLapTime: 0
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

    info.sessionBestLapTime = $prop('variable.bestLapTime');
    const bestLapTime = new Date("00:" + info.sessionBestLapTime);
    const bestLapTimeSecs = bestLapTime.getMinutes() * 60 + bestLapTime.getSeconds();

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
        if (remainingTime > gWhiteFlagRuleLapPct * bestLapTimeSecs)
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
        info.fuelNeeded += g_FuelReserve;
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
    if (g_DebugLitersPerLap >= 0)
    {
        return g_DebugLitersPerLap;
    }

    const trackId = $prop('TrackId');
    const carId = $prop('CarId');
    const combo = String(trackId) + String(carId);
    const comboValid = trackId != null && carId != null;

    if (comboValid && combo != root['lastCombo'])
    {
        root['fuelLitersPerLap'] = null;
        root['lastCombo'] = combo;
    }

    const fuelLitersPerLap = Number($prop('DataCorePlugin.Computed.Fuel_LitersPerLap'));

    if (fuelLitersPerLap > 0)
    {
        // Remember the last valid consumption for this combo
        // Because sometimes when advancing session we lose the computed value
        root['fuelLitersPerLap'] = fuelLitersPerLap;
        return fuelLitersPerLap;
    }

    if(root['fuelLitersPerLap'] > 0)
    {
        return root['fuelLitersPerLap'];
    }

    return 0;
}

function getFuelDisplayInfo()
{
    let info = {
        unit: "L",
        convert: 1
    };

    const fuelLevel = String($prop('GameRawData.SessionData.CarSetup.Chassis.Rear.FuelLevel'));
    if (fuelLevel.indexOf("Kg") != -1)
    {
        info.unit = "Kg";
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