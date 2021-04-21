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
using System.Windows;
using AltController.Actions;
using AltController.Core;

namespace AltController.UserControls
{
    public partial class MoveThePointerControl : System.Windows.Controls.UserControl
    {
        // Members
        private bool _isLoaded = false;
        private MoveThePointerAction _currentAction = new MoveThePointerAction();
        private NamedItemList _displayAreas = new NamedItemList();

        /// <summary>
        /// Constructor
        /// </summary>
        public MoveThePointerControl()
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
            _isLoaded = true;

            // Overlay positions
            GUIUtils.PopulateDisplayAreaList(_displayAreas);
            this.RelativeToCombo.ItemsSource = _displayAreas;

            if (_currentAction != null)
            {
                if (_currentAction.AbsoluteMove)
                {
                    this.AbsoluteRadioButton.IsChecked = true;
                }
                else
                {
                    this.RelativeRadioButton.IsChecked = true;
                }
                if (_currentAction.PercentOrPixels)
                {
                    this.PercentRadioButton.IsChecked = true;
                }
                else
                {
                    this.PixelsRadioButton.IsChecked = true;
                }
                RelativeToCombo.SelectedValue = (int)_currentAction.RelativeTo;
                this.XAmountSlider.Value = _currentAction.X;
                this.YAmountSlider.Value = _currentAction.Y;
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is MoveThePointerAction)
            {
                _currentAction = (MoveThePointerAction)action;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            _currentAction = new MoveThePointerAction();
            _currentAction.AbsoluteMove = this.AbsoluteRadioButton.IsChecked == true;
            _currentAction.PercentOrPixels = this.PercentRadioButton.IsChecked == true;
            NamedItem item = (NamedItem)this.RelativeToCombo.SelectedItem;
            if (item != null)
            {
                _currentAction.RelativeTo = (EDisplayArea)item.ID;
            }
            _currentAction.X = XAmountSlider.Value;
            _currentAction.Y = YAmountSlider.Value;

            return _currentAction;
        }

        private void MoveTypeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                this.AmountGroupBox.Header = this.AbsoluteRadioButton.IsChecked == true ? Properties.Resources.String_To_position : Properties.Resources.String_By_amount;
                this.OffsetGroupBox.IsEnabled = (this.AbsoluteRadioButton.IsChecked == true || this.PercentRadioButton.IsChecked == true);
                this.XAmountSlider.Minimum = this.AbsoluteRadioButton.IsChecked == true ? 0 : -this.XAmountSlider.Maximum;
                this.YAmountSlider.Minimum = this.AbsoluteRadioButton.IsChecked == true ? 0 : -this.YAmountSlider.Maximum;
            }
        }

        private void UnitsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                bool isAbsolute = this.AbsoluteRadioButton.IsChecked == true;
                bool isPercent = this.PercentRadioButton.IsChecked == true;
                this.XUnitsLabel.Text = this.PercentRadioButton.IsChecked == true ? "%" : "px";
                this.YUnitsLabel.Text = this.PercentRadioButton.IsChecked == true ? "%" : "px";
                this.XAmountSlider.Maximum = isPercent ? 100.0 : SystemParameters.VirtualScreenWidth;
                this.XAmountSlider.Minimum = this.AbsoluteRadioButton.IsChecked == true ? 0 : -this.XAmountSlider.Maximum;
                this.YAmountSlider.Maximum = isPercent ? 100.0 : SystemParameters.VirtualScreenHeight;
                this.YAmountSlider.Minimum = this.AbsoluteRadioButton.IsChecked == true ? 0 : -this.YAmountSlider.Maximum;
                this.OffsetGroupBox.IsEnabled = isAbsolute || isPercent;
            }
        }

    }
}
