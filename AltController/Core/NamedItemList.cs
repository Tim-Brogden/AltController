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
using System.Collections.ObjectModel;

namespace AltController.Core
{
    public class NamedItemList : ObservableCollection<NamedItem>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NamedItemList()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public NamedItemList(NamedItemList list)
        {
            if (list != null)
            {
                foreach (NamedItem item in list)
                {
                    Add(new NamedItem(item.ID, item.Name));
                }
            }
        }

        /// <summary>
        /// Get the item with a particular ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public NamedItem GetItemByID(long id)
        {
            NamedItem matchedItem = null;
            foreach (NamedItem item in this)
            {
                if (item.ID == id)
                {
                    matchedItem = item;
                    break;
                }
            }

            return matchedItem;
        }

        /// <summary>
        /// Find the first item with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public NamedItem GetItemByName(string name)
        {
            NamedItem matchedItem = null;
            foreach (NamedItem item in this)
            {
                if (item.Name.Equals(name))
                {
                    matchedItem = item;
                    break;
                }
            }

            return matchedItem;
        }

        /// <summary>
        /// Find the ID of the next item after the item with the specified id
        /// </summary>
        /// <param name="detailsList"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public long FindNextID(long id)
        {
            long nextID = id;

            // Find the specified ID in the list
            bool foundItem = false;
            foreach (NamedItem details in this)
            {
                if (foundItem)
                {
                    // Looking for the next item with a positive ID i.e. don't return default item
                    if (details.ID > -1)
                    {
                        nextID = details.ID;
                        break;
                    }
                }
                else if (details.ID == id)
                {
                    // Found specified ID
                    foundItem = true;
                }
                else if (details.ID > 0 && nextID == id)
                {
                    // Store the first positive ID in the list in case we need to wrap round
                    nextID = details.ID;
                }
            }

            return nextID;
        }

        /// <summary>
        /// Find the ID of the item before the item with the specified id
        /// </summary>
        /// <param name="detailsList"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public long FindPreviousID(long id)
        {
            long previousID = id;

            foreach (NamedItem details in this)
            {
                if (details.ID == id)
                {
                    // Found specified ID
                    // If we have found a positive previous ID then break,
                    // otherwise continue so that we find the last positive ID in the list (i.e. wrap round)
                    if (previousID != id)
                    {
                        break;
                    }
                }
                else if (details.ID > -1)
                {
                    // Found a positive ID so store
                    previousID = details.ID;
                }
            }

            return previousID;
        }

        /// <summary>
        /// Get a new ID
        /// </summary>
        /// <returns></returns>
        public long GetFirstUnusedID()
        {
            bool exists;
            long id = 1;
            do
            {
                exists = false;
                foreach (NamedItem item in this)
                {
                    if (item.ID == id)
                    {
                        exists = true;
                        id++;
                        break;
                    }
                }
            }
            while (exists);

            return id;
        }

        /// <summary>
        /// Get a new name
        /// </summary>
        /// <param name="stem"></param>
        /// <returns></returns>
        public string GetFirstUnusedName(string stem, bool allowStemAsName)
        {
            bool exists;
            string name;
            int count = 1;
            name = allowStemAsName ? stem : stem + "1";
            do
            {
                exists = false;
                foreach (NamedItem item in this)
                {
                    if (item.Name == name)
                    {
                        exists = true;
                        count++;
                        name = stem + count.ToString();
                        break;
                    }
                }
            }
            while (exists);

            return name;
        }
    }
}
