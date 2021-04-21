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
using System.Windows.Input;
using System.Windows.Interop;
using AltController.Core;
using AltController.Event;
using AltController.Input;
using AltController.Sys;

namespace AltController
{
    /// <summary>
    /// Window containing custom buttons that can perform actions
    /// </summary>
    public partial class CustomWindow : Window
    {
        // Members
        private bool _isLoaded = false;
        private CustomWindowSource _customWindowSource;
        private IParentWindow _parentWindow;

        // Properties
        public CustomWindowSource CustomWindowSource 
        { 
            get { return _customWindowSource; } 
            set { _customWindowSource = value; UpdateDisplay(); } 
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomWindow(IParentWindow parent)
        {
            _parentWindow = parent;

            InitializeComponent();

            this.CustomControl.SetAppConfig(parent.GetAppConfig());

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close,
               new ExecutedRoutedEventHandler(delegate(object sender, ExecutedRoutedEventArgs args) { this.Close(); })));
            this.CustomControl.Pressed += new AltControlEventHandler(CustomControl_ButtonEvent);
            this.CustomControl.Released += new AltControlEventHandler(CustomControl_ButtonEvent);
            this.CustomControl.Inside += new AltControlEventHandler(CustomControl_ButtonEvent);
            this.CustomControl.Outside += new AltControlEventHandler(CustomControl_ButtonEvent);
        }

        /// <summary>
        /// Window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            UpdateDisplay();
        }

        /// <summary>
        /// Disable window activation if required
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            // Set the window style to noactivate
            WindowInteropHelper helper = new WindowInteropHelper(this);
            uint gwlExStyle = WindowsAPI.GetWindowLong(helper.Handle, WindowsAPI.GWL_EXSTYLE);
            WindowsAPI.SetWindowLong(helper.Handle, WindowsAPI.GWL_EXSTYLE, gwlExStyle | WindowsAPI.WS_EX_NOACTIVATE);
        }

        /// <summary>
        /// Drag window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Enable window activation during dragging
            WindowInteropHelper helper = new WindowInteropHelper(this);
            uint gwlExStyle = WindowsAPI.GetWindowLong(helper.Handle, WindowsAPI.GWL_EXSTYLE);
            WindowsAPI.SetWindowLong(helper.Handle, WindowsAPI.GWL_EXSTYLE, gwlExStyle & (0xFFFFFFFF^WindowsAPI.WS_EX_NOACTIVATE));

            // Activate the window
            Activate();

            // Move the window
            DragMove();

            // Disable window activation again
            gwlExStyle = WindowsAPI.GetWindowLong(helper.Handle, WindowsAPI.GWL_EXSTYLE); 
            WindowsAPI.SetWindowLong(helper.Handle, WindowsAPI.GWL_EXSTYLE, gwlExStyle | WindowsAPI.WS_EX_NOACTIVATE);
        }

        /// <summary>
        /// Update the display
        /// </summary>
        private void UpdateDisplay()
        {
            if (_isLoaded && _customWindowSource != null)
            {
                this.CustomControl.SetSource(_customWindowSource);
                this.Topmost = _customWindowSource.TopMost;
                this.TitleTextBlock.Text = _customWindowSource.WindowTitle;
            }
        }

        /// <summary>
        /// Handle custom button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CustomControl_ButtonEvent(object sender, AltControlEventArgs args)
        {
            // Send event to parent
            _parentWindow.SubmitEvent(args);
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Tell parent when window is closing
            _parentWindow.ChildWindowClosing(this);
        }        
    }
}
