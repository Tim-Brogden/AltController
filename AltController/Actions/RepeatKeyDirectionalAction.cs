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
using System.Xml;
using System.Windows;
using AltController.Core;
using AltController.Event;

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
        private double _configSensitivity = 1.0;
        private ELRUDState _longerPressesDirection = ELRUDState.Right;

        // Internal variables
        private long _lastPressTimeTicks;
        private bool _isPressed = false;
        private long _pressDurationTicks;

        // Properties
        public int UpdateEveryMS { get { return (int)(_configUpdateEveryTicks / TimeSpan.TicksPerMillisecond); } set { _configUpdateEveryTicks = value * TimeSpan.TicksPerMillisecond; Updated(); } }
        public ELRUDState LongerPressesDirection { get { return _longerPressesDirection; } set { _longerPressesDirection = value; Updated(); } }
        public double Sensitivity { get { return _configSensitivity; } set { _configSensitivity = value; } }

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
                return string.Format(Properties.Resources.String_Repeat_key + " '{0}', " + Properties.Resources.String_press_every + " {1}s{2}, " + Properties.Resources.String_sensitivity + " {3}",
                                                base.KeyName,
                                                UpdateEveryMS * 0.001,
                                                longerPressesText != "" ? ", " + Properties.Resources.String_longer_presses_towards + " " + longerPressesText.ToLower() : "",
                                                _configSensitivity.ToString("0.00"));
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
            if (_configUpdateEveryTicks < 0L)
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
            _configUpdateEveryTicks = long.Parse(element.GetAttribute("updateevery"), System.Globalization.CultureInfo.InvariantCulture);
            _configSensitivity = double.Parse(element.GetAttribute("sensitivity"), System.Globalization.CultureInfo.InvariantCulture);
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

            element.SetAttribute("updateevery", _configUpdateEveryTicks.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("sensitivity", _configSensitivity.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("direction", _longerPressesDirection.ToString());
        }

        /// <summary>
        /// Perform the action
        /// </summary>
        public override void StartAction(IStateManager parent, AltControlEventArgs cargs)
        {
            long currentTime = DateTime.Now.Ticks;

            // See if time to press            
            if (currentTime - _lastPressTimeTicks > _configUpdateEveryTicks)
            {
                _lastPressTimeTicks = currentTime;

                // Get normalised pointer position
                ParamValue xValue = GetParamValue(0, cargs, parent);
                ParamValue yValue = GetParamValue(1, cargs, parent);
                if (xValue.Value == null || yValue.Value == null) return;  // Shouldn't ever happen
                Point normalisedPos = new Point((double)xValue.Value, (double)yValue.Value);
                
                // Get reqd press time
                double targetAmount = GetRequiredPressAmount(normalisedPos);                    
                _pressDurationTicks = (long)(_configUpdateEveryTicks * targetAmount);

                // If release is due to be less than min press time before next update, don't bother releasing
                if (_configUpdateEveryTicks - _pressDurationTicks < _minPressTimeTicks)
                {
                    _pressDurationTicks = _configUpdateEveryTicks;
                }

                // Don't press if already pressed or the duration is very short
                if (!_isPressed && _pressDurationTicks > _minPressTimeTicks)
                {
                    // Press key
                    parent.KeyStateManager.SetKeyState(VirtualKey, true);
                    _isPressed = true;
                    IsOngoing = true;
                }
            }
            else if (_isPressed && currentTime - _lastPressTimeTicks > _pressDurationTicks)
            {
                // Time to release the key
                parent.KeyStateManager.SetKeyState(VirtualKey, false);
                _isPressed = false;
                IsOngoing = false;
            }
        }

        /// <summary>
        /// Continue the action so that the key eventually gets released automatically
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void ContinueAction(IStateManager parent, AltControlEventArgs args)
        {
            if (_isPressed)
            {
                if (DateTime.Now.Ticks - _lastPressTimeTicks > _pressDurationTicks)
                {
                    // Release the key
                    parent.KeyStateManager.SetKeyState(VirtualKey, false);
                    _isPressed = false;
                    IsOngoing = false;
                }
            }
            else
            {
                IsOngoing = false;
            }
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
            double amount = 0.0;

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
