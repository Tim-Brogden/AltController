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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AltController.Core;
using AltController.Event;
using AltController.Input;

namespace AltController.UserControls
{
    /// <summary>
    /// Interaction logic for ViewKeyboardControl.xaml
    /// </summary>
    public partial class ViewKeyboardControl : UserControl, IInputViewer
    {
        // Members
        private bool _isLoaded;
        private static VirtualKeyData[] _keysByKeyCode;
        private static Dictionary<ushort, VirtualKeyData> _charKeysByScanCode;
        private Dictionary<System.Windows.Forms.Keys, Button> _keyToButtonMapping;
        private KeyboardSource _source;
        private AltControlEventArgs _currentSelection;
        private Button _selectedButton;
        private Dictionary<Button, HighlightInfo> _buttonHighlighting = new Dictionary<Button, HighlightInfo>();
        private Brush _defaultForegroundBrush;
        private FontWeight _defaultFontWeight;
        private Brush _defaultBorderBrush;
        private Thickness _defaultBorderThickness;
        private Brush _selectedBrush;
        private Brush _defaultsBrush;
        private Brush _configuredBrush;
        private Brush _noHighlightBrush;

        // Events
        public event AltControlEventHandler SelectionChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewKeyboardControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set which source to display
        /// </summary>
        /// <param name="source"></param>
        public void SetSource(BaseSource source)
        {
            _source = source as KeyboardSource;
            if (_currentSelection != null)
            {
                _currentSelection.SourceID = _source.ID;
                RaiseSelectionChangedIfRequired(_currentSelection);
            }
        }

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                _selectedBrush = Brushes.Red;
                _defaultsBrush = new LinearGradientBrush(Colors.LightGray, Colors.Gray, 90.0);
                _configuredBrush = new LinearGradientBrush(Colors.LightBlue, Colors.SteelBlue, 90.0);
                _noHighlightBrush = this.A.Background;
                _defaultForegroundBrush = this.A.Foreground;
                _defaultFontWeight = this.A.FontWeight;
                _defaultBorderBrush = this.A.BorderBrush;
                _defaultBorderThickness = this.A.BorderThickness;

                if (_keysByKeyCode == null)
                {
                    _keysByKeyCode = KeyUtils.GetVirtualKeysByKeyCode();
                    _charKeysByScanCode = KeyUtils.GetVirtualKeysByScanCode();
                }

                // Bind keys
                _keyToButtonMapping = CreateKeyToButtonMapping();
                BindButtons(_keyToButtonMapping);

                if (_source != null)
                {
                    if (_currentSelection == null)
                    {
                        // Select an initial key  
                        _currentSelection = new AltControlEventArgs(_source.ID, EControlType.Keyboard, ESide.None, 0, ELRUDState.None, EEventReason.None);
                        _currentSelection.Data = (byte)System.Windows.Forms.Keys.A;
                    }
                    SetSelectedControl(_currentSelection);
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
                System.Windows.Forms.Keys key = (System.Windows.Forms.Keys)args.Data;
                Button button = KeyToButton(key);
                if (button != null)
                {
                    SelectButton(button);
                }
            }
        }

        /// <summary>
        /// Tag and name the button controls with the appropriate key data
        /// </summary>
        /// <param name="buttonBindings"></param>
        private void BindButtons(Dictionary<System.Windows.Forms.Keys, Button> buttonBindings)
        {
            Dictionary<System.Windows.Forms.Keys, Button>.Enumerator eKey = buttonBindings.GetEnumerator();
            while (eKey.MoveNext())
            {
                System.Windows.Forms.Keys key = eKey.Current.Key;
                Button button = eKey.Current.Value;
                
                // Tag the button with the virtual key and set its name
                VirtualKeyData vk = _keysByKeyCode[(byte)key];
                button.Tag = vk;
                if (vk != null)
                {
                    button.Content = vk.TinyName;
                }
            }

            // Special case - enter key
            NumPadEnter.Tag = _keysByKeyCode[(byte)System.Windows.Forms.Keys.Enter];
        }

