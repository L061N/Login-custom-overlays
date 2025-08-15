using benofficial2.Plugin;

namespace benofficial2.Tests
{
    [TestClass]
    public sealed class FuelCalcTests
    {
        [TestMethod]
        public void UnknownConsumption()
        {
            FuelCalcModule.CalculateFuel(/*fuelLevel*/ 0.0,
                /*consumptionPerLapAvg*/ 0.0,
                /*currentLapHighPrecision*/ 1.0,
                /*estimatedTotalLaps*/ 20,
                /*isRace*/ false,
                /*isOval*/ false,
                /*maxFuelAllowed*/ 70,
                /*fuelReserve*/ 0.5,
                /*extraConsumptionPct*/ 1.0,
                /*extraRaceLaps*/ 0.0,
                /*extraRaceLapsOval*/ 3.0,
                /*evenFuelStints*/ false,
                out double remainingLaps,
                out int pitLap,
                out int pitWindowLap,
                out int pitStopsNeeded,
                out double refuelNeeded,
                out bool pitIndicatorOn,
                out bool pitWindowIndicatorOn,
                out double extraFuelAtFinish,
                out double consumptionTargetForExtraLap);

            Assert.AreEqual(remainingLaps, 0.0, Constants.LapEpsilon);
            Assert.AreEqual(pitLap, 0);
            Assert.AreEqual(pitWindowLap, 0);
            Assert.AreEqual(pitStopsNeeded, 0);
            Assert.AreEqual(refuelNeeded, 0.0, Constants.FuelEpsilon);
            Assert.AreEqual(pitIndicatorOn, false);
            Assert.AreEqual(pitWindowIndicatorOn, false);
            Assert.AreEqual(extraFuelAtFinish, 0.0, Constants.FuelEpsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.0, Constants.FuelEpsilon);
        }

        [TestMethod]
        public void RaceOverFueled()
        {
            FuelCalcModule.CalculateFuel(/*fuelLevel*/ 70.0,
                /*consumptionPerLapAvg*/ 0.5,
                /*currentLapHighPrecision*/ 0.0,
                /*estimatedTotalLaps*/ 20,
                /*isRace*/ true,
                /*isOval*/ false,
                /*maxFuelAllowed*/ 70,
                /*fuelReserve*/ 0.5,
                /*extraConsumptionPct*/ 1.0,
                /*extraRaceLaps*/ 0.0,
                /*extraRaceLapsOval*/ 3.0,
                /*evenFuelStints*/ false,
                out double remainingLaps,
                out int pitLap,
                out int pitWindowLap,
                out int pitStopsNeeded,
                out double refuelNeeded,
                out bool pitIndicatorOn,
                out bool pitWindowIndicatorOn,
                out double extraFuelAtFinish,
                out double consumptionTargetForExtraLap);

            Assert.AreEqual(remainingLaps, 139.0, Constants.FuelEpsilon);
            Assert.AreEqual(pitLap, 139);
            Assert.AreEqual(pitWindowLap, 0);
            Assert.AreEqual(pitStopsNeeded, 0);
            Assert.AreEqual(refuelNeeded, 0.0, Constants.FuelEpsilon);
            Assert.AreEqual(pitIndicatorOn, false);
            Assert.AreEqual(pitWindowIndicatorOn, false);
            Assert.AreEqual(extraFuelAtFinish, 60.0, Constants.FuelEpsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.49642857142857144, Constants.FuelEpsilon);
        }

