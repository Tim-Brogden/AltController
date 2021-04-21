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
using AltController.Core;

namespace AltController.Event
{
    /// <summary>
    /// Stores a reported application event
    /// </summary>
    public class EventReport
    {
        // Members
        private DateTime _timestamp;
        private EEventType _eventType;
        private EventArgs _args;

        // Properties
        public DateTime Timestamp { get { return _timestamp; } }
        public EEventType EventType { get { return _eventType; } }
        public EventArgs Args { get { return _args; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="args"></param>
        public EventReport(DateTime timestamp, EEventType eventType, EventArgs args)
        {
            _timestamp = timestamp;
            _eventType = eventType;
            _args = args;
        }
    }
}
