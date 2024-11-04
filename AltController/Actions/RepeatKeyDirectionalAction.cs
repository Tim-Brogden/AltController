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
using System.Globalization;
using System.Xml;
using System.Windows;
using AltController.Core;
using AltController.Event;
using AltController.Input;

namespace AltController.Actions
{
    /// <summary>
    /// Action for mapping pointer movements to key presses
    /// </summary>
    public class RepeatKeyDirectionalAction : BaseKeyAction
    {
        // Config
        private const long _defaultUpdateEveryTicks = 500L * TimeSpan.TicksPerMillisecond;
        private const long _minPressTimeTicks = 20L * TimeSpan.TicksPerMillisecond;
        private long _configUpdateEveryTicks = _defaultUpdateEveryTicks;
        private long _configTimeToMaxValueTicks = 0L;
        private long _configTimeToMinValueTicks = 0L;
        private double _configSensitivity = 1.0;
        private ELRUDState _longerPressesDirection = ELRUDState.Right;

        // Internal variables
        private long _lastPressTimeTicks;
        private bool _isPressed = false;
        private long _pressDurationTicks;
        private long _lastReleaseTimeTicks = DateTime.Now.Ticks;
        private double _currentPressAmount = 0.0;

        // Properties
        public int UpdateEveryMS { get { return (int)(_configUpdateEveryTicks / TimeSpan.TicksPerMillisecond); } set { _configUpdateEveryTicks = value * TimeSpan.TicksPerMillisecond; Updated(); } }
        public int TimeToMaxValueMS { get { return (int)(_configTimeToMaxValueTicks / TimeSpan.TicksPerMillisecond); } set { _configTimeToMaxValueTicks = value * TimeSpan.TicksPerMillisecond; Updated(); } }
        public int TimeToMinValueMS { get { return (int)(_configTimeToMinValueTicks / TimeSpan.TicksPerMillisecond); } set { _configTimeToMinValueTicks = value * TimeSpan.TicksPerMillisecond; Updated(); } }
        public ELRUDState LongerPressesDirection { get { return _longerPressesDirection; } set { _longerPressesDirection = value; Updated(); } }
        public double Sensitivity { get { return _configSensitivity; } set { _configSensitivity = value; Updated(); } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.RepeatKeyDirectional; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RepeatKeyDirectionalAction()
            : base()
        {
            ParamDefinition[] requiredParams = new ParamDefinition[]
                                                { new ParamDefinition("X", EDataType.Double),
                                                  new ParamDefinition("Y", EDataType.Double) };
            SetRequiredParameters(requiredParams);
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                string longerPressesText = "";
                switch (_longerPressesDirection)
                {
                    case ELRUDState.Left:
                        longerPressesText = Properties.Resources.String_Left; break;
                    case ELRUDState.Right:
                        longerPressesText = Properties.Resources.String_Right; break;
                    case ELRUDState.Up:
                        longerPressesText = Properties.Resources.String_Top; break;
                    case ELRUDState.Down:
                        longerPressesText = Properties.Resources.String_Bottom; break;
                }
                return string.Format(Properties.Resources.String_Repeat_key + " '{0}', " + Properties.Resources.String_press_every + " {1}s{2}, " + Properties.Resources.String_sensitivity + " {3}{4}{5}",
                                                base.KeyName,
                                                UpdateEveryMS * 0.001,
                                                longerPressesText != "" ? ", " + Properties.Resources.String_longer_presses_towards + " " + longerPressesText.ToLower() : "",
                                                _configSensitivity.ToString("0.00"),
                                                TimeToMaxValueMS > 0 ? ", " + (TimeToMaxValueMS * 0.001).ToString("0.00") + "s " + Properties.Resources.String_to_reach_max : "",
                                                TimeToMinValueMS > 0 ? ", " + (TimeToMinValueMS * 0.001).ToString("0.00") + "s " + Properties.Resources.String_to_auto_cancel : "");
            }
        }
        public override string ShortName
        {
            get
            {
                return string.Format(Properties.Resources.String_Repeat_key + " '{0}'", base.KeyName);
            }
        }

