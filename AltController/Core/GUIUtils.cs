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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AltController.Config;
using AltController.Event;
using AltController.Input;

namespace AltController.Core
{
    /// <summary>
    /// GUI utility methods
    /// </summary>
    class GUIUtils
    {
        private static VirtualKeyData[] _virtualKeyData;

        /// <summary>
        /// Get a display area size
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public static Size GetDisplayAreaSize(EDisplayArea area)
        {
            Size size;
            switch (area)
            {
                case EDisplayArea.PrimaryScreen:
                    size = new Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
                    break;
                case EDisplayArea.PrimaryWorkingArea:
                    size = new Size(SystemParameters.FullPrimaryScreenWidth, SystemParameters.FullPrimaryScreenHeight);
                    break;
                case EDisplayArea.WorkingArea:
                    size = new Size(SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
                    break;
                default:
                    size = new Size(SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
                    break;
            }

            return size;
        }

        /// <summary>
        /// Populate the list of items for a display area combo box
        /// </summary>
        /// <param name="list"></param>
        public static void PopulateDisplayAreaList(NamedItemList list)
        {
            Utils utils = new Utils();
            list.Clear();
            foreach (EDisplayArea area in Enum.GetValues(typeof(EDisplayArea)))
            {
                list.Add(new NamedItem((long)area, utils.GetDisplayAreaName(area)));
            }
        }

        /// <summary>
        /// Populate the list of items for a combo box
        /// </summary>
        public static void PopulateDisplayableListWithNamedItems(NamedItemList list,
                                                                    NamedItemList namedItems,
                                                                    bool addShortCutItems)
        {
            list.Clear();

            if (addShortCutItems)
            {
                list.Add(new NamedItem(Constants.LastUsedID, Properties.Resources.String_Last_used));
                list.Add(new NamedItem(Constants.NextID, Properties.Resources.String_Next));
                list.Add(new NamedItem(Constants.PreviousID, Properties.Resources.String_Previous));
            }

            foreach (NamedItem item in namedItems)
            {
                list.Add(new NamedItem(item.ID, item.Name));
            }
        }

        /// <summary>
        /// Populate the list of items for a key selection combo box
        /// </summary>
        /// <param name="modesList"></param>
        public static void PopulateDisplayableListWithKeys(NamedItemList list)
        {
            if (_virtualKeyData == null)
            {
                _virtualKeyData = KeyUtils.GetVirtualKeysByKeyCode();
            }

            // Sort the names into alphabetical order for readability
            List<long> idList = new List<long>();
            List<string> keyNamesList = new List<string>();
            for (uint i = 0; i < _virtualKeyData.Length; i++)
            {
                VirtualKeyData vk = _virtualKeyData[i];
                if (vk != null)
                {                    
                    idList.Add((int)_virtualKeyData[i].KeyCode);
                    keyNamesList.Add(_virtualKeyData[i].Name);
                }
            }
            long[] keyIDs = idList.ToArray();
            string[] keyNames = keyNamesList.ToArray();
            Array.Sort<string, long>(keyNames, keyIDs);

            // Add key-name pairs to the displayable list
            // Add the "length 1" items first, sorted alpabetically for readability
            list.Clear();
            for (int i = 0; i < keyNames.Length; i++)
            {
                if (keyNames[i].Length == 1)
                {
                    list.Add(new NamedItem(keyIDs[i], keyNames[i]));
                }
            }
            // Now add the remaining named keys, sorted alpabetically
            for (int i = 0; i < keyNames.Length; i++)
            {
                if (keyNames[i].Length > 1)
                {
                    list.Add(new NamedItem(keyIDs[i], keyNames[i]));
                }
            }
        }

        /// <summary>
        /// Crop and append ellipsis to long strings
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLen"></param>
        /// <returns></returns>
        public static string CapDisplayStringLength(string str, int maxLen)
        {
            string result;
            if (str.Length > maxLen)
            {
                result = str.Substring(0, maxLen - 3) + "...";
            }
            else
            {
                result = str;
            }

            return result;
        }

        /// <summary>
        /// Convert an event type to a string
        /// </summary>
        /// <param name="eEventType"></param>
        /// <returns></returns>
        public static string EventTypeToString(EEventType eEventType)
        {
            string name;
            switch (eEventType)
            {
                case EEventType.KeyEvent:
                    name = Properties.Resources.String_Key; break;
                case EEventType.RegionEvent:
                    name = Properties.Resources.String_Region; break;
                case EEventType.MouseButtonEvent:
                    name = Properties.Resources.String_Mouse_Button; break;
                case EEventType.MouseScrollEvent:
                    name = Properties.Resources.String_Scroll_Wheel; break;
                case EEventType.Control:
                    name = Properties.Resources.String_Input_Event; break;
                case EEventType.StateChange:
                    name = Properties.Resources.String_Situation_Changed; break;
                case EEventType.ProfileChange:
                    name = Properties.Resources.String_Profile; break;
                case EEventType.ToggleKeyEvent:
                    name = Properties.Resources.String_Key_Toggled; break;
                case EEventType.WindowRegionEvent:
                    name = Properties.Resources.String_Window; break;
                default:
                    name = Properties.Resources.String_Other; break;
            }

            return name;
        }

        /// <summary>
        /// Get the list of action types that are valid for a particular event
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static NamedItemList GetValidActionTypeList(AltControlEventArgs args, Profile profile)
        {
            NamedItemList actionTypeList = new NamedItemList();
            Utils utils = new Utils();

            BaseSource inputSource = profile.GetInputSource(args.SourceID);
            if (inputSource != null)
            {
                foreach (EActionType actionType in Enum.GetValues(typeof(EActionType)))
                {
                    // Action types are allowed by default
                    bool add = true;

                    switch (inputSource.SourceType)
                    {
                        case ESourceType.CustomWindow:
                            // Disallow some actions for custom window buttons
                            switch (actionType)
                            {
                                case EActionType.MouseClick:
                                case EActionType.MouseDoubleClick:
                                case EActionType.MouseHold:
                                case EActionType.MouseRelease:
                                case EActionType.ToggleMouseButton:
                                    add = (args.EventReason == EEventReason.Dwelled) || (args.EventReason == EEventReason.Inside) || (args.EventReason == EEventReason.Outside);
                                    break;
                                case EActionType.RepeatKeyDirectional:
                                case EActionType.MoveThePointer:
                                    add = false;
                                    break;
                            }
                            break;
                        case ESourceType.Keyboard:
                            // Disable continuous actions with keyboard input
                            switch (actionType)
                            {
                                case EActionType.RepeatKeyDirectional:
                                    add = false;
                                    break;
                            }
                            break;
                        case ESourceType.Mouse:
                            switch (actionType)
                            {
                                case EActionType.RepeatKeyDirectional:
                                    add = (args.EventReason == EEventReason.Updated);
                                    break;
                                case EActionType.MoveThePointer:
                                    // Don't allow controlling the pointer with itself
                                    add = false;
                                    break;
                                case EActionType.MouseClick:
                                case EActionType.MouseDoubleClick:
                                case EActionType.MouseHold:
                                case EActionType.MouseRelease:
                                case EActionType.ToggleMouseButton:
                                    add = (args.EventReason == EEventReason.Dwelled) || (args.EventReason == EEventReason.Inside) || (args.EventReason == EEventReason.Outside);
                                    break;
                                default:
                                    add = (args.EventReason != EEventReason.Moved && args.EventReason != EEventReason.Updated);
                                    break;
                            }
                            break;
                    }

                    if (add)
                    {
                        actionTypeList.Add(new NamedItem((long)actionType, utils.GetActionTypeName(actionType)));
                    }
                }
            }

            return actionTypeList;
        }

        /// <summary>
        /// Create an image brush for an image file
        /// </summary>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        /// <remarks>Image file path may be relative to Profiles directory</remarks>
        public static ImageBrush CreateImageBrush(string imageFile, string profilesDir)
        {
            ImageBrush imageBrush = null;
            try
            {
                if (!string.IsNullOrEmpty(imageFile))
                {
                    if (!imageFile.Contains(":"))
                    {
                        // Relative path - try to find in the user's profiles folder
                        imageFile = System.IO.Path.Combine(profilesDir, imageFile);
                    }
                    System.IO.FileInfo fi = new System.IO.FileInfo(imageFile);

                    if (fi.Exists)
                    {
                        // Create brush
                        BitmapImage bi = new BitmapImage(new Uri(fi.FullName));
                        imageBrush = new ImageBrush(bi);
                    }
                }
            }
            catch (Exception)
            {
                imageBrush = null;
            }

            return imageBrush;
        }

        /// <summary>
        /// Get a coloured brush
        /// </summary>
        /// <param name="colourName"></param>
        /// <param name="defaultBrush"></param>
        /// <returns></returns>
        public static SolidColorBrush GetBrushFromColour(string colourName, byte aVal, SolidColorBrush defaultBrush)
        {
            SolidColorBrush brush = null;
            try
            {
                if (!string.IsNullOrEmpty(colourName))
                {
                    Color colour = (Color)ColorConverter.ConvertFromString(colourName);
                    colour.A = aVal;
                    brush = new SolidColorBrush(colour);
                }
            }
            finally
            {
                if (brush == null)
                {
                    brush = defaultBrush;
                }
            }

            return brush;
        }
    }
}
