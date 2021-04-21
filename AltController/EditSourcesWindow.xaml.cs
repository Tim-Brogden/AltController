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
using System.Windows;
using System.Windows.Controls;
using AltController.Core;
using AltController.Input;

namespace AltController
{
    /// <summary>
    /// Edit input sources window
    /// </summary>
    public partial class EditSourcesWindow : Window
    {
        private NamedItemList _inputSourceList;
        private NamedItemList _sourceTypeList = new NamedItemList();

        public NamedItemList InputSources { get { return _inputSourceList; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="itemTypeName"></param>
        public EditSourcesWindow(NamedItemList itemList)
        {
            _inputSourceList = itemList;

            InitializeComponent();
        }

        /// <summary>
        /// Window loaded event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate sources combo
            PopulateSourceTypeCombo();

            // Populate items list
            PopulateSourcesList();
        }

        /// <summary>
        /// Selected item changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SourcesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Enable / disable buttons
            BaseSource source = (BaseSource)this.SourcesList.SelectedItem;
            if (source != null)
            {
                this.DeleteButton.IsEnabled = true;
            }
            else
            {
                this.DeleteButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Add button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected type of source
            NamedItem selectedItem = (NamedItem)this.SourceTypeCombo.SelectedItem;
            if (selectedItem != null)
            {
                string nameStem = selectedItem.Name;
                long id = _inputSourceList.GetFirstUnusedID();
                string name = _inputSourceList.GetFirstUnusedName(nameStem, true);

                // Count the number of sources of this type (including the new one)
                ESourceType sourceType = (ESourceType)selectedItem.ID;
                int typeCount = 1;
                foreach (BaseSource source in _inputSourceList)
                {
                    if (source.SourceType == sourceType)
                    {
                        typeCount++;
                    }
                }

                // Create new source
                BaseSource newItem = null;
                switch (sourceType)
                {
                    case ESourceType.Mouse:
                        if (typeCount == 1)
                        {
                            newItem = new MouseSource(id, name);
                        }
                        else
                        {
                            MessageBox.Show(Properties.Resources.String_Add_mouse_error_description, Properties.Resources.String_Add_input_error_title, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        break;
                    case ESourceType.Keyboard:
                        if (typeCount == 1)
                        {
                            newItem = new KeyboardSource(id, name);
                        }
                        else
                        {
                            MessageBox.Show(Properties.Resources.String_Add_keyboard_error_description, Properties.Resources.String_Add_input_error_title, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        break;
                    case ESourceType.CustomWindow:
                        newItem = new CustomWindowSource(id, name);
                        break;
                }

                if (newItem != null)
                {
                    // Add to list
                    _inputSourceList.Add(newItem);

                    // Select new item
                    this.SourcesList.SelectedIndex = _inputSourceList.Count - 1;
                }
            }
        }

        /// <summary>
        /// Delete button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = this.SourcesList.SelectedIndex;
            if (selectedIndex > -1 && selectedIndex < _inputSourceList.Count)
            {
                // Remove selected
                _inputSourceList.RemoveAt(selectedIndex);

                // Select item
                if (selectedIndex < _inputSourceList.Count)
                {
                    this.SourcesList.SelectedIndex = selectedIndex;
                }
                else if (selectedIndex > 0)
                {
                    this.SourcesList.SelectedIndex = selectedIndex - 1;
                }
            }            
        }

        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
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
        /// Populate the source type combo
        /// </summary>
        private void PopulateSourceTypeCombo()
        {
            // Add items
            Utils utils = new Utils();
            _sourceTypeList.Clear();
            foreach (ESourceType sourceType in Enum.GetValues(typeof(ESourceType)))
            {
                if (sourceType != ESourceType.Internal)
                {
                    _sourceTypeList.Add(new NamedItem((long)sourceType, utils.GetSourceTypeName(sourceType)));
                }
            }

            // Bind
            this.SourceTypeCombo.ItemsSource = _sourceTypeList;
            
            // Select first item
            this.SourceTypeCombo.SelectedIndex = 0;
        }

        /// <summary>
        /// Populate the list of items
        /// </summary>
        private void PopulateSourcesList()
        {
            // Bind
            this.SourcesList.ItemsSource = _inputSourceList;

            // Select first item
            if (_inputSourceList.Count > 0)
            {
                this.SourcesList.SelectedIndex = 0;
            }
        }

    }
}
