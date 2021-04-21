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
using AltController.Config;
using AltController.Core;
using AltController.Event;
using AltController.Input;

namespace AltController.UserControls
{
    /// <summary>
    /// Interaction logic for ViewMouseControl.xaml
    /// </summary>
    public partial class ViewMouseControl : UserControl, IInputViewer
    {
        // Members
        private bool _isLoaded;
        private MouseSource _source;
        private ScreenRegionList _regionsList;
        private NamedItemList _regionsComboList = new NamedItemList();
        private AltControlEventArgs _currentSelection;
        private Button _selectedButton = null;
        private ScreenRegion _selectedRegion;
        private Dictionary<Button, HighlightInfo> _buttonHighlighting = new Dictionary<Button, HighlightInfo>();
        private Brush _defaultForegroundBrush;
        private FontWeight _defaultFontWeight;
        private Brush _defaultBorderBrush;
        private Thickness _defaultBorderThickness;
        private Brush _selectedBrush;
        private Brush _defaultsBrush;
        private Brush _configuredBrush;
        private Brush _noHighlightBrush;

        public event AltControlEventHandler SelectionChanged;

        public ViewMouseControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the application configuration
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            RegionsControl.SetAppConfig(appConfig);
        }

        /// <summary>
        /// Set which source to display
        /// </summary>
        /// <param name="source"></param>
        public void SetSource(BaseSource source)
        {
            _source = source as MouseSource;
            _regionsList = _source.Profile.ScreenRegions;
            RegionsControl.SetRegionsList(_regionsList);

            if (_isLoaded)
            {
                InitialiseDisplay();
                if (_currentSelection != null)
                {                    
                    _currentSelection.SourceID = _source.ID;
                    RaiseSelectionChangedIfRequired(_currentSelection);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                _selectedBrush = Brushes.Red;
                _defaultsBrush = new LinearGradientBrush(Colors.LightGray, Colors.Gray, 90.0);
                _configuredBrush = new LinearGradientBrush(Colors.LightBlue, Colors.SteelBlue, 90.0);
                _noHighlightBrush = this.Pointer.Background;
                _defaultForegroundBrush = this.Pointer.Foreground;
                _defaultFontWeight = this.Pointer.FontWeight;
                _defaultBorderBrush = this.Pointer.BorderBrush;
                _defaultBorderThickness = this.Pointer.BorderThickness;

                RegionsComboBox.ItemsSource = _regionsComboList;            

                // Initialise screen regions display
                InitialiseDisplay();

                // Select initial control
                if (_source != null)
                {
                    if (_currentSelection == null)
                    {
                        _currentSelection = new AltControlEventArgs(_source.ID, EControlType.MousePointer, ESide.None, 0, ELRUDState.None, EEventReason.None);
                        if (_regionsList != null && _regionsList.Count != 0)
                        {
                            _currentSelection.Data = (byte)_regionsList[0].ID;
                        }
                    }
                    SetSelectedControl(_currentSelection);
                    RaiseSelectionChangedIfRequired(_currentSelection);
                }
            }
        }

        /// <summary>
        /// Initialise screen regions display
        /// </summary>
        private void InitialiseDisplay()
        {
            if (_source != null)
            {
                // Remember the current region selection
                long regionID = _currentSelection != null ? _currentSelection.Data : Constants.NoneID;

                // Populate canvas
                RegionsControl.RefreshBackground();
                RegionsControl.RefreshDisplay();

                // Populate regions combo box
                PopulateRegionsComboBox(_regionsList);

                // Select the same region if possible
                if (_regionsList.GetItemByID(regionID) == null)
                {
                    regionID = Constants.NoneID;
                }
                RegionsComboBox.SelectedValue = regionID;
            }
        }

        /// <summary>
        /// Populate the regions combo box
        /// </summary>
        /// <param name="_regionsList"></param>
        private void PopulateRegionsComboBox(ScreenRegionList _regionsList)
        {
            _regionsComboList.Clear();
            _regionsComboList.Add(new NamedItem(Constants.NoneID, "N/A"));
            if (_regionsList != null)
            {
                foreach (ScreenRegion region in _regionsList)
                {
                    _regionsComboList.Add(region);
                }
            }
        }

        /// <summary>
        /// Handle screen region click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionsControl_SelectedRegionChanged(object sender, EventArgs e)
        {
            long regionID = Constants.NoneID;
            if (RegionsControl.SelectedRegions.Count != 0)
            {
                regionID = RegionsControl.SelectedRegions[0].ID;
            }
            RegionsComboBox.SelectedValue = regionID;
        }

        /// <summary>
        /// Handle change of selected screen region
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = RegionsComboBox.SelectedIndex;
            if (selectedIndex > -1)
            {
                if (selectedIndex > 0 && selectedIndex < _regionsComboList.Count) 
                {
                    // A region selected
                    _selectedRegion = (ScreenRegion)_regionsComboList[selectedIndex];
                    RegionsControl.SetSelectedRegions(new List<ScreenRegion>() { _selectedRegion });
                }
                else
                {
                    // Region N/A selected
                    _selectedRegion = null;
                    RegionsControl.SetSelectedRegions(null);
                }

                if (_currentSelection != null)
                {
                    _currentSelection.Data = _selectedRegion != null ? (byte)_selectedRegion.ID : (byte)Constants.NoneID;
                    RaiseSelectionChangedIfRequired(_currentSelection);
                }
            }
        }

