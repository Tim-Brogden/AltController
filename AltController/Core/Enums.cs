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

namespace AltController.Core
{
    /// <summary>
    /// Types of input source
    /// </summary>
    public enum ESourceType
    {
        Internal,
        Mouse,
        Keyboard,
        CustomWindow
    }

    /// <summary>
    /// Types of input control
    /// </summary>
    public enum EControlType
    {
        None,
        MousePointer,
        MouseButtons,
        Keyboard,
        CustomButton
    }

    /// <summary>
    /// Sides
    /// </summary>
    public enum ESide
    {
        None,
        Left,
        Right
    }

    /// <summary>
    /// Reasons for raising an event
    /// </summary>
    public enum EEventReason
    {
        None,
        Pressed,
        Released,
        Moved,
        Updated,
        Dwelled,
        Inside,
        Outside
    }

    /// <summary>
    /// Legacy Controller / mouse button type
    /// </summary>
    [Flags]
    public enum ELegacyButtonState
    {
        None,
        A,
        B,
        Back,
        Big,
        LeftShoulder,
        //LeftStick,
        RightShoulder,
        //RightStick,
        Start,
        X,
        Y,
        Left,
        Middle,
        Right,
        X1,
        X2
    }

    /// <summary>
    /// Mouse buttons
    /// </summary>
    public enum EMouseButton
    {
        None,
        Left,
        Middle,
        Right,
        X1,
        X2
    }

    /// <summary>
    /// Left-right-up-down states
    /// </summary>
    [Flags]
    public enum ELRUDState
    {
        None=0,
        Left=1,
        Right=2,
        Up=4,
        Down=8,
        UpLeft=Up|Left,
        UpRight=Up|Right,
        DownLeft=Down|Left,
        DownRight=Down|Right
    }
    
    /// <summary>
    /// Types of action that can be performed
    /// </summary>
    public enum EActionType
    {
        TypeKey,
        TypeText,
        HoldKey,
        ReleaseKey,
        RepeatKey,
        RepeatKeyDirectional,
        ToggleKey,
        ToggleMouseButton,
        MouseHold,
        MouseClick,
        MouseDoubleClick,
        MouseRelease,    
        ChangeMode,
        ChangePage,
        ScrollUp,
        ScrollDown,
        RepeatScrollUp,
        RepeatScrollDown,
        StopScrolling,
        MoveThePointer,
        MenuOption,
        Wait,
        StopOngoingActions
    }

    /// <summary>
    /// Types of event that can be raised
    /// </summary>
    public enum EEventType
    {
        Unknown,
        Control,
        StateChange,
        ProfileChange,
        KeyEvent,
        ToggleKeyEvent,
        RegionEvent,
        MouseButtonEvent,
        MouseScrollEvent,
        WindowRegionEvent,
        MenuOptionEvent
    }

    /// <summary>
    /// Data types for event state data
    /// </summary>
    public enum EDataType
    {
        None = 0,
        Bool = 1,
        Integer = 2,
        Double = 3,
        LRUD = 4
    }

    /// <summary>
    /// Type of highlighting
    /// </summary>
    public enum EHighlightType
    {
        None = 0,
        Default = 1,
        Configured = 2,
        Selected = 3
    }

    /// <summary>
    /// Type of key press event
    /// </summary>
    [Flags]
    public enum EKeyEventType
    {
        None = 0,
        Press = 1,
        Release = 2,
        Type = Press|Release
    }

    /// <summary>
    /// Type of action list
    /// </summary>
    public enum EActionListType
    {
        Parallel,
        Series
    }

    /// <summary>
    /// Type of pointer change
    /// </summary>
    public enum ECursorType
    {
        User,
        Standard,
        Blank
    }

    /// <summary>
    /// Type of display area
    /// </summary>
    public enum EDisplayArea
    {
        PrimaryScreen,      // The main display (full)
        PrimaryWorkingArea, // The main display (excluding taskbar, etc.)
        Desktop,            // The whole desktop (possibly spanning multiple screens)
        WorkingArea,        // The whole desktop (excluding taskbar, etc.)
        ActiveWindow        // The currently active window
    }

    /// <summary>
    /// Type of shape
    /// </summary>
    public enum EShape
    {
        Rectangle,
        Ellipse,
        EllipseSector,
        Annulus,
        AnnulusSector
    }

    /// <summary>
    /// Main menu options
    /// </summary>
    public enum EMainMenuOption
    {
        None,
        DrawScreenRegions,
        ShowScreenRegionNames,
        DrawPointerIndicatorLine,
        DrawStateOverlay,
        ShowTitleBars
    }

}
