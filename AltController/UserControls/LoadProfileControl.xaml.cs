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
using System.Windows;
using System.Windows.Controls;
using System.IO;
using AltController.Actions;
using AltController.Config;
using AltController.Core;

namespace AltController.UserControls
{
    /// <summary>
    /// Control for editing Load profile actions
    /// </summary>
    public partial class LoadProfileControl : UserControl
    {
        // Fields
        private NamedItemList _profileListItems = new NamedItemList();
        private LoadProfileAction _currentAction = new LoadProfileAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public LoadProfileControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Control loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ProfileNameCombo.ItemsSource = _profileListItems;

            RefreshDisplay();
        }

        /// <summary>
        /// Populate the list of profiles
        /// </summary>
        public void SetAppConfig(AppConfig appConfig)
        {
            // Add an option to load a new profile
            _profileListItems.Clear();
            _profileListItems.Add(new NamedItem(Constants.NoneID, Properties.Resources.String_NewProfileOption));

            // Get profiles directory
            string defaultProfilesDir = Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
            string profilesDir = appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);

            // Add profiles in profiles directory
            DirectoryInfo dirInfo = new DirectoryInfo(profilesDir);
            if (dirInfo.Exists)
            {
                // Find profiles in directory
                FileInfo[] fileList = dirInfo.GetFiles("*" + Constants.ProfileFileExtension);

                // Get list of file names
                for (int i = 0; i < fileList.Length; i++)
                {
                    string fileName = fileList[i].Name;
                    if (fileName.EndsWith(Constants.ProfileFileExtension))
                    {
                        fileName = fileName.Substring(0, fileName.Length - Constants.ProfileFileExtension.Length);
                    }
                    _profileListItems.Add(new NamedItem(i + 1, fileName));
                }
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is LoadProfileAction)
            {
                _currentAction = (LoadProfileAction)action;
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Redisplay the action
        /// </summary>
        private void RefreshDisplay()
        {
            if (IsLoaded && _currentAction != null)
            {
                long id = Constants.NoneID;
                if (!string.IsNullOrEmpty(_currentAction.ProfileName))
                {
                    NamedItem item = _profileListItems.GetItemByName(_currentAction.ProfileName);
                    if (item != null)
                    {
                        id = item.ID;
                    }
                }
                this.ProfileNameCombo.SelectedValue = id;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem profileNameItem = (NamedItem)this.ProfileNameCombo.SelectedItem;
            if (profileNameItem != null)
            {
                _currentAction = new LoadProfileAction();
                _currentAction.ProfileName = profileNameItem.ID != Constants.NoneID ? profileNameItem.Name : "";
            }

            return _currentAction;
        }
    }
}
