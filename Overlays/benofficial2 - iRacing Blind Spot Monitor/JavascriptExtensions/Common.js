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

function isLoneQual()
{
    var sessionTypeName = $prop('DataCorePlugin.GameData.SessionTypeName');
    return String(sessionTypeName).indexOf('Lone Qualify') != -1;
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

function getIndexedProp(name, index)
{
    return $prop(name + '_' + format(index, '00'));
}