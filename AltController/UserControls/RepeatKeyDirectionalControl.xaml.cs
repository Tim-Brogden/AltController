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
    public partial class RepeatKeyDirectionalControl : UserControl
    {
        // Members
        private bool _isLoaded;
        private NamedItemList _keyListItems = new NamedItemList();
        private NamedItemList _directionListItems = new NamedItemList();
        private RepeatKeyDirectionalAction _currentAction = new RepeatKeyDirectionalAction();
        private GradientStop _whiteGradientStop;
        private GradientStop _blackGradientStop;
        private LinearGradientBrush _visualRepresentationBrush;

        /// <summary>
        /// Constructor
        /// </summary>
        public RepeatKeyDirectionalControl()
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

                this.LongerTowardsCombo.ItemsSource = _directionListItems;

                GUIUtils.PopulateDisplayableListWithKeys(_keyListItems);
                this.KeyToPressCombo.ItemsSource = _keyListItems;

                _isLoaded = true;

                if (_currentAction != null)
                {
                    this.KeyToPressCombo.SelectedValue = _currentAction.UniqueKeyID;
                    this.RepeatEverySlider.Value = _currentAction.UpdateEveryMS * 0.001;
                    this.TimeToMaxSlider.Value = _currentAction.TimeToMaxValueMS * 0.001;
                    this.TimeToMinSlider.Value = _currentAction.TimeToMinValueMS * 0.001;
                    this.SensitivitySlider.Value = _currentAction.Sensitivity;
                    this.LongerTowardsCombo.SelectedValue = (long)_currentAction.LongerPressesDirection;

                    this.AdditionalOptionsGroupBox.IsExpanded = _currentAction.TimeToMaxValueMS > 0 || _currentAction.TimeToMinValueMS > 0;
                }
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is RepeatKeyDirectionalAction)
            {
                _currentAction = (RepeatKeyDirectionalAction)action;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem keyItem = (NamedItem)this.KeyToPressCombo.SelectedItem;
            NamedItem directionItem = (NamedItem)this.LongerTowardsCombo.SelectedItem;
            if (keyItem != null && directionItem != null)
            {
                _currentAction = new RepeatKeyDirectionalAction();
                _currentAction.UniqueKeyID = keyItem.ID;
                _currentAction.UpdateEveryMS = (int)(this.RepeatEverySlider.Value * 1000);
                _currentAction.TimeToMaxValueMS = (int)(this.TimeToMaxSlider.Value * 1000);
                _currentAction.TimeToMinValueMS = (int)(this.TimeToMinSlider.Value * 1000);
                if (this.SensitivitySlider.Value > 0.0)
                {
                    _currentAction.Sensitivity = this.SensitivitySlider.Value;
                }
                else
                {
                    _currentAction.Sensitivity = 1.0;
                }
                _currentAction.LongerPressesDirection = (ELRUDState)directionItem.ID;
            }

            return _currentAction;
        }

        /// <summary>
        /// Update the visual representation of the key press strength
        /// </summary>
        private void RefreshVisualRepresentation()
        {
            if (_isLoaded)
            {
                NamedItem directionItem = (NamedItem)this.LongerTowardsCombo.SelectedItem;
                if (directionItem != null)
                {
                    double sensitivity = Math.Max(0.001, this.SensitivitySlider.Value);
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
    }
}
