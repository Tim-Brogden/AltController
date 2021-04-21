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
using AltController.Core;
using AltController.Event;

namespace AltController.Actions
{
    /// <summary>
    /// Action for pressing a keyboard key, and possibly performing repeat presses
    /// </summary>
    public class RepeatKeyAction : BaseKeyAction
    {
        // Configuration
        private const long _minDurationTicks = 20L * TimeSpan.TicksPerMillisecond;
        private long _pressDurationTicks = 50L * TimeSpan.TicksPerMillisecond;
        private long _repeatEveryTicks = 500L * TimeSpan.TicksPerMillisecond;
        private long _stopAfterTicks = 0L;

        // State
        private long _lastPressTimeTicks;
        private long _stopTimeTicks;
        private bool _isPressed;

        public int PressDurationMS { get { return (int)(_pressDurationTicks / TimeSpan.TicksPerMillisecond); } set { _pressDurationTicks = value * TimeSpan.TicksPerMillisecond; Updated(); } }
        public int RepeatEveryMS { get { return (int)(_repeatEveryTicks / TimeSpan.TicksPerMillisecond); } set { _repeatEveryTicks = value * TimeSpan.TicksPerMillisecond; Updated(); } }
        public int StopAfterMS { get { return (int)(_stopAfterTicks / TimeSpan.TicksPerMillisecond); } set { _stopAfterTicks = value * TimeSpan.TicksPerMillisecond; Updated(); } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.RepeatKey; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RepeatKeyAction()
            : base()
        {
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                string name;
                name = string.Format(Properties.Resources.String_Press_key_for_X,
                                        base.KeyName,
                                        PressDurationMS * 0.001, 
                                        RepeatEveryMS * 0.001);
                if (StopAfterMS > 0)
                {
                    name += string.Format(", " + Properties.Resources.String_stop_after_X, StopAfterMS * 0.001);
                }

                return name;
            }
        }
        public override string ShortName
        {
            get
            {
                return string.Format(Properties.Resources.String_Repeat_key_X, base.KeyName);
            }
        }

        /// <summary>
        /// Validate data
        /// </summary>
        protected override void Updated()
        {
            base.Updated();

            _pressDurationTicks = Math.Max(_minDurationTicks, _pressDurationTicks);
            _repeatEveryTicks = Math.Max(_minDurationTicks, _repeatEveryTicks);
            _stopAfterTicks = Math.Max(0L, _stopAfterTicks);
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _pressDurationTicks = long.Parse(element.GetAttribute("duration"), System.Globalization.CultureInfo.InvariantCulture);
            _repeatEveryTicks = long.Parse(element.GetAttribute("repeatevery"), System.Globalization.CultureInfo.InvariantCulture);
            _stopAfterTicks = long.Parse(element.GetAttribute("stopafter"), System.Globalization.CultureInfo.InvariantCulture);

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("duration", _pressDurationTicks.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("repeatevery", _repeatEveryTicks.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("stopafter", _stopAfterTicks.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Start the action
        /// </summary>
        public override void StartAction(IStateManager parent, AltControlEventArgs cargs)
        {
            // Enable repeating
            parent.KeyStateManager.SetCanKeyRepeat(VirtualKey, true);

            // Press the key
            parent.KeyStateManager.SetKeyState(VirtualKey, true);

            // Initialise the state
            _lastPressTimeTicks = DateTime.Now.Ticks;
            _stopTimeTicks = _lastPressTimeTicks + _stopAfterTicks;
            _isPressed = true;

            IsOngoing = true;
        }

        /// <summary>
        /// Continue the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void ContinueAction(IStateManager parent, AltControlEventArgs args)
        {
            long currentTicks = DateTime.Now.Ticks;

            // Check that repeating hasn't been stopped by a "release key" action
            if (parent.KeyStateManager.GetCanKeyRepeat(VirtualKey))
            {
                if (_isPressed)
                {
                    if (currentTicks - _lastPressTimeTicks > _pressDurationTicks)
                    {
                        // Release the key
                        parent.KeyStateManager.SetKeyState(VirtualKey, false);
                        _isPressed = false;
                    }
                }
                else
                {
                    if (currentTicks - _lastPressTimeTicks > _repeatEveryTicks)
                    {
                        // Press the key again
                        parent.KeyStateManager.SetKeyState(VirtualKey, true);
                        _isPressed = true;
                        _lastPressTimeTicks = currentTicks;
                    }
                }

                // Decide whether and when to stop
                if (_stopAfterTicks > 0L && currentTicks > _stopTimeTicks)
                {
                    // Release the key before stopping
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
    }
}
