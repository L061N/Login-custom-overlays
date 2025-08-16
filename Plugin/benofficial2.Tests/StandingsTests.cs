/*
    benofficial2's Official Overlays
    Copyright (C) 2025 benofficial2

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

using benofficial2.Plugin;

namespace benofficial2.Tests
{
    [TestClass]
    public sealed class StandingsTests
    {
        [TestMethod]
        public void EstimateTotalLaps_LapRace_EnoughTime()
        {
            int laps;
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ -1.0, /*sessionTotalLaps*/ 30, /*sessionTimeRemain*/ 3600.0, /*avgLapTime*/ 90.0);
            Assert.AreEqual(30, laps);

            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 0.0, /*sessionTotalLaps*/ 30, /*sessionTimeRemain*/ 3600.0, /*avgLapTime*/ 90.0);
            Assert.AreEqual(30, laps);

            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 15.0, /*sessionTotalLaps*/ 30, /*sessionTimeRemain*/ 3600.0, /*avgLapTime*/ 90.0);
            Assert.AreEqual(30, laps);

            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 30.0, /*sessionTotalLaps*/ 30, /*sessionTimeRemain*/ 3600.0, /*avgLapTime*/ 90.0);
            Assert.AreEqual(30, laps);

            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 31.0, /*sessionTotalLaps*/ 30, /*sessionTimeRemain*/ 3600.0, /*avgLapTime*/ 90.0);
            Assert.AreEqual(30, laps);
        }

        [TestMethod]
        public void EstimateTotalLaps_TimeRace_ExtraLap_HitZeroFlush()
        {
            int laps;
            double avgLapTime = 90.0;

            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 0.0, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ 900.0, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 8.7, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ 117.0, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Not getting the white flag here because the timer would hit zero just as would cross the line on the next lap. So we get an extra lap.
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 8.9, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ 99.0, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 9.1, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ 81.0, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 9.7, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ 27.0, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Getting the white flag here because the timer will hit zero just as we cross the line.
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 9.9, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ 9.0, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Should have had the white flag by now. Timer reached zero.
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 10.1, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ 0.0, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 10.7, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ 0.0, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Estimate keeps going up at this point as if we did not get the white flag.
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ 10.9, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ 0.0, /*avgLapTime*/ avgLapTime);
            //Assert.AreEqual(11, laps);
        }

        [TestMethod]
        public void EstimateTotalLaps_TimeRace_NoExtraLap_HitZeroEarly()
        {
            int laps;
            double lap, time;
            double avgLapTime = 89.0;
            double sessionTimeTotal = 900.0;

            lap = 0.0;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            lap = 8.7;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            lap = 8.9;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            lap = 9.1;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            lap = 9.7;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Getting the white flag here because the timer will hit zero early in the next lap.
            lap = 9.9;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Got the white flag, timer almost zero.
            lap = 10.1;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Timer reached zero. 
            lap = 10.3;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            lap = 10.7;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Estimate keeps going up at this point as if we did not get the white flag.
            lap = 10.9;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            //Assert.AreEqual(11, laps);
        }

        [TestMethod]
        public void EstimateTotalLaps_TimeRace_ExtraLap_HitZeroLate()
        {
            int laps;
            double lap, time;
            double avgLapTime = 91.0;
            double sessionTimeTotal = 900.0;

            lap = 0.0;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            lap = 8.7;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Not getting white flag here because timer could run out very close to crossing the line on next lap.
            lap = 8.9;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            lap = 9.1;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            lap = 9.7;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Timer hit zero, but we still have one more lap since we did not get the white flag on the previous lap.
            lap = 9.9;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Should have had the white flag by now.
            lap = 10.1;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            lap = 10.7;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(11, laps);

            // Estimate keeps going up to 12 laps at this point as if we did not get the white flag.
            lap = 10.9;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            //Assert.AreEqual(11, laps);
        }

        [TestMethod]
        public void EstimateTotalLaps_TimeRace_NoExtraLap_HitZeroLate()
        {
            int laps;
            double lap, time;
            double avgLapTime = 92.0;
            double sessionTimeTotal = 900.0;

            lap = 0.0;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(10, laps);

            lap = 8.7;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(10, laps);

            // Getting the white flag here because the timer would run out on the next lap.
            lap = 8.9;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(10, laps);

            // Should have had the white flag here.
            lap = 9.1;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(10, laps);

            lap = 9.7;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            Assert.AreEqual(10, laps);

            // Timer hit zero
            // Estimate keeps going up at this point as if we did not get the white flag.
            lap = 9.9;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            //Assert.AreEqual(10, laps);

            lap = 10.1;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            //Assert.AreEqual(10, laps);

            lap = 10.7;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            //Assert.AreEqual(10, laps);

            lap = 10.9;
            time = Math.Max(0.0, sessionTimeTotal - (avgLapTime * lap));
            laps = StandingsModule.EstimateTotalLaps(/*currentLapHighPrecision*/ lap, /*sessionTotalLaps*/ 0, /*sessionTimeRemain*/ time, /*avgLapTime*/ avgLapTime);
            //Assert.AreEqual(10, laps);
        }
    }
}
