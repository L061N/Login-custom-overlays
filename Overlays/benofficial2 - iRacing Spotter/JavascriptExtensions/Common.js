/*
    benofficial2's Official Overlays
    Copyright (C) 2023-2025 benofficial2

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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