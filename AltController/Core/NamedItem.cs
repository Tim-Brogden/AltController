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
using System.ComponentModel;
using System.Xml;

namespace AltController.Core
{
    /// <summary>
    /// Stores an item with an ID and name
    /// </summary>
    public class NamedItem : INotifyPropertyChanged
    {
        // Members
        private long _id = Constants.DefaultID;
        private string _name = "";

        // Events
        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public long ID 
        { 
            get 
            { 
                return _id; 
            } 
            set 
            {
                if (_id != value)
                {
                    _id = value; 
                    NotifyPropertyChanged("ID");
                }
            } 
        }
        public virtual string Name 
        { 
            get 
            { 
                return _name; 
            } 
            set 
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            } 
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public NamedItem()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public NamedItem(long id, string name)
        {
            ID = id;
            Name = name;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="item"></param>
        public NamedItem(NamedItem item)
        {
            ID = item.ID;
            Name = item.Name;
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public virtual void FromXml(XmlElement element)
        {
            ID = long.Parse(element.Attributes["id"].Value, System.Globalization.CultureInfo.InvariantCulture);
            Name = element.Attributes["name"].Value;
        }

        /// <summary>
        /// Write out to xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public virtual void ToXml(XmlElement element, XmlDocument doc)
        {
            element.SetAttribute("id", ID.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("name", Name);
        }

        /// <summary>
        /// Notify change
        /// </summary>
        /// <param name="info"></param>
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }

}
