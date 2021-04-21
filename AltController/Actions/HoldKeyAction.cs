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
    /// Action for releasing a keyboard key
    /// </summary>
    public class HoldKeyAction : BaseKeyAction
    {
        // Configuration
        private long _pressDurationTicks = 0;

        // State
        private long _lastPressTimeTicks;

        public int PressDurationMS { get { return (int)(_pressDurationTicks / TimeSpan.TicksPerMillisecond); } set { _pressDurationTicks = value * TimeSpan.TicksPerMillisecond; Updated(); } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.HoldKey; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public HoldKeyAction()
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
                if (PressDurationMS > 0)
                {
                    name = string.Format(Properties.Resources.String_Hold_down_key_for_X, base.KeyName, PressDurationMS * 0.001);
                }
                else
                {
                    name = string.Format(Properties.Resources.String_Hold_down_key_X, base.KeyName);
                }
                return name;
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            if (element.HasAttribute("duration"))
            {
                _pressDurationTicks = long.Parse(element.GetAttribute("duration"), System.Globalization.CultureInfo.InvariantCulture);
            }

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
        }

        /// <summary>
        /// Perform the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void StartAction(IStateManager parent, AltControlEventArgs args)
        {
            // Press key
            parent.KeyStateManager.SetKeyState(VirtualKey, true);

            // Record time of press
            _lastPressTimeTicks = DateTime.Now.Ticks;

            IsOngoing = _pressDurationTicks > 0;
        }

        /// <summary>
        /// Continue the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void ContinueAction(IStateManager parent, AltControlEventArgs args)
        {
            if (DateTime.Now.Ticks - _lastPressTimeTicks > _pressDurationTicks)
            {
                // Release key
                parent.KeyStateManager.SetKeyState(VirtualKey, false);

                // Action complete
                IsOngoing = false;
            }
        }

        /// <summary>
        /// Stop the action
        /// </summary>
        /// <param name="parent"></param>
        public override void StopAction(IStateManager parent)
        {
            parent.KeyStateManager.SetKeyState(VirtualKey, false);
            IsOngoing = false;
        }
    }
}
