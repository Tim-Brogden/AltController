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
using System.Collections.Generic;
using System.IO;
using AltController.Config;
using AltController.Core;
using AltController.Event;

namespace AltController.Sys
{
    /// <summary>
    /// Manages keyboard state
    /// </summary>
    public class MouseManager
    {
        private bool[] _buttonsWePressed = new bool[6];
        private IStateManager _parent;
        private bool _canScrollUpRepeat;
        private bool _canScrollDownRepeat;

        // This property tells the repeat scroll action to stop
        public bool CanScrollUpRepeat { get { return _canScrollUpRepeat; } set { _canScrollUpRepeat = value; } }
        public bool CanScrollDownRepeat { get { return _canScrollDownRepeat; } set { _canScrollDownRepeat = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public MouseManager(IStateManager parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Initialise
        /// </summary>
        public void Initialise()
        {
        }

        /// <summary>
        /// Release any mouse buttons that are held
        /// </summary>
        public void ReleaseAllButtons()
        {
            // Release any buttons we pressed
            for (int i = 1; i < _buttonsWePressed.Length; i++)
            {
                SetButtonState((EMouseButton)i, false);
            }
        }

        /// <summary>
        /// Toggle a mouse button (press if unpressed, and vice versa)
        /// </summary>
        /// <param name="eButton"></param>
        public void ToggleButtonState(EMouseButton eButton)
        {
            bool buttonDown = !_buttonsWePressed[(int)eButton];
            SetButtonState(eButton, buttonDown);
        }

        /// <summary>
        /// Emulate a mouse button down or up using Windows API
        /// </summary>
        public void SetButtonState(EMouseButton eButton, bool buttonDown)
        {
            // Check button is not already in required state
            if (_buttonsWePressed[(int)eButton] != buttonDown)     // Whether we think the button is up/down                               
            {
                INPUT[] inputs = new INPUT[1];
                inputs[0].type = WindowsAPI.INPUT_MOUSE;
                switch (eButton)
                {
                    case EMouseButton.Left:
                        inputs[0].mi.dwFlags = buttonDown ? WindowsAPI.MOUSEEVENTF_LEFTDOWN : WindowsAPI.MOUSEEVENTF_LEFTUP;
                        break;
                    case EMouseButton.Middle:
                        inputs[0].mi.dwFlags = buttonDown ? WindowsAPI.MOUSEEVENTF_MIDDLEDOWN : WindowsAPI.MOUSEEVENTF_MIDDLEUP;
                        break;
                    case EMouseButton.Right:
                        inputs[0].mi.dwFlags = buttonDown ? WindowsAPI.MOUSEEVENTF_RIGHTDOWN : WindowsAPI.MOUSEEVENTF_RIGHTUP;
                        break;
                    case EMouseButton.X1:
                        inputs[0].mi.dwFlags = buttonDown ? WindowsAPI.MOUSEEVENTF_XDOWN : WindowsAPI.MOUSEEVENTF_XUP;
                        inputs[0].mi.mouseData = WindowsAPI.XBUTTON1;
                        break;
                    case EMouseButton.X2:
                        inputs[0].mi.dwFlags = buttonDown ? WindowsAPI.MOUSEEVENTF_XDOWN : WindowsAPI.MOUSEEVENTF_XUP;
                        inputs[0].mi.mouseData = WindowsAPI.XBUTTON2;
                        break;
                }
                WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));

                // Report mouse button event if required
                if (_parent.IsDiagnosticsEnabled)
                {
                    AltMouseButtonEventArgs args = new AltMouseButtonEventArgs(eButton, buttonDown);
                    EventReport report = new EventReport(DateTime.Now, EEventType.MouseButtonEvent, args);
                    _parent.ReportEvent(report);
                }

                // Store the key state so that we know which buttons we pressed
                _buttonsWePressed[(int)eButton] = buttonDown;
            }
        }

        /// <summary>
        /// Emulate a mouse scroll up or down using Windows API
        /// </summary>
        public void MouseScroll(bool isUp)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = WindowsAPI.INPUT_MOUSE;
            inputs[0].mi.dwFlags = WindowsAPI.MOUSEEVENTF_WHEEL;
            inputs[0].mi.mouseData = (uint)(isUp ? 120 : -120);
            WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));

            // Report mouse button event if required
            if (_parent.IsDiagnosticsEnabled)
            {
                AltMouseScrollEventArgs args = new AltMouseScrollEventArgs(isUp);
                EventReport report = new EventReport(DateTime.Now, EEventType.MouseScrollEvent, args);
                _parent.ReportEvent(report);
            }
        }

        /// <summary>
        /// Move the mouse pointer
        /// X and Y are normalised absolute screen coords (0 to 65535)
        /// Relative mouse moves will be scaled by the system, according to the mouse speed options in the control panel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveMouse(int x, int y, bool absolute)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = WindowsAPI.INPUT_MOUSE;
            inputs[0].mi.dwFlags = WindowsAPI.MOUSEEVENTF_MOVE;
            if (absolute)
            {
                inputs[0].mi.dwFlags |= WindowsAPI.MOUSEEVENTF_ABSOLUTE;
            }
            inputs[0].mi.dx = x;            
            inputs[0].mi.dy = y;
            WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }

    }
}
