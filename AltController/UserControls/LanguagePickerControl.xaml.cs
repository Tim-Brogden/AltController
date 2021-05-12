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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AltController.Config;

namespace AltController.UserControls
{
    /// <summary>
    /// Language picker
    /// </summary>
    public partial class LanguagePickerControl : UserControl
    {
        // Fields
        private List<LanguageItem> _languageItems;

        // Properties
        public string SelectedLanguage
        {
            get { return (string)GetValue(SelectedLanguageProperty); }
            set { SetValue(SelectedLanguageProperty, value); }
        }
        private static readonly DependencyProperty SelectedLanguageProperty =
            DependencyProperty.Register(
            "SelectedLanguage",
            typeof(string),
            typeof(LanguagePickerControl),
            new FrameworkPropertyMetadata(SelectedLanguagePropertyChanged)
        );
        public List<LanguageItem> LanguageItems { get { return _languageItems; } }

        // Routed events
        public static readonly RoutedEvent SelectedLanguageChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectedLanguageChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LanguagePickerControl));
        public event RoutedEventHandler SelectedLanguageChanged
        {
            add { AddHandler(SelectedLanguageChangedEvent, value); }
            remove { RemoveHandler(SelectedLanguageChangedEvent, value); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public LanguagePickerControl()
        {
            PopulateLanguageList();

            InitializeComponent();

            LanguageCombo.DataContext = this;
        }

        /// <summary>
        /// Design mode changed
        /// </summary>
        private static void SelectedLanguagePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            LanguagePickerControl control = source as LanguagePickerControl;
            if (control.IsLoaded)
            {
                control.RaiseEvent(new RoutedEventArgs(SelectedLanguageChangedEvent));
            }
        }

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Make sure a colour is selected
            if (SelectedLanguage == null && _languageItems.Count != 0)
            {
                SelectedLanguage = _languageItems[0].LanguageCode;
            }
        }

        /// <summary>
        /// Populate the list of languages
        /// </summary>
        private void PopulateLanguageList()
        {
            _languageItems = new List<LanguageItem>();
            Dictionary<string, string>.Enumerator eLang = AppConfig.SupportedLanguages.GetEnumerator();
            while (eLang.MoveNext())
            {
                _languageItems.Add(new LanguageItem(eLang.Current.Key, eLang.Current.Value));
            }
        }
    }

    /// <summary>
    /// Data for an item in the language picker combo box
    /// </summary>
    public class LanguageItem 
    {
        public string LanguageCode { get; set; }
        public string Name { get; set; }
        public ImageSource Image { get; set; }

        public LanguageItem(string languageCode, string name)
        {
            LanguageCode = languageCode;
            Name = name;
            try
            {
                Image = new BitmapImage(new Uri("pack://application:,,,/AltController;component/Images/lang-icons/" + languageCode + ".png"));
            }
            catch (Exception)
            {
                Image = null;
            }
        }
        
    }
}
