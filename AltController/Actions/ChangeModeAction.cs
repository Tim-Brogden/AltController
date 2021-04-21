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
    /// Action for changing logical mode
    /// </summary>
    public class ChangeModeAction : BaseAction
    {
        private long _modeID = Constants.DefaultID;
        private string _modeName = "";

        public long ModeID { get { return _modeID; } set { _modeID = value; Updated(); } }
        public string ModeName { get { return _modeName; } set { _modeName = value; Updated(); } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.ChangeMode; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public ChangeModeAction()
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
                return string.Format(Properties.Resources.String_Select_mode + " '{0}'", _modeName);
           }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _modeID = long.Parse(element.GetAttribute("modeid"), System.Globalization.CultureInfo.InvariantCulture);
            _modeName = element.GetAttribute("modename");

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("modeid", ModeID.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("modename", ModeName);
        }

        /// <summary>
        /// Perform the action
        /// </summary>
        /// <param name="parent"></param>
        public override void StartAction(IStateManager parent, AltControlEventArgs cargs)
        {
            parent.SetMode(ModeID);
        }
    }
}
