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

using System;
using System.Collections.Generic;
using System.Linq;

namespace benofficial2.Plugin
{
    public class FuelConsumptionTracker
    {
        private double lastLapPosition = -1.0;
        private bool lastLapValid = false;

        private bool wasInvalidated = false;
        private int lapIncidentCount = 0;

        private double lapFuelStart = -1.0;

        private readonly List<double> allConsumptions = new List<double>();

        public void Update(double lapPosition, double fuelLevel, bool invalidate, int incidentCount)
        {
            // First update → initialize
            if (lastLapPosition < 0.0)
            {
                lastLapPosition = lapPosition;
                lapFuelStart = fuelLevel;
                wasInvalidated = invalidate;
                lapIncidentCount = incidentCount;
                return;
            }

            // Detect lap completion (lapPosition wrapped around)
            if (lapPosition < lastLapPosition)
            {
                double lapFuelConsumed = lapFuelStart - fuelLevel;

                bool incidentHappened = (incidentCount > lapIncidentCount);
                if (!wasInvalidated && !incidentHappened && lapFuelConsumed > 0)
                {
                    allConsumptions.Add(lapFuelConsumed);
                    lastLapValid = true;
                }
                else
                {
                    lastLapValid = false;
                }

                // Reset for next lap
                lapFuelStart = fuelLevel;
                wasInvalidated = false;
                lapIncidentCount = incidentCount;
            }

            // Update flags during current lap
            if (invalidate) wasInvalidated = true;

            lastLapPosition = lapPosition;
        }

        /// <summary>
        /// Average consumption over the last N valid laps.
        /// </summary>
        public double GetRecentConsumption(int lastLaps)
        {
            if (allConsumptions.Count == 0 || lastLaps <= 0) return 0.0;
            var recent = allConsumptions.Skip(Math.Max(0, allConsumptions.Count - lastLaps));
            return recent.Average();
        }

        /// <summary>
        /// Consumption at the given percentile (0–100) across ALL valid laps ever tracked.
        /// Example: 50 = median, 90 = 90th percentile.
        /// </summary>
        public double GetConsumption(int percentile)
        {
            if (allConsumptions.Count == 0) return 0.0;

            percentile = Math.Max(0, Math.Min(100, percentile));

            var sorted = allConsumptions.OrderBy(x => x).ToList();
            double index = (percentile / 100.0) * (sorted.Count - 1);
            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);

            if (lower == upper)
                return sorted[lower];

            double fraction = index - lower;
            return sorted[lower] + (sorted[upper] - sorted[lower]) * fraction;
        }

        public int GetValidLapCount() => allConsumptions.Count;

        public bool IsLastLapValid() => lastLapValid;

        public void Reset()
        {
            lastLapPosition = -1.0;
            lastLapValid = false;
            wasInvalidated = false;
            lapIncidentCount = 0;
            lapFuelStart = -1.0;
            allConsumptions.Clear();
        }
    }
}
