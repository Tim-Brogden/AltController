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
using System.Windows.Shapes;
using AltController.Core;
using AltController.Config;
using AltController.Event;

namespace AltController
{
    /// <summary>
    /// Invisible window that superimposes screen regions over the desktop / working area / active window
    /// </summary>
    public partial class OverlayWindow : Window
    {
        // Config
        private bool _isLoaded;
        private IParentWindow _parentWindow;
        private AppConfig _appConfig;
        private Profile _profile;
        
        private bool _drawScreenRegions = Constants.DefaultDrawScreenRegions;
        private bool _drawPointerIndicator = Constants.DefaultDrawPointerIndicatorLine;
        private bool _drawStateOverlay = Constants.DefaultDrawStateOverlay;

        private int _pointerIndicatorStyle = Constants.DefaultPointerIndicatorStyle;
        private string _pointerIndicatorColourName = Constants.DefaultPointerIndicatorColour;
        private int _pointerIndicatorRadius = Constants.DefaultPointerIndicatorRadius;
        private int _pointerIndicatorLineThickness = Constants.DefaultPointerIndicatorLineThickness;

        private string _stateOverlayColourName = Constants.DefaultStateOverlayTextColour;
        private string _stateOverlayBgColourName = Constants.DefaultStateOverlayBgColour;
        private double _stateOverlayTranslucency = Constants.DefaultStateOverlayTranslucency;
        private double _stateOverlayFontSize = Constants.DefaultStateOverlayFontSize;
        private double _stateOverlayXPos = Constants.DefaultStateOverlayXPos;
        private double _stateOverlayYPos = Constants.DefaultStateOverlayYPos;

        // State
        private EDisplayArea _overlayPosition = Constants.DefaultOverlayArea;
        private Size _overlayWindowSize = new Size();
        private Size _primaryScreenSize = new Size();
        private Point _centrePoint = new Point(0.0, 0.0);
        private Rect _activeWindowRect = new Rect();
        private Ellipse _pointerCircle;
        private Line _radialLine;
        private LogicalState _logicalState = new LogicalState();
        private TextBlock _stateTextBlock;

