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
using System.Windows;
using System.Windows.Controls;
using AltController.Actions;

namespace AltController.UserControls
{
    public partial class TypeTextControl : UserControl
    {
        // Members
        private TypeTextAction _currentAction = new TypeTextAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public TypeTextControl()
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
            if (_currentAction != null)
            {
                this.TextToTypeTextBox.Text = _currentAction.TextToType;
            }
            else
            {
                this.TextToTypeTextBox.Text = Properties.Resources.String_Enter_text;
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is TypeTextAction)
            {
                _currentAction = (TypeTextAction)action;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            if (this.TextToTypeTextBox.Text != "" && this.TextToTypeTextBox.Text != Properties.Resources.String_Enter_text)
            {
                _currentAction = new TypeTextAction();
                _currentAction.TextToType = this.TextToTypeTextBox.Text;
            }

            return _currentAction;
        }

    }
}
