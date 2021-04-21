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
using System.Reflection;
using AltController.Core;
using AltController.Input;

namespace AltController.Config
{
    /// <summary>
    /// Configuration profile that maps input sources and controls to actions,
    /// according to the current logical state (mode, application and page)
    /// </summary>
    public class Profile
    {
        private string _profileName;
        //private string _directory;
        private string _profileNotes;
        private NamedItemList _modeDetails;
        private NamedItemList _appDetails;
        private NamedItemList _pageDetails;
        private NamedItemList _inputSources;
        private ScreenRegionList _screenRegions;
        private Dictionary<long, ModeMappingTable> _modeMappings;

        public string Name { get { return _profileName; } set { _profileName = value; } }
        public string ProfileNotes { get { return _profileNotes; } set { _profileNotes = value; } }
        public NamedItemList ModeDetails { get { return _modeDetails; } set { _modeDetails = value; } }
        public NamedItemList AppDetails { get { return _appDetails; } set { _appDetails = value; } }
        public NamedItemList PageDetails { get { return _pageDetails; } set { _pageDetails = value;  } }
        public ScreenRegionList ScreenRegions { get { return _screenRegions; } set { _screenRegions = value; } }
        public NamedItemList InputSources 
        { 
            get { return _inputSources; } 
            set 
            { 
                _inputSources = value;
                foreach (BaseSource source in _inputSources)
                {
                    source.Profile = this;
                }
            } 
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Profile()
        {
            _profileName = Properties.Resources.String_NewProfile;
            _profileNotes = "";

            _modeDetails = new NamedItemList();
            _appDetails = new NamedItemList();
            _pageDetails = new NamedItemList();
            _inputSources = new NamedItemList();
            _screenRegions = new ScreenRegionList();
            _modeMappings = new Dictionary<long, ModeMappingTable>();

            // Create default mode, app, page
            _modeDetails.Add(new NamedItem(Constants.DefaultID, Properties.Resources.String_Default));
            _appDetails.Add(new NamedItem(Constants.DefaultID, Properties.Resources.String_Default));
            _pageDetails.Add(new NamedItem(Constants.DefaultID, Properties.Resources.String_Default));
            _modeMappings[Constants.DefaultID] = new ModeMappingTable(Constants.DefaultID);
        }

        /// <summary>
        /// Initialise from a file
        /// </summary>
        /// <param name="filePath"></param>
        public bool FromFile(string filePath)
        {
            // Read file
            string xml = File.ReadAllText(filePath, Encoding.UTF8);
            
            // Create xml doc
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            // Upgrade if necessary
            ProfileUpgrader upgrader = new ProfileUpgrader();
            bool upgraded = upgrader.Upgrade(ref doc);

            // Parse and validate
            FromXml(doc);
            Validate();

            return upgraded;
        }
        
        /// <summary>
        /// Initialise from an xml string
        /// </summary>
        /// <param name="xml"></param>
        public void FromXml(XmlDocument doc)
        {
            try
            {
                // Read notes
                XmlNode notesNode = doc.SelectSingleNode("/profile/notes");
                if (notesNode != null)
                {
                    _profileNotes = notesNode.InnerText;
                }

                // Read sources
                XmlElement sourcesElement = doc.SelectSingleNode("/profile/sources") as XmlElement;
                if (sourcesElement != null && sourcesElement.HasChildNodes)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    foreach (XmlNode node in sourcesElement.ChildNodes)
                    {
                        if (node is XmlElement)
                        {
                            string typeFullName = string.Format("{0}.{1}", typeof(BaseSource).Namespace, node.Name);
                            if (assembly.GetType(typeFullName, false, false) != null)
                            {
                                BaseSource source = (BaseSource)assembly.CreateInstance(typeFullName);
                                if (source != null)
                                {
                                    source.FromXml((XmlElement)node);
                                    AddInput(source);
                                }
                            }
                        }
                    }
                }

                // Read screen regions
                XmlElement regionsElement = doc.SelectSingleNode("/profile/regions") as XmlElement;
                if (regionsElement != null)
                {
                    _screenRegions.FromXml(regionsElement);
                }
                
                // Read modes, apps and pages
                ReadNameValueDetailsList(_modeDetails, doc.SelectNodes("/profile/modes/mode"));
                ReadNameValueDetailsList(_appDetails, doc.SelectNodes("/profile/apps/app"));
                ReadNameValueDetailsList(_pageDetails, doc.SelectNodes("/profile/pages/page"));

                // Read action lists
                XmlNodeList actionLists = doc.SelectNodes("/profile/mapping/actionlist");
                if (actionLists != null)
                {
                    foreach (XmlElement actionListElement in actionLists)
                    {
                        ActionList actionList = new ActionList();
                        actionList.FromXml(actionListElement);
                        if (actionList.Count != 0)
                        {
                            ActionMappingTable actionMappings = GetActionsForState(actionList.LogicalState, false);
                            actionMappings.SetActions(actionList.EventArgs.ToID(), actionList);
                        }
                    }
                }

                // Set the numbering
                RenumberActionLists();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to load profile from Xml", ex);
            }
        }

