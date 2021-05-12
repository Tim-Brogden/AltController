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
using AltController.Event;
using AltController.Config;
using AltController.Input;
using AltController.Actions;

namespace AltController
{
    /// <summary>
    /// Profile editor window
    /// </summary>
    public partial class EditProfileWindow : Window
    {
        private bool _isLoaded;
        private IParentWindow _parentWindow;
        private Profile _profile = null;
        private FrameworkElement _visibleInputControl = null;
        private AltControlEventArgs _selectedControl = null;
        private ActionList _currentActionList;
        private int _numActionLists = 0;
        private bool _profileEdited = false;
        private Dictionary<EEventReason, RadioButton> _eventReasonButtons = new Dictionary<EEventReason, RadioButton>();
        private NamedItemList _actionListItems = new NamedItemList();

        // Commands
        public static RoutedCommand PreviousCommand = new RoutedCommand();
        public static RoutedCommand NextCommand = new RoutedCommand();

        /// <summary>
        /// Set the profile to edit
        /// </summary>
        public Profile CurrentProfile
        {
            set
            {
                _profile = value;
                if (_isLoaded)
                {
                    InitialiseDisplay();
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public EditProfileWindow(IParentWindow parent)
        {
            _parentWindow = parent;

            InitializeComponent();

            SelectMouseInput.SetAppConfig(_parentWindow.GetAppConfig());

            // Connect input control event handlers
            SelectMouseInput.SelectionChanged += new AltControlEventHandler(Control_SelectionChanged);
            SelectKeyboardInput.SelectionChanged += new AltControlEventHandler(Control_SelectionChanged);
            SelectCustomWindowInput.SelectionChanged += new AltControlEventHandler(Control_SelectionChanged);

            // Actions list
            ActionsList.AddClicked += new RoutedEventHandler(AddActionButton_Click);
            ActionsList.EditClicked += new RoutedEventHandler(EditActionButton_Click);
            ActionsList.DeleteClicked += new RoutedEventHandler(DeleteActionButton_Click);
            ActionsList.MoveUpClicked += new RoutedEventHandler(MoveUpButton_Click);
            ActionsList.MoveDownClicked += new RoutedEventHandler(MoveDownButton_Click);
        }

        /// <summary>
        /// Set the application configuration
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            SelectMouseInput.SetAppConfig(appConfig);
        }

        /// <summary>
        /// Record whether the profile has been changed
        /// </summary>
        /// <param name="profileEdited"></param>
        /// <returns></returns>
        private void SetProfileEdited(bool profileEdited)
        {
            _profileEdited = profileEdited;
            if (_isLoaded)
            {
                this.ApplyButton.IsEnabled = _profileEdited;
            }
        }

        /// <summary>
        /// Set the currently displayed logical state
        /// </summary>
        /// <param name="eventToConfigure"></param>
        /// <param name="actions"></param>
        public void ShowLogicalState(LogicalState logicalState)
        {
            // Update the data displayed if the form is loaded
            if (_isLoaded && logicalState != null)
            {
                // Select situation
                this.ModeCombo.SelectedValue = logicalState.ModeID;
                this.AppCombo.SelectedValue = logicalState.AppID;
                this.PageCombo.SelectedValue = logicalState.PageID;
            }
        }

        /// <summary>
        /// Handle window loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isLoaded = true;
                ErrorMessage.Clear();
                InitialiseDisplay();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF001, ex);
            }
        }

