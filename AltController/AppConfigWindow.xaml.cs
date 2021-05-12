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
using System.IO;
using System.Windows;
using AltController.Config;
using AltController.Core;
using AltController.Sys;

namespace AltController
{
    /// <summary>
    /// Personalised application options
    /// </summary>
    public partial class AppConfigWindow : Window
    {
        // Members
        private AppConfig _appConfig = new AppConfig();
        private NamedItemList _keyListItems = new NamedItemList();
        private NamedItemList _hotkeys = new NamedItemList();
        private bool _hotkeysChanged = false;
        private bool _selectionChanging = false;

        // Properties
        public bool HotkeysChanged { get { return _hotkeysChanged; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public AppConfigWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the current app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        /// <summary>
        /// Get the app config
        /// </summary>
        /// <returns></returns>
        public AppConfig GetAppConfig()
        {
            return _appConfig;
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Hotkey selection
            GUIUtils.PopulateDisplayableListWithKeys(_keyListItems);
            _keyListItems.Insert(0, new NamedItem(0, "None"));
            this.KeyboardKeyCombo.ItemsSource = _keyListItems;

            DisplayConfig();

            LanguageCombo.SelectedLanguageChanged += this.LanguageCombo_SelectedLanguageChanged;
        }

        /// <summary>
        /// Display config
        /// </summary>
        private void DisplayConfig() 
        {
            // Start up
            this.LanguageCombo.SelectedLanguage = _appConfig.GetStringVal(Constants.ConfigLanguageCode, Constants.DefaultLanguageCode);
            this.AutoLoadCheckbox.IsChecked = _appConfig.GetBoolVal(Constants.ConfigAutoLoadLastProfile, Constants.DefaultAutoLoadLastProfile);
            this.AutoOpenCustomWindowsCheckbox.IsChecked = _appConfig.GetBoolVal(Constants.ConfigAutoOpenCustomWindows, Constants.DefaultAutoOpenCustomWindows);
            this.DrawScreenRegionsCheckbox.IsChecked = _appConfig.GetBoolVal(Constants.ConfigDrawScreenRegions, Constants.DefaultDrawScreenRegions);
            this.DrawScreenRegionNamesCheckbox.IsChecked = _appConfig.GetBoolVal(Constants.ConfigShowScreenRegionNames, Constants.DefaultShowScreenRegionNames);
            this.DrawPointerIndicatorCheckbox.IsChecked = _appConfig.GetBoolVal(Constants.ConfigDrawPointerIndicatorLine, Constants.DefaultDrawPointerIndicatorLine);
            this.DrawStateOverlayCheckbox.IsChecked = _appConfig.GetBoolVal(Constants.ConfigDrawStateOverlay, Constants.DefaultDrawStateOverlay);
            this.CustomWindowTitleBarsCheckbox.IsChecked = _appConfig.GetBoolVal(Constants.ConfigCustomWindowTitleBars, Constants.DefaultCustomWindowTitleBars);
            this.DrawRegionForceSquareCheckbox.IsChecked = _appConfig.GetBoolVal(Constants.ConfigDrawRegionForceSquare, Constants.DefaultDrawRegionForceSquare);

            // Timing
            this.InputPollingIntervalSlider.Value = 0.001 * _appConfig.GetIntVal(Constants.ConfigInputPollingIntervalMS, Constants.DefaultInputPollingIntervalMS);
            this.UIUpdateIntervalSlider.Value = 0.001 * _appConfig.GetIntVal(Constants.ConfigUIUpdateIntervalMS, Constants.DefaultUIUpdateIntervalMS);
            this.DwellTimeSlider.Value = 0.001 * _appConfig.GetIntVal(Constants.ConfigDwellTimeMS, Constants.DefaultDwellTimeMS);

            // Actions
            this.AutoStopPressCheckbox.IsChecked = _appConfig.GetBoolVal(Constants.ConfigAutoStopPressActions, Constants.DefaultAutoStopPressActions);
            this.AutoStopInsideCheckbox.IsChecked = _appConfig.GetBoolVal(Constants.ConfigAutoStopInsideActions, Constants.DefaultAutoStopInsideActions);
            if (_appConfig.GetBoolVal(Constants.ConfigUseScanCodes, Constants.DefaultUseScanCodes))
            {
                UseScanCodesRadioButton.IsChecked = true;
            }
            else
            {
                VirtualKeysRadioButton.IsChecked = true;
            }

            // Display
            if (_appConfig.GetIntVal(Constants.ConfigPointerIndicatorStyle, Constants.DefaultPointerIndicatorStyle) == Constants.PointerCircle)
            {
                CircleRadioButton.IsChecked = true;
            }
            else
            {
                LineRadioButton.IsChecked = true;
            }
            this.PointerIndicatorColourCombo.SelectedColour = _appConfig.GetStringVal(Constants.ConfigPointerIndicatorColour, Constants.DefaultPointerIndicatorColour);
            this.PointerIndicatorRadiusSlider.Value = _appConfig.GetIntVal(Constants.ConfigPointerIndicatorRadius, Constants.DefaultPointerIndicatorRadius);
            this.StateOverlayBgColourCombo.SelectedColour = _appConfig.GetStringVal(Constants.ConfigStateOverlayBgColour, Constants.DefaultStateOverlayBgColour);
            this.StateOverlayTextColourCombo.SelectedColour = _appConfig.GetStringVal(Constants.ConfigStateOverlayTextColour, Constants.DefaultStateOverlayTextColour);
            this.StateOverlayTranslucencySlider.Value = 100.0 * _appConfig.GetDoubleVal(Constants.ConfigStateOverlayTranslucency, Constants.DefaultStateOverlayTranslucency);
            this.StateOverlayFontSizeSlider.Value = _appConfig.GetDoubleVal(Constants.ConfigStateOverlayFontSize, Constants.DefaultStateOverlayFontSize);
            this.StateOverlayXPosSlider.Value = 100.0 * _appConfig.GetDoubleVal(Constants.ConfigStateOverlayXPos, Constants.DefaultStateOverlayXPos);
            this.StateOverlayYPosSlider.Value = 100.0 * _appConfig.GetDoubleVal(Constants.ConfigStateOverlayYPos, Constants.DefaultStateOverlayYPos);

            // Hotkeys
            _hotkeys.Clear();
            int drawRegionsHotkey = _appConfig.GetIntVal(Constants.ConfigDrawScreenRegionsHotkey, 0);
            _hotkeys.Add(new NamedItem(drawRegionsHotkey, Properties.Resources.String_Toggle_screen_regions));
            int regionNamesHotkey = _appConfig.GetIntVal(Constants.ConfigShowScreenRegionNamesHotkey, 0);
            _hotkeys.Add(new NamedItem(regionNamesHotkey, Properties.Resources.String_Toggle_region_names));
            int indicatorLineHotkey = _appConfig.GetIntVal(Constants.ConfigDrawPointerIndicatorLineHotkey, 0);
            _hotkeys.Add(new NamedItem(indicatorLineHotkey, Properties.Resources.String_Toggle_pointer_indicator));
            int stateOverlayHotkey = _appConfig.GetIntVal(Constants.ConfigDrawStateOverlayHotkey, 0);
            _hotkeys.Add(new NamedItem(stateOverlayHotkey, Properties.Resources.String_Toggle_state_overlay));
            int titleBarsHotkey = _appConfig.GetIntVal(Constants.ConfigCustomWindowTitleBarsHotkey, 0);
            _hotkeys.Add(new NamedItem(titleBarsHotkey, Properties.Resources.String_Toggle_title_bars));
            this.HotkeyList.DataContext = _hotkeys;
            this.HotkeyList.SelectedItem = _hotkeys[0];

            // Folders
            string defaultProfilesDir = Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
            this.ProfilesFolderTextBox.Text = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);
        }

        /// <summary>
        /// Reset to defaults
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.OK == MessageBox.Show(
                Properties.Resources.String_Reset_options_description, 
                Properties.Resources.String_Reset_options_title,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel))
            {
                // Remember certain settings
                string defaultProfilesDir = Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                string folder = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);
                string lastUsedProfile = _appConfig.GetStringVal(Constants.ConfigLastUsedProfile, null);
                double dpiX = _appConfig.GetDoubleVal(Constants.ConfigDPIXSetting, 1.0);
                double dpiY = _appConfig.GetDoubleVal(Constants.ConfigDPIYSetting, 1.0);

