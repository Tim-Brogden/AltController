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
using AltController.Sys;

namespace AltController.Actions
{
    /// <summary>
    /// Click, double-click, hold or release a mouse button
    /// </summary>
    public class ToggleMouseButtonAction : BaseAction
    {
        // Members
        private EMouseButton _mouseButton = EMouseButton.Left;        

        // Properties
        public EMouseButton MouseButton { get { return _mouseButton; } set { _mouseButton = value; Updated(); } }

        /// <summary>
        /// Return the type of action
        /// </summary>
        public override EActionType ActionType
        {
            get 
            {
                return EActionType.ToggleMouseButton;
            }
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                Utils utils = new Utils();
                return string.Format(Properties.Resources.String_Toggle_mouse_button_X, utils.GetMouseButtonName(_mouseButton));
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ToggleMouseButtonAction()
            : base()
        {
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _mouseButton = (EMouseButton)Enum.Parse(typeof(EMouseButton), element.GetAttribute("mousebutton"));

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);
            element.SetAttribute("mousebutton", _mouseButton.ToString());
        }

        /// <summary>
        /// Start the action
        /// </summary>
        public override void StartAction(IStateManager parent, AltControlEventArgs cargs)
        {
            parent.MouseStateManager.ToggleButtonState(_mouseButton);
            IsOngoing = false;
        }        
    }
}