        [TestMethod]
        public void RaceOneStop()
        {
            FuelCalcModule.CalculateFuel(/*fuelLevel*/ 70.0,
                /*consumptionPerLapAvg*/ 0.5,
                /*currentLapHighPrecision*/ 0.0,
                /*estimatedTotalLaps*/ 200,
                /*isRace*/ true,
                /*isOval*/ false,
                /*maxFuelAllowed*/ 70,
                /*fuelReserve*/ 0.5,
                /*extraConsumptionPct*/ 1.0,
                /*extraRaceLaps*/ 0.0,
                /*extraRaceLapsOval*/ 3.0,
                /*evenFuelStints*/ false,
                out double remainingLaps,
                out int pitLap,
                out int pitWindowLap,
                out int pitStopsNeeded,
                out double refuelNeeded,
                out bool pitIndicatorOn,
                out bool pitWindowIndicatorOn,
                out double extraFuelAtFinish,
                out double consumptionTargetForExtraLap);

            Assert.AreEqual(remainingLaps, 139.0, Constants.FuelEpsilon);
            Assert.AreEqual(pitLap, 139);
            Assert.AreEqual(pitWindowLap, 63);
            Assert.AreEqual(pitStopsNeeded, 1);
            Assert.AreEqual(refuelNeeded, 30.805, Constants.FuelEpsilon);
            Assert.AreEqual(pitIndicatorOn, false);
            Assert.AreEqual(pitWindowIndicatorOn, false);
            Assert.AreEqual(extraFuelAtFinish, 0.0, Constants.FuelEpsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.49642857142857144, Constants.FuelEpsilon);
        }

        [TestMethod]
        public void RaceOneStopAtWindow()
        {
            FuelCalcModule.CalculateFuel(/*fuelLevel*/ 39.0,
                /*consumptionPerLapAvg*/ 0.5,
                /*currentLapHighPrecision*/ 62.0,
                /*estimatedTotalLaps*/ 200,
                /*isRace*/ true,
                /*isOval*/ false,
                /*maxFuelAllowed*/ 70,
                /*fuelReserve*/ 0.5,
                /*extraConsumptionPct*/ 1.0,
                /*extraRaceLaps*/ 0.0,
                /*extraRaceLapsOval*/ 3.0,
                /*evenFuelStints*/ false,
                out double remainingLaps,
                out int pitLap,
                out int pitWindowLap,
                out int pitStopsNeeded,
                out double refuelNeeded,
                out bool pitIndicatorOn,
                out bool pitWindowIndicatorOn,
                out double extraFuelAtFinish,
                out double consumptionTargetForExtraLap);

            Assert.AreEqual(remainingLaps, 77.0, Constants.FuelEpsilon);
            Assert.AreEqual(pitLap, 139);
            Assert.AreEqual(pitWindowLap, 63);
            Assert.AreEqual(pitStopsNeeded, 1);
            Assert.AreEqual(refuelNeeded, 30.805, Constants.FuelEpsilon);
            Assert.AreEqual(pitIndicatorOn, false);
            Assert.AreEqual(pitWindowIndicatorOn, true);
            Assert.AreEqual(extraFuelAtFinish, 0.0, Constants.FuelEpsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.49358974358974361, Constants.FuelEpsilon);
        }

        [TestMethod]
        public void RaceOneStopAtPitLap()
        {
            FuelCalcModule.CalculateFuel(/*fuelLevel*/ 1.0,
                /*consumptionPerLapAvg*/ 0.5,
                /*currentLapHighPrecision*/ 138.0,
                /*estimatedTotalLaps*/ 200,
                /*isRace*/ true,
                /*isOval*/ false,
                /*maxFuelAllowed*/ 70,
                /*fuelReserve*/ 0.5,
                /*extraConsumptionPct*/ 1.0,
                /*extraRaceLaps*/ 0.0,
                /*extraRaceLapsOval*/ 3.0,
                /*evenFuelStints*/ false,
                out double remainingLaps,
                out int pitLap,
                out int pitWindowLap,
                out int pitStopsNeeded,
                out double refuelNeeded,
                out bool pitIndicatorOn,
                out bool pitWindowIndicatorOn,
                out double extraFuelAtFinish,
                out double consumptionTargetForExtraLap);

            Assert.AreEqual(remainingLaps, 1.0, Constants.FuelEpsilon);
            Assert.AreEqual(pitLap, 139);
            Assert.AreEqual(pitWindowLap, 63);
            Assert.AreEqual(pitStopsNeeded, 1);
            Assert.AreEqual(refuelNeeded, 30.805, Constants.FuelEpsilon);
            Assert.AreEqual(pitIndicatorOn, true);
            Assert.AreEqual(pitWindowIndicatorOn, true);
            Assert.AreEqual(extraFuelAtFinish, 0.0, Constants.FuelEpsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.25, Constants.FuelEpsilon);
        }

