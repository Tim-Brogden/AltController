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
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace AltController.Core
{
    /// <summary>
    /// Stores the security rule for a command which could be executed by a StartProgram action
    /// </summary>
    public class CommandRuleItem : INotifyPropertyChanged
    {
        // Fields
        private string _command = "";
        private ECommandAction _actionType = ECommandAction.AskMe;
        private static NamedItemList _actionChoices = new NamedItemList() 
        { new NamedItem((int)ECommandAction.Run, Properties.Resources.String_Run), 
            new NamedItem((int)ECommandAction.DontRun, Properties.Resources.String_DontRun), 
            new NamedItem((int)ECommandAction.AskMe, Properties.Resources.String_AskMe) };

        // Properties
        public string Command
        {
            get { return _command; }
            set
            {
                if (_command != value)
                {
                    // Check value provided
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentException(Properties.Resources.E_CommandEmpty);
                    }

                    // Validate regex
                    if (value.StartsWith("^"))
                    {
                        try
                        {
                            Regex regex = new Regex(value);
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException(Properties.Resources.E_InvalidRegex + " " + Properties.Resources.String_Details + ": " + ex.Message);
                        }
                    }

                    _command = value;
                    NotifyPropertyChanged("Command");
                }
            }
        }
        public ECommandAction ActionType
        {
            get { return _actionType; }
            set
            {
                if (_actionType != value)
                {
                    _actionType = value;
                    NotifyPropertyChanged("ActionType");
                }
            }
        }
        public NamedItem ActionItem
        {
            get { return _actionChoices.GetItemByID((int)_actionType); }
            set
            {
                ECommandAction actionType = (value != null) ? (ECommandAction)value.ID : ECommandAction.None;
                if (actionType != _actionType)
                {
                    _actionType = actionType;
                    NotifyPropertyChanged("ActionItem");
                }
            }
        }
        public NamedItemList Choices { get { return _actionChoices; } }

        // Events
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CommandRuleItem()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="command"></param>
        /// <param name="actionType"></param>
        public CommandRuleItem(string command, ECommandAction actionType)
        {
            _command = command;
            _actionType = actionType;
        }

        /// <summary>
        /// Notify change
        /// </summary>
        /// <param name="info"></param>
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }       
    }
}
