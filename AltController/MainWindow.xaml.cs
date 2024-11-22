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
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using AltController.Actions;
using AltController.Core;
using AltController.Config;
using AltController.Event;
using AltController.Input;
using AltController.Sys;
using Microsoft.Win32;
using System.Text;
//using System.Diagnostics;

namespace AltController
{
    /// <summary>
    /// Main application window
    /// </summary>
    public partial class MainWindow : Window, IParentWindow
    {
        // Threading
        private double _uiUpdateIntervalMS = Constants.DefaultUIUpdateIntervalMS;
        private int _waitForExitMS = 200;
        private DispatcherTimer _timer = new DispatcherTimer();
        private ThreadManager _manager = new ThreadManager();

        // Configuration
        private Profile _profile;
        private AppConfig _appConfig = new AppConfig();
        private LogicalState _logicalState = new LogicalState();
        private Dictionary<string, uint> _hotkeys = new Dictionary<string, uint>();

        // Windows
        private IntPtr _windowHandle = IntPtr.Zero;
        private HwndSource _source = null;
        private EditProfileWindow _editorDialog;
        private ProfileSummaryWindow _profileSummaryDialog;
        private DiagnosticsWindow _logInformationDialog;
        private OverlayWindow _overlayDialog;
        private HelpAboutWindow _helpAboutWindow;
        private Dictionary<long, CustomWindow> _openCustomWindows = new Dictionary<long,CustomWindow>();
        private CustomMessageBox _confirmCommandMessageBox;
        private CustomMessageBox _commandBlockedMessageBox;

        // State
        private bool _isExiting = false;
        private bool _profileChanged = false;
        private bool _newCanExecute = true;
        private bool _openCanExecute = true;
        private bool _saveAsCanExecute = true;
        private double _dpiX = 1.0;
        private double _dpiY = 1.0;
        private StartProgramAction _pendingStartProgramAction;

        // Status display
        private EventReportHandler EventReported;

        // Commands
        public static RoutedCommand OpenCustomWindow1 = new RoutedCommand();
        public static RoutedCommand OpenCustomWindow2 = new RoutedCommand();
        public static RoutedCommand OpenCustomWindow3 = new RoutedCommand();
        public static RoutedCommand OpenCustomWindow4 = new RoutedCommand();
        public static RoutedCommand OpenAllCustomWindows = new RoutedCommand();

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            try
            {
                // Load the application config
                _appConfig.Load();

                // Set UI language
                SetUILanguage();
            }
            catch (Exception)
            {
            }

            InitializeComponent();
        }

        /// <summary>
        /// Handle Windows messages
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case WindowsAPI.HOTKEY_ID:
                            int hotkey = (int)lParam;
                            //Trace.WriteLine(string.Format("Hotkey pressed: keyCode {0} modifiers {1}", (System.Windows.Forms.Keys)(hotkey >> 16), hotkey & 0xFFFF));
                            Dictionary<string, uint>.Enumerator eHotkey = _hotkeys.GetEnumerator();
                            while (eHotkey.MoveNext())
                            {
                                if (hotkey == eHotkey.Current.Value)
                                {
                                    switch (eHotkey.Current.Key)
                                    {
                                        case Constants.ConfigDrawScreenRegionsHotkey:
                                            ViewDrawScreenRegions.IsChecked = !ViewDrawScreenRegions.IsChecked;
                                            break;
                                        case Constants.ConfigShowScreenRegionNamesHotkey:
                                            ViewShowScreenRegionNames.IsChecked = !ViewShowScreenRegionNames.IsChecked;
                                            break;
                                        case Constants.ConfigDrawPointerIndicatorLineHotkey:
                                            ViewDrawPointerIndicator.IsChecked = !ViewDrawPointerIndicator.IsChecked;
                                            break;
                                        case Constants.ConfigDrawStateOverlayHotkey:
                                            ViewDrawStateOverlay.IsChecked = !ViewDrawStateOverlay.IsChecked;
                                            break;
                                        case Constants.ConfigCustomWindowTitleBarsHotkey:
                                            WindowShowTitleBars.IsChecked = !WindowShowTitleBars.IsChecked;
                                            break;
                                    }
                                }
                            }
                            //handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Register a method to handle input events
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void AttachEventReportHandler(EventReportHandler handler)
        {
            EventReported += handler;
        }

        /// <summary>
        /// Deregister a method to handle input events
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void DetachEventReportHandler(EventReportHandler handler)
        {
            EventReported -= handler;
        }

        /// <summary>
        /// Enable or disable diagnostics
        /// </summary>
        /// <param name="enable"></param>
        public void ConfigureDiagnostics(bool enable)
        {
            _manager.ConfigureDiagnostics(enable);
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
                ClearMessages();

                // Create user data folder if required
                if (!Directory.Exists(AppConfig.UserDataDir))
                {
                    Directory.CreateDirectory(AppConfig.UserDataDir);
                }

                // First time initialisation
                if (!AppConfig.FileExists())
                {
                    CreateProfilesDir();
                    CopySampleProfiles();
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN001, ex);
            }

