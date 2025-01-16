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

// How much fuel to keep as a reserve in liters to avoid bogging down.
const g_FuelReserve = 0.5;

// Maximum amount of time remaining (in percentage of best lap) for the white flag to be shown.
// It is unknown what is the exact rule used by iRacing. Could be 60% of avg from last 3 race laps.
const gWhiteFlagRuleLapPct = 0.60;

// How long is the short parade lap at supported tracks (as a percentage of a lap).
// This is the default values for tracks we don't have in the database.
const g_ShortParadeLapPct = 0.50;

// Use the current session's fastest time from the player for fuel calculations.
// Default to false, because can cause under fueling when significantly off-pace.
const g_UsePlayersFastestTime = false;

// Enable a fuel warning before the race when under-fueled.
const g_EnablePreRaceFuelWarning = true;