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
using System.Collections.ObjectModel;
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
                this.WindowWidthSlider.Maximum = SystemParameters.PrimaryScreenWidth;
                this.WindowWidthSlider.Value = _customWindowSource.WindowWidth;
                this.WindowHeightSlider.Maximum = SystemParameters.PrimaryScreenHeight;
                this.WindowHeightSlider.Value = _customWindowSource.WindowHeight;
                this.TranslucencySlider.Value = 100.0 * _customWindowSource.Translucency;
                this.TopMostCheckBox.IsChecked = _customWindowSource.TopMost;
                this.GhostBackgroundCheckBox.IsChecked = _customWindowSource.GhostBackground;
                this.WindowBackgroundColourCombo.SelectedColour = _customWindowSource.BackgroundColour;

                // Background colour N/A for ghost mode
                this.WindowBackgroundColourCombo.IsEnabled = !_customWindowSource.GhostBackground;

                // Font families
                ComboBoxItem defaultItem = new ComboBoxItem();
                defaultItem.Content = "";
                ButtonFontCombo.Items.Add(defaultItem);
                List<string> fontFamilies = GetListOfFonts();
                foreach (string fontFamily in fontFamilies)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = fontFamily;
                    ButtonFontCombo.Items.Add(item);
                }
                ButtonFontCombo.SelectedValue = "";

                // Button properties
                this.ButtonXSlider.Maximum = _customWindowSource.WindowWidth;
                this.ButtonWidthSlider.Maximum = _customWindowSource.WindowWidth;
                this.ButtonYSlider.Maximum = _customWindowSource.WindowHeight;
                this.ButtonHeightSlider.Maximum = _customWindowSource.WindowHeight;
                this.ButtonBorderColourCombo.SelectedColour = Constants.DefaultCustomButtonBorderColour;
                this.ButtonBackgroundColourCombo.SelectedColour = Constants.DefaultCustomButtonBackgroundColour;
                this.ButtonFontSizeSlider.Value = Constants.DefaultCustomButtonFontSize;
                this.ButtonTextColourCombo.SelectedColour = Constants.DefaultCustomButtonTextColour;

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_CUST001, ex);
            }
        }

        /// <summary>
        /// Enumerate system fonts
        /// </summary>
        /// <returns></returns>
        private List<string> GetListOfFonts()
        {
            List<string> fontFamilies = new List<string>();
            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
            {
                fontFamilies.Add(fontFamily.Source);
            }
            fontFamilies.Sort();
            return fontFamilies;
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
                    double newX = ButtonXSlider.Value + ButtonWidthSlider.Value;
                    double newY = ButtonYSlider.Value;
                    if (newX > ButtonXSlider.Maximum - ButtonWidthSlider.Value)
                    {
                        newX = ButtonXSlider.Value;
                        newY = Math.Min(ButtonYSlider.Value + ButtonHeightSlider.Value, WindowHeightSlider.Value - ButtonHeightSlider.Value);
                    }
                    string name = _customWindowSource.CustomButtons.GetFirstUnusedName("Button", false);
                    CustomButtonData buttonData = new CustomButtonData(
                        (byte)id, 
                        name, 
                        "",
                        newX,
                        newY, 
                        this.ButtonWidthSlider.Value, 
                        this.ButtonHeightSlider.Value, 
                        this.ButtonBorderThicknessSlider.Value,
                        this.ButtonBorderColourCombo.SelectedColour,
                        this.ButtonBackgroundColourCombo.SelectedColour,
                        "",
                        0.01 * this.ButtonTranslucencySlider.Value,
                        (string)this.ButtonFontCombo.SelectedValue,
                        this.ButtonFontSizeSlider.Value,
                        this.ButtonTextColourCombo.SelectedColour);

                    // Add to custom window
                    PreviewControl.CreateButton(buttonData);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_CUST002, ex);
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
                ErrorMessage.Show(Properties.Resources.E_CUST003, ex);
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
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    if (data.Count != 0)
                    {
                        CustomButtonData buttonData = data[0];
                        ButtonNameTextBox.Text = buttonData.Name;
                        ButtonTextTextBox.Text = buttonData.Text;
                        ButtonXSlider.Value = buttonData.X;
                        ButtonYSlider.Value = buttonData.Y;
                        ButtonWidthSlider.Value = buttonData.Width;
                        ButtonHeightSlider.Value = buttonData.Height;
                        ButtonBorderThicknessSlider.Value = buttonData.BorderThickness;
                        ButtonBorderColourCombo.SelectedColour = buttonData.BorderColour;
                        ButtonBackgroundColourCombo.SelectedColour = buttonData.BackgroundColour;
                        ButtonTranslucencySlider.Value = 100.0 * buttonData.BackgroundTranslucency;
                        ButtonFontCombo.SelectedValue = buttonData.FontName;
                        ButtonFontSizeSlider.Value = buttonData.FontSize;
                        ButtonTextColourCombo.SelectedColour = buttonData.TextColour;
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
                    ErrorMessage.Show(Properties.Resources.E_CUST004, ex);
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
                    ErrorMessage.Show(Properties.Resources.E_CUST005, ex);
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
                    ErrorMessage.Show(Properties.Resources.E_CUST006, ex);
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
                ErrorMessage.Show(Properties.Resources.E_CUST007, ex);
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
                    _customWindowSource.Translucency = 0.01 * this.TranslucencySlider.Value;
                    PreviewControl.RefreshBackgroundColours();
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST008, ex);
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
                    ErrorMessage.Show(Properties.Resources.E_CUST009, ex);
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
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    if (data.Count != 0)
                    {
                        CustomButtonData first = data[0];
                        double x = Math.Max(0, Math.Min(_customWindowSource.WindowWidth - first.Width, args.Value));
                        double diff = x - first.X;
                        if (diff != 0)
                        {
                            foreach (CustomButtonData buttonData in data)
                            {
                                buttonData.X = Math.Max(0, Math.Min(_customWindowSource.WindowWidth - buttonData.Width, buttonData.X + diff));
                            }
                            PreviewControl.RefreshSelectedButtons();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST010, ex);
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
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    if (data.Count != 0)
                    {
                        CustomButtonData first = data[0];
                        double y = Math.Max(0, Math.Min(_customWindowSource.WindowHeight - first.Height, args.Value));
                        double diff = y - first.Y;
                        if (diff != 0)
                        {
                            foreach (CustomButtonData buttonData in data)
                            {
                                buttonData.Y = Math.Max(0, Math.Min(_customWindowSource.WindowHeight - buttonData.Height, buttonData.Y + diff));
                            }
                            PreviewControl.RefreshSelectedButtons();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST011, ex);
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
                    bool updated = false;
                    double width = Math.Max(Constants.MinCustomButtonSize, Math.Min(_customWindowSource.WindowWidth, args.Value));
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    foreach (CustomButtonData buttonData in data)
                    {
                        if (buttonData.Width != width)
                        {
                            buttonData.Width = width;
                            updated = true;
                        }
                    }
                    if (updated)
                    {
                        PreviewControl.RefreshSelectedButtons();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST012, ex);
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
                    bool updated = false;
                    double height = Math.Max(Constants.MinCustomButtonSize, Math.Min(_customWindowSource.WindowHeight, args.Value));
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    foreach (CustomButtonData buttonData in data)
                    {
                        if (buttonData.Height != height)
                        {
                            buttonData.Height = height;
                            updated = true;
                        }
                    }
                    if (updated)
                    {
                        PreviewControl.RefreshSelectedButtons();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST013, ex);
                }
            }
        }

        /// <summary>
        /// Button border thickness changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ButtonBorderThicknessSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();
                    bool updated = false;
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    foreach (CustomButtonData buttonData in data) 
                    {
                        if (buttonData.BorderThickness != args.Value)
                        {
                            buttonData.BorderThickness = args.Value;
                            updated = true;
                        }
                    }
                    if (updated)
                    {
                        PreviewControl.RefreshSelectedButtons();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST014, ex);
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
                    bool updated = false;
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    foreach (CustomButtonData buttonData in data)
                    {
                        if (buttonData.Name != ButtonNameTextBox.Text)
                        {
                            buttonData.Name = ButtonNameTextBox.Text;
                            updated = true;
                        }
                    }
                    if (updated)
                    {
                        PreviewControl.RefreshSelectedButtons();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST015, ex);
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
                    bool updated = false;
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    foreach (CustomButtonData buttonData in data)
                    {
                        if (buttonData.Text != ButtonTextTextBox.Text)
                        {
                            buttonData.Text = ButtonTextTextBox.Text;
                            updated = true;
                        }
                    }
                    if (updated)
                    {
                        PreviewControl.RefreshSelectedButtons();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST016, ex);
                }
            }
        }

        /// <summary>
        /// Button text colour changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonTextColourCombo_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_isLoaded && this.ButtonTextColourCombo.SelectedColour != null)
            {
                try
                {
                    ErrorMessage.Clear();
                    string colour = this.ButtonTextColourCombo.SelectedColour;
                    bool updated = false;
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    foreach (CustomButtonData buttonData in data)
                    {
                        if (buttonData.TextColour != colour)
                        {
                            buttonData.TextColour = colour;
                            updated = true;
                        }
                    }
                    if (updated)
                    {
                        PreviewControl.RefreshSelectedButtons();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST024, ex);
                }
            }
        }
        /// <summary>
        /// Button border colour changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBorderColourCombo_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_isLoaded && this.ButtonBorderColourCombo.SelectedColour != null)
            {
                try
                {
                    ErrorMessage.Clear();
                    string colour = this.ButtonBorderColourCombo.SelectedColour;
                    bool updated = false;
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    foreach (CustomButtonData buttonData in data)
                    {
                        if (buttonData.BorderColour != colour)
                        {
                            buttonData.BorderColour = colour;
                            updated = true;
                        }
                    }
                    if (updated)
                    {
                        PreviewControl.RefreshSelectedButtons();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST017, ex);
                }
            }
        }

        /// <summary>
        /// Button background colour changed
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
                    string colour = this.ButtonBackgroundColourCombo.SelectedColour;
                    bool updated = false;
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    foreach (CustomButtonData buttonData in data)
                    {
                        if (buttonData.BackgroundColour != colour)
                        {
                            buttonData.BackgroundColour = colour;
                            updated = true;
                        }
                    }
                    if (updated)
                    {
                        PreviewControl.RefreshSelectedButtonBackground();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST018, ex);
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
                List<CustomButtonData> data = PreviewControl.GetSelectionData();
                if (data.Count != 0)
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

                        foreach (CustomButtonData buttonData in data)
                        {
                            buttonData.BackgroundImage = fileName;
                        }
                        PreviewControl.RefreshSelectedButtonBackground();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_CUST019, ex);
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
                List<CustomButtonData> data = PreviewControl.GetSelectionData();
                if (data.Count != 0)
                {
                    foreach (CustomButtonData buttonData in data)
                    {
                        buttonData.BackgroundImage = "";
                    }
                    PreviewControl.RefreshSelectedButtonBackground();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_CUST020, ex);
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
                    bool updated = false;
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    foreach (CustomButtonData buttonData in data)
                    {
                        if (buttonData.BackgroundTranslucency != 0.01 * ButtonTranslucencySlider.Value)
                        {
                            buttonData.BackgroundTranslucency = 0.01 * ButtonTranslucencySlider.Value;
                            updated = true;
                        }
                    }
                    if (updated)
                    {
                        PreviewControl.RefreshSelectedButtonBackground();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST021, ex);
                }
            }
        }

        /// <summary>
        /// Button font changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonFontCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();
                    ComboBoxItem item = (ComboBoxItem)ButtonFontCombo.SelectedItem;
                    bool updated = false;
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    foreach (CustomButtonData buttonData in data)
                    {
                        if (item != null && buttonData.FontName != (string)item.Content)
                        {
                            buttonData.FontName = (string)item.Content;
                            updated = true;
                        }
                    }
                    if (updated)
                    {
                        PreviewControl.RefreshSelectedButtons();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST022, ex);
                }
            }
        }

        /// <summary>
        /// Button font size changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonFontSizeSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ErrorMessage.Clear();
                    bool updated = false;
                    List<CustomButtonData> data = PreviewControl.GetSelectionData();
                    foreach (CustomButtonData buttonData in data)
                    {
                        if (buttonData.FontSize != ButtonFontSizeSlider.Value)
                        {
                            buttonData.FontSize = ButtonFontSizeSlider.Value;
                            updated = true;                            
                        }
                    }
                    if (updated)
                    {
                        PreviewControl.RefreshSelectedButtons();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_CUST023, ex);
                }
            }
        }
    }
}
