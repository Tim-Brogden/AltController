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
using System.Xml;
using System.Reflection;
using AltController.Actions;
using AltController.Core;
using AltController.Event;

namespace AltController.Config
{
    /// <summary>
    /// Container class for the list of actions to perform in a given logical state when a particular event occurs
    /// </summary>
    public class ActionList : List<BaseAction>
    {
        // Configuration
        private EActionListType _executionMode = EActionListType.Series;
        private LogicalState _logicalState;
        private AltControlEventArgs _eventArgs;
        private int _id = 0;

        // Execution
        private bool _isActive = false;
        private bool _isOngoing = false;
        private AltControlEventArgs _currentEvent;

        public EActionListType ExecutionMode { get { return _executionMode; } set { _executionMode = value; } }
        public LogicalState LogicalState { get { return _logicalState; } set { _logicalState = value; } }
        public AltControlEventArgs EventArgs { get { return _eventArgs; } set { _eventArgs = value; } }
        public int ID { get { return _id; } set { _id = value; } }
        public bool IsActive 
        { 
            get { return _isActive; } 
            set { _isActive = value; _isOngoing = false; } 
        }
        public bool IsOngoing { get { return _isOngoing; } protected set { _isOngoing = value; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ActionList()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logicalState"></param>
        /// <param name="eventID"></param>
        public ActionList(LogicalState logicalState, AltControlEventArgs args)
        {
            _logicalState = logicalState;
            _eventArgs = args;
        }

        /// <summary>
        /// Start performing the actions
        /// </summary>
        /// <param name="stateHandler"></param>
        /// <param name="args"></param>
        public void Start(IStateManager stateHandler, AltControlEventArgs args)
        {
            // Reset ongoing status
            IsOngoing = false;
            _currentEvent = args;

            // Start performing actions
            foreach (BaseAction action in this)
            {
                // Start the action
                action.StartAction(stateHandler, args);

                // See if the action needs to be continued in next cycle
                if (action.IsOngoing)
                {
                    IsOngoing = true;

                    if (_executionMode == EActionListType.Series)
                    {
                        // Can't move on to the next action yet, because this one is ongoing
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Continue the actions
        /// </summary>
        /// <param name="stateHandler"></param>
        public void Continue(IStateManager stateHandler)
        {
            // Reset ongoing status
            IsOngoing = false;
            bool foundCurrentAction = false;

            foreach (BaseAction action in this)
            {
                // Skip completed actions at the start
                if (!foundCurrentAction && !action.IsOngoing)
                {
                    continue;
                }
                foundCurrentAction = true;

                if (!action.IsOngoing)
                {
                    // Action isn't started yet
                    if (_executionMode == EActionListType.Series)
                    {
                        // Start the next action in the series
                        action.StartAction(stateHandler, _currentEvent);
                    }
                }
                else
                {
                    // Continue the current action
                    action.ContinueAction(stateHandler, _currentEvent);
                }

                // See if the action is finished yet
                if (action.IsOngoing)
                {
                    IsOngoing = true;

                    if (_executionMode == EActionListType.Series)
                    {
                        // Can't move on to the next action yet, because this one is ongoing
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Stop the actions being performed
        /// </summary>
        /// <param name="stateHandler"></param>
        public void Stop(IStateManager stateHandler)
        {
            // Stop any ongoing actions
            foreach (BaseAction action in this)
            {
                if (action.IsOngoing)
                {
                    action.StopAction(stateHandler);
                }
            }
            IsOngoing = false;
        }

        /// <summary>
        /// Read from xml
        /// </summary>
        /// <param name="actionListElement"></param>
        public void FromXml(XmlElement actionListElement)
        {
            // Read logical state
            _logicalState = new LogicalState();
            _logicalState.FromXml(actionListElement);

            // Read event args
            _eventArgs = AltControlEventArgs.FromXml(actionListElement);

            // Read execution mode
            _executionMode = (EActionListType)Enum.Parse(typeof(EActionListType), actionListElement.GetAttribute("executionmode"));

            // Read actions
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (XmlNode actionNode in actionListElement.ChildNodes)
            {
                if (actionNode is XmlElement)
                {
                    XmlElement actionElement = (XmlElement)actionNode;

                    // Read the action
                    string typeFullName = string.Format("{0}.{1}", typeof(BaseAction).Namespace, actionElement.Name);
                    if (assembly.GetType(typeFullName, false, false) != null)
                    {
                        BaseAction action = (BaseAction)assembly.CreateInstance(typeFullName);
                        if (action != null)
                        {
                            action.FromXml(actionElement);
                            this.Add(action);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Write to xml
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement parentElement, XmlDocument doc)
        {
            // Write logical state
            _logicalState.ToXml(parentElement, doc);

            // Write eventtype
            AltControlEventArgs.ToXml(_eventArgs, parentElement, doc);

            // Write execution mode
            parentElement.SetAttribute("executionmode", _executionMode.ToString());

            // Write actions
            foreach (BaseAction action in this)
            {
                XmlElement actionElement = doc.CreateElement(action.GetType().Name);
                action.ToXml(actionElement, doc);
                parentElement.AppendChild(actionElement);
            }
        }
    }
}
