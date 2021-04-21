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
using System.Threading;
using System.Globalization;
using AltController.Config;
using AltController.Event;

namespace AltController.Core
{
    /// <summary>
    /// Manages the thread for polling input sources
    /// </summary>
    public class ThreadManager
    {
        // State
        private StateManager _stateManager;

        // Inter-thread comms
        private const int _maxEventsInBuffer = 1000;
        private List<EventReport> _pendingEventReports;
        private List<EventReport> _reportedEvents;
        private List<AltControlEventArgs> _pendingEventSubmissions;
        private List<AltControlEventArgs> _submittedEvents;
        private Profile _pendingProfile = new Profile();
        private AppConfig _pendingAppConfig = new AppConfig();
        private bool _configChanged = false;
        private bool _eventsReported = false;
        private bool _eventsSubmitted = false;
        private object _mutex = new object();
        
        // Threading
        private static Thread _stateManagerThread;
        private delegate void UpdateCallback();
        private bool _continue = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public ThreadManager()
        {
            // Set up inter-thread comms
            InitialiseBuffers();
        }

        /// <summary>
        /// Apply a new profile
        /// </summary>
        /// <param name="profile"></param>
        public void SetProfile(Profile profile)
        {
            lock (_mutex)
            {
                _pendingProfile = profile;
                _configChanged = true;
            }
        }

        /// <summary>
        /// Apply new configuration settings
        /// </summary>
        /// <param name="profile"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            lock (_mutex)
            {
                _pendingAppConfig = appConfig;
                _configChanged = true;
            }
        }

        /// <summary>
        /// Enable or disable diagnostics
        /// </summary>
        /// <param name="enable"></param>
        public void ConfigureDiagnostics(bool enable)
        {
            _stateManager.ConfigureDiagnostics(enable);
        }

        /// <summary>
        /// Start polling input sources
        /// </summary>
        public void StartPolling()
        {
            if (!_continue)
            {
                _continue = true;

                ThreadStart threadStart = new ThreadStart(RunStateManager);
                _stateManagerThread = new Thread(threadStart);
                _stateManagerThread.Start();
            }
        }

        /// <summary>
        /// Start the polling loop
        /// </summary>
        private void RunStateManager()
        {
            // Set the culture
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            // This blocks until the state handler exits
            _stateManager = new StateManager(this);
            _stateManager.Run();

            // State handler exited
            _continue = false;
        }

        /// <summary>
        /// Signal to the state handler that it should polling input sources
        /// </summary>
        public void StopPolling()
        {
            _continue = false;
        }

        /// <summary>
        /// Return whether or not the child container should continue polling input sources or not
        /// </summary>
        /// <returns></returns>
        public bool ContinuePolling()
        {
            return _continue;
        }

        /// <summary>
        /// Transfer pending events to report to the list of events for other threads to read from
        /// </summary>
        public List<EventReport> GetNewEventReports()
        {
            // Clear already reported events
            _reportedEvents.Clear();

            if (_eventsReported)
            {
                // Switch buffers
                lock (_mutex)
                {
                    List<EventReport> temp = _pendingEventReports;
                    _pendingEventReports = _reportedEvents;
                    _reportedEvents = temp;
                    _eventsReported = false;
                }
            }

            return _reportedEvents;
        }

        /// <summary>
        /// Submit an event raised by the UI thread (e.g. custom window) to the state manager thread
        /// </summary>
        /// <param name="args"></param>
        public void SubmitEvent(AltControlEventArgs args)
        {
            lock (_mutex)
            {
                _pendingEventSubmissions.Add(args);

                // Check buffer isn't overflowing
                if (_pendingEventSubmissions.Count > _maxEventsInBuffer)
                {
                    // Remove half of max capacity
                    _pendingEventSubmissions.RemoveRange(0, _maxEventsInBuffer >> 1);
                }

                _eventsSubmitted = true;
            }
        }
        
        /// <summary>
        /// Initialise buffers for inter-thread comms
        /// </summary>
        private void InitialiseBuffers()
        {
            _pendingEventReports = new List<EventReport>();
            _reportedEvents = new List<EventReport>();
            _pendingEventSubmissions = new List<AltControlEventArgs>();
            _submittedEvents = new List<AltControlEventArgs>();
        }

        /// <summary>
        /// Allow the state manager to check for new profile
        /// </summary>
        /// <param name="stateHandler"></param>
        public void ReceiveConfigUpdates(StateManager stateHandler)
        {
            if (_configChanged)
            {
                lock (_mutex)
                {
                    if (_pendingAppConfig != null)
                    {
                        stateHandler.SetAppConfig(_pendingAppConfig);
                        _pendingAppConfig = null;
                    }

                    if (_pendingProfile != null)
                    {
                        stateHandler.SetProfile(_pendingProfile);
                        _pendingProfile = null;
                    }

                    _configChanged = false;
                }
            }
        }

        /// <summary>
        /// Allow the state manager to check for any events submitted from the UI thread
        /// </summary>
        /// <param name="stateHandler"></param>
        public List<AltControlEventArgs> GetNewEventSubmissions()
        {
            // Clear previous submissions
            _submittedEvents.Clear();

            if (_eventsSubmitted)
            {
                // Switch buffers
                lock (_mutex)
                {
                    List<AltControlEventArgs> temp = _pendingEventSubmissions;
                    _pendingEventSubmissions = _submittedEvents;
                    _submittedEvents = temp;
                    _eventsSubmitted = false;
                }
            }

            return _submittedEvents;
        }

        /// <summary>
        /// Add event to list of pending events to report to other threads
        /// </summary>
        public void ScheduleEventReport(EventReport report)
        {
            lock (_mutex)
            {
                _pendingEventReports.Add(report);
                _eventsReported = true;
            }
        }

    }
}
