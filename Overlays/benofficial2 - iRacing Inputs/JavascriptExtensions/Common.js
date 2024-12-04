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