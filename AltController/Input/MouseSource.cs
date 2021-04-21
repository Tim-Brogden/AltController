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
using AltController.Config;
using AltController.Core;
using AltController.Event;
using AltController.Controls;
//using System.Diagnostics;

namespace AltController.Input
{
    /// <summary>
    /// Represents a mouse, or equivalent pointing device
    /// </summary>
    public class MouseSource : BaseSource
    {
        // Members
        private PointerControl _pointerControl;
        private MouseButtonSet _buttonSetControl;
        private bool _isActive;
        private Rect _virtualScreenArea = new Rect(0.0, 0.0, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
        private Point _screenCoords = new Point();
        private Point _windowCoords = new Point();
        private bool[] _isInRegion = new bool[256];
        private long _lastEventReportTime = 0L;
        private long _uiUpdateIntervalTicks = Constants.DefaultUIUpdateIntervalMS * TimeSpan.TicksPerMillisecond;

        // Properties
        public override ESourceType SourceType { get { return ESourceType.Mouse; } }
        public PointerControl Pointer { get { return _pointerControl; } }
        public Point WindowCoords { get { return _windowCoords; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public MouseSource()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public MouseSource(long id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public MouseSource(MouseSource source)
            : base(source)
        {
        }

        /// <summary>
        /// Set app config
        /// </summary>
        /// <param name="appConfig"></param>
        public override void SetAppConfig(AppConfig appConfig)
        {
            _virtualScreenArea = new Rect(0.0, 0.0, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            _uiUpdateIntervalTicks = appConfig.GetIntVal(Constants.ConfigUIUpdateIntervalMS, Constants.DefaultUIUpdateIntervalMS) * TimeSpan.TicksPerMillisecond;
            base.SetAppConfig(appConfig);
        }

        /// <summary>
        /// Initialise controls
        /// </summary>
        protected override void InitialiseControls()
        {
            _pointerControl = new PointerControl(this);
            _buttonSetControl = new MouseButtonSet(this);
            InputControls = new BaseControl[] { _pointerControl, _buttonSetControl };
        }

        /// <summary>
        /// Get the types of event that this source supports for the specified control
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override List<EEventReason> GetSupportedEventReasons(AltControlEventArgs args)
        {
            List<EEventReason> supportedReasons = null;
            switch (args.ControlType)
            {
                case EControlType.MouseButtons:
                    supportedReasons = _buttonSetControl.SupportedEventReasons;
                    break;
                case EControlType.MousePointer:
                    // Mouse pointer actions require a screen region (specified in the Data parameter)
                    if (args.Data != 0)
                    {
                        supportedReasons = _pointerControl.SupportedEventReasons;
                    }
                    else
                    {
                        supportedReasons = null;
                    }
                    break;
            }

            return supportedReasons;
        }

        /// <summary>
        /// Enable or disable monitoring for a type of event
        /// </summary>
        /// <param name="args"></param>
        public override void ConfigureEventMonitoring(AltControlEventArgs args, bool enable)
        {
            // Update whether the source is active or not            
            if (args.ControlType == EControlType.MousePointer)
            {
                _pointerControl.ConfigureEventMonitoring(args, enable);
            }
            _isActive = _buttonSetControl.IsActive | _pointerControl.IsActive;
        }

        /// <summary>
        /// Return whether the mouse pointer is in the specified region
        /// </summary>
        /// <param name="regionID"></param>
        /// <returns></returns>
        public bool IsPointerInRegion(byte regionID)
        {
            return _isInRegion[regionID];
        }

        /// <summary>
        /// Update the state of the pointer
        /// </summary>
        /// <param name="stateManager"></param>
        public override void UpdateState(IStateManager stateManager)
        {
            long currentTicks = DateTime.Now.Ticks;
            bool updateUI = currentTicks - _lastEventReportTime > _uiUpdateIntervalTicks;
            if (_isActive || updateUI)
            {
                // Store screen position
                _screenCoords = new Point(System.Windows.Forms.Cursor.Position.X / stateManager.DPI_X, System.Windows.Forms.Cursor.Position.Y / stateManager.DPI_Y);

                if (_isActive)
                {
                    // Store window coords
                    ScreenRegionList regionsList = Profile.ScreenRegions;
                    Rect windowRect = regionsList.OverlayPosition == EDisplayArea.ActiveWindow ? stateManager.CurrentWindowRect : stateManager.OverlayWindowRect;
                    _windowCoords = ScreenToNormalisedWindowPosition(_screenCoords, windowRect);

                    // Store which screen regions the pointer is in                
                    foreach (ScreenRegion region in regionsList)
                    {
                        byte regionID = (byte)region.ID;
                        _isInRegion[regionID] = region.Contains(_windowCoords);
                    }

                    // Update controls
                    foreach (BaseControl control in InputControls)
                    {
                        control.UpdateState(stateManager);
                    }
                }

                // Report controller events that the UI needs
                if (updateUI)
                {
                    AltControlEventArgs args = new AltControlEventArgs();
                    args.SourceID = ID;
                    args.ControlType = EControlType.MousePointer;
                    args.EventReason = EEventReason.Updated;
                    args.Param0.DataType = EDataType.Double;
                    args.Param0.Value = _screenCoords.X;
                    args.Param1.DataType = EDataType.Double;
                    args.Param1.Value = _screenCoords.Y;
                    EventReport report = new EventReport(DateTime.Now, EEventType.Control, args);
                    stateManager.ReportEvent(report);

                    _lastEventReportTime = currentTicks;
                }
            }
        }

        /// <summary>
        /// Convert screen co-ords to normalised window co-ords
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="windowRect"></param>
        /// <returns></returns>
        private Point ScreenToNormalisedWindowPosition(Point screenCoords, Rect windowRect)
        {
            Point point = new Point();

            if (windowRect.Width > 0.0)
            {
                point.X = (screenCoords.X - windowRect.Left) / windowRect.Width;
            }

            if (windowRect.Height > 0.0)
            {
                point.Y = (screenCoords.Y - windowRect.Top) / windowRect.Height;
            }

            return point;
        }
    }
}
