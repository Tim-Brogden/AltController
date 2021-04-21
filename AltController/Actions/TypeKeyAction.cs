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
using System.Windows.Forms;
using System.Xml;
using AltController.Core;
using AltController.Event;
using AltController.Sys;
namespace AltController.Actions
{
    /// <summary>
    /// Action for typing a keyboard key or key combination
    /// </summary>
    public class TypeKeyAction : HoldKeyAction
    {
        // Configuration
        private bool _isAltModifier = false;
        private bool _isControlModifier = false;
        private bool _isShiftModifier = false;
        private bool _isWinModifier = false;

        public bool IsAltModifierSet { get { return _isAltModifier; } set { _isAltModifier = value; Updated(); } }
        public bool IsControlModifierSet { get { return _isControlModifier; } set { _isControlModifier = value; Updated(); } }
        public bool IsShiftModifierSet { get { return _isShiftModifier; } set { _isShiftModifier = value; Updated(); } }
        public bool IsWinModifierSet { get { return _isWinModifier; } set { _isWinModifier = value; Updated(); } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.TypeKey; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public TypeKeyAction()
            :base()
        {
            PressDurationMS = Constants.DefaultPressTimeMS;
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                return string.Format(Properties.Resources.String_Type + " {0}{1}{2}{3}{4}",
                                        IsAltModifierSet ? "Alt+" : "",
                                        IsControlModifierSet ? "Ctrl+" : "",
                                        IsShiftModifierSet ? "Shift+" : "",
                                        IsWinModifierSet ? "Win+" : "",
                                        base.KeyName);
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _isAltModifier = bool.Parse(element.GetAttribute("alt"));
            _isControlModifier = bool.Parse(element.GetAttribute("control"));
            _isShiftModifier = bool.Parse(element.GetAttribute("shift"));
            _isWinModifier = bool.Parse(element.GetAttribute("win"));

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("alt", _isAltModifier.ToString());
            element.SetAttribute("control", _isControlModifier.ToString());
            element.SetAttribute("shift", _isShiftModifier.ToString());
            element.SetAttribute("win", _isWinModifier.ToString());
        }

        /// <summary>
        /// Start the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void StartAction(IStateManager parent, AltControlEventArgs args)
        {
            SetModifierKeys(parent, true);

            // Press down key
            base.StartAction(parent, args);
        }

        /// <summary>
        /// Continue the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override void ContinueAction(IStateManager parent, AltControlEventArgs args)
        {
            // Release key after press duration
            base.ContinueAction(parent, args);

            // When the key has been released, release any modifiers
            if (!IsOngoing) {
                SetModifierKeys(parent, false);
            }
        }

        /// <summary>
        /// Stop the action
        /// </summary>
        /// <param name="parent"></param>
        public override void StopAction(IStateManager parent)
        {
            base.StopAction(parent);
            SetModifierKeys(parent, false);
        }

        /// <summary>
        /// Release modifier keys
        /// </summary>        
        private void SetModifierKeys(IStateManager parent, bool press)
        {
            KeyPressManager keyStateManager = parent.KeyStateManager;
            if (_isAltModifier) keyStateManager.SetKeyState(Keys.LShiftKey, press);
            if (_isControlModifier) keyStateManager.SetKeyState(Keys.LControlKey, press);
            if (_isShiftModifier) keyStateManager.SetKeyState(Keys.LShiftKey, press);
            if (_isWinModifier) keyStateManager.SetKeyState(Keys.LWin, press);
        }

    }
}
