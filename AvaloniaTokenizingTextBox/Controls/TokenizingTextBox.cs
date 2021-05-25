using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using System;
using System.Collections;

namespace AvaloniaTokenizingTextBox.Controls
{
    /// <summary>
    /// Class TokenizingTextBox.
    /// <para>Implements <see cref="Avalonia.Controls.ListBox" /></para>
    /// </summary>
    /// <seealso cref="Avalonia.Controls.ListBox" />
    public class TokenizingTextBox : ListBox
    {
        #region Private Fields

        private const string PART_TextBox = "PART_TextBox";
        private const string PART_WrapPanel = "PART_WrapPanel";
        private TokenTextBox? _textBox;
        private WrapPanel? _wrapPanel;

        /// <summary>
        /// The default value for the <see cref="ItemsControl.ItemsPanel"/> property.
        /// </summary>
        private static readonly FuncTemplate<IPanel> DefaultPanel =
            new(() => new StackPanel() { Orientation = Orientation.Horizontal });

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Defines the <see cref="InputText"/> property.
        /// </summary>
        public static readonly StyledProperty<string> InputTextProperty =
        AvaloniaProperty.Register<TokenizingTextBox, string>(nameof(InputText));

        /// <summary>
        /// Defines the <see cref="TokenDelimiterProperty"/> property.
        /// </summary>
        public static readonly StyledProperty<string> TokenDelimiterProperty =
        AvaloniaProperty.Register<TokenizingTextBox, string>(nameof(TokenDelimiter));

        public string InputText
        {
            get => GetValue(InputTextProperty);
            set => SetValue(InputTextProperty, value);
        }

        public string TokenDelimiter
        {
            get => GetValue(TokenDelimiterProperty);
            set => SetValue(TokenDelimiterProperty, value);
        }

        #endregion Public Properties

        #region Constructors

        static TokenizingTextBox()
        {
            ItemsPanelProperty.OverrideDefaultValue<TokenizingTextBox>(DefaultPanel);
        }

        #endregion

        #region Protected Methods

        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            return new ItemContainerGenerator<TokenizingTextBoxItem>(
                this,
                TokenizingTextBoxItem.ContentProperty,
                TokenizingTextBoxItem.ContentTemplateProperty);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_textBox != null && _wrapPanel != null)
            {
                _textBox.RemoveHandler(TextInputEvent, TextBox_TextChanged);
                _textBox.MyKeyDown -= TextBox_KeyDown;
                _textBox.GotFocus -= TextBox_GotFocus;
                _wrapPanel.KeyDown -= WrapPanel_KeyDown;
            }

            _textBox = (TokenTextBox)e.NameScope.Get<Control>(PART_TextBox);
            _wrapPanel = (WrapPanel)e.NameScope.Get<Control>(PART_WrapPanel);

            if (_textBox != null && _wrapPanel != null)
            {
                _textBox.AddHandler(TextInputEvent, TextBox_TextChanged, RoutingStrategies.Tunnel);
                _textBox.MyKeyDown += TextBox_KeyDown;
                _textBox.GotFocus += TextBox_GotFocus;
                _wrapPanel.KeyDown += WrapPanel_KeyDown;
            }
        }

        #endregion Protected Methods

        #region Public Methods

        public void DeleteSelected()
        {
            if (SelectedItem == null) return;
            int index = IndexOf(Items, SelectedItem);
            (Items as IList)?.RemoveAt(index);
            _textBox?.Focus();
        }

        #endregion Public Methods

        #region Private Methods

        private void AddToken(string token)
        {
            if (token.Length > 0)
                (Items as IList)?.Add(token);
        }


        private void TextBox_GotFocus(object? sender, GotFocusEventArgs e) => SelectedIndex = -1;

        private void TextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            int currentCursorPosition = _textBox.SelectionStart;
            int selectionLength = currentCursorPosition + _textBox.SelectionEnd;
            switch (e.Key)
            {
                case Key.Back when currentCursorPosition == 0 && selectionLength == 0 && ItemCount > 0:
                    e.Handled = true;
                    var container = ItemContainerGenerator.ContainerFromIndex(ItemCount - 1);
                    if (container is TokenizingTextBoxItem element)
                    {
                        element.Focus();
                        SelectedIndex = ItemCount - 1;
                    }
                    e.Handled = true;
                    break;
            }
        }

        private void TextBox_TextChanged(object? sender, TextInputEventArgs e)
        {
            string text = _textBox.Text;

            if (!string.IsNullOrEmpty(TokenDelimiter) && e.Text.Contains(TokenDelimiter)) //t.Contains(TokenDelimiter))
            {
                string t = text + e.Text;

                bool lastDelimited = t[t.Length - 1] == TokenDelimiter[0];

                string[] tokens = t.Split(new[] { TokenDelimiter }, StringSplitOptions.RemoveEmptyEntries);
                int numberToProcess = lastDelimited ? tokens.Length : tokens.Length - 1;
                for (int position = 0; position < numberToProcess; position++)
                    AddToken(tokens[position]);

                if (lastDelimited)
                {
                    _textBox.Text = string.Empty;
                }
                else
                {
                    _textBox.Text = tokens[tokens.Length - 1];
                    _textBox.CaretIndex = _textBox.Text.Length;
                }
                e.Handled = true; //handle the event so the delimiter doesn't display
            }
        }

        private void WrapPanel_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Back when ItemCount > 0:
                case Key.Delete when ItemCount > 0:
                    if (SelectedItem == null)
                        break;
                    int index = IndexOf(Items, SelectedItem);
                    (Items as IList)?.RemoveAt(index);
                    _textBox?.Focus();
                    break;
            }
        }

        #endregion Private Methods
    }
}