        /// <summary>
        /// Create the mapping between buttons and virtual keys, taking account of the user's keyboard layout
        /// </summary>
        private Dictionary<System.Windows.Forms.Keys, Button> CreateKeyToButtonMapping()
        {
            Dictionary<System.Windows.Forms.Keys, Button> buttonMapping = new Dictionary<System.Windows.Forms.Keys, Button>();

            // Get the default layout
            Dictionary<System.Windows.Forms.Keys, ushort> defaultCharScanCodes = GetDefaultCharScanCodes();
            Dictionary<System.Windows.Forms.Keys, Button> defaultButtonBindings = GetDefaultButtonBindings();

            // Loop through keys that have a button
            Dictionary<System.Windows.Forms.Keys, Button>.Enumerator eKey = defaultButtonBindings.GetEnumerator();
            while (eKey.MoveNext())
            {
                System.Windows.Forms.Keys keyCode = eKey.Current.Key;
                Button button = eKey.Current.Value;

                // See if it's a character key that depends upon keyboard layout
                if (defaultCharScanCodes.ContainsKey(keyCode))
                {
                    ushort scanCode = defaultCharScanCodes[keyCode];
                    if (scanCode != 0 && _charKeysByScanCode.ContainsKey(scanCode))
                    {
                        keyCode = _charKeysByScanCode[scanCode].KeyCode;
                    }
                }

                buttonMapping[keyCode] = button;
            }

            return buttonMapping;
        }

