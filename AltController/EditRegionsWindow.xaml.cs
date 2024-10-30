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
using System.Windows.Input;
using AltController.Core;
using AltController.Config;
using AltController.Event;
using System.Windows.Media.Imaging;

namespace AltController
{
    /// <summary>
    /// Edit screen regions window
    /// </summary>
    public partial class EditRegionsWindow : Window
    {
        // Config
        private Utils _utils = new Utils();
        private AppConfig _appConfig;
        private Profile _profile;
        private IParentWindow _parentWindow;
        private ScreenRegionList _regionsList = new ScreenRegionList();
        private NamedItemList _overlayPositions = new NamedItemList();
        private NamedItemList _modesList = new NamedItemList();
        private NamedItemList _appsList = new NamedItemList();
        private NamedItemList _shapesList = new NamedItemList();

        // State
        private bool _isLoaded;
        private List<ScreenRegion> _selectedRegions = new List<ScreenRegion>();
        private bool _suppressPositionChangeEvents = false;
        private Size _bgImageSize = Size.Empty;

        // Commands
        public static RoutedCommand PreviousCommand = new RoutedCommand();
        public static RoutedCommand NextCommand = new RoutedCommand();

        // Get / set the data to edit
        public Profile CurrentProfile 
        { 
            get { return _profile; } 
            set
            {
                _profile = value;

                _regionsList = _profile.ScreenRegions;
                RegionsControl.SetRegionsList(_regionsList);
            } 
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public EditRegionsWindow(IParentWindow parentWindow)
        {
            InitializeComponent();

            _parentWindow = parentWindow;

            _appConfig = parentWindow.GetAppConfig();
            RegionsControl.SetAppConfig(_appConfig);
            RegionsControl.SelectedRegionChanged += new EventHandler(RegionsControl_SelectedRegionChanged);
            RegionsControl.FinishedEditing += new EventHandler(RegionsControl_FinishedEditing);
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
                _isLoaded = true;

                // Default translucency label
                this.DefaultTranslucencyLabel.Text = string.Format("{0} ({1}%)", 
                    Properties.Resources.String_Default, 
                    (int)(100 * _appConfig.GetDoubleVal(Constants.ConfigDefaultRegionTranslucency, Constants.DefaultRegionTranslucency))
                    );

                // Force square region option
                bool forceSquare = _appConfig.GetBoolVal(Constants.ConfigDrawRegionForceSquare, Constants.DefaultDrawRegionForceSquare);
                this.ForceSquareCheckbox.IsChecked = forceSquare;

                // Overlay positions
                GUIUtils.PopulateDisplayAreaList(_overlayPositions);
                this.OverlayPositionCombo.ItemsSource = _overlayPositions;

                // Populate modes
                _modesList.Clear();
                _modesList.Add(new NamedItem(Constants.DefaultID, Properties.Resources.String_All_Option));
                _modesList.Add(new NamedItem(Constants.NoneID, Properties.Resources.String_None_Option));
                foreach (NamedItem item in _profile.ModeDetails)
                {
                    if (item.ID != Constants.DefaultID)
                    {
                        _modesList.Add(item);
                    }
                }
                this.ModeCombo.ItemsSource = _modesList;

                // Populate apps
                _appsList.Clear();
                _appsList.Add(new NamedItem(Constants.DefaultID, Properties.Resources.String_All_Option));
                _appsList.Add(new NamedItem(Constants.NoneID, Properties.Resources.String_None_Option));
                foreach (AppItem item in _profile.AppDetails)
                {
                    if (item.ID != Constants.DefaultID && !item.Snooze)
                    {
                        _appsList.Add(item);
                    }
                }
                this.AppCombo.ItemsSource = _appsList;

                // Populate shapes
                _shapesList.Clear();
                foreach (EShape shape in Enum.GetValues(typeof(EShape)))
                {
                    string name = _utils.GetShapeName(shape);
                    _shapesList.Add(new NamedItem((long)shape, name));
                }
                this.ShapeComboBox.ItemsSource = _shapesList;

                // Initial combo selections
                this.ColoursCombo.SelectedColour = Constants.DefaultScreenRegionColour;
                this.BackgroundColourCombo.SelectedColour = "";
                this.ModeCombo.SelectedIndex = 0;
                this.AppCombo.SelectedIndex = (_appsList.Count > 2) ? 2 : 0;    // First app, or (All) if no apps in profile
                this.ShapeComboBox.SelectedIndex = 0;

                // Select overlay option
                this.OverlayPositionCombo.SelectedValue = (int)_regionsList.OverlayPosition;

                // Select first region if possible
                if (_regionsList.Count != 0)
                {
                    RegionsControl.SetSelectedRegions(new List<ScreenRegion> { (ScreenRegion)_regionsList[0] });
                }
                else
                {
                    // Show 'Region 0 of 0'
                    string strTemplate = Properties.Resources.Regions_NavigateTextBlock;
                    NavigateTextBlock.Text = string.Format(strTemplate, 0, 0);
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG001, ex);
            }
        }

        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Warn if identical regions on top of each other
            bool duplicateRegions = false;
            for (int i = 0; i < _regionsList.Count - 1; i++)
            {
                ScreenRegion firstRegion = (ScreenRegion)_regionsList[i];
                for (int j = i + 1; j < _regionsList.Count; j++)
                {
                    ScreenRegion secondRegion = (ScreenRegion)_regionsList[j];
                    if (firstRegion.IsSameShapeAs(secondRegion))
                    {
                        duplicateRegions = true;
                        break;
                    }
                }

                if (duplicateRegions)
                {
                    break;
                }
            }            

