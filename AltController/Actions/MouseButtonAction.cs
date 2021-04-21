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
        private EMouseButton _mouseButton = EMouseButton.Left;
        private bool _pressOrRelease = true;
        private long _pressDurationTicks = System.Windows.Forms.SystemInformation.DoubleClickTime * TimeSpan.TicksPerMillisecond / 6;
        private int _numPressesRequired = 1;

        // State
        private bool _isPressed;
        private int _pressCount;
        private long _lastActionTimeTicks;

        // Properties
        public EMouseButton MouseButton { get { return _mouseButton; } set { _mouseButton = value; Updated(); } }
        public bool PressOrRelease { get { return _pressOrRelease; } set { _pressOrRelease = value; Updated(); } }
        public long PressDurationTicks { get { return _pressDurationTicks; } set { _pressDurationTicks = value; Updated(); } }
        public int NumPressesRequired { get { return _numPressesRequired; } set { _numPressesRequired = value; Updated(); } }

        /// <summary>
        /// Return the type of action
        /// </summary>
        public override EActionType ActionType
        {
            get 
            {
                EActionType actionType;
                if (_pressOrRelease)
                {
                    if (_pressDurationTicks > 0L)
                    {
                        actionType = (_numPressesRequired == 1) ? EActionType.MouseClick : EActionType.MouseDoubleClick;
                    }
                    else
                    {
                        actionType = EActionType.MouseHold;
                    }
                }
                else
                {
                    actionType = EActionType.MouseRelease;
                }

                return actionType; 
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
                string verb;
                switch (ActionType)
                {
                    case EActionType.MouseClick:
                        verb = Properties.Resources.String_Click; break;
                    case EActionType.MouseDoubleClick:
                        verb = Properties.Resources.String_Double_click; break;
                    case EActionType.MouseHold:
                        verb = Properties.Resources.String_Hold; break;
                    default:
                        verb = Properties.Resources.String_Release; break;
                }
                Utils utils = new Utils();
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
                return Name.Replace(" mouse button", "");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MouseButtonAction()
            : base()
        {
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
            _pressOrRelease = bool.Parse(element.GetAttribute("pressorrelease"));
            _pressDurationTicks = long.Parse(element.GetAttribute("pressduration"), System.Globalization.CultureInfo.InvariantCulture);
            _numPressesRequired = int.Parse(element.GetAttribute("numpresses"), System.Globalization.CultureInfo.InvariantCulture);

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);
            element.SetAttribute("mousebutton", _mouseButton.ToString());
            element.SetAttribute("pressorrelease", _pressOrRelease.ToString());
            element.SetAttribute("pressduration", _pressDurationTicks.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("numpresses", _numPressesRequired.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Start the action
        /// </summary>
        public override void StartAction(IStateManager parent, AltControlEventArgs cargs)
        {
            MouseManager mouseManager = parent.MouseStateManager;
            if (_pressOrRelease)
            {
                // Press mouse button task (click, hold or double-click)

                // Press the mouse button
                mouseManager.SetButtonState(_mouseButton, true);
                _isPressed = true;
                _pressCount = 1;
                _lastActionTimeTicks = DateTime.Now.Ticks;

                IsOngoing = (_pressDurationTicks > 0);
            }
            else
            {
                // Release button task
                mouseManager.SetButtonState(_mouseButton, false);

                IsOngoing = false;
            }
        }

        /// <summary>
        /// Continue the action
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
                    if (_pressCount >= _numPressesRequired)
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
