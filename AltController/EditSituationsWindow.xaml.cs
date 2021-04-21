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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using AltController.Core;
using AltController.Sys;

namespace AltController
{
    /// <summary>
    /// Edit situations window
    /// </summary>
    public partial class EditSituationsWindow : Window
    {
        // Members
        private NamedItemList _appDetailsList = new NamedItemList();
        private NamedItemList _openAppsList = new NamedItemList();
        private List<int> _processIDs = new List<int>();
        private Dictionary<int, string> _processesTable = new Dictionary<int, string>();        

        /// <summary>
        /// Constructor
        /// </summary>
        public EditSituationsWindow()
        {
            InitializeComponent();

            this.EditModesControl.SelectionChanged += new SelectionChangedEventHandler(EditModesControl_SelectionChanged);
            this.EditModesControl.AddClicked += new RoutedEventHandler(EditModesControl_AddClicked);
            this.EditPagesControl.SelectionChanged += new SelectionChangedEventHandler(EditPagesControl_SelectionChanged);
            this.EditPagesControl.AddClicked += new RoutedEventHandler(EditPagesControl_AddClicked);
        }

        // Get / set the data to edit
        public NamedItemList ModeDetailsList { get { return EditModesControl.NamedItemList; } set { EditModesControl.NamedItemList = value; } }
        public NamedItemList AppDetailsList { get { return _appDetailsList; } set { _appDetailsList = value; PopulateAppList(); } }
        public NamedItemList PageDetailsList { get { return EditPagesControl.NamedItemList; } set { EditPagesControl.NamedItemList = value; } }

        /// <summary>
        /// Loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Bind
            this.AppCombo.ItemsSource = _openAppsList;

            // Populate apps combo
            PopulateAppCombo();

            // Populate apps list
            PopulateAppList();
        }

        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Check that no app appears twice
            bool duplicatesFound = false;
            Dictionary<string, bool> distinctAppNames = new Dictionary<string, bool>();
            foreach (NamedItem item in _appDetailsList)
            {
                if (distinctAppNames.ContainsKey(item.Name))
                {
                    duplicatesFound = true;
                    break;
                }
                else
                {
                    distinctAppNames[item.Name] = true;
                }
            }

            if (duplicatesFound)
            {
                // Error
                MessageBox.Show(Properties.Resources.String_Duplicate_app_description,
                                Properties.Resources.String_Duplicate_app_title,
                                MessageBoxButton.OK, 
                                MessageBoxImage.Information);
            }
            else
            {
                // OK
                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Refresh the app combo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateAppCombo();
        }

        /// <summary>
        /// Add mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditModesControl_AddClicked(object sender, RoutedEventArgs e)
        {
            long id = ModeDetailsList.GetFirstUnusedID();
            string name = ModeDetailsList.GetFirstUnusedName(Properties.Resources.String_Mode, false);
            NamedItem newItem = new NamedItem(id, name);
            this.EditModesControl.AddItem(newItem);
        }

        /// <summary>
        /// Add app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddAppButton_Click(object sender, RoutedEventArgs e)
        {
            NamedItem selectedApp = (NamedItem)this.AppCombo.SelectedItem;
            if (selectedApp != null)
            {
                // Add the currently selected app
                string appName = selectedApp.Name;
                long id = _appDetailsList.GetFirstUnusedID();

                NamedItem newItem = new NamedItem(id, appName);
                _appDetailsList.Add(newItem);

                // Select the new app
                this.AppList.SelectedIndex = _appDetailsList.Count - 1;
            }
        }

