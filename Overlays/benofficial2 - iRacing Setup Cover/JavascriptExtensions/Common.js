function isGameIRacing()
{
    return $prop('DataCorePlugin.CurrentGame') == 'IRacing';
}

function isGameRunning()
{
    return $prop('DataCorePlugin.GameRunning');
}