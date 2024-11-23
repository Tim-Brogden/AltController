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
using System.Diagnostics;
using System.Text.RegularExpressions;
using AltController.Actions;

namespace AltController.Core
{
    /// <summary>
    /// Starts processes
    /// </summary>
    public static class ProcessManager
    {
        private static Regex _executableFileRegex = new Regex("\\.(bat|bin|cmd|exe)$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Starts a process
        /// </summary>
        /// <param name="action"></param>
        public static void Start(StartProgramAction action)
        {
            string fileTarget = action.GetTarget();

            ProcessStartInfo psi = null;
            if (fileTarget != "")
            {
                if (action.ProgramArgs != "")
                {
                    psi = new ProcessStartInfo(fileTarget, action.ProgramArgs);
                }
                else
                {
                    psi = new ProcessStartInfo(fileTarget);
                }
            }
            else if (action.ProgramArgs != "")
            {
                psi = new ProcessStartInfo(action.ProgramArgs);
            }

            if (psi != null)
            {
                Process.Start(psi);
            }
        }

        /// <summary>
        /// Start a process with an application or document path
        /// </summary>
        /// <param name="fileName"></param>
        public static void Start(string fileName)
        {
            Process.Start(fileName);
        }

        /// <summary>
        /// See whether a process is already running
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        public static bool IsRunning(string processName)
        {
            bool isRunning = false;
            if (processName != "")
            {
                // Convert to friendly name without extension if required
                if (_executableFileRegex.IsMatch(processName))
                {
                    processName = processName.Substring(0, processName.Length - 4);
                }

                Process[] processes = Process.GetProcessesByName(processName);
                if (processes != null && processes.Length != 0)
                {
                    isRunning = true;
                }
            }

            return isRunning;
        }
    }
}
