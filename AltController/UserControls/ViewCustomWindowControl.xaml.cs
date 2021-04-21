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
using System.Windows.Media;
using AltController.Config;
using AltController.Core;
using AltController.Event;
using AltController.Input;

namespace AltController.UserControls
{
    /// <summary>
    /// Interaction logic for ViewCustomWindowControl.xaml
    /// </summary>
    public partial class ViewCustomWindowControl : UserControl, IInputViewer
    {
        // Members
        private AppConfig _appConfig;
        private bool _isLoaded = false;
        private bool _isDesignMode = false;
        private bool _enableColours = true;
        private CustomWindowSource _source;
        private AltControlEventArgs _currentSelection;
        private Button _selectedButton = null;
        private Dictionary<Button, HighlightInfo> _buttonHighlighting = new Dictionary<Button, HighlightInfo>();
        private Brush _defaultForegroundBrush;
        private FontWeight _defaultFontWeight;
        private Brush _defaultBorderBrush;
        private Thickness _defaultBorderThickness;
        private Brush _selectedBrush;
        private Brush _defaultsBrush;
        private Brush _configuredBrush;
        private Brush _noHighlightBrush;
        private const int _tabOrderGranularity = 50;
        private Button _dragButton;
        private bool _dragStarted;
        private Point _dragStartPoint;

        // Properties
        public bool IsDesignMode { get { return _isDesignMode; } set { _isDesignMode = value; } }
        public bool EnableColours { get { return _enableColours; } set { _enableColours = value; } }

        // Events
        public event AltControlEventHandler SelectionChanged;
        public event AltControlEventHandler Pressed;
        public event AltControlEventHandler Released;
        public event AltControlEventHandler Inside;
        public event AltControlEventHandler Outside;

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewCustomWindowControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the application configuration
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }
        
