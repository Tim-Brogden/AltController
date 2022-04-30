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
using System.Windows;
using AltController.Config;
using AltController.Event;
using AltController.Input;
using AltController.Sys;

namespace AltController.Core
{
    /// <summary>
    /// Interface for parent window
    /// </summary>
    public interface IParentWindow
    {
        Profile GetCurrentProfile();
        AppConfig GetAppConfig();
        void ApplyNewProfile(Profile profile);
        void ChildWindowClosing(Window window);
        void AttachEventReportHandler(EventReportHandler handler);
        void DetachEventReportHandler(EventReportHandler handler);
        void SubmitEvent(AltControlEventArgs args);
        void ConfigureDiagnostics(bool enable);
    }

    /// <summary>
    /// Interface for state manager
    /// </summary>
    public interface IStateManager
    {
        Profile CurrentProfile { get; }
        KeyPressManager KeyStateManager { get; }
        MouseManager MouseStateManager { get; }
        Rect CurrentWindowRect { get; }
        Rect OverlayWindowRect { get; }
        double DPI_X { get; }
        double DPI_Y { get; }
        bool IsDiagnosticsEnabled { get; }
        int SeqNumber { get; }
        void ReportEvent(EventReport report);
        void Reset();
        void SetMode(long modeID);
        void SetApp(string appName);
        void SetPage(long pageID);
        void SetCurrentWindowRect(RECT rectangle);
    }

    /// <summary>
    /// Interface for controls which display inputs
    /// </summary>
    public interface IInputViewer
    {
        AltControlEventArgs GetSelectedControl();
        void SetSource(BaseSource source);
        void SetSelectedControl(AltControlEventArgs args);
        void ClearHighlighting();
        void HighlightEvent(AltControlEventArgs args, HighlightInfo highlightInfo);
    }
}
