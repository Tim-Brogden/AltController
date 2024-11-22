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
    /// Show or minimise custom window(s)
    /// </summary>
    public class CustomWindowStateAction : BaseAction
    {
        // Fields
        private long _windowID = Constants.DefaultID;
        private string _windowTitle = "";
        private EWindowState _windowState = EWindowState.Minimise | EWindowState.Normal;

        // Properties
        public long WindowID { get { return _windowID; } set { _windowID = value; } }
        public string WindowTitle { get { return _windowTitle; } set { _windowTitle = value; } }
        public EWindowState WindowState { get { return _windowState; } set { _windowState = value; } }

        /// <summary>
        /// Type of action
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.CustomWindowState; }
        }

        /// <summary>
        /// Action name
        /// </summary>
        public override string Name
        {
            get 
            {
                string name = Properties.Resources.String_None;
                switch (_windowID)
                {
                    case Constants.NoneID:
                        name = Properties.Resources.String_ShowOrHideCustomWindow; break;
                    case Constants.DefaultID:
                        switch (_windowState)
                        {
                            case EWindowState.Minimise:
                                name = Properties.Resources.String_HideAllCustomWindows; break;
                            case EWindowState.Normal:
                                name = Properties.Resources.String_ShowAllCustomWindows; break;
                            case EWindowState.Minimise | EWindowState.Normal:
                                name = Properties.Resources.String_ShowOrHideAllCustomWindows; break;
                        }
                        break;
                    default:
                        switch (_windowState)
                        {
                            case EWindowState.Minimise:
                                name = string.Format(Properties.Resources.String_HideCustomWindowX, _windowTitle); break;
                            case EWindowState.Normal:
                                name = string.Format(Properties.Resources.String_ShowCustomWindowX, _windowTitle); break;
                            case EWindowState.Minimise | EWindowState.Normal:
                                name = string.Format(Properties.Resources.String_ShowOrHideCustomWindowX, _windowTitle); break;
                        }
                        break;
                }
                return name;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomWindowStateAction()
            : base()
        {
        }

        /// <summary>
        /// Load from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _windowID = long.Parse(element.GetAttribute("windowid"), System.Globalization.CultureInfo.InvariantCulture);
            _windowState = (EWindowState)Enum.Parse(typeof(EWindowState), element.GetAttribute("state"));
            _windowTitle = element.GetAttribute("windowtitle");

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

            element.SetAttribute("windowid", _windowID.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("state", _windowState.ToString());
            element.SetAttribute("windowtitle", _windowTitle);
        }

        /// <summary>
        /// Start the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void StartAction(IStateManager parent, AltControlEventArgs args)
        {
            if (_windowID != Constants.NoneID)
            {
                EventArgs wargs = new CustomWindowEventArgs(_windowID, _windowState);
                EventReport report = new EventReport(DateTime.Now, EEventType.ShowOrHideCustomWindow, wargs);
                parent.ReportEvent(report);
            }
            IsOngoing = false;
        }
    }
}