        /// <summary>
        /// Tell the parent form that the window is closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_parentWindow != null)
            {
                try
                {
                    _parentWindow.ChildWindowClosing(this);
                }
                catch (Exception)
                {
                }
            }
        }

        // Initialise the UI for the current profile
        private void InitialiseDisplay()
        {
            if (_profile == null)
            {
                return;
            }

            // No changes made yet
            SetProfileEdited(false);

            // Create a dictionary of event reason radio buttons
            _eventReasonButtons[EEventReason.Pressed] = this.ReasonPressed;
            _eventReasonButtons[EEventReason.Released] = this.ReasonReleased;
            //_eventReasonButtons[EEventReason.Changed] = this.ReasonChanged;
            _eventReasonButtons[EEventReason.Updated] = this.ReasonUpdated;
            _eventReasonButtons[EEventReason.Dwelled] = this.ReasonDwelled;
            _eventReasonButtons[EEventReason.Inside] = this.ReasonInside;
            _eventReasonButtons[EEventReason.Outside] = this.ReasonOutside;

            // Bind situations
            this.ModeCombo.ItemsSource = _profile.ModeDetails;
            this.AppCombo.ItemsSource = _profile.AppDetails;
            this.PageCombo.ItemsSource = _profile.PageDetails;

            // Bind inputs
            this.InputCombo.ItemsSource = _profile.InputSources;

            // Bind actions list
            this.ActionsList.NamedItemList = _actionListItems;

            if (_profile.GetNumActionLists() != 0)
            {
                // Select the first action list
                NavigateToActionList(1);
            }
            else
            {
                // Select some drop down values if possible
                this.ModeCombo.SelectedValue = Constants.DefaultID;
                this.AppCombo.SelectedValue = Constants.DefaultID;
                this.PageCombo.SelectedValue = Constants.DefaultID;
                if (InputCombo.Items.Count != 0)
                {
                    InputCombo.SelectedIndex = 0;
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
            try
            {
                ApplyAnyChanges();
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF002, ex);
            }
        }

        /// <summary>
        /// Apply button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();
                ApplyAnyChanges();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF003, ex);
            }
        }

        /// <summary>
        /// Apply any changes to the profile being edited
        /// </summary>
        private void ApplyAnyChanges()
        {
            if (_profileEdited)
            {
                // Apply changes
                Profile profile = new Profile();
                profile.FromXml(_profile.ToXml());
                _parentWindow.ApplyNewProfile(profile);

                // Record that changes have been applied
                SetProfileEdited(false);
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
        /// Add action button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                if (_currentActionList != null)
                {
                    // Create action editor
                    EditActionWindow actionEditor = new EditActionWindow(null, _currentActionList.EventArgs, _profile);

                    // Show editor
                    if (actionEditor.ShowDialog() == true)
                    {
                        // Add new action
                        BaseAction action = actionEditor.GetAction();

                        // Add action to profile
                        _currentActionList.Add(action);

                        // Update display
                        // When first action is added, trigger a refresh to update the highlighting
                        // otherwise, just select the new action
                        _actionListItems.Add(new NamedItem(_actionListItems.Count, action.Name));
                        if (_currentActionList.Count == 1)
                        {
                            _profile.RenumberActionLists();
                            RefreshActionsList();
                        }
                        else
                        {
                            this.ActionsList.SelectedIndex = _actionListItems.Count - 1;
                        }

                        // Record change
                        SetProfileEdited(true);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF004, ex);
            }
        }

        /// <summary>
        /// Edit action button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                int actionIndex;
                BaseAction action;
                GetSelectedAction(out action, out actionIndex);

                if (action != null)
                {
                    // Create action editor
                    EditActionWindow actionEditor = new EditActionWindow(action, _currentActionList.EventArgs, _profile);

                    // Show editor
                    if (actionEditor.ShowDialog() == true)
                    {
                        // Update action
                        action = actionEditor.GetAction();
                        _currentActionList[actionIndex] = action;

                        // Update display
                        _actionListItems[actionIndex] = new NamedItem(actionIndex, action.Name);

                        // Set selection
                        this.ActionsList.SelectedIndex = actionIndex;

                        // Record change
                        SetProfileEdited(true);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF005, ex);
            }
        }

        /// <summary>
        /// Delete action button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                BaseAction currentAction;
                int actionIndex;
                GetSelectedAction(out currentAction, out actionIndex);
                if (currentAction != null)
                {
                    // Remove action from profile
                    _currentActionList.RemoveAt(actionIndex);

                    if (_currentActionList.Count == 0)
                    {
                        // All actions deleted, so trigger a refresh to update the highlighting
                        _profile.RenumberActionLists();
                        RefreshActionsList();
                    }

                    // Record change
                    SetProfileEdited(true);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF006, ex);
            }
        }

        /// <summary>
        /// Move an action up in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                BaseAction currentAction;
                int actionIndex;
                GetSelectedAction(out currentAction, out actionIndex);
                if (currentAction != null && actionIndex > 0)
                {
                    // Move action up the order
                    _currentActionList.RemoveAt(actionIndex);
                    _currentActionList.Insert(actionIndex - 1, currentAction);

                    // Record change
                    SetProfileEdited(true);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF007, ex);
            }
        }

        /// <summary>
        /// Move an action down in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                BaseAction currentAction;
                int actionIndex;
                GetSelectedAction(out currentAction, out actionIndex);
                if (currentAction != null && actionIndex < _currentActionList.Count - 1)
                {
                    // Move action up the order
                    _currentActionList.RemoveAt(actionIndex);
                    _currentActionList.Insert(actionIndex + 1, currentAction);

                    // Record change
                    SetProfileEdited(true);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF008, ex);
            }
        }

        /// <summary>
        /// Handle edit situations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditSituations_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                // Create dialog for editing modes, apps and pages
                EditSituationsWindow dialog = new EditSituationsWindow();
                dialog.ModeDetailsList = new NamedItemList(_profile.ModeDetails);
                dialog.AppDetailsList = new NamedItemList(_profile.AppDetails);
                dialog.PageDetailsList = new NamedItemList(_profile.PageDetails);

                // Show dialog
                if (dialog.ShowDialog() == true)
                {
                    // Update profile
                    _profile.ModeDetails = dialog.ModeDetailsList;
                    _profile.AppDetails = dialog.AppDetailsList;
                    _profile.PageDetails = dialog.PageDetailsList;
                    _profile.Validate();

                    // Indicate profile changed
                    SetProfileEdited(true);

                    // Determine the mode, app and page to select after the drop downs have been repopulated
                    long modeID = Constants.DefaultID;
                    if (this.ModeCombo.SelectedItem != null)
                    {
                        modeID = ((NamedItem)this.ModeCombo.SelectedItem).ID;
                        if (_profile.GetModeDetails(modeID) == null)
                        {
                            // Mode has been deleted so will select default
                            modeID = Constants.DefaultID;
                        }
                    }
                    long appID = Constants.DefaultID;
                    if (this.AppCombo.SelectedItem != null)
                    {
                        appID = ((NamedItem)this.AppCombo.SelectedItem).ID;
                        if (_profile.GetAppDetails(appID) == null)
                        {
                            // App has been deleted so will select default
                            appID = Constants.DefaultID;
                        }
                    }
                    long pageID = Constants.DefaultID;
                    if (this.PageCombo.SelectedItem != null)
                    {
                        pageID = ((NamedItem)this.PageCombo.SelectedItem).ID;
                        if (_profile.GetPageDetails(pageID) == null)
                        {
                            // Page has been deleted so will select default
                            pageID = Constants.DefaultID;
                        }
                    }

                    // Rebind situations
                    this.ModeCombo.ItemsSource = _profile.ModeDetails;
                    this.AppCombo.ItemsSource = _profile.AppDetails;
                    this.PageCombo.ItemsSource = _profile.PageDetails;

                    // Select appropriate values
                    this.ModeCombo.SelectedValue = modeID;
                    this.AppCombo.SelectedValue = appID;
                    this.PageCombo.SelectedValue = pageID;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF009, ex);
            }
        }

        /// <summary>
        /// Handle edit inputs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditInputs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                // Create a copy of the current profile to edit
                Profile profileCopy = new Profile();
                profileCopy.FromXml(_profile.ToXml());                

                // Show dialog
                EditSourcesWindow dialog = new EditSourcesWindow(profileCopy.InputSources);
                if (dialog.ShowDialog() == true)
                {
                    // Update profile's input sources
                    _profile.InputSources = dialog.InputSources;
                    _profile.Validate();

                    // Indicate profile changed
                    SetProfileEdited(true);
                    
                    // Rebind inputs combo
                    this.InputCombo.ItemsSource = _profile.InputSources;

                    // Select last source if possible (most often, the user will have added a new source)
                    if (_profile.InputSources.Count > 0)
                    {
                        this.InputCombo.SelectedIndex = _profile.InputSources.Count - 1;
                    }
                    else
                    {
                        // No inputs - hide current viewer control
                        ShowInputControl(null);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF010, ex);
            }
        }

        /// <summary>
        /// Design custom window clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DesignButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NamedItem selectedInput = (NamedItem)InputCombo.SelectedItem;
                if (selectedInput != null)
                {
                    CustomWindowSource source = _profile.GetInputSource(selectedInput.ID) as CustomWindowSource;
                    if (source != null)
                    {
                        // Copy current custom window
                        CustomWindowSource copiedSource = new CustomWindowSource(source);

                        // Show window designer
                        CustomWindowDesigner windowDesigner = new CustomWindowDesigner(_parentWindow, copiedSource);
                        if (true == windowDesigner.ShowDialog())
                        {
                            // Update custom window in profile
                            for (int i = 0; i < _profile.InputSources.Count; i++)
                            {
                                if (_profile.InputSources[i] == source)
                                {
                                    _profile.UpdateInput(i, windowDesigner.CustomWindow);
                                    _profile.Validate();
                                    SetProfileEdited(true);
                                    break;
                                }
                            }

                            // Update UI by reselecting input
                            InputCombo.SelectedValue = source.ID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF011, ex);
            }
        }

        /// <summary>
        /// Edit screen regions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditRegionsButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Create dialog for editing screen regions
                EditRegionsWindow dialog = new EditRegionsWindow(_parentWindow);

                // Create a copy of the current profile to edit
                Profile profileCopy = new Profile();
                profileCopy.FromXml(_profile.ToXml());
                dialog.CurrentProfile = profileCopy;

                // Show dialog
                if (dialog.ShowDialog() == true)
                {
                    // Update profile 
                    _profile.ScreenRegions = profileCopy.ScreenRegions;
                    _profile.Validate();

                    // Indicate profile changed
                    SetProfileEdited(true);

                    // Refresh the mouse control viewer if it is currently visible
                    if (_selectedControl != null &&
                        (_selectedControl.ControlType == EControlType.MousePointer || _selectedControl.ControlType == EControlType.MouseButtons))
                    {
                        // Refresh viewer control
                        MouseSource source = _profile.GetInputSource(ESourceType.Mouse) as MouseSource;
                        if (source != null)
                        {
                            SelectMouseInput.SetSource(source);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF012, ex);
            }
        }
        
        /// <summary>
        /// Handle selection of mode, app or page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogicalStateCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();
                RefreshActionsList();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF013, ex);
            }
        }

        /// <summary>
        /// Handle selection of input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                bool enableDesign = false;
                IInputViewer viewerControl = null; 
                NamedItem selectedInput = (NamedItem)InputCombo.SelectedItem;
                if (selectedInput != null)
                {
                    viewerControl = GetSourceViewControl(selectedInput.ID);
                }

                if (viewerControl != null)
                {
                    BaseSource source = _profile.GetInputSource(selectedInput.ID);
                    enableDesign = (source.SourceType == ESourceType.CustomWindow);

                    viewerControl.SetSource(source);
                    ShowInputControl((FrameworkElement)viewerControl);
                    _selectedControl = viewerControl.GetSelectedControl();
                }
                else
                {
                    _selectedControl = null;
                    ShowInputControl(null);
                }

                RefreshEventReasons();
                RefreshActionsList();
                DesignButton.IsEnabled = enableDesign;
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF014, ex);
            }
        }

        /// <summary>
        /// Handle change of selected control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Control_SelectionChanged(object sender, AltControlEventArgs args)
        {
            try
            {
                ErrorMessage.Clear();

                // Ignore initial selection events from controls which are hidden
                FrameworkElement sourceViewControl = (FrameworkElement)sender;
                NamedItem selectedSource = (NamedItem)this.InputCombo.SelectedItem; 
                if (selectedSource != null && GetSourceViewControl(selectedSource.ID) == sourceViewControl)
                {
                    _selectedControl = args;

                    RefreshEventReasons();
                    RefreshActionsList();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF015, ex);
            }
        }

        /// <summary>
        /// Decide whether navigate command can execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            int actionListID = _currentActionList != null ? _currentActionList.ID : 0;
            e.CanExecute = _numActionLists != 0 && (_numActionLists > 1 || actionListID == 0);
            e.Handled = true;
        }

        /// <summary>
        /// Go to the next action list in the profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                int id = _currentActionList != null ? _currentActionList.ID : 0;
                id = (id < _numActionLists) ? id + 1 : 1;
                NavigateToActionList(id);
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF016, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Decide whether navigate command can execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            int actionListID = _currentActionList != null ? _currentActionList.ID : 0;
            e.CanExecute = _numActionLists != 0 && (_numActionLists > 1 || actionListID == 0);
            e.Handled = true;
        }

        /// <summary>
        /// Go to the previous action list in the profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                int id = _currentActionList != null ? _currentActionList.ID : 0;
                id = (id > 1) ? id - 1 : _numActionLists;
                NavigateToActionList(id);
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF017, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Show the action list with the specified ID
        /// </summary>
        /// <param name="id"></param>
        private void NavigateToActionList(int id)
        {
            ActionList actionList = _profile.GetActionListByID(id);
            if (actionList != null)
            {
                // Show the right state
                ShowLogicalState(actionList.LogicalState);

                AltControlEventArgs args = new AltControlEventArgs(actionList.EventArgs);

                // Show the right input
                InputCombo.SelectedValue = args.SourceID;

                // Set the right control
                IInputViewer viewerControl = GetSourceViewControl(args.SourceID);
                viewerControl.SetSelectedControl(args);

                // Set the selected control
                // Note: the position of this statement is important: other event handlers may change the selected control
                _selectedControl = args;

                // Select the right reason
                RefreshEventReasons();
                SetSelectedEventReason(actionList.EventArgs.EventReason);

                // Show the actions
                RefreshActionsList();
            }
        }

        /// <summary>
        /// Handle selection change for event reason radio buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReasonButtons_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();
                RefreshActionsList();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF018, ex);
            }
        }

        /// <summary>
        /// Execution mode of action list changed by user (parallel / series)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecutionModeButtons_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentActionList != null)
                {
                    EActionListType executionMode = (sender == this.SeriesRadioButton) ? EActionListType.Series : EActionListType.Parallel;
                    if (_currentActionList.ExecutionMode != executionMode)
                    {
                        _currentActionList.ExecutionMode = executionMode;
                        if (_currentActionList.Count > 0)
                        {
                            // Only register change if this action list isn't empty
                            SetProfileEdited(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PROF019, ex);
            }
        }

        /// <summary>
        /// Refresh the actions list
        /// </summary>
        private void RefreshActionsList()
        {
            NamedItem selectedMode = (NamedItem)this.ModeCombo.SelectedItem;
            NamedItem selectedApp = (NamedItem)this.AppCombo.SelectedItem;
            NamedItem selectedPage = (NamedItem)this.PageCombo.SelectedItem;
            NamedItem selectedSource = (NamedItem)this.InputCombo.SelectedItem;
            EEventReason selectedReason = GetSelectedEventReason();

            bool situationDefined = selectedMode != null && selectedApp != null && selectedPage != null;
            bool controlDefined = selectedSource != null && _selectedControl != null;
            if (situationDefined && controlDefined)
            {
                LogicalState logicalState = new LogicalState(selectedMode.ID, selectedApp.ID, selectedPage.ID);
                AltControlEventArgs ev = _selectedControl;

                // Set highlighting in control viewer
                HighlightMappedControls(logicalState, ev);

                if (selectedReason != EEventReason.None)
                {
                    ev.EventReason = selectedReason;

                    // Set the group box header
                    string suffix = "";
                    if ((ev.ControlType == EControlType.MousePointer || ev.ControlType == EControlType.MouseButtons)
                        && ev.Data != 0)
                    {
                        ScreenRegion region = (ScreenRegion)_profile.ScreenRegions.GetItemByID(ev.Data);
                        if (region != null)
                        {
                            suffix = string.Format(" '{0}'", region.Name);
                        }
                    }
                    this.ActionsGroupBox.Header = string.Format(Properties.Resources.String_ActionsToPerformLabel + " {0} {1}{2}",
                                                    ev.GetControlTypeName(),
                                                    ev.GetStateName(),
                                                    suffix);

                    // Populate list
                    PopulateActionsList(logicalState, ev);
                    if (_actionListItems.Count > 0)
                    {
                        this.ActionsList.SelectedIndex = 0;
                    }

                    // Enable add button
                    ActionsList.AddEnabled = true;
                }
            }
            else
            {
                // Clear the event highlighting
                UnhighlightMappedControls();
            }

            if (!situationDefined || !controlDefined || selectedReason == EEventReason.None)
            {
                // Set the group box header
                this.ActionsGroupBox.Header = Properties.Resources.Profile_ActionsGroupBox;

                // Clear the action list
                _currentActionList = null;
                _actionListItems.Clear();

                // Disable buttons
                ActionsList.AddEnabled = false;
            }

            // Update navigation buttons panel
            int actionListID = _currentActionList != null ? _currentActionList.ID : 0;
            _numActionLists = _profile.GetNumActionLists();
            string strTemplate = Properties.Resources.Profile_NavigateActionListsTextBlock;
            NavigateActionListsTextBlock.Text = string.Format(strTemplate, actionListID, _numActionLists);
            bool canNavigate = _numActionLists != 0 && (_numActionLists > 1 || actionListID == 0);
            PreviousActionListButton.IsEnabled = canNavigate;
            NextActionListButton.IsEnabled = canNavigate;
        }

        /// <summary>
        /// Display the list of actions for the selected logical state
        /// </summary>
        private void PopulateActionsList(LogicalState logicalState, AltControlEventArgs ev)
        {
            // Clear existing actions
            _currentActionList = null;
            NamedItemList listItems = _actionListItems;
            listItems.Clear();

            // Get actions table
            ActionMappingTable actionMappings = _profile.GetActionsForState(logicalState, false);

            // Get action list for this event
            _currentActionList = actionMappings.GetActions(ev.ToID());
            if (_currentActionList != null)
            {
                // Action list exists
                if (_currentActionList.ExecutionMode == EActionListType.Series)
                {
                    this.SeriesRadioButton.IsChecked = true;
                }
                else
                {
                    this.ParallelRadioButton.IsChecked = true;
                }

                int i = 0;
                foreach (BaseAction action in _currentActionList)
                {
                    listItems.Add(new NamedItem(i, action.Name));
                    i++;
                }
            }
            else
            {
                // Create new action list, with series execution mode by default
                _currentActionList = new ActionList();
                _currentActionList.ExecutionMode = EActionListType.Series;
                _currentActionList.LogicalState = new LogicalState(logicalState);
                _currentActionList.EventArgs = new AltControlEventArgs(ev);
                actionMappings.SetActions(ev.ToID(), _currentActionList);

                this.SeriesRadioButton.IsChecked = true;
            }
        }

        /// <summary>
        /// Highlight controls that have actions mapped to them
        /// </summary>
        /// <param name="actionMappings"></param>
        private void HighlightMappedControls(LogicalState logicalState, AltControlEventArgs selectedEvent)
        {
            // Prepare to store mapped reasons for this control and this situation
            Dictionary<EEventReason, bool> reasonsMapped = new Dictionary<EEventReason, bool>();

            IInputViewer viewerControl = GetSourceViewControl(selectedEvent.SourceID);
            if (viewerControl != null)
            {
                // Clear current highlighting
                viewerControl.ClearHighlighting();
                long controlID = selectedEvent.ToControlID();

                // Get action lists, including defaults
                ActionMappingTable actionMappings = _profile.GetActionsForState(logicalState, true);

                // Determine the highlight type for each control
                Dictionary<long, EHighlightType> highlightingTable = new Dictionary<long, EHighlightType>();
                Dictionary<long, ActionList>.Enumerator eActionList = actionMappings.GetEnumerator();
                while (eActionList.MoveNext())
                {
                    // Only interested in events for the currently displayed source
                    ActionList actionList = eActionList.Current.Value;
                    if (actionList.Count > 0 && actionList.EventArgs.SourceID == selectedEvent.SourceID)
                    {
                        // See if these actions are inherited defaults
                        bool sameLogicalState = (actionList.LogicalState.ModeID == logicalState.ModeID) &&
                                                (actionList.LogicalState.AppID == logicalState.AppID) &&
                                                (actionList.LogicalState.PageID == logicalState.PageID);

                        long targetControlID = actionList.EventArgs.ToControlID();
                        if (sameLogicalState)
                        {
                            highlightingTable[targetControlID] = EHighlightType.Configured;

                            if (actionList.EventArgs.ToControlID() == controlID)
                            {
                                // Same control (possibly different reason)
                                reasonsMapped[actionList.EventArgs.EventReason] = true;
                            }
                        }
                        else
                        {
                            if (!highlightingTable.ContainsKey(targetControlID))
                            {
                                highlightingTable[targetControlID] = EHighlightType.Default;
                            }
                        }
                    }
                }

                string configuredToolTip = Properties.Resources.String_Actions_defined;
                string defaultToolTip = Properties.Resources.String_Inherits_actions;

                // Highlight controls
                Dictionary<long, EHighlightType>.Enumerator eDict = highlightingTable.GetEnumerator();
                while (eDict.MoveNext())
                {
                    EHighlightType highlightType = eDict.Current.Value;
                    long targetControlID = eDict.Current.Key;
                    AltControlEventArgs targetControl = AltControlEventArgs.FromID(targetControlID);
                    if (targetControl.SourceID == selectedEvent.SourceID)
                    {
                        viewerControl.HighlightEvent(targetControl, new HighlightInfo(highlightType, highlightType == EHighlightType.Configured ? configuredToolTip : defaultToolTip));
                    }
                }
            }

            // Make mapped radio buttons bold
            Dictionary<EEventReason, RadioButton>.Enumerator eReason = _eventReasonButtons.GetEnumerator();
            while (eReason.MoveNext())
            {
                EEventReason reason = eReason.Current.Key;
                bool highlight = reasonsMapped.ContainsKey(reason);
                RadioButton button = eReason.Current.Value;
                button.FontWeight = highlight ? FontWeights.Bold : FontWeights.Normal;
            }
        }

        /// <summary>
        /// Clear highlights
        /// </summary>
        private void UnhighlightMappedControls()
        {
            // Remove radio button bold highlighting
            foreach (RadioButton button in _eventReasonButtons.Values)
            {
                button.FontWeight = FontWeights.Normal;
            }

            // Clear control highlights
            IInputViewer viewerControl = _visibleInputControl as IInputViewer;
            if (viewerControl != null)
            {
                viewerControl.ClearHighlighting();
            }
        }

        /// <summary>
        /// Get the control that displays the specified input source
        /// </summary>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        private IInputViewer GetSourceViewControl(long sourceID)
        {
            IInputViewer viewerControl = null;
            BaseSource source = _profile.GetInputSource(sourceID);
            switch (source.SourceType)
            {
                case ESourceType.Mouse:
                    viewerControl = this.SelectMouseInput; break;
                case ESourceType.Keyboard:
                    viewerControl = this.SelectKeyboardInput; break;
                case ESourceType.CustomWindow:
                    viewerControl = this.SelectCustomWindowInput; break;
            }

            return viewerControl;
        }

        /// <summary>
        /// Get the selected action and it's index in the actions list
        /// </summary>
        /// <param name="currentAction"></param>
        /// <param name="actionIndex"></param>
        private void GetSelectedAction(out BaseAction currentAction, out int actionIndex)
        {
            currentAction = null;
            actionIndex = -1;
            if (_currentActionList != null)
            {
                actionIndex = this.ActionsList.SelectedIndex;
                if (actionIndex > -1 && actionIndex < _currentActionList.Count)
                {
                    currentAction = _currentActionList[actionIndex];
                }
            }
        }

        /// <summary>
        /// Refresh the enabling / disabling of event reasons when the input source or control changes
        /// </summary>
        private void RefreshEventReasons()
        {
            List<EEventReason> supportedReasons = null;

            NamedItem selectedInput = (NamedItem)InputCombo.SelectedItem;
            if (selectedInput != null && _selectedControl != null)
            {
                BaseSource source = _profile.GetInputSource(selectedInput.ID);
                supportedReasons = source.GetSupportedEventReasons(_selectedControl);
            }

            // Enable / disable radio buttons
            Dictionary<EEventReason, RadioButton>.Enumerator eRadio = _eventReasonButtons.GetEnumerator();
            while (eRadio.MoveNext())
            {
                eRadio.Current.Value.IsEnabled = supportedReasons != null && supportedReasons.Contains(eRadio.Current.Key);

                // If the currently checked button is now disabled, uncheck it
                if (eRadio.Current.Value.IsChecked == true && !eRadio.Current.Value.IsEnabled)
                {
                    eRadio.Current.Value.IsChecked = false;
                }
            }

            // If no button is selected, select the first one that is supported
            if (supportedReasons != null && 
                supportedReasons.Count > 0 &&
                GetSelectedEventReason() == EEventReason.None)
            {
                SetSelectedEventReason(supportedReasons[0]);
            }
        }

        /// <summary>
        /// Select an event reason radio button
        /// </summary>
        /// <param name="reason"></param>
        private void SetSelectedEventReason(EEventReason reason)
        {
            if (_eventReasonButtons.ContainsKey(reason))
            {
                _eventReasonButtons[reason].IsChecked = true;
            }
        }

        /// <summary>
        /// Get the selected event reason radio button
        /// </summary>
        /// <returns></returns>
        private EEventReason GetSelectedEventReason()
        {
            EEventReason reason = EEventReason.None;
            Dictionary<EEventReason, RadioButton>.Enumerator eRadio = _eventReasonButtons.GetEnumerator();
            while (eRadio.MoveNext())
            {
                if (eRadio.Current.Value.IsChecked == true)
                {
                    reason = eRadio.Current.Key;
                    break;
                }
            }

            return reason;
        }

        /// <summary>
        /// Select which input control to show
        /// </summary>
        /// <param name="controlToShow"></param>
        private void ShowInputControl(FrameworkElement controlToShow)
        {
            if (_visibleInputControl != null)
            {
                _visibleInputControl.Visibility = Visibility.Collapsed;
            }
            if (controlToShow != null)
            {
                controlToShow.Visibility = Visibility.Visible;
            }
            _visibleInputControl = controlToShow;
        }

        /// <summary>
        /// Preview button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileSummaryWindow profileSummaryDialog = new ProfileSummaryWindow(null);
            profileSummaryDialog.CurrentProfile = _profile;
            profileSummaryDialog.ShowDialog();
        }

    }
}
