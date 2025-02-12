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
using System.Text;
using System.Threading.Tasks;

namespace benofficial2.Plugin
{
    public class Driver
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Position { get; set; }


        public Driver(string name, int id, int position)
        {
            Name = name;
            Id = id;
            Position = position;
        }

        public override string ToString()
        {
            return $"Position: {Position}, Name: {Name}, ID: {Id}";
        }
    }

    public class Leaderboard
    {
        private const int MaxDrivers = 64;
        public List<Driver> Drivers { get; set; }

        public string Name { get; set; }
        public int ClassId { get; set; }

        public Leaderboard(string name, int classId)
        {
            Name = name;
            ClassId = classId;
            Drivers = new List<Driver>();
        }

        public bool AddDriver(Driver driver)
        {
            if (Drivers.Count >= MaxDrivers)
            {
                Console.WriteLine("Cannot add more drivers. Leaderboard is full.");
                return false;
            }

            Drivers.Add(driver);
            return true;
        }

        public void SortDriversByPosition()
        {
            Drivers.Sort((x, y) => x.Position.CompareTo(y.Position));
        }

        public void Display()
        {
            Console.WriteLine($"Leaderboard: {Name}");
            foreach (var driver in Drivers)
            {
                Console.WriteLine(driver);
            }
        }
    }

    public class LeaderboardCollection
    {
        private readonly List<Leaderboard> _leaderboards;

        public LeaderboardCollection()
        {
            _leaderboards = new List<Leaderboard>();
        }

        public IReadOnlyList<Leaderboard> Leaderboards => _leaderboards.AsReadOnly();

        public void AddLeaderboard(Leaderboard leaderboard)
        {
            _leaderboards.Add(leaderboard);
        }

        public void ChangeDriverPosition(int classId, int driverId, int newPosition)
        {
            var leaderboard = _leaderboards.Find(lb => lb.ClassId == classId);
            if (leaderboard == null)
            {
                Console.WriteLine($"No leaderboard found with ClassId {classId}.");
                return;
            }

            var driver = leaderboard.Drivers.Find(d => d.Id == driverId);
            if (driver == null)
            {
                Console.WriteLine($"No driver found with Id {driverId} in ClassId {classId}.");
                return;
            }

            driver.Position = newPosition;
            leaderboard.SortDriversByPosition();
            Console.WriteLine($"Updated position for Driver ID {driverId} to {newPosition} in ClassId {classId}.");
        }

        public void DisplayAll()
        {
            foreach (var leaderboard in _leaderboards)
            {
                leaderboard.Display();
                Console.WriteLine();
            }
        }
    }
}
