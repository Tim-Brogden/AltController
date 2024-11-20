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
using AltController.Actions;
using AltController.Core;

namespace AltController.Event
{
    /// <summary>
    /// Event data for a start process action
    /// </summary>
    class StartProgramEventArgs : EventArgs
    {
        // Fields
        public StartProgramAction Action;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action"></param>
        public StartProgramEventArgs(StartProgramAction action)
            : base()
        {
            Action = action;
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Action != null ? Action.Name : "";
        }
    }
}
