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
using AltController.Sys;

namespace AltController.Actions
{
    /// <summary>
    /// Click, double-click, hold or release a mouse button
    /// </summary>
    public class MouseButtonAction : BaseAction
    {
        // Members
        private EActionType _actionType = EActionType.MouseClick;
        private EMouseButton _mouseButton = EMouseButton.Left;
        private long _pressDurationTicks = System.Windows.Forms.SystemInformation.DoubleClickTime * TimeSpan.TicksPerMillisecond / 6;

        // State
        private bool _isPressed;
        private int _pressCount;
        private long _lastActionTimeTicks;

        // Properties
        public EMouseButton MouseButton { get { return _mouseButton; } set { _mouseButton = value; Updated(); } }

        /// <summary>
        /// Return the type of action
        /// </summary>
        public override EActionType ActionType
        {
            get 
            {
                return _actionType; 
            }
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                string verb = "";
                switch (_actionType)
                {
                    case EActionType.MouseClick:
                        verb = Properties.Resources.String_Click; break;
                    case EActionType.MouseDoubleClick:
                        verb = Properties.Resources.String_Double_click; break;
                    case EActionType.MouseHold:
                        verb = Properties.Resources.String_Hold; break;
                    case EActionType.MouseRelease:
                        verb = Properties.Resources.String_Release; break;
                    case EActionType.ToggleMouseButton:
                        verb = Properties.Resources.String_Toggle; break;
                }
                Utils utils = new Utils();
                // TODO: This causes unnatural word order with translated strings
                return string.Format("{0} '{1}' " + Properties.Resources.String_Mouse_Button.ToLower(), verb, utils.GetMouseButtonName(_mouseButton));
            }
        }

        /// <summary>
        /// Return a short name
        /// </summary>
        public override string ShortName
        {
            get
            {
                return Name.Replace(" mouse button", "");   // TODO: This doesn't work with translated strings
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MouseButtonAction()
            : base()
        {
        }

        public MouseButtonAction(EActionType actionType)
        {
            _actionType = actionType;
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            string buttonStr = element.GetAttribute("mousebutton");
            switch (buttonStr)
            {
                // Legacy numbering (upgrade to v1.5)
                case "0": _mouseButton = EMouseButton.Left; break;
                case "1": _mouseButton = EMouseButton.Middle; break;
                case "2": _mouseButton = EMouseButton.Right; break;
                case "3": _mouseButton = EMouseButton.X1; break;
                case "4": _mouseButton = EMouseButton.X2; break;
                default:
                    // Current
                    _mouseButton = (EMouseButton)Enum.Parse(typeof(EMouseButton), buttonStr);
                    break;
            }
            if (element.HasAttribute("actiontype"))
            {
                _actionType = (EActionType)Enum.Parse(typeof(EActionType), element.GetAttribute("actiontype"));
            }
            else
            {
                // Legacy code (upgrade to v2.0)
                bool pressOrRelease = bool.Parse(element.GetAttribute("pressorrelease"));
                long pressDurationTicks = long.Parse(element.GetAttribute("pressduration"), System.Globalization.CultureInfo.InvariantCulture);
                int numPressesRequired = int.Parse(element.GetAttribute("numpresses"), System.Globalization.CultureInfo.InvariantCulture);
                if (pressOrRelease)
                {
                    if (pressDurationTicks > 0L)
                    {
                        _actionType = (numPressesRequired == 1) ? EActionType.MouseClick : EActionType.MouseDoubleClick;
                    }
                    else
                    {
                        _actionType = EActionType.MouseHold;
                    }
                }
                else
                {
                    _actionType = EActionType.MouseRelease;
                }
            }

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);
            element.SetAttribute("actiontype", _actionType.ToString());
            element.SetAttribute("mousebutton", _mouseButton.ToString());
        }

        /// <summary>
        /// Start the action
        /// </summary>
        public override void StartAction(IStateManager parent, AltControlEventArgs cargs)
        {
            MouseManager mouseManager = parent.MouseStateManager;
            switch (_actionType) {
                case EActionType.MouseRelease:
                    // Release button task
                    mouseManager.SetButtonState(_mouseButton, false);
                    IsOngoing = false;
                    break;
                case EActionType.ToggleMouseButton:
                    mouseManager.ToggleButtonState(_mouseButton);
                    IsOngoing = false;
                    break;
                default:
                    {
                        // Press mouse button task (click, hold or double-click)
                        mouseManager.SetButtonState(_mouseButton, true);
                        _isPressed = true;
                        _pressCount = 1;
                        _lastActionTimeTicks = DateTime.Now.Ticks;
                        IsOngoing = _actionType != EActionType.MouseHold;
                    }
                    break;
            }
        }

        /// <summary>
        /// Continue the click or double-click action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void ContinueAction(IStateManager parent, AltControlEventArgs args)
        {
            long currentTimeTicks = DateTime.Now.Ticks;
            if (currentTimeTicks - _lastActionTimeTicks > _pressDurationTicks)
            {
                if (_isPressed)
                {
                    // Release  button
                    parent.MouseStateManager.SetButtonState(_mouseButton, false);
                    if (_actionType != EActionType.MouseDoubleClick || _pressCount > 1)
                    {
                        // Finished action
                        IsOngoing = false;
                    }
                }
                else
                {
                    // Press button again
                    parent.MouseStateManager.SetButtonState(_mouseButton, true);
                    _pressCount++;
                }

                _isPressed = !_isPressed;
                _lastActionTimeTicks = currentTimeTicks;
            }
        }

        /// <summary>
        /// Stop the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void StopAction(IStateManager parent)
        {
            if (_isPressed)
            {
                // Release the button before stopping
                parent.MouseStateManager.SetButtonState(_mouseButton, false); 
                _isPressed = false;
            }
            IsOngoing = false;
        }
    }
}