            try {
                // Store update rate
                _uiUpdateIntervalMS = _appConfig.GetIntVal(Constants.ConfigUIUpdateIntervalMS, Constants.DefaultUIUpdateIntervalMS);

                // Create default profile
                _profile = CreateDefaultProfile();

                // Set window title
                this.Title = Constants.ApplicationName;

                // Position window
                PositionWindow();

                // Register hotkeys
                RegisterHotkeys();

                // Initialise display options
                this.ViewDrawScreenRegions.IsChecked = _appConfig.GetBoolVal(Constants.ConfigDrawScreenRegions, Constants.DefaultDrawScreenRegions);
                this.ViewShowScreenRegionNames.IsChecked = _appConfig.GetBoolVal(Constants.ConfigShowScreenRegionNames, Constants.DefaultShowScreenRegionNames);
                this.ViewDrawPointerIndicator.IsChecked = _appConfig.GetBoolVal(Constants.ConfigDrawPointerIndicatorLine, Constants.DefaultDrawPointerIndicatorLine);
                this.ViewDrawStateOverlay.IsChecked = _appConfig.GetBoolVal(Constants.ConfigDrawStateOverlay, Constants.DefaultDrawStateOverlay);
                this.WindowShowTitleBars.IsChecked = _appConfig.GetBoolVal(Constants.ConfigCustomWindowTitleBars, Constants.DefaultCustomWindowTitleBars);

                // Tell this window to handle a change in logical state
                AttachEventReportHandler(HandleEventReport);

                // Get display DPI
                InitialiseDPISettings();

                // Subscribe to display setting changes
                SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

                // Configure state manager
                ConfigureStateManager();
                
                // Load the last used profile
                LoadProfile(null);

                // Start monitoring inputs
                StartMonitoringSources();

                // Start updating the GUI
                StartTimer();
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN002, ex);
            }
        }

        /// <summary>
        /// Update the list of recent profiles
        /// </summary>
        /// <param name="loadedProfile"></param>
        private void UpdateRecentProfiles(string loadedProfile)
        {
            // Add new profile to the recent profiles list
            List<string> recentProfilesList = new List<string>();
            int count = 0;
            if (!string.IsNullOrEmpty(loadedProfile))
            {
                recentProfilesList.Add(loadedProfile);
                count++;
            }
            string recentProfilesStr = _appConfig.GetStringVal(Constants.ConfigUserRecentProfilesList, "");
            string[] recentProfilesArray = recentProfilesStr.Split(',');
            foreach (string recentProfile in recentProfilesArray)
            {
                if (recentProfile != "" && !recentProfilesList.Contains(recentProfile))
                {
                    recentProfilesList.Add(recentProfile);
                    if (++count == Constants.MaxRecentProfiles)
                    {
                        break;
                    }
                }
            }

            // If a new profile has been added to the list, save it back to the app config
            if (!string.IsNullOrEmpty(loadedProfile))
            {
                // Convert to string
                StringBuilder sb = new StringBuilder();
                bool first = true;
                foreach (string path in recentProfilesList)
                {
                    if (!first)
                    {
                        sb.Append(',');
                    }
                    sb.Append(path);
                    first = false;
                }

                // Update config
                _appConfig.SetStringVal(Constants.ConfigUserRecentProfilesList, sb.ToString());
            }

            // Refresh recent profiles menu
            RefreshRecentProfilesMenu(recentProfilesList);
        }

        /// <summary>
        /// Update the recent profiles list
        /// </summary>
        /// <param name="recentProfilesList"></param>
        private void RefreshRecentProfilesMenu(List<string> recentProfilesList)
        {
            int menuItemCount = 0;
            RecentFiles.Items.Clear();
            foreach (string filePath in recentProfilesList)
            {
                try
                {
                    // Check that the file exists and get the file name
                    FileInfo fi = new FileInfo(filePath);
                    if (fi.Exists)
                    {
                        // Get the display name
                        string fileName = fi.Name;
                        if (fileName.EndsWith(Constants.ProfileFileExtension))
                        {
                            fileName = fileName.Substring(0, fileName.Length - Constants.ProfileFileExtension.Length);
                        }

                        menuItemCount++;

                        MenuItem menuItem = new MenuItem();
                        menuItem.Tag = fi.FullName;                        
                        menuItem.ToolTip = fi.FullName;
                        menuItem.Header = string.Format("_{0} {1}", menuItemCount, fileName);
                        menuItem.Click += this.LoadRecentProfile_Click;
                        RecentFiles.Items.Add(menuItem);
                    }
                }
                catch (Exception)
                {
                    // Ignore errors
                }
            }

            Visibility visibility = menuItemCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            RecentFilesSeparator.Visibility = visibility;
            RecentFiles.Visibility = visibility;
        }

        /// <summary>
        /// Load a recent profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadRecentProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuItem)
                {
                    MenuItem menuItem = (MenuItem)sender;
                    if (menuItem.Tag is string)
                    {
                        string filePath = (string)menuItem.Tag;
                        LoadProfile(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN016, ex);
            }
        }

        /// <summary>
        /// Set the window position
        /// </summary>
        private void PositionWindow()
        {
            try
            {
                string positionKey = "window_pos_main";
                if (System.Windows.Forms.SystemInformation.MonitorCount > 1)
                {
                    positionKey += "_multiscreen";
                }
                string posStr = _appConfig.GetStringVal(positionKey, "");
                if (posStr != "")
                {
                    Point windowPos = Point.Parse(posStr);
                    if (windowPos.X >= 0.0 && windowPos.X < SystemParameters.VirtualScreenWidth &&
                        windowPos.Y >= 0.0 && windowPos.Y < SystemParameters.VirtualScreenHeight)
                    {
                        this.Left = windowPos.X;
                        this.Top = windowPos.Y;
                    }
                }
                else
                {
                    // Position somewhere centre-left by default, out of the way of any custom windows
                    this.Left = 0.3 * SystemParameters.PrimaryScreenWidth;
                    this.Top = 0.4 * SystemParameters.PrimaryScreenHeight;
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN003, ex);
            }
        }

        /// <summary>
        /// Window moved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_LocationChanged(object sender, EventArgs e)
        {            
            if (IsLoaded && 
                !double.IsNaN(this.Left) && !double.IsNaN(this.Top) &&
                this.Left >= 0.0 && this.Top >= 0.0)
            {
                string positionKey = "window_pos_main";
                if (System.Windows.Forms.SystemInformation.MonitorCount > 1)
                {
                    positionKey += "_multiscreen";
                }
                Point windowPos = new Point(this.Left, this.Top);
                _appConfig.SetStringVal(positionKey, windowPos.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Send the app config to the state manager thread
        /// </summary>
        private void ConfigureStateManager()
        {
            AppConfig configCopy = new AppConfig();
            configCopy.FromXml(_appConfig.ToXml());
            _manager.SetAppConfig(configCopy);
        }

        /// <summary>
        /// Register or re-register hotkeys
        /// </summary>
        private void RegisterHotkeys()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                WindowsAPI.UnregisterHotKey(_windowHandle, WindowsAPI.HOTKEY_ID);
            }

            // Read hotkey config
            ReadHotkeyConfig();

            HotkeyBindingConverter converter = new HotkeyBindingConverter();
            Dictionary<string, uint>.Enumerator eHotkey = _hotkeys.GetEnumerator();
            while (eHotkey.MoveNext())
            {
                uint hotkey = eHotkey.Current.Value;
                string description = "";
                uint modifiers = hotkey & 0xFFFF;
                uint keyCode = hotkey >> 16;
                if (keyCode != 0)
                {
                    if (_windowHandle == IntPtr.Zero)
                    {
                        _windowHandle = new WindowInteropHelper(this).Handle;
                        _source = HwndSource.FromHwnd(_windowHandle);
                        _source.AddHook(HwndHook);
                    }

                    // Register hotkeys
                    if (WindowsAPI.RegisterHotKey(_windowHandle, WindowsAPI.HOTKEY_ID, modifiers, keyCode))
                    {
                        description = "\t" + (string)converter.Convert((long)hotkey, typeof(string), null, CultureInfo.InvariantCulture);
                    }
                }

                // Show shortcuts next to View menu items
                switch (eHotkey.Current.Key)
                {
                    case Constants.ConfigDrawScreenRegionsHotkey:
                        ViewDrawScreenRegions.Header = Properties.Resources.Main_ViewDrawScreenRegions + description;
                        break;
                    case Constants.ConfigShowScreenRegionNamesHotkey:
                        ViewShowScreenRegionNames.Header = Properties.Resources.Main_ViewDrawScreenRegionNames + description;
                        break;
                    case Constants.ConfigDrawPointerIndicatorLineHotkey:
                        ViewDrawPointerIndicator.Header = Properties.Resources.Main_ViewDrawPointerIndicator + description;
                        break;
                    case Constants.ConfigDrawStateOverlayHotkey:
                        ViewDrawStateOverlay.Header = Properties.Resources.Main_ViewDrawStateOverlay + description;
                        break;
                    case Constants.ConfigCustomWindowTitleBarsHotkey:
                        WindowShowTitleBars.Header = Properties.Resources.Main_WindowShowTitleBars + description;
                        break;
                }
            }
        }

        /// <summary>
        /// Read the hotkey configuration from the application config
        /// </summary>
        private void ReadHotkeyConfig()
        {
            _hotkeys.Clear();
            _hotkeys[Constants.ConfigDrawScreenRegionsHotkey] = (uint)_appConfig.GetIntVal(Constants.ConfigDrawScreenRegionsHotkey, 0);
            _hotkeys[Constants.ConfigShowScreenRegionNamesHotkey] = (uint)_appConfig.GetIntVal(Constants.ConfigShowScreenRegionNamesHotkey, 0);
            _hotkeys[Constants.ConfigDrawPointerIndicatorLineHotkey] = (uint)_appConfig.GetIntVal(Constants.ConfigDrawPointerIndicatorLineHotkey, 0);
            _hotkeys[Constants.ConfigDrawStateOverlayHotkey] = (uint)_appConfig.GetIntVal(Constants.ConfigDrawStateOverlayHotkey, 0);
            _hotkeys[Constants.ConfigCustomWindowTitleBarsHotkey] = (uint)_appConfig.GetIntVal(Constants.ConfigCustomWindowTitleBarsHotkey, 0);
        }

        /// <summary>
        /// Deregister hotkeys
        /// </summary>
        private void UnregisterHotkeys()
        {
            if (_source != null && _windowHandle != IntPtr.Zero)
            {
                _source.RemoveHook(HwndHook);
                WindowsAPI.UnregisterHotKey(_windowHandle, WindowsAPI.HOTKEY_ID);
                _source = null;
                _windowHandle = IntPtr.Zero;
            }
        }


        /// <summary>
        /// Handle display settings change in Control Panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            // Store display DPI
            if (InitialiseDPISettings())
            {
                // Reapply app config
                ConfigureStateManager();
            }

            // Reconfigure overlay window
            if (_overlayDialog != null)
            {
                _overlayDialog.InitialiseDisplaySettings();
            }
        }

        /// <summary>
        /// Get display DPI
        /// </summary>
        private bool InitialiseDPISettings()
        {
            bool success = false;
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                _dpiX = source.CompositionTarget.TransformToDevice.M11;
                _dpiY = source.CompositionTarget.TransformToDevice.M22;

                _appConfig.SetDoubleVal(Constants.ConfigDPIXSetting, _dpiX);
                _appConfig.SetDoubleVal(Constants.ConfigDPIYSetting, _dpiY);

                success = true;
            }

            return success;
        }

        /// <summary>
        /// Create the user's profiles directory if it doesn't exist
        /// </summary>
        private void CreateProfilesDir()
        {
            string defaultProfilesDir = Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
            string profilesDir = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);
            if (!Directory.Exists(profilesDir))
            {
                Directory.CreateDirectory(profilesDir);
            }
        }

        /// <summary>
        /// Copy the sample profiles to user's profiles directory
        /// </summary>
        private void CopySampleProfiles()
        {
            // Check we're not running in dev environment
            if (AppConfig.BaseDir != AppConfig.UserDataDir)
            {
                // Get the user's profiles directory
                string defaultProfilesDir = Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                string destDir = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);

                // Get the sample profiles directory
                string sourceDir = Path.Combine(AppConfig.BaseDir, Constants.ProfilesFolderName);
                DirectoryInfo sourceDirInfo = new System.IO.DirectoryInfo(sourceDir);
                if (sourceDirInfo.Exists && Directory.Exists(destDir))
                {
                    FileInfo[] sourceFiles = sourceDirInfo.GetFiles("*.*");

                    // Deploy missing sample profiles
                    foreach (FileInfo sourceFile in sourceFiles)
                    {
                        string destFilePath = Path.Combine(destDir, sourceFile.Name);
                        FileInfo destFile = new FileInfo(destFilePath);
                        if (!destFile.Exists)
                        {
                            File.Copy(sourceFile.FullName, destFilePath, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Ask the user whether they wish to save if appropriate
            MessageBoxResult result = MessageBoxResult.None;
            if (_profileChanged)
            {
                result = System.Windows.MessageBox.Show(
                    Properties.Resources.String_Save_profile_description, 
                    Properties.Resources.String_Save_profile_title, 
                    MessageBoxButton.YesNoCancel, 
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    this.SaveExecuted(sender, null);
                }
            }

            // Close app unless user cancelled
            if (result != MessageBoxResult.Cancel)
            {
                _isExiting = true;
                UnregisterHotkeys();
                CloseChildWindows();
                StopTimer();
                StopMonitoringSources();
                DetachEventReportHandler(HandleEventReport);
                _appConfig.Save();
            }
            else
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Can New command execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _newCanExecute;
            e.Handled = true;
        }

        /// <summary>
        /// File New
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ClearMessages();

                // Ask the user whether they wish to save if appropriate
                MessageBoxResult result = MessageBoxResult.None;
                if (_profileChanged)
                {
                    result = System.Windows.MessageBox.Show(
                                                        Properties.Resources.String_Save_profile_description,
                                                        Properties.Resources.String_Save_profile_title,
                                                        MessageBoxButton.YesNoCancel,
                                                        MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        this.SaveExecuted(sender, e);
                    }
                }

                if (result != MessageBoxResult.Cancel)
                {
                    // Create a new profile
                    Profile profile = CreateDefaultProfile();

                    // Show the profile name
                    this.ProfileLabel.Text = profile.Name;
                    this.ProfileLabel.ToolTip = profile.Name;

                    // Clear last used profile in app settings
                    _appConfig.SetStringVal(Constants.ConfigLastUsedProfile, null);

                    // Reset the logical state
                    _logicalState = new LogicalState();

                    // Apply the profile
                    ApplyNewProfile(profile);

                    _profileChanged = false;
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN004, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Create default profile
        /// </summary>
        /// <returns></returns>
        private Profile CreateDefaultProfile()
        {
            // Create a new profile
            Profile profile = new Profile();

            // Add mouse and keyboard sources
            Utils utils = new Utils();
            profile.AddInput(new MouseSource(1L, utils.GetSourceTypeName(ESourceType.Mouse)));
            profile.AddInput(new KeyboardSource(2L, utils.GetSourceTypeName(ESourceType.Keyboard)));

            return profile;
        }

        /// <summary>
        /// Can Open command execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _openCanExecute;
            e.Handled = true;
        }

        /// <summary>
        /// File Open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ClearMessages();

                // Ask the user whether they wish to save if appropriate
                MessageBoxResult result = MessageBoxResult.None;
                if (_profileChanged)
                {
                    result = System.Windows.MessageBox.Show(
                                                        Properties.Resources.String_Save_profile_description,
                                                        Properties.Resources.String_Save_profile_title,
                                                        MessageBoxButton.YesNoCancel,
                                                        MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        this.SaveExecuted(sender, e);
                    }
                }

                if (result != MessageBoxResult.Cancel)
                {
                    System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                    string defaultProfilesFolder = Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                    dialog.InitialDirectory = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesFolder);
                    dialog.Multiselect = false;
                    dialog.Filter = Properties.Resources.String_Profile_files_filter;
                    dialog.CheckFileExists = true;
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        // Load the selected profile
                        LoadProfile(dialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN005, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Can Save command execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _profileChanged;
            e.Handled = true;
        }

        /// <summary>
        /// File Save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ClearMessages();

                // Get the currently loaded profile path
                string filePath = _appConfig.GetStringVal(Constants.ConfigLastUsedProfile, null);

                // Save the profile
                if (filePath != null && File.Exists(filePath))
                {
                    _profile.ToFile(filePath);

                    // Disable the save button
                    _profileChanged = false;
                }
                else
                {
                    // No existing profile, so launch save dialog
                    SaveAsExecuted(sender, e);
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN006, ex);
            }

            if (e != null)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Can Save As command execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _saveAsCanExecute;
            e.Handled = true;
        }

        /// <summary>
        /// File Save As
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ClearMessages();

                string defaultProfilesDir = Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                string profilesDir = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);

                System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
                dialog.InitialDirectory = profilesDir;
                dialog.CheckPathExists = true;
                dialog.DefaultExt = Constants.ProfileFileExtension;
                dialog.Filter = Properties.Resources.String_Profile_files_filter;
                string filePath = _appConfig.GetStringVal(Constants.ConfigLastUsedProfile, null);
                if (filePath != null)
                {
                    FileInfo fi = new FileInfo(filePath);
                    if (fi.Exists) {
                        dialog.FileName = fi.Name;
                    }
                }

                // Show the save dialog
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Save the profile
                    _profile.ToFile(dialog.FileName);

                    // Update the profile name
                    FileInfo fi = new FileInfo(dialog.FileName);
                    _profile.Name = fi.Name.Replace(Constants.ProfileFileExtension, "");
                    //_profile.Directory = fi.DirectoryName;
                    this.ProfileLabel.Text = _profile.Name;
                    this.ProfileLabel.ToolTip = _profile.Name;

                    // Store last used profile in app settings
                    _appConfig.SetStringVal(Constants.ConfigLastUsedProfile, dialog.FileName);

                    // Refresh the recent profiles list
                    UpdateRecentProfiles(dialog.FileName);

                    // Disable the save button
                    _profileChanged = false;
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN007, ex);
            }

            if (e != null)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Can Close command execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// File Close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Edit the profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();

                if (_editorDialog == null)
                {
                    // Disable buttons
                    _newCanExecute = false;
                    _openCanExecute = false;
                    EditScreenRegions.IsEnabled = false;

                    _editorDialog = new EditProfileWindow(this);

                    // Create a copy of the current profile to edit
                    Profile profileCopy = new Profile();
                    profileCopy.FromXml(_profile.ToXml());
                    _editorDialog.CurrentProfile = profileCopy;

                    // Show the dialog
                    _editorDialog.Show();
                }
            }        
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN008, ex);
            }
        }

        /// <summary>
        /// Edit the screen regions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditScreenRegions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();

                EditRegionsWindow regionsDialog = new EditRegionsWindow(this);
                    
                // Create a copy of the current profile to edit
                Profile profileCopy = new Profile();
                profileCopy.FromXml(_profile.ToXml());
                regionsDialog.CurrentProfile = profileCopy;

                // Show the dialog
                if (true == regionsDialog.ShowDialog())
                {
                    profileCopy.Validate();
                    ApplyNewProfile(profileCopy);
                }                
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN009, ex);
            }
        }

        /// <summary>
        /// View - Profile summary 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewProfileSummary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();

                // Create dialog if reqd
                if (_profileSummaryDialog == null)
                {
                    _profileSummaryDialog = new ProfileSummaryWindow(this);

                    // Set the profile to show
                    _profileSummaryDialog.CurrentProfile = _profile;

                    // Show the dialog
                    _profileSummaryDialog.Show();
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN010, ex);
            }
        }

        /// <summary>
        /// Close the editing dialog
        /// </summary>
        private void CloseChildWindows()
        {
            // Close the edit dialog if required
            if (_editorDialog != null && _editorDialog.IsLoaded)
            {
                _editorDialog.Close();
                _editorDialog = null;
            }

            // Close the profile summary dialog if required
            if (_profileSummaryDialog != null && _profileSummaryDialog.IsLoaded)
            {
                _profileSummaryDialog.Close();
                _profileSummaryDialog = null;
            }

            // Close the log information window if required
            if (_logInformationDialog != null && _logInformationDialog.IsLoaded)
            {
                _logInformationDialog.Close();
                _logInformationDialog = null;
            }

            // Close the screen regions overlay window
            if (_overlayDialog != null && _overlayDialog.IsLoaded)
            {
                _overlayDialog.Close();
                _overlayDialog = null;
            }

            // Close the help/about window
            if (_helpAboutWindow != null && _helpAboutWindow.IsLoaded)
            {
                _helpAboutWindow.Close();
                _helpAboutWindow = null;
            }

            // Close custom windows
            CustomWindow[] windowsToClose = new CustomWindow[_openCustomWindows.Values.Count];
            _openCustomWindows.Values.CopyTo(windowsToClose, 0);
            foreach (CustomWindow window in windowsToClose)
            {
                window.Close();
            }

            if (_confirmCommandMessageBox != null)
            {
                _confirmCommandMessageBox.Close();
            }

            if (_commandBlockedMessageBox != null)
            {
                _commandBlockedMessageBox.Close();
            }
        }

        /// <summary>
        /// Handle Choose Actions window closing
        /// </summary>
        public void ChildWindowClosing(Window childWindow)
        {
            if (childWindow == _editorDialog)
            {
                _editorDialog = null;

                // Enable load / edit / save / save as
                _newCanExecute = true;
                _openCanExecute = true;
                EditScreenRegions.IsEnabled = true;
            }
            else if (childWindow == _confirmCommandMessageBox)
            {
                if (!_isExiting)
                {
                    HandleStartProgramConfirmation(_confirmCommandMessageBox);
                }
                _confirmCommandMessageBox = null;
            }
            else if (childWindow == _commandBlockedMessageBox)
            {
                _commandBlockedMessageBox = null;
            }
            else if (childWindow == _profileSummaryDialog)
            {
                _profileSummaryDialog = null;
            }
            else if (childWindow == _logInformationDialog)
            {
                _logInformationDialog = null;
            }
            else if (childWindow == _overlayDialog)
            {
                _overlayDialog = null;
            }
            else if (childWindow == _helpAboutWindow)
            {
                _helpAboutWindow = null;
            }
            else if (childWindow is CustomWindow)
            {
                // Remove window from table of open custom windows
                CustomWindow customWindow = (CustomWindow)childWindow;
                if (_openCustomWindows.ContainsKey(customWindow.CustomWindowSource.ID))
                {
                    _openCustomWindows.Remove(customWindow.CustomWindowSource.ID);
                }
            }
        }

        /// <summary>
        /// Perform a pending Start Program action
        /// </summary>
        /// <param name="action"></param>
        private void HandleStartProgramConfirmation(CustomMessageBox messageBox)
        {
            string thisCommand = "";
            try
            {
                if (_pendingStartProgramAction != null)
                {
                    StartProgramAction action = _pendingStartProgramAction;
                    thisCommand = action.GetCommandLine();

                    // Handle "Don't ask again"
                    if (messageBox.DontAskAgain && messageBox.Result != MessageBoxResult.Cancel)
                    {
                        CommandRuleManager ruleManager = new CommandRuleManager();
                        ruleManager.FromConfig(_appConfig);
                        ECommandAction actionType = messageBox.Result == MessageBoxResult.Yes ? ECommandAction.Run : ECommandAction.DontRun;
                        CommandRuleItem rule = ruleManager.FindRule(thisCommand);
                        if (rule == null)
                        {
                            // Create new rule
                            rule = new CommandRuleItem(thisCommand, actionType);
                            ruleManager.Rules.Add(rule);
                        }
                        else
                        {
                            // Change rule's action type
                            rule.ActionType = actionType;
                        }
                        ruleManager.ToConfig(_appConfig);
                    }

                    if (messageBox.Result == MessageBoxResult.Yes)
                    {
                        // Start the program
                        if (!action.CheckIfRunning || !ProcessManager.IsRunning(action.ProgramName))
                        {
                            ProcessManager.Start(action);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN021 + " " + thisCommand, ex);
            }
        }

        /// <summary>
        /// Load a config profile from a file
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadProfile(string filePath)
        {
            if (filePath == null)
            {
                // Load from most recent location if required
                bool autoLoadLastProfile = _appConfig.GetBoolVal(Constants.ConfigAutoLoadLastProfile, Constants.DefaultAutoLoadLastProfile);
                if (autoLoadLastProfile)
                {
                    filePath = _appConfig.GetStringVal(Constants.ConfigLastUsedProfile, null);
                }
            }

            // Try to load the profile
            bool profileUpgraded = false;
            Profile profile = null;
            if (filePath != null && File.Exists(filePath))
            {
                try
                {
                    // Load from file
                    profile = new Profile();
                    profileUpgraded = profile.FromFile(filePath);

                    // Set profile name
                    FileInfo fi = new FileInfo(filePath);
                    profile.Name = fi.Name.Replace(Constants.ProfileFileExtension, "");
                    //profile.Directory = fi.DirectoryName;

                    // Update last used profile
                    _appConfig.SetStringVal(Constants.ConfigLastUsedProfile, filePath);

                    // Update the recent profiles list
                    UpdateRecentProfiles(filePath);
                }
                catch (Exception ex)
                {
                    ShowError(Properties.Resources.E_MAIN011, ex);
                    profile = null;

                    // Remove invalid profile from app config
                    _appConfig.SetStringVal(Constants.ConfigLastUsedProfile, null);
                }
            }

            // If no profile was loaded, create a blank profile
            if (profile == null)
            {
                profile = CreateDefaultProfile();

                // Update the recent profiles list
                UpdateRecentProfiles(null);
            }

            // Set the window title
            this.ProfileLabel.Text = profile.Name;
            this.ProfileLabel.ToolTip = profile.Name;

            // Reset the logical state
            _logicalState = new LogicalState();

            // Apply the profile
            ApplyNewProfile(profile);
            _profileChanged = false;    // Set to value of 'profileUpgraded' if you wish to prompt the user to save after upgrade

            // Auto open windows if required
            bool autoOpen = _appConfig.GetBoolVal(Constants.ConfigAutoOpenCustomWindows, Constants.DefaultAutoOpenCustomWindows);
            if (autoOpen)
            {
                DoOpenAllCustomWindows();
            }
        }

        /// <summary>
        /// Return the current profile
        /// </summary>
        /// <returns></returns>
        public Profile GetCurrentProfile()
        {
            return _profile;
        }

        /// <summary>
        /// Get the application configuration
        /// </summary>
        /// <returns></returns>
        public AppConfig GetAppConfig()
        {
            return _appConfig;
        }

        /// <summary>
        /// Apply a new profile, either after loading from file, or after editing
        /// </summary>
        /// <param name="profile"></param>
        public void ApplyNewProfile(Profile profile)
        {
            // Store the profile
            _profile = profile;
            _profileChanged = true;

            // Report the change
            if (EventReported != null)
            {
                AltStringValEventArgs args = new AltStringValEventArgs(profile.Name);
                EventReport report = new EventReport(DateTime.Now, EEventType.ProfileChange, args);
                EventReported(report);
            }

            // Reset logical state
            _logicalState = new LogicalState();

            // Configure state manager
            Profile profileCopy = new Profile();
            profileCopy.FromXml(profile.ToXml());
            _manager.SetProfile(profileCopy);

            // Refresh the GUI
            UpdateGUI_Situation(_logicalState);

            // Refresh the profile summary window
            if (_profileSummaryDialog != null)
            {
                _profileSummaryDialog.CurrentProfile = profile;
            }

            // Refresh the overlay window
            if (_overlayDialog != null)
            {
                _overlayDialog.CurrentProfile = profile;
            }

            // Refresh the custom windows
            UpdateCustomWindows();
        }

        /// <summary>
        /// Start monitoring input sources
        /// </summary>
        private void StartMonitoringSources()
        {
            _manager.StartPolling();
        }

        /// <summary>
        /// Stop monitoring input sources
        /// </summary>
        private void StopMonitoringSources()
        {
            _manager.StopPolling();
            Thread.Sleep(_waitForExitMS);
        }

        /// <summary>
        /// Start updating the GUI
        /// </summary>
        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(_uiUpdateIntervalMS);
            _timer.Tick += new EventHandler(UpdateGUI);
            _timer.Start();
        }

        /// <summary>
        /// Stop updating the GUI
        /// </summary>
        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        /// <summary>
        /// Perform a GUI update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void UpdateGUI(Object sender, EventArgs args)
        {
            try
            {
                List<EventReport> eventsToHandle = _manager.GetNewEventReports();
                if (EventReported != null)
                {
                    foreach (EventReport report in eventsToHandle)
                    {
                        EventReported(report);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Handle events which relate to the main window
        /// </summary>
        /// <param name="ev"></param>
        private void HandleEventReport(EventReport report)
        {
            if (report.EventType == EEventType.StateChange)
            {
                AltStateChangeEventArgs args = (AltStateChangeEventArgs)report.Args;
                _logicalState = args.LogicalState;
                UpdateGUI_Situation(_logicalState);
            }
            else if (report.EventType == EEventType.LoadProfile)
            {
                AltStringValEventArgs args = (AltStringValEventArgs)report.Args;
                HandleLoadProfileEvent(args);
            }
            else if (report.EventType == EEventType.StartProgram)
            {
                StartProgramEventArgs args = (StartProgramEventArgs)report.Args;
                HandleStartProgramEvent(args);
            }
            else if (report.EventType == EEventType.MenuOptionEvent)
            {
                MenuOptionEventArgs args = (MenuOptionEventArgs)report.Args;
                ToggleMenuOption(args.Option);
            }
            else if (report.EventType == EEventType.ShowOrHideCustomWindow)
            {
                CustomWindowEventArgs args = (CustomWindowEventArgs)report.Args;
                if (args.WindowID < 0)
                {
                    DoOpenAllCustomWindows(args.WindowState);
                }
                else
                {
                    DoOpenCustomWindow(args.WindowID, args.WindowState);
                }
            }
        }

        /// <summary>
        /// Update the UI following a change of situation
        /// </summary>
        private void UpdateGUI_Situation(LogicalState logicalState)
        {
            // State labels
            NamedItem modeDetails = _profile.GetModeDetails(logicalState.ModeID);
            if (modeDetails != null)
            {
                ModeName.Text = modeDetails.Name;
            }
            else
            {
                ModeName.Text = "N/K";
            }
            AppItem appDetails = _profile.GetAppDetails(logicalState.AppID);
            if (appDetails != null)
            {
                AppName.Text = appDetails.LongName;
            }
            else
            {
                AppName.Text = "N/K";
            }
            NamedItem pageDetails = _profile.GetPageDetails(logicalState.PageID);
            if (pageDetails != null)
            {
                PageName.Text = pageDetails.Name;
            }
            else
            {
                PageName.Text = "N/K";
            }
        }

        /// <summary>
        /// Handle a request to load a profile
        /// Null profile name means load last used, empty profile name means load a blank profile
        /// </summary>
        /// <param name="args"></param>
        private void HandleLoadProfileEvent(AltStringValEventArgs args)
        {
            try
            {
                // Disallow when the editor is open or current profile is unsaved
                if (_editorDialog != null)
                {
                    ShowError(Properties.Resources.E_MAIN019, new Exception(Properties.Resources.E_MAIN019));
                }
                else if (_profileChanged)
                {
                    ShowError(Properties.Resources.E_MAIN020, new Exception(Properties.Resources.E_MAIN020));
                }
                else
                {
                    // Get the file name of the profile to load
                    string filePath = args.Val;
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        // Add extension if required
                        if (!filePath.EndsWith(Constants.ProfileFileExtension))
                        {
                            filePath += Constants.ProfileFileExtension;
                        }

                        // Get the profiles directory
                        string defaultProfilesDir = Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                        string profilesDir = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);

                        // Prepend the directory
                        filePath = Path.Combine(profilesDir, filePath);
                    }

                    LoadProfile(filePath);
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN018, ex);
            }
        }

        /// <summary>
        /// Handle a request to start a program
        /// </summary>
        /// <param name="args"></param>
        private void HandleStartProgramEvent(StartProgramEventArgs args)
        {
            string thisCommand = "";
            try
            {
                StartProgramAction action = args.Action;

                // Check if the program is already running if required, and not already scheduled to occur
                if (_confirmCommandMessageBox == null && _commandBlockedMessageBox == null)
                {
                    // Get the command to execute
                    thisCommand = action.GetCommandLine();

                    // Check whether the program is allowed
                    CommandRuleManager ruleManager = new CommandRuleManager();
                    ruleManager.FromConfig(_appConfig);
                    CommandRuleItem matchingRule = ruleManager.ApplyRules(thisCommand);
                    ECommandAction actionType = (matchingRule != null) ? matchingRule.ActionType : ECommandAction.AskMe;

                    bool canStart = false;
                    if (actionType != ECommandAction.DontRun &&
                        (!action.CheckIfRunning || !ProcessManager.IsRunning(action.ProgramName)))
                    {
                        canStart = true;
                    }

                    switch (actionType)
                    {
                        case ECommandAction.Run:
                            if (canStart)
                            {
                                // Start the program now
                                ProcessManager.Start(action);
                            }
                            break;
                        case ECommandAction.DontRun:
                            {
                                // Add a disallow rule to the config if not already present, so that the user can easily change it
                                if (matchingRule != null && !matchingRule.Command.Equals(thisCommand))
                                {
                                    ruleManager.Rules.Add(new CommandRuleItem(thisCommand, ECommandAction.DontRun));
                                    ruleManager.ToConfig(_appConfig);
                                }

                                string message = string.Format(Properties.Resources.Info_CommandDisallowedMessage,
                                                            Constants.ApplicationName, Environment.NewLine, thisCommand);
                                _commandBlockedMessageBox = new CustomMessageBox(this, message, Properties.Resources.Info_CommandDisallowed, MessageBoxButton.OK, true, false);
                                _commandBlockedMessageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                                _commandBlockedMessageBox.IsModal = false;
                                _commandBlockedMessageBox.Show();
                                break;
                            }
                        case ECommandAction.AskMe:
                        default:
                            {
                                if (canStart)
                                {
                                    // Ask the user for confirmation, without blocking this thread
                                    _pendingStartProgramAction = action;
                                    string message = string.Format(Properties.Resources.Q_RunCommandMessage,
                                                                    Constants.ApplicationName, Environment.NewLine, thisCommand);
                                    _confirmCommandMessageBox = new CustomMessageBox(this, message, Properties.Resources.Q_RunCommand, MessageBoxButton.YesNoCancel, true, true);
                                    _confirmCommandMessageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                                    _confirmCommandMessageBox.IsModal = false;
                                    _confirmCommandMessageBox.Show();
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN017, ex);
            }
        }

        /// <summary>
        /// Submit an event raised from a dialog (e.g. custom window) to the state manager thread
        /// </summary>
        /// <param name="args"></param>
        public void SubmitEvent(AltControlEventArgs args)
        {
            _manager.SubmitEvent(args);
        }

        /// <summary>
        /// Create or refresh the custom windows
        /// </summary>
        private void UpdateCustomWindows()
        {
            // Close any open custom windows that have been deleted and update any ones
            List<CustomWindow> windowsToClose = new List<CustomWindow>();
            Dictionary<long, CustomWindow>.Enumerator eCustomWindow = _openCustomWindows.GetEnumerator();
            while (eCustomWindow.MoveNext())
            {
                BaseSource source = _profile.GetInputSource(eCustomWindow.Current.Key);
                if (source == null || source.SourceType != ESourceType.CustomWindow)
                {
                    // Prepare to close custom window
                    windowsToClose.Add(eCustomWindow.Current.Value);
                }
                else
                {
                    // Update open custom window
                    CustomWindowSource customWindowSource = (CustomWindowSource)source;
                    eCustomWindow.Current.Value.CustomWindowSource = customWindowSource;
                }
            }

            // Close windows that have been deleted
            foreach (CustomWindow window in windowsToClose)
            {
                window.Close();
            }
            windowsToClose.Clear();

            // Recreate window menu items
            while (WindowMenu.Items.Count > 0 &&
                WindowMenu.Items[0] is MenuItem)
            {
                WindowMenu.Items.RemoveAt(0);
            }
            
            int windowIndex = 0;
            foreach (BaseSource source in _profile.InputSources)
            {
                if (source.SourceType == ESourceType.CustomWindow)
                {
                    CustomWindowSource customWindowSource = (CustomWindowSource)source;

                    // Add window menu item for this window
                    MenuItem menuItem = new MenuItem();
                    menuItem.Click += new RoutedEventHandler(CustomWindowMenuItem_Click);
                    menuItem.Header = Properties.Resources.String_Open + " " + customWindowSource.WindowTitle + "...";
                    menuItem.Tag = source.ID;
                    if (windowIndex < 4)
                    {
                        // Can open upto 4 custom windows using Ctrl+1...4
                        menuItem.InputGestureText = string.Format("Ctrl+{0}", windowIndex + 1);
                    }
                    WindowMenu.Items.Insert(windowIndex, menuItem);

                    windowIndex++;
                }
            }

            if (windowIndex != 0)
            {
                // Add 'Open all' option
                MenuItem menuItem = new MenuItem();
                menuItem.Command = OpenAllCustomWindows;
                menuItem.Header = Properties.Resources.String_Open_all + "...";
                menuItem.InputGestureText = "Ctrl+0";
                WindowMenu.Items.Insert(0, menuItem);

                // Show menu
                WindowMenu.Visibility = Visibility.Visible;
            }
            else
            {
                WindowMenu.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Open all custom windows command executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenAllCustomWindows_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ClearMessages();
                DoOpenAllCustomWindows();
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN012, ex);
            }
        }

        /// <summary>
        /// Open all custom windows
        /// </summary>
        private void DoOpenAllCustomWindows(EWindowState windowState = EWindowState.Normal)
        {
            foreach (BaseSource source in _profile.InputSources)
            {
                if (source.SourceType == ESourceType.CustomWindow)
                {
                    DoOpenCustomWindow(source.ID, windowState);
                }
            }
        }

        /// <summary>
        /// Handle open custom window command (Ctrl+1...4)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCustomWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ClearMessages();

                int windowIndex = -1;
                if (e.Command == OpenCustomWindow1)
                {
                    windowIndex = 0;
                }
                else if (e.Command == OpenCustomWindow2)
                {
                    windowIndex = 1;
                }
                else if (e.Command == OpenCustomWindow3)
                {
                    windowIndex = 2;
                }
                else if (e.Command == OpenCustomWindow4)
                {
                    windowIndex = 3;
                }
                
                int index = 0;
                foreach (BaseSource source in _profile.InputSources)
                {
                    if (source.SourceType == ESourceType.CustomWindow)
                    {
                        if (index == windowIndex)
                        {
                            DoOpenCustomWindow(source.ID);
                            break;
                        }
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN013, ex);
            }
            
            e.Handled = true;
        }

        /// <summary>
        /// Open a custom window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();

                MenuItem menuItem = (MenuItem)sender;
                long windowID = (long)menuItem.Tag;
                DoOpenCustomWindow(windowID);
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN014, ex);
            }
        }

        /// <summary>
        /// Open the specified custom window
        /// </summary>
        /// <param name="windowID"></param>
        private void DoOpenCustomWindow(long windowID, EWindowState windowState = EWindowState.Normal)
        {
            CustomWindowSource customWindowSource = null;
            int windowIndex = 0;
            foreach (BaseSource source in _profile.InputSources)
            {
                if (source.SourceType == ESourceType.CustomWindow)
                {
                    if (source.ID == windowID)
                    {
                        customWindowSource = (CustomWindowSource)source;
                        break;
                    }
                    windowIndex++;
                }
            }
        
            if (customWindowSource != null)
            {
                if (_openCustomWindows.ContainsKey(customWindowSource.ID))
                {
                    // Window already created
                    CustomWindow customWindow = _openCustomWindows[customWindowSource.ID];
                    if (customWindow.IsLoaded)
                    {
                        switch (windowState)
                        {
                            case EWindowState.Normal:
                                customWindow.WindowState = WindowState.Normal; break;
                            case EWindowState.Minimise:
                                customWindow.WindowState = WindowState.Minimized; break;
                            default:
                                if (customWindow.WindowState == WindowState.Normal)
                                {
                                    customWindow.WindowState = WindowState.Minimized;
                                }
                                else 
                                {
                                    customWindow.WindowState = WindowState.Normal;
                                }
                                break;
                        }
                    }
                }
                else
                {
                    // Custom window not open
                    CustomWindow customWindow = new CustomWindow(this);
                    customWindow.ShowTitleBar = WindowShowTitleBars.IsChecked;
                    customWindow.WindowIndex = windowIndex;
                    customWindow.CustomWindowSource = customWindowSource;
                    customWindow.Show();

                    if (windowState == EWindowState.Minimise)
                    {
                        customWindow.WindowState = WindowState.Minimized;
                    }

                    // Record that window is open
                    _openCustomWindows[customWindowSource.ID] = customWindow;
                }
            }
        }

        private void HelpUserGuide_Click(object sender, RoutedEventArgs e)
        {
            ClearMessages();

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Constants.UserGuideURL));
            }
            catch (Exception)
            {
            }
            e.Handled = true;
        }

        /// <summary>
        /// Show Help - About
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpAbout_Click(object sender, RoutedEventArgs e)
        {
            ClearMessages();

            if (_helpAboutWindow == null)
            {
                _helpAboutWindow = new HelpAboutWindow(this);
                _helpAboutWindow.Show();
            }
            else
            {
                _helpAboutWindow.WindowState = WindowState.Normal;
            }
        }

        /// <summary>
        /// Show notes window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewProfileNotes_Click(object sender, RoutedEventArgs e)
        {
            ClearMessages();

            ProfileNotesWindow notesWindow = new ProfileNotesWindow();
            notesWindow.ProfileNotes = _profile.ProfileNotes;
            if (notesWindow.ShowDialog() == true)
            {
                // User clicked OK - save their notes if required
                if (notesWindow.ProfileNotes != _profile.ProfileNotes)
                {
                    _profile.ProfileNotes = notesWindow.ProfileNotes;
                    _profileChanged = true;
                }
            }
        }

        /// <summary>
        /// View menu - overlay option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewOverlayOption_Changed(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            // Create overlays window if required
            if (_overlayDialog == null && 
                (ViewDrawScreenRegions.IsChecked || 
                ViewDrawPointerIndicator.IsChecked ||
                ViewDrawStateOverlay.IsChecked))
            {
                _overlayDialog = new OverlayWindow(this);
                _overlayDialog.SetAppConfig(_appConfig);
                _overlayDialog.CurrentProfile = _profile;
                _overlayDialog.Show();
            }

            if (_overlayDialog != null)
            {
                _overlayDialog.DrawScreenRegions = this.ViewDrawScreenRegions.IsChecked;
                _overlayDialog.ShowScreenRegionNames = this.ViewShowScreenRegionNames.IsChecked;
                _overlayDialog.DrawPointerIndicator = this.ViewDrawPointerIndicator.IsChecked;
                _overlayDialog.DrawStateOverlay = this.ViewDrawStateOverlay.IsChecked;
            }
        }

        /// <summary>
        /// Window menu - show title bars option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowShowTitleBars_Changed(object sender, RoutedEventArgs e)
        {
            bool show = this.WindowShowTitleBars.IsChecked;
            foreach (CustomWindow window in _openCustomWindows.Values)
            {
                window.ShowTitleBar = show;
            }
        }

        /// <summary>
        /// Tools - options menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolsOptions_Click(object sender, RoutedEventArgs e)
        {
            ClearMessages();
            
            // Create a copy of the app config
            AppConfig configCopy = new AppConfig();
            configCopy.FromXml(_appConfig.ToXml());

            AppConfigWindow appConfigWindow = new AppConfigWindow();
            appConfigWindow.SetAppConfig(configCopy);
            if (appConfigWindow.ShowDialog() == true)
            {
                // App config changed
                _appConfig = appConfigWindow.GetAppConfig();
                _uiUpdateIntervalMS = _appConfig.GetIntVal(Constants.ConfigUIUpdateIntervalMS, Constants.DefaultUIUpdateIntervalMS);
                _timer.Interval = TimeSpan.FromMilliseconds(_uiUpdateIntervalMS);

                // Reapply app config
                ConfigureStateManager();

                // Update hotkeys if required
                if (appConfigWindow.HotkeysChanged)
                {
                    RegisterHotkeys();
                }

                // Update editor window
                if (_editorDialog != null)
                {
                    _editorDialog.SetAppConfig(_appConfig);
                }

                // Update overlays window
                if (_overlayDialog != null)
                {
                    _overlayDialog.SetAppConfig(_appConfig);
                }
            }
        }

        /// <summary>
        /// Set UI language
        /// </summary>
        private void SetUILanguage()
        {            
            string languageCode = _appConfig.GetStringVal(Constants.ConfigLanguageCode, "");
            if (AppConfig.SupportedLanguages.ContainsKey(languageCode))
            {
                CultureInfo culture = new CultureInfo(languageCode);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            else
            {
                languageCode = Thread.CurrentThread.CurrentUICulture.Name;
                if (!AppConfig.SupportedLanguages.ContainsKey(languageCode))
                {
                    languageCode = Constants.DefaultLanguageCode;
                }
                _appConfig.SetStringVal(Constants.ConfigLanguageCode, languageCode);
            }
        }

        /// <summary>
        /// Show Log information window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpEventLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMessages();

                // Create dialog if reqd
                if (_logInformationDialog == null)
                {
                    _logInformationDialog = new DiagnosticsWindow(this);
                
                    // Show the dialog
                    _logInformationDialog.Show();
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.E_MAIN015, ex);
            }
        }

        /// <summary>
        /// Toggle a main menu option
        /// </summary>
        /// <param name="option"></param>
        private void ToggleMenuOption(EMainMenuOption option)
        {
            switch (option)
            {
                case EMainMenuOption.DrawScreenRegions:
                    ViewDrawScreenRegions.IsChecked = !ViewDrawScreenRegions.IsChecked;
                    break;
                case EMainMenuOption.ShowScreenRegionNames:
                    ViewShowScreenRegionNames.IsChecked = !ViewShowScreenRegionNames.IsChecked;
                    break;
                case EMainMenuOption.DrawPointerIndicatorLine:
                    ViewDrawPointerIndicator.IsChecked = !ViewDrawPointerIndicator.IsChecked;
                    break;
                case EMainMenuOption.DrawStateOverlay:
                    ViewDrawStateOverlay.IsChecked = !ViewDrawStateOverlay.IsChecked;
                    break;
                case EMainMenuOption.ShowTitleBars:
                    WindowShowTitleBars.IsChecked = !WindowShowTitleBars.IsChecked;
                    break;
            }
        }

        /// <summary>
        /// Show error message link
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        private void ShowError(string message, Exception ex)
        {
            AboutPanel.Visibility = Visibility.Collapsed;
            InfoMessage.Visibility = Visibility.Collapsed;
            ErrorMessage.Show(message, ex);
        }

        /// <summary>
        /// Clear error message
        /// </summary>
        private void ClearMessages()
        {
            InfoMessage.Visibility = Visibility.Collapsed;
            ErrorMessage.Clear();
            AboutPanel.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
