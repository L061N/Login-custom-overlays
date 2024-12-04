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