        // Properties
        public Profile CurrentProfile
        {
            set
            {
                _profile = value;
                _logicalState = new LogicalState();
                RegionsControl.SetRegionsList(_profile.ScreenRegions);
                if (_isLoaded)
                {
                    InitialiseDisplaySettings();
                    RefreshCurrentState();
                }
            }
        }
        public bool DrawScreenRegions
        {
            set
            {
                _drawScreenRegions = value;
                RegionsControl.Visibility = _drawScreenRegions ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public bool ShowScreenRegionNames
        {
            set
            {
                RegionsControl.ShowRegionNames = value;
            }
        }
        public bool DrawPointerIndicator
        {
            set
            {
                _drawPointerIndicator = value;
                if (!value)
                {
                    ClearPointerIndicator();
                }
            }
        }
        public bool DrawStateOverlay
        {
            set
            {
                _drawStateOverlay = value;
                if (_isLoaded)
                {
                    RefreshCurrentState();
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OverlayWindow(IParentWindow parent)
        {
            _parentWindow = parent;
            _appConfig = parent.GetAppConfig();
            InitializeComponent();

            RegionsControl.SetAppConfig(_appConfig);
        }

        /// <summary>
        /// Set the application configuration
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;

            // Pointer indicator config
            ClearPointerIndicator();
            _pointerIndicatorStyle = _appConfig.GetIntVal(Constants.ConfigPointerIndicatorStyle, Constants.DefaultPointerIndicatorStyle);
            _pointerIndicatorColourName = _appConfig.GetStringVal(Constants.ConfigPointerIndicatorColour, Constants.DefaultPointerIndicatorColour);
            _pointerIndicatorRadius = _appConfig.GetIntVal(Constants.ConfigPointerIndicatorRadius, Constants.DefaultPointerIndicatorRadius);
            _pointerIndicatorLineThickness = Math.Max(1, _appConfig.GetIntVal(Constants.ConfigPointerIndicatorLineThickness, Constants.DefaultPointerIndicatorLineThickness));

            // Mode / page overlay
            ClearStateOverlay();
            _stateOverlayColourName = _appConfig.GetStringVal(Constants.ConfigStateOverlayTextColour, Constants.DefaultStateOverlayTextColour);
            _stateOverlayBgColourName = _appConfig.GetStringVal(Constants.ConfigStateOverlayBgColour, Constants.DefaultStateOverlayBgColour);
            _stateOverlayTranslucency = _appConfig.GetDoubleVal(Constants.ConfigStateOverlayTranslucency, Constants.DefaultStateOverlayTranslucency);
            _stateOverlayFontSize = Math.Max(1.0, _appConfig.GetDoubleVal(Constants.ConfigStateOverlayFontSize, Constants.DefaultStateOverlayFontSize));
            _stateOverlayXPos = _appConfig.GetDoubleVal(Constants.ConfigStateOverlayXPos, Constants.DefaultStateOverlayXPos);
            _stateOverlayYPos = _appConfig.GetDoubleVal(Constants.ConfigStateOverlayYPos, Constants.DefaultStateOverlayYPos);
            _stateOverlayTranslucency = Math.Max(0.0, Math.Min(1.0, _stateOverlayTranslucency));
            _stateOverlayXPos = Math.Max(0.0, Math.Min(1.0, _stateOverlayXPos));
            _stateOverlayYPos = Math.Max(0.0, Math.Min(1.0, _stateOverlayYPos));
            if (_isLoaded && _drawStateOverlay)
            {
                ShowCurrentState();
            }

            // Controls
            RegionsControl.SetAppConfig(appConfig);
        }

        /// <summary>
        /// Initialise display settings
        /// </summary>
        public void InitialiseDisplaySettings()
        {
            _overlayPosition = _profile.ScreenRegions.OverlayPosition;

            // Store the primary screen size
            _primaryScreenSize = new Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);

            // Store the centre point for the radial line
            _centrePoint.X = SystemParameters.FullPrimaryScreenWidth * 0.5;
            _centrePoint.Y = SystemParameters.FullPrimaryScreenHeight * 0.5;

            // Size the window to cover the screen / working area / desktop
            this.Left = 0;
            this.Top = 0;
            _overlayWindowSize = GUIUtils.GetDisplayAreaSize(_overlayPosition);
            this.Width = _overlayWindowSize.Width;
            this.Height = _overlayWindowSize.Height;
            this.Topmost = true;

            SizeRegionsControl();
        }

        // These methods prevent the window from being activated on clicking
        /*
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WindowsAPI.WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(WindowsAPI.MA_NOACTIVATE);
            }
            else
            {
                return IntPtr.Zero;
            }
        }*/

        /// <summary>
        /// Window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;

            // Show or hide controls
            RegionsControl.Visibility = _drawScreenRegions ? Visibility.Visible : Visibility.Hidden;

            if (_profile != null)
            {
                // Initialise display
                InitialiseDisplaySettings();
            }

            _parentWindow.AttachEventReportHandler(HandleEventReport);
        }

        /// <summary>
        /// Draw an overlay to show where the pointer is
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void ShowPointerIndicator(double x, double y)
        {
            if (_pointerIndicatorStyle == Constants.PointerCircle)
            {
                DrawPointerCircle(x, y);
            }
            else
            {
                DrawPointerLine(x, y);
            }
        }

        /// <summary>
        /// Clear the pointer overlay
        /// </summary>
        private void ClearPointerIndicator()
        {
            if (_pointerIndicatorStyle == Constants.PointerCircle)
            {
                ClearPointerCircle();
            }
            else
            {
                ClearPointerLine();
            }
        }

        /// <summary>
        /// Draw a circle to show the current mouse position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void DrawPointerCircle(double x, double y)
        {
            if (_pointerCircle == null)
            {
                // Create crosshair
                _pointerCircle = new Ellipse();
                _pointerCircle.Width = 2 * _pointerIndicatorRadius;
                _pointerCircle.Height = 2 * _pointerIndicatorRadius;
                _pointerCircle.Stroke = GetBrushFromColour(_pointerIndicatorColourName, Brushes.LightGray, 1.0);
                _pointerCircle.StrokeThickness = _pointerIndicatorLineThickness;
                ControlsCanvas.Children.Add(_pointerCircle);
            }

            // Offset by one pixel so that the line isn't clickable
            Canvas.SetLeft(_pointerCircle, x - _pointerIndicatorRadius);
            Canvas.SetTop(_pointerCircle, y - _pointerIndicatorRadius);
        }

        /// <summary>
        /// Clear the pointer circle overlay
        /// </summary>
        private void ClearPointerCircle()
        {
            if (_pointerCircle != null)
            {
                ControlsCanvas.Children.Remove(_pointerCircle);
                _pointerCircle = null;
            }
        }

        /// <summary>
        /// Draw a radial line to show the current mouse position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void DrawPointerLine(double x, double y)
        {
            if (_radialLine == null)
            {
                // Create radial line with zero length
                _radialLine = new Line();
                _radialLine.Stroke = GetBrushFromColour(_pointerIndicatorColourName, Brushes.LightGray, 1.0);
                _radialLine.StrokeThickness = _pointerIndicatorLineThickness;
                _radialLine.X1 = _centrePoint.X;
                _radialLine.Y1 = _centrePoint.Y;
                _radialLine.X2 = _centrePoint.X;
                _radialLine.Y2 = _centrePoint.Y;
                ControlsCanvas.Children.Add(_radialLine);
            }

            // Make sure the line doesn't quite reach the cursor position (so it's not clickable)
            x += (x < _centrePoint.X) ? 5 : -5;
            y += (y < _centrePoint.Y) ? 5 : -5;

            _radialLine.X2 = x;
            _radialLine.Y2 = y;            
        }        

        /// <summary>
        /// Clear the radial line
        /// </summary>
        private void ClearPointerLine()
        {
            if (_radialLine != null)
            {
                ControlsCanvas.Children.Remove(_radialLine);
                _radialLine = null;
            }
        }

        /// <summary>
        /// Handle change of active app or size / position of its window
        /// </summary>
        /// <param name="report"></param>
        private void HandleEventReport(EventReport report)
        {
            if (report.EventType == EEventType.WindowRegionEvent)
            {
                ClearPointerIndicator();

                // Try to ensure that the overlay window stays on top
                this.Topmost = false;
                this.Topmost = true;

                WindowRegionEventArgs args = (WindowRegionEventArgs)report.Args;
                _activeWindowRect = args.Rect;
                if (_overlayPosition == EDisplayArea.ActiveWindow)
                {
                    SizeRegionsControl();
                }
            }            
            else if (report.EventType == EEventType.Control)
            {
                AltControlEventArgs args = (AltControlEventArgs)report.Args;
                if (args.ControlType == EControlType.MousePointer && _drawPointerIndicator)
                {
                    double x = (double)args.GetParamValue(0).Value;
                    double y = (double)args.GetParamValue(1).Value;
                    
                    ShowPointerIndicator(x, y);
                }
            }
            else if (report.EventType == EEventType.StateChange)
            {
                AltStateChangeEventArgs args = (AltStateChangeEventArgs)report.Args;
                _logicalState = args.LogicalState;
                RegionsControl.CurrentState = _logicalState;
                if (_drawStateOverlay)
                {
                    ShowCurrentState();
                }
            }
        }

        /// <summary>
        /// Position and size the regions control in the overlay window
        /// </summary>
        private void SizeRegionsControl()
        {
            if (_overlayPosition == EDisplayArea.ActiveWindow)
            {
                Canvas.SetLeft(RegionsControl, _activeWindowRect.X);
                Canvas.SetTop(RegionsControl, _activeWindowRect.Y);
                RegionsControl.Width = _activeWindowRect.Width;
                RegionsControl.Height = _activeWindowRect.Height;
            }
            else
            {
                Canvas.SetLeft(RegionsControl, 0.0);
                Canvas.SetTop(RegionsControl, 0.0);
                RegionsControl.Width = _overlayWindowSize.Width;
                RegionsControl.Height = _overlayWindowSize.Height;
            }
        }

        /// <summary>
        /// Refresh the mode / page overlay
        /// </summary>
        private void RefreshCurrentState()
        {
            if (_drawStateOverlay)
            {
                ShowCurrentState();
            }
            else
            {
                ClearStateOverlay();
            }
        }

        /// <summary>
        /// Show the current mode and page in a textblock overlay
        /// </summary>
        /// <param name="logicalState"></param>
        private void ShowCurrentState()
        {
            // State labels
            string text = "";
            if (_logicalState.ModeID != Constants.DefaultID)
            {
                NamedItem modeDetails = _profile.GetModeDetails(_logicalState.ModeID);
                if (modeDetails != null)
                {
                    text = modeDetails.Name;
                }
            }
            if (_logicalState.PageID != Constants.DefaultID)
            {
                NamedItem pageDetails = _profile.GetPageDetails(_logicalState.PageID);
                if (pageDetails != null)
                {
                    if (text != "")
                    {
                        text += " - ";
                    }
                    text += pageDetails.Name;
                }
            }

            if (_stateTextBlock == null)
            {
                _stateTextBlock = new TextBlock();
                double xPadding = 10.0;
                double yPadding = 5.0;
                _stateTextBlock.Background = GetBrushFromColour(_stateOverlayBgColourName, Brushes.White, 1.0 - _stateOverlayTranslucency);
                _stateTextBlock.Foreground = GetBrushFromColour(_stateOverlayColourName, Brushes.Black, 1.0);
                _stateTextBlock.FontSize = _stateOverlayFontSize;
                _stateTextBlock.Padding = new Thickness(xPadding, yPadding, xPadding, yPadding);
                ControlsCanvas.Children.Add(_stateTextBlock);
                if (_stateOverlayXPos <= 0.5)
                {
                    Canvas.SetLeft(_stateTextBlock, _stateOverlayXPos * _primaryScreenSize.Width);
                }
                else
                {
                    Canvas.SetRight(_stateTextBlock, (1.0 - _stateOverlayXPos) * _primaryScreenSize.Width);
                }
                if (_stateOverlayYPos >= 0.5)
                {
                    Canvas.SetBottom(_stateTextBlock, (1.0 - _stateOverlayYPos) * _primaryScreenSize.Height);
                }
                else
                {
                    Canvas.SetTop(_stateTextBlock, _stateOverlayYPos * _primaryScreenSize.Height);
                }
            }
            _stateTextBlock.Text = text;
            _stateTextBlock.Visibility = text != "" ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Remove the state overlay
        /// </summary>
        private void ClearStateOverlay()
        {
            if (_stateTextBlock != null)
            {
                ControlsCanvas.Children.Remove(_stateTextBlock);
                _stateTextBlock = null;
            }
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Stop receiving events
            _parentWindow.DetachEventReportHandler(HandleEventReport);

            // Tell the parent window that we're closing
            _parentWindow.ChildWindowClosing(this);
        }

        /// <summary>
        /// Get a coloured brush
        /// </summary>
        /// <param name="colourName"></param>
        /// <param name="defaultBrush"></param>
        /// <param name="opacity"></param>
        /// <returns></returns>
        private SolidColorBrush GetBrushFromColour(string colourName, SolidColorBrush defaultBrush, double opacity)
        {
            SolidColorBrush brush;
            try
            {
                Color colour = (Color)ColorConverter.ConvertFromString(colourName);
                brush = new SolidColorBrush(colour);
                brush.Opacity = opacity;
            }
            catch
            {
                brush = defaultBrush;
            }

            return brush;
        }
    }
}
