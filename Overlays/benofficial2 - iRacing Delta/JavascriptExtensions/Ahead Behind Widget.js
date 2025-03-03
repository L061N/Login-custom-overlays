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

function getHeadToHeadProp(index, prop)
{
    if (index > 0)
    {
        return $prop('benofficial2.Delta.Behind.' + prop);
    }
    return $prop('benofficial2.Delta.Ahead.' + prop);
}

function getDriverLastLapDeltaToPlayer(relativeIdx)
{
    const playerTime = $prop('DataCorePlugin.GameData.LastLapTime');
    const driverTime = getHeadToHeadProp(relativeIdx, 'LastLapTime');
    return computeDeltaTime(playerTime, driverTime);
}