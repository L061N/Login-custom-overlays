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
        return $prop('DataCorePlugin.GameRawData.Telemetry.IsReplayPlaying');
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