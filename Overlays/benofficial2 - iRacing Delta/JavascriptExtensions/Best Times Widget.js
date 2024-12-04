// Returns true to continuously display the delta between the last lap and
// the best time as it was on the previous lap.
function showDeltaToPreviousBest()
{
    // Hide until the new best time is updated.
    // Because trackPercent gets to 0 before the best time is updated.
    if ($prop('variable.lapTimesUpdated') == false)
    {
        return false;
    } 
    
    // Hide in the last 3% because that's when we update the static variables,
    // so the times would be wrong for a moment.
    var trackPercent = $prop('DataCorePlugin.GameData.TrackPositionPercent');
    if (trackPercent >= 0.97)
    {
        return false;
    }

    // No need to show the delta of the previous stint.
    if (isOutLap())
    {
        return false;
    }

    return true;
}

// Returns the delta between the last lap time and the all-time best time
// as it was on the previous lap. 
function getDeltaToPreviousAllTimeBest()
{
    if (!showDeltaToPreviousBest())
    {
        return '';
    }

    var lastLapTime = $prop('DataCorePlugin.GameData.LastLapTime');
    var previousBestTime = $prop('variable.previousAllTimeBest')

    // Empty if one of the time is invalid.
    return computeDeltaTime(lastLapTime, previousBestTime);
}

// Used by the Best Times Widget.
// Returns the delta between the last lap time and the session best time
// as it was on the previous lap. 
function getDeltaToPreviousSessionBest()
{
    if (!showDeltaToPreviousBest())
    {
        return '';
    }

    var lastLapTime = $prop('DataCorePlugin.GameData.LastLapTime');
    var previousBestTime = $prop('variable.previousSessionBestTime');

    // Empty if one of the time is invalid.
    return computeDeltaTime(lastLapTime, previousBestTime);
}
