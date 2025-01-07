/*
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

// Enable the Spotter that shows orange bars when the spotter is calling a car left/right 
const g_EnableSpotter = true;

// Distance from another car at which the Spotter's orange bars will show. Also used for showing Blind Spot Monitor.
const g_SpotterThreshold = 5.5;

// Height of the Spotter's orange bars (between 1-129)
const g_SpotterHeight = 129;

// Width of the Spotter's orange bars
const g_SpotterWidth = 12;

// Thickness of the Spotter's border (between 0-3)
const g_SpotterBorder = 3;

// Minimum height of the spotter's orange bar so it's easier to notice (0 to disable)
const g_SpotterMinHeight = 15;

// Enable the Rejoin Helper that shows the gap with the next incomming car
const g_EnableRejoinHelper = true;

// Speed in Km/h below which the Rejoin Helper will trigger
const g_RejoinHelperMinSpeed = 35;

// Minimum gap in seconds at which the Rejoin Helper will show the 'Clear'
const g_RejoinHelperClear = 3.5;

// Minimum gap in seconds at which the Rejoin Helper will show the 'Care'
const g_RejoinHelperCare = 1.5;

// Enable the Blind Spot Monitor that shows a yellow warning sign when cars overlap (iRacing does not support left/right)
const g_EnableBlindSpotMonitor = false;