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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using AltController.Config;
using AltController.Core;
using AltController.Event;
using AltController.Input;

namespace AltController
{
    /// <summary>
    /// Custom window designer window
    /// </summary>
    public partial class CustomWindowDesigner : Window
    {
        // Members
        private IParentWindow _parentWindow;
        private CustomWindowSource _customWindowSource;
        private bool _isLoaded = false;

        // Properties
        public CustomWindowSource CustomWindow { get { return _customWindowSource; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomWindowDesigner(IParentWindow parentWindow, CustomWindowSource customWindowSource)
        {
            _parentWindow = parentWindow;
            _customWindowSource = customWindowSource;

            InitializeComponent();

            this.PreviewControl.SetAppConfig(parentWindow.GetAppConfig());
            this.PreviewControl.SetSource(_customWindowSource);
            this.PreviewControl.SelectionChanged += new AltControlEventHandler(PreviewControl_SelectionChanged);
        }

        /// <summary>
        /// Window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Window properties
                this.WindowTitleTextBox.Text = _customWindowSource.WindowTitle;
                this.WindowWidthSlider.Value = _customWindowSource.WindowWidth;
                this.WindowHeightSlider.Value = _customWindowSource.WindowHeight;
                this.TranslucencySlider.Value = _customWindowSource.Translucency;
                this.TopMostCheckBox.IsChecked = _customWindowSource.TopMost;
                this.GhostBackgroundCheckBox.IsChecked = _customWindowSource.GhostBackground;
                this.WindowBackgroundColourCombo.SelectedColour = _customWindowSource.BackgroundColour;

                // Background colour N/A for ghost mode
                this.WindowBackgroundColourCombo.IsEnabled = !_customWindowSource.GhostBackground;

                // Button properties
                this.ButtonXSlider.Maximum = _customWindowSource.WindowWidth;
                this.ButtonWidthSlider.Maximum = _customWindowSource.WindowWidth;
                this.ButtonYSlider.Maximum = _customWindowSource.WindowHeight;
                this.ButtonHeightSlider.Maximum = _customWindowSource.WindowHeight;
                this.ButtonBackgroundColourCombo.SelectedColour = "LightGray";

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                ErrorMessage.Show("Error loading window", ex);
            }
        }

        /// <summary>
        /// OK clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Cancel clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Add button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                long id = _customWindowSource.CustomButtons.GetFirstUnusedID();
                if (id < 256)
                {
                    string name = _customWindowSource.CustomButtons.GetFirstUnusedName("Button", false);
                    CustomButtonData buttonData = new CustomButtonData(
                        (byte)id, 
                        name, 
                        "", 
                        0, 
                        0, 
                        this.ButtonWidthSlider.Value, 
                        this.ButtonHeightSlider.Value, 
                        this.ButtonBackgroundColourCombo.SelectedColour,
                        "",
                        this.ButtonTranslucencySlider.Value);

                    // Add to custom window
                    PreviewControl.CreateButton(buttonData);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show("Error adding new custom button", ex);
            }
        }

