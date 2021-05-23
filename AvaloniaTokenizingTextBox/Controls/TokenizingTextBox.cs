using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AvaloniaTokenizingTextBox.Controls
{
    public class TokenizingTextBox : ListBox
    {
        private const string PART_TextBox = "PART_TextBox";
        private const string PART_WrapPanel = "PART_WrapPanel";
        private TokenTextBox? _textBox;
        private WrapPanel? _wrapPanel;

        public static readonly StyledProperty<string> InputTextProperty =
        AvaloniaProperty.Register<TokenizingTextBox, string>(nameof(InputText));

        public static readonly StyledProperty<string> TokenDelimiterProperty =
        AvaloniaProperty.Register<TokenizingTextBox, string>(nameof(TokenDelimiter));

        public static readonly DirectProperty<TokenizingTextBox, ObservableCollection<TokenizingTextBoxItem>> TokensProperty =
            AvaloniaProperty.RegisterDirect<TokenizingTextBox, ObservableCollection<TokenizingTextBoxItem>>(nameof(Tokens), o => o.Tokens, (o, v) => o.Tokens = v);

        private ObservableCollection<TokenizingTextBoxItem> _tokens = new();

        [Content]
        public ObservableCollection<TokenizingTextBoxItem> Tokens
        {
            get { return _tokens; }
            private set { SetAndRaise(TokensProperty, ref _tokens, value); }
        }

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

        public TokenizingTextBox()
        {
        }

        private void Tokens_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Items = Tokens;

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
                Tokens.CollectionChanged -= Tokens_CollectionChanged;
                //_textBox.KeyDown -= TextBox_KeyDown;
                _textBox.RemoveHandler(TextInputEvent, TextBox_TextChanged);
                _textBox.MyKeyDown -= TextBox_KeyDown;
                _textBox.GotFocus -= TextBox_GotFocus;
                _wrapPanel.KeyDown -= WrapPanel_KeyDown;
                //_textBox.TextInput -= TextBox_TextChanged; //TextInput doesn't work?
            }

            _textBox = (TokenTextBox)e.NameScope.Get<Control>(PART_TextBox);
            _wrapPanel = (WrapPanel)e.NameScope.Get<Control>(PART_WrapPanel);

            if (_textBox != null && _wrapPanel != null)
            {
                Tokens.CollectionChanged += Tokens_CollectionChanged;
                //_textBox.KeyDown += TextBox_KeyDown; //Backspace isn't detected if Caret position is at beginning?
                _textBox.AddHandler(TextInputEvent, TextBox_TextChanged, RoutingStrategies.Tunnel);
                _textBox.MyKeyDown += TextBox_KeyDown;
                _textBox.GotFocus += TextBox_GotFocus;
                _wrapPanel.KeyDown += WrapPanel_KeyDown;
                //_textBox.TextInput += TextBox_TextChanged; //TextInput doesn't work?
            }
        }

        private void TextBox_GotFocus(object? sender, GotFocusEventArgs e) => SelectedIndex = -1;

        private void WrapPanel_KeyDown(object? sender, KeyEventArgs e)
        {
            int currentCursorPosition = _textBox.SelectionStart;
            int itemsCount = ItemsCount(Items);

            switch (e.Key)
            {
                case Key.Back when itemsCount > 0:
                case Key.Delete when itemsCount > 0:
                    if (SelectedItem == null) break;
                    int index = _tokens.IndexOf((TokenizingTextBoxItem)SelectedItem);
                    Tokens.RemoveAt(index);
                    break;
            }
        }

        private void TextBox_TextChanged(object? sender, TextInputEventArgs e)
        {
            string text = _textBox.Text;

            if (text == null) return;

            if (!string.IsNullOrEmpty(TokenDelimiter) && e.Text.Contains(TokenDelimiter)) //t.Contains(TokenDelimiter))
            {
                string t = text + e.Text;

                bool lastDelimited = t[t.Length - 1] == TokenDelimiter[0];

                string[] tokens = t.Split(new[] { TokenDelimiter }, StringSplitOptions.RemoveEmptyEntries);
                int numberToProcess = lastDelimited ? tokens.Length : tokens.Length - 1;
                for (int position = 0; position < numberToProcess; position++)
                {
                    AddToken(tokens[position]);
                }

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

        private void TextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            int currentCursorPosition = _textBox.SelectionStart;
            int selectionLength = currentCursorPosition + _textBox.SelectionEnd;
            int itemsCount = ItemsCount(Items);
            switch (e.Key)
            {
                case Key.Back when currentCursorPosition == 0 && selectionLength == 0 && itemsCount > 0:
                    e.Handled = true;
                    var container = ItemContainerGenerator.ContainerFromIndex(itemsCount - 1);
                    if (container is TokenizingTextBoxItem element)
                    {
                        element.Focus();
                        //_selectedToken = element;
                        SelectedIndex = itemsCount - 1;
                    }
                    e.Handled = true;
                    break;
            }
        }

        private void AddToken(string token)
        {
            if (token.Length > 0)
            {
                var newToken = new TokenizingTextBoxItem() { Content = token };
                Tokens.Add(newToken);
            }
        }

        private static int ItemsCount(IEnumerable items)
        {
            int count = 0;
            foreach (var item in items)
                count++;

            return count;
        }
    }
}