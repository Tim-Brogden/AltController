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

namespace AltController.Core
{
    /// <summary>
    /// General utility methods
    /// </summary>
    public class Utils
    {                
        /// <summary>
        /// Get the name for each type of action
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public string GetActionTypeName(EActionType actionType)
        {
            string name;
            switch (actionType)
            {
                case EActionType.ChangeMode:
                    name = Properties.Resources.String_Change_mode; break;
                case EActionType.ChangePage:
                    name = Properties.Resources.String_Change_page; break;
                case EActionType.HoldKey:
                    name = Properties.Resources.String_Hold_key; break;
                case EActionType.MenuOption:
                    name = Properties.Resources.String_Toggle_menu_option; break;
                case EActionType.MouseClick:
                    name = Properties.Resources.String_Mouse_click; break;
                case EActionType.MouseDoubleClick:
                    name = Properties.Resources.String_Mouse_double_click; break;
                case EActionType.MouseHold:
                    name = Properties.Resources.String_Mouse_hold; break;
                case EActionType.MouseRelease:
                    name = Properties.Resources.String_Mouse_release; break;
                case EActionType.MoveThePointer:
                    name = Properties.Resources.String_Move_the_pointer; break;
                case EActionType.ReleaseKey:
                    name = Properties.Resources.String_Release_key; break;
                case EActionType.RepeatKey:
                    name = Properties.Resources.String_Repeat_key; break;
                case EActionType.RepeatKeyDirectional:
                    name = Properties.Resources.String_Repeat_key_directional; break;
                case EActionType.RepeatScrollDown:
                    name = Properties.Resources.String_Repeat_scroll_down; break;
                case EActionType.RepeatScrollUp:
                    name = Properties.Resources.String_Repeat_scroll_up; break;
                case EActionType.ScrollDown:
                    name = Properties.Resources.String_Scroll_down; break;
                case EActionType.ScrollUp:
                    name = Properties.Resources.String_Scroll_up; break;
                case EActionType.StopOngoingActions:
                    name = Properties.Resources.String_Stop_ongoing_actions; break;
                case EActionType.StopScrolling:
                    name = Properties.Resources.String_Stop_scrolling; break;
                case EActionType.ToggleKey:
                    name = Properties.Resources.String_Toggle_key; break;
                case EActionType.ToggleMouseButton:
                    name = Properties.Resources.String_Toggle_mouse_button; break;
                case EActionType.TypeKey:
                    name = Properties.Resources.String_Type_key; break;
                case EActionType.TypeText:
                    name = Properties.Resources.String_Type_text; break;
                case EActionType.Wait:
                    name = Properties.Resources.String_Wait; break;
                case EActionType.LoadProfile:
                    name = Properties.Resources.String_LoadProfile; break;
                case EActionType.StartProgram:
                    name = Properties.Resources.String_StartProgram; break;
                case EActionType.ActivateWindow:
                    name = Properties.Resources.String_Activate_window; break;
                case EActionType.MaximiseWindow:
                    name = Properties.Resources.String_MaximiseWindow; break;
                case EActionType.MinimiseWindow:
                    name = Properties.Resources.String_MinimiseWindow; break;
                default:
                    name = "N/K"; break;
            }

            return name;
        }

        public string GetDisplayAreaName(EDisplayArea area)
        {
            string name;
            switch (area)
            {
                case EDisplayArea.PrimaryScreen:
                    name = Properties.Resources.String_Primary_screen;
                    break;
                case EDisplayArea.PrimaryWorkingArea:
                    name = Properties.Resources.String_Primary_working_area;
                    break;
                case EDisplayArea.WorkingArea:
                    name = Properties.Resources.String_Virtual_working_area;
                    break;
                case EDisplayArea.ActiveWindow:
                    name = Properties.Resources.String_Active_window;
                    break;
                case EDisplayArea.Desktop:
                default:
                    name = Properties.Resources.String_Virtual_desktop;
                    break;
            }

            return name;
        }

        /// <summary>
        /// Get the name for each type of input source
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public string GetSourceTypeName(ESourceType sourceType)
        {
            string name;
            switch (sourceType)
            {
                case ESourceType.CustomWindow:
                    name = Properties.Resources.String_Custom_Window; break;
                case ESourceType.Keyboard:
                    name = Properties.Resources.String_Keyboard; break;
                case ESourceType.Mouse:
                    name = Properties.Resources.String_Mouse; break;
                case ESourceType.Internal:
                    name = Properties.Resources.String_Internal; break;
                default:
                    name = "N/K"; break;
            }

            return name;
        }

        /// <summary>
        /// Shape to string
        /// </summary>
        /// <param name="shape"></param>
        /// <returns></returns>
        public string GetShapeName(EShape shape) 
        {
            string name;
            switch (shape)
            {
                case EShape.Annulus:
                    name = Properties.Resources.String_Annulus; break;
                case EShape.AnnulusSector:
                    name = Properties.Resources.String_Annulus_sector; break;
                case EShape.Ellipse:
                    name = Properties.Resources.String_Ellipse; break;
                case EShape.EllipseSector:
                    name = Properties.Resources.String_Ellipse_sector; break;
                case EShape.Rectangle:
                    name = Properties.Resources.String_Rectangle; break;
                default:
                    name = "N/K"; break;
            }

            return name;
        }

        /// <summary>
        /// Reason to string
        /// </summary>
        /// <param name="eventReason"></param>
        /// <returns></returns>
        public string GetReasonName(EEventReason reason)
        {
            string name;
            switch (reason)
            {
                case EEventReason.Pressed:
                    name = Properties.Resources.String_Pressed; break;
                case EEventReason.Released:
                    name = Properties.Resources.String_Released; break;
                case EEventReason.Dwelled:
                    name = Properties.Resources.String_Dwelled; break;
                case EEventReason.Inside:
                    name = Properties.Resources.String_Inside; break;
                case EEventReason.Outside:
                    name = Properties.Resources.String_Outside; break;
                case EEventReason.Moved:
                    name = Properties.Resources.String_Moved; break;
                case EEventReason.Updated:
                    name = Properties.Resources.String_Updated; break;
                default:
                    name = Properties.Resources.String_None; break;
            }
            return name;
        }

        /// <summary>
        /// Mouse button to string
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public string GetMouseButtonName(EMouseButton button)
        {
            string name;
            switch (button) 
            {
                case EMouseButton.Left:
                    name = Properties.Resources.String_Left; break;
                case EMouseButton.Middle:
                    name = Properties.Resources.String_Middle; break;
                case EMouseButton.Right:
                    name = Properties.Resources.String_Right; break;
                case EMouseButton.X1:
                    name = Properties.Resources.String_X1; break;
                case EMouseButton.X2:
                    name = Properties.Resources.String_X2; break;
                default:
                    name = "N/K"; break;
            }

            return name;
        }        

        /// <summary>
        /// Return whether a parameter type conversion is possible
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        public bool IsDataTypeConversionValid(EDataType fromType, EDataType toType)
        {
            bool isValid = false;
            
            if (fromType == toType)
            {
                isValid = true;
            }

            return isValid;
        }
    }
}