        /// <summary>
        /// Set which source to display
        /// </summary>
        /// <param name="source"></param>
        public void SetSource(BaseSource source)
        {
            _source = source as CustomWindowSource;
            InitialiseDisplay();
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;

                // Button brushes
                _selectedBrush = Brushes.Red;
                _defaultsBrush = new LinearGradientBrush(Colors.LightGray, Colors.Gray, 90.0);
                _configuredBrush = new LinearGradientBrush(Colors.LightBlue, Colors.SteelBlue, 90.0);
                _noHighlightBrush = this.InvisibleButton.Background;
                _defaultForegroundBrush = this.InvisibleButton.Foreground;
                _defaultFontWeight = this.InvisibleButton.FontWeight;
                _defaultBorderBrush = this.InvisibleButton.BorderBrush;
                _defaultBorderThickness = this.InvisibleButton.BorderThickness;

                // Enable drag drop if required
                if (AllowDrop)
                {
                    CustomWindowCanvas.Drop += CustomWindowCanvas_Drop;
                }

                InitialiseDisplay();
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
                    SetCurrentButton(button, EEventReason.None);
                }
            }
        }

        /// <summary>
        /// Custom button click (design mode only)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentButton((Button)sender, EEventReason.None);
            RaiseEventIfRequired(_currentSelection);
            e.Handled = true;
        }

        /// <summary>
        /// Custom button mouse down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDesignMode)
            {
                if (AllowDrop)
                {
                    Button button = (Button)sender;
                    _dragStartPoint = e.GetPosition(CustomWindowCanvas);
                    _dragButton = button;
                    _dragStarted = false;
                    CustomWindowCanvas.PreviewMouseMove += CustomWindowCanvas_PreviewMouseMove;
                }
            }
            else
            {
                SetCurrentButton((Button)sender, EEventReason.Pressed);
                RaiseEventIfRequired(_currentSelection);
            }
        }

        /// <summary>
        /// Handle mouse move (only in design mode)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomWindowCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragButton != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point mousePos = e.GetPosition(CustomWindowCanvas);
                    Vector diff = mousePos - _dragStartPoint;

                    if (!_dragStarted)
                    {
                        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                        {
                            CustomButtonData buttonData = (CustomButtonData)_dragButton.Tag;

                            // Perform drag & drop operation
                            _dragStarted = true;
                            DataObject dragData = new DataObject(typeof(CustomButtonData).FullName, buttonData);
                            try
                            {
                                //DragDrop.AddGiveFeedbackHandler(CustomWindowCanvas, CustomWindowCanvas_GiveFeedback);
                                CustomWindowCanvas.DragOver += new DragEventHandler(CustomWindowCanvas_DragOver);
                                DragDrop.DoDragDrop(_dragButton, dragData, DragDropEffects.Move);
                            }
                            finally
                            {
                                //DragDrop.RemoveGiveFeedbackHandler(CustomWindowCanvas, CustomWindowCanvas_GiveFeedback);
                                CustomWindowCanvas.DragOver -= CustomWindowCanvas_DragOver;
                            }
                        }
                    }
                }
                else
                {
                    if (_dragStarted)
                    {
                        _dragStarted = false;
                        // Reposition button in case mouse button released outside canvas area
                        RefreshButtonLayout(_dragButton);
                    }
                    CustomWindowCanvas.PreviewMouseMove -= CustomWindowCanvas_PreviewMouseMove;
                    _dragButton = null;
                }
            }
        }

        /// <summary>
        /// Move button during drag drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomWindowCanvas_DragOver(object sender, DragEventArgs e)
        {
            if (_dragButton != null)
            {
                Point mousePos = e.GetPosition(CustomWindowCanvas);
                Vector diff = mousePos - _dragStartPoint;
                CustomButtonData buttonData = (CustomButtonData)_dragButton.Tag;

                double x = Math.Max(0.0, Math.Min(buttonData.X + diff.X, CustomWindowCanvas.Width - buttonData.Width));
                double y = Math.Max(0.0, Math.Min(buttonData.Y + diff.Y, CustomWindowCanvas.Height - buttonData.Height));
                Canvas.SetLeft(_dragButton, x);
                Canvas.SetTop(_dragButton, y);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handle drag drop (only in design mode)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomWindowCanvas_Drop(object sender, DragEventArgs e)
        {
            if ((e.Effects & DragDropEffects.Move) != 0 &&
                e.Data.GetDataPresent(typeof(CustomButtonData).FullName) &&
                _dragButton != null)
            {
                CustomButtonData buttonData = (CustomButtonData)_dragButton.Tag;
                Point mousePos = e.GetPosition(CustomWindowCanvas);
                Vector diff = mousePos - _dragStartPoint;

                // Reposition the button
                double x = Math.Max(0.0, Math.Min(buttonData.X + diff.X, CustomWindowCanvas.Width - buttonData.Width));
                double y = Math.Max(0.0, Math.Min(buttonData.Y + diff.Y, CustomWindowCanvas.Height - buttonData.Height));
                Canvas.SetLeft(_dragButton, x);
                Canvas.SetTop(_dragButton, y);

                // Update the button data on completion of drag drop
                buttonData.X = x;
                buttonData.Y = y;

                // Raise event to select the button we moved
                SetCurrentButton(_dragButton, EEventReason.None);
                RaiseEventIfRequired(_currentSelection);

                // Mark drag as complete
                _dragStarted = false;
                _dragButton = null;
                CustomWindowCanvas.PreviewMouseMove -= CustomWindowCanvas_PreviewMouseMove;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Custom button mouse up (not used in design mode)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetCurrentButton((Button)sender, EEventReason.Released);
            RaiseEventIfRequired(_currentSelection);
        }

        /// <summary>
        /// Custom button mouse enter (not used in design mode)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomButton_MouseEnter(object sender, MouseEventArgs e)
        {
            SetCurrentButton((Button)sender, EEventReason.Inside);
            RaiseEventIfRequired(_currentSelection);
        }

        /// <summary>
        /// Custom button mouse leave (not used in design mode)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomButton_MouseLeave(object sender, MouseEventArgs e)
        {
            SetCurrentButton((Button)sender, EEventReason.Outside);
            RaiseEventIfRequired(_currentSelection);
        }

        /// <summary>
        /// Display the custom window buttons
        /// </summary>
        private void InitialiseDisplay()
        {
            if (_isLoaded && _source != null)
            {
                // Remove any existing custom buttons from the display
                CustomWindowCanvas.Children.Clear();

                // Size the control
                CustomWindowCanvas.Width = _source.WindowWidth;
                CustomWindowCanvas.Height = _source.WindowHeight;

                // Control background
                RefreshBackgroundColours();

                // Clear highlighting and selection
                _buttonHighlighting.Clear();
                _selectedButton = null;
                _currentSelection = null;

                // Add buttons to display
                foreach (CustomButtonData buttonData in _source.CustomButtons)
                {
                    AddButtonToDisplay(buttonData);
                }

                // Select an initial button if we're in design mode
                if (_isDesignMode)
                {
                    if (_currentSelection == null)
                    {
                        Button button = GetFirstButton();
                        if (button != null)
                        {
                            SetCurrentButton(button, EEventReason.None);
                        }
                    }
                    else
                    {
                        SetSelectedControl(_currentSelection);
                    }
                    RaiseEventIfRequired(_currentSelection);
                }

                // Update the tab order
                RefreshTabOrder();
            }
        }

        /// <summary>
        /// Create a new button
        /// </summary>
        /// <param name="buttonData"></param>
        public void CreateButton(CustomButtonData buttonData)
        {
            // Create button
            _source.CustomButtons.Add(buttonData);

            // Add to display
            Button button = AddButtonToDisplay(buttonData);

            // Update the tab order
            RefreshTabOrder();

            // Select button
            SetCurrentButton(button, EEventReason.None);
            RaiseEventIfRequired(_currentSelection);
        }

        /// <summary>
        /// Add a custom button 
        /// </summary>
        /// <param name="buttonData"></param>
        private Button AddButtonToDisplay(CustomButtonData buttonData)
        {
            // Add button to display
            Button button = new Button();
            button.Tag = buttonData;
            if (_isDesignMode)
            {
                button.Click += new RoutedEventHandler(CustomButton_Click);
                if (AllowDrop)
                {
                    button.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(CustomButton_PreviewMouseLeftButtonDown);
                    button.Cursor = Cursors.Hand;
                }
            }
            else
            {
                button.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(CustomButton_PreviewMouseLeftButtonDown);
                button.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(CustomButton_PreviewMouseLeftButtonUp);
                button.MouseEnter += new MouseEventHandler(CustomButton_MouseEnter);
                button.MouseLeave += new MouseEventHandler(CustomButton_MouseLeave);
            }
            CustomWindowCanvas.Children.Add(button);

            // Display the button
            RefreshButtonLayout(button);
            RefreshButtonBackground(button);

            return button;
        }

        /// <summary>
        /// Remove selected custom button
        /// </summary>
        /// <param name="buttonData"></param>
        public void RemoveSelectedButton()
        {
            if (_selectedButton != null)
            {
                // Remove selected button
                CustomWindowCanvas.Children.Remove(_selectedButton);
                _source.CustomButtons.Remove((CustomButtonData)_selectedButton.Tag);

                // Select first button
                Button button = GetFirstButton();
                if (button != null)
                {
                    SetCurrentButton(button, EEventReason.None);
                }
                else
                {
                    _currentSelection = null;
                    _selectedButton = null;
                }
                RaiseEventIfRequired(_currentSelection);
            }
        }

        /// <summary>
        /// Return the first button if there is one
        /// </summary>
        /// <returns></returns>
        private Button GetFirstButton()
        {
            Button firstButton = null;
            if (CustomWindowCanvas.Children.Count > 0)
            {
                firstButton = (Button)CustomWindowCanvas.Children[0];
            }

            return firstButton;
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
                Button button = GetButtonFromEvent(args);
                if (button != null)
                {
                    _buttonHighlighting[button] = highlight;
                    HighlightButton(button, highlight);
                }
            }
        }

        /// <summary>
        /// Get the button corresponding to the specified event
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private Button GetButtonFromEvent(AltControlEventArgs args)
        {
            Button matchedButton = null;

            foreach (Button button in CustomWindowCanvas.Children)
            {
                CustomButtonData buttonData = (CustomButtonData)button.Tag;
                if (buttonData != null && buttonData.ID == args.Data)
                {
                    matchedButton = button;
                    break;
                }
            }

            return matchedButton;
        }

        /// <summary>
        /// Handle button selection
        /// </summary>
        /// <param name="buttonData"></param>
        /// <param name="reason">Reason=None signifies selection in design mode</param>
        private void SetCurrentButton(Button button, EEventReason reason)
        {
            // Highlight
            if (_isDesignMode)
            {
                HighlightSelectedButton(button);
            }

            // Set current button
            CustomButtonData buttonData = (CustomButtonData)button.Tag;
            _currentSelection = new AltControlEventArgs(_source.ID,
                                                        EControlType.CustomButton,
                                                        ESide.None,
                                                        0,
                                                        ELRUDState.None,
                                                        reason);
            _currentSelection.Data = (byte)buttonData.ID;
            _currentSelection.Name = buttonData.Name;
        }

        /// <summary>
        /// Raise event if needed
        /// </summary>
        /// <param name="args"></param>
        private void RaiseEventIfRequired(AltControlEventArgs args)
        {
            EEventReason reason = (args != null) ? args.EventReason : EEventReason.None;
            switch (reason)
            {
                case EEventReason.Pressed:
                    if (Pressed != null)
                    {
                        Pressed(this, args);
                    }
                    break;
                case EEventReason.Released:
                    if (Released != null)
                    {
                        Released(this, args);
                    }
                    break;
                case EEventReason.Inside:
                    if (Inside != null)
                    {
                        Inside(this, args);
                    } 
                    break;
                case EEventReason.Outside:
                    if (Outside != null)
                    {
                        Outside(this, args);
                    } 
                    break;
                case EEventReason.None:
                    if (SelectionChanged != null)
                    {
                        SelectionChanged(this, args);
                    }
                    break;
            }
        }

        /// <summary>
        /// Highlight the selected button
        /// </summary>
        /// <param name="button"></param>
        private void HighlightSelectedButton(Button button)
        {
            if (_selectedButton != null)
            {
                _selectedButton.Foreground = _defaultForegroundBrush;
                _selectedButton.FontWeight = _defaultFontWeight;
                _selectedButton.BorderBrush = _defaultBorderBrush;
                _selectedButton.BorderThickness = _defaultBorderThickness;
            }
            _selectedButton = button;
            if (button != null)
            {
                button.Foreground = _selectedBrush;
                button.FontWeight = FontWeights.Bold;
                button.BorderBrush = _selectedBrush;
                button.BorderThickness = new Thickness(2);
                button.Focus();
            }
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

        /// <summary>
        /// Ensure that the tab order of the buttons is correct
        /// </summary>
        private void RefreshTabOrder()
        {
            // Calculate a value for buttons to allow them to be sorted into tab order (horiz then vert)
            Button[] orderedButtons = new Button[_source.CustomButtons.Count];
            double[] tabOrderVal = new double[_source.CustomButtons.Count];
            int i = 0;
            foreach (Button button in CustomWindowCanvas.Children)
            {
                CustomButtonData buttonData = (CustomButtonData)button.Tag;
                orderedButtons[i] = button;
                tabOrderVal[i] = (_source.WindowWidth / _tabOrderGranularity) * (buttonData.Y + buttonData.Height / 2) / _tabOrderGranularity + buttonData.X / _tabOrderGranularity;
                i++;
            }

            // Sort
            Array.Sort(tabOrderVal, orderedButtons);

            // Set tab order
            i = 0;
            foreach (Button button in orderedButtons)
            {
                button.TabIndex = i;
                i++;
            }
        }

        /// <summary>
        /// Redisplay all buttons
        /// </summary>
        public void RefreshLayout()
        {
            if (_isLoaded)
            {
                // Size the control
                CustomWindowCanvas.Width = _source.WindowWidth;
                CustomWindowCanvas.Height = _source.WindowHeight;

                foreach (Button button in CustomWindowCanvas.Children)
                {
                    CustomButtonData buttonData = (CustomButtonData)button.Tag;

                    // Validate the button data
                    buttonData.X = Math.Max(0.0, Math.Min(_source.WindowWidth - buttonData.Width, buttonData.X));
                    buttonData.Width = Math.Max(Constants.MinCustomButtonSize, Math.Min(_source.WindowWidth - buttonData.X, buttonData.Width));
                    buttonData.Y = Math.Max(0.0, Math.Min(_source.WindowHeight - buttonData.Height, buttonData.Y));
                    buttonData.Height = Math.Max(Constants.MinCustomButtonSize, Math.Min(_source.WindowHeight - buttonData.Y, buttonData.Height));

                    // Redisplay the button
                    RefreshButtonLayout(button);
                }
            }
        }

        /// <summary>
        /// Get the data for the selected button
        /// </summary>
        /// <returns></returns>
        public CustomButtonData GetSelectedButtonData()
        {
            CustomButtonData buttonData = null;
            if (_selectedButton != null)
            {
                buttonData = (CustomButtonData)_selectedButton.Tag;
            }

            return buttonData;
        }

        /// <summary>
        /// Redisplay the selected button (size, position, text)
        /// </summary>
        public void RefreshSelectedButtonLayout()
        {
            if (_isLoaded && _selectedButton != null)
            {
                RefreshButtonLayout(_selectedButton);
            }
        }

        /// <summary>
        /// Refresh the selected button's background
        /// </summary>
        public void RefreshSelectedButtonBackground()
        {
            if (_isLoaded && _selectedButton != null)
            {
                RefreshButtonBackground(_selectedButton);
            }
        }        

        /// <summary>
        /// Refresh a button
        /// </summary>
        /// <param name="button"></param>
        private void RefreshButtonLayout(Button button)
        {
            CustomButtonData buttonData = (CustomButtonData)button.Tag;

            button.Content = buttonData.Text;
            button.Width = buttonData.Width;
            button.Height = buttonData.Height;
            Canvas.SetLeft(button, buttonData.X);
            Canvas.SetTop(button, buttonData.Y);
        }

        /// <summary>
        /// Refresh a button's background
        /// </summary>
        /// <param name="button"></param>
        private void RefreshButtonBackground(Button button)
        {
            if (EnableColours)
            {
                CustomButtonData buttonData = (CustomButtonData)button.Tag;
                Brush brush;
                string defaultProfilesDir = System.IO.Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                string profilesDir = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);
                ImageBrush imageBrush = GUIUtils.CreateImageBrush(buttonData.BackgroundImage, profilesDir);
                if (imageBrush != null)
                {
                    imageBrush.Stretch = Stretch.Fill;
                    imageBrush.Opacity = Math.Max(0.0, Math.Min(1.0, 1.0 - buttonData.BackgroundTranslucency));
                    brush = imageBrush;
                }
                else
                {
                    byte alpha = (byte)(0xFF & (int)(255.0 * (1.0 - buttonData.BackgroundTranslucency)));
                    brush = GUIUtils.GetBrushFromColour(buttonData.BackgroundColour, alpha, Brushes.LightGray);
                }
                button.Background = brush;
            }
        }

        /// <summary>
        /// Refresh the background of the control
        /// </summary>
        public void RefreshBackgroundColours()
        {
            if (_isLoaded && EnableColours)
            {
                // Refresh window background
                if (!_isDesignMode)
                {
                    // Not design mode - translucent background, or no background if ghost mode
                    byte alpha = (byte)(0xFF & (int)(255.0 * (1.0 - _source.Translucency)));
                    this.CustomWindowCanvas.Background = 
                        _source.GhostBackground ? null : GUIUtils.GetBrushFromColour(_source.BackgroundColour, alpha, Brushes.White);
                }
                else
                {
                    // No translucency in design mode, force white background if ghost mode
                    this.CustomWindowCanvas.Background = 
                        _source.GhostBackground ? Brushes.White : GUIUtils.GetBrushFromColour(_source.BackgroundColour, 0xFF, Brushes.White);
                }

                // Refresh button backgrounds
                foreach (Button button in CustomWindowCanvas.Children)
                {
                    RefreshButtonBackground(button);
                }
            }
        }

    }
}
