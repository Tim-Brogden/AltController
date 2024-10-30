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
using AltController.Config;
using AltController.Event;
using AltController.Actions;

namespace AltController
{
    /// <summary>
    /// Edit action window
    /// </summary>
    public partial class EditActionWindow : Window
    {
        private Utils _utils = new Utils();
        private FrameworkElement _visibleActionGrid;
        private BaseAction _currentAction;
        private AltControlEventArgs _inputEvent;
        private Profile _profile;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action">Can be null</param>
        /// <param name="args"></param>
        /// <param name="profile"></param>
        public EditActionWindow(BaseAction action, AltControlEventArgs args, Profile profile)
        {
            InitializeComponent();

            _currentAction = action;
            _inputEvent = args;
            _profile = profile;
        }

        /// <summary>
        /// Return the action created when the user clicked OK
        /// </summary>
        /// <returns></returns>
        public BaseAction GetAction()
        {
            return _currentAction;
        }

        /// <summary>
        /// Loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Bind child controls
            this.ChangeModeDetails.SetModesList(_profile.ModeDetails);
            this.ChangePageDetails.SetPagesList(_profile.PageDetails);

            // Action types
            this.ActionTypeCombo.ItemsSource = GUIUtils.GetValidActionTypeList(_inputEvent, _profile);

            // Display settings for current action
            if (_currentAction != null)
            {
                switch (_currentAction.ActionType)
                {
                    case EActionType.TypeKey:
                        this.TypeKeyDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.TypeText:
                        this.TypeTextDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.HoldKey:
                        this.HoldKeyDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.ReleaseKey:
                        this.ReleaseKeyDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.RepeatKey:
                        this.RepeatKeyDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.ToggleKey:
                        this.ToggleKeyDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.StopOngoingActions:
                    case EActionType.ScrollUp:
                    case EActionType.ScrollDown:
                    case EActionType.StopScrolling:
                        // Nothing to do
                        break;
                    case EActionType.RepeatScrollUp:
                    case EActionType.RepeatScrollDown:
                        this.RepeatScrollDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.MouseClick:
                    case EActionType.MouseDoubleClick:
                    case EActionType.MouseHold:
                    case EActionType.MouseRelease:
                        this.MouseButtonActionDetails.ActionType = _currentAction.ActionType;
                        this.MouseButtonActionDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.ToggleMouseButton:
                        this.ToggleMouseButtonActionDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.MoveThePointer:
                        this.MoveThePointerDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.ChangeMode:
                        this.ChangeModeDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.ChangePage:
                        this.ChangePageDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.ChangePointer:
                        this.ChangePointerDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.Wait:
                        this.WaitDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.RepeatKeyDirectional:
                        this.RepeatKeyDirectionalDetails.SetCurrentAction(_currentAction);
                        break;
                    case EActionType.MenuOption:
                        this.MenuActionDetails.SetCurrentAction(_currentAction);
                        break;
                }

                // Select action type
                this.ActionTypeCombo.SelectedValue = (long)_currentAction.ActionType;
            }
            else
            {
                // Select the first action type
                if (this.ActionTypeCombo.Items.Count > 0)
                {
                    this.ActionTypeCombo.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// New type of action selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                NamedItem selectedItem = (NamedItem)this.ActionTypeCombo.SelectedItem;
                if (selectedItem != null)
                {
                    EActionType actionType = (EActionType)selectedItem.ID;
                    switch (actionType)
                    {
                        case EActionType.TypeKey:
                            ShowActionGrid(this.TypeKeyDetails);
                            break;
                        case EActionType.TypeText:
                            ShowActionGrid(this.TypeTextDetails);
                            break;
                        case EActionType.HoldKey:
                            ShowActionGrid(this.HoldKeyDetails);
                            break;
                        case EActionType.ReleaseKey:
                            ShowActionGrid(this.ReleaseKeyDetails);
                            break;
                        case EActionType.RepeatKey:
                            ShowActionGrid(this.RepeatKeyDetails);
                            break;
                        case EActionType.ToggleKey:
                            ShowActionGrid(this.ToggleKeyDetails);
                            break;
                        case EActionType.StopOngoingActions:
                        case EActionType.ScrollUp:
                        case EActionType.ScrollDown:
                        case EActionType.StopScrolling:
                            ShowActionGrid(null);
                            break;
                        case EActionType.RepeatScrollUp:
                            this.RepeatScrollDetails.IsUp = true;
                            ShowActionGrid(this.RepeatScrollDetails);
                            break;
                        case EActionType.RepeatScrollDown:
                            this.RepeatScrollDetails.IsUp = false;
                            ShowActionGrid(this.RepeatScrollDetails);
                            break;
                        case EActionType.MouseClick:
                        case EActionType.MouseDoubleClick:
                        case EActionType.MouseHold:
                        case EActionType.MouseRelease:
                            this.MouseButtonActionDetails.ActionType = actionType;
                            ShowActionGrid(this.MouseButtonActionDetails);
                            break;
                        case EActionType.ToggleMouseButton:
                            ShowActionGrid(this.ToggleMouseButtonActionDetails);
                            break;
                        case EActionType.MoveThePointer:
                            ShowActionGrid(this.MoveThePointerDetails);
                            break;
                        case EActionType.ChangeMode:
                            ShowActionGrid(this.ChangeModeDetails);
                            break;
                        case EActionType.ChangePage:
                            ShowActionGrid(this.ChangePageDetails);
                            break;
                        case EActionType.ChangePointer:
                            //PopulateActionParams(new ChangePointerAction());
                            ShowActionGrid(this.ChangePointerDetails);
                            break;
                        case EActionType.Wait:
                            ShowActionGrid(this.WaitDetails);
                            break;
                        case EActionType.RepeatKeyDirectional:
                            ShowActionGrid(this.RepeatKeyDirectionalDetails);
                            break;
                        case EActionType.MenuOption:
                            ShowActionGrid(this.MenuActionDetails);
                            break;
                    }
                }
                else
                {
                    ShowActionGrid(null);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ACTION001, ex);
            }
        }

