/*
Alt Controller
--------------
Copyright 2013 Tim Brogden
http://altcontroller.net

Description
-----------
A free program for mapping computer inputs, such as pointer movements and button presses, 
to actions, such as key presses. The aim of this program is to help make third-party programs,
such as computer games, more accessible to users with physical difficulties.

License
-------
This file is part of Alt Controller. 
Alt Controller is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Alt Controller is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Alt Controller.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AltController.Core;
using AltController.Event;

namespace AltController.UserControls
{
    /// <summary>
    /// Interaction logic for CustomSliderControl.xaml
    /// </summary>
    public partial class CustomSliderControl : UserControl
    {
        // Members
        bool _isLoaded = false;
        private double _minimum = 0.0;
        private double _maximum = 1.0;
        private double _currentVal = 0.0;
        private double _smallChange = 0.02;
        private double _largeChange = 0.1;
        private bool _isLogScale = false;
        private double _logBase = 5;
        private int _decimalPlaces = 3;

        // Properties
        public double Value 
        { 
            get { return _currentVal; } 
            set 
            {
                double val = Math.Max(_minimum, Math.Min(_maximum, value));
                val = Math.Round(val, _decimalPlaces);
                if (val != _currentVal)
                {
                    _currentVal = val;
                    if (_isLoaded)
                    {
                        this.ValueTextBox.Text = _currentVal.ToString();
                        SetSliderVal(this.ValueSlider, _currentVal);
                    }
                    RaiseEventIfRequired();
                }
            } 
        }
        public double Minimum 
        { 
            get { return _minimum; } 
            set 
            { 
                _minimum = value;
                if (_isLoaded)
                {
                    if (_isLogScale)
                    {
                        this.ValueSlider.Minimum = ConvertToLogBase(_minimum);
                    }
                    else
                    {
                        this.ValueSlider.Minimum = _minimum;
                    }
                }
            } 
        }
        public double Maximum 
        { 
            get { return _maximum; } 
            set 
            { 
                _maximum = value;
                if (_isLoaded)
                {
                    if (_isLogScale)
                    {
                        this.ValueSlider.Maximum = ConvertToLogBase(_maximum);
                    }
                    else
                    {
                        this.ValueSlider.Maximum = _maximum;
                    }
                }
            } 
        }
        public double SmallChange
        {
            get { return _smallChange; }
            set
            {
                _smallChange = value;
                if (_isLoaded)
                {
                    this.ValueSlider.SmallChange = value;
                }
            }
        }
        public double LargeChange 
        { 
            get { return _largeChange; } 
            set 
            { 
                _largeChange = value;
                if (_isLoaded)
                {
                    this.ValueSlider.LargeChange = value;
                }
            } 
        }
        public bool IsLogScale { get { return _isLogScale; } set { _isLogScale = value; } }
        public double LogBase { get { return _logBase; } set { _logBase = value; } }
        public int DecimalPlaces { get { return _decimalPlaces; } set { _decimalPlaces = value; } }

        // Events
        public event DoubleValChangedEventHandler ValueChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomSliderControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Slider changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoaded)
            {
                // Convert slider value to actual value
                double newVal;
                if (_isLogScale)
                {
                    newVal = ConvertFromLogBase(this.ValueSlider.Value);
                }
                else
                {
                    newVal = this.ValueSlider.Value;
                }

                Value = newVal;
            }
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLogScale)
            {
                this.ValueSlider.Minimum = ConvertToLogBase(_minimum);
                this.ValueSlider.Maximum = ConvertToLogBase(_maximum);
            }
            else
            {
                this.ValueSlider.Minimum = _minimum;
                this.ValueSlider.Maximum = _maximum;
            }
            this.ValueSlider.SmallChange = _smallChange;
            this.ValueSlider.LargeChange = _largeChange;
            
            this.ValueTextBox.Text = _currentVal.ToString();
            SetSliderVal(this.ValueSlider, _currentVal);

            _isLoaded = true;
        }

        /// <summary>
        /// Set the value of a slider control
        /// </summary>
        /// <param name="slider"></param>
        /// <param name="val"></param>
        private void SetSliderVal(Slider slider, double val)
        {
            // Convert to log value if required
            if (_isLogScale)
            {
                slider.Value = ConvertToLogBase(val);
            }
            else
            {
                slider.Value = val;
            }
        }

        /// <summary>
        /// Convert to log scale
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private double ConvertToLogBase(double val)
        {
            return Math.Log(val, _logBase) + 1;
        }

        /// <summary>
        /// Convert from log scale
        /// </summary>
        /// <returns></returns>
        private double ConvertFromLogBase(double val)
        {
            return Math.Pow(_logBase, val - 1);
        }

        /// <summary>
        /// Report change in value
        /// </summary>
        private void RaiseEventIfRequired()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, new DoubleValEventArgs(_currentVal));
            }
        }

        // --------------- Changes by T. Kozlowski, Jan. 2018
        // Now changing the value in the text box triggers an update event immediately, without waiting until the keyboard focus is lost.
        // Now possible to adjust the values using the scrollwheel.

        /// <summary>
        /// Text changed
        /// </summary>
        /// <remarks>
        /// Don't call the Value setter if the parsed value is outside the accepted range. This would be annoying if the user 
        /// was in the middle of editing the value and their temporarily invalid value was automatically corrected.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            double val;
            if (double.TryParse(this.ValueTextBox.Text, out val)
                && val >= _minimum && val <= _maximum)
            {
                Value = val;
            }
        }

        /// <summary>
        /// Scrollwheel used 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Value += _smallChange * e.Delta / 120;      // Delta is in multiples of 120
        }

        /// <summary>
        /// Scrollwheel used 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Value += _smallChange * e.Delta / 120;      // Delta is in multiples of 120
        }

    }
}
