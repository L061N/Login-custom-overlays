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
    return $prop('DataCorePlugin.CurrentGame') == 'IRacing';
}

function isGameRunning()
{
    return $prop('DataCorePlugin.GameRunning');
}

function isInvalidTime(time)
{
    return time == null || time == '00:00:00' || time == '00:00.000';
}

function convertToTimestamp(seconds) 
{
    // Ensure the input is a number
    if (typeof seconds !== 'number' || isNaN(seconds)) 
    {
        throw new Error('Input must be a valid number');
    }

    // Get whole minutes
    const minutes = Math.floor(seconds / 60);

    // Get remaining seconds (including milliseconds)
    const remainingSeconds = seconds % 60;

    // Extract whole seconds and milliseconds
    const wholeSeconds = Math.floor(remainingSeconds);
    const milliseconds = Math.floor((remainingSeconds - wholeSeconds) * 1000);

    // Format minutes, seconds, and milliseconds with leading zeros
    const formattedMinutes = String(minutes).padStart(2, '0');
    const formattedSeconds = String(wholeSeconds).padStart(2, '0');
    const formattedMilliseconds = String(milliseconds).padStart(3, '0');

    // Combine into the desired format
    return `${formattedMinutes}:${formattedSeconds}.${formattedMilliseconds}`;
}
