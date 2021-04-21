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
using System.Globalization;

namespace AltController.Core
{
    /// <summary>
    /// Data to describe a custom button in a custom window 
    /// </summary>
    public class CustomButtonData : NamedItem
    {
        // Members
        private string _text = "";
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private string _backgroundColour = "";
        private string _bgImage = "";
        private double _bgTranslucency = Constants.DefaultCustomButtonTranslucency;

        // Properties
        public string Text { get { return _text; } set { _text = value; } }
        public double X { get { return _x; } set { _x = value; } }
        public double Y { get { return _y; } set { _y = value; } }
        public double Width { get { return _width; } set { _width = value; } }
        public double Height { get { return _height; } set { _height = value; } }
        public string BackgroundColour { get { return _backgroundColour; } set { _backgroundColour = value; } }
        public string BackgroundImage { get { return _bgImage; } set { _bgImage = value; } }
        public double BackgroundTranslucency { get { return _bgTranslucency; } set { _bgTranslucency = value; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomButtonData()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public CustomButtonData(byte id, 
            string name, 
            string text, 
            double x, 
            double y, 
            double width, 
            double height, 
            string backgroundColour,
            string bgImage,
            double bgTranslucency
            )
            : base(id, name)
        {
            _text = text;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _backgroundColour = backgroundColour;
            _bgImage = bgImage;
            _bgTranslucency = bgTranslucency;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="button"></param>
        public CustomButtonData(CustomButtonData button)
        {
            ID = button.ID;
            Name = button.Name;
            _text = button.Text;
            _x = button.X;
            _y = button.Y;
            _width = button.Width;
            _height = button.Height;
            _backgroundColour = button.BackgroundColour;
            _bgImage = button.BackgroundImage;
            _bgTranslucency = button.BackgroundTranslucency;
        }

        /// <summary>
        /// Read from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            _text = element.GetAttribute("text");
            _x = double.Parse(element.GetAttribute("x"), System.Globalization.CultureInfo.InvariantCulture);
            _y = double.Parse(element.GetAttribute("y"), System.Globalization.CultureInfo.InvariantCulture);
            _width = double.Parse(element.GetAttribute("width"), System.Globalization.CultureInfo.InvariantCulture);
            _height = double.Parse(element.GetAttribute("height"), System.Globalization.CultureInfo.InvariantCulture);
            _backgroundColour = element.GetAttribute("backgroundcolour");
            if (element.HasAttribute("bgimage"))
            {
                _bgImage = element.GetAttribute("bgimage");
            }
            if (element.HasAttribute("translucency"))
            {
                _bgTranslucency = double.Parse(element.GetAttribute("translucency"), CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Write to xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("text", _text);
            element.SetAttribute("x", _x.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("y", _y.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("width", _width.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("height", _height.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("backgroundcolour", _backgroundColour);
            element.SetAttribute("bgimage", _bgImage);
            element.SetAttribute("translucency", _bgTranslucency.ToString(CultureInfo.InvariantCulture));
        }
    }
}
