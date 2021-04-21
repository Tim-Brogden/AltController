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
    /// Action for repeatedly scrolling up or down
    /// </summary>
    public class RepeatScrollWheelAction : BaseAction
    {
        // Configuration
        private const long _minDurationTicks = 20L * TimeSpan.TicksPerMillisecond;
        private long _repeatEveryTicks = 500L * TimeSpan.TicksPerMillisecond;
        private long _stopAfterTicks = 0L;
        private bool _isScrollUp;

        // State
        private long _lastScrollTimeTicks;
        private long _stopTimeTicks;

        // Properties
        public int RepeatEveryMS { get { return (int)(_repeatEveryTicks / TimeSpan.TicksPerMillisecond); } set { _repeatEveryTicks = value * TimeSpan.TicksPerMillisecond; Updated(); } }
        public int StopAfterMS { get { return (int)(_stopAfterTicks / TimeSpan.TicksPerMillisecond); } set { _stopAfterTicks = value * TimeSpan.TicksPerMillisecond; Updated(); } }
        public bool IsUp { get { return _isScrollUp; } set { _isScrollUp = value; Updated(); } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get
            { 
                return _isScrollUp ? EActionType.RepeatScrollUp : EActionType.RepeatScrollDown; 
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RepeatScrollWheelAction()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public RepeatScrollWheelAction(bool isScrollUp)
            : base()
        {
            _isScrollUp = isScrollUp;
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                string direction = _isScrollUp ? Properties.Resources.String_up : Properties.Resources.String_down;
                string name = string.Format(Properties.Resources.String_Repeat_scroll_X,
                                        direction,
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
                string direction = _isScrollUp ? Properties.Resources.String_up : Properties.Resources.String_down;
                return string.Format(Properties.Resources.String_Scroll_repeatedly_X, direction);
            }
        }

        /// <summary>
        /// Validate data
        /// </summary>
        protected override void Updated()
        {
            base.Updated();

            _repeatEveryTicks = Math.Max(_minDurationTicks, _repeatEveryTicks);
            _stopAfterTicks = Math.Max(0L, _stopAfterTicks);
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _isScrollUp = bool.Parse(element.GetAttribute("isup"));
            _repeatEveryTicks = long.Parse(element.GetAttribute("repeatevery"), System.Globalization.CultureInfo.InvariantCulture);
            _stopAfterTicks = long.Parse(element.GetAttribute("stopafter"), System.Globalization.CultureInfo.InvariantCulture);

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("isup", _isScrollUp.ToString());
            element.SetAttribute("repeatevery", _repeatEveryTicks.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("stopafter", _stopAfterTicks.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
  
        /// <summary>
        /// Perform the action
        /// </summary>
        public override void StartAction(IStateManager parent, AltControlEventArgs cargs)
        {
            // Stop scrolling in the other direction
            parent.MouseStateManager.CanScrollUpRepeat = _isScrollUp;
            parent.MouseStateManager.CanScrollDownRepeat = !_isScrollUp;

            // Perform mouse scroll            
            parent.MouseStateManager.MouseScroll(_isScrollUp);

            // Initialise the state
            _lastScrollTimeTicks = DateTime.Now.Ticks;
            _stopTimeTicks = _lastScrollTimeTicks + _stopAfterTicks;

            IsOngoing = true;
        }

        /// <summary>
        /// Continue the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void ContinueAction(IStateManager parent, AltControlEventArgs args)
        {
            // Check that repeating hasn't been stopped by a "stop scrolling" action
            bool canScroll = _isScrollUp ? parent.MouseStateManager.CanScrollUpRepeat : parent.MouseStateManager.CanScrollDownRepeat;
            if (canScroll)
            {
                long currentTicks = DateTime.Now.Ticks;
                if (currentTicks - _lastScrollTimeTicks > _repeatEveryTicks)
                {
                    // Scroll again
                    parent.MouseStateManager.MouseScroll(_isScrollUp);
                    _lastScrollTimeTicks = currentTicks;
                }

                // Decide whether and when to stop
                if (_stopAfterTicks > 0L && currentTicks > _stopTimeTicks)
                {
                    IsOngoing = false;
                }
            }
            else
            {
                IsOngoing = false;
            }
        }
    }
}
