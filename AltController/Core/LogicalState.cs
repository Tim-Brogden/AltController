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

namespace AltController.Core
{
    /// <summary>
    /// Represents the logical state of the application
    /// </summary>
    public class LogicalState
    {
        private long _modeID = Constants.DefaultID;
        private long _appID = Constants.DefaultID;
        private long _pageID = Constants.DefaultID;

        public long ModeID { get { return _modeID; } set { _modeID = value; } }
        public long AppID { get { return _appID; } set { _appID = value; } }
        public long PageID { get { return _pageID; } set { _pageID = value; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LogicalState()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="state"></param>
        public LogicalState(LogicalState state)
        {
            _modeID = state.ModeID;
            _appID = state.AppID;
            _pageID = state.PageID;
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="modeID"></param>
        /// <param name="appID"></param>
        /// <param name="pageID"></param>
        public LogicalState(long modeID, long appID, long pageID)
        {
            _modeID = modeID;
            _appID = appID;
            _pageID = pageID;
        }

        /// <summary>
        /// Decide if this state is a superset of the specified set
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool Contains(LogicalState state)
        {
            bool result = (_modeID == Constants.DefaultID || _modeID == state.ModeID) &&
                            (_appID == Constants.DefaultID || _appID == state.AppID) &&
                            (_pageID == Constants.DefaultID || _pageID == state.PageID);
            return result;
        }

        /// <summary>
        /// Read from xml
        /// </summary>
        /// <param name="parentElement"></param>
        public void FromXml(XmlElement parentElement)
        {
            _modeID = long.Parse(parentElement.GetAttribute("mode"), System.Globalization.CultureInfo.InvariantCulture);
            _appID = long.Parse(parentElement.GetAttribute("app"), System.Globalization.CultureInfo.InvariantCulture);
            _pageID = long.Parse(parentElement.GetAttribute("page"), System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Write to xml
        /// </summary>
        /// <param name="parentElement"></param>
        public void ToXml(XmlElement parentElement, XmlDocument doc)
        {
            parentElement.SetAttribute("mode", _modeID.ToString(System.Globalization.CultureInfo.InvariantCulture));
            parentElement.SetAttribute("app", _appID.ToString(System.Globalization.CultureInfo.InvariantCulture));
            parentElement.SetAttribute("page", _pageID.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
