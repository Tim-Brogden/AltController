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

namespace AltController.Config
{
    public class ScreenRegionList : NamedItemList
    {
        // Members
        private string _refImage = "";
        private EDisplayArea _overlayPosition = Constants.DefaultOverlayArea;

        // Config
        public string RefImage { get { return _refImage; } set { _refImage = value; } }
        public EDisplayArea OverlayPosition { get { return _overlayPosition; } set { _overlayPosition = value; } }

        /// <summary>
        /// Read from xml
        /// </summary>
        /// <param name="element"></param>
        public void FromXml(XmlElement element)
        {
            // Reference image (screenshot)
            _refImage = element.GetAttribute("refimage");

            // Position of screen region overlays
            _overlayPosition = (EDisplayArea)Enum.Parse(typeof(EDisplayArea), element.GetAttribute("overlayposition"));

            // Screen regions
            Clear();
            XmlNodeList regionNodes = element.SelectNodes("region");
            foreach (XmlElement regionElement in regionNodes)
            {
                ScreenRegion region = new ScreenRegion();
                region.FromXml(regionElement);
                this.Add(region);
            }
        }

        /// <summary>
        /// Write to xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement element, XmlDocument doc)
        {
            // Reference image (screenshot)
            element.SetAttribute("refimage", _refImage);

            // Screen region overlay position
            element.SetAttribute("overlayposition", _overlayPosition.ToString());

            // Screen regions
            foreach (ScreenRegion region in this)
            {
                XmlElement regionElement = doc.CreateElement("region");
                region.ToXml(regionElement, doc);
                element.AppendChild(regionElement);
            }
        }
    }
}
