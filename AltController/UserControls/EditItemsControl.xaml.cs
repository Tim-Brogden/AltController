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
using AltController.Core;

namespace AltController.UserControls
{
    /// <summary>
    /// Interaction logic for EditItemsControl.xaml
    /// </summary>
    public partial class EditItemsControl : UserControl
    {
        // Members
        private NamedItemList _namedItemList = new NamedItemList();
        private bool _addEnabled = false;
        private bool _editVisible = false;
        private bool _isLoaded = false;

        // Properties
        public NamedItemList NamedItemList { get { return _namedItemList; } set { _namedItemList = value; PopulateItemsList(); } }
        public bool EditVisible
        {
            get { return _editVisible; }
            set
            {
                if (value != _editVisible)
                {
                    _editVisible = value;
                    if (_isLoaded)
                    {
                        EditButton.Visibility = _editVisible ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }
        public bool AddEnabled
        {
            get { return _addEnabled; }
            set
            {
                if (value != _addEnabled)
                {
                    _addEnabled = value;
                    if (_isLoaded)
                    {
                        AddButton.IsEnabled = _addEnabled;
                    }
                }
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _isLoaded ? ItemsList.SelectedIndex : -1;
            }
            set
            {
                if (_isLoaded)
                {
                    ItemsList.SelectedIndex = value;
                }
            }
        }

        // Events
        public event RoutedEventHandler AddClicked;
        public event RoutedEventHandler EditClicked;
        public event RoutedEventHandler DeleteClicked;
        public event RoutedEventHandler MoveUpClicked;
        public event RoutedEventHandler MoveDownClicked;
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public EditItemsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Add a new item to the list
        /// </summary>
        /// <param name="newItem"></param>
        public void AddItem(NamedItem newItem)
        {
            _namedItemList.Add(newItem);

            // Select new item
            if (this.IsLoaded)
            {
                this.ItemsList.SelectedIndex = _namedItemList.Count - 1;
            }
        }

        /// <summary>
        /// Get the currently selected item
        /// </summary>
        /// <returns></returns>
        public NamedItem GetSelectedItem()
        {
            NamedItem selectedItem = null;
            int selectedIndex = this.ItemsList.SelectedIndex;
            if (selectedIndex > -1)
            {
                selectedItem = _namedItemList[selectedIndex];
            }

            return selectedItem;
        }

        /// <summary>
        /// Loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;

            // Configure buttons
            AddButton.IsEnabled = _addEnabled;
            EditButton.Visibility = _editVisible ? Visibility.Visible : Visibility.Collapsed;

            // Populate items list
            PopulateItemsList();
        }

        /// <summary>
        /// Selected item changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Pass the event up to the parent
            if (SelectionChanged != null)
            {
                SelectionChanged(sender, e);
            }

            NamedItem selectedItem = (NamedItem)this.ItemsList.SelectedItem;
            if (selectedItem != null)
            {
                // Enable editing unless it's the default item in the list
                this.EditButton.IsEnabled = (selectedItem.ID != Constants.DefaultID);
                this.DeleteButton.IsEnabled = (selectedItem.ID != Constants.DefaultID);

                // Enable move up and down buttons if reqd
                this.MoveUpButton.IsEnabled = (this.ItemsList.SelectedIndex > 0);
                this.MoveDownButton.IsEnabled = (this.ItemsList.SelectedIndex < _namedItemList.Count - 1);
            }
            else
            {
                // Disable delete button
                this.EditButton.IsEnabled = false;
                this.DeleteButton.IsEnabled = false;

                // Disable move up and down buttons
                this.MoveUpButton.IsEnabled = false;
                this.MoveDownButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Add button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddClicked != null)
            {
                AddClicked(sender, e);
            }
        }

        /// <summary>
        /// Edit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditClicked != null)
            {
                EditClicked(sender, e);
            }
        }

        /// <summary>
        /// Delete button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteClicked != null)
            {
                DeleteClicked(sender, e);
            }

            int selectedIndex = this.ItemsList.SelectedIndex;
            if (selectedIndex > -1 && selectedIndex < _namedItemList.Count)
            {
                // Remove selected
                _namedItemList.RemoveAt(selectedIndex);

                // Select item
                if (selectedIndex < _namedItemList.Count)
                {
                    this.ItemsList.SelectedIndex = selectedIndex;
                }
                else if (selectedIndex > 0)
                {
                    this.ItemsList.SelectedIndex = selectedIndex - 1;
                }
            }
        }

        /// <summary>
        /// Move up button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (MoveUpClicked != null)
            {
                MoveUpClicked(sender, e);
            }

            int selectedIndex = this.ItemsList.SelectedIndex;
            if (selectedIndex > 0 && selectedIndex < _namedItemList.Count)
            {
                // Remove selected
                NamedItem selectedItem = _namedItemList[selectedIndex];
                _namedItemList.RemoveAt(selectedIndex);
                _namedItemList.Insert(selectedIndex - 1, selectedItem);

                // Select moved item
                this.ItemsList.SelectedIndex = selectedIndex - 1;
            }
        }

        /// <summary>
        /// Move down button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (MoveDownClicked != null)
            {
                MoveDownClicked(sender, e);
            }

            int selectedIndex = this.ItemsList.SelectedIndex;
            if (selectedIndex > -1 && selectedIndex < _namedItemList.Count - 1)
            {
                // Remove selected
                NamedItem selectedItem = _namedItemList[selectedIndex];
                _namedItemList.RemoveAt(selectedIndex);
                _namedItemList.Insert(selectedIndex + 1, selectedItem);

                // Select moved item
                this.ItemsList.SelectedIndex = selectedIndex + 1;
            }
        }

        /// <summary>
        /// Populate the list of items
        /// </summary>
        private void PopulateItemsList()
        {
            if (this.IsLoaded)
            {
                // Bind
                this.ItemsList.ItemsSource = _namedItemList;

                // Select first item
                if (_namedItemList.Count > 0)
                {
                    this.ItemsList.SelectedIndex = 0;
                }
            }
        }

    }
}
