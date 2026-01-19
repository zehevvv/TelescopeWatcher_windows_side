using System;

namespace TelescopeWatcher
{
    /// <summary>
    /// Shared settings for telescope control that synchronizes values between forms
    /// </summary>
    public class TelescopeSettings
    {
        private static TelescopeSettings? _instance;
        private static readonly object _lock = new object();

        private int _stepsPerSecond = 100;
        private int _timeBetweenSteps = 10;
        private int _focusSpeed = 9;

        // Steps per second values corresponding to trackbar positions
        public readonly int[] StepsPerSecondValues = { 3, 10, 100, 1000, 10000 };

        public event EventHandler? StepsPerSecondChanged;
        public event EventHandler? FocusSpeedChanged;

        private TelescopeSettings()
        {
        }

        public static TelescopeSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new TelescopeSettings();
                        }
                    }
                }
                return _instance;
            }
        }

        public int StepsPerSecond
        {
            get => _stepsPerSecond;
            set
            {
                if (_stepsPerSecond != value)
                {
                    _stepsPerSecond = value;
                    CalculateTimeBetweenSteps();
                    StepsPerSecondChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int TimeBetweenSteps
        {
            get => _timeBetweenSteps;
            private set => _timeBetweenSteps = value;
        }

        public int FocusSpeed
        {
            get => _focusSpeed;
            set
            {
                if (_focusSpeed != value)
                {
                    _focusSpeed = value;
                    FocusSpeedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void CalculateTimeBetweenSteps()
        {
            double timeMs = 1000.0 / _stepsPerSecond;
            _timeBetweenSteps = (int)Math.Round(timeMs);

            // Special case for 10000 steps/second
            if (_stepsPerSecond == 10000)
            {
                _timeBetweenSteps = 0;
            }
        }

        /// <summary>
        /// Gets the trackbar index for the current steps per second value
        /// </summary>
        public int GetTrackbarIndexForStepsPerSecond()
        {
            for (int i = 0; i < StepsPerSecondValues.Length; i++)
            {
                if (StepsPerSecondValues[i] == _stepsPerSecond)
                {
                    return i;
                }
            }
            return 2; // Default to 100 steps/second
        }

        /// <summary>
        /// Sets steps per second from a trackbar index
        /// </summary>
        public void SetStepsPerSecondFromTrackbarIndex(int index)
        {
            if (index >= 0 && index < StepsPerSecondValues.Length)
            {
                StepsPerSecond = StepsPerSecondValues[index];
            }
        }
    }
}
