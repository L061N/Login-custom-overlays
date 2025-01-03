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
    const sessionTypeName = $prop('DataCorePlugin.GameData.SessionTypeName');
    return String(sessionTypeName).indexOf('Race') != -1;   
}

function isQual()
{
    const sessionTypeName = $prop('DataCorePlugin.GameData.SessionTypeName');
    return String(sessionTypeName).indexOf('Qual') != -1;
}

function isLoneQual()
{
    const sessionTypeName = $prop('DataCorePlugin.GameData.SessionTypeName');
    return String(sessionTypeName).indexOf('Lone Qualify') != -1;
}

function isPractice()
{
    const sessionTypeName = $prop('DataCorePlugin.GameData.SessionTypeName');
    return (String(sessionTypeName).indexOf('Practice') != -1) ||
           (String(sessionTypeName).indexOf('Warmup') != -1) ||
           (String(sessionTypeName).indexOf('Testing') != -1);
}

function isOffline()
{
    const sessionTypeName = $prop('DataCorePlugin.GameData.SessionTypeName');
    return String(sessionTypeName).indexOf('Offline') != -1;
}

function getSessionStarted(delay)
{
    // 0: Invalid
    // 1: GetInCar
    // 2: Warmup
    // 3: ParadeLaps
    // 4: Racing
    // 5: Checkered
    // 6: Cooldown
    const state = $prop('DataCorePlugin.GameRawData.Telemetry.SessionState');

    if (state < 4)
    {
        root['changed'] = null
        return false;
    }

    // State change confirmed after a delay
    if (root['changed'] == null)
    {
        root['changed'] = Date.now() + delay;
    }

    if (Date.now() < root['changed'])
    {
        return false;
    }
    
    return true;
}

function getIndexedProp(name, index)
{
    return $prop(name + '_' + format(index, '00'));
}