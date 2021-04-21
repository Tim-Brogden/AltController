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
using AltController.Core;

namespace AltController.Config
{
    /// <summary>
    /// Table defining the configuration of actions for a particular logical mode
    /// </summary>
    public class ModeMappingTable
    {
        private Dictionary<long, AppMappingTable> _appMappings = new Dictionary<long, AppMappingTable>();
        private long _id;

        public long ID { get { return _id; } set { _id = value;} }

        /// <summary>
        /// Constructor
        /// </summary>
        public ModeMappingTable()
        {
            // Create mappings for default app
            _appMappings[Constants.DefaultID] = new AppMappingTable(Constants.DefaultID);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ModeMappingTable(long id)
        {
            // Create mappings for default app
            ID = id;
            _appMappings[Constants.DefaultID] = new AppMappingTable(Constants.DefaultID);
        }

        /// <summary>
        /// Get the action mappings table for a particular logical state
        /// Creates an empty action mappings table if no actions are currently mapped
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionMappingTable GetActionsForState(LogicalState state, bool includeDefaults)
        {
            ActionMappingTable matchingTable = null;
            if (_appMappings.ContainsKey(state.AppID))
            {
                matchingTable = _appMappings[state.AppID].GetActionsForPage(state.PageID, includeDefaults);
            }

            ActionMappingTable table;
            if (includeDefaults)
            {
                // Merge in default actions
                ActionMappingTable defaultActions = _appMappings[Constants.DefaultID].GetActionsForPage(state.PageID, includeDefaults);
                table = ActionMappingTable.Combine(matchingTable, defaultActions);
            }
            else
            {
                // Create table if not present
                if (matchingTable == null)
                {
                    AppMappingTable appMapping = new AppMappingTable(state.AppID);
                    _appMappings[state.AppID] = appMapping;
                    matchingTable = appMapping.GetActionsForPage(state.PageID, includeDefaults);
                }

                table = matchingTable;
            }

            return table;
        }

        /// <summary>
        /// Return an enumerator for looping through app mappings
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, AppMappingTable>.Enumerator GetEnumerator()
        {
            return _appMappings.GetEnumerator();
        }

        /// <summary>
        /// Remove any invalid apps
        /// </summary>
        /// <param name="profile"></param>
        public void ValidateApps(Profile profile)
        {
            // Delete invalid items
            long itemToDelete;
            do
            {
                // See if any item doesn't have corresponding details
                itemToDelete = -1;
                foreach (long itemID in _appMappings.Keys)
                {
                    if (profile.GetAppDetails(itemID) == null)
                    {
                        itemToDelete = itemID;
                        break;
                    }
                }

                if (itemToDelete > -1)
                {
                    // Delete item
                    _appMappings.Remove(itemToDelete);
                }
            }
            while (itemToDelete > -1);
        }
    }
    
}
