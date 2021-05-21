using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.ObjectModel;

namespace AvaloniaTokenizingTextBox.Controls
{
    public class TokenizingTextBox : ItemsControl
    {
        private const string PART_TextBox = "PART_TextBox";
        private const string PART_WrapPanel = "PART_WrapPanel";
        private TextBox? _textBox;
        private WrapPanel? _wrapPanel;

        public static readonly StyledProperty<string> InputTextProperty =
        AvaloniaProperty.Register<TokenizingTextBox, string>(nameof(InputText));

        public static readonly StyledProperty<string> TokenDelimiterProperty =
        AvaloniaProperty.Register<TokenizingTextBox, string>(nameof(TokenDelimiter));

        public static readonly DirectProperty<TokenizingTextBox, ObservableCollection<TokenizingTextBoxItem>> TokenItemsProperty =
                        AvaloniaProperty.RegisterDirect<TokenizingTextBox, ObservableCollection<TokenizingTextBoxItem>>(
                        nameof(TokenItems),
                        o => o.TokenItems);

        private ObservableCollection<TokenizingTextBoxItem> _tokenItems = new();

        public ObservableCollection<TokenizingTextBoxItem> TokenItems
        {
            get { return _tokenItems; }
            set
            {
                SetAndRaise(TokenItemsProperty, ref _tokenItems, value);
            }
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
            Items = new ObservableCollection<TokenizingTextBoxItem>();
        }

        static TokenizingTextBox()
        {
        }

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

            if (_textBox != null)
            {
                _textBox.KeyDown -= TextBox_KeyDown;
                _textBox.RemoveHandler(TextInputEvent, TextBox_TextChanged);
                //_textBox.TextInput -= TextBox_TextChanged; //TextInput doesn't work?
            }

            _textBox = (TextBox)e.NameScope.Get<Control>(PART_TextBox);
            _wrapPanel = (WrapPanel)e.NameScope.Get<Control>(PART_WrapPanel);

            if (_textBox != null)
            {
                _textBox.KeyDown += TextBox_KeyDown;
                _textBox.AddHandler(TextInputEvent, TextBox_TextChanged, RoutingStrategies.Tunnel);
                //_textBox.TextInput += TextBox_TextChanged; //TextInput doesn't work?
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

        private void TextBox_KeyDown(object? sender, KeyEventArgs e) { }

        private void AddToken(string token)
        {
            if (token.Length > 0)
            {
                var tokens = new ObservableCollection<TokenizingTextBoxItem>();

                foreach (var item in Items)
                {
                    tokens.Add((TokenizingTextBoxItem)item);
                }

                tokens.Add(new TokenizingTextBoxItem() { Content = token });
                Items = tokens;
                TokenItems.Add(new TokenizingTextBoxItem() { Content = token });
            }
        }

        //protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Create Token?");
        //    base.ItemsChanged(e);
        //}

        //private void AddToken()
        //{
        //    var text = _textBox.Text;
        //    _textBox.Text = string.Empty;
        //    AddToken(text);
        //}
    }
}