        /// <summary>
        /// Add an input source to the profile
        /// </summary>
        /// <param name="source"></param>
        public void AddInput(BaseSource source)
        {
            source.Profile = this;
            _inputSources.Add(source);
        }

        /// <summary>
        /// Update an input source
        /// </summary>
        /// <param name="index"></param>
        /// <param name="source"></param>
        public void UpdateInput(int index, BaseSource source)
        {
            if (index > -1 && index < _inputSources.Count)
            {
                source.Profile = this;
                _inputSources[index] = source;
            }
        }

        /// <summary>
        /// Write profile to a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="profile"></param>
        public void ToFile(string filePath)
        {
            // Convert to xml
            XmlDocument doc = ToXml();

            // Write to file
            StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            XmlWriter xWriter = XmlTextWriter.Create(sw, settings);
            doc.Save(xWriter);
            sw.Close();
        }

        /// <summary>
        /// Convert to a string representation
        /// </summary>
        public XmlDocument ToXml()
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                // Root node
                XmlElement profileElement = doc.CreateElement("profile");
                doc.AppendChild(profileElement);

                // Version
                profileElement.SetAttribute("version", Constants.FileVersion);

                // Notes
                XmlElement notesElement = doc.CreateElement("notes");
                notesElement.InnerText = _profileNotes;
                profileElement.AppendChild(notesElement);

                // Sources
                XmlElement sourcesElement = doc.CreateElement("sources");
                foreach (BaseSource source in _inputSources)
                {
                    XmlElement element = doc.CreateElement(source.GetType().Name);
                    source.ToXml(element, doc);
                    sourcesElement.AppendChild(element);
                }
                profileElement.AppendChild(sourcesElement);

                // Screen regions
                XmlElement regionsElement = doc.CreateElement("regions");
                _screenRegions.ToXml(regionsElement, doc);
                profileElement.AppendChild(regionsElement);

                // Modes, apps and pages nodes
                XmlElement modesElement = doc.CreateElement("modes");
                WriteNameValueDetailsList(modesElement, _modeDetails, "mode", doc);
                profileElement.AppendChild(modesElement);
                XmlElement appsElement = doc.CreateElement("apps");
                WriteNameValueDetailsList(appsElement, _appDetails, "app", doc);
                profileElement.AppendChild(appsElement);
                XmlElement pagesElement = doc.CreateElement("pages");
                WriteNameValueDetailsList(pagesElement, _pageDetails, "page", doc);
                profileElement.AppendChild(pagesElement);

