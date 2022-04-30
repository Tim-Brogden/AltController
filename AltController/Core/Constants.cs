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
    /// Application-wide constants
    /// </summary>
    public class Constants
    {
        public const string FileVersion = "1.96";    // Update this when the profile file format changes
        public const string AppVersion = "1.96";    // Update this when the application version changes
        
        public const long LastUsedID = -4;
        public const long NextID = -3;
        public const long PreviousID = -2;
        public const long DefaultID = -1;
        public const long NoneID = 0;
        public const string ApplicationName = "Alt Controller";
        public const string AuthorName = "Tim Brogden";
        public const string TranslatorNames = "Georg Niedermeier (DE)";
        public const string UserGuideURL = "https://altcontroller.net/docs/user-guide/";
        public const string MessageLogFileName = "Message Log.txt";
        public const string ProfilesFolderName = "Profiles";
        public const string ConfigFileName = "config.xml";
        public const int PointerCircle = 1;
        public const int PointerLine = 2;

        // App config settings
        public const string ConfigDrawScreenRegions = "draw_regions";
        public const string ConfigDrawScreenRegionsHotkey = "draw_regions_hotkey";
        public const string ConfigShowScreenRegionNames = "show_region_names";
        public const string ConfigShowScreenRegionNamesHotkey = "show_region_names_hotkey";
        public const string ConfigDrawPointerIndicatorLine = "draw_pointer_indicator_line";
        public const string ConfigDrawPointerIndicatorLineHotkey = "draw_pointer_indicator_line_hotkey";
        public const string ConfigDrawStateOverlay = "draw_state_overlay";
        public const string ConfigDrawStateOverlayHotkey = "draw_state_overlay_hotkey";
        public const string ConfigCustomWindowTitleBarsHotkey = "custom_window_title_bars_hotkey";
        public const string ConfigDrawRegionForceSquare = "draw_region_force_square";
        public const string ConfigLastUsedProfile = "last_profile";
        public const string ConfigLanguageCode = "language_code";
        public const string ConfigAutoLoadLastProfile = "auto_load_last_profile";
        public const string ConfigAutoOpenCustomWindows = "auto_open_custom_windows";
        public const string ConfigUseScanCodes = "use_directx_key_strokes";
        public const string ConfigInputPollingIntervalMS = "input_polling_interval_ms";
        public const string ConfigUIUpdateIntervalMS = "ui_update_interval_ms";
        public const string ConfigDwellTimeMS = "dwell_time_ms";
        public const string ConfigDPIXSetting = "current_dpi_x";
        public const string ConfigDPIYSetting = "current_dpi_y";
        public const string ConfigProfilesDir = "profiles_dir";
        public const string ConfigPointerIndicatorStyle = "pointer_indicator_style";
        public const string ConfigPointerIndicatorColour = "pointer_indicator_colour";
        public const string ConfigPointerIndicatorRadius = "pointer_indicator_radius";
        public const string ConfigPointerIndicatorLineThickness = "pointer_indicator_line_thickness";
        public const string ConfigStateOverlayTextColour = "state_overlay_text_colour";
        public const string ConfigStateOverlayBgColour = "state_overlay_bg_colour";
        public const string ConfigStateOverlayTranslucency = "state_overlay_translucency";
        public const string ConfigStateOverlayFontSize = "state_overlay_font_size";
        public const string ConfigStateOverlayXPos = "state_overlay_x_pos";
        public const string ConfigStateOverlayYPos = "state_overlay_y_pos";
        public const string ConfigCustomWindowTitleBars = "custom_window_title_bars";
        public const string ConfigAutoStopPressActions = "auto_stop_press_actions";
        public const string ConfigAutoStopInsideActions = "auto_stop_inside_actions";

        // Default config values
        public const bool DefaultDrawScreenRegions = true;
        public const bool DefaultShowScreenRegionNames = false;
        public const bool DefaultDrawPointerIndicatorLine = false;
        public const bool DefaultDrawStateOverlay = false;
        public const bool DefaultDrawRegionForceSquare = false;
        public const string DefaultLanguageCode = "en-GB";
        public const string DefaultLanguageName = "English (GB)";
        public const bool DefaultAutoLoadLastProfile = true;
        public const bool DefaultAutoOpenCustomWindows = true;
        public const bool DefaultUseScanCodes = true;
        public const int MinInputPollingIntervalMS = 10;
        public const int DefaultInputPollingIntervalMS = 20;
        public const int WindowPollingIntervalMS = 200;
        public const int DefaultUIUpdateIntervalMS = 40;
        public const int DefaultDwellTimeMS = 200;
        public const int DefaultPointerIndicatorStyle = PointerCircle;
        public const string DefaultPointerIndicatorColour = "LightGray";
        public const int DefaultPointerIndicatorRadius = 12;
        public const int DefaultPointerIndicatorLineThickness = 2;
        public const string DefaultStateOverlayTextColour = "Snow";
        public const string DefaultStateOverlayBgColour = "Black";
        public const double DefaultStateOverlayTranslucency = 0.5;
        public const double DefaultStateOverlayFontSize = 20.0;
        public const double DefaultStateOverlayXPos = 0.05;
        public const double DefaultStateOverlayYPos = 0.05;
        public const bool DefaultCustomWindowTitleBars = true;
        public const bool DefaultAutoStopPressActions = false;
        public const bool DefaultAutoStopInsideActions = false;

        // Other defaults
        public const int DefaultPressTimeMS = 50;
        public const int DefaultCustomWindowWidthPx = 300;
        public const int DefaultCustomWindowHeightPx = 200;
        public const double DefaultCustomWindowTranslucency = 0.5;
        public const double DefaultCustomButtonTranslucency = 0.5;
        public const double DefaultCustomButtonBorderThickness = 1.0;
        public const string DefaultCustomButtonBorderColour = "Black";
        public const string DefaultCustomButtonBackgroundColour = "LightGray";
        public const string DefaultScreenRegionColour = "LightGray";
        public const double DefaultCustomButtonFontSize = 12.0;
        public const string DefaultCustomButtonTextColour = "Black";
        public const double DefaultScreenRegionTranslucency = 0.5;
        public const bool DefaultIsGhostBackground = true;
        public const bool DefaultIsTopMost = true;
        public const int MinCustomButtonSize = 1;
        public const EDisplayArea DefaultOverlayArea = EDisplayArea.PrimaryScreen;
    }
}
