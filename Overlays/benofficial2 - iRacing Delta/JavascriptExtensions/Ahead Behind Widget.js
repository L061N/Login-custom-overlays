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

function showAheadBehindWidget()
{
    if (isRace())
    {
        var lap = $prop('DataCorePlugin.GameData.CurrentLap');
        return lap > 1;
    }
    return false;
}

function getDriverAheadDeltaColor()
{
    var ourTime = $prop('DataCorePlugin.GameData.LastLapTime');
    var theirTime = $prop('IRacingExtraProperties.iRacing_NonRelativeDriverAheadInClass_00_LastLapTime');

    return computeDeltaTimeColor(ourTime, theirTime);
}

function getDriverBehindDeltaColor()
{
    var ourTime = $prop('DataCorePlugin.GameData.LastLapTime');
    var theirTime = $prop('IRacingExtraProperties.iRacing_NonRelativeDriverBehindInClass_00_LastLapTime');

    return computeDeltaTimeColor(ourTime, theirTime);
}