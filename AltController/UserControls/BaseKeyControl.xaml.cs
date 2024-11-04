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
using System.Windows.Media;
using AltController.Core;
using AltController.Actions;

namespace AltController.UserControls
{
    public partial class BaseKeyControl : UserControl
    {
        // Members
        private bool _isLoaded;
        private EActionType _actionType;
        private long _selectedKeyID = (long)System.Windows.Forms.Keys.A;
        private BaseKeyAction _currentAction = null;
        private NamedItemList _keyListItems = new NamedItemList();
        private NamedItemList _directionListItems = new NamedItemList();
        private GradientStop _whiteGradientStop;
        private GradientStop _blackGradientStop;
        private LinearGradientBrush _visualRepresentationBrush;

        public long SelectedKeyID
        { 
            get 
            {
                return _selectedKeyID;
            }
            set
            {
                _selectedKeyID = value;
            }
        }      

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseKeyControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Control loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _whiteGradientStop = new GradientStop(Colors.White, 0.0);
                _blackGradientStop = new GradientStop(Colors.Black, 1.0);
                GradientStopCollection gradientStops = new GradientStopCollection();
                gradientStops.Add(_whiteGradientStop);
                gradientStops.Add(_blackGradientStop);
                _visualRepresentationBrush = new LinearGradientBrush(gradientStops);
                _visualRepresentationBrush.StartPoint = new Point(0.0, 0.0);
                _visualRepresentationBrush.EndPoint = new Point(1.0, 0.0);
                VisualRepresentationLabel.Background = _visualRepresentationBrush;

                _directionListItems.Clear();
                _directionListItems.Add(new NamedItem((long)ELRUDState.None, Properties.Resources.String_None_Option));
                _directionListItems.Add(new NamedItem((long)ELRUDState.Left, Properties.Resources.String_Left));
                _directionListItems.Add(new NamedItem((long)ELRUDState.Right, Properties.Resources.String_Right));
                _directionListItems.Add(new NamedItem((long)ELRUDState.Up, Properties.Resources.String_Top));
                _directionListItems.Add(new NamedItem((long)ELRUDState.Down, Properties.Resources.String_Bottom));
                LongerTowardsCombo.ItemsSource = _directionListItems;
                LongerTowardsCombo.SelectedValue = (long)ELRUDState.Right;

                GUIUtils.PopulateDisplayableListWithKeys(_keyListItems);
                KeyboardKeyCombo.ItemsSource = _keyListItems;

                _isLoaded = true;

