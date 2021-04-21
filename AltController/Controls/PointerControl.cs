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
using AltController.Config;
using AltController.Core;
using AltController.Input;
using AltController.Event;

namespace AltController.Controls
{
    /// <summary>
    /// Monitors mouse pointer position
    /// </summary>
    public class PointerControl : BaseControl
    {
        // Members
        private Dictionary<byte, DateTime> _insideScreenRegions = new Dictionary<byte, DateTime>();
        private int _numMappings;
        private int[] _mappingCount;
        private Dictionary<byte, bool> _hasDwelled = new Dictionary<byte, bool>();
        private int _dwellTimeMS = Constants.DefaultDwellTimeMS;

        /// <summary>
        /// Get the event reasons supported by this control
        /// </summary>
        public override List<EEventReason> SupportedEventReasons
        {
            get { return new List<EEventReason> { EEventReason.Updated, EEventReason.Dwelled, EEventReason.Inside, EEventReason.Outside }; }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public PointerControl(MouseSource parent)
            : base(parent)
        {
            // Initialise array of number of mappings for each event reason
            int max = 0;
            foreach (EEventReason reason in Enum.GetValues(typeof(EEventReason))) 
            {
                max = Math.Max(max, (int)reason);
            }
            _mappingCount = new int[max + 1];
        }

        /// <summary>
        /// Handle change of app config
        /// </summary>
        /// <param name="appConfig"></param>
        public override void SetAppConfig(AppConfig appConfig)
        {
            _dwellTimeMS = appConfig.GetIntVal(Constants.ConfigDwellTimeMS, Constants.DefaultDwellTimeMS);            
        }

        /// <summary>
        /// Turn event handling on or off for a type of event
        /// </summary>
        /// <param name="args"></param>
        /// <param name="enable"></param>
        public override void ConfigureEventMonitoring(AltControlEventArgs args, bool enable)
        {
            int change = enable ? 1 : -1;
            _mappingCount[(int)args.EventReason] += change;
            _numMappings += change;
            IsActive = _numMappings > 0;
        }

        /// <summary>
        /// Return whether there are any mappings for the specified reason
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        private bool IsMapped(EEventReason reason)
        {
            return _mappingCount[(int)reason] > 0;
        }

        /// <summary>
        /// Update the state of the mouse and raise any required events
        /// </summary>
        /// <param name="newState"></param>
        public override void UpdateState(IStateManager stateManager)
        {
            // See which regions the pointer is in
            MouseSource mouseSource = (MouseSource)Parent;
            ScreenRegionList regionsList = Parent.Profile.ScreenRegions;
            AltControlEventArgs args;
            foreach (ScreenRegion region in regionsList)
            {
                byte regionID = (byte)region.ID;
                bool isInside = mouseSource.IsPointerInRegion(regionID);
                if (isInside)
                {
                    // See if the pointer just entered the region
                    if (!_insideScreenRegions.ContainsKey(regionID))
                    {
                        _insideScreenRegions[regionID] = DateTime.Now;

                        // Report if required
                        if (stateManager.IsDiagnosticsEnabled)
                        {
                            AltRegionChangeEventArgs ev = new AltRegionChangeEventArgs(region.Name, EEventReason.Inside);
                            EventReport report = new EventReport(DateTime.Now, EEventType.RegionEvent, ev);
                            stateManager.ReportEvent(report);
                        }

                        if (IsMapped(EEventReason.Inside))
                        {
                            args = new AltControlEventArgs();
                            args.SourceID = Parent.ID;
                            args.ControlType = EControlType.MousePointer;
                            args.EventReason = EEventReason.Inside;
                            args.Data = regionID;
                            RaiseEvent(args);
                        }
                    }
                    else if (IsMapped(EEventReason.Dwelled) &&
                            !_hasDwelled.ContainsKey(regionID) &&
                            (DateTime.Now - _insideScreenRegions[regionID]).TotalMilliseconds > _dwellTimeMS)
                    {
                        _hasDwelled[regionID] = true;

                        // Report if required
                        if (stateManager.IsDiagnosticsEnabled)
                        {
                            AltRegionChangeEventArgs ev = new AltRegionChangeEventArgs(region.Name, EEventReason.Dwelled);
                            EventReport report = new EventReport(DateTime.Now, EEventType.RegionEvent, ev);
                            stateManager.ReportEvent(report);
                        }

                        // Pointer dwelled                        
                        args = new AltControlEventArgs();
                        args.SourceID = Parent.ID;
                        args.ControlType = EControlType.MousePointer;
                        args.EventReason = EEventReason.Dwelled;
                        args.Data = regionID;
                        RaiseEvent(args);
                    }

                    if (IsMapped(EEventReason.Updated))
                    {
                        args = new AltControlEventArgs();
                        args.SourceID = Parent.ID;
                        args.ControlType = EControlType.MousePointer;
                        args.EventReason = EEventReason.Updated;
                        args.Data = regionID;
                        args.Param0.DataType = EDataType.Double;
                        args.Param0.Value = region.Rectangle.Width > 0.0 ? (mouseSource.WindowCoords.X - region.Rectangle.Left) / region.Rectangle.Width : 0.0;
                        args.Param1.DataType = EDataType.Double;
                        args.Param1.Value = region.Rectangle.Height > 0.0 ? (mouseSource.WindowCoords.Y - region.Rectangle.Top) / region.Rectangle.Height : 0.0;
                        RaiseEvent(args);
                    }
                }
                else
                {
                    // Outside the region
                    // See if the pointer just left the region
                    if (_insideScreenRegions.ContainsKey(regionID))
                    {
                        _insideScreenRegions.Remove(regionID);

                        if (_hasDwelled.ContainsKey(regionID))
                        {
                            _hasDwelled.Remove(regionID);
                        }

                        // Report if required
                        if (stateManager.IsDiagnosticsEnabled)
                        {
                            AltRegionChangeEventArgs ev = new AltRegionChangeEventArgs(region.Name, EEventReason.Outside);
                            EventReport report = new EventReport(DateTime.Now, EEventType.RegionEvent, ev);
                            stateManager.ReportEvent(report);
                        }

                        // Raise Outside event whenever Inside is mapped
                        // to enable auto stopping of ongoing Inside actions on leaving the region.
                        if (IsMapped(EEventReason.Outside) ||
                            IsMapped(EEventReason.Inside))
                        {
                            args = new AltControlEventArgs();
                            args.SourceID = Parent.ID;
                            args.ControlType = EControlType.MousePointer;
                            args.EventReason = EEventReason.Outside;
                            args.Data = regionID;
                            RaiseEvent(args);
                        }
                    }
                }
            }
        }
    }
}
