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
    /// Data describing a program defined in the Apps section of the Edit Situations window.
    /// </summary>
    public class AppItem : NamedItem
    {
        // Members
        private bool _snooze = false;

        // Properties
        public bool Snooze 
        { 
            get { return _snooze; }
            set { 
                _snooze = value;
                NotifyPropertyChanged("LongName");
            }
        }

        public override string Name
        {
            get { return base.Name; }
            set
            {
                base.Name = value;
                NotifyPropertyChanged("LongName");
            }
        }

        public string LongName
        {
            get
            {
                string name = Name;
                if (_snooze)
                {
                    name += " (" + Properties.Resources.String_snooze + ")";
                }
                return name;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AppItem()
            :base()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="snooze"></param>
        public AppItem(long id,
            string name,
            bool snooze = false)
            : base(id, name)
        {
            _snooze = snooze;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="item"></param>
        public AppItem(AppItem item)
            :base(item)
        {
            _snooze = item._snooze;
        }

        /// <summary>
        /// Read from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            // v1.94
            if (element.HasAttribute("snooze"))
            {
                _snooze = bool.Parse(element.GetAttribute("snooze"));
            }
            // End v1.94
        }

        /// <summary>
        /// Write to xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("snooze", _snooze.ToString());
        }

    }

}