                if (_currentAction != null)
                {
                    DisplayAction(_currentAction);
                }                
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is BaseKeyAction)
            {
                _currentAction = (BaseKeyAction)action;
                _actionType = _currentAction.ActionType;
                if (_isLoaded)
                {
                    DisplayAction(_currentAction);
                }
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem keyboardKey = (NamedItem)KeyboardKeyCombo.SelectedItem;
            if (keyboardKey != null)
            {
                switch (_actionType)
                {
                    case EActionType.TypeKey:
                        {
                            TypeKeyAction ac = new TypeKeyAction();
                            ac.IsAltModifierSet = AltCheck.IsChecked == true;
                            ac.IsControlModifierSet = ControlCheck.IsChecked == true;
                            ac.IsShiftModifierSet = ShiftCheck.IsChecked == true;
                            ac.IsWinModifierSet = WinCheck.IsChecked == true;
                            _currentAction = ac;
                        }
                        break;
                    case EActionType.HoldKey:
                        {
                            HoldKeyAction ac = new HoldKeyAction();
                            ac.PressDurationMS = ReleaseCheckbox.IsChecked == true ? (int)(HoldForSlider.Value * 1000.0) : 0;
                            _currentAction = ac;
                        }
                        break;
                    case EActionType.RepeatKey:
                        {
                            RepeatKeyAction ac = new RepeatKeyAction();
                            ac.PressDurationMS = (int)(RepeatHoldForSlider.Value * 1000.0);
                            ac.RepeatEveryMS = (int)(RepeatEverySlider.Value * 1000.0);
                            ac.StopAfterMS = (int)(StopAfterSlider.Value * 1000.0);
                            _currentAction = ac;
                        }
                        break;
                    case EActionType.RepeatKeyDirectional:
                        {
                            NamedItem directionItem = (NamedItem)LongerTowardsCombo.SelectedItem;
                            RepeatKeyDirectionalAction ac = new RepeatKeyDirectionalAction();
                            ac.UpdateEveryMS = (int)(RepeatDirectionalEverySlider.Value * 1000);
                            ac.TimeToMaxValueMS = (int)(TimeToMaxSlider.Value * 1000);
                            ac.TimeToMinValueMS = (int)(TimeToMinSlider.Value * 1000);
                            if (SensitivitySlider.Value > 0.0)
                            {
                                ac.Sensitivity = SensitivitySlider.Value;
                            }
                            else
                            {
                                ac.Sensitivity = 1.0;
                            }
                            ac.LongerPressesDirection = (ELRUDState)directionItem.ID;
                            _currentAction = ac;
                        }
                        break;
                    case EActionType.ToggleKey:
                        {
                            ToggleKeyAction ac = new ToggleKeyAction();
                            _currentAction = ac;
                        }
                        break;
                    case EActionType.ReleaseKey:
                        {
                            ReleaseKeyAction ac = new ReleaseKeyAction();
                            _currentAction = ac;
                        }
                        break;
                }
                if (_currentAction != null)
                {
                    _currentAction.UniqueKeyID = keyboardKey.ID;
                }
            }

            return _currentAction;
        }

