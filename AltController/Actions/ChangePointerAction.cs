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
    /// Tries to show the mouse pointer when it is hidden
    /// </summary>
    public class ChangePointerAction : BaseAction
    {
        // Members
        private ECursorType _cursorType = ECursorType.Standard;

        // Properties
        public ECursorType CursorType { get { return _cursorType; } set { _cursorType = value; Updated(); } }

        /// <summary>
        /// Type of action
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.ChangePointer; }
        }

        /// <summary>
        /// Name of action
        /// </summary>
        public override string Name
        {
            get
            {
                string name;
                switch (_cursorType)
                {
                    case ECursorType.Blank:
                        name = Properties.Resources.Action_HiddenRadioButton; break;
                    case ECursorType.User:
                        name = Properties.Resources.Action_RestoreUserRadioButton; break;
                    case ECursorType.Standard:
                    default:
                        name = Properties.Resources.Action_StandardRadioButton; break;
                }
                
                return name;
            }
        }

        /// <summary>
        /// Short name of action
        /// </summary>
        public override string ShortName
        {
            get
            {
                string name;
                switch (_cursorType)
                {
                    case ECursorType.Blank:
                        name = Properties.Resources.String_Hide_pointer; break;
                    case ECursorType.User:
                        name = Properties.Resources.String_Hide_pointer; break;
                    case ECursorType.Standard:
                    default:
                        name = Properties.Resources.String_Standard_pointer; break;
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
            _cursorType = (ECursorType)Enum.Parse(typeof(ECursorType), element.GetAttribute("changetype"));

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("changetype", _cursorType.ToString());
        }

        /// <summary>
        /// Start the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void StartAction(IStateManager parent, AltControlEventArgs args)
        {
            switch (_cursorType)
            {
                case ECursorType.Blank:
                    parent.MouseStateManager.ApplyBlankCursor(); break;
                case ECursorType.User:
                    parent.MouseStateManager.RestoreUsersCursor(); break;
                case ECursorType.Standard:
                default:
                    parent.MouseStateManager.ApplyStandardCursor(); break;
            } 
        }
    }
}
