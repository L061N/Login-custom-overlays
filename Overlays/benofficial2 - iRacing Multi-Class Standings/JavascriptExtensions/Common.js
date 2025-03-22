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

function isGameRunning()
{
    return isnull($prop('benofficial2.iRacingRunning'), false);
}

function isReplayPlaying()
{
    return isnull($prop('benofficial2.Session.ReplayPlaying'), false);
}

function isDriving()
{
    return isGameRunning() && !isReplayPlaying();
}

function isInPitLane()
{
    return $prop('DataCorePlugin.GameData.IsInPitLane');
}

function isRace()
{
    return isnull($prop('benofficial2.Session.Race'), false);
}

function isRaceStarted()
{
    return isnull($prop('benofficial2.Session.RaceStarted'), false);
}

function isRaceFinished()
{
    return isnull($prop('benofficial2.Session.RaceFinished'), false);
}

function isRaceInProgress()
{
    return isRaceStarted() && !isRaceFinished();
}

function isQual()
{
    return isnull($prop('benofficial2.Session.Qual'), false);
}

function isPractice()
{
    return isnull($prop('benofficial2.Session.Practice'), false);
}

function isOffline()
{
    return isnull($prop('benofficial2.Session.Offline'), false);
}

function isInvalidTime(time)
{
    return time == null || time == '00:00:00' || time == '00:00.000' || time == '00:00.0000000';
}

function formatSecondsToTimecode(totalSeconds) 
{
    if (totalSeconds < 0 || totalSeconds > 172800)
    {
        return '';
    }

    totalSeconds = Math.floor(totalSeconds); // Ensure it's an integer
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;
    
    if (hours > 0) 
    {
        return `${hours}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    } 
    else 
    {
        return `${minutes}:${seconds.toString().padStart(2, '0')}`;
    }
}

// Parses a timecode string in the format "00:00.0000000" and converts it to "0:00.000"
function formatTimecode(timecode) 
{
    // Extract minutes, seconds, and milliseconds using regex
    const match = String(timecode).match(/(\d{2}):(\d{2})\.(\d{3})/);
    
    if (!match) 
    {
        return "";
    }
    
    let minutes = parseInt(match[1], 10);
    let seconds = parseInt(match[2], 10);
    let milliseconds = match[3];
    
    // Convert to the desired format "0:00.000"
    return `${minutes}:${seconds.toString().padStart(2, '0')}.${milliseconds}`;
}

