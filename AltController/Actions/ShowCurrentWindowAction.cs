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
using System.Xml;
using AltController.Core;
using AltController.Event;
using AltController.Sys;

namespace AltController.Actions
{
    /// <summary>
    /// Maximise/restore or minimise the currently active window
    /// </summary>
    public class ShowCurrentWindowAction : BaseAction
    {
        // Fields
        private bool _maximiseOrMinimise = false;

        // Properties
        public bool MaximiseOrMinimise { get { return _maximiseOrMinimise; } set { _maximiseOrMinimise = value; } }

        /// <summary>
        /// Type of action
        /// </summary>
        public override EActionType ActionType
        {
            get { return _maximiseOrMinimise ? EActionType.MaximiseWindow : EActionType.MinimiseWindow; }
        }

        /// <summary>
        /// Name of action
        /// </summary>
        public override string Name
        {
            get { return _maximiseOrMinimise ? Properties.Resources.String_MaximiseOrRestoreWindow : Properties.Resources.String_MinimiseWindow; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ShowCurrentWindowAction()
            : base()
        {
        }

        public ShowCurrentWindowAction(bool maximiseOrMinimise)
        {
            _maximiseOrMinimise = maximiseOrMinimise;
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _maximiseOrMinimise = bool.Parse(element.GetAttribute("maximise"));

            base.FromXml(element);
        }

        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("maximise", _maximiseOrMinimise.ToString());
        }

        /// <summary>
        /// Start the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void StartAction(IStateManager parent, AltControlEventArgs args)
        {
            try
            {
                IntPtr hWnd = WindowsAPI.GetForegroundWindow();
                if (hWnd != IntPtr.Zero)
                {
                    // Check it's an appropriate type of window
                    uint style = WindowsAPI.GetWindowStyle(hWnd);
                    uint maxOrMinStyle = _maximiseOrMinimise ? WindowsAPI.WS_MAXIMIZEBOX : WindowsAPI.WS_MINIMIZEBOX;
                    if (WindowsAPI.IsStandardWindowStyle(style) && (style & maxOrMinStyle) != 0)
                    {
                        // Get the window state
                        if (_maximiseOrMinimise)
                        {
                            // Maximise / restore
                            uint showState = WindowsAPI.GetWindowShowState(hWnd);
                            if (showState == WindowsAPI.SW_SHOWMAXIMIZED) 
                            {
                                WindowsAPI.ShowWindow(hWnd, WindowsAPI.SW_RESTORE);
                            }
                            else
                            {
                                WindowsAPI.ShowWindow(hWnd, WindowsAPI.SW_SHOWMAXIMIZED);
                            }
                        }
                        else
                        {
                            // Minimise
                            WindowsAPI.ForceMinimiseWindow(hWnd);
                        }                       
                    }
                }
            }
            catch (Exception)
            {
            }

            IsOngoing = false;
        }
    }
}