        /// <summary>
        /// Key clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            Button keyButton = (Button)sender;
            VirtualKeyData vk = ButtonToKey(keyButton);
            if (vk != null)
            {
                System.Windows.Forms.Keys key = vk.KeyCode;
                _currentSelection = new AltControlEventArgs(_source.ID,
                                                        EControlType.Keyboard,
                                                        ESide.None,
                                                        0,
                                                        ELRUDState.None,
                                                        EEventReason.None);
                _currentSelection.Data = (byte)key;
                _currentSelection.ExtraData = vk.WindowsScanCode;

                SelectButton((Button)sender);
                RaiseSelectionChangedIfRequired(_currentSelection);
            }
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
        public void HighlightEvent(AltControlEventArgs args, HighlightInfo highlightInfo)
        {
            if (_isLoaded)
            {
                Button button = KeyToButton((System.Windows.Forms.Keys)args.Data);
                if (button != null)
                {
                    _buttonHighlighting[button] = highlightInfo;
                    HighlightButton(button, highlightInfo);
                }
            }
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

        /// <summary>
        /// Find the button for a key
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private Button KeyToButton(System.Windows.Forms.Keys key)
        {
            Button button = null;
            if (_keyToButtonMapping.ContainsKey(key))
            {
                button = _keyToButtonMapping[key];
            }

            return button;
        }

        /// <summary>
        /// Get the key corresponding to a button in the UI
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        private VirtualKeyData ButtonToKey(Button button)
        {
            return (VirtualKeyData)button.Tag;
        }

        /// <summary>
        /// Get the default scan code for each character key
        /// </summary>
        /// <returns></returns>
        private Dictionary<System.Windows.Forms.Keys, ushort> GetDefaultCharScanCodes()
        {
            Dictionary<System.Windows.Forms.Keys, ushort> defaultCharScanCodes = new Dictionary<System.Windows.Forms.Keys, ushort>();
            defaultCharScanCodes[System.Windows.Forms.Keys.D0] = 11;
            defaultCharScanCodes[System.Windows.Forms.Keys.D1] = 2;
            defaultCharScanCodes[System.Windows.Forms.Keys.D2] = 3;
            defaultCharScanCodes[System.Windows.Forms.Keys.D3] = 4;
            defaultCharScanCodes[System.Windows.Forms.Keys.D4] = 5;
            defaultCharScanCodes[System.Windows.Forms.Keys.D5] = 6;
            defaultCharScanCodes[System.Windows.Forms.Keys.D6] = 7;
            defaultCharScanCodes[System.Windows.Forms.Keys.D7] = 8;
            defaultCharScanCodes[System.Windows.Forms.Keys.D8] = 9;
            defaultCharScanCodes[System.Windows.Forms.Keys.D9] = 10;
            defaultCharScanCodes[System.Windows.Forms.Keys.A] = 30;
            defaultCharScanCodes[System.Windows.Forms.Keys.B] = 48;
            defaultCharScanCodes[System.Windows.Forms.Keys.C] = 46;
            defaultCharScanCodes[System.Windows.Forms.Keys.D] = 32;
            defaultCharScanCodes[System.Windows.Forms.Keys.E] = 18;
            defaultCharScanCodes[System.Windows.Forms.Keys.F] = 33;
            defaultCharScanCodes[System.Windows.Forms.Keys.G] = 34;
            defaultCharScanCodes[System.Windows.Forms.Keys.H] = 35;
            defaultCharScanCodes[System.Windows.Forms.Keys.I] = 23;
            defaultCharScanCodes[System.Windows.Forms.Keys.J] = 36;
            defaultCharScanCodes[System.Windows.Forms.Keys.K] = 37;
            defaultCharScanCodes[System.Windows.Forms.Keys.L] = 38;
            defaultCharScanCodes[System.Windows.Forms.Keys.M] = 50;
            defaultCharScanCodes[System.Windows.Forms.Keys.N] = 49;
            defaultCharScanCodes[System.Windows.Forms.Keys.O] = 24;
            defaultCharScanCodes[System.Windows.Forms.Keys.P] = 25;
            defaultCharScanCodes[System.Windows.Forms.Keys.Q] = 16;
            defaultCharScanCodes[System.Windows.Forms.Keys.R] = 19;
            defaultCharScanCodes[System.Windows.Forms.Keys.S] = 31;
            defaultCharScanCodes[System.Windows.Forms.Keys.T] = 20;
            defaultCharScanCodes[System.Windows.Forms.Keys.U] = 22;
            defaultCharScanCodes[System.Windows.Forms.Keys.V] = 47;
            defaultCharScanCodes[System.Windows.Forms.Keys.W] = 17;
            defaultCharScanCodes[System.Windows.Forms.Keys.X] = 45;
            defaultCharScanCodes[System.Windows.Forms.Keys.Y] = 21;
            defaultCharScanCodes[System.Windows.Forms.Keys.Z] = 44;
            defaultCharScanCodes[System.Windows.Forms.Keys.Oem1] = 39;
            defaultCharScanCodes[System.Windows.Forms.Keys.Oemplus] = 13;
            defaultCharScanCodes[System.Windows.Forms.Keys.Oemcomma] = 51;
            defaultCharScanCodes[System.Windows.Forms.Keys.OemMinus] = 12;
            defaultCharScanCodes[System.Windows.Forms.Keys.OemPeriod] = 52;
            defaultCharScanCodes[System.Windows.Forms.Keys.OemQuestion] = 53;
            defaultCharScanCodes[System.Windows.Forms.Keys.Oemtilde] = 40;
            defaultCharScanCodes[System.Windows.Forms.Keys.OemOpenBrackets] = 26;
            defaultCharScanCodes[System.Windows.Forms.Keys.Oem5] = 86;
            defaultCharScanCodes[System.Windows.Forms.Keys.Oem6] = 27;
            defaultCharScanCodes[System.Windows.Forms.Keys.Oem7] = 43;
            defaultCharScanCodes[System.Windows.Forms.Keys.Oem8] = 41;

            return defaultCharScanCodes;
        }

        /// <summary>
        /// Get the default mapping between buttons and key codes
        /// </summary>
        /// <returns></returns>
        private Dictionary<System.Windows.Forms.Keys, Button> GetDefaultButtonBindings()
        {
            Dictionary<System.Windows.Forms.Keys, Button> defaultButtonBindings = new Dictionary<System.Windows.Forms.Keys, Button>();
            defaultButtonBindings[System.Windows.Forms.Keys.F1] = F1;
            defaultButtonBindings[System.Windows.Forms.Keys.F2] = F2;
            defaultButtonBindings[System.Windows.Forms.Keys.F3] = F3;
            defaultButtonBindings[System.Windows.Forms.Keys.F4] = F4;
            defaultButtonBindings[System.Windows.Forms.Keys.F5] = F5;
            defaultButtonBindings[System.Windows.Forms.Keys.F6] = F6;
            defaultButtonBindings[System.Windows.Forms.Keys.F7] = F7;
            defaultButtonBindings[System.Windows.Forms.Keys.F8] = F8;
            defaultButtonBindings[System.Windows.Forms.Keys.F9] = F9;
            defaultButtonBindings[System.Windows.Forms.Keys.F10] = F10;
            defaultButtonBindings[System.Windows.Forms.Keys.F11] = F11;
            defaultButtonBindings[System.Windows.Forms.Keys.F12] = F12;

            defaultButtonBindings[System.Windows.Forms.Keys.F13] = F13;
            defaultButtonBindings[System.Windows.Forms.Keys.F14] = F14;
            defaultButtonBindings[System.Windows.Forms.Keys.F15] = F15;
            defaultButtonBindings[System.Windows.Forms.Keys.F16] = F16;
            defaultButtonBindings[System.Windows.Forms.Keys.F17] = F17;
            defaultButtonBindings[System.Windows.Forms.Keys.F18] = F18;
            defaultButtonBindings[System.Windows.Forms.Keys.F19] = F19;
            defaultButtonBindings[System.Windows.Forms.Keys.F20] = F20;
            defaultButtonBindings[System.Windows.Forms.Keys.F21] = F21;
            defaultButtonBindings[System.Windows.Forms.Keys.F22] = F22;
            defaultButtonBindings[System.Windows.Forms.Keys.F23] = F23;
            defaultButtonBindings[System.Windows.Forms.Keys.F24] = F24;

            defaultButtonBindings[System.Windows.Forms.Keys.D0] = D0;
            defaultButtonBindings[System.Windows.Forms.Keys.D1] = D1;
            defaultButtonBindings[System.Windows.Forms.Keys.D2] = D2;
            defaultButtonBindings[System.Windows.Forms.Keys.D3] = D3;
            defaultButtonBindings[System.Windows.Forms.Keys.D4] = D4;
            defaultButtonBindings[System.Windows.Forms.Keys.D5] = D5;
            defaultButtonBindings[System.Windows.Forms.Keys.D6] = D6;
            defaultButtonBindings[System.Windows.Forms.Keys.D7] = D7;
            defaultButtonBindings[System.Windows.Forms.Keys.D8] = D8;
            defaultButtonBindings[System.Windows.Forms.Keys.D9] = D9;

            defaultButtonBindings[System.Windows.Forms.Keys.A] = A;
            defaultButtonBindings[System.Windows.Forms.Keys.B] = B;
            defaultButtonBindings[System.Windows.Forms.Keys.C] = C;
            defaultButtonBindings[System.Windows.Forms.Keys.D] = D;
            defaultButtonBindings[System.Windows.Forms.Keys.E] = E;
            defaultButtonBindings[System.Windows.Forms.Keys.F] = F;
            defaultButtonBindings[System.Windows.Forms.Keys.G] = G;
            defaultButtonBindings[System.Windows.Forms.Keys.H] = H;
            defaultButtonBindings[System.Windows.Forms.Keys.I] = I;
            defaultButtonBindings[System.Windows.Forms.Keys.J] = J;
            defaultButtonBindings[System.Windows.Forms.Keys.K] = K;
            defaultButtonBindings[System.Windows.Forms.Keys.L] = L;
            defaultButtonBindings[System.Windows.Forms.Keys.M] = M;
            defaultButtonBindings[System.Windows.Forms.Keys.N] = N;
            defaultButtonBindings[System.Windows.Forms.Keys.O] = O;
            defaultButtonBindings[System.Windows.Forms.Keys.P] = P;
            defaultButtonBindings[System.Windows.Forms.Keys.Q] = Q;
            defaultButtonBindings[System.Windows.Forms.Keys.R] = R;
            defaultButtonBindings[System.Windows.Forms.Keys.S] = S;
            defaultButtonBindings[System.Windows.Forms.Keys.T] = T;
            defaultButtonBindings[System.Windows.Forms.Keys.U] = U;
            defaultButtonBindings[System.Windows.Forms.Keys.V] = V;
            defaultButtonBindings[System.Windows.Forms.Keys.W] = W;
            defaultButtonBindings[System.Windows.Forms.Keys.X] = X;
            defaultButtonBindings[System.Windows.Forms.Keys.Y] = Y;
            defaultButtonBindings[System.Windows.Forms.Keys.Z] = Z;

            defaultButtonBindings[System.Windows.Forms.Keys.Oem1] = SemiColon;
            defaultButtonBindings[System.Windows.Forms.Keys.Oemplus] = EqualsSign;
            defaultButtonBindings[System.Windows.Forms.Keys.Oemcomma] = Comma;
            defaultButtonBindings[System.Windows.Forms.Keys.OemMinus] = MinusSign;
            defaultButtonBindings[System.Windows.Forms.Keys.OemPeriod] = Fullstop;
            defaultButtonBindings[System.Windows.Forms.Keys.OemQuestion] = Slash;
            defaultButtonBindings[System.Windows.Forms.Keys.Oemtilde] = Apostrophe;
            defaultButtonBindings[System.Windows.Forms.Keys.OemOpenBrackets] = LeftBracket;
            defaultButtonBindings[System.Windows.Forms.Keys.Oem5] = Backslash;
            defaultButtonBindings[System.Windows.Forms.Keys.Oem6] = RightBracket;
            defaultButtonBindings[System.Windows.Forms.Keys.Oem7] = Hash;
            defaultButtonBindings[System.Windows.Forms.Keys.Oem8] = BackTick;

            defaultButtonBindings[System.Windows.Forms.Keys.Escape] = Escape;
            defaultButtonBindings[System.Windows.Forms.Keys.Back] = Back;
            defaultButtonBindings[System.Windows.Forms.Keys.Tab] = Tab;
            defaultButtonBindings[System.Windows.Forms.Keys.Enter] = Enter;
            defaultButtonBindings[System.Windows.Forms.Keys.CapsLock] = CapsLock;
            defaultButtonBindings[System.Windows.Forms.Keys.LShiftKey] = LShiftKey;
            defaultButtonBindings[System.Windows.Forms.Keys.RShiftKey] = RShiftKey;
            defaultButtonBindings[System.Windows.Forms.Keys.LControlKey] = LControlKey;
            defaultButtonBindings[System.Windows.Forms.Keys.RControlKey] = RControlKey;
            defaultButtonBindings[System.Windows.Forms.Keys.LMenu] = LMenu;
            defaultButtonBindings[System.Windows.Forms.Keys.RMenu] = RMenu;
            defaultButtonBindings[System.Windows.Forms.Keys.LWin] = LWin;
            defaultButtonBindings[System.Windows.Forms.Keys.RWin] = RWin;
            defaultButtonBindings[System.Windows.Forms.Keys.Space] = Space;
            defaultButtonBindings[System.Windows.Forms.Keys.Apps] = Apps;

            defaultButtonBindings[System.Windows.Forms.Keys.PrintScreen] = PrintScreen;
            defaultButtonBindings[System.Windows.Forms.Keys.Scroll] = Scroll;
            defaultButtonBindings[System.Windows.Forms.Keys.Pause] = Pause;

            defaultButtonBindings[System.Windows.Forms.Keys.Insert] = Insert;
            defaultButtonBindings[System.Windows.Forms.Keys.Delete] = Delete;
            defaultButtonBindings[System.Windows.Forms.Keys.Home] = Home;
            defaultButtonBindings[System.Windows.Forms.Keys.End] = End;
            defaultButtonBindings[System.Windows.Forms.Keys.PageUp] = PageUp;
            defaultButtonBindings[System.Windows.Forms.Keys.PageDown] = PageDown;

            defaultButtonBindings[System.Windows.Forms.Keys.Left] = Left;
            defaultButtonBindings[System.Windows.Forms.Keys.Right] = Right;
            defaultButtonBindings[System.Windows.Forms.Keys.Up] = Up;
            defaultButtonBindings[System.Windows.Forms.Keys.Down] = Down;

            defaultButtonBindings[System.Windows.Forms.Keys.NumLock] = NumLock;
            defaultButtonBindings[System.Windows.Forms.Keys.NumPad0] = NumPad0;
            defaultButtonBindings[System.Windows.Forms.Keys.NumPad1] = NumPad1;
            defaultButtonBindings[System.Windows.Forms.Keys.NumPad2] = NumPad2;
            defaultButtonBindings[System.Windows.Forms.Keys.NumPad3] = NumPad3;
            defaultButtonBindings[System.Windows.Forms.Keys.NumPad4] = NumPad4;
            defaultButtonBindings[System.Windows.Forms.Keys.NumPad5] = NumPad5;
            defaultButtonBindings[System.Windows.Forms.Keys.NumPad6] = NumPad6;
            defaultButtonBindings[System.Windows.Forms.Keys.NumPad7] = NumPad7;
            defaultButtonBindings[System.Windows.Forms.Keys.NumPad8] = NumPad8;
            defaultButtonBindings[System.Windows.Forms.Keys.NumPad9] = NumPad9;
            defaultButtonBindings[System.Windows.Forms.Keys.Add] = NumPadAdd;
            defaultButtonBindings[System.Windows.Forms.Keys.Subtract] = NumPadSubtract;
            defaultButtonBindings[System.Windows.Forms.Keys.Multiply] = NumPadMultiply;
            defaultButtonBindings[System.Windows.Forms.Keys.Divide] = NumPadDivide;
            defaultButtonBindings[System.Windows.Forms.Keys.Decimal] = NumPadDecimal;

            return defaultButtonBindings;
        }

    }
}
