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
using System.Collections.Generic;
using AltController.Core;
using AltController.Event;
using AltController.Controls;

namespace AltController.Input
{
    public class KeyboardSource : BaseSource
    {
        private KeyboardControl _keyboardControl;

        /// <summary>
        /// Get the type of input source
        /// </summary>
        public override ESourceType SourceType
        {
            get { return ESourceType.Keyboard; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public KeyboardSource()
            : base()
        {            
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public KeyboardSource(long id, string name)
            : base(id, name)
        {            
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public KeyboardSource(KeyboardSource source)
            :base(source)
        {
            InitialiseControls();
        }

        /// <summary>
        /// Initialise controls
        /// </summary>
        protected override void InitialiseControls()
        {
            _keyboardControl = new KeyboardControl(this);
            InputControls = new BaseControl[] { _keyboardControl };
        }

        /// <summary>
        /// Get the types of event that this source supports for the specified control
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override List<EEventReason> GetSupportedEventReasons(AltControlEventArgs args)
        {
            return _keyboardControl.SupportedEventReasons;
        }

        /// <summary>
        /// Update the keyboard state if the control is active
        /// </summary>
        /// <param name="stateManager"></param>
        public override void UpdateState(IStateManager stateManager)
        {
            _keyboardControl.UpdateState(stateManager);
        }
    }
}
