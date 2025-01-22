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

// Interval in ms at which we refresh the array. Set 0 to use SimHub's min interval.
const g_RefreshIntervalMs = 0;

// Shared driver info about all cars.
function getDriverInfo()
{
    // De-initialize when game not running, and before the race starts
    if (!isGameRunning() || (isRace() && getSessionState(0) < 4))
    {  
        root["sessionTime"] = null;
        root["driverInfo"] = null;
        root["refreshTime"] = null;
        return null;
    }

    // De-initialize when changing/restarting session
    const sessionTime = NewRawData().Telemetry["SessionTime"];
    if (root["sessionTime"] == null || sessionTime < root["sessionTime"])
    {
        root["sessionTime"] = sessionTime;    
        root["driverInfo"] = null;
        root["refreshTime"] = null;
        return null;
    }

    // Initialize
    if (root["driverInfo"] == null)
    {
        root["driverInfo"] = [];
        root["refreshTime"] = Date.now();
        //log("driverInfo initialized");
    }

    // Refresh the data at a given interval
    if (root["refreshTime"] <= Date.now())
    {
        root["refreshTime"] = Date.now() + g_RefreshIntervalMs;
           
        for (let i = 0; i < 64; i++) 
        {
            const number = getLeaderboardProp('CarNumber', 0, i);
            if (number == null || number == '')
            {
                continue;
            }

            let info = root["driverInfo"][number];
            if (info == null)
            {
                info = {
                    inPitSince: null,
                    currentLap: 0,
                    outLap: 0,
                };
            }
                
            info.currentLap = getLeaderboardProp('CurrentLap', 0, i);    
            const inPit = getLeaderboardProp('IsInPit', 0, i);
            
            if (inPit)
            {
                if (info.inPitSince == null)
                {
                    info.inPitSince = Date.now()
                }
            }
            else
            {
                // If the driver is in pit for a short time, consider this a glitch in telemetry.
                if (info.inPitSince != null
                    && info.inPitSince + 1500 < Date.now())
                {
                    // Remember the lap when they exited the pit.
                    // We use 'their' lap, not the leader's lap.
                    info.outLap = info.currentLap;
                    
                    // Edge case when the pit exit is before the finish line.
                    // The currentLap will increment, so consider the next lap an out lap too.
                    let trackPct = getLeaderboardProp('LapDistPct', 0, i);
                    if (trackPct > 0.5)
                    {
                        info.outLap++;
                    }
                }
                
                info.inPitSince = null;
            }
            
            root["driverInfo"][number] = info;
        }
    }

    root["sessionTime"] = sessionTime;
    return root["driverInfo"];
}