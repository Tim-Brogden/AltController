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
//using System.Diagnostics;
using System.Windows.Forms;
using AltController.Config;
using AltController.Core;
using AltController.Input;
using AltController.Event;
using AltController.Sys;

namespace AltController.Controls
{
    /// <summary>
    /// Monitors mouse button states
    /// </summary>
    public class MouseButtonSet : BaseControl
    {
        // Members
        private List<byte> _buttonsToMonitor = new List<byte>();
        private bool[] _isButtonPressed = new bool[6];
        private Keys[] _mouseButtonKeyCodes = new Keys[6];

        /// <summary>
        /// Get the event reasons supported by this control
        /// </summary>
        public override List<EEventReason> SupportedEventReasons
        {
            get { return new List<EEventReason> { EEventReason.Pressed, EEventReason.Released }; }
        }        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buttons"></param>
        public MouseButtonSet(MouseSource parent)
            :base(parent)
        {
            _mouseButtonKeyCodes[(int)EMouseButton.None] = Keys.None;
            _mouseButtonKeyCodes[(int)EMouseButton.Left] = Keys.LButton;
            _mouseButtonKeyCodes[(int)EMouseButton.Middle] = Keys.MButton;
            _mouseButtonKeyCodes[(int)EMouseButton.Right] = Keys.RButton;
            _mouseButtonKeyCodes[(int)EMouseButton.X1] = Keys.XButton1;
            _mouseButtonKeyCodes[(int)EMouseButton.X2] = Keys.XButton2;
        }

        /// <summary>
        /// Enable event monitoring
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="enable"></param>
        public override void Connect(AltControlEventHandler handler, bool enable)
        {
            base.Connect(handler, enable);

            // Determine which buttons to monitor
            _buttonsToMonitor.Clear();
            if (enable)
            {
                ActionMappingTable table = Parent.Profile.GetActionsForControlType(EControlType.MouseButtons);
                Dictionary<long, ActionList>.Enumerator eTable = table.GetEnumerator();
                while (eTable.MoveNext())
                {
                    ActionList actionList = eTable.Current.Value;
                    if (actionList.EventArgs.ControlType == EControlType.MouseButtons &&
                        !_buttonsToMonitor.Contains(actionList.EventArgs.ButtonID))
                    {
                        _buttonsToMonitor.Add(actionList.EventArgs.ButtonID);
                        //Trace.WriteLine("Monitoring mouse button: " + (EMouseButton)actionList.EventArgs.ButtonID);
                    }
                }
            }
            IsActive = _buttonsToMonitor.Count != 0;
        }

        /// <summary>
        /// Handle a change in mouse button state and generate any button events
        /// </summary>
        /// <param name="newState"></param>
        public override void UpdateState(IStateManager stateManager)
        {
            bool isPressed;
            foreach (byte button in _buttonsToMonitor)
            {
                isPressed = (WindowsAPI.GetKeyState((ushort)_mouseButtonKeyCodes[button]) & 0x8000) != 0;
                if (isPressed != _isButtonPressed[button])
                {
                    //Trace.WriteLine(string.Format("{0} {1}", (EMouseButton)button, isPressed ? "pressed" : "released"));
                    _isButtonPressed[button] = isPressed;
                    RaiseAnyButtonEvents((EMouseButton)button, isPressed);
                }
            }
        }

        /// <summary>
        /// Raise a button event if required
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="buttonFlag"></param>
        private void RaiseAnyButtonEvents(EMouseButton mouseButton, bool isPressEvent)
        {
            AltControlEventArgs args;
            MouseSource mouseSource = (MouseSource)Parent;

            // Raise event for each screen region that the pointer is inside
            ScreenRegionList regionsList = Parent.Profile.ScreenRegions;
            foreach (ScreenRegion region in regionsList)
            {
                byte regionID = (byte)region.ID;
                if (mouseSource.IsPointerInRegion(regionID))
                {
                    args = new AltControlEventArgs();
                    args.SourceID = Parent.ID;
                    args.ControlType = EControlType.MouseButtons;
                    args.ButtonID = (byte)mouseButton;
                    args.Data = regionID;
                    if (isPressEvent)
                    {
                        args.EventReason = EEventReason.Pressed;
                        RaiseEvent(args);
                    }
                    else
                    {
                        args.EventReason = EEventReason.Released;
                        RaiseEvent(args);
                    }
                }
            }

            // Raise event without screen region
            args = new AltControlEventArgs();
            args.SourceID = Parent.ID;
            args.ControlType = EControlType.MouseButtons;
            args.ButtonID = (byte)mouseButton;
            args.Data = 0;
            if (isPressEvent)
            {
                args.EventReason = EEventReason.Pressed;
                RaiseEvent(args);
            }
            else
            {
                args.EventReason = EEventReason.Released;
                RaiseEvent(args);
            }       
        }
    }
}
