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
    /// Maximise / minimise the currently active window
    /// </summary>
    public class WindowStateAction : BaseAction
    {
        // Fields
        private EWindowState _windowState = EWindowState.Minimise;

        // Properties
        public EWindowState WindowState { get { return _windowState; } set { _windowState = value; } }

        /// <summary>
        /// Type of action
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.WindowState; }
        }

        /// <summary>
        /// Name of action
        /// </summary>
        public override string Name
        {
            get 
            {
                string name = "";
                switch (_windowState)
                {
                    case EWindowState.Minimise:
                        name = Properties.Resources.String_MinimiseWindow; break;
                    case EWindowState.Maximise:
                        name = Properties.Resources.String_MaximiseWindow; break;
                    case EWindowState.Maximise | EWindowState.Normal:
                        name = Properties.Resources.String_MaximiseOrRestoreWindow; break;
                    default:
                        name = Properties.Resources.String_None; break;
                }
                return name;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public WindowStateAction()
            : base()
        {
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _windowState = (EWindowState)Enum.Parse(typeof(EWindowState), element.GetAttribute("state"));

            base.FromXml(element);
        }

        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("state", _windowState.ToString());
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
                    if (WindowsAPI.IsStandardWindowStyle(style))
                    {
                        uint showState = WindowsAPI.GetWindowShowState(hWnd);
                        switch (_windowState)
                        {
                            case EWindowState.Minimise:
                                if (showState != WindowsAPI.SW_SHOWMINIMIZED && (style & WindowsAPI.WS_MINIMIZEBOX) != 0)
                                {
                                    WindowsAPI.ForceMinimiseWindow(hWnd);
                                }
                                break;
                            case EWindowState.Maximise:
                                if (showState != WindowsAPI.SW_SHOWMAXIMIZED && (style & WindowsAPI.WS_MAXIMIZEBOX) != 0)
                                {
                                    WindowsAPI.ShowWindow(hWnd, WindowsAPI.SW_SHOWMAXIMIZED);
                                }
                                break;
                            case EWindowState.Maximise | EWindowState.Normal:
                                if (showState == WindowsAPI.SW_SHOWMAXIMIZED)
                                {
                                    WindowsAPI.ShowWindow(hWnd, WindowsAPI.SW_RESTORE);
                                }
                                else
                                {
                                    if ((style & WindowsAPI.WS_MAXIMIZEBOX) != 0)
                                    {
                                        WindowsAPI.ShowWindow(hWnd, WindowsAPI.SW_SHOWMAXIMIZED);
                                    }
                                }
                                break;
                            default:
                                break;
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