                // Mapping node
                XmlElement mappingElement = doc.CreateElement("mapping");
                foreach (ModeMappingTable modeMappings in _modeMappings.Values)
                {
                    // Loop through app mappings
                    Dictionary<long, AppMappingTable>.Enumerator eAppMappings = modeMappings.GetEnumerator();
                    while (eAppMappings.MoveNext())
                    {
                        // Loop through page mappings
                        AppMappingTable appMappings = eAppMappings.Current.Value;
                        Dictionary<long, ActionMappingTable>.Enumerator ePageMappings = appMappings.GetEnumerator();
                        while (ePageMappings.MoveNext())
                        {
                            // Loop through event mappings
                            ActionMappingTable actionMappings = ePageMappings.Current.Value;
                            Dictionary<long, ActionList>.Enumerator eActionMappings = actionMappings.GetEnumerator();
                            while (eActionMappings.MoveNext())
                            {
                                ActionList actionList = eActionMappings.Current.Value;
                                if (actionList.Count > 0)
                                {
                                    XmlElement actionListElement = doc.CreateElement("actionlist");
                                    actionList.ToXml(actionListElement, doc);
                                    mappingElement.AppendChild(actionListElement);
                                }
                            }
                        }
                    }
                }
                profileElement.AppendChild(mappingElement);

