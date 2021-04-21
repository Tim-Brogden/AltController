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
using System.IO;
using System.Text;
using AltController.Core;

namespace AltController.Config
{
    /// <summary>
    /// Stores application-wide config settings
    /// </summary>
    public class AppConfig
    {
        // Members
        private static string _baseDir;
        private static string _userDataDir;
        private Dictionary<string, string> _keyValueDictionary = new Dictionary<string, string>();

        // Properties
        public static string BaseDir
        {
            get
            {
                if (_baseDir == null)
                {
                    string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    if (exeDir.EndsWith("Debug") || exeDir.EndsWith("Release"))
                    {
                        // Dev environment
                        _baseDir = Path.Combine(exeDir, "..", "..");
                        DirectoryInfo di = new DirectoryInfo(_baseDir);
                        _baseDir = di.FullName;
                    }
                    else
                    {
                        // Installed
                        _baseDir = exeDir;
                    }
                }

                return _baseDir;
            }
        }
        public static string UserDataDir
        {
            get
            {
                if (_userDataDir == null)
                {
                    if (BaseDir != Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))
                    {
                        // Dev environment
                        _userDataDir = BaseDir;
                    }
                    else
                    {
                        // Installed
                        // Legacy support: if folder under My Documents exists, use that.
                        // Otherwise use folder under Local App Data in order to avoid permissions issues with writing to user's Documents folder
                        _userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Constants.ApplicationName);
                        if (!Directory.Exists(_userDataDir))
                        {
                            _userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Constants.ApplicationName);                            
                        }
                    }
                }

                return _userDataDir;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AppConfig()
        {
        }

        /// <summary>
        /// Return whether the config file exists
        /// </summary>
        /// <returns></returns>
        public static bool FileExists()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                Constants.ApplicationName,
                                Constants.ConfigFileName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Load from file
        /// </summary>
        public bool Load()
        {
            bool success = true;
            try
            {
                // See if config file exists
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                Constants.ApplicationName,
                                                Constants.ConfigFileName);
                if (File.Exists(filePath))
                {
                    string xml = File.ReadAllText(filePath);
                    success = FromXml(xml);
                }
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Initialise from xml string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public bool FromXml(string xml)
        {
            bool success = true;
            try
            {
                // Load from file
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                // Read settings
                XmlNodeList settingElements = doc.SelectNodes("/config/setting");
                foreach (XmlElement settingElement in settingElements)
                {
                    string name = settingElement.GetAttribute("name");
                    string val = settingElement.GetAttribute("value");
                    _keyValueDictionary[name] = val;
                }
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Save to file
        /// </summary>
        public bool Save()
        {
            bool success = true;
            try
            {
                // Create config dir if required
                string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Constants.ApplicationName);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // Save XML to file
                string filePath = Path.Combine(configDir, Constants.ConfigFileName);
                string xml = ToXml();
                File.WriteAllText(filePath, xml);
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Convert to Xml
        /// </summary>
        /// <returns></returns>
        public string ToXml()
        {
            XmlDocument doc = new XmlDocument();

            // Root element
            XmlElement configElement = doc.CreateElement("config");
            doc.AppendChild(configElement);

            // Settings
            Dictionary<string, string>.Enumerator eDict = _keyValueDictionary.GetEnumerator();
            while (eDict.MoveNext())
            {
                XmlElement entryElement = doc.CreateElement("setting");
                entryElement.SetAttribute("name", eDict.Current.Key);
                entryElement.SetAttribute("value", eDict.Current.Value);
                configElement.AppendChild(entryElement);
            }

            // Save to string
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            XmlWriter xWriter = XmlTextWriter.Create(sb, settings);
            doc.Save(xWriter);

            return sb.ToString();
        }

        /// <summary>
        /// Set a param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void SetVal(string key, object val)
        {
            if (val != null)
            {
                _keyValueDictionary[key] = val.ToString();
            }
            else if (_keyValueDictionary.ContainsKey(key))
            {
                _keyValueDictionary.Remove(key);
            }
        }

        /// <summary>
        /// Get a bool param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public bool GetBoolVal(string key, bool defaultVal)
        {
            bool boolVal;
            string val = GetStringVal(key, null);
            if (val == null || !bool.TryParse(val, out boolVal))
            {
                boolVal = defaultVal;
            }

            return boolVal;
        }

        /// <summary>
        /// Get an int param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public int GetIntVal(string key, int defaultVal)
        {
            int intVal;
            string val = GetStringVal(key, null);
            if (val == null || !int.TryParse(val, out intVal))
            {
                intVal = defaultVal;
            }

            return intVal;
        }

        /// <summary>
        /// Get a double param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public double GetDoubleVal(string key, double defaultVal)
        {
            double dblVal;
            string val = GetStringVal(key, null);
            if (val == null || !double.TryParse(val, out dblVal))
            {
                dblVal = defaultVal;
            }

            return dblVal;
        }

        /// <summary>
        /// Get a float param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public float GetFloatVal(string key, float defaultVal)
        {
            float fVal;
            string val = GetStringVal(key, null);
            if (val == null || !float.TryParse(val, out fVal))
            {
                fVal = defaultVal;
            }

            return fVal;
        }

        /// <summary>
        /// Get a string param value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetStringVal(string key, string defaultVal)
        {
            string val;
            if (_keyValueDictionary.ContainsKey(key))
            {
                val = _keyValueDictionary[key];
            }
            else
            {
                val = defaultVal;
            }

            return val;
        }
    }
}