            if (!duplicateRegions || 
                MessageBoxResult.Yes == MessageBox.Show(Properties.Resources.String_Duplicate_regions_description,
                Properties.Resources.String_Duplicate_regions_title,
                    MessageBoxButton.YesNoCancel, 
                    MessageBoxImage.Question, 
                    MessageBoxResult.No))
            {
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
        /// Region selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionsControl_SelectedRegionChanged(object sender, EventArgs e)
        {
            try
            {
                ClearMessages();
                UpdateRegionName();
                _selectedRegions = new List<ScreenRegion>(RegionsControl.SelectedRegions);

                RefreshRegionDetails();
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG002, ex);
            }
        }

        /// <summary>
        /// Draw button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();
                StartDrawingRegion();
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG003, ex);
            }
        }

        /// <summary>
        /// Enable drawing
        /// </summary>
        private void StartDrawingRegion()
        {
            DrawButton.IsEnabled = false;
            bool lockYAxis = this.ForceSquareCheckbox.IsChecked == true;
            RegionsControl.StartDrawingRegion(lockYAxis);
        }

        /// <summary>
        /// Handle region edits performed in user control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionsControl_FinishedEditing(object sender, EventArgs e)
        {
            try
            {
                ClearMessages();

                // Temporarily prevent the Left and Top position controls from triggering value change events
                // which override the user's region positioning in the user control
                _suppressPositionChangeEvents = true;
                RefreshRegionDetails();
                _suppressPositionChangeEvents = false;
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG004, ex);
            }
        }

        /// <summary>
        /// Add new region
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddRegionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();
                string selectedColour = this.ColoursCombo.SelectedColour;
                string selectedBgColour = this.BackgroundColourCombo.SelectedColour;
                NamedItem selectedMode = (NamedItem)this.ModeCombo.SelectedItem;
                NamedItem selectedApp = (NamedItem)this.AppCombo.SelectedItem;
                NamedItem selectedShape = (NamedItem)this.ShapeComboBox.SelectedItem;
                if (selectedMode != null && selectedApp != null && selectedShape != null && selectedColour != null && selectedBgColour != null && _regionsList != null)
                {
                    long id = _regionsList.GetFirstUnusedID();
                    string name = _regionsList.GetFirstUnusedName(Properties.Resources.String_Region, false);
                    Rect rect = new Rect(0.05, 0.05, 0.01 * WidthSlider.Value, 0.01 * HeightSlider.Value);    // Add near top left
                    double holeSizeFraction = 0.01 * HoleSizeSlider.Value;
                    double startAngleDeg = StartAngleSlider.Value;
                    double sweepAngleDeg = SweepAngleSlider.Value;
                    double translucency = DefaultTranslucencyCheckbox.IsChecked == false ? 0.01 * TranslucencySlider.Value : -1.0;
                    ScreenRegion newRegion = new ScreenRegion(id,
                        name,
                        (EShape)selectedShape.ID,
                        rect,
                        holeSizeFraction,
                        startAngleDeg,
                        sweepAngleDeg,
                        selectedColour,
                        selectedBgColour,
                        "",
                        translucency,
                        new LogicalState(selectedMode.ID, selectedApp.ID, Constants.DefaultID));
                    _regionsList.Add(newRegion);

                    // Refresh display
                    RegionsControl.RefreshDisplay();

                    // Select the new region
                    RegionsControl.SetSelectedRegions(new List<ScreenRegion> { newRegion });
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG005, ex);
            }
        }

        /// <summary>
        /// Delete selected region
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteRegionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();
                if (_selectedRegions.Count != 0)
                {
                    // Remove from list
                    foreach (ScreenRegion region in _selectedRegions)
                    {
                        _regionsList.Remove(region);
                    }

                    // Refresh display
                    RegionsControl.RefreshDisplay();

                    // Select first region if possible
                    List<ScreenRegion> toSelect = new List<ScreenRegion>();
                    if (_regionsList.Count != 0)
                    {
                        toSelect.Add((ScreenRegion)_regionsList[0]);
                    }
                    RegionsControl.SetSelectedRegions(toSelect);
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG006, ex);
            }
        }

        /// <summary>
        /// Allow the user to select all screen regions by pressing Ctrl + A
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_regionsList != null)
            {
                // Create a list of all screen regions
                ScreenRegion[] allRegions = new ScreenRegion[_regionsList.Count];
                _regionsList.CopyTo(allRegions, 0);

                RegionsControl.SetSelectedRegions(new List<ScreenRegion>(allRegions));
            }

            e.Handled = true;
        }

        /// <summary>
        /// Decide whether navigate command can execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            int regionCount = (_regionsList != null) ? _regionsList.Count : 0;
            int regionIndex = GetSelectedRegionIndex();
            e.CanExecute = regionCount > 1 || (regionCount == 1 && regionIndex == -1);
            e.Handled = true;
        }

        /// <summary>
        /// Previous region command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ClearMessages();
                if (_regionsList != null && _regionsList.Count != 0)
                {
                    int regionIndex = GetSelectedRegionIndex();
                    regionIndex = (regionIndex + _regionsList.Count - 1) % _regionsList.Count;
                    RegionsControl.SetSelectedRegions(new List<ScreenRegion> { (ScreenRegion)_regionsList[regionIndex] });
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG007, ex);
            }
            e.Handled = true;
        }

        /// <summary>
        /// Next region command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ClearMessages();
                if (_regionsList != null && _regionsList.Count != 0)
                {
                    int regionIndex = GetSelectedRegionIndex();
                    regionIndex = (regionIndex + 1) % _regionsList.Count;
                    RegionsControl.SetSelectedRegions(new List<ScreenRegion> { (ScreenRegion)_regionsList[regionIndex] });
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG008, ex);
            }
            e.Handled = true;
        }

        /// <summary>
        /// Get the index of the first selected region
        /// </summary>
        /// <returns></returns>
        private int GetSelectedRegionIndex()
        {
            int regionIndex = -1;
            if (_selectedRegions.Count > 0)
            {
                ScreenRegion region = _selectedRegions[0];
                regionIndex = _regionsList.IndexOf(region);
            }

            return regionIndex;
        }

        /// <summary>
        /// Display details for the (first) selected region
        /// </summary>
        private void RefreshRegionDetails()
        {
            bool isSelection = _selectedRegions.Count > 0;
            if (isSelection)
            {
                ScreenRegion region = _selectedRegions[0];

                if (_selectedRegions.Count == 1)
                {
                    this.RegionNameText.Text = region.Name;
                    this.RegionNameText.IsEnabled = true;
                    DrawButton.IsEnabled = true;
                    EnableOrDisableShapeOptions();   // Show shape options according to shape type when single region selected
                }
                else
                {
                    this.RegionNameText.Text = string.Format(Properties.Resources.String_N_regions_selected, _selectedRegions.Count);
                    this.RegionNameText.IsEnabled = false;
                    DrawButton.IsEnabled = false;
                    // Show all shape options when multiple regions selected
                    HoleSizeSlider.IsEnabled = true;
                    StartAngleSlider.IsEnabled = true;
                    SweepAngleSlider.IsEnabled = true;
                }
                this.ColoursCombo.SelectedColour = region.Colour;
                this.BackgroundColourCombo.SelectedColour = region.BackgroundColour;
                // Check that the mode is valid to avoid deselecting mode option
                if (_modesList.GetItemByID(region.ShowInState.ModeID) != null)
                {
                    this.ModeCombo.SelectedValue = region.ShowInState.ModeID;
                }                
                // Check that the app is valid to avoid deselecting app option
                if (_appsList.GetItemByID(region.ShowInState.AppID) != null)
                {
                    this.AppCombo.SelectedValue = region.ShowInState.AppID;
                }                
                DisplayRegionCoords(region);
                ShapeComboBox.SelectedValue = (long)region.Shape;
                HoleSizeSlider.Value = 100.0 * region.HoleSize;
                StartAngleSlider.Value = region.StartAngle;
                SweepAngleSlider.Value = region.SweepAngle;
                bool defaultTranslucency = region.Translucency < 0.0 || region.Translucency > 1.0;
                if (!defaultTranslucency)
                {
                    TranslucencySlider.Value = 100.0 * region.Translucency;
                }
                DefaultTranslucencyCheckbox.IsChecked = defaultTranslucency;
                TranslucencySlider.Visibility = defaultTranslucency ? Visibility.Hidden : Visibility.Visible;
                DefaultTranslucencyLabel.Visibility = defaultTranslucency ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                this.RegionNameText.Text = "";
                this.RegionNameText.IsEnabled = false;
                DrawButton.IsEnabled = false;
                HoleSizeSlider.IsEnabled = false;
                StartAngleSlider.IsEnabled = false;
                SweepAngleSlider.IsEnabled = false;
            }

            ColoursCombo.IsEnabled = isSelection;
            BackgroundColourCombo.IsEnabled = isSelection;
            ModeCombo.IsEnabled = isSelection;
            AppCombo.IsEnabled = isSelection;
            LeftSlider.IsEnabled = isSelection;
            TopSlider.IsEnabled = isSelection;
            WidthSlider.IsEnabled = isSelection;
            HeightSlider.IsEnabled = isSelection;
            ShapeComboBox.IsEnabled = isSelection;
            BrowseRegionImageButton.IsEnabled = isSelection;
            ClearRegionImageButton.IsEnabled = isSelection;
            TranslucencySlider.IsEnabled = isSelection;
            DefaultTranslucencyCheckbox.IsEnabled = isSelection;
            DeleteRegionButton.IsEnabled = isSelection;

            RefreshNavigatePanel();
        }

        /// <summary>
        /// Refresh the navigate button panel
        /// </summary>
        private void RefreshNavigatePanel()
        {
            int regionCount = (_regionsList != null) ? _regionsList.Count : 0;
            int regionIndex = GetSelectedRegionIndex();
            bool canNavigate = regionCount > 1 || (regionCount == 1 && regionIndex == -1);
            NextButton.IsEnabled = canNavigate;
            PreviousButton.IsEnabled = canNavigate;
            string strTemplate = Properties.Resources.Regions_NavigateTextBlock;
            NavigateTextBlock.Text = string.Format(strTemplate, regionIndex + 1, regionCount);
        }

        /// <summary>
        /// Display the co-ordinates of the selected screen region
        /// </summary>
        /// <param name="region"></param>
        private void DisplayRegionCoords(ScreenRegion region)
        {
            if (region != null)
            {
                this.LeftSlider.Value = 100.0 * region.Rectangle.Left;
                this.TopSlider.Value = 100.0 * region.Rectangle.Top;
                this.WidthSlider.Value = 100.0 * region.Rectangle.Width;
                this.HeightSlider.Value = 100.0 * region.Rectangle.Height;
            }
        }     

        /// <summary>
        /// Browse for a background screenshot image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();

                string defaultProfilesDir = System.IO.Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                string profilesDir = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);

                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.InitialDirectory = profilesDir;
                dialog.Multiselect = false;
                dialog.Filter = Properties.Resources.String_Image_files_filter;
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Store as a relative path if possible
                    string filepath;
                    System.IO.FileInfo fi = new System.IO.FileInfo(dialog.FileName);
                    System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(profilesDir);
                    if (fi.Directory.FullName == dirInfo.FullName)
                    {
                        // Profile is in Profiles directory, so store as relative path
                        filepath = fi.Name;
                    }
                    else
                    {
                        // Profile isn't in Profiles directory, so store full path
                        filepath = fi.FullName;
                    }

                    SetBackgroundImage(filepath);
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG009, ex);
            }
        }

        /// <summary>
        /// Clear the background screenshot image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();
                SetBackgroundImage("");
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG010, ex);
            }
        }

        private void SetBackgroundImage(string filepath)
        {
            _profile.ScreenRegions.RefImage = filepath;
            RegionsControl.RefreshBackground();

            _bgImageSize = Size.Empty;
            NamedItem item = (NamedItem)OverlayPositionCombo.SelectedItem;
            if (item != null && EDisplayArea.ActiveWindow == (EDisplayArea)item.ID)
            {
                // Resize regions control with the image's aspect ratio
                ResizeRegionsControl();
            }
        }

        /// <summary>
        /// Browse for region background image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseRegionImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();
                if (_selectedRegions.Count > 0)
                {
                    string defaultProfilesDir = System.IO.Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                    string profilesDir = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);

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

                        foreach (ScreenRegion region in _selectedRegions)
                        {
                            region.BackgroundImage = fileName;
                            region.BackgroundColour = "";
                        }

                        BackgroundColourCombo.SelectedColour = "";
                        RegionsControl.RefreshSelectedRegionBorderAndFill();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG011, ex);
            }            
        }

        /// <summary>
        /// Clear region background image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearRegionImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();
                if (_selectedRegions.Count > 0)
                {
                    foreach (ScreenRegion region in _selectedRegions)
                    {
                        region.BackgroundImage = "";
                    }

                    RegionsControl.RefreshSelectedRegionBorderAndFill();
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG012, ex);
            }
        }

        /// <summary>
        /// Set translucency checkbox changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DefaultTranslucencyCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                try
                {
                    ClearMessages();
                    bool defaultTranslucency = DefaultTranslucencyCheckbox.IsChecked == true;
                    TranslucencySlider.Visibility = defaultTranslucency ? Visibility.Hidden : Visibility.Visible;
                    DefaultTranslucencyLabel.Visibility = defaultTranslucency ? Visibility.Visible : Visibility.Hidden;
                    if (_selectedRegions.Count > 0)
                    {
                        double val = defaultTranslucency ? -1.0 : 0.01 * TranslucencySlider.Value;
                        foreach (ScreenRegion region in _selectedRegions)
                        {
                            region.Translucency = val;
                        }

                        RegionsControl.RefreshSelectedRegionOpacity();
                    }
                }
                catch (Exception ex)
                {
                    ShowError(Properties.Resources.E_REG013, ex);
                }
            }
        }

        /// <summary>
        /// Region translucency changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TranslucencySlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            if (_isLoaded)
            {
                try
                {
                    ClearMessages();
                    if (_selectedRegions.Count > 0)
                    {
                        double val = TranslucencySlider.Value;
                        foreach (ScreenRegion region in _selectedRegions)
                        {
                            region.Translucency = 0.01 * val;
                        }

                        RegionsControl.RefreshSelectedRegionOpacity();
                    }
                }
                catch (Exception ex)
                {
                    ShowError(Properties.Resources.E_REG013, ex);
                }
            }
        }

        /// <summary>
        /// Rename selected mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionNameText_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                ClearMessages();
                UpdateRegionName();
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG014, ex);
            }
        }

        /// <summary>
        /// Update the name of the selected region if it has changed
        /// </summary>
        private void UpdateRegionName()
        {
            if (_selectedRegions.Count == 1 && this.RegionNameText.Text != "" && this.RegionNameText.Text != _selectedRegions[0].Name)
            {
                _selectedRegions[0].Name = this.RegionNameText.Text;
            }
        }

        /// <summary>
        /// Colour changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColoursCombo_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();
                if (_selectedRegions.Count > 0 && this.ColoursCombo.SelectedColour != null)
                {
                    string colour = (string)this.ColoursCombo.SelectedColour;
                    foreach (ScreenRegion region in _selectedRegions)
                    {
                        region.Colour = colour;
                    }

                    RegionsControl.RefreshSelectedRegionBorderAndFill();
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG015, ex);
            }
        }

        /// <summary>
        /// Background colour changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundColourCombo_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();
                if (_selectedRegions.Count > 0 && this.BackgroundColourCombo.SelectedColour != null)
                {
                    string colour = (string)this.BackgroundColourCombo.SelectedColour;
                    foreach (ScreenRegion region in _selectedRegions)
                    {
                        region.BackgroundColour = colour;
                        if (colour != "")
                        {
                            region.BackgroundImage = "";
                        }
                    }

                    RegionsControl.RefreshSelectedRegionBorderAndFill();
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG015, ex);
            }
        }

        /// <summary>
        /// Mode changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ClearMessages();
                if (_selectedRegions.Count > 0 && this.ModeCombo.SelectedItem != null)
                {
                    foreach (ScreenRegion region in _selectedRegions)
                    {
                        region.ShowInState.ModeID = ((NamedItem)this.ModeCombo.SelectedItem).ID;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG016, ex);
            }
        }

        /// <summary>
        /// App changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ClearMessages();
                if (_selectedRegions.Count > 0 && this.AppCombo.SelectedItem != null)
                {
                    foreach (ScreenRegion region in _selectedRegions)
                    {
                        region.ShowInState.AppID = ((NamedItem)this.AppCombo.SelectedItem).ID;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG017, ex);
            }
        }

        /// <summary>
        /// Overlay position changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverlayPositionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ClearMessages();
                NamedItem item = (NamedItem)OverlayPositionCombo.SelectedItem;
                if (item != null)
                {
                    _profile.ScreenRegions.OverlayPosition = (EDisplayArea)item.ID;
                    ResizeRegionsControl();
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG018, ex);
            }
        }

        /// <summary>
        /// Window resize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeRegionsControl();
        }

        /// <summary>
        /// Size the regions canvas according with the correct aspect ratio
        /// </summary>
        /// <param name="position"></param>
        private void ResizeRegionsControl()
        {
            double maxWidth = OuterBorder.ActualWidth - 4;  // -4 accounts for outer and inner border widths
            double maxHeight = OuterBorder.ActualHeight - 4;
            if (!double.IsNaN(maxWidth) && !double.IsNaN(maxHeight) && 
                maxWidth > 0 && maxHeight > 1.0)
            {
                Size targetSize;
                EDisplayArea position = _profile.ScreenRegions.OverlayPosition;
                if (position == EDisplayArea.ActiveWindow)
                {
                    if (!string.IsNullOrEmpty(_profile.ScreenRegions.RefImage) && _bgImageSize == Size.Empty)
                    {
                        try
                        {
                            string imageFile = _profile.ScreenRegions.RefImage;
                            if (!imageFile.Contains(":")) 
                            {
                                // Relative path - try to find in the user's profiles folder
                                string defaultDir = System.IO.Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                                string profilesDir = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultDir);
                                imageFile = System.IO.Path.Combine(profilesDir, imageFile);
                            }
                            System.IO.FileInfo fi = new System.IO.FileInfo(imageFile);

                            if (fi.Exists)
                            {
                                BitmapImage image = new BitmapImage(new Uri(fi.FullName));
                                _bgImageSize = new Size(image.Width, image.Height);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }

                    if (_bgImageSize != Size.Empty)
                    {
                        targetSize = _bgImageSize;
                    }
                    else
                    {
                        targetSize = new Size(maxWidth, maxHeight);
                    }
                }
                else
                {
                    targetSize = GUIUtils.GetDisplayAreaSize(position);
                }                

                double width;
                double height;
                if (maxWidth * targetSize.Height > maxHeight * targetSize.Width)
                {
                    // Constrained by height
                    height = maxHeight;
                    width = maxHeight * targetSize.Width / targetSize.Height;
                }
                else
                {
                    // Constrained by width
                    width = maxWidth;
                    height = maxWidth * targetSize.Height / targetSize.Width;
                }

                RegionsControl.Width = width;
                RegionsControl.Height = height;
            }
        }
        
        /// <summary>
        /// Left changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            try
            {
                if (!_suppressPositionChangeEvents)
                {
                    ClearMessages();
                    double val = args.Value * 0.01;
                    if (_selectedRegions.Count > 0)
                    {
                        foreach (ScreenRegion region in _selectedRegions)
                        {
                            if (Math.Abs(region.Rectangle.X - val) > 0.000001)
                            {
                                // Update region
                                Rect rect = region.Rectangle;
                                rect.X = Math.Max(0.01 - rect.Width, Math.Min(1.0, val));
                                region.Rectangle = rect;
                            }
                        }

                        // Redisplay
                        RegionsControl.RefreshSelectedRegionGeometry();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG019, ex);
            }
        }

        /// <summary>
        /// Top changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            try
            {
                if (!_suppressPositionChangeEvents)
                {
                    ClearMessages();
                    double val = args.Value * 0.01;
                    if (_selectedRegions.Count > 0)
                    {
                        foreach (ScreenRegion region in _selectedRegions)
                        {
                            if (Math.Abs(region.Rectangle.Y - val) > 0.000001)
                            {
                                // Update region
                                Rect rect = region.Rectangle;
                                rect.Y = Math.Max(0.01 - rect.Height, Math.Min(1.0, val));
                                region.Rectangle = rect;
                            }
                        }

                        // Redisplay
                        RegionsControl.RefreshSelectedRegionGeometry();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG020, ex);
            }
        }

        /// <summary>
        /// Width changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WidthSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            try
            {
                ClearMessages();
                double val = args.Value * 0.01;
                if (_selectedRegions.Count > 0)
                {
                    foreach (ScreenRegion region in _selectedRegions)
                    {
                        if (Math.Abs(region.Rectangle.Width - val) > 0.000001)
                        {
                            // Update region
                            Rect rect = region.Rectangle;
                            rect.Width = Math.Max(0.01, Math.Min(1.0, val));
                            region.Rectangle = rect;
                        }
                    }

                    // Redisplay
                    RegionsControl.RefreshSelectedRegionGeometry();                    
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG021, ex);
            }
        }

        /// <summary>
        /// Height changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeightSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            try
            {
                ClearMessages();
                double val = args.Value * 0.01;
                if (_selectedRegions.Count > 0)
                {
                    foreach (ScreenRegion region in _selectedRegions)
                    {
                        if (Math.Abs(region.Rectangle.Height - val) > 0.000001)
                        {
                            // Update region
                            Rect rect = region.Rectangle;
                            rect.Height = Math.Max(0.01, Math.Min(1.0, val));
                            region.Rectangle = rect;
                        }
                    }

                    // Redisplay
                    RegionsControl.RefreshSelectedRegionGeometry();                    
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG022, ex);
            }
        }

        /// <summary>
        /// Shape changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShapeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ClearMessages();
                NamedItem selectedItem = ShapeComboBox.SelectedItem as NamedItem;
                if (selectedItem != null)
                {
                    EShape shape = (EShape)selectedItem.ID;
                    if (_selectedRegions.Count > 0)
                    {
                        foreach (ScreenRegion region in _selectedRegions)
                        {
                            region.Shape = shape;
                        }

                        // Redisplay
                        RegionsControl.RefreshSelectedRegionGeometry();

                        if (_selectedRegions.Count == 1)
                        {
                            // Show shape options according to shape type when single region is selected
                            EnableOrDisableShapeOptions();
                        }
                        else
                        {
                            // Show all shape options when multiple regions selected
                            HoleSizeSlider.IsEnabled = true;
                            StartAngleSlider.IsEnabled = true;
                            SweepAngleSlider.IsEnabled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG023, ex);
            }
        }

        /// <summary>
        /// Enable or disable hole and angle options according to the selected region shape
        /// </summary>
        private void EnableOrDisableShapeOptions()
        {
            bool enableHoleSize = false;
            bool enableAngles = false;
            NamedItem selectedItem = ShapeComboBox.SelectedItem as NamedItem;
            if (selectedItem != null)
            {
                EShape shape = (EShape)selectedItem.ID;
                switch (shape)
                {
                    case EShape.EllipseSector:
                        enableAngles = true;
                        break;
                    case EShape.Annulus:
                        enableHoleSize = true;
                        break;
                    case EShape.AnnulusSector:
                        enableHoleSize = true;
                        enableAngles = true;
                        break;
                }           
            }

            // Enable or disable controls
            HoleSizeSlider.IsEnabled = enableHoleSize;
            StartAngleSlider.IsEnabled = enableAngles;
            SweepAngleSlider.IsEnabled = enableAngles;
        }

        /// <summary>
        /// Hole size changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HoleSizeSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            try
            {
                ClearMessages();
                double val = args.Value * 0.01;
                if (_selectedRegions.Count > 0)
                {
                    foreach (ScreenRegion region in _selectedRegions)
                    {
                        if (Math.Abs(region.HoleSize - val) > 0.000001)
                        {
                            // Update region
                            val = Math.Max(0.01, Math.Min(0.99, val));
                            region.HoleSize = val;
                        }
                    }

                    // Redisplay
                    RegionsControl.RefreshSelectedRegionGeometry();
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG024, ex);
            }
        }

        /// <summary>
        /// Start angle changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StartAngleSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            try
            {
                ClearMessages();
                double val = args.Value;
                if (_selectedRegions.Count > 0)
                {
                    foreach (ScreenRegion region in _selectedRegions)
                    {
                        if (Math.Abs(region.StartAngle - val) > 0.000001)
                        {
                            // Update region
                            val = Math.Max(0.0, Math.Min(360.0, val));
                            region.StartAngle = val;
                        }
                    }

                    // Redisplay
                    RegionsControl.RefreshSelectedRegionGeometry();
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG025, ex);
            }
        }

        /// <summary>
        /// Sweep angle changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SweepAngleSlider_ValueChanged(object sender, DoubleValEventArgs args)
        {
            try
            {
                ClearMessages();
                double val = args.Value;
                if (_selectedRegions.Count > 0)
                {
                    foreach (ScreenRegion region in _selectedRegions)
                    {
                        if (Math.Abs(region.SweepAngle - val) > 0.000001)
                        {
                            // Update region
                            val = Math.Max(1.0, Math.Min(359.0, val));
                            region.SweepAngle = val;
                        }

                        // Redisplay
                        RegionsControl.RefreshSelectedRegionGeometry();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_REG026, ex);
            }
        }

        /// <summary>
        /// Show error message link
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        private void ShowError(string message, Exception ex)
        {
            if (_isLoaded)
            {
                ErrorMessage.Show(message, ex);
            }
        }

        /// <summary>
        /// Clear error message
        /// </summary>
        private void ClearMessages()
        {
            if (_isLoaded)
            {
                ErrorMessage.Clear();
            }
        }
    }
}
