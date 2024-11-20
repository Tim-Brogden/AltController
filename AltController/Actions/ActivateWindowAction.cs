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
using System.Diagnostics;
using AltController.Core;
using AltController.Event;
using AltController.Sys;

namespace AltController.Actions
{
    /// <summary>
    /// Set the active window
    /// </summary>
    public class ActivateWindowAction : BaseAction
    {
        // Fields
        private string _programName = "";
        private string _windowTitle = "";
        private string _windowTitleLower = "";
        private EMatchType _matchType = EMatchType.Equals;
        private bool _restoreIfMinimised = true;
        private bool _minimiseIfActive = true;
        private bool _includeHiddenWindows = false;
        private IntPtr _matchedWindow;
        private CallBackPtr _enumWindowsCallback;

        // Properties
        public string ProgramName { get { return _programName; } set { _programName = value; } }
        public string WindowTitle { get { return _windowTitle; } set { _windowTitle = value; _windowTitleLower = value.ToLower(); } }
        public EMatchType MatchType { get { return _matchType; } set { _matchType = value; } }
        public bool RestoreIfMinimised { get { return _restoreIfMinimised; } set { _restoreIfMinimised = value; } }
        public bool MinimiseIfActive { get { return _minimiseIfActive; } set { _minimiseIfActive = value; } }
        public bool IncludeHiddenWindows { get { return _includeHiddenWindows; } set { _includeHiddenWindows = value; } }
        public override EActionType ActionType
        {
            get { return EActionType.ActivateWindow; }
        }

        /// <summary>
        /// Name of action
        /// </summary>
        public override string Name
        {
            get 
            {
                string name = ShortName;
                if (_restoreIfMinimised)
                {
                    if (_minimiseIfActive)
                    {
                        name += string.Format(" ({0}, {1})", Properties.Resources.String_restore, Properties.Resources.String_minimise);
                    }
                    else
                    {
                        name += string.Format(" ({0})", Properties.Resources.String_restore);
                    }
                }
                else if (_minimiseIfActive)
                {
                    name += string.Format(" ({0})", Properties.Resources.String_minimise);
                }
                return name;
            }
        }

        public override string ShortName
        {
            get
            {
                string matchTypeText;
                switch (_matchType)
                {
                    case EMatchType.StartsWith:
                        matchTypeText = Properties.Resources.String_starting + " "; break;
                    case EMatchType.EndsWith:
                        matchTypeText = Properties.Resources.String_ending + " "; break;
                    default:
                        matchTypeText = ""; break;
                }
            
                return Properties.Resources.String_Activate_window + string.Format(" {0}'{1}'", matchTypeText, _windowTitle);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActivateWindowAction()
            : base()
        {
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _programName = element.GetAttribute("program");
            _windowTitle = element.GetAttribute("window");
            _matchType = (EMatchType)Enum.Parse(typeof(EMatchType), element.GetAttribute("matchtype"));
            _restoreIfMinimised = bool.Parse(element.GetAttribute("restore"));
            _minimiseIfActive = bool.Parse(element.GetAttribute("minimise"));
            _includeHiddenWindows = bool.Parse(element.GetAttribute("includehidden"));

            _windowTitleLower = _windowTitle.ToLower();

            base.FromXml(element);
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("program", _programName);
            element.SetAttribute("window", _windowTitle);
            element.SetAttribute("matchtype", _matchType.ToString());
            element.SetAttribute("restore", _restoreIfMinimised.ToString());
            element.SetAttribute("minimise", _minimiseIfActive.ToString());
            element.SetAttribute("includehidden", _includeHiddenWindows.ToString());
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
                _matchedWindow = IntPtr.Zero;
                _enumWindowsCallback = new CallBackPtr(this.EnumWindowsCallback);
                if (_programName != "")                
                {
                    string processName = _programName;
                    if (processName.EndsWith(".exe"))
                    {
                        processName = processName.Substring(0, processName.Length - 4);
                    }
                    Process[] processes = Process.GetProcessesByName(processName);
                    if (processes != null)
                    {
                        foreach (Process process in processes)
                        {
                            foreach (ProcessThread thread in process.Threads)
                            {
                                if (!WindowsAPI.EnumThreadWindows((uint)thread.Id, _enumWindowsCallback, IntPtr.Zero))
                                {
                                    break;
                                }
                            }

                            if (_matchedWindow != IntPtr.Zero)
                            {
                                // Matched a window
                                break;
                            }
                        }
                    }
                }
                else
                {
                    WindowsAPI.EnumWindows(_enumWindowsCallback, IntPtr.Zero);
                }

                if (_matchedWindow != IntPtr.Zero)
                {
                    WindowsAPI.ForceActivateWindow(_matchedWindow, _restoreIfMinimised, _minimiseIfActive);
                }
            }
            catch (Exception)
            {
            }

            IsOngoing = false;
        }

        /// <summary>
        /// Callback for EnumThreadWindows / EnumWindows calls
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            try
            {
                // Only consider relevant windows
                uint style = WindowsAPI.GetWindowStyle(hWnd);
                bool isMatch = WindowsAPI.IsStandardWindowStyle(style);
                if (isMatch) 
                {
                    string titleBarText = WindowsAPI.GetTitleBarText(hWnd).ToLower();
                    switch(_matchType)
                    {
                        case EMatchType.StartsWith:
                            isMatch = titleBarText.StartsWith(_windowTitleLower);
                            break;
                        case EMatchType.EndsWith:
                            isMatch = titleBarText.EndsWith(_windowTitleLower);
                            break;
                        case EMatchType.Equals:
                            isMatch = titleBarText == _windowTitleLower;
                            break;
                        default:
                            isMatch = false;
                            break;
                    }

                    if (isMatch)
                    {
                        // Matched window
                        _matchedWindow = hWnd;
                    }
                    else
                    {
                        // Recurse through child windows
                        WindowsAPI.EnumChildWindows(hWnd, _enumWindowsCallback, IntPtr.Zero);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }

            // Continue unless matched
            return _matchedWindow == IntPtr.Zero;
        }
    }
}