                // Reset config
                _appConfig = new AppConfig();

                // Keep certain settings
                _appConfig.SetStringVal(Constants.ConfigProfilesDir, folder);
                _appConfig.SetStringVal(Constants.ConfigLastUsedProfile, lastUsedProfile);
                _appConfig.SetDoubleVal(Constants.ConfigDPIXSetting, dpiX);
                _appConfig.SetDoubleVal(Constants.ConfigDPIYSetting, dpiY);

                // Force hotkey update
                _hotkeysChanged = true;

                // Refresh display
                DisplayConfig();
            }
        }

        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Start up
            _appConfig.SetStringVal(Constants.ConfigLanguageCode, this.LanguageCombo.SelectedLanguage);
            _appConfig.SetBoolVal(Constants.ConfigAutoLoadLastProfile, this.AutoLoadCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigAutoOpenCustomWindows, this.AutoOpenCustomWindowsCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigDrawScreenRegions, this.DrawScreenRegionsCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigShowScreenRegionNames, this.DrawScreenRegionNamesCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigDrawPointerIndicatorLine, this.DrawPointerIndicatorCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigDrawStateOverlay, this.DrawStateOverlayCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigCustomWindowTitleBars, this.CustomWindowTitleBarsCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigDrawRegionForceSquare, this.DrawRegionForceSquareCheckbox.IsChecked == true);

            // Timing
            _appConfig.SetIntVal(Constants.ConfigInputPollingIntervalMS, (int)(this.InputPollingIntervalSlider.Value * 1000));
            _appConfig.SetIntVal(Constants.ConfigUIUpdateIntervalMS, (int)(this.UIUpdateIntervalSlider.Value * 1000));
            _appConfig.SetIntVal(Constants.ConfigDwellTimeMS, (int)(this.DwellTimeSlider.Value * 1000));

            // Actions
            _appConfig.SetBoolVal(Constants.ConfigAutoStopPressActions, this.AutoStopPressCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigAutoStopInsideActions, this.AutoStopInsideCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigUseScanCodes, this.UseScanCodesRadioButton.IsChecked == true);

            // Display
            _appConfig.SetIntVal(Constants.ConfigPointerIndicatorStyle, this.CircleRadioButton.IsChecked == true ? Constants.PointerCircle : Constants.PointerLine);
            _appConfig.SetStringVal(Constants.ConfigPointerIndicatorColour, this.PointerIndicatorColourCombo.SelectedColour);
            _appConfig.SetIntVal(Constants.ConfigPointerIndicatorRadius, (int)this.PointerIndicatorRadiusSlider.Value);
            _appConfig.SetStringVal(Constants.ConfigStateOverlayBgColour, this.StateOverlayBgColourCombo.SelectedColour);
            _appConfig.SetStringVal(Constants.ConfigStateOverlayTextColour, this.StateOverlayTextColourCombo.SelectedColour);
            _appConfig.SetDoubleVal(Constants.ConfigStateOverlayTranslucency, 0.01 * this.StateOverlayTranslucencySlider.Value);
            _appConfig.SetDoubleVal(Constants.ConfigStateOverlayFontSize, this.StateOverlayFontSizeSlider.Value);
            _appConfig.SetDoubleVal(Constants.ConfigStateOverlayXPos, 0.01 * this.StateOverlayXPosSlider.Value);
            _appConfig.SetDoubleVal(Constants.ConfigStateOverlayYPos, 0.01 * this.StateOverlayYPosSlider.Value);

            // Hotkeys
            _appConfig.SetIntVal(Constants.ConfigDrawScreenRegionsHotkey, (int)_hotkeys[0].ID);
            _appConfig.SetIntVal(Constants.ConfigShowScreenRegionNamesHotkey, (int)_hotkeys[1].ID);
            _appConfig.SetIntVal(Constants.ConfigDrawPointerIndicatorLineHotkey, (int)_hotkeys[2].ID);
            _appConfig.SetIntVal(Constants.ConfigDrawStateOverlayHotkey, (int)_hotkeys[3].ID);
            _appConfig.SetIntVal(Constants.ConfigCustomWindowTitleBarsHotkey, (int)_hotkeys[4].ID);

            // Folders
            string defaultProfilesDir = Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
            string profilesDir = ProfilesFolderTextBox.Text;
            if (profilesDir != defaultProfilesDir)
            {
                _appConfig.SetStringVal(Constants.ConfigProfilesDir, profilesDir);
            }
            else
            {
                _appConfig.SetStringVal(Constants.ConfigProfilesDir, null);
            }            

            // Config changed
            DialogResult = true;

            this.Close();
        }

        /// <summary>
        /// Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Choose a profiles folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {            
            DirectoryInfo di = new DirectoryInfo(ProfilesFolderTextBox.Text);
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (di.Exists)
            {
                dialog.SelectedPath = di.FullName;
            }
            dialog.Description = string.Format(Properties.Resources.String_Choose_folder_description, Constants.ApplicationName);
            dialog.ShowNewFolderButton = true;
            if (System.Windows.Forms.DialogResult.OK == dialog.ShowDialog())
            {
                ProfilesFolderTextBox.Text = dialog.SelectedPath;
                ProfilesFolderTextBox.ToolTip = dialog.SelectedPath;
            }           

            e.Handled = true;
        }

        /// <summary>
        /// Hotkey key changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyboardKeyCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            NamedItem keyItem = (NamedItem)KeyboardKeyCombo.SelectedItem;
            long key = keyItem != null ? (long)keyItem.ID : 0L;
            bool enableModifiers = key != 0L;

            // Update selected hotkey
            NamedItem item = (NamedItem)HotkeyList.SelectedItem;
            if (item != null && !_selectionChanging)
            {
                long modifiers = enableModifiers ? item.ID & 0xFFFF : 0L;
                item.ID = (key << 16) | modifiers;
                _hotkeysChanged = true;
            }

            // Enable or disable modifiers
            AltCheck.IsEnabled = enableModifiers;
            ControlCheck.IsEnabled = enableModifiers;
            ShiftCheck.IsEnabled = enableModifiers;
            WinCheck.IsEnabled = enableModifiers;

            // Clear modifiers if no key selected
            if (!enableModifiers)
            {
                AltCheck.IsChecked = false;
                ControlCheck.IsChecked = false;
                ShiftCheck.IsChecked = false;
                WinCheck.IsChecked = false;
            }
        }

        /// <summary>
        /// Hotkey modifier changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotkeyModifierChanged(object sender, RoutedEventArgs e)
        {
            // Update selected hotkey
            if (!_selectionChanging)
            {
                NamedItem item = (NamedItem)HotkeyList.SelectedItem;
                if (item != null && item.ID != 0L)
                {
                    long key = item.ID >> 16;
                    long modifiers = item.ID & 0xFFFF;
                    if (sender == AltCheck)
                    {
                        modifiers ^= WindowsAPI.MOD_ALT;
                    }
                    else if (sender == ControlCheck)
                    {
                        modifiers ^= WindowsAPI.MOD_CONTROL;
                    }
                    else if (sender == ShiftCheck)
                    {
                        modifiers ^= WindowsAPI.MOD_SHIFT;
                    }
                    else if (sender == WinCheck)
                    {
                        modifiers ^= WindowsAPI.MOD_WIN;
                    }
                    item.ID = (key << 16) | modifiers;
                }
                _hotkeysChanged = true;
            }
        }

        /// <summary>
        /// Selected hotkey row chaanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotkeyList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            NamedItem item = (NamedItem)HotkeyList.SelectedItem;
            if (item != null)
            {
                _selectionChanging = true;
                KeyboardKeyCombo.SelectedValue = item.ID >> 16;
                AltCheck.IsChecked = (item.ID & WindowsAPI.MOD_ALT) != 0;
                ControlCheck.IsChecked = (item.ID & WindowsAPI.MOD_CONTROL) != 0;
                ShiftCheck.IsChecked = (item.ID & WindowsAPI.MOD_SHIFT) != 0;
                WinCheck.IsChecked = (item.ID & WindowsAPI.MOD_WIN) != 0;
                _selectionChanging = false;
            }
        }

        /// <summary>
        /// Pointer indicator type changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerIndicatorType_Changed(object sender, RoutedEventArgs e)
        {
            PointerIndicatorRadiusSlider.IsEnabled = CircleRadioButton.IsChecked == true;
        }

        /// <summary>
        /// Handle language change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LanguageCombo_SelectedLanguageChanged(object sender, RoutedEventArgs e)
        {
            RestartRequiredLabel.Visibility = Visibility.Visible;
        }
    }
}
