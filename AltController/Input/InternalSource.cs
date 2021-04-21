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
using System.Diagnostics;
using AltController.Core;
using AltController.Config;
using AltController.Sys;

namespace AltController.Input
{
    public class InternalSource : BaseSource
    {
        private long _uiUpdateIntervalTicks = Constants.WindowPollingIntervalMS * TimeSpan.TicksPerMillisecond;
        private long _lastUpdateTicks = DateTime.Now.Ticks;
        private int _currentProcessID = -1;
        private WINDOWINFO _windowInfo = new WINDOWINFO(true);

        /// <summary>
        /// Return what type of source this is
        /// </summary>
        public override ESourceType SourceType
        {
            get { return ESourceType.Internal; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public InternalSource()
            : base()
        {            
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public InternalSource(long id, string name)
            : base(id, name)
        {            
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public InternalSource(InternalSource source)
            :base(source)
        {           
        }

        /// <summary>
        /// Detect change of application focus
        /// </summary>
        /// <param name="stateManager"></param>
        public override void UpdateState(IStateManager stateManager)
        {
            // Only do every so often
            long nowTicks = DateTime.Now.Ticks;
            if (nowTicks - _lastUpdateTicks > _uiUpdateIntervalTicks)
            {
                try
                {
                    _lastUpdateTicks = nowTicks;

                    // Get the current window
                    IntPtr handle = WindowsAPI.GetForegroundWindow();
                    if (handle != IntPtr.Zero)
                    {
                        // Get window's process ID
                        int processID;
                        WindowsAPI.GetWindowThreadProcessId(handle, out processID);

                        if (processID != _currentProcessID)
                        {
                            _currentProcessID = processID;

                            // Current process change - get exe name
                            Process process = Process.GetProcessById(processID);
                            if (process != null)
                            {
                                // Update the state manager
                                stateManager.SetApp(process.ProcessName);
                            }
                        }

                        // Get window size and position.
                        // Report change of window dimensions after any change of app
                        // so that screen regions are not briefly drawn over an app that they do not apply to.
                        if (WindowsAPI.GetWindowInfo(handle, ref _windowInfo))
                        {
                            // Update state manager
                            stateManager.SetCurrentWindowRect(_windowInfo.rcWindow);
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore errors
                }
            }
        }
    }
}
