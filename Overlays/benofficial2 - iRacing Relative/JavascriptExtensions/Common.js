function isGameIRacing()
{
    return $prop('DataCorePlugin.CurrentGame') == 'IRacing';
}

function isGameRunning()
{
    return $prop('DataCorePlugin.GameRunning');
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

function getIndexedProp(name, index)
{
	return $prop(name + '_' + format(index, '00'));
}

// Returns the value of a leaderboard property. Pass classIndex=0 for the global leaderboard.
// Ex:
//   getLeaderboardProp('IsInPit', 1, 0)
//   returns value of IRacingExtraProperties.iRacing_Class_01_Leaderboard_Driver_00_IsInPit
//
//   getLeaderboardProp('IsInPit', 0, 0)
//   returns value of IRacingExtraProperties.iRacing_Leaderboard_Driver_00_IsInPit
function getLeaderboardProp(name, classIndex, driverIndex)
{
    if (classIndex > 0 )
    {
        return $prop('IRacingExtraProperties.iRacing_Class_' 
        + format(classIndex, '00') 
        + '_Leaderboard_Driver_' 
        + format(driverIndex, '00')
        + '_'
        + name);    
    }
    else
    {
        return $prop('IRacingExtraProperties.iRacing_Leaderboard_Driver_' 
        + format(driverIndex, '00')
        + '_'
        + name);    
    }
}

// Get the SessionState with a confirmation delay in ms.
// 0: Invalid
// 1: GetInCar
// 2: Warmup
// 3: ParadeLaps
// 4: Racing
// 5: Checkered
// 6: Cooldown
function getSessionState(delay)
{
    state = $prop('DataCorePlugin.GameRawData.Telemetry.SessionState');

    // Initialize
    if (root['confirmed'] == null)
    {
        root['confirmed'] = state;
    }

    // Reset timer every time state changes
    if (state != root['state'])
    {
        root['state'] = state;
        root['changed'] = Date.now() + delay;
    }
    
    // State change confirmed after a delay
    if (Date.now() >= root['changed'])
    {
        root['confirmed'] = state;
    }
    
    return root['confirmed'];
}