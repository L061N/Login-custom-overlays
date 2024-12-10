//
// Config.js
// Author: benofficial2
// Date: 2023/09/10
//
// Use this file to configure which car supports which in-car setting.
//
// If this code was helpful to you, please consider following me on Twitch
// http://twitch.tv/benofficial2
//

function carIsGT3()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'mclaren720sgt3') ||
           (carId == 'chevyvettez06rgt3') ||
           (carId == 'fordmustanggt3') ||
           (carId == 'audir8lmsevo2gt3') ||
           (carId == 'ferrari296gt3') ||
           (carId == 'porsche992rgt3') ||
           (carId == 'mercedesamggt3') ||
           (carId == 'bmwm4gt3') ||
           (carId == 'lamborghinievogt3') ||
           (carId == 'mclarenmp4') ||
           (carId == 'porsche911rgt3') ||
           (carId == 'ferrari488gt3') ||
           (carId == 'audir8gt3') ||
           (carId == 'bmwz4gt3');
}

function carIsGT4()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'bmwm4gt4') ||
           (carId == 'bmwm4evogt4') ||
           (carId == 'mercedesamggt4') ||
           (carId == 'amvantagegt4') ||
           (carId == 'porsche718gt4') ||
           (carId == 'mclaren570sgt4');
}

function carIsGTE()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'bmwm8gte') ||
           (carId == 'porsche991rsr') ||
           (carId == 'ferrari488gte') ||
           (carId == 'c8rvettegte') ||
           (carId == 'fordgt2017');
}

function carIsGTP()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'cadillacvseriesrgtp') ||
           (carId == 'acuraarx06gtp') ||
           (carId == 'porsche963gtp');
}

// Used for the small top green box.
function carHasPushToPass()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'dallarair18') ||
           (carId == 'dallaradw12') ||
           (carId == 'superformulasf23 toyota') ||
           (carId == 'superformulasf23 honda');
}

// Used for the upper green box.
function carHasPushToPassCount()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'dallarair18') ||
           (carId == 'dallaradw12');
}

// Used for the upper green box.
function carHasPushToPassTimer()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'superformulasf23 toyota') ||
           (carId == 'superformulasf23 honda');
}

// Used for the small top green box.
function carHasPushToPassCooldown()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'superformulasf23 toyota') ||
           (carId == 'superformulasf23 honda');
}

// Used for the small top green box.
function carHasDrsDetection()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'mclarenmp430') || 
           (carId == 'mercedesw12') || 
           (carId == 'mercedesw13');
}

// Used for the small top green box.
function carHasDrsCount()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'formularenault35');
}

// Used for the upper green box.
function carHasErs()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return carIsGTP() || 
           (carId == 'mercedesw12') || 
           (carId == 'mercedesw13') ||
           (carId == 'mclarenmp430');
}

// Used for the upper green box.
function carHasBoost()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'radicalsr10');
}

// Used for the upper green box.
function carHasEnginePowerMode()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'williamsfw31');
}

// Used for the lower green box.
function carHasDeployMode()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return carIsGTP() || 
           (carId == 'mercedesw12') || 
           (carId == 'mercedesw13') ||
           (carId == 'mclarenmp430');
}

// Used for the lower green box.
// This mode shows a target SoC value, or 'Qual'.
function carHasDeployModeType1()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'mclarenmp430');
}

// Used for the lower green box.
// This mode shows a named mode 'Build', 'Bal', etc.
function carHasDeployModeType2()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return carIsGTP() || 
           (carId == 'mercedesw12') ||
           (carId == 'mercedesw13');
}

// Used for the lower green box.
function carHasThrottleShaping()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return carIsGT3() || 
           (carId == 'formularenault35') ||
           (carId == 'radicalsr10') ||
           (carId == 'dallarap217') ||
           (carId == 'dallarair01') ||
           (carId == 'superformulasf23 toyota') ||
           (carId == 'superformulasf23 honda') ||
           (carId == 'ligierjsp320');
}

// Used for the lower green box.
function carHasFuelMix()
{
    const carId = $prop('DataCorePlugin.GameData.CarId');
    return carIsGTE() || 
           (carId == 'dallarair18') ||
           (carId == 'dallaradw12') ||
           (carId == 'williamsfw31');
}

// Used for cars that show their ARB as 'P2' instead of '1'.
function carHasARBModeP()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'dallarair01');
}

// Used for the upper orange box.
function carHasFrontARB()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'dallarair18') ||
           (carId == 'dallaradw12') ||
           (carId == 'dallarair01');
}

// Used for the upper orange box.
function carHasExitDiff()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'williamsfw31');
}

// Used for the upper orange box.
function carHasEntryDiffPreload()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'mclarenmp430');
}

// Used for the upper orange box.
function carHasTC2()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return carIsGTE() ||
           carIsGTP() ||
           (carId == 'dallarap217');
}

// Used for the lower orange box.
function carHasRearARB()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'dallarair18') ||
           (carId == 'dallaradw12') ||
           (carId == 'dallarair01');
}

// Used for the lower orange box.
function carHasEntryDiff()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'mclarenmp430') ||
           (carId == 'mercedesw13') ||
           (carId == 'williamsfw31');
}

// Used for the lower orange box.
function carHasTC()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return carIsGT3() ||
           carIsGTE() ||
           carIsGT4() ||
           carIsGTP() ||
           (carId == 'dallarap217') ||
           (carId == 'ligierjsp320');
}

// Used for the upper red box.
function carHasTwoPartBrakeBias()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'mclarenmp430');
}

// Used for the lower red box.
function carHasWeightJacker()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'dallarair18') ||
           (carId == 'dallaradw12');
}

// Used for the lower red box.
function carHasTwoPartPeakBrakeBias()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'mclarenmp430');
}

// Used for the lower red box.
function carHasABS()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return carIsGT3() ||
           carIsGT4();
}

// Used for the lower red box.
function carHasFineBrakeBias()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'mercedesw13');
}

// Used for the lower red box.
function carHasBrakeBiasMigration()
{
    var carId = $prop('DataCorePlugin.GameData.CarId');
    return (carId == 'cadillacvseriesrgtp') ||
           (carId == 'porsche963gtp');
}