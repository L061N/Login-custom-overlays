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

using System.ComponentModel;
using System.Globalization;

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

    public class RelativeSettings : INotifyPropertyChanged
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

    public class TrackMapSettings : INotifyPropertyChanged
    {
        private int _backgroundOpacity = 0;

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

    public class DeltaSettings : INotifyPropertyChanged
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

    public class TelemetrySettings : INotifyPropertyChanged
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

    public class DashSettings : INotifyPropertyChanged
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

    public class LaunchAssistSettings : INotifyPropertyChanged
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

    public class SpotterSettings : INotifyPropertyChanged
    {
        private bool _enabled = true;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }

        private float _threshold = 5.5f;
        private string _thresholdString = "5.5";
        private bool _thresholdValid = true;

        public float Threshold
        {
            get { return _threshold; }
        }
        public string ThresholdString
        {
            get => _thresholdString;
            set
            {
                if (_thresholdString != value)
                {
                    _thresholdString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _threshold = result;
                        ThresholdValid = true;
                    }
                    else
                    {
                        _threshold = 0;
                        ThresholdValid = false;
                    }

                    OnPropertyChanged(nameof(Threshold));
                    OnPropertyChanged(nameof(ThresholdString));
                }
            }
        }
        public bool ThresholdValid
        {
            get => _thresholdValid;
            private set
            {
                if (_thresholdValid != value)
                {
                    _thresholdValid = value;
                    OnPropertyChanged(nameof(ThresholdValid));
                }
            }
        }

        private int _height = 129;

        public int Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        private int _minHeight = 15;

        public int MinHeight
        {
            get { return _minHeight; }
            set
            {
                if (_minHeight != value)
                {
                    _minHeight = value;
                    OnPropertyChanged(nameof(MinHeight));
                }
            }
        }

        private int _width = 12;

        public int Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        private int _border = 3;

        public int Border
        {
            get { return _border; }
            set
            {
                if (_border != value)
                {
                    _border = value;
                    OnPropertyChanged(nameof(Border));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RejoinHelperSettings : INotifyPropertyChanged
    {
        private bool _enabled = true;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }

        private float _minClearGap = 3.5f;
        private string _minClearGapString = "3.5";
        private bool _minClearGapValid = true;

        public float MinClearGap
        {
            get { return _minClearGap; }
        }
        public string MinClearGapString
        {
            get => _minClearGapString;
            set
            {
                if (_minClearGapString != value)
                {
                    _minClearGapString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _minClearGap = result;
                        MinClearGapValid = true;
                    }
                    else
                    {
                        _minClearGap = 0;
                        MinClearGapValid = false;
                    }

                    OnPropertyChanged(nameof(MinClearGap));
                    OnPropertyChanged(nameof(MinClearGapString));
                }
            }
        }
        public bool MinClearGapValid
        {
            get => _minClearGapValid;
            private set
            {
                if (_minClearGapValid != value)
                {
                    _minClearGapValid = value;
                    OnPropertyChanged(nameof(MinClearGapValid));
                }
            }
        }

        private float _minCareGap = 1.5f;
        private string _minCareGapString = "1.5";
        private bool _minCareGapValid = true;

        public float MinCareGap
        {
            get { return _minCareGap; }
        }
        public string MinCareGapString
        {
            get => _minCareGapString;
            set
            {
                if (_minCareGapString != value)
                {
                    _minCareGapString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _minCareGap = result;
                        MinCareGapValid = true;
                    }
                    else
                    {
                        _minCareGap = 0;
                        MinCareGapValid = false;
                    }

                    OnPropertyChanged(nameof(MinCareGap));
                    OnPropertyChanged(nameof(MinCareGapString));
                }
            }
        }
        public bool MinCareGapValid
        {
            get => _minCareGapValid;
            private set
            {
                if (_minCareGapValid != value)
                {
                    _minCareGapValid = value;
                    OnPropertyChanged(nameof(MinCareGapValid));
                }
            }
        }

        private int _minSpeed = 35;

        public int MinSpeed
        {
            get { return _minSpeed; }
            set
            {
                if (_minSpeed != value)
                {
                    _minSpeed = value;
                    OnPropertyChanged(nameof(MinSpeed));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BlindSpotMonitorSettings : INotifyPropertyChanged
    {
        private bool _enabled = false;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

        public class FuelCalcSettings : INotifyPropertyChanged
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

        private float _fuelReserve = 0.5f;
        private string _fuelReserveString = "0.5";
        private bool _fuelReserveValid = true;

        public float FuelReserve
        {
            get { return _fuelReserve; }
        }
        public string FuelReserveString
        {
            get => _fuelReserveString;
            set
            {
                if (_fuelReserveString != value)
                {
                    _fuelReserveString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _fuelReserve = result;
                        FuelReserveValid = true;
                    }
                    else 
                    {
                        _fuelReserve = 0;
                        FuelReserveValid = false;
                    }

                    OnPropertyChanged(nameof(FuelReserve));
                    OnPropertyChanged(nameof(FuelReserveString));
                }
            }
        }
        public bool FuelReserveValid
        {
            get => _fuelReserveValid;
            private set
            {
                if (_fuelReserveValid != value)
                {
                    _fuelReserveValid = value;
                    OnPropertyChanged(nameof(FuelReserveValid));
                }
            }
        }

        private float _extralLaps = 0.0f;
        private string _extraLapsString = "0";
        private bool _extraLapsValid = true;

        public float ExtraLaps
        {
            get { return _extralLaps; }
        }

        public string ExtraLapsString
        {
            get => _extraLapsString;
            set
            {
                if (_extraLapsString != value)
                {
                    _extraLapsString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _extralLaps = result;
                        ExtraLapsValid = true;
                    }
                    else
                    {
                        _extralLaps = 0;
                        ExtraLapsValid = false;
                    }

                    OnPropertyChanged(nameof(ExtraLaps));
                    OnPropertyChanged(nameof(ExtraLapsString));
                }
            }
        }
        public bool ExtraLapsValid
        {
            get => _extraLapsValid;
            private set
            {
                if (_extraLapsValid != value)
                {
                    _extraLapsValid = value;
                    OnPropertyChanged(nameof(ExtraLapsValid));
                }
            }
        }

        private float _extraConsumptionPct = 2.0f;
        private string _extraConsumptionPctString = "2.0";
        private bool _extraConsumptionPctValid = true;

        public float ExtraConsumptionPct
        {
            get { return _extraConsumptionPct; }
        }

        public string ExtraConsumptionPctString
        {
            get => _extraConsumptionPctString;
            set
            {
                if (_extraConsumptionPctString != value)
                {
                    _extraConsumptionPctString = value;

                    // Convert to float when set
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        _extraConsumptionPct = result;
                        ExtraConsumptionPctValid = true;
                    }
                    else
                    {
                        _extraConsumptionPct = 0;
                        ExtraConsumptionPctValid = false;
                    }

                    OnPropertyChanged(nameof(ExtraConsumptionPct));
                    OnPropertyChanged(nameof(ExtraConsumptionPctString));
                }
            }
        }
        public bool ExtraConsumptionPctValid
        {
            get => _extraConsumptionPctValid;
            private set
            {
                if (_extraConsumptionPctValid != value)
                {
                    _extraConsumptionPctValid = value;
                    OnPropertyChanged(nameof(ExtraConsumptionPctValid));
                }
            }
        }

        private bool _enablePreRaceWarning = true;

        public bool EnablePreRaceWarning
        {
            get { return _enablePreRaceWarning; }
            set
            {
                if (_enablePreRaceWarning != value)
                {
                    _enablePreRaceWarning = value;
                    OnPropertyChanged(nameof(EnablePreRaceWarning));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Settings class, make sure it can be correctly serialized using JSON.net
    /// </summary>
    public class PluginSettings : INotifyPropertyChanged
    {
        public bool CheckForUpdates { get; set; } = true;

        public StandingsSettings Standings { get; set; }
        public RelativeSettings Relative { get; set; }
        public TrackMapSettings TrackMap { get; set; }
        public DeltaSettings Delta { get; set; }
        public TelemetrySettings Telemetry { get; set; }
        public DashSettings Dash { get; set; }
        public LaunchAssistSettings LaunchAssist { get; set; }
        public SpotterSettings Spotter { get; set; }
        public RejoinHelperSettings RejoinHelper { get; set; }
        public BlindSpotMonitorSettings BlindSpotMonitor { get; set; }
        public FuelCalcSettings FuelCalc { get; set; }        

        public PluginSettings()
        {
            Standings = new StandingsSettings();
            Relative = new RelativeSettings();
            TrackMap = new TrackMapSettings();
            Delta = new DeltaSettings();
            Telemetry = new TelemetrySettings();
            Dash = new DashSettings();
            LaunchAssist = new LaunchAssistSettings();
            Spotter = new SpotterSettings();
            RejoinHelper = new RejoinHelperSettings();
            BlindSpotMonitor = new BlindSpotMonitorSettings();
            FuelCalc = new FuelCalcSettings();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}