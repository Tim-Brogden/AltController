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
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.IO;
using AltController.Config;
using AltController.Core;
using AltController.Event;

namespace AltController
{
    /// <summary>
    /// Diagnostics window
    /// </summary>
    public partial class DiagnosticsWindow : Window
    {
        // Members
        private IParentWindow _parentWindow;
        private ObservableCollection<EventReportDetails> _eventLog = new ObservableCollection<EventReportDetails>();
        private bool _isRecording = false;
        private Utils _utils = new Utils();

        // Private class
        public class EventReportDetails
        {
            public string Timestamp { get; set; }
            public string EventType { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }

            public EventReportDetails()
            {
                Timestamp = "";
                EventType = "";
                Title = "";
                Content = "";
            }
        }

        // Properties
        public ObservableCollection<EventReportDetails> EventLog { get { return _eventLog; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public DiagnosticsWindow(IParentWindow parent)
        {
            _parentWindow = parent;
            
            InitializeComponent();
            this.EventsGrid.AutoGenerateColumns = false;
            this.EventsGrid.DataContext = EventLog;
        }

        /// <summary>
        /// Start clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartRecording();
        }

        /// <summary>
        /// Stop clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
        }

        /// <summary>
        /// Save clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the save dialog
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.InitialDirectory = AppConfig.UserDataDir;
            dialog.CheckPathExists = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = Properties.Resources.String_Text_files_filter;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Prepare csv text
                string filePath = dialog.FileName;
                StringBuilder sb = new StringBuilder();
                foreach (EventReportDetails eventItem in _eventLog)
                {
                    sb.Append("'");
                    sb.Append(eventItem.Timestamp);
                    sb.Append("','");
                    sb.Append(eventItem.EventType);
                    sb.Append("','");
                    sb.Append(eventItem.Title);
                    sb.Append("','");
                    sb.Append(eventItem.Content);
                    sb.AppendLine("'");
                }

