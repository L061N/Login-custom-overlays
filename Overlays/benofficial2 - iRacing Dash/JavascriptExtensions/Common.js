//
// Ben's In-Car Dash - Common.js
// Author: benofficial2
// Date: 2023/09/10
//
// If this code was helpful to you, please consider following me on Twitch
// http://twitch.tv/benofficial2
//
function isGameIRacing()
{
    return $prop('DataCorePlugin.CurrentGame') == 'IRacing';
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

function isGameRunning()
{
    return $prop('DataCorePlugin.GameRunning');
}

function isDriving()
{
    return isGameRunning() && !isReplayPlaying();
}

function isTowing()
{
    return $prop('DataCorePlugin.GameRawData.Telemetry.PlayerCarTowTime') > 0;
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
    return String(sessionTypeName).indexOf('Offline') != -1;
}

// Returns True if the specified flag is out.
// Supported colors: 'Black', 'Blue', 'Checkered', 'Green', 'Orange', 'White', 'Yellow'.
function isFlagOut(color)
{
    var flagOut = $prop('DataCorePlugin.GameData.Flag_' + color);
    return flagOut != 0;
}

function shouldPitThisLap()
{
    var fuelRemainingLaps = $prop('DataCorePlugin.Computed.Fuel_RemainingLaps');
    var trackPercentRemaining = 1 - $prop('DataCorePlugin.GameData.TrackPositionPercent');
    var fuelAlertLaps = $prop('DataCorePlugin.GameData.CarSettings_FuelAlertLaps')
    return (fuelRemainingLaps - trackPercentRemaining) < fuelAlertLaps;
}

// Returns the P2P Cooldown Time (aka ReTime) in seconds based on the current track.
function getP2pTotalCooldown()
{
    const trackId = $prop('DataCorePlugin.GameData.TrackId');
    const trackIds120 = 'twinring fullrc, fuji gp';
    if (trackIds120.indexOf(trackId) != -1) return 120;
    return 100;
}

//
// Returns an object with properties related to the car's Push To Pass status (aka OT).
//
// Notes:
// This function shouldn't be called directly in each property that need it.
// Instead, create a Dashboard Variable with only this code:
//    return generateP2pStatus();
//
// This will ensure that each value is updated once per frame in lock-step.
//
// Example on how to use in a dashboard property:
//    const p2p = $prop('variable.p2pStatus');
//    return p2p.activated;
//
// For help visit: https://forums.iracing.com/discussion/48050/i-made-a-sim-hub-dashboard-that-shows-the-ot-status-ot-left-and-retime-amongst-other-things
//
function generateP2pStatus()
{
    var p2p = {
        activated: false,
        count: 0,
        timeLeft: 0,
        timeUsed: 0,
        cooldown: 0,
    };

    const carId = $prop('DataCorePlugin.GameData.CarId');
    if (carId == 'dallarair18')
    {
        // For the IndyCar, the values are directly exposed in telemetry.
        const gamePushToPassActive = $prop('DataCorePlugin.GameData.PushToPassActive');
        const gamePushToPassCount = $prop('DataCorePlugin.GameRawData.Telemetry.PlayerP2P_Count');
        p2p.activated = gamePushToPassActive;
        p2p.count = gamePushToPassCount;
    }
    else if (carId == 'superformulasf23 toyota' || carId == 'superformulasf23 honda')
    {
        // For Super Formula, the telemetry ony exposes 'activated' and 'timeLeft'.
        // We have to generate the other values.
        const gamePushToPassActive = $prop('DataCorePlugin.GameData.PushToPassActive');
        const gamePushToPassCount = $prop('DataCorePlugin.GameRawData.Telemetry.PlayerP2P_Count');
        p2p.activated = gamePushToPassActive;
        p2p.timeLeft = gamePushToPassCount * 1000;
            
        // Initialize persistent variables.
        if (root["activated"] == null) { root["activated"] = false; }
        if (root["timeUsed"] == null) { root["timeUsed"] = 0; }
                            
        // Check if Push-to-Pass was toggled
        if (gamePushToPassActive != root["activated"])
        {
            root["activated"] = gamePushToPassActive;

            if (gamePushToPassActive)
            {
                root["activatedTime"] = Date.now();
                root["deactivatedTime"] = null;
            }
            else
            {
                const activatedDuration = Date.now() - root["activatedTime"];
                root["timeUsed"] = root["timeUsed"] + activatedDuration;
                root["activatedTime"] = null;
                root["deactivatedTime"] = Date.now();
            }
        }

        // Reset state when out of car, resetting or towing.
        // 'Enter' state happens after the green flag or after leaving pit box.
        const isEnterState = $prop('DataCorePlugin.GameRawData.Telemetry.EnterExitReset') == 2;
        if (!isEnterState)
        {
            root["timeUsed"] = 0;
            root["deactivatedTime"] = 0;
        }

        // Update 'timeUsed' and 'cooldown' timers
        if (gamePushToPassActive)
        {
            const activatedDuration = Date.now() - root["activatedTime"];
            p2p.timeUsed = root["timeUsed"] + activatedDuration;
        }
        else
        {
            p2p.timeUsed = root["timeUsed"];
            
            // 'cooldown' is only used in race
            if (isRace() && root["deactivatedTime"] != null)
            {
                // Total cooldown time in milliseconds. This is hardcoded per track.
                const p2pTotalCooldown = (getP2pTotalCooldown()) * 1000;

                const deactivatedDuration = Date.now() - root["deactivatedTime"];
                if (deactivatedDuration < p2pTotalCooldown)
                {
                    p2p.cooldown = p2pTotalCooldown - deactivatedDuration;
                }
            }
        }
    }

    return p2p;
}
