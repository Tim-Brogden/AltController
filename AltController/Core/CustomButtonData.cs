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
        private double _borderThickness = Constants.DefaultCustomButtonBorderThickness;
        private string _borderColour = Constants.DefaultCustomButtonBorderColour;
        private string _backgroundColour = "";
        private string _bgImage = "";
        private double _bgTranslucency = Constants.DefaultCustomButtonTranslucency;
        private string _fontName = "";
        private double _fontSize = Constants.DefaultCustomButtonFontSize;
        private string _textColour = Constants.DefaultCustomButtonTextColour;
        private ELRUDState _textAlignment = ELRUDState.None;

        // Properties
        public string Text { get { return _text; } set { _text = value; } }
        public double X { get { return _x; } set { _x = value; } }
        public double Y { get { return _y; } set { _y = value; } }
        public double Width { get { return _width; } set { _width = value; } }
        public double Height { get { return _height; } set { _height = value; } }
        public double BorderThickness { get { return _borderThickness; } set { _borderThickness = value; } }
        public string BorderColour { get { return _borderColour; } set { _borderColour = value; } }
        public string BackgroundColour { get { return _backgroundColour; } set { _backgroundColour = value; } }
        public string BackgroundImage { get { return _bgImage; } set { _bgImage = value; } }
        public double BackgroundTranslucency { get { return _bgTranslucency; } set { _bgTranslucency = value; } }
        public string FontName { get { return _fontName; } set { _fontName = value; } }
        public double FontSize { get { return _fontSize; } set { _fontSize = value; } }
        public string TextColour { get { return _textColour; } set { _textColour = value; } }
        public ELRUDState TextAlignment { get { return _textAlignment; } set { _textAlignment = value; } }

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
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="borderThickness"></param>
        /// <param name="borderColour"></param>
        /// <param name="backgroundColour"></param>
        /// <param name="bgImage"></param>
        /// <param name="bgTranslucency"></param>
        /// <param name="fontName"></param>
        /// <param name="fontSize"></param>
        /// <param name="textColour"></param>
        /// <param name="textAlignment"></param>
        public CustomButtonData(byte id,
            string name,
            string text,
            double x,
            double y,
            double width,
            double height,
            double borderThickness,
            string borderColour,
            string backgroundColour,
            string bgImage,
            double bgTranslucency,
            string fontName,
            double fontSize,
            string textColour,
            ELRUDState textAlignment)
            : base(id, name)
        {
            _text = text;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _borderThickness = borderThickness;
            _borderColour = borderColour;
            _backgroundColour = backgroundColour;
            _bgImage = bgImage;
            _bgTranslucency = bgTranslucency;
            _fontName = fontName;
            _fontSize = fontSize;
            _textColour = textColour;
            _textAlignment = textAlignment;
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
            _borderThickness = button.BorderThickness;
            _borderColour = button.BorderColour;
            _backgroundColour = button.BackgroundColour;
            _bgImage = button.BackgroundImage;
            _bgTranslucency = button.BackgroundTranslucency;
            _fontName = button.FontName;
            _fontSize = button.FontSize;
            _textColour = button.TextColour;
            _textAlignment = button.TextAlignment;
        }

        /// <summary>
        /// Read from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            _text = element.GetAttribute("text");
            _x = double.Parse(element.GetAttribute("x"), CultureInfo.InvariantCulture);
            _y = double.Parse(element.GetAttribute("y"), CultureInfo.InvariantCulture);
            _width = double.Parse(element.GetAttribute("width"), CultureInfo.InvariantCulture);
            _height = double.Parse(element.GetAttribute("height"), CultureInfo.InvariantCulture);
            _backgroundColour = element.GetAttribute("backgroundcolour");
            if (element.HasAttribute("bgimage"))
            {
                _bgImage = element.GetAttribute("bgimage");
            }
            if (element.HasAttribute("translucency"))
            {
                _bgTranslucency = double.Parse(element.GetAttribute("translucency"), CultureInfo.InvariantCulture);
            }
            // v1.92
            if (element.HasAttribute("borderthickness"))
            {
                _borderThickness = double.Parse(element.GetAttribute("borderthickness"), CultureInfo.InvariantCulture);
            }
            if (element.HasAttribute("bordercolour"))
            {
                _borderColour = element.GetAttribute("bordercolour");
            }
            if (element.HasAttribute("fontname"))
            {
                _fontName = element.GetAttribute("fontname");
            }
            if (element.HasAttribute("fontsize"))
            {
                _fontSize = double.Parse(element.GetAttribute("fontsize"), CultureInfo.InvariantCulture);
            }
            // End v1.92
            // v1.93
            if (element.HasAttribute("textcolour"))
            {
                _textColour = element.GetAttribute("textcolour");
            }
            // End v1.93
            // v2.0
            if (element.HasAttribute("textalignment"))
            {
                _textAlignment = (ELRUDState)Enum.Parse(typeof(ELRUDState), element.GetAttribute("textalignment"));
            }
            // End v2.0
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
            element.SetAttribute("x", _x.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("y", _y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("width", _width.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("height", _height.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("borderthickness", _borderThickness.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("bordercolour", _borderColour);
            element.SetAttribute("backgroundcolour", _backgroundColour);
            element.SetAttribute("bgimage", _bgImage);
            element.SetAttribute("translucency", _bgTranslucency.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("fontname", _fontName);
            element.SetAttribute("fontsize", _fontSize.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("textcolour", _textColour);
            element.SetAttribute("textalignment", _textAlignment.ToString());
        }
    }
}