        /// <summary>
        /// Validate when settings change
        /// </summary>
        protected override void Updated()
        {
            base.Updated();

            // Validate settings
            if (_configUpdateEveryTicks <= 0L)
            {
                _configUpdateEveryTicks = _defaultUpdateEveryTicks;
            }
            if (_configSensitivity <= 0.0)
            {
                _configSensitivity = 1.0;
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _configUpdateEveryTicks = long.Parse(element.GetAttribute("updateevery"), CultureInfo.InvariantCulture);
            if (element.HasAttribute("timetomax"))
            {
                _configTimeToMaxValueTicks = long.Parse(element.GetAttribute("timetomax"), CultureInfo.InvariantCulture);
            }
            if (element.HasAttribute("timetomin"))
            {
                _configTimeToMinValueTicks = long.Parse(element.GetAttribute("timetomin"), CultureInfo.InvariantCulture);
            }
            _configSensitivity = double.Parse(element.GetAttribute("sensitivity"), CultureInfo.InvariantCulture);
            _longerPressesDirection = (ELRUDState)Enum.Parse(typeof(ELRUDState), element.GetAttribute("direction"));

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("updateevery", _configUpdateEveryTicks.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("timetomax", _configTimeToMaxValueTicks.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("timetomin", _configTimeToMinValueTicks.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("sensitivity", _configSensitivity.ToString("0.00", CultureInfo.InvariantCulture));
            element.SetAttribute("direction", _longerPressesDirection.ToString());
        }

        /// <summary>
        /// Perform the action
        /// </summary>
        public override void StartAction(IStateManager parent, AltControlEventArgs cargs)
        {
            // Check that the pointer is still inside the region
            MouseSource source = (MouseSource)parent.CurrentProfile.GetInputSource(ESourceType.Mouse);
            if (source == null || !source.IsPointerInRegion(cargs.Data))
            {
                StopAction(parent);
                return;
            }

            long currentTime = DateTime.Now.Ticks;
            IsOngoing = true;

            // See if it's time to release the key
            if (_isPressed && currentTime - _lastPressTimeTicks > _pressDurationTicks)
            {
                parent.KeyStateManager.SetKeyState(VirtualKey, false);
                _isPressed = false;
            }
            
            // See if it's time to press the key
            if (currentTime - _lastPressTimeTicks > _configUpdateEveryTicks)
            {
                _lastPressTimeTicks = currentTime;

                // Get normalised pointer position
                ParamValue xValue = GetParamValue(0, cargs);
                ParamValue yValue = GetParamValue(1, cargs);
                if (xValue.Value == null || yValue.Value == null) return;  // Shouldn't ever happen
                Point normalisedPos = new Point((double)xValue.Value, (double)yValue.Value);
                
                // Get reqd press time
                double targetAmount = GetRequiredPressAmount(normalisedPos);

                // Legacy: Support old time to max and time to auto cancel settings from around v1.31
                if (_configTimeToMinValueTicks > 0L)
                {
                    // Decay the current press amount if applicable
                    _currentPressAmount -= (currentTime - _lastReleaseTimeTicks) / (double)_configTimeToMinValueTicks;
                    _currentPressAmount = Math.Max(0.0, _currentPressAmount);

                    // Calc proportion of the repeat interval to press for
                    if (_configTimeToMaxValueTicks > 0L)
                    {
                        // Calculate the press amount required to get to the target value
                        // and the remaining time if that takes less than the update period
                        long remainingTimeTicks;
                        if (targetAmount > _currentPressAmount)
                        {
                            // Press the key until we reach the target value
                            _pressDurationTicks = Math.Min(_configUpdateEveryTicks, (long)((targetAmount - _currentPressAmount) * _configTimeToMaxValueTicks));
                            remainingTimeTicks = _configUpdateEveryTicks - _pressDurationTicks;
                        }
                        else
                        {
                            // Release the key until we reach the target value
                            _pressDurationTicks = 0L;
                            remainingTimeTicks = Math.Min(_configUpdateEveryTicks, (long)((_currentPressAmount - targetAmount) * _configTimeToMinValueTicks));
                        }

                        if (remainingTimeTicks > 0L)
                        {
                            // Adjust for decay during times the key isn't pressed after the target value is reached

                            // Default adjustment is equal to non-pressing time shared proportionately between press and non-press!
                            long defaultDecayTicks = (long)(remainingTimeTicks * (_configTimeToMinValueTicks / (double)(_configTimeToMaxValueTicks + _configTimeToMinValueTicks)));
                            long adjustmentTicks;
                            if (defaultDecayTicks > targetAmount * _configTimeToMinValueTicks)
                            {
                                // Can't decay by more than target amount (decay stops when zero is reached)
                                adjustmentTicks = (long)(targetAmount * _configTimeToMaxValueTicks);
                            }
                            else if (defaultDecayTicks > _configTimeToMinValueTicks * (1.0 - targetAmount))
                            {
                                // Can't decay by more than 1.0 - target amount (max value is 1.0)
                                adjustmentTicks = remainingTimeTicks - (long)(_configTimeToMinValueTicks * (1.0 - targetAmount));
                            }
                            else
                            {
                                adjustmentTicks = remainingTimeTicks - defaultDecayTicks;
                            }
                            _pressDurationTicks += adjustmentTicks;
                        }
                    }
                }
                else
                {
                    _currentPressAmount = 0.0;
                    _pressDurationTicks = (long)(_configUpdateEveryTicks * targetAmount);
                }

                // If release is due to be less than min press time before next update, don't bother releasing
                //if (_configUpdateEveryTicks - _pressDurationTicks < _minPressTimeTicks)
                //{
                //    _pressDurationTicks = _configUpdateEveryTicks;
                //}

                // Don't press if already pressed or the duration is very short
                if (!_isPressed && _pressDurationTicks > _minPressTimeTicks)
                {
                    // Legacy: Support old time to max and time to auto cancel settings from around v1.31
                    // Update the current press amount for the time at which the press will finish
                    // I.e. only decay press amount while not pressed
                    if (_configTimeToMaxValueTicks > 0L)
                    {
                        _currentPressAmount += _pressDurationTicks / (double)_configTimeToMaxValueTicks;
                        _currentPressAmount = Math.Min(1.0, _currentPressAmount);
                    }
                    else
                    {
                        _currentPressAmount = 1.0;
                    }
                    _lastReleaseTimeTicks = currentTime + _pressDurationTicks;

                    // Press key
                    parent.KeyStateManager.SetKeyState(VirtualKey, true);
                    _lastPressTimeTicks = currentTime;
                    _isPressed = true;
                }
            }
        }

        /// <summary>
        /// Continue the action so that the key eventually gets released automatically
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void ContinueAction(IStateManager parent, AltControlEventArgs args)
        {
            this.StartAction(parent, args);
        }

        /// <summary>
        /// Stop the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void StopAction(IStateManager parent)
        {
            if (_isPressed)
            {
                // Release the key before stopping
                parent.KeyStateManager.SetKeyState(VirtualKey, false);
                _isPressed = false;
            }
            IsOngoing = false;
        }

        /// <summary>
        /// Get the press required according to the orientation of the action and the pointer position
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private double GetRequiredPressAmount(Point point)
        {
            double amount;

            // Scale the amount according to the orientation of the action, if any
            switch (_longerPressesDirection)
            {
                case ELRUDState.Left:
                    // Max at left
                    amount = 1.0 - point.X;
                    break;
                case ELRUDState.Right:
                    // Max at right
                    amount = point.X;
                    break;
                case ELRUDState.Up:
                    // Max at top
                    amount = 1.0 - point.Y;
                    break;
                case ELRUDState.Down:
                    // Max at bottom
                    amount = point.Y;
                    break;
                case ELRUDState.None:
                default:
                    // Uniform
                    amount = 0.5;
                    break;
            }

            // Amplify by the sensitivity
            amount = Math.Max(0.0, Math.Min(amount * _configSensitivity, 1.0));

            return amount;
        }

    }
}