        /// <summary>
        /// Get the selected control
        /// </summary>
        /// <returns></returns>
        public AltControlEventArgs GetSelectedControl()
        {
            return _currentSelection;
        }

        /// <summary>
        /// Set the selected control
        /// </summary>
        /// <param name="args"></param>
        public void SetSelectedControl(AltControlEventArgs args)
        {
            _currentSelection = args;
            if (_isLoaded)
            {
                Button button = GetButtonFromEvent(args);
                if (button != null)
                {
                    SelectButton(button);

                    RegionsComboBox.SelectedValue = (long)args.Data;
                }
            }
        }        

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            _currentSelection = new AltControlEventArgs(_source.ID,
                                                        EControlType.MouseButtons,
                                                        ESide.None,
                                                        (byte)EMouseButton.Left,
                                                        ELRUDState.None,
                                                        EEventReason.None);
            _currentSelection.Data = _selectedRegion != null ? (byte)_selectedRegion.ID : (byte)Constants.NoneID;
            SelectButton((Button)sender);
            RaiseSelectionChangedIfRequired(_currentSelection);
        }

        private void MiddleButton_Click(object sender, RoutedEventArgs e)
        {
            _currentSelection = new AltControlEventArgs(_source.ID,
                                                        EControlType.MouseButtons,
                                                        ESide.None,
                                                        (byte)EMouseButton.Middle,
                                                        ELRUDState.None,
                                                        EEventReason.None);
            _currentSelection.Data = _selectedRegion != null ? (byte)_selectedRegion.ID : (byte)Constants.NoneID;
            SelectButton((Button)sender);
            RaiseSelectionChangedIfRequired(_currentSelection);
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            _currentSelection = new AltControlEventArgs(_source.ID,
                                                        EControlType.MouseButtons,
                                                        ESide.None,
                                                        (byte)EMouseButton.Right,
                                                        ELRUDState.None,
                                                        EEventReason.None);
            _currentSelection.Data = _selectedRegion != null ? (byte)_selectedRegion.ID : (byte)Constants.NoneID;
            SelectButton((Button)sender);
            RaiseSelectionChangedIfRequired(_currentSelection);
        }

        private void X1Button_Click(object sender, RoutedEventArgs e)
        {
            _currentSelection = new AltControlEventArgs(_source.ID,
                                                        EControlType.MouseButtons,
                                                        ESide.None,
                                                        (byte)EMouseButton.X1,
                                                        ELRUDState.None,
                                                        EEventReason.None);
            _currentSelection.Data = _selectedRegion != null ? (byte)_selectedRegion.ID : (byte)Constants.NoneID;
            SelectButton((Button)sender);
            RaiseSelectionChangedIfRequired(_currentSelection);
        }

        private void X2Button_Click(object sender, RoutedEventArgs e)
        {
            _currentSelection = new AltControlEventArgs(_source.ID,
                                                        EControlType.MouseButtons,
                                                        ESide.None,
                                                        (byte)EMouseButton.X2,
                                                        ELRUDState.None,
                                                        EEventReason.None);
            _currentSelection.Data = _selectedRegion != null ? (byte)_selectedRegion.ID : (byte)Constants.NoneID;
            SelectButton((Button)sender);
            RaiseSelectionChangedIfRequired(_currentSelection);
        }

        private void Pointer_Click(object sender, RoutedEventArgs e)
        {
            _currentSelection = new AltControlEventArgs(_source.ID,
                                                        EControlType.MousePointer,
                                                        ESide.None,
                                                        0,
                                                        ELRUDState.None,
                                                        EEventReason.None);
            _currentSelection.Data = _selectedRegion != null ? (byte)_selectedRegion.ID : (byte)Constants.NoneID;
            SelectButton((Button)sender);
            RaiseSelectionChangedIfRequired(_currentSelection);
        }

        /// <summary>
        /// Raise event if needed
        /// </summary>
        /// <param name="args"></param>
        private void RaiseSelectionChangedIfRequired(AltControlEventArgs args)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, args);
            }
        }

        /// <summary>
        /// Clear highlighting
        /// </summary>
        public void ClearHighlighting()
        {
            foreach (Button button in _buttonHighlighting.Keys)
            {
                HighlightButton(button, new HighlightInfo(EHighlightType.None, null));
            }
            _buttonHighlighting.Clear();
        }

        /// <summary>
        /// Highlight the specified event
        /// </summary>
        /// <param name="args"></param>
        /// <param name="highlightType"></param>
        public void HighlightEvent(AltControlEventArgs args, HighlightInfo highlight)
        {
            if (_isLoaded)
            {
                // Check the event is for the selected region
                long regionID = _selectedRegion != null ? _selectedRegion.ID : Constants.NoneID;
                if (args.Data == regionID)
                {
                    Button button = GetButtonFromEvent(args);
                    if (button != null)
                    {
                        _buttonHighlighting[button] = highlight;
                        HighlightButton(button, highlight);
                    }
                }
            }
        }

        /// <summary>
        /// Get the button corresponding to an event
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private Button GetButtonFromEvent(AltControlEventArgs args)
        {
            Button button = null;
            if (args.ControlType == EControlType.MouseButtons)
            {
                switch (args.ButtonID)
                {
                    case (byte)EMouseButton.Left:
                        button = LeftButton; break;
                    case (byte)EMouseButton.Middle:
                        button = MiddleButton; break;
                    case (byte)EMouseButton.Right:
                        button = RightButton; break;
                    case (byte)EMouseButton.X1:
                        button = X1Button; break;
                    case (byte)EMouseButton.X2:
                        button = X2Button; break;
                }
            }
            else if (args.ControlType == EControlType.MousePointer)
            {
                button = Pointer;
            }

            return button;
        }

        /// <summary>
        /// Select the specified button and deselect any previous selection
        /// </summary>
        /// <param name="button"></param>
        private void SelectButton(Button button)
        {
            if (_selectedButton != null)
            {
                _selectedButton.Foreground = _defaultForegroundBrush;
                _selectedButton.FontWeight = _defaultFontWeight;
                _selectedButton.BorderBrush = _defaultBorderBrush;
                _selectedButton.BorderThickness = _defaultBorderThickness;
            }
            button.Foreground = _selectedBrush;
            button.FontWeight = FontWeights.Bold;
            button.BorderBrush = _selectedBrush;
            button.BorderThickness = new Thickness(2);
            button.Focus();

            _selectedButton = button;
        }
        
        /// <summary>
        /// Highlight the selected control
        /// </summary>
        /// <param name="button"></param>
        private void HighlightButton(Button button, HighlightInfo highlight)
        {            
            switch (highlight.HighlightType)
            {
                case EHighlightType.None:
                    button.Background = _noHighlightBrush;
                    break;
                case EHighlightType.Default:
                    button.Background = _defaultsBrush;
                    break;
                case EHighlightType.Configured:
                    button.Background = _configuredBrush;
                    break;
            }
            button.ToolTip = highlight.ToolTip;
        }
    }
}
