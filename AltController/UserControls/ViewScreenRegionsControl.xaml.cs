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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AltController.Config;
using AltController.Core;

namespace AltController.UserControls
{
    /// <summary>
    /// Control for displaying screen regions
    /// </summary>
    public partial class ViewScreenRegionsControl : UserControl
    {
        // Config
        private AppConfig _appConfig;
        private const double _defaultStrokeThickness = 2;
        private const double _selectedStrokeThickness = 4;
        private bool _isLoaded;
        private ScreenRegionList _regionsList;
        private bool _isDesignMode;
        private bool _canSelectRegions;
        private bool _allowMultiSelect;
        private bool _showBackground;
        private bool _showRegionNames;

        // State
        private Brush _defaultBackgroundBrush;
        private List<ScreenRegion> _selectedRegions = new List<ScreenRegion>();
        private double _canvasWidth = 0.0;
        private double _canvasHeight = 0.0;
        private bool _lockYAxis;
        private bool _currentlyDrawing;
        private Point _startPoint;
        private Rectangle _regionOutlineRect;
        private Path _dragControl;
        private bool _dragStarted;
        private Point _dragStartPoint;
        private LogicalState _currentState = new LogicalState();

        // Events
        public event EventHandler SelectedRegionChanged;
        public event EventHandler FinishedEditing;

