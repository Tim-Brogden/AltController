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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;
using AltController.Actions;
using AltController.Config;
using AltController.Core;

namespace AltController
{
    /// <summary>
    /// Window that shows a summary of the current profile
    /// </summary>
    public partial class ProfileSummaryWindow : Window
    {
        // Members
        private IParentWindow _parentWindow;
        private Profile _profile;

        /// <summary>
        /// Set the profile to display
        /// </summary>
        /// <param name="profile"></param>
        public Profile CurrentProfile
        {
            set
            {
                _profile = value;
                if (this.IsLoaded)
                {
                    DisplayProfileSummary();
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProfileSummaryWindow(IParentWindow parent)
        {
            _parentWindow = parent;

            InitializeComponent();
        }


        /// <summary>
        /// Window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_profile != null)
            {
                DisplayProfileSummary();
            }
        }

        /// <summary>
        /// Display the current profile summary
        /// </summary>
        private void DisplayProfileSummary()
        {
            ProfileSummaryGrid.Children.Clear();
            ProfileSummaryGrid.RowDefinitions.Clear();
            ProfileSummaryGrid.ColumnDefinitions.Clear();

            // Brushes
            Brush redBrush = new LinearGradientBrush(Colors.LightSalmon, Colors.DarkSalmon, 90.0);
            Brush yellowBrush = Brushes.LightYellow;
            Brush blueBrush = new LinearGradientBrush(Colors.LightBlue, Colors.RoyalBlue, 90.0);

            // Add rows
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(25);
            ProfileSummaryGrid.RowDefinitions.Add(rowDef);
            rowDef = new RowDefinition();
            rowDef.Height = new GridLength(25);
            ProfileSummaryGrid.RowDefinitions.Add(rowDef);
            foreach (NamedItem modeDetails in _profile.ModeDetails)
            {
                rowDef = new RowDefinition();
                ProfileSummaryGrid.RowDefinitions.Add(rowDef);
            }

            // Add columns
            ColumnDefinition colDef = new ColumnDefinition();
            colDef.Width = new GridLength(100);
            ProfileSummaryGrid.ColumnDefinitions.Add(colDef);
            foreach (NamedItem appDetails in _profile.AppDetails)
            {
                colDef = new ColumnDefinition();
                colDef.MinWidth = 100;
                //colDef.Width = new GridLength(280);
                ProfileSummaryGrid.ColumnDefinitions.Add(colDef);
            }

            // Add headings
            Panel panel = CreateCell(0, 0, 2, 1, blueBrush, HorizontalAlignment.Center, VerticalAlignment.Center);
            TextBlock textBlock = AddTextToCell(panel, Properties.Resources.String_Modes);
            textBlock.FontSize = 14.0;

            panel = CreateCell(0, 1, 1, _profile.AppDetails.Count, redBrush, HorizontalAlignment.Center, VerticalAlignment.Center);
            textBlock = AddTextToCell(panel, Properties.Resources.String_Apps);
            textBlock.FontSize = 14.0;

            int colIndex = 1;
            foreach (NamedItem appDetails in _profile.AppDetails)
            {
                panel = CreateCell(1, colIndex, 1, 1, yellowBrush, HorizontalAlignment.Center, VerticalAlignment.Center);
                textBlock = AddTextToCell(panel, appDetails.Name);
                textBlock.FontSize = 12.0;
                colIndex++;
            }

            // Add cells
            int rowIndex = 2;
            foreach (NamedItem modeDetails in _profile.ModeDetails)
            {
                panel = CreateCell(rowIndex, 0, 1, 1, yellowBrush, HorizontalAlignment.Center, VerticalAlignment.Center);
                textBlock = AddTextToCell(panel, modeDetails.Name);
                textBlock.FontSize = 12.0;

                colIndex = 1;
                foreach (NamedItem appDetails in _profile.AppDetails)
                {
                    panel = CreateCell(rowIndex, colIndex, 1, 1, Brushes.White, HorizontalAlignment.Left, VerticalAlignment.Top);

                    DataTable table = new DataTable();
                    table.Columns.Add(Properties.Resources.String_Input);
                    table.Columns.Add(Properties.Resources.String_Actions);

                    foreach (NamedItem pageDetails in _profile.PageDetails)
                    {
                        LogicalState state = new LogicalState(modeDetails.ID, appDetails.ID, pageDetails.ID);
                        ActionMappingTable actionMapping = _profile.GetActionsForState(state, false);
                        Dictionary<long, ActionList>.Enumerator eActionList = actionMapping.GetEnumerator();
                        int rowForPage = 0;
                        while (eActionList.MoveNext())
                        {
                            ActionList actionList = eActionList.Current.Value;

                            if (actionList.Count > 0)
                            {
                                string suffix = "";
                                if ((actionList.EventArgs.ControlType == EControlType.MousePointer || 
                                    actionList.EventArgs.ControlType == EControlType.MouseButtons)
                                    && actionList.EventArgs.Data != 0)
                                {
                                    ScreenRegion region = (ScreenRegion)_profile.ScreenRegions.GetItemByID(actionList.EventArgs.Data);
                                    if (region != null)
                                    {
                                        suffix = string.Format(" '{0}'", region.Name);
                                    }
                                }
                                string eventName = string.Format("{0} {1}{2}",
                                                                    actionList.EventArgs.GetControlTypeName(),
                                                                    actionList.EventArgs.GetStateName(),
                                                                    suffix);

                                StringBuilder sb = new StringBuilder();
                                int index = 0;
                                foreach (BaseAction action in actionList)
                                {
                                    if (index++ > 0)
                                    {
                                        sb.AppendLine();
                                    }
                                    sb.Append(action.ShortName);
                                }

                                // Separate pages with extra row
                                if (pageDetails.ID != Constants.DefaultID && rowForPage++ == 0)
                                {
                                    DataRow separatorRow = table.NewRow();
                                    separatorRow[0] = "--- " + pageDetails.Name + " ---";
                                    separatorRow[1] = "--- " + pageDetails.Name + " ---";
                                    table.Rows.Add(separatorRow);
                                }

                                DataRow row = table.NewRow();
                                row[0] = eventName;
                                row[1] = sb.ToString();
                                table.Rows.Add(row);
                            }
                        }                        
                    }

                    if (table.Rows.Count > 0)
                    {
                        AddTableToCell(panel, table);
                    }

                    colIndex++;
                }

                rowIndex++;
            }
        }

        /// <summary>
        /// Create a panel in a grid cell 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="background"></param>
        /// <returns></returns>
        private Panel CreateCell(int row, 
                                int col, 
                                int rowSpan,
                                int colSpan,
                                Brush background, 
                                HorizontalAlignment horizAlign, 
                                VerticalAlignment vertAlign)
        {
            Border border = new Border();
            border.Background = background;
            border.BorderBrush = Brushes.Black;
            border.BorderThickness = new Thickness(1);

            StackPanel panel = new StackPanel();
            panel.HorizontalAlignment = horizAlign;
            panel.VerticalAlignment = vertAlign;
            border.Child = panel;

            ProfileSummaryGrid.Children.Add(border);
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            if (rowSpan > 1)
            {
                Grid.SetRowSpan(border, rowSpan);
            }
            if (colSpan > 1)
            {
                Grid.SetColumnSpan(border, colSpan);
            }

            return panel;
        }

        /// <summary>
        /// Set the content of a cell
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="text"></param>
        /// <param name="horizAlign"></param>
        /// <param name="vertAlign"></param>
        /// <returns></returns>
        private TextBlock AddTextToCell(Panel parent, string text)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Text = text;
            parent.Children.Add(textBlock);

            return textBlock;
        }

        /// <summary>
        /// Add a table to a grid cell
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="table"></param>
        private void AddTableToCell(Panel parent, DataTable table)
        {
            DataGrid dataGrid = new DataGrid();
            dataGrid.IsReadOnly = true;
            dataGrid.ItemsSource = table.DefaultView;
            parent.Children.Add(dataGrid);
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_parentWindow != null)
            {
                _parentWindow.ChildWindowClosing(this);
            }
        }

        /// <summary>
        /// Close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
