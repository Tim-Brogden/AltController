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
using System.Windows.Forms;
using AltController.Core;
using AltController.Event;

namespace AltController.Sys
{
    /// <summary>
    /// Manages keyboard state
    /// </summary>
    public class KeyPressManager
    {
        private bool[] _keysWePressed = new bool[256];
        private bool[] _keysWeCanRepeat = new bool[256];
        private bool[] _isExtendedKey = new bool[256];
        private VirtualKeyData[] _virtualKeyData;
        private IStateManager _parent;
        private bool _useScanCodes = Constants.DefaultUseScanCodes;

        // Properties
        public bool UseScanCodes { set { _useScanCodes = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public KeyPressManager(IStateManager parent)
        {
            _parent = parent;

            // Initialise extended keys array
            InitialiseExtendedKeysList();
        }

        /// <summary>
        /// Initialise
        /// </summary>
        public void Initialise()
        {
            // Initialise keyboard scan codes
            _virtualKeyData = KeyUtils.GetVirtualKeysByKeyCode();
        }

        /// <summary>
        /// Destroy
        /// </summary>
        public void Destroy()
        {
        }

        /// <summary>
        /// Release any keyboard keys that are held
        /// </summary>
        public void ReleaseAllKeys()
        {
            // Release any keys we pressed
            for (int i = 0; i < 256; i++)
            {
                if (_keysWePressed[i])
                {
                    SetKeyState(_virtualKeyData[i], false);
                }
            }
        }

        /// <summary>
        /// Emulate a key down or up using Windows API
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyDown"></param>
        public void SetKeyState(Keys key, bool keyDown)
        {
            SetKeyState(_virtualKeyData[(byte)key], keyDown);
        }

        /// <summary>
        /// Emulate a key down or up using Windows API
        /// </summary>
        /// <param name="keyCode"></param>
        public void SetKeyState(VirtualKeyData vk, bool keyDown)
        {
            // Don't release key if it's already released
            if (vk != null && (keyDown || _keysWePressed[(byte)vk.KeyCode]))
            {
                INPUT[] inputs = new INPUT[1];
                inputs[0].type = WindowsAPI.INPUT_KEYBOARD;
                inputs[0].ki.dwFlags = keyDown ? 0u : WindowsAPI.KEYEVENTF_KEYUP;
                if (_useScanCodes && vk.WindowsScanCode != 0)
                {
                    // Use scan code
                    inputs[0].ki.dwFlags |= WindowsAPI.KEYEVENTF_SCANCODE;
                    inputs[0].ki.wScan = vk.WindowsScanCode;

                    // Extended key flag
                    if (_isExtendedKey[(byte)vk.KeyCode])
                    {
                        inputs[0].ki.dwFlags |= WindowsAPI.KEYEVENTF_EXTENDEDKEY;
                    }
                }
                else
                {
                    // Use virtual key code (scan code zero indicates vk must be used)
                    inputs[0].ki.wVk = (ushort)vk.KeyCode;
                }

                WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));

                // Store the key state so that we know which keys we pressed
                _keysWePressed[(byte)vk.KeyCode] = keyDown;

                // Report key event if required
                if (_parent.IsDiagnosticsEnabled)
                {
                    AltKeyEventArgs args = new AltKeyEventArgs(vk.KeyCode, keyDown);
                    EventReport report = new EventReport(DateTime.Now, EEventType.KeyEvent, args);
                    _parent.ReportEvent(report);
                }
            }
        }

        /// <summary>
        /// Set whether or not we are allowed to repeat a given key
        /// </summary>
        /// <param name="vk"></param>
        /// <param name="keyCanRepeat"></param>
        public void SetCanKeyRepeat(VirtualKeyData vk, bool keyCanRepeat)
        {
            if (vk != null)
            {
                _keysWeCanRepeat[(byte)vk.KeyCode] = keyCanRepeat;
            }
        }

        /// <summary>
        /// Get whether or not we are allowed to repeat a given key
        /// </summary>
        /// <param name="vk"></param>
        /// <param name="keyCanRepeat"></param>
        public bool GetCanKeyRepeat(VirtualKeyData vk)
        {
            bool canRepeat = false;
            if (vk != null)
            {
                canRepeat = _keysWeCanRepeat[(byte)vk.KeyCode];
            }

            return canRepeat;
        }

        /// <summary>
        /// Toggle the specified key
        /// </summary>
        /// <param name="key"></param>
        public void ToggleKeyState(VirtualKeyData vk)
        {
            if (vk != null)
            {
                SetKeyState(vk, !_keysWePressed[(byte)vk.KeyCode]);
            }
        }

        /// <summary>
        /// Emulate typing a string, which can include unicode characters
        /// </summary>
        /// <param name="textToType"></param>
        public void TypeString(string textToType)
        {
            INPUT[] inputs = new INPUT[2 * textToType.Length];
            int i = 0;
            foreach (char ch in textToType)
            {
                inputs[i].type = WindowsAPI.INPUT_KEYBOARD;
                inputs[i].ki.dwFlags = WindowsAPI.KEYEVENTF_UNICODE;
                inputs[i].ki.wScan = ch;
                i++;
                inputs[i].type = WindowsAPI.INPUT_KEYBOARD;
                inputs[i].ki.dwFlags = WindowsAPI.KEYEVENTF_UNICODE | WindowsAPI.KEYEVENTF_KEYUP;
                inputs[i].ki.wScan = ch;
                i++;
            }
            WindowsAPI.SendInput((uint)inputs.Length, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }

        /// <summary>
        /// Check whether a key is pressed or not
        /// </summary>
        /// <param name="key"></param>
        public bool IsKeyPressed(Keys key)
        {
            ushort keyState = WindowsAPI.GetKeyState((int)key);
            return ((keyState & 0x8000) != 0);
        }

        /// <summary>
        /// Check whether a key is toggled or not
        /// </summary>
        /// <param name="key"></param>
        public bool IsKeyToggled(Keys key)
        {
            ushort keyState = WindowsAPI.GetKeyState((int)key);
            return ((keyState & 0x0001) != 0);
        }

        /// <summary>
        /// Initialise the list of extended keys
        /// </summary>
        private void InitialiseExtendedKeysList()
        {
            List<Keys> extendedKeys = KeyUtils.GetExtendedKeys();
            foreach (Keys key in extendedKeys)
            {
                _isExtendedKey[(byte)key] = true;
            }
        }
    }
}
