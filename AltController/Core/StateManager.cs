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
using System.Diagnostics;
using System.Windows;
using System.Threading;
using AltController.Sys;
using AltController.Config;
using AltController.Event;
using AltController.Input;

namespace AltController.Core
{
    /// <summary>
    /// Monitors the input sources and maps them to actions
    /// </summary>
    public class StateManager : IStateManager
    {
        // Parent
        private ThreadManager _parent;

        // Configuration
        private int _pollingIntervalMS = Constants.DefaultInputPollingIntervalMS;
        private Profile _profile = new Profile();
        private AppConfig _appConfig = new AppConfig();
        private bool _isDiagnosticsEnabled = false;

        // State
        private LogicalState _lastUsedLogicalState = new LogicalState();
        private LogicalState _logicalState = new LogicalState();
        private Rect _currentWindowRect = new Rect();
        private Rect _overlayWindowRect = new Rect();
        private KeyPressManager _keyStateManager;
        private MouseManager _mouseStateManager;
        private ActionMappingTable _currentActionSet = new ActionMappingTable();
        private List<ActionList> _ongoingActionLists = new List<ActionList>();
        private double _dpiX = 1.0;
        private double _dpiY = 1.0;
        private bool _autoStopPressActions = Constants.DefaultAutoStopPressActions;
        private bool _autoStopInsideActions = Constants.DefaultAutoStopInsideActions;

        // Internal inputs
        private InternalSource _internalInputSource = new InternalSource();

