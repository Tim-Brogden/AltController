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
using System.Globalization;
using System.Xml;
using AltController.Core;

namespace AltController.Event
{
    /// <summary>
    /// Event data provided by input controls
    /// </summary>
    public class AltControlEventArgs : EventArgs
    {
        // The control raising the event
        public long SourceID;
        public EControlType ControlType;        
        public ESide Side;

        // The control state
        public EEventReason EventReason;
        public byte ButtonID;
        public ELRUDState LRUDState;
        public byte Data;
        public ushort ExtraData;

        // Optional name
        public string Name = "";

        // Parameter data
        public ParamValue Param0;
        public ParamValue Param1;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public AltControlEventArgs()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public AltControlEventArgs(AltControlEventArgs args)
            : base()
        {
            SourceID = args.SourceID;
            ControlType = args.ControlType;
            Side = args.Side;
            ButtonID = args.ButtonID;
            LRUDState = args.LRUDState;
            Data = args.Data;
            ExtraData = args.ExtraData;
            EventReason = args.EventReason;
            Name = args.Name;
            Param0 = args.Param0;
            Param1 = args.Param1;
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public AltControlEventArgs(long sourceID,
                                    EControlType controlType,
                                    ESide side,
                                    byte buttonID,
                                    ELRUDState lrudState,
                                    EEventReason eventReason)
        {
            SourceID = sourceID;
            ControlType = controlType;
            Side = side;
            ButtonID = buttonID;
            LRUDState = lrudState;
            EventReason = eventReason;
        }
        
        /// <summary>
        /// Convert to an event ID
        /// </summary>
        /// <returns></returns>
        /// <remarks>Excludes ExtraData param</remarks>
        public long ToID()
        {
            long id = (SourceID & 0xF) |
                     ((long)ControlType << 4) |
                     ((long)Side << 8) |
                     ((long)ButtonID << 12) |
                     ((long)LRUDState << 16) |
                     ((long)Data << 20) |                     
                     ((long)EventReason << 28);
            return id;
        }

        /// <summary>
        /// Convert from an ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AltControlEventArgs FromID(long id)
        {
            long sourceID = id & 0xF;
            EControlType controlType = (EControlType)((id >> 4) & 0xF);
            ESide side = (ESide)((id >> 8) & 0xF);
            byte buttonID = (byte)((id >> 12) & 0xF);
            ELRUDState lrudState = (ELRUDState)((id >> 16) & 0xF);
            byte data = (byte)((id >> 20) & 0xFF);
            EEventReason reason = (EEventReason)((id >> 28));

            AltControlEventArgs ev = new AltControlEventArgs(sourceID, controlType, side, buttonID, lrudState, reason);
            ev.Data = data;

            return ev;
        }

        /// <summary>
        /// Return a reason-independent ID
        /// </summary>
        /// <returns></returns>
        public long ToControlID()
        {
            return ToID() & 0xFFFFFFF;  // low 28 bits
        }

        /// <summary>
        /// Get a friendly name for the control this event applies to
        /// </summary>
        /// <returns></returns>
        public string GetControlTypeName()
        {
            string name = "";

            switch (ControlType)
            {
                case EControlType.MousePointer:
                    name = Properties.Resources.String_Pointer;
                    break;
                case EControlType.MouseButtons:
                    name = Properties.Resources.String_Mouse_Button;
                    break;                
                case EControlType.Keyboard:
                    name = Properties.Resources.String_Key;
                    break;
                case EControlType.CustomButton:
                    name = Properties.Resources.String_Custom_Button;
                    break;
                default:
                    name = Properties.Resources.String_None;
                    break;
            }

            return name;
        }

        /// <summary>
        /// Get a friendly name for this type of button state
        /// </summary>
        /// <returns></returns>
        public string GetStateName()
        {
            string name;
            Utils utils = new Utils();

            switch (ControlType)
            {
                case EControlType.MouseButtons:
                    {
                        string buttonStr = utils.GetMouseButtonName((EMouseButton)ButtonID);
                        name = string.Format("'{0}' {1}", buttonStr, utils.GetReasonName(EventReason));
                    }
                    break;
                case EControlType.Keyboard:
                    VirtualKeyData vk = KeyUtils.GetVirtualKeyByKeyCode((System.Windows.Forms.Keys)Data);
                    name = string.Format("'{0}' {1}", (vk != null) ? vk.Name : "N/K", utils.GetReasonName(EventReason));
                    break;
                case EControlType.CustomButton:
                    name = string.Format("'{0}' {1}", Name, utils.GetReasonName(EventReason));
                    break;
                case EControlType.MousePointer:
                default:
                    name = utils.GetReasonName(EventReason);
                    break;
            }

            return name;
        }

        /// <summary>
        /// Get the value of an event parameter
        /// </summary>
        /// <param name="paramIndex"></param>
        /// <returns></returns>
        public ParamValue GetParamValue(int paramIndex)
        {
            switch (paramIndex)
            {
                case 0:
                    return Param0;
                case 1:
                    return Param1;
                default:
                    // Error
                    return new ParamValue();
            }
        }

        /// <summary>
        /// Write to xml node
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public static void ToXml(AltControlEventArgs ev, XmlElement element, XmlDocument doc)
        {
            if (ev != null)
            {
                element.SetAttribute("sourceid", ev.SourceID.ToString(CultureInfo.InvariantCulture));
                element.SetAttribute("controltype", ev.ControlType.ToString());
                element.SetAttribute("side", ev.Side.ToString());
                element.SetAttribute("buttonid", ev.ButtonID.ToString(CultureInfo.InvariantCulture));
                element.SetAttribute("lrudstate", ev.LRUDState.ToString());
                element.SetAttribute("eventdata", ev.Data.ToString(CultureInfo.InvariantCulture));
                element.SetAttribute("extraeventdata", ev.ExtraData.ToString(CultureInfo.InvariantCulture));
                element.SetAttribute("eventreason", ev.EventReason.ToString());
                element.SetAttribute("eventname", ev.Name);
            }
        }

        /// <summary>
        /// Read from an xml node
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static AltControlEventArgs FromXml(XmlElement element)
        {
            AltControlEventArgs ev;

            // Source ID and control type are required attributes
            if (element.HasAttribute("sourceid") &&
                element.HasAttribute("controltype"))
            {
                long sourceID = long.Parse(element.GetAttribute("sourceid"), CultureInfo.InvariantCulture);
                EControlType controlType = (EControlType)Enum.Parse(typeof(EControlType), element.GetAttribute("controltype"));

                // Optional attributes
                ESide side = element.HasAttribute("side") ? (ESide)Enum.Parse(typeof(ESide), element.GetAttribute("side")) : ESide.None;

                // Read buttonid 
                byte buttonID = (byte)0;
                if (element.HasAttribute("buttonid"))
                {
                    buttonID = byte.Parse(element.GetAttribute("buttonid"), NumberStyles.Number, CultureInfo.InvariantCulture);
                }
                else if (element.HasAttribute("buttonstate"))
                {
                    // For formats prior to v1.5, convert legacy button state to button ID 
                    // (This really belongs in the profile upgrader)
                    string buttonStr = element.GetAttribute("buttonstate");
                    ELegacyButtonState legacyButtonState = ELegacyButtonState.None;
                    if (Enum.TryParse<ELegacyButtonState>(buttonStr, out legacyButtonState))
                    {
                        switch (legacyButtonState)
                        {
                            // Mouse buttons
                            case ELegacyButtonState.Left: buttonID = (byte)EMouseButton.Left; break;
                            case ELegacyButtonState.Middle: buttonID = (byte)EMouseButton.Middle; break;
                            case ELegacyButtonState.Right: buttonID = (byte)EMouseButton.Right; break;
                            case ELegacyButtonState.X1: buttonID = (byte)EMouseButton.X1; break;
                            case ELegacyButtonState.X2: buttonID = (byte)EMouseButton.X2; break;
                            default:
                                buttonID = (byte)legacyButtonState; break;  // Controller button
                        }
                    }
                }
                                
                ELRUDState lrudState = element.HasAttribute("lrudstate") ? (ELRUDState)Enum.Parse(typeof(ELRUDState), element.GetAttribute("lrudstate")) : ELRUDState.None;
                EEventReason eventReason = element.HasAttribute("eventreason") ? (EEventReason)Enum.Parse(typeof(EEventReason), element.GetAttribute("eventreason")) : EEventReason.None;
                string eventName = element.HasAttribute("eventname") ? element.GetAttribute("eventname") : "";

                byte data = 0;
                ushort extraData = 0;
                if (element.HasAttribute("eventdata"))
                {
                    data = byte.Parse(element.GetAttribute("eventdata"), CultureInfo.InvariantCulture);
                    
                    if (element.HasAttribute("extraeventdata"))
                    {
                        extraData = ushort.Parse(element.GetAttribute("extraeventdata"), CultureInfo.InvariantCulture);
                        if (extraData != 0 && controlType == EControlType.Keyboard)
                        {
                            // Extra data contains keyboard scan code - set data to be the corresponding virtual key
                            // because user could be using a different keyboard layout so the saved virtual key code (data) could now be incorrect
                            VirtualKeyData vk = KeyUtils.GetVirtualKeyByScanCode(extraData);
                            if (vk != null)
                            {
                                data = (byte)vk.KeyCode;
                            }
                        }
                    }
                    else if (controlType == EControlType.Keyboard)
                    {
                        // Extra data not present for keyboard event means it's an old profile, so set it now
                        VirtualKeyData vk = KeyUtils.GetVirtualKeyByKeyCode((System.Windows.Forms.Keys)data);
                        if (vk != null)
                        {
                            extraData = vk.WindowsScanCode;
                        }                            
                    }
                }

                ev = new AltControlEventArgs(sourceID, controlType, side, buttonID, lrudState, eventReason);
                ev.Data = data;
                ev.ExtraData = extraData;
                ev.Name = eventName;
            }
            else
            {
                ev = null;
            }

            return ev;
        }
    }
}
