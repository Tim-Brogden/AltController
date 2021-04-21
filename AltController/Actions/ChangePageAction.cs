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
    /// Action for changing logical page
    /// </summary>
    public class ChangePageAction : BaseAction
    {
        private long _pageID = Constants.DefaultID;
        private string _pageName = "";

        public long PageID { get { return _pageID; } set { _pageID = value; Updated(); } }
        public string PageName { get { return _pageName; } set { _pageName = value; Updated(); } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.ChangePage; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public ChangePageAction()
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
                return string.Format(Properties.Resources.String_Select_page + " '{0}'", _pageName);
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _pageID = long.Parse(element.GetAttribute("pageid"), System.Globalization.CultureInfo.InvariantCulture);
            _pageName = element.GetAttribute("pagename");

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("pageid", PageID.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("pagename", PageName);
        }

        /// <summary>
        /// Perform the action
        /// </summary>
        /// <param name="parent"></param>
        public override void StartAction(IStateManager parent, AltControlEventArgs cargs)
        {
            parent.SetPage(PageID);
        }
    }
}
