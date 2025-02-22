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

using GameReaderCommon;
using SimHub.Plugins;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System;

namespace benofficial2.Plugin
{
    public class StandingsSettings : INotifyPropertyChanged
    {
        private int _backgroundOpacity = 60;

        public int BackgroundOpacity
        {
            get { return _backgroundOpacity; }
            set
            {
                if (_backgroundOpacity != value)
                {
                    _backgroundOpacity = value;
                    OnPropertyChanged(nameof(BackgroundOpacity));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Driver
    {
        public bool RowVisible { get; set; } = false;
        public int Position { get; set; } = 0;
        public string CarNumber { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class CarClass
    {
        public const int MaxDrivers = 64;
        public List<Driver> Drivers;

        public CarClass()
        {
            Drivers = new List<Driver>(Enumerable.Range(0, MaxDrivers).Select(x => new Driver()));
        }
    }

    public class Standings : IPluginModule
    {
        private DateTime _lastPacketTime = DateTime.MinValue;
        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(500);

        public StandingsSettings Settings { get; set; }

        public const int MaxClasses = 4;
        public List<CarClass> CarClasses;

        public Standings()
        {
            CarClasses = new List<CarClass>(Enumerable.Range(0, MaxClasses).Select(x => new CarClass()));
        }

        public void Init(PluginManager pluginManager, benofficial2 plugin)
        {
            Settings = plugin.ReadCommonSettings<StandingsSettings>("StandingsSettings", () => new StandingsSettings());
            plugin.AttachDelegate(name: "Standings.BackgroundOpacity", valueProvider: () => Settings.BackgroundOpacity);

            for (int classIdx = 0; classIdx < MaxClasses; classIdx++)
            {
                CarClass carClass = CarClasses[classIdx];
                for (int driverIdx = 0; driverIdx < CarClass.MaxDrivers; driverIdx++)
                {
                    Driver driver = carClass.Drivers[driverIdx];
                    plugin.AttachDelegate(name: $"Standings.Class{classIdx:00}.Driver{driverIdx:00}.RowVisible", valueProvider: () => driver.RowVisible);
                    plugin.AttachDelegate(name: $"Standings.Class{classIdx:00}.Driver{driverIdx:00}.Position", valueProvider: () => driver.Position);
                    plugin.AttachDelegate(name: $"Standings.Class{classIdx:00}.Driver{driverIdx:00}.CarNumber", valueProvider: () => driver.CarNumber);
                    plugin.AttachDelegate(name: $"Standings.Class{classIdx:00}.Driver{driverIdx:00}.Name", valueProvider: () => driver.Name);
                }
            }
        }

        public void DataUpdate(PluginManager pluginManager, benofficial2 plugin, ref GameData data)
        {
            if (data.NewData.PacketTime - _lastPacketTime < _updateInterval) return;
            _lastPacketTime = data.NewData.PacketTime;

            for (int classIdx = 0; classIdx < MaxClasses; classIdx++)
            {
                CarClass carClass = CarClasses[classIdx];
                if (classIdx < data.NewData.OpponentsClassses.Count)
                {
                    List<Opponent> opponents = data.NewData.OpponentsClassses[classIdx].Opponents;
                    for (int driverIdx = 0; driverIdx < CarClass.MaxDrivers; driverIdx++)
                    {
                        Driver driver = carClass.Drivers[driverIdx];
                        if (driverIdx < opponents.Count)
                        {
                            Opponent opponent = opponents[driverIdx];
                            driver.RowVisible = opponent.Position > 0;
                            driver.Position = opponent.Position;
                            driver.CarNumber = opponent.CarNumber;
                            driver.Name = opponent.Name;
                        }
                        else
                        {
                            ResetDriver(driver);
                        }
                    }
                }
                else
                {
                    for (int driverIdx = 0; driverIdx < CarClass.MaxDrivers; driverIdx++)
                    {
                        Driver driver = carClass.Drivers[driverIdx];
                        ResetDriver(driver);
                    }
                }
            }
        }

        public void End(PluginManager pluginManager, benofficial2 plugin)
        {
            plugin.SaveCommonSettings("StandingsSettings", Settings);
        }

        public void ResetDriver(Driver driver)
        {
            driver.RowVisible = false;
            driver.Position = 0;
            driver.CarNumber = "";
            driver.Name = "";
        }
    }
}
