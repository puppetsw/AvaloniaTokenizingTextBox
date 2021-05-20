using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;

namespace AvaloniaTokenizingTextBox.Controls
{
    public class TokenizingTextBox : ListBox
    {
        private const string PART_TextBox = "PART_TextBox";
        private const string PART_WrapPanel = "PART_WrapPanel";
        private TextBox? _textBox;
        private WrapPanel? _wrapPanel;

        /// <summary>
        /// Gets or sets SearchText.
        /// </summary>
        public string InputText
        {
            get { return (string)GetValue(InputTextProperty); }
            set { SetValue(InputTextProperty, value); }
        }

        /// <summary>
        /// Defines the SearchText property.
        /// </summary>
        public static readonly StyledProperty<string> InputTextProperty =
        AvaloniaProperty.Register<TokenizingTextBox, string>(nameof(InputText));

        public TokenizingTextBox()
        {
            SelectionModeProperty.OverrideMetadata(typeof(TokenizingTextBox), new StyledPropertyMetadata<SelectionMode>(SelectionMode.Multiple));
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
                //_textBox.TextInput -= TextBox_TextChanged;
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
            string t = _textBox.Text;

            if (t == null) return;

            if (!string.IsNullOrEmpty(" ") && t.Contains(" "))
            {
                bool lastDelimited = t[t.Length - 1] == " "[0];

                string[] tokens = t.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
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
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {

            base.OnKeyDown(e);
        }

        private void TextBox_KeyDown(object? sender, KeyEventArgs e) { }

        private void AddToken(string token) 
        {
            if (token.Length > 0)
            {
                var tokens = new AvaloniaList<string>();

                foreach (var item in Items)
                {
                    tokens.Add((string)item);
                }

                tokens.Add(token);

                Items = tokens;
            }

        }

        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Create Token?");
            base.ItemsChanged(e);
        }

        private void AddToken()
        {
            var text = _textBox.Text;
            _textBox.Text = string.Empty;
            AddToken(text);
        }
    }
}