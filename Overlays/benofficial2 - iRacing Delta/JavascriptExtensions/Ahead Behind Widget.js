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

function getDriverPositionInClass(relativeIdx)
{
    // SimHub does not return a live position
    //const pos = getopponentleaderboardposition_aheadbehind_playerclassonly(relativeIdx);
    //return driverclassposition(pos);

    const playerPos = isnull($prop('benofficial2.Player.LivePositionInClass'), 0);
    if (playerPos <= 0) return 0;
    return playerPos + relativeIdx;
}

function getDriverLastLapDeltaToPlayer(relativeIdx)
{
    const playerTime = $prop('DataCorePlugin.GameData.LastLapTime');

    const pos = getopponentleaderboardposition_aheadbehind_playerclassonly(relativeIdx);
    const driverTime = driverlastlap(pos);

    return computeDeltaTime(playerTime, driverTime);
}