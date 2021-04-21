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
    /// Action for waiting for an amount of time, for use in a series of actions in an action list
    /// </summary>
    public class WaitAction : BaseAction
    {
        // Configuration
        private long _waitTimeTicks = 1000L * TimeSpan.TicksPerMillisecond;

        // State
        private long _startTimeTicks;

        // Properties
        public int WaitTimeMS { get { return (int)(_waitTimeTicks / TimeSpan.TicksPerMillisecond); } set { _waitTimeTicks = value * TimeSpan.TicksPerMillisecond; Updated(); } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.Wait; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public WaitAction()
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
                return string.Format(Properties.Resources.String_Wait_for_X, WaitTimeMS * 0.001);
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            _waitTimeTicks = long.Parse(element.GetAttribute("duration"), System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("duration", _waitTimeTicks.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Start the action
        /// </summary>
        public override void StartAction(IStateManager parent, AltControlEventArgs cargs)
        {
            _startTimeTicks = DateTime.Now.Ticks;

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
            if (currentTicks - _startTimeTicks > _waitTimeTicks)
            {
                // Finished waiting
                IsOngoing = false;
            }
        }
    }
}
