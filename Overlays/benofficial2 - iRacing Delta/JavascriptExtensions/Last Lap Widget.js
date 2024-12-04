// Used at the end of a lap to display the delta with the reference time
// that was used on the previous lap.
function getDeltaToPreviousReferenceTime()
{
    // Fix for the first laps of the race.
    var lap = $prop('DataCorePlugin.GameData.CurrentLap');
    if (isRace() && lap <= 2)
    {
        return '';
    }

    var lastLapTime = $prop('DataCorePlugin.GameData.LastLapTime');
    var previousReferenceTime = $prop('variable.previousReferenceTime');
    return computeDeltaTime(lastLapTime, previousReferenceTime);
}

// This is for the delta shown on the big lap timer.
function showTimerDelta()
{
    var lap = $prop('DataCorePlugin.GameData.CurrentLap');
    if (lap <= 1)
    {
        return false;
    }
    
    var lastTime = $prop('DataCorePlugin.GameData.LastLapTime');
    if (isInvalidTime(lastTime))
    {
        // Happens when getting out of the pit after a reset.
        return false;
    }

    var time = $prop('DataCorePlugin.GameData.CurrentLapTime');
    var d = new Date(time);
    var min = d.getMinutes();
    var sec = d.getSeconds();
    var hideAfterSecs = 10

    if (min > 0 || sec > hideAfterSecs)
    {
        return false;
    }

    return true;
}