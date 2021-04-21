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
using System.Xml;
using AltController.Core;
using AltController.Event;

namespace AltController.Actions
{
    /// <summary>
    /// Action for scrolling up or down
    /// </summary>
    public class ScrollWheelAction : BaseAction
    {
        // Configuration
        private bool _isScrollUp;

        public bool IsUp { get { return _isScrollUp; } set { _isScrollUp = value; Updated(); } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return _isScrollUp ? EActionType.ScrollUp : EActionType.ScrollDown; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ScrollWheelAction()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public ScrollWheelAction(bool isScrollUp)
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
                return string.Format(Properties.Resources.String_Scroll_X, direction);
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _isScrollUp = bool.Parse(element.GetAttribute("isup"));

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
        }
    }
}
