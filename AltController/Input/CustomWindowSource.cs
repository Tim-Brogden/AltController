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
using System.Collections.Generic;
using System.Xml;
using AltController.Core;
using AltController.Controls;
using AltController.Event;

namespace AltController.Input
{
    /// <summary>
    /// Custom window input source
    /// </summary>
    public class CustomWindowSource : BaseSource
    {
        private string _windowTitle;
        private double _windowWidthPx = Constants.DefaultCustomWindowWidthPx;
        private double _windowHeightPx = Constants.DefaultCustomWindowHeightPx;
        private string _backgroundColour = "";
        private bool _topMost = Constants.DefaultIsTopMost;
        private bool _ghostBackground = Constants.DefaultIsGhostBackground;
        private double _translucency = Constants.DefaultCustomWindowTranslucency;
        private CustomWindowButtonSet _customButtonsControl;

        /// <summary>
        /// Get the type of input source
        /// </summary>
        public override ESourceType SourceType
        {
            get { return ESourceType.CustomWindow; }
        }
        public string WindowTitle { get { return _windowTitle; } set { _windowTitle = value; } }
        public double WindowWidth { get { return _windowWidthPx; } set { _windowWidthPx = value; } }
        public double WindowHeight { get { return _windowHeightPx; } set { _windowHeightPx = value; } }
        public string BackgroundColour { get { return _backgroundColour; } set { _backgroundColour = value; } }
        public bool TopMost { get { return _topMost; } set { _topMost = value; } }
        public bool GhostBackground { get { return _ghostBackground; } set { _ghostBackground = value; } }
        public double Translucency { get { return _translucency; } set { _translucency = value; } }
        public NamedItemList CustomButtons { get { return _customButtonsControl.CustomButtons; } set { _customButtonsControl.CustomButtons = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomWindowSource()
            :base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public CustomWindowSource(long id, string name)
            : base(id, name)
        {
            _windowTitle = name;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <returns></returns>
        public CustomWindowSource(CustomWindowSource source)
            :base(source)
        {
            _windowTitle = source._windowTitle;
            _windowWidthPx = source._windowWidthPx;
            _windowHeightPx = source._windowHeightPx;
            _backgroundColour = source._backgroundColour;
            _topMost = source._topMost;
            _ghostBackground = source._ghostBackground;
            _translucency = source._translucency;

            foreach (CustomButtonData button in source.CustomButtons)
            {
                _customButtonsControl.CustomButtons.Add(new CustomButtonData(button));
            }
        }

        /// <summary>
        /// Initialise controls
        /// </summary>
        protected override void InitialiseControls()
        {
            _customButtonsControl = new CustomWindowButtonSet(this);
            InputControls = new BaseControl[] { _customButtonsControl };
        }

        /// <summary>
        /// Get the types of event that this source supports for the specified control
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override List<EEventReason> GetSupportedEventReasons(AltControlEventArgs args)
        {
            return _customButtonsControl.SupportedEventReasons;
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            _windowWidthPx = double.Parse(element.GetAttribute("windowwidthpx"), System.Globalization.CultureInfo.InvariantCulture);
            _windowHeightPx = double.Parse(element.GetAttribute("windowheightpx"), System.Globalization.CultureInfo.InvariantCulture);
            _windowTitle = element.GetAttribute("windowtitle");
            _backgroundColour = element.GetAttribute("backgroundcolour");
            _topMost = bool.Parse(element.GetAttribute("topmost"));
            _ghostBackground = bool.Parse(element.GetAttribute("ghostbackground"));
            _translucency = double.Parse(element.GetAttribute("translucency"), System.Globalization.CultureInfo.InvariantCulture);

            // Read button data
            XmlNodeList buttonElements = element.SelectNodes("buttons/button");
            foreach (XmlElement buttonElement in buttonElements)
            {
                CustomButtonData buttonData = new CustomButtonData();
                buttonData.FromXml(buttonElement);
                CustomButtons.Add(buttonData);
            }
        }

        /// <summary>
        /// Write the source config to an xml node
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("windowwidthpx", _windowWidthPx.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("windowheightpx", _windowHeightPx.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("windowtitle", _windowTitle);
            element.SetAttribute("backgroundcolour", _backgroundColour);
            element.SetAttribute("topmost", _topMost.ToString());
            element.SetAttribute("ghostbackground", _ghostBackground.ToString());
            element.SetAttribute("translucency", _translucency.ToString(System.Globalization.CultureInfo.InvariantCulture));

            // Write button data
            XmlElement buttonsElement = doc.CreateElement("buttons");
            foreach (CustomButtonData buttonData in this.CustomButtons)
            {
                XmlElement buttonElement = doc.CreateElement("button");
                buttonData.ToXml(buttonElement, doc);
                buttonsElement.AppendChild(buttonElement);
            }
            element.AppendChild(buttonsElement);
        }

        /// <summary>
        /// Enable or disable monitoring for a type of event
        /// </summary>
        /// <param name="args"></param>
        public override void ConfigureEventMonitoring(AltControlEventArgs args, bool enable)
        {
            _customButtonsControl.ConfigureEventMonitoring(args, enable);
        }

        /// <summary>
        /// Handle an external event generated by a custom window
        /// </summary>
        /// <param name="args"></param>
        public override void ReceiveExternalEvent(AltControlEventArgs args)
        {
            // Check source is active and the event type is for this source            
            _customButtonsControl.ReceiveExternalEvent(args);               
        }

        /// <summary>
        /// Update the button states if the control is active
        /// </summary>
        /// <param name="stateManager"></param>
        public override void UpdateState(IStateManager stateManager)
        {            
            _customButtonsControl.UpdateState(stateManager);
        }
    }
}
