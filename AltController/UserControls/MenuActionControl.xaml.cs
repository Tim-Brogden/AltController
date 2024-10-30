﻿/*
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
using System.Windows;
using System.Windows.Controls;
using AltController.Core;
using AltController.Actions;

namespace AltController.UserControls
{
    public partial class MenuActionControl : UserControl
    {
        // Members
        private NamedItemList _menuOptionListItems = new NamedItemList();
        private MenuOptionAction _currentAction = new MenuOptionAction();
        private EMainMenuOption _menuOption = EMainMenuOption.DrawStateOverlay;

        // Properties
        public EMainMenuOption MenuOption { get { return _menuOption; } set { _menuOption = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public MenuActionControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Control loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (EMainMenuOption option in Enum.GetValues(typeof(EMainMenuOption)))
            {
                if (option != EMainMenuOption.None)
                {
                    _menuOptionListItems.Add(new NamedItem((long)option, GUIUtils.MainMenuOptionToString(option)));
                }
            }
            this.MenuActionCombo.ItemsSource = _menuOptionListItems;

            if (_currentAction != null)
            {
                this.MenuActionCombo.SelectedValue = (long)_currentAction.MenuOption;
            }            
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is MenuOptionAction)
            {
                _currentAction = (MenuOptionAction)action;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem selectedItem = (NamedItem)this.MenuActionCombo.SelectedItem;
            if (selectedItem != null)
            {
                _currentAction = new MenuOptionAction();
                _currentAction.MenuOption = (EMainMenuOption)selectedItem.ID;                
            }

            return _currentAction;
        }

    }
}