                return doc;
            }
            catch (Exception)
            {
                throw new Exception("Unable to write profile to Xml");
            }
        }

        /// <summary>
        /// Read name value items from a list of xml nodes
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="nameValueNodeList"></param>
        private void ReadNameValueDetailsList(NamedItemList itemList, XmlNodeList nameValueNodeList)
        {
            itemList.Clear();
            if (nameValueNodeList != null)
            {
                foreach (XmlNode node in nameValueNodeList)
                {
                    if (node is XmlElement)
                    {
                        NamedItem item = new NamedItem();
                        item.FromXml((XmlElement)node);

                        itemList.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Write a list of name value items to an xml node
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="nameValueItems"></param>
        /// <param name="elementName"></param>
        /// <param name="doc"></param>
        private void WriteNameValueDetailsList(XmlElement parentElement,
                                                NamedItemList nameValueItems,
                                                string elementName,
                                                XmlDocument doc)
        {
            if (nameValueItems != null)
            {
                foreach (NamedItem item in nameValueItems)
                {
                    XmlElement itemElement = doc.CreateElement(elementName);
                    parentElement.AppendChild(itemElement);

                    item.ToXml(itemElement, doc);
                }
            }
        }

        /// <summary>
        /// Get the details for specified mode
        /// </summary>
        /// <param name="modeID"></param>
        /// <returns></returns>
        public NamedItem GetModeDetails(long modeID)
        {
            return ModeDetails.GetItemByID(modeID);
        }

        /// <summary>
        /// Get the details for specified app
        /// </summary>
        /// <param name="appID"></param>
        /// <returns></returns>
        public NamedItem GetAppDetails(long appID)
        {
            return AppDetails.GetItemByID(appID);
        }

        /// <summary>
        /// Get the details for the specified app name
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public NamedItem GetAppDetails(string appName)
        {
            // Make it case insensitive
            string appNameLower = appName.ToLower();

            NamedItem appDetails = null;
            foreach (NamedItem item in AppDetails)
            {
                if (item.Name.ToLower() == appNameLower)
                {
                    appDetails = item;
                    break;
                }
            }

            return appDetails;
        }

        /// <summary>
        /// Get the details for specified page
        /// </summary>
        /// <param name="pageID"></param>
        /// <returns></returns>
        public NamedItem GetPageDetails(long pageID)
        {
            return PageDetails.GetItemByID(pageID);
        }

        /// <summary>
        /// Get the specified input source
        /// </summary>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        public BaseSource GetInputSource(long sourceID)
        {
            return (BaseSource)InputSources.GetItemByID(sourceID);
        }

        /// <summary>
        /// Get the first source of the specified type
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public BaseSource GetInputSource(ESourceType sourceType)
        {
            BaseSource matchedSource = null;
            foreach (BaseSource source in _inputSources)
            {
                if (source.SourceType == sourceType)
                {
                    matchedSource = source;
                    break;
                }
            }

            return matchedSource;
        }

        /// <summary>
        /// Return the actions for the specified logical state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionMappingTable GetActionsForState(LogicalState state, bool includeDefaults)
        {
            ActionMappingTable matchedMappings = null;
            if (_modeMappings.ContainsKey(state.ModeID))
            {
                matchedMappings = _modeMappings[state.ModeID].GetActionsForState(state, includeDefaults);
            }

            ActionMappingTable table;
            if (includeDefaults)
            {
                // Merge in default actions
                ActionMappingTable defaultActions = _modeMappings[Constants.DefaultID].GetActionsForState(state, includeDefaults);
                table = ActionMappingTable.Combine(matchedMappings, defaultActions);
            }
            else
            {
                // Create new mode if required
                if (matchedMappings == null)
                {
                    ModeMappingTable modeMapping = new ModeMappingTable(state.ModeID);
                    _modeMappings[state.ModeID] = modeMapping;
                    matchedMappings = modeMapping.GetActionsForState(state, includeDefaults);
                }

                table = matchedMappings;
            }

            return table;
        }

        /// <summary>
        /// Get the action lists for a particular control type
        /// </summary>
        /// <returns></returns>
        public ActionMappingTable GetActionsForControlType(EControlType controlType)
        {
            ActionMappingTable table = new ActionMappingTable();
            foreach (ModeMappingTable modeMappings in _modeMappings.Values)
            {
                // Loop through app mappings
                Dictionary<long, AppMappingTable>.Enumerator eAppMappings = modeMappings.GetEnumerator();
                while (eAppMappings.MoveNext())
                {
                    // Loop through page mappings
                    AppMappingTable appMappings = eAppMappings.Current.Value;
                    Dictionary<long, ActionMappingTable>.Enumerator ePageMappings = appMappings.GetEnumerator();
                    while (ePageMappings.MoveNext())
                    {
                        // Loop through event mappings
                        ActionMappingTable actionMappings = ePageMappings.Current.Value;
                        Dictionary<long, ActionList>.Enumerator eActionMappings = actionMappings.GetEnumerator();
                        while (eActionMappings.MoveNext())
                        {
                            ActionList actionList = eActionMappings.Current.Value;
                            if (actionList.EventArgs.ControlType == controlType)
                            {
                                table.SetActions(eActionMappings.Current.Key, actionList);
                            }
                        }
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Reassign ID numbers to action lists (call after action lists have been added or deleted)
        /// </summary>
        public void RenumberActionLists()
        {
            int id = 0;
            foreach (ModeMappingTable modeMappings in _modeMappings.Values)
            {
                // Loop through app mappings
                Dictionary<long, AppMappingTable>.Enumerator eAppMappings = modeMappings.GetEnumerator();
                while (eAppMappings.MoveNext())
                {
                    // Loop through page mappings
                    AppMappingTable appMappings = eAppMappings.Current.Value;
                    Dictionary<long, ActionMappingTable>.Enumerator ePageMappings = appMappings.GetEnumerator();
                    while (ePageMappings.MoveNext())
                    {
                        // Loop through event mappings
                        ActionMappingTable actionMappings = ePageMappings.Current.Value;
                        Dictionary<long, ActionList>.Enumerator eActionMappings = actionMappings.GetEnumerator();
                        while (eActionMappings.MoveNext())
                        {
                            ActionList actionList = eActionMappings.Current.Value;
                            if (actionList.Count != 0)
                            {
                                actionList.ID = ++id;
                            }
                            else
                            {
                                actionList.ID = 0;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the action list with the specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionList GetActionListByID(int id)
        {
            foreach (ModeMappingTable modeMappings in _modeMappings.Values)
            {
                // Loop through app mappings
                Dictionary<long, AppMappingTable>.Enumerator eAppMappings = modeMappings.GetEnumerator();
                while (eAppMappings.MoveNext())
                {
                    // Loop through page mappings
                    AppMappingTable appMappings = eAppMappings.Current.Value;
                    Dictionary<long, ActionMappingTable>.Enumerator ePageMappings = appMappings.GetEnumerator();
                    while (ePageMappings.MoveNext())
                    {
                        // Loop through event mappings
                        ActionMappingTable actionMappings = ePageMappings.Current.Value;
                        Dictionary<long, ActionList>.Enumerator eActionMappings = actionMappings.GetEnumerator();
                        while (eActionMappings.MoveNext())
                        {
                            ActionList actionList = eActionMappings.Current.Value;
                            if (actionList.ID == id)
                            {
                                return actionList;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Count the action lists in the profile
        /// </summary>
        /// <returns></returns>
        public int GetNumActionLists()
        {
            int count = 0;
            foreach (ModeMappingTable modeMappings in _modeMappings.Values)
            {
                // Loop through app mappings
                Dictionary<long, AppMappingTable>.Enumerator eAppMappings = modeMappings.GetEnumerator();
                while (eAppMappings.MoveNext())
                {
                    // Loop through page mappings
                    AppMappingTable appMappings = eAppMappings.Current.Value;
                    Dictionary<long, ActionMappingTable>.Enumerator ePageMappings = appMappings.GetEnumerator();
                    while (ePageMappings.MoveNext())
                    {
                        // Loop through event mappings
                        ActionMappingTable actionMappings = ePageMappings.Current.Value;
                        Dictionary<long, ActionList>.Enumerator eActionMappings = actionMappings.GetEnumerator();
                        while (eActionMappings.MoveNext())
                        {
                            ActionList actionList = eActionMappings.Current.Value;
                            if (actionList.Count != 0)
                            {
                                count++;
                            }
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Validate the profile by removing any invalid mappings 
        /// and ensuring that mode / page / region names are consistent
        /// </summary>
        public void Validate()
        {
            // Screen regions
            ValidateScreenRegions();

            // Modes
            ValidateModeMappings();

            foreach (ModeMappingTable modeMapping in _modeMappings.Values)
            {
                // Apps
                modeMapping.ValidateApps(this);

                Dictionary<long, AppMappingTable>.Enumerator appEnum = modeMapping.GetEnumerator();
                while (appEnum.MoveNext())
                {
                    // Pages
                    AppMappingTable appMapping = appEnum.Current.Value;
                    appMapping.ValidatePages(this);

                    Dictionary<long, ActionMappingTable>.Enumerator pageEnum = appMapping.GetEnumerator();
                    while (pageEnum.MoveNext())
                    {
                        // Actions
                        ActionMappingTable pageMapping = pageEnum.Current.Value;
                        pageMapping.ValidateEventTypes(this);
                        pageMapping.ValidateActions(this);
                    }
                }
            }

            // Renumber the action lists in case any have been deleted
            RenumberActionLists();
        }

        /// <summary>
        /// Remove any invalid mode mappings
        /// </summary>
        private void ValidateModeMappings()
        {
            // Delete invalid items
            long itemToDelete;
            do
            {
                // See if any item doesn't have corresponding mode details
                itemToDelete = -1;
                foreach (long itemID in _modeMappings.Keys)
                {
                    if (GetModeDetails(itemID) == null)
                    {
                        itemToDelete = itemID;
                        break;
                    }
                }

                if (itemToDelete > -1)
                {
                    // Delete item
                    _modeMappings.Remove(itemToDelete);
                }
            }
            while (itemToDelete > -1);
        }

        /// <summary>
        /// Validate screen regions
        /// </summary>
        private void ValidateScreenRegions()
        {
            foreach (ScreenRegion region in _screenRegions)
            {
                // If screen region is set to be displayed for a mode or app that has been deleted, set it's display options to None
                if (region.ShowInState.ModeID > 0 && GetModeDetails(region.ShowInState.ModeID) == null)
                {
                    region.ShowInState.ModeID = Constants.NoneID;
                }
                if (region.ShowInState.AppID > 0 && GetAppDetails(region.ShowInState.AppID) == null)
                {
                    region.ShowInState.AppID = Constants.NoneID;
                }
            }            
        }

    }
}
