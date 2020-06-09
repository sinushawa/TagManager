﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TagManager
{
    /// <summary>
    /// Interaction logic for EditableTextBlock.xaml
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {

        #region Constructor

        public EditableTextBlock()
        {
            InitializeComponent();
            base.Focusable = true;
            base.FocusVisualStyle = null;
        }

        #endregion Constructor

        #region Member Variables

        // We keep the old text when we go into editmode
        // in case the user aborts with the escape key
        public string oldText;

        #endregion Member Variables

        #region Properties

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof(string), typeof(EditableTextBlock), new PropertyMetadata(""));

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }
        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register( "IsEditable", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(true));

        public bool IsInEditMode
        {
            get
            {   return (bool)GetValue(IsInEditModeProperty); }
            set
            { SetValue(IsInEditModeProperty, value); }
        }

        public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register( "IsInEditMode", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(false));

        public string TextFormat
        {
            get { return (string)GetValue(TextFormatProperty); }
            set
            {
                if (value == "") value = "{0}";
                SetValue(TextFormatProperty, value);
            }
        }

        public static readonly DependencyProperty TextFormatProperty = DependencyProperty.Register( "TextFormat", typeof(string), typeof(EditableTextBlock), new PropertyMetadata("{0}"));

        public string FormattedText
        {
            get { return String.Format(TextFormat, Text); }
        }

        public static readonly DependencyProperty   IsShortcutProperty = DependencyProperty.Register("IsShortcut", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(false));

        public bool IsShortcut
        {
            get { return (bool)GetValue(FontStyleProperty); }
            set
            {
                SetValue(FontStyleProperty, value);
            }
        }
        #endregion Properties

        #region Event Handlers

        // Invoked when we enter edit mode.
        void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;

            // Give the TextBox input focus
            txt.Focus();

            txt.SelectAll();
        }

        // Invoked when we exit edit mode.
        void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.IsInEditMode = false;
        }

        // Invoked when the user edits the annotation.
        void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Text != "")
                {
                    this.IsInEditMode = false;
                    e.Handled = true;
                }
                else
                {
                    this.IsInEditMode = false;
                    Text = oldText;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                this.IsInEditMode = false;
                Text = oldText;
                e.Handled = true;
            }
        }

        #endregion Event Handlers

    }
}
