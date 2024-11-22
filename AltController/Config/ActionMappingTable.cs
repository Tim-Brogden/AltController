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
using AltController.Actions;
using AltController.Input;

namespace AltController.Config
{
    /// <summary>
    /// Define the lists of actions to perform for each type of event
    /// </summary>
    public class ActionMappingTable
    {
        private Dictionary<long, ActionList> _eventTypeMappings = new Dictionary<long, ActionList>();
        private long _id;

        public long ID { get { return _id; } set { _id = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionMappingTable()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionMappingTable(long id)
        {
            ID = id;
        }

        /// <summary>
        /// Configure actions for a particular event type
        /// </summary>
        /// <param name="eventTypeID"></param>
        /// <param name="actionList"></param>
        public void SetActions(long eventTypeID, ActionList actionList)
        {
            _eventTypeMappings[eventTypeID] = actionList;
        }

        /// <summary>
        /// Get the set of actions to perform for a particular event type
        /// </summary>
        /// <param name="eventTypeID"></param>
        /// <returns></returns>
        public ActionList GetActions(long eventTypeID)
        {
            ActionList actionList = null;
            if (_eventTypeMappings.ContainsKey(eventTypeID))
            {
                actionList = _eventTypeMappings[eventTypeID];
            }

            return actionList;
        }

        /// <summary>
        /// Return an enumerator for looping through action lists
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, ActionList>.Enumerator GetEnumerator()
        {
            return _eventTypeMappings.GetEnumerator();
        }

        /// <summary>
        /// Validate the events which trigger action lists
        /// </summary>
        /// <param name="profile"></param>
        public void ValidateEventTypes(Profile profile)
        {
            // Create a list of invalid event type IDs
            List<long> eventTypeIDsToDelete = new List<long>();
            Dictionary<long, ActionList>.Enumerator eEventTypes = _eventTypeMappings.GetEnumerator();
            while (eEventTypes.MoveNext())
            {
                bool isValid = true;
                ActionList actionList = eEventTypes.Current.Value;
                BaseSource inputSource = profile.GetInputSource(actionList.EventArgs.SourceID);
                if (inputSource == null)
                {
                    // Source has been deleted
                    isValid = false;
                }
                else
                {
                    EControlType controlType = actionList.EventArgs.ControlType;
                    switch (inputSource.SourceType)
                    {
                        case ESourceType.Mouse:
                            isValid = controlType == EControlType.MouseButtons || controlType == EControlType.MousePointer;
                            break;
                        case ESourceType.Keyboard:
                            isValid = controlType == EControlType.Keyboard;
                            break;
                        case ESourceType.CustomWindow:
                            isValid = controlType == EControlType.CustomButton;
                            break;
                    }

                    if (isValid)
                    {
                        if (inputSource.SourceType == ESourceType.CustomWindow)
                        {
                            // Custom window source - event's data is the ID of the custom button
                            CustomWindowSource customWindowSource = (CustomWindowSource)inputSource;
                            if (customWindowSource.CustomButtons.GetItemByID(actionList.EventArgs.Data) == null)
                            {
                                // Button has been deleted from custom window
                                isValid = false;
                            }
                        }
                        else if (inputSource.SourceType == ESourceType.Mouse && actionList.EventArgs.Data != 0)
                        {
                            if (profile.ScreenRegions.GetItemByID(actionList.EventArgs.Data) == null)
                            {
                                // Screen region has been deleted from custom window
                                isValid = false;
                            }
                        }
                    }
                }

                if (!isValid)
                {
                    eventTypeIDsToDelete.Add(eEventTypes.Current.Key);
                }
            }

            // Delete action lists with invalid event type IDs
            foreach (long eventTypeID in eventTypeIDsToDelete)
            {
                _eventTypeMappings.Remove(eventTypeID);
            }
        }

        /// <summary>
        /// Remove any invalid actions after the master lists of modes, apps and pages have been updated
        /// </summary>
        /// <param name="profile"></param>
        public void ValidateActions(Profile profile)
        {
            // Validate individual actions in each list
            foreach (ActionList actionList in _eventTypeMappings.Values)
            {
                // See if any item doesn't have corresponding details
                if (actionList != null)
                {
                    // Delete invalid items
                    List<int> itemsToDelete = new List<int>();

                    for (int i = 0; i < actionList.Count; i++)
                    {
                        BaseAction action = actionList[i];
                        if (action is ChangeModeAction)
                        {
                            long modeID = ((ChangeModeAction)action).ModeID;
                            if (modeID > -1)
                            {
                                NamedItem modeDetails = profile.GetModeDetails(modeID);
                                if (modeDetails == null)
                                {
                                    // Prepare to delete the action
                                    itemsToDelete.Add(i);
                                    continue;
                                }
                                else
                                {
                                    // Update the action name
                                    ((ChangeModeAction)action).ModeName = modeDetails.Name;
                                }
                            }
                        }
                        else if (action is ChangePageAction)
                        {
                            long pageID = ((ChangePageAction)action).PageID;
                            if (pageID > -1)
                            {
                                NamedItem pageDetails = profile.GetPageDetails(pageID);
                                if (pageDetails == null)
                                {
                                    // Prepare to delete the action
                                    itemsToDelete.Add(i);
                                    continue;
                                }
                                else
                                {
                                    // Update the action name
                                    ((ChangePageAction)action).PageName = pageDetails.Name;
                                }
                            }
                        }
                        else if (action is CustomWindowStateAction)
                        {
                            long windowID = ((CustomWindowStateAction)action).WindowID;
                            if (windowID > 0)
                            {
                                NamedItem source = profile.GetInputSource(windowID);
                                if (source is CustomWindowSource)
                                {
                                    // Update the window title
                                    ((CustomWindowStateAction)action).WindowTitle = ((CustomWindowSource)source).WindowTitle;
                                }
                                else
                                {
                                    // Prepare to delete the action
                                    itemsToDelete.Add(i);
                                    continue;
                                }
                            }
                        }

                        // Check that the parameter sources are still valid
                        //if (action.ParameterSources != null)
                        //{
                        //    foreach (ParameterSourceArgs args in action.ParameterSources)
                        //    {
                        //        if (args != null && args.ControlArgs != null)
                        //        {
                        //            BaseSource source = profile.GetInputSource(args.ControlArgs.SourceID);
                        //            if (source == null)
                        //            {
                        //                itemsToDelete.Add(i);
                        //                continue;
                        //            }
                        //        }
                        //    }
                        //}
                    }

                    // Remove actions as required
                    for (int j = itemsToDelete.Count - 1; j > -1; j--)
                    {
                        actionList.RemoveAt(itemsToDelete[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Merge default values into an action mapping table
        /// </summary>
        /// <param name="primaryTable"></param>
        /// <param name="secondaryTable"></param>
        /// <returns></returns>
        public static ActionMappingTable Combine(ActionMappingTable primaryTable, ActionMappingTable secondaryTable)
        {
            ActionMappingTable combinedTable = new ActionMappingTable();
            
            // Populate action sets from the primary values
            if (primaryTable != null)
            {
                Dictionary<long, ActionList>.Enumerator primaryEnum = primaryTable.GetEnumerator();
                while (primaryEnum.MoveNext())
                {
                    ActionList actions = primaryEnum.Current.Value;
                    if (actions != null && actions.Count > 0)
                    {
                        // Add to combined table
                        combinedTable.SetActions(primaryEnum.Current.Key, actions);
                    }
                }
            }

            // Merge in secondary actions if no primary ones are present
            if (secondaryTable != null)
            {
                Dictionary<long, ActionList>.Enumerator secondaryEnum = secondaryTable.GetEnumerator();
                while (secondaryEnum.MoveNext())
                {
                    long eventTypeID = secondaryEnum.Current.Key;
                    ActionList primaryActions = combinedTable.GetActions(eventTypeID);
                    if (primaryActions == null || primaryActions.Count == 0)
                    {
                        combinedTable.SetActions(eventTypeID, secondaryEnum.Current.Value);
                    }
                }
            }

            return combinedTable;
        }

    }
}