        // Properties
        public Profile CurrentProfile { get { return _profile; } }
        public AppConfig AppConfig { get { return _appConfig; } }
        public KeyPressManager KeyStateManager { get { return _keyStateManager; } }
        public MouseManager MouseStateManager { get { return _mouseStateManager; } }
        public Rect CurrentWindowRect { get { return _currentWindowRect; } }
        public Rect OverlayWindowRect { get { return _overlayWindowRect; } }
        public double DPI_X { get { return _dpiX; } }
        public double DPI_Y { get { return _dpiY; } }
        public bool IsDiagnosticsEnabled { get { return _isDiagnosticsEnabled; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public StateManager(ThreadManager parent)
        {
            _parent = parent;
            _keyStateManager = new KeyPressManager(this);
            _mouseStateManager = new MouseManager(this);
        }

        /// <summary>
        /// Apply new app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            // Store
            _appConfig = appConfig;

            // Update DPI settings
            _dpiX = appConfig.GetDoubleVal(Constants.ConfigDPIXSetting, 1.0);
            _dpiY = appConfig.GetDoubleVal(Constants.ConfigDPIYSetting, 1.0);

            // Update state manager
            int pollingIntervalMS = appConfig.GetIntVal(Constants.ConfigInputPollingIntervalMS, Constants.DefaultInputPollingIntervalMS);
            if (pollingIntervalMS >= Constants.MinInputPollingIntervalMS)
            {
                _pollingIntervalMS = pollingIntervalMS;
            }
            _autoStopPressActions = appConfig.GetBoolVal(Constants.ConfigAutoStopPressActions, Constants.DefaultAutoStopPressActions);
            _autoStopInsideActions = appConfig.GetBoolVal(Constants.ConfigAutoStopInsideActions, Constants.DefaultAutoStopInsideActions);

            // Update key press manager
            _keyStateManager.UseScanCodes = appConfig.GetBoolVal(Constants.ConfigUseScanCodes, Constants.DefaultUseScanCodes);

            // Update inputs
            _internalInputSource.SetAppConfig(_appConfig);
            foreach (BaseSource inputSource in _profile.InputSources)
            {
                inputSource.SetAppConfig(_appConfig);
            }
        }

        /// <summary>
        /// Apply a new profile
        /// </summary>
        /// <param name="profile"></param>
        public void SetProfile(Profile profile)
        {
            // Reset state
            Reset();
            _logicalState = new LogicalState();
            _currentActionSet = new ActionMappingTable();

            // Disconnect inputs
            foreach (BaseSource inputSource in _profile.InputSources)
            {
                inputSource.Connect(HandleEvent, false);
            }

            // Replace existing profile
            _profile = profile;

            // Store overlay window size
            _overlayWindowRect = new Rect(GUIUtils.GetDisplayAreaSize(_profile.ScreenRegions.OverlayPosition));

            // Configure input sources
            foreach (BaseSource inputSource in _profile.InputSources)
            {
                inputSource.SetAppConfig(_appConfig);
                inputSource.Connect(HandleEvent, true);
            }

            // Initialise the set of actions to perform
            UpdateCurrentActionSet();
        }

        /// <summary>
        /// Enable or disable diagnostics
        /// </summary>
        /// <param name="enable"></param>
        public void ConfigureDiagnostics(bool enable)
        {
            _isDiagnosticsEnabled = enable;
        }

        /// <summary>
        /// Update the state of the input sources
        /// </summary>
        public void Run()
        {
            StartRun();

            while (_parent.ContinuePolling())
            {
                // Check for configuration updates
                _parent.ReceiveConfigUpdates(this);

                // Continue any existing actions
                ContinueOngoingTasks();

                // Handle any events submitted from other threads
                foreach (AltControlEventArgs args in _parent.GetNewEventSubmissions())
                {
                    BaseSource inputSource = _profile.GetInputSource(args.SourceID);
                    if (inputSource != null)
                    {
                        inputSource.ReceiveExternalEvent(args);
                    }
                }

                // Update internal source to check for change of application focus
                _internalInputSource.UpdateState(this);

                // Update sources to check for new events to raise
                foreach (BaseSource source in _profile.InputSources)
                {
                    source.UpdateState(this);
                }

                Thread.Sleep(_pollingIntervalMS);
            }

            // Clean up before exiting
            EndRun();
        }

        /// <summary>
        /// Initialise system before running
        /// </summary>
        private void StartRun()
        {
            _keyStateManager.Initialise();
            _mouseStateManager.Initialise();
        }

        /// <summary>
        /// Restore neutral system state before exiting
        /// </summary>
        //protected override void EndRun()
        private void EndRun()
        {
            Reset();

            _keyStateManager.Destroy();
            _mouseStateManager.Destroy();
        }

        /// <summary>
        /// Handle an event triggered by a source and perform the appropriate actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="eventTypeID"></param>
        public void HandleEvent(object sender, AltControlEventArgs args)
        {
            // Perform actions required
            ActionList actionList = _currentActionSet.GetActions(args.ToID());
            if (actionList != null && actionList.IsActive)
            {
                // Start the action list
                bool ongoing = actionList.IsOngoing;
                actionList.Start(this, args);

                // Add to ongoing actions if required
                if (!ongoing && actionList.IsOngoing)
                {
                    _ongoingActionLists.Add(actionList);
                }
            }

            // Auto stop corresponding press / inside events
            AutoStopOppositeActions(args);
        }

        /// <summary>
        /// Auto stop corresponding press / inside events when key / button is released or on leaving region
        /// </summary>
        private void AutoStopOppositeActions(AltControlEventArgs args)
        {
            // Get the opposite event
            EEventReason oppositeReason = EEventReason.None;
            if (args.EventReason == EEventReason.Released && _autoStopPressActions)
            {
                oppositeReason = EEventReason.Pressed;
            }
            else if (args.EventReason == EEventReason.Outside && _autoStopInsideActions)
            {
                oppositeReason = EEventReason.Inside;
            }

            if (oppositeReason != EEventReason.None)
            {
                // Stop actions for opposite event
                AltControlEventArgs stopArgs = new AltControlEventArgs(args);
                stopArgs.EventReason = oppositeReason;
                ActionList actionList = _currentActionSet.GetActions(stopArgs.ToID());
                if (actionList != null && actionList.IsOngoing)
                {
                    actionList.Stop(this);
                }
            }
        }

        /// <summary>
        /// Report an event to the UI
        /// </summary>
        /// <param name="report"></param>
        public void ReportEvent(EventReport report)
        {
            _parent.ScheduleEventReport(report);
        }

        /// <summary>
        /// Continue tasks which persist over more than one update cycle
        /// </summary>
        private void ContinueOngoingTasks()
        {
            // Actions
            int i = 0;
            while (i < _ongoingActionLists.Count)
            {
                ActionList actionList = _ongoingActionLists[i];
                if (actionList.IsActive)
                {
                    actionList.Continue(this);
                }

                // Prepare to remove completed or inactive actions from the ongoing list
                if ((!actionList.IsActive || !actionList.IsOngoing) && i < _ongoingActionLists.Count)
                {
                    _ongoingActionLists.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// Stop any ongoing actions and release all keys and mouse buttons
        /// </summary>
        public void Reset()
        {
            // Clear all ongoing actions
            foreach (ActionList actionList in _ongoingActionLists)
            {
                actionList.Stop(this);
            }
            _ongoingActionLists.Clear();

            // Reset the keyboard state
            _keyStateManager.ReleaseAllKeys();

            // Reset the mouse state
            _mouseStateManager.ReleaseAllButtons();
        }

        /// <summary>
        /// Change the current mode
        /// </summary>
        /// <param name="modeID"></param>
        public void SetMode(long modeID)
        {
            // Calculate new state
            LogicalState newState = new LogicalState(_logicalState);
            switch (modeID)
            {
                case Constants.LastUsedID:
                    newState.ModeID = _lastUsedLogicalState.ModeID;
                    break;
                case Constants.NextID:
                    newState.ModeID = _profile.ModeDetails.FindNextID(newState.ModeID);
                    break;
                case Constants.PreviousID:
                    newState.ModeID = _profile.ModeDetails.FindPreviousID(newState.ModeID);
                    break;
                default:
                    newState.ModeID = modeID;
                    break;
            }

            SetLogicalState(newState);
        }

        /// <summary>
        /// Change the current app
        /// </summary>
        /// <param name="appID"></param>
        public void SetApp(string appName)
        {
            // Calculate new state
            LogicalState newState = new LogicalState(_logicalState);
            NamedItem appDetails = _profile.GetAppDetails(appName);
            if (appDetails != null)
            {
                // This app is in the profile
                newState.AppID = appDetails.ID;
            }
            else 
            {
                // Current app not included in the profile so change to default app
                newState.AppID = Constants.DefaultID;
            }           

            SetLogicalState(newState);
        }

        /// <summary>
        /// Change the current page
        /// </summary>
        /// <param name="pageID"></param>
        public void SetPage(long pageID)
        {
            // Calculate new state
            LogicalState newState = new LogicalState(_logicalState);
            switch (pageID)
            {
                case Constants.LastUsedID:
                    newState.PageID = _lastUsedLogicalState.PageID;
                    break;
                case Constants.NextID:
                    newState.PageID = _profile.PageDetails.FindNextID(newState.PageID);
                    break;
                case Constants.PreviousID:
                    newState.PageID = _profile.PageDetails.FindPreviousID(newState.PageID);
                    break;
                default:
                    newState.PageID = pageID;
                    break;
            }

            SetLogicalState(newState);
        }

        /// <summary>
        /// Store the current window's display area
        /// </summary>
        /// <param name="rectangle"></param>
        public void SetCurrentWindowRect(RECT rectangle)
        {
            Rect rect = new Rect(new Point(rectangle.Left / _dpiX, rectangle.Top / _dpiY),
                                new Point(rectangle.Right / _dpiX, rectangle.Bottom / _dpiY));
            if (!_currentWindowRect.Equals(rect))
            {
                _currentWindowRect = rect;

                // Report change
                Rect rectCopy = new Rect(rect.TopLeft, rect.BottomRight);
                WindowRegionEventArgs args = new WindowRegionEventArgs(rectCopy);
                EventReport report = new EventReport(DateTime.Now, EEventType.WindowRegionEvent, args);
                _parent.ScheduleEventReport(report);
            }
        }

        /// <summary>
        /// Update the current logical state of the application
        /// </summary>
        /// <param name="newState"></param>
        private void SetLogicalState(LogicalState newState)
        {
            // Update last used logical state
            bool changed = false;
            if (newState.ModeID != _logicalState.ModeID)
            {
                _lastUsedLogicalState.ModeID = _logicalState.ModeID;
                changed = true;
            }
            if (newState.AppID != _logicalState.AppID)
            {
                _lastUsedLogicalState.AppID = _logicalState.AppID;
                changed = true;
            }
            if (newState.PageID != _logicalState.PageID)
            {
                _lastUsedLogicalState.PageID = _logicalState.PageID;
                changed = true;
            }

            if (changed)
            {
                // Set new state
                _logicalState = newState;

                // Recalculate current action set
                UpdateCurrentActionSet();

                // Report state change if required
                AltStateChangeEventArgs args = new AltStateChangeEventArgs(new LogicalState(newState));
                EventReport report = new EventReport(DateTime.Now, EEventType.StateChange, args);
                _parent.ScheduleEventReport(report);
            }
        }

        /// <summary>
        /// Update the current set of actions according to the logical state
        /// </summary>
        private void UpdateCurrentActionSet()
        {
            // Determine new actions
            ActionMappingTable newActionSet = _profile.GetActionsForState(_logicalState, true);

            // Deactivate old actions
            Dictionary<long, ActionList>.Enumerator eOld = _currentActionSet.GetEnumerator();
            while (eOld.MoveNext())
            {
                ActionList actionList = eOld.Current.Value;
                if (newActionSet.GetActions(eOld.Current.Key) == null)
                {
                    actionList.IsActive = false;
                    BaseSource eventSource = _profile.GetInputSource(actionList.EventArgs.SourceID);
                    if (eventSource != null)
                    {
                        eventSource.ConfigureEventMonitoring(actionList.EventArgs, false);
                    }
                }
            }

            // Stop ongoing actions that have been deactivated
            int i = 0;
            while (i < _ongoingActionLists.Count)
            {
                ActionList actionList = _ongoingActionLists[i];
                if (!actionList.IsActive)
                {
                    actionList.Stop(this);
                    _ongoingActionLists.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            // Activate new actions
            Dictionary<long, ActionList>.Enumerator eNew = newActionSet.GetEnumerator();
            while (eNew.MoveNext())
            {
                ActionList actionList = eNew.Current.Value;
                if (!actionList.IsActive)
                {
                    actionList.IsActive = true;
                    BaseSource eventSource = _profile.GetInputSource(actionList.EventArgs.SourceID);
                    if (eventSource != null)
                    {
                        eventSource.ConfigureEventMonitoring(actionList.EventArgs, true);
                    }
                }
            }

            _currentActionSet = newActionSet;
        }

    }
}