        /// <summary>
        /// App deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteAppButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = this.AppList.SelectedIndex;
            if (selectedIndex > -1 && selectedIndex < _appDetailsList.Count)
            {                
                // Remove selected
                _appDetailsList.RemoveAt(selectedIndex);

                // Select item
                if (selectedIndex < _appDetailsList.Count)
                {
                    this.AppList.SelectedIndex = selectedIndex;
                }
                else if (selectedIndex > 0)
                {
                    this.AppList.SelectedIndex = selectedIndex - 1;
                }
            }
        }    

        /// <summary>
        /// Add page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditPagesControl_AddClicked(object sender, RoutedEventArgs e)
        {
            long id = PageDetailsList.GetFirstUnusedID();
            string name = PageDetailsList.GetFirstUnusedName(Properties.Resources.String_Page, false);
            NamedItem newItem = new NamedItem(id, name);
            this.EditPagesControl.AddItem(newItem);
        }

        /// <summary>
        /// Selected mode changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditModesControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NamedItem selectedItem = this.EditModesControl.GetSelectedItem();
            if (selectedItem != null)
            {
                this.ModeNameText.Text = selectedItem.Name;
            }
            this.ModeNameText.IsEnabled = (selectedItem != null) && (selectedItem.ID != Constants.DefaultID);
        }

        /// <summary>
        /// Selected list item changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NamedItem selectedItem = (NamedItem)this.AppList.SelectedItem;
            if (selectedItem != null && selectedItem.ID != Constants.DefaultID)
            {
                // Select the app in the list
                long id = selectedItem.Name.ToLower().GetHashCode();
                this.AppCombo.SelectedValue = id;

                this.DeleteAppButton.IsEnabled = true;
            }
            else
            {
                this.DeleteAppButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Selected app changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NamedItem selectedItem = (NamedItem)this.AppList.SelectedItem;
            NamedItem selectedApp = (NamedItem)this.AppCombo.SelectedItem;
            if (selectedItem != null && selectedItem.ID != Constants.DefaultID &&
                selectedApp != null && selectedApp.Name != selectedItem.Name)
            {
                selectedItem.Name = selectedApp.Name;
            }
        }

        /// <summary>
        /// Selected page changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditPagesControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NamedItem selectedItem = this.EditPagesControl.GetSelectedItem();
            if (selectedItem != null)
            {
                this.PageNameText.Text = selectedItem.Name;
            }
            this.PageNameText.IsEnabled = (selectedItem != null) && (selectedItem.ID != Constants.DefaultID);
        }

        /// <summary>
        /// Rename selected mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModeNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            NamedItem selectedItem = this.EditModesControl.GetSelectedItem();
            if (selectedItem != null && this.ModeNameText.Text != "" && this.ModeNameText.Text != selectedItem.Name)
            {
                selectedItem.Name = this.ModeNameText.Text;
            }
        }

        /// <summary>
        /// Rename selected page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            NamedItem selectedItem = this.EditPagesControl.GetSelectedItem();
            if (selectedItem != null && this.PageNameText.Text != "" && this.PageNameText.Text != selectedItem.Name)
            {
                selectedItem.Name = this.PageNameText.Text;
            }
        }

        /// <summary>
        /// Populate the apps combo
        /// </summary>
        private void PopulateAppCombo()
        {
            // Add the names of active processes
            try
            {
                _processesTable.Clear();
                _processIDs.Clear();
                CallBackPtr callBack = new CallBackPtr(this.EnumWindowsCallback);
                WindowsAPI.EnumWindows(callBack, 0);
            }
            catch (Exception)
            {
                // Ignore errors
            }

            // Add profile's apps too
            foreach (NamedItem item in _appDetailsList)
            {
                if (item.ID != Constants.DefaultID)
                {
                    int id = item.Name.ToLower().GetHashCode();
                    _processesTable[id] = item.Name;
                }
            }

            // Get the current selection, if any
            long selectedID = 0L;
            if (AppCombo.SelectedItem != null)
            {
                selectedID = ((NamedItem)AppCombo.SelectedItem).ID;
            }

            // Populate combo
            _openAppsList.Clear();
            string[] processNames = _processesTable.Values.ToArray();
            Array.Sort(processNames);
            foreach (string processName in processNames)
            {
                int id = processName.ToLower().GetHashCode();
                _openAppsList.Add(new NamedItem(id, processName));
            }

            // Select same app, or else the first one
            if (selectedID != 0 && _openAppsList.GetItemByID(selectedID) != null)
            {
                this.AppCombo.SelectedValue = selectedID;
            }
            else if (_openAppsList.Count != 0)
            {
                this.AppCombo.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Callback for EnumWindows call
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private bool EnumWindowsCallback(int hWnd, int lParam)
        {
            try
            {
                // Get window's process ID
                int processID;
                WindowsAPI.GetWindowThreadProcessId((IntPtr)hWnd, out processID);
                if (!_processIDs.Contains(processID))
                {
                    // Check that it's a relevant type of window
                    if (WindowsAPI.IsWindowVisible((IntPtr)hWnd))
                    {
                        _processIDs.Add(processID);

                        // Get process name if not already in table
                        Process process = Process.GetProcessById(processID);
                        if (process != null)
                        {
                            int id = process.ProcessName.ToLower().GetHashCode();
                            if (!_processesTable.ContainsKey(id))
                            {
                                _processesTable[id] = process.ProcessName;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }
            
            return true;
        }

        /// <summary>
        /// Populate the list of apps
        /// </summary>
        private void PopulateAppList()
        {
            if (this.IsLoaded)
            {
                // Bind
                this.AppList.ItemsSource = _appDetailsList;

                // Select first item
                if (_appDetailsList.Count > 0)
                {
                    this.AppList.SelectedIndex = 0;
                }
            }
        }
    }
}