        /// <summary>
        /// Delete button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                // Remove selected button from custom window
                this.PreviewControl.RemoveSelectedButton();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show("Error deleting custom button", ex);
            }
        }

        /// <summary>
        /// Custom button selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PreviewControl_SelectionChanged(object sender, AltControlEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();

                    // Show button details
                    CustomButtonData buttonData = PreviewControl.GetSelectedButtonData();
                    if (buttonData != null)
                    {
                        ButtonNameTextBox.Text = buttonData.Name;
                        ButtonTextTextBox.Text = buttonData.Text;
                        ButtonXSlider.Value = buttonData.X;
                        ButtonYSlider.Value = buttonData.Y;
                        ButtonWidthSlider.Value = buttonData.Width;
                        ButtonHeightSlider.Value = buttonData.Height;
                        ButtonBackgroundColourCombo.SelectedColour = buttonData.BackgroundColour;
                        ButtonTranslucencySlider.Value = buttonData.BackgroundTranslucency;
                        BrowseButtonImageButton.IsEnabled = true;
                        ClearButtonImageButton.IsEnabled = true;
                        DeleteButton.IsEnabled = true;
                    }
                    else
                    {
                        BrowseButtonImageButton.IsEnabled = false;
                        ClearButtonImageButton.IsEnabled = false;
                        DeleteButton.IsEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error handling selection of custom button", ex);
                }
            }
        }

        /// <summary>
        /// Window title changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoaded)
            {
                _customWindowSource.WindowTitle = this.WindowTitleTextBox.Text;
            }
        }

        /// <summary>
        /// Window width changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void WindowWidthSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();

                    // Get the width value, but don't let the window squash the buttons
                    double width = args.Value;
                    foreach (CustomButtonData buttonData in CustomWindow.CustomButtons)
                    {
                        width = Math.Max(buttonData.X + buttonData.Width, width);
                    }

                    _customWindowSource.WindowWidth = width;
                    this.ButtonXSlider.Maximum = width;
                    this.ButtonWidthSlider.Maximum = width;
                    PreviewControl.RefreshLayout();
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while setting window size", ex);
                }
            }
        }

        /// <summary>
        /// Window height changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void WindowHeightSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();

                    // Get the height value, but don't let the window squash the buttons
                    double height = args.Value;
                    foreach (CustomButtonData buttonData in CustomWindow.CustomButtons)
                    {
                        height = Math.Max(buttonData.Y + buttonData.Height, height);
                    }

                    _customWindowSource.WindowHeight = height;
                    this.ButtonYSlider.Maximum = height;
                    this.ButtonHeightSlider.Maximum = height;
                    PreviewControl.RefreshLayout();
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while setting window size", ex);
                }
            }
        }

        /// <summary>
        /// Window background changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowBackgroundColourCombo_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded && this.WindowBackgroundColourCombo.SelectedColour != null)
                {
                    ErrorMessage.Clear();
                    _customWindowSource.BackgroundColour = this.WindowBackgroundColourCombo.SelectedColour;
                    PreviewControl.RefreshBackgroundColours();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show("Error while selecting background colour", ex);
            }

        }

        /// <summary>
        /// Window translucency changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TranslucencySlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();
                    _customWindowSource.Translucency = this.TranslucencySlider.Value;
                    PreviewControl.RefreshBackgroundColours();
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while setting window translucency", ex);
                }
            }
        }

        /// <summary>
        /// Ghost background check box clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GhostBackgroundCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();
                    _customWindowSource.GhostBackground = (this.GhostBackgroundCheckBox.IsChecked == true);                   
                    this.WindowBackgroundColourCombo.IsEnabled = !_customWindowSource.GhostBackground;
                    PreviewControl.RefreshBackgroundColours();
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while changing ghost background option", ex);
                }
            }
        }

        /// <summary>
        /// Top most check box clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopMostCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                _customWindowSource.TopMost = (this.TopMostCheckBox.IsChecked == true);
            }
        }

        /// <summary>
        /// Button data changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ButtonXSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();
                    CustomButtonData buttonData = PreviewControl.GetSelectedButtonData();
                    if (buttonData != null)
                    {
                        buttonData.X = Math.Max(0, Math.Min(_customWindowSource.WindowWidth - buttonData.Width, args.Value));
                        PreviewControl.RefreshSelectedButtonLayout();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while positioning button", ex);
                }
            }
        }

        /// <summary>
        /// Button data changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ButtonYSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();
                    CustomButtonData buttonData = PreviewControl.GetSelectedButtonData();
                    if (buttonData != null)
                    {
                        buttonData.Y = Math.Max(0, Math.Min(_customWindowSource.WindowHeight - buttonData.Height, args.Value));
                        PreviewControl.RefreshSelectedButtonLayout();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while positioning button", ex);
                }
            }
        }

        /// <summary>
        /// Button data changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ButtonWidthSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();
                    CustomButtonData buttonData = PreviewControl.GetSelectedButtonData();
                    if (buttonData != null)
                    {
                        buttonData.Width = Math.Max(Constants.MinCustomButtonSize, Math.Min(_customWindowSource.WindowWidth - buttonData.X, args.Value));
                        PreviewControl.RefreshSelectedButtonLayout();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while resizing button", ex);
                }
            }
        }

        /// <summary>
        /// Button data changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ButtonHeightSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();
                    CustomButtonData buttonData = PreviewControl.GetSelectedButtonData();
                    if (buttonData != null)
                    {
                        buttonData.Height = Math.Max(Constants.MinCustomButtonSize, Math.Min(_customWindowSource.WindowHeight - buttonData.Y, args.Value));
                        PreviewControl.RefreshSelectedButtonLayout();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while resizing button", ex);
                }
            }
        }

        /// <summary>
        /// Button data changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoaded && this.ButtonNameTextBox.Text != "")
            {
                try
                {
                    ErrorMessage.Clear();
                    CustomButtonData buttonData = PreviewControl.GetSelectedButtonData();
                    if (buttonData != null)
                    {
                        buttonData.Name = ButtonNameTextBox.Text;
                        PreviewControl.RefreshSelectedButtonLayout();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while changing button name", ex);
                }
            }
        }

        /// <summary>
        /// Button data changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonTextTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();
                    CustomButtonData buttonData = PreviewControl.GetSelectedButtonData();
                    if (buttonData != null)
                    {
                        buttonData.Text = ButtonTextTextBox.Text;
                        PreviewControl.RefreshSelectedButtonLayout();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while changing button text", ex);
                }
            }
        }

        /// <summary>
        /// Button colour changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBackgroundColourCombo_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_isLoaded && this.ButtonBackgroundColourCombo.SelectedColour != null)
            {
                try
                {
                    ErrorMessage.Clear();
                    CustomButtonData buttonData = PreviewControl.GetSelectedButtonData();
                    if (buttonData != null)
                    {
                        buttonData.BackgroundColour = (string)this.ButtonBackgroundColourCombo.SelectedColour;
                        PreviewControl.RefreshSelectedButtonBackground();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while changing button colour", ex);
                }
            }
        }


        /// <summary>
        /// Browse for button background image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButtonImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();
                CustomButtonData buttonData = PreviewControl.GetSelectedButtonData();
                if (buttonData != null)
                {
                    string defaultProfilesDir = System.IO.Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                    string profilesDir = _parentWindow.GetAppConfig().GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);

                    System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                    dialog.InitialDirectory = profilesDir;
                    dialog.Multiselect = false;
                    dialog.Filter = Properties.Resources.String_Image_files_filter;
                    dialog.CheckFileExists = true;
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        // Store as a relative path if possible
                        string fileName;
                        System.IO.FileInfo fi = new System.IO.FileInfo(dialog.FileName);
                        System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(profilesDir);
                        if (fi.Directory.FullName == dirInfo.FullName)
                        {
                            // Profile is in Profiles directory, so store as relative path
                            fileName = fi.Name;
                        }
                        else
                        {
                            // Profile isn't in Profiles directory, so store full path
                            fileName = fi.FullName;
                        }
                        
                        buttonData.BackgroundImage = fileName;

                        PreviewControl.RefreshSelectedButtonBackground();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show("Error while setting button background image", ex);
            }
        }

        /// <summary>
        /// Clear button background image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButtonImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();
                CustomButtonData buttonData = PreviewControl.GetSelectedButtonData();
                if (buttonData != null)
                {
                    buttonData.BackgroundImage = "";

                    PreviewControl.RefreshSelectedButtonBackground();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show("Error while clearing background image", ex);
            }
        }

        /// <summary>
        /// Button background translucency changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ButtonTranslucencySlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();
                    CustomButtonData buttonData = PreviewControl.GetSelectedButtonData();
                    if (buttonData != null)
                    {
                        buttonData.BackgroundTranslucency = ButtonTranslucencySlider.Value;

                        PreviewControl.RefreshSelectedButtonBackground();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show("Error while setting button translucency", ex);
                }
            }
        }
    }
}
