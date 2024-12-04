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

// Returns the value of a leaderboard property.
// Ex:
//   getLeaderboardProp('IsInPit', 1, 0)
//   returns value of IRacingExtraProperties.iRacing_Class_01_Leaderboard_Driver_00_IsInPit
function getLeaderboardProp(name, classIndex, driverIndex)
{
    return $prop('IRacingExtraProperties.iRacing_Class_' 
        + format(classIndex, '00') 
        + '_Leaderboard_Driver_' 
        + format(driverIndex, '00')
        + '_'
        + name);
}

function addZero(i) 
{
	if (i < 10) 
	{
		i = "0" + i;
	}
	return i;
}

function getTimeFromSeconds(seconds)
{
    if (seconds > 172800)
    {
        return '';
    }
	var	date = new Date(1000 * parseFloat(seconds));
	if (date.getUTCHours() > 0)
	{
		return date.getUTCHours() + ':' + addZero(date.getUTCMinutes()) + ':' + addZero(date.getUTCSeconds());
	}
	return date.getUTCMinutes() + ':' + addZero(date.getUTCSeconds());
}

function isInvalidTime(time)
{
    return time == null || time == '00:00:00' || time == '00:00.000';
}

function carHasTireCompounds()
{
    var tire = $prop('DataCorePlugin.GameRawData.SessionData.CarSetup.Tires.TireCompound.TireCompound');
    var tire2 = $prop('DataCorePlugin.GameRawData.SessionData.CarSetup.TiresAero.TireCompound.TireCompound');
    return tire != null || tire2 != null;
}

function lastPitLap(index)
{
    // Does not work for standings
    return 0;

    /*var currentLap = $prop('PersistantTrackerPlugin.DriverAhead_0' + (index - 1) + '_CurrentLap');
    var lapsSincePit = $prop('IRacingExtraProperties.iRacing_DriverAhead_0' + (index - 1) + '_LapsSinceLastPit');
    return currentLap - lapsSincePit - 1;*/
}

function isLeadFocusedRow(index)
{
    // Hack because we don't have a property 'SLB_LeadFocusedRowCount';
    var dividerVisible = $prop('IRacingExtraProperties.SLB_Top15DividerVisible');
    return dividerVisible && index <= 3;
    
    //var pos = getIndexedProp('IRacingExtraProperties.SLB_PositionInClass', index);
    //return Number(String(pos).substring(0, String(pos).length - 1)) == index;
}

function rowTop(index)
{
    var gap = 0;
    if (!isLeadFocusedRow(index))
    {
        gap = 3;
    }
    return 25 + gap;
}

function isConnected(index)
{
    var pos = getIndexedProp('IRacingExtraProperties.SLB_Position', index);
    var i = parseInt(pos) - 1;
    if (i >= 0)
    {
        return $prop('IRacingExtraProperties.iRacing_Leaderboard_Driver_' + addZero(i) + '_IsConnected');
    }
    return true;
}

function tireCompoundVisible(index)
{
    var tireString = getIndexedProp('IRacingExtraProperties.SLB_TireCompound', index);
    if (!(String(tireString).length > 0))
    {
        return false;
    }

    const tireType = getTireType(index);
    if (tireType == 'Hidden')
    {
        return false;
    }

    if (isRace())
    {
        return raceInProgress();
    }

    var inPit = getIndexedProp('IRacingExtraProperties.SLB_IsInPit', index);
    var connected = isConnected(index);
    return connected && !inPit;
}

// Returns True if the specified flag is out.
// Supported colors: 'Black', 'Blue', 'Checkered', 'Green', 'Orange', 'White', 'Yellow'.
function isFlagOut(color)
{
    var flagOut = $prop('DataCorePlugin.GameData.Flag_' + color);
    return flagOut != 0;
}

function getCarIdx(fromCarPosition)
{
    const positions = $prop('GameRawData.Telemetry.CarIdxPosition');
    if (positions != null)
    {
        for (let i = 0; i < positions.length; i++) 
        {
            if (positions[i] == fromCarPosition)
            {
                return i;
            }
        }
    }   
    return null;    
}

function getTireType(fromSLBIndex)
{
    let pos = getIndexedProp('IRacingExtraProperties.SLB_Position', fromSLBIndex);
    pos = pos.substring(0, pos.length - 2);
    const idx = getCarIdx(pos);
    const tires = $prop('GameRawData.Telemetry.CarIdxTireCompound')
    if (tires == null || idx == null)
    {
        return 'Dry';
    }
    switch(tires[idx]) 
    {
        case -1:
        return 'Hidden'

        case 0:
        return 'Dry'
        
        case 1:
        return 'Wet'

        default:
        return 'Unknown'
    } 
}

function raceInProgress()
{
    if (isRace())
    {
        if (root["greenTime"] == null)
        {
            // For the race to be considered in progress, we need to have been in state 4 at least once.
            const isGreen = $prop('DataCorePlugin.GameRawData.Telemetry.SessionState') == 4;
            if (isGreen)
            {
                root["greenTime"] = Date.now();
            }
        }

        if (root["greenTime"] != null)
        {
            // Wait a few seconds until the data is ready
            return Date.now() - root["greenTime"] > 3000;
        }
        return false;
    }
    else
    {
        root["greenTime"] = null;
        return false;
    }
}