        /// <summary>
        /// OK Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            _currentAction = CreateAction();
            if (_currentAction != null)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(Properties.Resources.String_Add_action_error_description, 
                                Properties.Resources.String_Add_action_error_title, 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Information);
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
        /// Create an action according to the UI selections
        /// </summary>
        /// <returns></returns>
        private BaseAction CreateAction()
        {
            BaseAction action = null;

            NamedItem selectedItem = (NamedItem)this.ActionTypeCombo.SelectedItem;
            if (selectedItem != null)
            {
                EActionType actionType = (EActionType)selectedItem.ID;
                switch (actionType)
                {
                    case EActionType.TypeKey:
                        action = this.TypeKeyDetails.GetCurrentAction();
                        break;
                    case EActionType.TypeText:
                        action = this.TypeTextDetails.GetCurrentAction();
                        break;
                    case EActionType.HoldKey:
                        action = this.HoldKeyDetails.GetCurrentAction();
                        break;
                    case EActionType.ReleaseKey:
                        action = this.ReleaseKeyDetails.GetCurrentAction();
                        break;
                    case EActionType.RepeatKey:
                        action = this.RepeatKeyDetails.GetCurrentAction();
                        break;
                    case EActionType.ToggleKey:
                        action = this.ToggleKeyDetails.GetCurrentAction();
                        break;
                    case EActionType.StopOngoingActions:                        
                        action = new StopOngoingActionsAction();
                        break;
                    case EActionType.ScrollUp:
                        action = new ScrollWheelAction(true);
                        break;
                    case EActionType.ScrollDown:
                        action = new ScrollWheelAction(false);
                        break;
                    case EActionType.RepeatScrollUp:
                    case EActionType.RepeatScrollDown:
                        action = this.RepeatScrollDetails.GetCurrentAction();
                        break;
                    case EActionType.StopScrolling:
                        action = new StopScrollingAction();
                        break;
                    case EActionType.MouseClick:
                    case EActionType.MouseDoubleClick:
                    case EActionType.MouseHold:
                    case EActionType.MouseRelease:
                        action = this.MouseButtonActionDetails.GetCurrentAction();
                        break;
                    case EActionType.ToggleMouseButton:
                        action = this.ToggleMouseButtonActionDetails.GetCurrentAction();
                        break;
                    case EActionType.MoveThePointer:
                        action = this.MoveThePointerDetails.GetCurrentAction();
                        break;
                    case EActionType.ChangeMode:
                        action = this.ChangeModeDetails.GetCurrentAction();
                        break;
                    case EActionType.ChangePage:
                        action = this.ChangePageDetails.GetCurrentAction();
                        break;
                    case EActionType.ChangePointer:
                        action = this.ChangePointerDetails.GetCurrentAction();
                        break;
                    case EActionType.Wait:
                        action = this.WaitDetails.GetCurrentAction();
                        break;
                    case EActionType.RepeatKeyDirectional:
                        action = this.RepeatKeyDirectionalDetails.GetCurrentAction();
                        break;
                    case EActionType.MenuOption:
                        action = this.MenuActionDetails.GetCurrentAction();
                        break;
                }
            }

            return action;
        }
        
        /// <summary>
        /// Make the specified action grid visible and hide the others
        /// </summary>
        /// <param name="grid"></param>
        private void ShowActionGrid(FrameworkElement gridToShow)
        {
            if (_visibleActionGrid != null)
            {
                _visibleActionGrid.Visibility = Visibility.Hidden;
            }
            if (gridToShow != null)
            {
                gridToShow.Visibility = Visibility.Visible;
            }
            _visibleActionGrid = gridToShow;
        }
    }
}
