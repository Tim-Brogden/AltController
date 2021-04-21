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
using AltController.Config;
using AltController.Core;
using AltController.Input;
using AltController.Event;

namespace AltController.Controls
{
    /// <summary>
    /// Monitors the state of keyboard keys
    /// </summary>
    public class KeyboardControl : BaseControl
    {
        // State
        private bool[] _isKeyPressed = new bool[256];
        private List<byte> _keysToMonitor = new List<byte>();

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
        public KeyboardControl(KeyboardSource parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Enable event monitoring
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="enable"></param>
        public override void Connect(AltControlEventHandler handler, bool enable)
        {
            base.Connect(handler, enable);

            // Determine which keys to monitor
            _keysToMonitor.Clear();
            if (enable)
            {
                ActionMappingTable table = Parent.Profile.GetActionsForControlType(EControlType.Keyboard);
                Dictionary<long, ActionList>.Enumerator eTable = table.GetEnumerator();
                while (eTable.MoveNext())
                {
                    ActionList actionList = eTable.Current.Value;
                    if (actionList.EventArgs.ControlType == EControlType.Keyboard &&
                        !_keysToMonitor.Contains(actionList.EventArgs.Data))
                    {
                        _keysToMonitor.Add(actionList.EventArgs.Data);
                        //Trace.WriteLine("Monitoring key: " + (System.Windows.Forms.Keys)actionList.EventArgs.Data);
                    }
                }
            }
            IsActive = _keysToMonitor.Count != 0;
        }
        
        /// <summary>
        /// Handle a change in controller state and generate any button events
        /// </summary>
        /// <param name="newState"></param>
        public override void UpdateState(IStateManager stateManager)
        {
            foreach (byte key in _keysToMonitor) 
            {
                // Get current key state
                bool isPressed = stateManager.KeyStateManager.IsKeyPressed((System.Windows.Forms.Keys)key);

                // Raise events if key has been pressed or released
                if (isPressed && !_isKeyPressed[key])
                {
                    AltControlEventArgs args = new AltControlEventArgs();
                    args.SourceID = Parent.ID;
                    args.ControlType = EControlType.Keyboard;
                    args.Data = key;
                    args.EventReason = EEventReason.Pressed;
                    //Trace.WriteLine(string.Format("{0} pressed", (System.Windows.Forms.Keys)key));
                    RaiseEvent(args);
                }
                else if (!isPressed && _isKeyPressed[key])
                {
                    AltControlEventArgs args = new AltControlEventArgs();
                    args.SourceID = Parent.ID;
                    args.ControlType = EControlType.Keyboard;
                    args.Data = key;
                    args.EventReason = EEventReason.Released;
                    //Trace.WriteLine(string.Format("{0} released", (System.Windows.Forms.Keys)key));
                    RaiseEvent(args);
                }

                // Store key state for next time
                _isKeyPressed[key] = isPressed;
            }            
        }

    }
}