        [TestMethod]
        public void RaceTwoStopEvenStints()
        {
            FuelCalcModule.CalculateFuel(/*fuelLevel*/ 70.0,
                /*consumptionPerLapAvg*/ 0.5,
                /*currentLapHighPrecision*/ 0.0,
                /*estimatedTotalLaps*/ 300,
                /*isRace*/ true,
                /*isOval*/ false,
                /*maxFuelAllowed*/ 70,
                /*fuelReserve*/ 0.5,
                /*extraConsumptionPct*/ 1.0,
                /*extraRaceLaps*/ 0.0,
                /*extraRaceLapsOval*/ 3.0,
                /*evenFuelStints*/ true,
                out double remainingLaps,
                out int pitLap,
                out int pitWindowLap,
                out int pitStopsNeeded,
                out double refuelNeeded,
                out bool pitIndicatorOn,
                out bool pitWindowIndicatorOn,
                out double extraFuelAtFinish,
                out double consumptionTargetForExtraLap);

            Assert.AreEqual(remainingLaps, 139.0, Constants.FuelEpsilon);
            Assert.AreEqual(pitLap, 139);
            Assert.AreEqual(pitWindowLap, 24);
            Assert.AreEqual(pitStopsNeeded, 2);
            Assert.AreEqual(refuelNeeded, 40.4025, Constants.FuelEpsilon);
            Assert.AreEqual(pitIndicatorOn, false);
            Assert.AreEqual(pitWindowIndicatorOn, false);
            Assert.AreEqual(extraFuelAtFinish, 0.0, Constants.FuelEpsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.49642857142857144, Constants.FuelEpsilon);
        }

        [TestMethod]
        public void RaceOneStopOval()
        {
            FuelCalcModule.CalculateFuel(/*fuelLevel*/ 70.0,
                /*consumptionPerLapAvg*/ 0.5,
                /*currentLapHighPrecision*/ 0.0,
                /*estimatedTotalLaps*/ 200,
                /*isRace*/ true,
                /*isOval*/ true,
                /*maxFuelAllowed*/ 70,
                /*fuelReserve*/ 0.5,
                /*extraConsumptionPct*/ 1.0,
                /*extraRaceLaps*/ 0.0,
                /*extraRaceLapsOval*/ 3.0,
                /*evenFuelStints*/ false,
                out double remainingLaps,
                out int pitLap,
                out int pitWindowLap,
                out int pitStopsNeeded,
                out double refuelNeeded,
                out bool pitIndicatorOn,
                out bool pitWindowIndicatorOn,
                out double extraFuelAtFinish,
                out double consumptionTargetForExtraLap);

            Assert.AreEqual(remainingLaps, 139.0, Constants.FuelEpsilon);
            Assert.AreEqual(pitLap, 139);
            Assert.AreEqual(pitWindowLap, 66);
            Assert.AreEqual(pitStopsNeeded, 1);
            Assert.AreEqual(refuelNeeded, 32.32, Constants.FuelEpsilon);
            Assert.AreEqual(pitIndicatorOn, false);
            Assert.AreEqual(pitWindowIndicatorOn, false);
            Assert.AreEqual(extraFuelAtFinish, 0.0, Constants.FuelEpsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.49642857142857144, Constants.FuelEpsilon);
        }
    }
}
