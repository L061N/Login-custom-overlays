function isGameIRacing()
{
    var running = $prop('DataCorePlugin.GameRunning') == 1;
    var iRacing = $prop('DataCorePlugin.CurrentGame') == 'IRacing'
    return running && iRacing;
}

function isReplayPlaying()
{
    if (isGameIRacing())
    {
        return $prop('DataCorePlugin.GameRawData.Telemetry.IsReplayPlaying');
    }
    return false;
}

// This is the clutch that's calibrated with a bite point.
function getClutch()
{
    return (1 - $prop('GameRawData.Telemetry.ClutchRaw')) * 100;

    // Must use the actual joystick value if we want to separate clutch1 / clutch2
    //var clutchPedal = $prop('JoystickPlugin.Heusinkveld_Sim_Pedals_Ultimate_RZ') / 65535;
    //var wheelPaddle = $prop('JoystickPlugin.F-CORE_RX') / 65535;
    //return Math.min(1, Math.max(clutchPedal, wheelPaddle)) * 100;
}

// This is the clutch that goes to 100%. It can be a button.
function getClutch2()
{
    return 0;

    // There's no property to get the second clutch independently. Must use joystick.
    //var wheelButton = $prop('InputStatus.JoystickPlugin.FANATEC_ClubSport_Wheel_Base_V2.5_B09');
    //var boxButton = $prop('InputStatus.JoystickPlugin.OSH_PB_Controller_(APEXRD)_B03');
    //return Math.min(1, Math.max(wheelButton, boxButton)) * 100;
}

function getThrottle()
{
    return $prop('Throttle');
}