                // Save to file
                File.WriteAllText(filePath, sb.ToString());
            }
        }

        /// <summary>
        /// Can Close command execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Close window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
            e.Handled = true;
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopRecording();
            if (_parentWindow != null)
            {
                _parentWindow.ChildWindowClosing(this);
            }
        }

        /// <summary>
        /// Start recording
        /// </summary>
        private void StartRecording()
        {
            if (!_isRecording)
            {
                _isRecording = true;

                // Disable things during recording
                this.StartButton.IsEnabled = false;
                this.StopButton.IsEnabled = true;
                this.SaveButton.IsEnabled = false;
                //this.ClearButton.IsEnabled = false;
                this.SummaryRadioButton.IsEnabled = false;
                this.DetailedRadioButton.IsEnabled = false;

                // Get the level of detail
                bool isDetailed = this.DetailedRadioButton.IsChecked == true;

                // Log the level of detail
                string timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff");
                EventReportDetails eventDetails = new EventReportDetails();
                eventDetails.Timestamp = timestamp;
                eventDetails.EventType = Properties.Resources.String_Level_of_detail;
                string title = isDetailed ? Properties.Resources.Log_DetailedRadioButton : Properties.Resources.Log_SummaryRadioButton;
                eventDetails.Title = title.Replace("_", "");
                AddEvent(eventDetails);

                // Log the current profile
                Profile profile = _parentWindow.GetCurrentProfile();
                AltStringValEventArgs args = new AltStringValEventArgs(profile.Name);
                EventReport report = new EventReport(DateTime.Now, EEventType.ProfileChange, args);
                HandleEventReport(report);

                // Ask the parent window to send event reports and enable diagnostics
                _parentWindow.AttachEventReportHandler(HandleEventReport);

                // Enable diagnostics if Detailed option is selected
                if (isDetailed)
                {
                    _parentWindow.ConfigureDiagnostics(true);
                }
            }
        }

        /// <summary>
        /// Stop recording
        /// </summary>
        private void StopRecording()
        {
            if (_isRecording)
            {
                _isRecording = false;

                // Ask the parent window to send event reports and turn off diagnostics if required
                if (this.DetailedRadioButton.IsChecked == true)
                {
                    _parentWindow.ConfigureDiagnostics(false);
                }
                _parentWindow.DetachEventReportHandler(HandleEventReport);

                // Re-enable buttons
                this.StopButton.IsEnabled = false;
                this.StartButton.IsEnabled = true;
                this.SaveButton.IsEnabled = true;
                //this.ClearButton.IsEnabled = true;
                this.SummaryRadioButton.IsEnabled = true;
                this.DetailedRadioButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handle a controller event
        /// </summary>
        /// <param name="ev"></param>
        private void HandleEventReport(EventReport report)
        {
            EventReportDetails eventDetails = new EventReportDetails();
            eventDetails.Timestamp = report.Timestamp.ToString("dd/MM/yyyy HH:mm:ss.fff");
            eventDetails.EventType = GUIUtils.EventTypeToString(report.EventType);

            Profile profile = _parentWindow.GetCurrentProfile();
            switch (report.EventType)
            {
                case EEventType.KeyEvent:
                    {
                        AltKeyEventArgs args = (AltKeyEventArgs)report.Args;
                        VirtualKeyData vk = KeyUtils.GetVirtualKeyByKeyCode(args.Key);
                        string reason = args.IsKeyDown ? Properties.Resources.String_Pressed : Properties.Resources.String_Released;
                        eventDetails.Title = string.Format("'{0}' {1}", vk != null ? vk.Name : "N/K", reason);
                    }
                    break;
                case EEventType.RegionEvent:
                    {
                        AltRegionChangeEventArgs args = (AltRegionChangeEventArgs)report.Args;
                        eventDetails.Title = string.Format("{0} '{1}'", _utils.GetReasonName(args.Reason), args.RegionName);
                    }
                    break;
                case EEventType.MouseButtonEvent:
                    {
                        AltMouseButtonEventArgs args = (AltMouseButtonEventArgs)report.Args;
                        string reason = args.IsButtonDown ? Properties.Resources.String_Pressed : Properties.Resources.String_Released;
                        eventDetails.Title = string.Format("'{0}' {1}", _utils.GetMouseButtonName(args.MouseButton), reason);
                    }
                    break;
                case EEventType.MouseScrollEvent:
                    {
                        AltMouseScrollEventArgs args = (AltMouseScrollEventArgs)report.Args;
                        string direction = args.IsUp ? Properties.Resources.String_up : Properties.Resources.String_down;
                        eventDetails.Title = string.Format(Properties.Resources.String_Scroll_X, direction);
                    }
                    break;
                // Exclude control events like pointer movement, because there could be a lot
                //case EEventType.Control:
                //    {
                //        AltControlEventArgs args = (AltControlEventArgs)report.Args;
                //        NamedItem inputSource = profile.GetInputSource(args.SourceID);
                //        eventDetails.Title = string.Format("{0} - {1} - {2}",
                //            inputSource != null ? inputSource.Name : "Unknown",
                //            args.GetControlTypeName(),
                //            args.GetStateName());
                //    }
                //    break;
                case EEventType.StateChange:
                    {
                        AltStateChangeEventArgs args = (AltStateChangeEventArgs)report.Args;
                        NamedItem modeDetails = profile.GetModeDetails(args.LogicalState.ModeID);
                        NamedItem appDetails = profile.GetAppDetails(args.LogicalState.AppID);
                        NamedItem pageDetails = profile.GetPageDetails(args.LogicalState.PageID);
                        eventDetails.Title = string.Format("{0} '{1}', {2} '{3}', {4} '{5}'",
                            Properties.Resources.String_Mode,
                            modeDetails != null ? modeDetails.Name : "N/K",
                            Properties.Resources.String_App,
                            appDetails != null ? appDetails.Name : "N/K",
                            Properties.Resources.String_Page,
                            pageDetails != null ? pageDetails.Name : "N/K");
                    }
                    break;
                case EEventType.WindowRegionEvent:
                    {
                        WindowRegionEventArgs args = (WindowRegionEventArgs)report.Args;
                        eventDetails.Title = string.Format("{0}={1:0} {2}={3:0} {4}={5:0} {6}={7:0}",
                                                            Properties.Resources.String_Left, args.Rect.Left,
                                                            Properties.Resources.String_Top, args.Rect.Top,
                                                            Properties.Resources.String_Right, args.Rect.Right,
                                                            Properties.Resources.String_Bottom, args.Rect.Bottom);
                    }
                    break;
                case EEventType.ProfileChange:
                    {
                        AltStringValEventArgs args = (AltStringValEventArgs)report.Args;
                        eventDetails.Title = string.Format(Properties.Resources.String_Profile_applied_X, args.Val);                        
                    }
                    break;
                default:
                    // Don't log
                    eventDetails = null;
                    break;
            }

            if (eventDetails != null)
            {
                AddEvent(eventDetails);
            }
        }

        /// <summary>
        /// Add an event
        /// </summary>
        /// <param name="eventDetails"></param>
        private void AddEvent(EventReportDetails eventDetails)
        {
            _eventLog.Add(eventDetails);
            ClearButton.IsEnabled = true;
        }

        /// <summary>
        /// Clear the table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _eventLog.Clear();
            ClearButton.IsEnabled = false;
        }

    }
}
