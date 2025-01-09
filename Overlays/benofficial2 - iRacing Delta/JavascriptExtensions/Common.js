/*
    benofficial2's Official Overlays
    Copyright (C) 2023-2025 benofficial2

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

function isGameIRacing()
{
    var running = $prop('DataCorePlugin.GameRunning') == 1;
    var iRacing = $prop('DataCorePlugin.CurrentGame') == 'IRacing'
    return running && iRacing;
}

function isReplayPlaying()
{
    if (isGameIRacing())
    {
        // There's a short moment when loading into a session when isReplayPlaying is false but position is -1
        const isReplayPlaying = $prop('DataCorePlugin.GameRawData.Telemetry.IsReplayPlaying');
        const position = $prop('DataCorePlugin.GameRawData.Telemetry.PlayerCarPosition');
        const trackSurface = $prop('DataCorePlugin.GameRawData.Telemetry.PlayerTrackSurface');
        return isReplayPlaying || position < 0 || trackSurface < 0;
    }
    return false;
}

function isInPitLane()
{
    return $prop('DataCorePlugin.GameData.IsInPitLane');
}

function isApproachingPits()
{
    const surface = $prop('GameRawData.Telemetry.PlayerTrackSurface');
    return surface == 2; // irsdk_AproachingPits = 2
}

function isGameRunning()
{
    return $prop('DataCorePlugin.GameRunning');
}

function isDriving()
{
    return isGameRunning() && !isReplayPlaying();
}

function isRace()
{
    var sessionTypeName = $prop('DataCorePlugin.GameData.SessionTypeName');
    return String(sessionTypeName).indexOf('Race') != -1;   
}

function isQual()
{
    var sessionTypeName = $prop('DataCorePlugin.GameData.SessionTypeName');
    return String(sessionTypeName).indexOf('Qual') != -1;
}

function isPractice()
{
    var sessionTypeName = $prop('DataCorePlugin.GameData.SessionTypeName');
    return (String(sessionTypeName).indexOf('Practice') != -1) ||
           (String(sessionTypeName).indexOf('Warmup') != -1) ||
           (String(sessionTypeName).indexOf('Testing') != -1);
}

function isOffline()
{
    var sessionTypeName = $prop('DataCorePlugin.GameData.SessionTypeName');
    var isPractice = String(sessionTypeName).indexOf('Offline') != -1;
}

function isInvalidTime(time)
{
    return time == null || time == '00:00:00' || time == '00:00.000';
}

function getSessionBestTime()
{
    // The live delta seems to always be against the best clean lap.
    var best = $prop('IRacingExtraProperties.iRacing_Player_SessionBestCleanLapTime');
    if (isInvalidTime(best))
    {
        // But when there's no clean lap, it is against this value instead??
        best = $prop('PersistantTrackerPlugin.SessionBest');
    }
    return best;    
}

function getReferenceLapTime()
{
    var best = null;
    if (isQual())
    {
        best = $prop('PersistantTrackerPlugin.AllTimeBest');
    }
    else if (isRace())
    {
        //best = $prop('DataCorePlugin.GameData.BestLapTime');
        best = $prop('IRacingExtraProperties.iRacing_Player_SessionBestCleanLapTime');
        //best = $prop('PersistantTrackerPlugin.SessionBest');
    }
    else if (isPractice())
    {
        best = $prop('IRacingExtraProperties.iRacing_Player_SessionBestCleanLapTime');
    }

    if (isInvalidTime(best))
    {
        // Fallback to last lap time.
        // Happens on the 2nd lap of the race.
        best = $prop('DataCorePlugin.GameData.LastLapTime');
    }

    return best;
}

function isReferenceLapTimeOk()
{
    if (isQual())
    {
        return $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToBestLap_OK');
    }
    else if (isRace())
    {
        return $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToSessionBestLap_OK');
    }
    else if (isPractice())
    {
        return $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToSessionBestLap_OK');   
    }
    else
    {
        return $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToSessionLastlLap_OK');
    }
}

function getReferenceLapTimeDelta()
{
    if (isQual())
    {
        return $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToBestLap_DD');
    }
    else if (isRace())
    {
        return $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToSessionBestLap_DD');
    }
    else if (isPractice())
    {
        return $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToSessionBestLap_DD');   
    }
    else
    {
        return $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToSessionLastlLap_DD');
    }
}

function getBestLiveDeltaTimeSecs()
{
    var delta = null;
    if (isQual())
    {
        delta = $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToBestLap');
        //delta = $prop('PersistantTrackerPlugin.AllTimeBestLiveDeltaSeconds');
    }
    else if (isRace())
    {   
        var lap = $prop('DataCorePlugin.GameData.CurrentLap');
        if (lap <= 2)
        {
            delta = $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToSessionLastlLap');
            //delta = $prop('PersistantTrackerPlugin.SessionBestLiveDeltaSeconds');
        }
        else
        {
            delta = $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToSessionBestLap');
            //delta = $prop('PersistantTrackerPlugin.SessionBestLiveDeltaSeconds');
        }
    }
    else if (isPractice())
    {
        delta = $prop('DataCorePlugin.GameRawData.Telemetry.LapDeltaToSessionBestLap');
        //delta = $prop('PersistantTrackerPlugin.SessionBestLiveDeltaSeconds');
    }

    return delta;
}

function computeDeltaTime(ourTime, theirTime)
{
    if (isInvalidTime(ourTime) || isInvalidTime(theirTime))
    {
        return '';
    }

    var our = new Date(ourTime)
    var their = new Date(theirTime);
    var delta = new Date(Math.abs(their - our));

    var sign = '-';
    if (our > their)
    {
        sign = '+';
    }

    if (delta.getSeconds() > 9 || delta.getMinutes() > 0)
    {
        return sign + "9.99";
    }

    var sec = String(delta.getSeconds())
    var mil = String(Math.floor(delta.getMilliseconds() / 10)).padStart(2, '0');
    return sign + sec + '.' + mil;
}

//  0: White (255, 255, 255)
//  1: Green ( 82, 224,  82)
// -1: Red   (255, 127, 102)
function getDeltaTimeColor(deltaTimeWithSign)
{
    var sign = String(deltaTimeWithSign).substring(0, 1);
    if (sign == '+')
    {
        return -1
    }
    else if (sign == '-')
    {
        return 1
    }
    return 0;
}

//  0: White (255, 255, 255)
//  1: Green ( 82, 224,  82)
// -1: Red   (255, 127, 102)
function computeDeltaTimeColor(ourTime, theirTime)
{
    if (isInvalidTime(ourTime) || isInvalidTime(theirTime))
    {
        return 0;
    }

    var our = new Date(ourTime)
    var their = new Date(theirTime);

    if (our > their)
    {
        return -1;
    }
    return 1;
}

function getClassLeaderboardProp(position)
{
    return 'IRacingExtraProperties.iRacing_ClassLeaderboard_Driver_' + String(position - 1).padStart(2, '0');
}

function formatLapTime(time, decimalCount)
{
    decimalCount = Math.max(1, Math.min(3, decimalCount));
    if (isInvalidTime(time))
    {
        var value = '--:--.';
        while (decimalCount-- > 0)
        {
            value += '-'
        }
        return value;
    }

    var value = String(time).substring(4, 8) + '.';
    var decimals = String(time).substring(9, 9 + decimalCount);
    var missingDecimals = decimalCount - decimals.length;
    while (missingDecimals-- > 0)
    {
        decimals += '0';
    }
    return value + decimals;
}

/// Check if a flag is out.
/// @param color Color of the flag as a String.
/// @return True if the flag is out.
/// Supported colors: Black, Blue, Checkered, Green, Orange, White, Yellow.
function isFlagOut(color)
{
    var flagOut = $prop('DataCorePlugin.GameData.Flag_' + color);
    return flagOut != 0;
}

function isOutLap()
{
    // Initialize static variables.
    if(root["isOutLap.lastLap"] == null) { root["isOutLap.lastLap"] = 0; }
    if(root["isOutLap.wasInPit"] == null) { root["isOutLap.wasInPit"] = false; }

    // Clear when changing lap.
    var lap = $prop('DataCorePlugin.GameData.CurrentLap');
    if (root["isOutLap.lastLap"] != lap)
    {
        root["isOutLap.lastLap"] = lap;
        root["isOutLap.wasInPit"] = false;
    }

    // Remember if we were ever in the pit during this lap.
    var isInPit = $prop('DataCorePlugin.GameData.IsInPit')
    if (isInPit)
    {
        root["isOutLap.wasInPit"] = true;
    }
    
    return root["isOutLap.wasInPit"];
}

function isRejoinHelperShown()
{
    // irsdk_OffTrack = 0
    const surface = $prop('GameRawData.Telemetry.PlayerTrackSurface');
    const speed = $prop('SpeedKmh');
    const isSlow = speed <= g_RejoinHelperMinSpeed;
    
    return (surface == 0 || isSlow);
}