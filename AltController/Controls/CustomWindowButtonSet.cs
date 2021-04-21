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
using AltController.Core;
using AltController.Config;
using AltController.Input;
using AltController.Event;

namespace AltController.Controls
{
    public class CustomWindowButtonSet : BaseControl
    {
        // Members
        private NamedItemList _customButtonData = new NamedItemList();
        private Dictionary<byte, DateTime> _isInside = new Dictionary<byte, DateTime>();
        private int[] _mappingCount;
        private Dictionary<byte, bool> _hasDwelled = new Dictionary<byte, bool>();
        private int _dwellTimeMS = Constants.DefaultDwellTimeMS;

        // Properties
        public NamedItemList CustomButtons { get { return _customButtonData; } set { _customButtonData = value; } }

        /// <summary>
        /// Get the supported event reasons
        /// </summary>
        public override List<EEventReason> SupportedEventReasons
        {
            get 
            { 
                return new List<EEventReason> 
                            { 
                                EEventReason.Pressed, 
                                EEventReason.Released,
                                EEventReason.Dwelled, 
                                EEventReason.Inside, 
                                EEventReason.Outside 
                            }; 
            }
        }        

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomWindowButtonSet(BaseSource parent)
            :base(parent)
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
            _mappingCount[(int)args.EventReason] += enable ? 1 : -1;
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
        /// Handle a button event generated in the custom window
        /// </summary>
        /// <param name="args"></param>
        public override void ReceiveExternalEvent(AltControlEventArgs args)
        {
            switch (args.EventReason)
            {
                case EEventReason.Pressed:
                    if (IsMapped(EEventReason.Pressed))
                    {
                        RaiseEvent(args);
                    }
                    break;
                case EEventReason.Released:
                    // Raise Released event whenever Pressed is mapped
                    // to enable auto stopping of ongoing Pressed actions on release.
                    if (IsMapped(EEventReason.Released) || IsMapped(EEventReason.Pressed))
                    {
                        RaiseEvent(args);
                    }
                    break;
                case EEventReason.Inside:
                    // Inside button area
                    _isInside[args.Data] = DateTime.Now;
                    if (IsMapped(EEventReason.Inside))
                    {
                        RaiseEvent(args);
                    }
                    break;
                case EEventReason.Outside:
                    // Outside button area
                    if (_isInside.ContainsKey(args.Data))
                    {
                        _isInside.Remove(args.Data);
                    }
                    // Reset dwell
                    if (_hasDwelled.ContainsKey(args.Data))
                    {
                        _hasDwelled.Remove(args.Data);
                    }
                    // Raise Outside event whenever Inside is mapped
                    // to enable auto stopping of ongoing Inside actions on leaving the button area.
                    if (IsMapped(EEventReason.Outside) || IsMapped(EEventReason.Inside))
                    {
                        RaiseEvent(args);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Check for dwell events for custom buttons
        /// </summary>
        /// <param name="newState"></param>
        public override void UpdateState(IStateManager stateManager)
        {
            if (IsMapped(EEventReason.Dwelled) && _isInside.Count > 0)
            {
                DateTime now = DateTime.Now;
                Dictionary<byte, DateTime>.Enumerator eCustomButton = _isInside.GetEnumerator();
                while (eCustomButton.MoveNext())
                {
                    // See if the dwell is long enough, and event not already raised
                    if (!_hasDwelled.ContainsKey(eCustomButton.Current.Key) &&
                        (now - eCustomButton.Current.Value).TotalMilliseconds > _dwellTimeMS)
                    {
                        // Dwelled over button
                        _hasDwelled[eCustomButton.Current.Key] = true;
                        AltControlEventArgs args = new AltControlEventArgs(Parent.ID,
                            EControlType.CustomButton,
                            ESide.None, 
                            0, 
                            ELRUDState.None, 
                            EEventReason.Dwelled);
                        args.Data = eCustomButton.Current.Key;
                        RaiseEvent(args);
                    }
                }
            }
        }

    }
}
