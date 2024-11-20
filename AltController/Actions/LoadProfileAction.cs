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
    /// Load a profile
    /// </summary>
    public class LoadProfileAction : BaseAction
    {
        // Fields
        private string _profileName = "";

        // Properties
        public string ProfileName { get { return _profileName; } set { _profileName = value; } }

        /// <summary>
        /// Type of action
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.LoadProfile; }
        }

        /// <summary>
        /// Action name
        /// </summary>
        public override string Name
        {
            get 
            {
                string name;
                if (_profileName != "")
                {
                    name = string.Format(Properties.Resources.String_LoadProfileX,  _profileName);
                }
                else
                {
                    name = Properties.Resources.String_LoadNewProfile; 
                }
                
                return name; 
            }
        }

        /// <summary>
        /// Action short name
        /// </summary>
        public override string ShortName
        {
            get
            {
                return _profileName != "" ? _profileName : Properties.Resources.String_DefaultProfileName;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public LoadProfileAction()
            : base()
        {
        }

        /// <summary>
        /// Load from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _profileName = element.GetAttribute("profile");

            base.FromXml(element);
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("profile", _profileName);
        }

        /// <summary>
        /// Start the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void StartAction(IStateManager parent, AltControlEventArgs args)
        {
            EventArgs lpargs = new LoadProfileEventArgs(_profileName);
            EventReport report = new EventReport(DateTime.Now, EEventType.LoadProfile, lpargs);
            parent.ReportEvent(report);
            IsOngoing = false;
        }
    }
}
