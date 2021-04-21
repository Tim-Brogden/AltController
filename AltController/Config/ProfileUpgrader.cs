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
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
using AltController.Core;

namespace AltController.Config
{
    /// <summary>
    /// Upgrades profiles to the current version
    /// </summary>
    public class ProfileUpgrader
    {
        private Dictionary<string, string> _upgradeStages = new Dictionary<string, string>();

        /// <summary>
        /// Constructor
        /// </summary>
        public ProfileUpgrader()
        {
            // Upgrade stages
            _upgradeStages["0.3"] = "0.5";
            _upgradeStages["0.5"] = "1.3";
            _upgradeStages["1.3"] = "1.4";
            _upgradeStages["1.4"] = "1.5";
            _upgradeStages["1.5"] = "1.6";
        }

        /// <summary>
        /// Upgrade a profile to the current version
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>whether upgraded or not</returns>
        public bool Upgrade(ref XmlDocument doc)
        {
            bool upgraded = false;
            try
            {
                // Upgrade the profile one version at a time
                string prevVersion = "0";
                string version;
                
                // Incrementally apply the sequence of upgrades
                while ((version = GetProfileVersionNumber(doc)) != prevVersion &&
                        version != Constants.FileVersion.ToString())
                {
                    prevVersion = version;
                    
                    // Find the upgrade XSL
                    string upgradeFileName = string.Format("Upgrade from v{0}.xsl", version);
                    string upgradeFilePath = Path.Combine(AppConfig.BaseDir, "AutoUpgrade", upgradeFileName);
                    if (File.Exists(upgradeFilePath))
                    {
                        // Load XSL transform
                        XslCompiledTransform transform = new XslCompiledTransform();
                        transform.Load(upgradeFilePath);

                        // Apply transform
                        StringBuilder sb = new StringBuilder();
                        XmlWriter writer = XmlWriter.Create(sb, transform.OutputSettings);
                        transform.Transform(doc, writer);
                        
                        // Replace doc with transformed doc
                        doc = new XmlDocument();
                        doc.LoadXml(sb.ToString());

                        upgraded = true;
                    }

                    // Programmatic upgrades (which don't change the version number)
                    if (version == "1.3")
                    {
                        UpgradeFrom_v1_3(doc);
                        upgraded = true;
                    }

                    if (version == "1.5")
                    {
                        UpgradeFrom_v1_5(doc);
                        upgraded = true;
                    }

                    if (_upgradeStages.ContainsKey(version))
                    {
                        SetProfileVersionNumber(doc, _upgradeStages[version]);
                        upgraded = true;    
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to upgrade profile to current version", ex);
            }

            return upgraded;
        }

        /// <summary>
        /// Upgrade from version 1.3
        /// </summary>
        /// <param name="doc"></param>
        private void UpgradeFrom_v1_3(XmlDocument doc)
        {            
            // Make region-specific background image and overlay mode apply to all regions
            XmlElement regionsElement = doc.SelectSingleNode("/profile/regions") as XmlElement;
            if (regionsElement != null)
            {
                // Defaults
                string refImage = "";
                string overlayPosition = EDisplayArea.ActiveWindow.ToString();

                // Take attribute values from the first region if present
                XmlElement regionElement = regionsElement.SelectSingleNode("region") as XmlElement;
                if (regionElement != null)
                {
                    if (regionElement.HasAttribute("refimage"))
                    {
                        refImage = regionElement.GetAttribute("refimage");
                    }
                    if (regionElement.HasAttribute("overlayposition"))
                    {
                        overlayPosition = regionElement.GetAttribute("overlayposition");
                    }
                }

                regionsElement.SetAttribute("refimage", refImage);
                regionsElement.SetAttribute("overlayposition", overlayPosition);
            }                      

            // Separate pointerto... actions into separate action lists for each screen region
            XmlNode mappingNode = doc.SelectSingleNode("//mapping");
            XmlNodeList actionLists = mappingNode.SelectNodes("actionlist[@controltype='MousePointer' and @eventdata='0']");
            foreach (XmlElement actionList in actionLists)
            {
                int i = 0;
                while (i < actionList.ChildNodes.Count)
                {
                    XmlElement actionElement = actionList.ChildNodes[i] as XmlElement;
                    if (actionElement != null && actionElement.HasAttribute("regionid"))
                    {
                        // Copy the action list except set the eventdata parameter to be the region ID
                        string regionIDVal = actionElement.GetAttribute("regionid");
                        XmlElement newActionList = doc.CreateElement("actionlist");
                        foreach (XmlAttribute attribute in actionList.Attributes)
                        {
                            newActionList.SetAttribute(attribute.Name, attribute.Value);
                        }
                        newActionList.SetAttribute("eventdata", regionIDVal);
                        if (actionElement.Name == "HoldKeyAction" || actionElement.Name == "ReleaseKeyAction")
                        {
                            newActionList.SetAttribute("eventreason", "Inside");
                        }
                        if (newActionList.GetAttribute("eventreason") == "Moved")
                        {
                            newActionList.SetAttribute("eventreason", "Updated");
                        }

                        // Move the action element to the new action list
                        actionList.RemoveChild(actionElement);
                        newActionList.AppendChild(actionElement);

                        // Add the new action list
                        mappingNode.AppendChild(newActionList);

                        if (actionElement.Name == "HoldKeyAction" &&
                            actionElement.HasAttribute("autorelease") &&
                            bool.Parse(actionElement.GetAttribute("autorelease")))
                        {
                            // Add action list to release the key on leaving the region
                            newActionList = doc.CreateElement("actionlist");
                            foreach (XmlAttribute attribute in actionList.Attributes)
                            {
                                newActionList.SetAttribute(attribute.Name, attribute.Value);
                            }
                            newActionList.SetAttribute("eventdata", regionIDVal);
                            newActionList.SetAttribute("eventreason", "Outside");

                            XmlElement releaseAction = doc.CreateElement("ReleaseKeyAction");
                            foreach (XmlAttribute attribute in actionElement.Attributes)
                            {
                                releaseAction.SetAttribute(attribute.Name, attribute.Value);
                            }

                            newActionList.AppendChild(releaseAction);
                            mappingNode.AppendChild(newActionList);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Upgrade from version 1.5
        /// </summary>
        /// <param name="doc"></param>
        private void UpgradeFrom_v1_5(XmlDocument doc)
        {
            List<Keys> extendedKeys = KeyUtils.GetExtendedKeys();

            // Adjustment to scan codes for certain extended keys
            XmlNodeList baseKeyActionsList = doc.SelectNodes("/profile/mapping/actionlist/*[@scancode]");
            foreach (XmlElement actionElement in baseKeyActionsList)
            {
                ushort scanCode;
                string scanCodeStr = actionElement.GetAttribute("scancode");
                if (ushort.TryParse(scanCodeStr, out scanCode) &&
                    actionElement.HasAttribute("key"))
                {
                    Keys keyCode;
                    string keyCodeStr = actionElement.GetAttribute("key");
                    if (Enum.TryParse<Keys>(keyCodeStr, out keyCode))
                    {
                        switch (keyCode)
                        {
                            case Keys.Pause:
                            case Keys.PrintScreen:
                            case Keys.NumLock:
                                scanCode &= 0xFEFF;     // Clears the 0x0100 bit
                                actionElement.SetAttribute("scancode", scanCode.ToString(CultureInfo.InvariantCulture));
                                break;
                            default:
                                if (extendedKeys.Contains(keyCode))
                                {
                                    scanCode |= 0x0100;     // Sets the 0x0100 bit
                                    actionElement.SetAttribute("scancode", scanCode.ToString(CultureInfo.InvariantCulture));
                                }
                                break;
                        }
                    }
                }
            }
        }
        
        /// <summary>
                 /// Get the version number of a profile
                 /// </summary>
                 /// <param name="filePath"></param>
                 /// <returns></returns>
        private string GetProfileVersionNumber(XmlDocument doc)
        {
            string version = "0.0";

            // Read version
            XmlNode versionNode = doc.SelectSingleNode("/profile/@version");
            if (versionNode != null)
            {
                version = versionNode.Value;
            }
            
            return version;
        }

        /// <summary>
        /// Set the profile version number
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="version"></param>
        private void SetProfileVersionNumber(XmlDocument doc, string version)
        {
            XmlNode versionNode = doc.SelectSingleNode("/profile/@version");
            if (versionNode != null)
            {
                versionNode.Value = version;
            }
        }
    }
}