        private void KeyboardKeyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SelectedKeyID = ((NamedItem)e.AddedItems[0]).ID;
            }
        }

        /// <summary>
        /// Release key checkbox changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReleaseCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            HoldForSlider.IsEnabled = ReleaseCheckbox.IsChecked == true;
            if (ReleaseCheckbox.IsChecked != true)
            {
                HoldForSlider.Value = 0;
            }
        }

        /// <summary>
        /// Direction changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LongerTowardsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshVisualRepresentation();
        }

        /// <summary>
        /// Sensitivity changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SensitivitySlider_ValueChanged(object sender, Event.DoubleValEventArgs args)
        {
            RefreshVisualRepresentation();
        }

        /// <summary>
        /// Update the visual representation of the key press strength
        /// </summary>
        private void RefreshVisualRepresentation()
        {
            if (_isLoaded)
            {
                NamedItem directionItem = (NamedItem)LongerTowardsCombo.SelectedItem;
                if (directionItem != null)
                {
                    double sensitivity = Math.Max(0.001, SensitivitySlider.Value);
                    _blackGradientStop.Offset = 1.0 / sensitivity;
                    ELRUDState direction = (ELRUDState)directionItem.ID;
                    switch (direction)
                    {
                        case ELRUDState.Left:
                            _visualRepresentationBrush.StartPoint = new Point(1.0, 0.0);
                            _visualRepresentationBrush.EndPoint = new Point(0.0, 0.0);
                            break;
                        case ELRUDState.Up:
                            _visualRepresentationBrush.StartPoint = new Point(0.0, 1.0);
                            _visualRepresentationBrush.EndPoint = new Point(0.0, 0.0);
                            break;
                        case ELRUDState.Right:
                            _visualRepresentationBrush.StartPoint = new Point(0.0, 0.0);
                            _visualRepresentationBrush.EndPoint = new Point(1.0, 0.0);
                            break;
                        case ELRUDState.Down:
                            _visualRepresentationBrush.StartPoint = new Point(0.0, 0.0);
                            _visualRepresentationBrush.EndPoint = new Point(0.0, 1.0);
                            break;
                        default:
                            _visualRepresentationBrush.StartPoint = new Point(-1000, -1000.0);
                            _visualRepresentationBrush.EndPoint = new Point(1000.0, 1000.0);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Show the correct information according to the action type
        /// </summary>
        private void DisplayActionType()
        {
            // Set text caption
            TypeKeySettings.Visibility = Visibility.Collapsed;
            HoldKeySettings.Visibility = Visibility.Collapsed;
            RepeatKeySettings.Visibility = Visibility.Collapsed;
            RepeatKeyDirectionalSettings.Visibility = Visibility.Collapsed;
            switch (_actionType)
            {
                case EActionType.TypeKey:
                    CaptionTextBlock.Text = Properties.Resources.Action_KeyToTypeLabel;
                    TypeKeySettings.Visibility = Visibility.Visible;
                    break;
                case EActionType.HoldKey:
                    CaptionTextBlock.Text = Properties.Resources.Action_HoldDownKeyLabel;
                    HoldKeySettings.Visibility = Visibility.Visible;
                    break;
                case EActionType.RepeatKey:
                    CaptionTextBlock.Text = Properties.Resources.Action_KeyToPressLabel;
                    RepeatKeySettings.Visibility = Visibility.Visible;
                    break;
                case EActionType.RepeatKeyDirectional:
                    CaptionTextBlock.Text = Properties.Resources.Action_KeyToPressLabel;
                    RepeatKeyDirectionalSettings.Visibility = Visibility.Visible;
                    break;
                case EActionType.ToggleKey:
                    CaptionTextBlock.Text = Properties.Resources.Action_KeyToToggleLabel;
                    break;
                case EActionType.ReleaseKey:
                    CaptionTextBlock.Text = Properties.Resources.Action_KeyToReleaseLabel;
                    break;
            }            
        }

        private void DisplayAction(BaseKeyAction action)
        {
            DisplayActionType();
            KeyboardKeyCombo.SelectedValue = action.UniqueKeyID;
            switch (action.ActionType)
            {
                case EActionType.TypeKey:
                    {
                        TypeKeyAction ac = (TypeKeyAction)action;
                        AltCheck.IsChecked = ac.IsAltModifierSet;
                        ControlCheck.IsChecked = ac.IsControlModifierSet;
                        ShiftCheck.IsChecked = ac.IsShiftModifierSet;
                        WinCheck.IsChecked = ac.IsWinModifierSet;
                    }
                    break;
                case EActionType.HoldKey:
                    {
                        HoldKeyAction ac = (HoldKeyAction)action;
                        ReleaseCheckbox.IsChecked = ac.PressDurationMS > 0;
                        HoldForSlider.IsEnabled = ac.PressDurationMS > 0;
                        HoldForSlider.Value = ac.PressDurationMS * 0.001;
                    }
                    break;
                case EActionType.RepeatKey:
                    {
                        RepeatKeyAction ac = (RepeatKeyAction)action;
                        RepeatEverySlider.Value = ac.RepeatEveryMS * 0.001;
                        RepeatHoldForSlider.Value = ac.PressDurationMS * 0.001;
                        StopAfterSlider.Value = ac.StopAfterMS * 0.001;
                    }
                    break;
                case EActionType.RepeatKeyDirectional:
                    {
                        RepeatKeyDirectionalAction ac = (RepeatKeyDirectionalAction)action;
                        LongerTowardsCombo.SelectedValue = (long)ac.LongerPressesDirection;
                        RepeatDirectionalEverySlider.Value = ac.UpdateEveryMS * 0.001;
                        SensitivitySlider.Value = ac.Sensitivity;

                        AdditionalOptionsGroupBox.IsExpanded = ac.TimeToMaxValueMS > 0 || ac.TimeToMinValueMS > 0;
                        TimeToMaxSlider.Value = ac.TimeToMaxValueMS * 0.001;
                        TimeToMinSlider.Value = ac.TimeToMinValueMS * 0.001;
                    }
                    break;
            }
        }

    }
}
