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
    /// Define the actions to perform for a particular application
    /// </summary>
    public class AppMappingTable
    {
        private Dictionary<long, ActionMappingTable> _actionMappings = new Dictionary<long, ActionMappingTable>();
        private long _id;

        public long ID { get { return _id; } set { _id = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public AppMappingTable()
        {
            // Create default mappings for this app
            _actionMappings[Constants.DefaultID] = new ActionMappingTable(Constants.DefaultID);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AppMappingTable(long id)
        {
            // Create default mappings for this app
            ID = id;
            _actionMappings[Constants.DefaultID] = new ActionMappingTable(Constants.DefaultID);
        }
        
        /// <summary>
        /// Return the page mappings for the specified application
        /// </summary>
        /// <param name="pageID"></param>
        /// <returns></returns>
        public ActionMappingTable GetActionsForPage(long pageID, bool includeDefaults)
        {
            ActionMappingTable matchingTable = null;
            if (_actionMappings.ContainsKey(pageID))
            {
                // Matched existing
                matchingTable = _actionMappings[pageID];
            }

            ActionMappingTable table;
            if (includeDefaults)
            {
                // Merge in default values
                table = ActionMappingTable.Combine(matchingTable, _actionMappings[Constants.DefaultID]);
            }
            else
            {
                // Create new entry if reqd
                if (matchingTable == null)
                {
                    // Create new
                    matchingTable = new ActionMappingTable(pageID);
                    _actionMappings[pageID] = matchingTable;
                }

                // Return matching table
                table = matchingTable;
            }

            return table;
        }

        /// <summary>
        /// Return an enumerator for looping through the action mappings
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, ActionMappingTable>.Enumerator GetEnumerator()
        {
            return _actionMappings.GetEnumerator();
        }

        /// <summary>
        /// Remove any invalid pages after the profile's page list has been updated
        /// </summary>
        /// <param name="profile"></param>
        public void ValidatePages(Profile profile)
        {
            // Delete invalid items
            long itemToDelete;
            do
            {
                // See if any item doesn't have corresponding details
                itemToDelete = -1;
                foreach (long itemID in _actionMappings.Keys)
                {
                    if (profile.GetPageDetails(itemID) == null)
                    {
                        itemToDelete = itemID;
                        break;
                    }
                }

                if (itemToDelete > -1)
                {
                    // Delete item
                    _actionMappings.Remove(itemToDelete);
                }
            }
            while (itemToDelete > -1);
        }
    }

}
