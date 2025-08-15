using benofficial2.Plugin;

namespace benofficial2.Tests
{
    [TestClass]
    public sealed class FuelCalcTests
    {
        private const double Epsilon = 1e-9;

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

            Assert.AreEqual(remainingLaps, 0.0, Epsilon);
            Assert.AreEqual(pitLap, 0);
            Assert.AreEqual(pitWindowLap, 0);
            Assert.AreEqual(pitStopsNeeded, 0);
            Assert.AreEqual(refuelNeeded, 0.0, Epsilon);
            Assert.AreEqual(pitIndicatorOn, false);
            Assert.AreEqual(pitWindowIndicatorOn, false);
            Assert.AreEqual(extraFuelAtFinish, 0.0, Epsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.0, Epsilon);
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

            Assert.AreEqual(remainingLaps, 139.0, Epsilon);
            Assert.AreEqual(pitLap, 139);
            Assert.AreEqual(pitWindowLap, 0);
            Assert.AreEqual(pitStopsNeeded, 0);
            Assert.AreEqual(refuelNeeded, 0.0, Epsilon);
            Assert.AreEqual(pitIndicatorOn, false);
            Assert.AreEqual(pitWindowIndicatorOn, false);
            Assert.AreEqual(extraFuelAtFinish, 60.0, Epsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.49642857142857144, Epsilon);
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

            Assert.AreEqual(remainingLaps, 139.0, Epsilon);
            Assert.AreEqual(pitLap, 139);
            Assert.AreEqual(pitWindowLap, 63);
            Assert.AreEqual(pitStopsNeeded, 1);
            Assert.AreEqual(refuelNeeded, 30.805, Epsilon);
            Assert.AreEqual(pitIndicatorOn, false);
            Assert.AreEqual(pitWindowIndicatorOn, false);
            Assert.AreEqual(extraFuelAtFinish, 0.0, Epsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.49642857142857144, Epsilon);
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

            Assert.AreEqual(remainingLaps, 77.0, Epsilon);
            Assert.AreEqual(pitLap, 139);
            Assert.AreEqual(pitWindowLap, 63);
            Assert.AreEqual(pitStopsNeeded, 1);
            Assert.AreEqual(refuelNeeded, 30.805, Epsilon);
            Assert.AreEqual(pitIndicatorOn, false);
            Assert.AreEqual(pitWindowIndicatorOn, true);
            Assert.AreEqual(extraFuelAtFinish, 0.0, Epsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.49358974358974361, Epsilon);
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

            Assert.AreEqual(remainingLaps, 1.0, Epsilon);
            Assert.AreEqual(pitLap, 139);
            Assert.AreEqual(pitWindowLap, 63);
            Assert.AreEqual(pitStopsNeeded, 1);
            Assert.AreEqual(refuelNeeded, 30.805, Epsilon);
            Assert.AreEqual(pitIndicatorOn, true);
            Assert.AreEqual(pitWindowIndicatorOn, true);
            Assert.AreEqual(extraFuelAtFinish, 0.0, Epsilon);
            Assert.AreEqual(consumptionTargetForExtraLap, 0.25, Epsilon);
        }
    }
}
