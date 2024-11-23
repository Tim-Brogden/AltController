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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using AltController.Actions;
using AltController.Core;

namespace AltController.UserControls
{
    /// <summary>
    /// Control for editing Start program actions
    /// </summary>
    public partial class StartProgramControl : UserControl
    {
        // Fields
        private StartProgramAction _currentAction = new StartProgramAction();
        private Window _parentWindow;

        // Properties
        public Window ParentWindow { set { _parentWindow = value; } }

        public StartProgramControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshDisplay();
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is StartProgramAction)
            {
                _currentAction = (StartProgramAction)action;
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Display the current action
        /// </summary>
        private void RefreshDisplay()
        {
            if (IsLoaded && _currentAction != null)
            {
                ProgramNameTextBox.Text = _currentAction.ProgramName;
                ProgramFolderTextBox.Text = _currentAction.ProgramFolder;
                ProgramArgsTextBox.Text = _currentAction.ProgramArgs;
                CheckIfRunningCheckBox.IsChecked = _currentAction.CheckIfRunning;
            }
        }

        /// <summary>
        /// Let the user browse to a program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseProgramButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.AddExtension = true;
            dialog.CheckFileExists = true;
            dialog.DefaultExt = ".exe";
            dialog.Filter = Properties.Resources.String_ExecutableFiles + "|*.bat; *.bin; *.cmd; *.exe; *.lnk";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            dialog.Multiselect = false;
            dialog.RestoreDirectory = true;
            dialog.ShowReadOnly = false;
            dialog.Title = Properties.Resources.StartProgram_ChooseProgramToolTip;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo fi = new FileInfo(dialog.FileName);
                ProgramNameTextBox.Text = fi.Name;
                ProgramFolderTextBox.Text = fi.DirectoryName;
                ProgramFolderTextBox.ToolTip = fi.DirectoryName;
            }
        }

        /// <summary>
        /// Browse folder clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = Properties.Resources.String_ChooseAFolder;
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            dialog.ShowNewFolderButton = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProgramFolderTextBox.Text = dialog.SelectedPath;
                ProgramFolderTextBox.ToolTip = ProgramFolderTextBox.Text != "" ? ProgramFolderTextBox.Text : null;
            }
        }

        /// <summary>
        /// Browse document clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.AddExtension = false;
            dialog.CheckFileExists = false;
            dialog.Filter = Properties.Resources.String_AllFiles + "|*.*";
            dialog.Multiselect = true;
            dialog.ShowReadOnly = false;
            dialog.Title = Properties.Resources.String_ChooseFilesOrDocuments;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                bool isFirst = true;
                foreach (string fileName in dialog.FileNames)
                {
                    if (!isFirst)
                    {
                        sb.Append(' ');
                    }
                    isFirst = false;
                    sb.Append('\"');
                    sb.Append(fileName);
                    sb.Append('\"');
                }
                ProgramArgsTextBox.Text = sb.ToString();
                ProgramArgsTextBox.ToolTip = ProgramArgsTextBox.Text != "" ? ProgramArgsTextBox.Text : null;
            }
        }
        
        /// <summary>
        /// Open a dialog to let the user check the program data they have entered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestActionButton_Click(object sender, RoutedEventArgs e)
        {
            string message;
            StartProgramAction action = (StartProgramAction)GetCurrentAction();
            CustomMessageBox messageBox;
            if (action != null)
            {
                message = string.Format(Properties.Resources.Q_TestCommandMessage, 
                                        Environment.NewLine, 
                                        action.GetCommandLine(),
                                        action.CheckIfRunning ? Properties.Resources.String_IfNotRunningOption + " " : "");
                messageBox = new CustomMessageBox(_parentWindow, message, Properties.Resources.Q_TestCommand, MessageBoxButton.YesNoCancel, true, false);
                messageBox.ShowDialog();
                if (messageBox.Result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool detectedAlreadyRunning = false;
                        if (action.CheckIfRunning)
                        {
                            detectedAlreadyRunning = ProcessManager.IsRunning(action.ProgramName);
                        }

                        if (detectedAlreadyRunning)
                        {
                            messageBox = new CustomMessageBox(_parentWindow, Properties.Resources.String_ProgramAlreadyRunningMessage, Properties.Resources.String_AlreadyRunning, MessageBoxButton.OK, true, false);
                            messageBox.ShowDialog();
                        }
                        else
                        {
                            ProcessManager.Start(action);
                            messageBox = new CustomMessageBox(_parentWindow, Properties.Resources.String_CommandSucceededMessage, Properties.Resources.String_CommandSucceeded, MessageBoxButton.OK, true, false);
                            messageBox.ShowDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        message = Properties.Resources.String_CommandErrorMessage + Environment.NewLine + ex.Message;
                        messageBox = new CustomMessageBox(_parentWindow, message, Properties.Resources.String_CommandError, MessageBoxButton.OK, true, false);
                        messageBox.ShowDialog();
                    }
                }
            }
            else
            {
                message = Properties.Resources.String_InvalidSettingsMessage;
                messageBox = new CustomMessageBox(_parentWindow, message, Properties.Resources.String_InvalidSettings, MessageBoxButton.OK, true, false);
                messageBox.ShowDialog();
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            string programName = this.ProgramNameTextBox.Text.Trim();
            string programFolder = this.ProgramFolderTextBox.Text.Trim();
            string programArgs = this.ProgramArgsTextBox.Text.Trim();
            if (programName != "" || programFolder != "" || programArgs != "")
            {
                _currentAction = new StartProgramAction();
                _currentAction.ProgramName = programName;
                _currentAction.ProgramFolder = programFolder;
                _currentAction.ProgramArgs = programArgs;
                _currentAction.CheckIfRunning = CheckIfRunningCheckBox.IsChecked == true;
            }
            else
            {
                _currentAction = null;
            }

            return _currentAction;
        }

        /// <summary>
        /// Folder text box lost kb focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgramFolderTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ProgramFolderTextBox.ToolTip = ProgramFolderTextBox.Text != "" ? ProgramFolderTextBox.Text : null;
        }

        /// <summary>
        /// Program args text box lost kb focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgramArgsTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ProgramArgsTextBox.ToolTip = ProgramArgsTextBox.Text != "" ? ProgramArgsTextBox.Text : null;
        }

    }
}