        // Properties
        public bool IsDesignMode { get { return _isDesignMode; } set { _isDesignMode = value; } }
        public bool CanSelectRegions { get { return _canSelectRegions; } set { _canSelectRegions = value; } }
        public bool AllowMultiSelect { get { return _allowMultiSelect; } set { _allowMultiSelect = value; } }
        public bool ShowBackground { get { return _showBackground; } set { _showBackground = value; } }
        public bool ShowRegionNames 
        { 
            get { return _showRegionNames; } 
            set 
            { 
                _showRegionNames = value; 
                RefreshDisplay(); 
            } 
        }
        public LogicalState CurrentState
        {
            get { return _currentState; } 
            set 
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    RefreshDisplay();
                }
            } 
        }
        public List<ScreenRegion> SelectedRegions { get { return _selectedRegions; } }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ViewScreenRegionsControl()
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
        /// Set the source which contains the regions
        /// </summary>
        /// <param name="source"></param>
        public void SetRegionsList(ScreenRegionList regionsList)
        {
            _regionsList = regionsList;
            RefreshDisplay();            
        }

        /// <summary>
        /// Set the selected screen region
        /// </summary>
        /// <param name="regions"></param>
        public void SetSelectedRegions(List<ScreenRegion> regions)
        {
            StopDrawingRegion();

            // Unhighlight current regions if any
            foreach (ScreenRegion region in _selectedRegions)
            {
                Path path = FindControlForRegion(region);
                if (path != null)
                {
                    path.StrokeThickness = _defaultStrokeThickness;
                    Canvas.SetZIndex(path, 0);
                }
            }
            _selectedRegions.Clear();

            // Store new selection(s)
            if (regions != null)
            {
                if (!_allowMultiSelect && regions.Count > 1)
                {
                    throw new Exception("Multiple selection of regions is not allowed.");
                }

                _selectedRegions.AddRange(regions);
            }

            // Highlight new regions
            foreach (ScreenRegion region in _selectedRegions)
            {
                Path path = FindControlForRegion(region);
                if (path != null)
                {
                    path.StrokeThickness = _selectedStrokeThickness;
                    Canvas.SetZIndex(path, 1);
                }
            }

            // Raise event
            if (SelectedRegionChanged != null)
            {
                SelectedRegionChanged(this, new EventArgs());
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

                _defaultBackgroundBrush = this.Background;

                RefreshBackground();
                RefreshDisplay();

                // Enable drag drop if required
                if (AllowDrop)
                {
                    RegionCanvas.Drop += RegionCanvas_Drop;
                }
            }
        }

        /// <summary>
        /// Handle change of canvas size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!double.IsNaN(e.NewSize.Width) && !double.IsNaN(e.NewSize.Height))
            {
                _canvasWidth = e.NewSize.Width;
                _canvasHeight = e.NewSize.Height;
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Set the background screenshot 
        /// </summary>
        public void RefreshBackground()
        {
            if (_showBackground)
            {
                Brush brush = null;
                if (_regionsList != null)
                {
                    string defaultProfilesDir = System.IO.Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                    string profilesDir = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);
                    ImageBrush imageBrush = GUIUtils.CreateImageBrush(_regionsList.RefImage, profilesDir);
                    if (imageBrush != null)
                    {
                        imageBrush.Stretch = Stretch.Fill;
                        imageBrush.AlignmentX = AlignmentX.Left;
                        imageBrush.AlignmentY = AlignmentY.Top;
                        brush = imageBrush;
                    }
                }

                if (brush == null)
                {
                    brush = _defaultBackgroundBrush;
                }

                this.RegionCanvas.Background = brush;
            }
        }

        /// <summary>
        /// Draw the screen regions
        /// </summary>
        public void RefreshDisplay()
        {
            if (_isLoaded)
            {
                // Clear canvas and drag rectangle 
                RegionCanvas.Children.Clear();
                _regionOutlineRect = null;
                StopDrawingRegion();

                // Check canvas size is known
                if (_regionsList != null && _canvasWidth > 0.0 && _canvasHeight > 0.0)
                {
                    // Draw regions
                    foreach (ScreenRegion region in _regionsList)
                    {
                        bool includeRegion = true;
                        if (!_isDesignMode)
                        {
                            includeRegion = region.ShowInState.Contains(_currentState);
                        }

                        if (includeRegion)
                        {
                            Path path = AddControlForRegion(region);
                            if (_selectedRegions.Contains(region))
                            {
                                path.StrokeThickness = _selectedStrokeThickness;   // Thicker border
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Refresh the selected regions' shape, size and position
        /// </summary>
        public void RefreshSelectedRegionGeometry()
        {
            StopDrawingRegion();

            foreach (ScreenRegion region in _selectedRegions)
            {
                Path path = FindControlForRegion(region);
                if (path != null)
                {
                    Rect rect = new Rect(_canvasWidth * region.Rectangle.Left,
                        _canvasHeight * region.Rectangle.Top,
                        _canvasWidth * region.Rectangle.Width,
                        _canvasHeight * region.Rectangle.Height);
                    path.Data = region.GetGeometry(rect);
                    Canvas.SetLeft(path, 0.0);
                    Canvas.SetTop(path, 0.0);
                }
            }
        }

        /// <summary>
        /// Update the opacity of the selected region's background image
        /// </summary>
        public void RefreshSelectedRegionOpacity()
        {
            foreach (ScreenRegion region in _selectedRegions)
            {
                Path path = FindControlForRegion(region);
                if (path != null)
                {
                    if (path.Fill is ImageBrush)
                    {
                        ((ImageBrush)path.Fill).Opacity = Math.Max(0.0, Math.Min(1.0, 1.0 - region.BackgroundTranslucency));
                    }
                }
            }
        }

        /// <summary>
        /// Update the selected region's border and fill
        /// </summary>
        public void RefreshSelectedRegionBorderAndFill()
        {
            foreach (ScreenRegion region in _selectedRegions)
            {
                Path path = FindControlForRegion(region);
                if (path != null)
                {
                    // Set stroke
                    path.Stroke = GUIUtils.GetBrushFromColour(region.Colour, 0xFF, Brushes.LightGray);

                    // Set background image or colour
                    string defaultProfilesDir = System.IO.Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
                    string profilesDir = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);
                    ImageBrush imageBrush = GUIUtils.CreateImageBrush(region.BackgroundImage, profilesDir);
                    if (imageBrush != null)
                    {
                        imageBrush.Stretch = Stretch.Fill;
                        imageBrush.Opacity = Math.Max(0.0, Math.Min(1.0, 1.0 - region.BackgroundTranslucency));
                        path.Fill = imageBrush;
                    }
                    else
                    {
                        path.Fill = GUIUtils.GetBrushFromColour(region.Colour, 0x40, Brushes.LightGray);
                    }
                }
            }
        }

        /// <summary>
        /// Add a rectangle control to the canvas for the specified region
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private Path AddControlForRegion(ScreenRegion region)
        {
            Rect rect = new Rect(_canvasWidth * region.Rectangle.Left,
                _canvasHeight * region.Rectangle.Top,
                _canvasWidth * region.Rectangle.Width,
                _canvasHeight * region.Rectangle.Height);

            // Set shape, size and position
            Path path = new Path();
            path.Data = region.GetGeometry(rect);
            path.Tag = region;

            // Set border
            path.Stroke = GUIUtils.GetBrushFromColour(region.Colour, 0xFF, Brushes.LightGray);
            path.StrokeThickness = _defaultStrokeThickness;

            // Set background image or colour
            string defaultProfilesDir = System.IO.Path.Combine(AppConfig.UserDataDir, Constants.ProfilesFolderName);
            string profilesDir = _appConfig.GetStringVal(Constants.ConfigProfilesDir, defaultProfilesDir);
            ImageBrush imageBrush = GUIUtils.CreateImageBrush(region.BackgroundImage, profilesDir);
            if (imageBrush != null)
            {
                imageBrush.Stretch = Stretch.Fill;
                imageBrush.Opacity = Math.Max(0.0, Math.Min(1.0, 1.0 - region.BackgroundTranslucency));
                path.Fill = imageBrush;
            }
            else if (_isDesignMode)
            {
                // Set a coloured background in design mode so that the region is clickable
                path.Fill = GUIUtils.GetBrushFromColour(region.Colour, 0x40, Brushes.LightGray);
            }            
            
            // Set cursor
            if (_isDesignMode)
            {
                path.Cursor = Cursors.Hand;
            }

            // Show selected region on top
            bool isSelected = _selectedRegions.Contains(region);
            Canvas.SetZIndex(path, isSelected ? 1 : 0);

            // Enable click selection if required
            if (_canSelectRegions)
            {
                path.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(ShapeControl_PreviewMouseLeftButtonDown);
                path.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(ShapeControl_PreviewMouseLeftButtonUp);
            }

            // Add to canvas
            Canvas.SetLeft(path, 0.0);
            Canvas.SetTop(path, 0.0);
            RegionCanvas.Children.Add(path);

            // Draw region names if required            
            if (_showRegionNames)
            {
                Point textPos = region.GetTextPosition();

                TextBlock textBlock = new TextBlock();
                textBlock.Text = region.Name;
                textBlock.TextAlignment = TextAlignment.Center;
                textBlock.TextTrimming = TextTrimming.None;
                textBlock.TextWrapping = TextWrapping.NoWrap;
                textBlock.FontSize = 20;
                textBlock.Foreground = GUIUtils.GetBrushFromColour(region.Colour, 0xFF, Brushes.LightGray);
                RegionCanvas.Children.Add(textBlock);
                Canvas.SetLeft(textBlock, _canvasWidth * (region.Rectangle.Left + region.Rectangle.Width * textPos.X) - 30.0);
                Canvas.SetTop(textBlock, _canvasHeight * (region.Rectangle.Top + region.Rectangle.Height * textPos.Y) - 10.0);
            }

            return path;
        }

        /// <summary>
        /// Find the shape control for a region
        /// </summary>
        /// <param name="regionToMatch"></param>
        /// <returns></returns>
        private Path FindControlForRegion(ScreenRegion regionToMatch)
        {
            Path path = null;
            foreach (FrameworkElement control in RegionCanvas.Children)
            {
                ScreenRegion region = control.Tag as ScreenRegion;
                if (region == regionToMatch)
                {
                    path = control as Path;
                    break;
                }
            }

            return path;
        }

        /// <summary>
        /// Mouse down on region
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShapeControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (AllowDrop && !_currentlyDrawing)
            {
                Path path = sender as Path;
                ScreenRegion region = path.Tag as ScreenRegion;
                if (!_allowMultiSelect || !Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (!_selectedRegions.Contains(region))
                    {
                        SetSelectedRegions(new List<ScreenRegion> { region });
                    }

                    _dragControl = path;
                    _dragStartPoint = e.GetPosition(RegionCanvas);
                    _dragStarted = false;
                    RegionCanvas.PreviewMouseMove += RegionCanvas_PreviewMouseMove;
                }
            }
        }

        private void ShapeControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_currentlyDrawing && !_dragStarted)
            {
                Path path = sender as Path;
                ScreenRegion region = path.Tag as ScreenRegion;
                if (_allowMultiSelect && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    // Ctrl+clicked region
                    bool alreadySelected = _selectedRegions.Contains(region);

                    // Duplicate the selections list
                    List<ScreenRegion> newSelections = new List<ScreenRegion>();
                    newSelections.AddRange(_selectedRegions);

                    if (alreadySelected)
                    {
                        newSelections.Remove(region);
                    }
                    else
                    {
                        newSelections.Add(region);
                    }
                    SetSelectedRegions(newSelections);
                }
                else if(!_selectedRegions.Contains(region))
                {
                    // Single selection
                    SetSelectedRegions(new List<ScreenRegion> { region });
                }
            }
        }

        /// <summary>
        /// Start drawing a screen region using click and drag
        /// </summary>
        public void StartDrawingRegion(bool lockYAxis)
        {
            if (!_currentlyDrawing)
            {
                // Set default cursor for regions
                foreach (FrameworkElement control in RegionCanvas.Children)
                {
                    if (control is Path)
                    {
                        control.Cursor = Cursors.Arrow;
                    }
                }

                _lockYAxis = lockYAxis;
                _currentlyDrawing = true;
                RegionCanvas.PreviewMouseLeftButtonDown += this.RegionCanvas_PreviewMouseLeftButtonDown;
                RegionCanvas.PreviewMouseLeftButtonUp += this.RegionCanvas_PreviewMouseLeftButtonUp;
                RegionCanvas.PreviewMouseMove += this.RegionCanvas_PreviewMouseMove;
            }
        }

        /// <summary>
        /// Stop drawing a screen region
        /// </summary>
        public void StopDrawingRegion()
        {
            if (_currentlyDrawing)
            {
                if (_regionOutlineRect != null)
                {
                    RegionCanvas.Children.Remove(_regionOutlineRect);
                    _regionOutlineRect = null;
                }

                // Set cursor for regions
                foreach (FrameworkElement control in RegionCanvas.Children)
                {
                    if (control is Path)
                    {
                        control.Cursor = Cursors.Hand;
                    }
                }
                _currentlyDrawing = false;
                RegionCanvas.PreviewMouseLeftButtonDown -= this.RegionCanvas_PreviewMouseLeftButtonDown;
                RegionCanvas.PreviewMouseLeftButtonUp -= this.RegionCanvas_PreviewMouseLeftButtonUp;
                RegionCanvas.PreviewMouseMove -= this.RegionCanvas_PreviewMouseMove;

                // Raise event
                if (FinishedEditing != null)
                {
                    FinishedEditing(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Left button down - start drawing rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only allow drawing when one region is selected
            if (_currentlyDrawing && _selectedRegions.Count == 1)
            {
                _startPoint = e.GetPosition(RegionCanvas);

                // Create drag rectangle
                if (_regionOutlineRect == null)
                {
                    _regionOutlineRect = new Rectangle();
                    _regionOutlineRect.Name = "Drag";
                    _regionOutlineRect.Stroke = GUIUtils.GetBrushFromColour(_selectedRegions[0].Colour, 0xFF, Brushes.LightGray);
                    _regionOutlineRect.StrokeThickness = _defaultStrokeThickness;
                    _regionOutlineRect.Width = 0;
                    _regionOutlineRect.Height = 0;
                    RegionCanvas.Children.Add(_regionOutlineRect);
                }
                Canvas.SetLeft(_regionOutlineRect, _startPoint.X);
                Canvas.SetTop(_regionOutlineRect, _startPoint.Y);
            }
        }

        /// <summary>
        /// Left button up - stop drawing rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentlyDrawing)
            {
                if (_selectedRegions.Count == 1 && _canvasWidth > 0.0 && _canvasHeight > 0.0)
                {
                    Point endPoint = e.GetPosition(RegionCanvas);

                    // Update current region
                    double left = Math.Min(_startPoint.X, endPoint.X) / _canvasWidth;
                    double top = Math.Min(_startPoint.Y, endPoint.Y) / _canvasHeight;
                    double right = Math.Max(_startPoint.X, endPoint.X) / _canvasWidth;
                    double bottom;
                    if (_lockYAxis)
                    {
                        bottom = top + Math.Abs(endPoint.X - _startPoint.X) / _canvasHeight;
                    }
                    else
                    {
                        bottom = Math.Max(_startPoint.Y, endPoint.Y) / _canvasHeight;
                    }                    
                    left = Math.Max(0.0, Math.Min(1.0, left));
                    right = Math.Max(0.0, Math.Min(1.0, right));
                    top = Math.Max(0.0, Math.Min(1.0, top));
                    bottom = Math.Max(0.0, Math.Min(1.0, bottom));
                    if (right > left && bottom > top)
                    {
                        _selectedRegions[0].Rectangle = new Rect(left, top, right - left, bottom - top);
                    }
                }

                // Redisplay
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Mouse moved in canvas area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_currentlyDrawing && _regionOutlineRect != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point mousePoint = e.GetPosition(RegionCanvas);

                    double left = Math.Min(_startPoint.X, mousePoint.X);
                    double top = Math.Min(_startPoint.Y, mousePoint.Y);
                    double right = Math.Max(_startPoint.X, mousePoint.X);
                    double bottom = Math.Max(_startPoint.Y, mousePoint.Y);
                    _regionOutlineRect.Width = right - left;
                    if (_lockYAxis)
                    {
                        _regionOutlineRect.Height = right - left;
                    }
                    else 
                    {
                        _regionOutlineRect.Height = bottom - top;
                    }
                    Canvas.SetLeft(_regionOutlineRect, left);
                    Canvas.SetTop(_regionOutlineRect, top);
                }
                else
                {
                    StopDrawingRegion();
                }
            }
            else if (_dragControl != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (!_dragStarted)
                    {
                        Point mousePos = e.GetPosition(RegionCanvas);
                        Vector diff = mousePos - _dragStartPoint;

                        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                        {
                            ScreenRegion region = (ScreenRegion)_dragControl.Tag;

                            // Perform drag & drop operation
                            _dragStarted = true;
                            DataObject dragData = new DataObject(typeof(ScreenRegion).FullName, region);
                            try
                            {
                                RegionCanvas.DragOver += new DragEventHandler(RegionCanvas_DragOver);
                                DragDrop.DoDragDrop(_dragControl, dragData, DragDropEffects.Move);
                            }
                            finally
                            {
                                RegionCanvas.DragOver -= RegionCanvas_DragOver;
                            }
                        }
                    }
                }
                else
                {
                    _dragControl = null;
                    _dragStarted = false;
                    RegionCanvas.PreviewMouseMove -= RegionCanvas_PreviewMouseMove;

                    // Reposition region as mouse button released outside canvas area
                    RefreshSelectedRegionGeometry();
                }
            }
        }

        /// <summary>
        /// Move button during drag drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionCanvas_DragOver(object sender, DragEventArgs e)
        {
            if (_dragControl != null)
            {
                foreach (ScreenRegion region in _selectedRegions)
                {
                    Path path = FindControlForRegion(region);
                    if (path != null)
                    {
                        Point mousePos = e.GetPosition(RegionCanvas);
                        Vector diff = mousePos - _dragStartPoint;

                        Canvas.SetLeft(path, diff.X);
                        Canvas.SetTop(path, diff.Y);
                    }
                }
                e.Handled = true;
            }
        }


        /// <summary>
        /// Handle drag drop (only in design mode)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionCanvas_Drop(object sender, DragEventArgs e)
        {
            if ((e.Effects & DragDropEffects.Move) != 0 &&
                e.Data.GetDataPresent(typeof(ScreenRegion).FullName))
            {
                Point mousePos = e.GetPosition(RegionCanvas);
                Vector diff = mousePos - _dragStartPoint;

                // Update the region data on completion of drag drop
                foreach (ScreenRegion region in _selectedRegions)
                {
                    double left = region.Rectangle.Left + diff.X / _canvasWidth;
                    double top = region.Rectangle.Top + diff.Y / _canvasHeight;

                    // Check region is in view
                    if (left > -region.Rectangle.Width && left < 1.0 &&
                        top > -region.Rectangle.Height && top < 1.0)
                    {
                        region.Rectangle = new Rect(left, top, region.Rectangle.Width, region.Rectangle.Height);
                    }
                }

                // Mark drag as complete
                _dragStarted = false;
                _dragControl = null;
                RegionCanvas.PreviewMouseMove -= RegionCanvas_PreviewMouseMove;

                // Refresh region
                RefreshSelectedRegionGeometry();

                // Raise event
                if (FinishedEditing != null)
                {
                    FinishedEditing(this, new EventArgs());
                }
                e.Handled = true;
            }
        }
    }
}
