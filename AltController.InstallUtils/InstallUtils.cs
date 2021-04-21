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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace AltController.InstallUtils
{
    [RunInstaller(true)]
    public partial class InstallUtils : System.Configuration.Install.Installer
    {
        private const string ApplicationName = "Alt Controller";
        private const string ProfilesFolderName = "Profiles";
        private const string MessageLogFileName = "Message Log.txt";
        private const string ConfigFileName = "config.xml";

        public InstallUtils()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Uninstall event
        /// </summary>
        /// <param name="savedState"></param>
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            DeleteSampleProfiles();
            DeleteLocalAppDataDir();
        }

        /// <summary>
        /// Delete the local application data folder which contains the config.xml file
        /// </summary>
        private void DeleteLocalAppDataDir()
        {
            try
            {
                string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationName);

                // Delete the files in the directory
                string configFilePath = Path.Combine(directory, ConfigFileName);
                if (File.Exists(configFilePath))
                {
                    File.Delete(configFilePath);
                }
                string messageLogFilePath = Path.Combine(directory, MessageLogFileName);
                if (File.Exists(messageLogFilePath))
                {
                    File.Delete(messageLogFilePath);
                }

                // Delete the directory if empty
                DirectoryInfo dirInfo = new DirectoryInfo(directory);
                if (dirInfo.Exists &&
                    dirInfo.GetFiles().Length == 0 &&
                    dirInfo.GetDirectories().Length == 0)
                {
                    Directory.Delete(directory);
                }
            }
            catch (Exception ex)
            {
                // Log error
                string message = string.Format("Error while removing local application data during uninstall. Details:{0}{1}",
                                                Environment.NewLine, ex.Message);
                LogEvent(message, EventLogEntryType.Warning);
            }
        }
 
        /// <summary>
        /// Delete any sample profiles that have been deployed to the user's profiles directory
        /// </summary>
        /// <remarks>
        /// Limitation - if another user has run the program (who didn't do the install), 
        /// their sample profiles won't be deleted (they'll have to delete them manually)
        /// </remarks>
        private void DeleteSampleProfiles()
        {
            try
            {
                // Get the root path
                string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string samplesProfilesDir = Path.Combine(rootDir, ProfilesFolderName);

                // Get the list of sample profiles
                DirectoryInfo di = new DirectoryInfo(samplesProfilesDir);
                FileInfo[] sampleProfilesList = di.GetFiles();

                // Get the current user's profiles directory
                string userProfilesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ApplicationName, ProfilesFolderName);
                if (!Directory.Exists(userProfilesDir))
                {
                    userProfilesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationName, ProfilesFolderName);
                }
                DirectoryInfo userProfilesDirInfo = new DirectoryInfo(userProfilesDir);
                if (userProfilesDirInfo.Exists)
                {
                    // Delete sample files that have been deployed to user's Profiles folder but not if modified by user
                    foreach (FileInfo sampleFile in sampleProfilesList)
                    {
                        string userFilePath = Path.Combine(userProfilesDir, sampleFile.Name);
                        FileInfo userFile = new FileInfo(userFilePath);
                        if (userFile.Exists && userFile.LastWriteTime <= sampleFile.LastWriteTime)
                        {
                            userFile.Delete();
                        }
                    }

                    // If the user's Profiles directory is empty, delete it
                    DirectoryInfo altControllerDirInfo = userProfilesDirInfo.Parent;
                    if (userProfilesDirInfo.Exists &&
                        userProfilesDirInfo.GetFiles().Length == 0 &&
                        userProfilesDirInfo.GetDirectories().Length == 0)
                    {
                        userProfilesDirInfo.Delete();

                        // If the Alt Controller directory is empty, delete it
                        if (altControllerDirInfo.GetFiles().Length == 0 &&
                            altControllerDirInfo.GetDirectories().Length == 0)
                        {
                            altControllerDirInfo.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                string message = string.Format("Error while removing sample profiles during uninstall. Details:{0}{1}",
                                                Environment.NewLine, ex.Message);
                LogEvent(message, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Write a warning to the error log
        /// </summary>
        /// <param name="message"></param>
        private void LogEvent(string message, EventLogEntryType eventType)
        {
            try
            {
                EventLog.WriteEntry(ApplicationName, message, eventType);
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }
}
