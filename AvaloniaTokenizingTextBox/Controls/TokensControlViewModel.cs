using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;

namespace AvaloniaTokenizingTextBox.Controls
{
    public class TokensControlViewModel
    {
        public ObservableCollection<object> Controls { get; } = new();

        private Control _lastTextBoxAdded;
        
        public TokensControlViewModel()
        {
            Controls.Add(new TextBox() { Text = "Test"});
            Controls.Add(new TextBlock() { Text = "Added Textblock"});
            Controls.Add(new TextBox() {});
            
            _lastTextBoxAdded = new TextBox() { Text = "1111"};
            _lastTextBoxAdded.KeyUp += OnTOnTextInput;
            
            Controls.Add(_lastTextBoxAdded);
        }

        private void OnTOnTextInput(object? sender, KeyEventArgs e)
        {
            if (_lastTextBoxAdded is not TextBox textBox || string.IsNullOrEmpty(textBox.Text))
            {
                return;
            }

            if (!textBox.Text.Contains("}"))
            {
                return;
            }

            _lastTextBoxAdded.KeyUp -= OnTOnTextInput;
            _lastTextBoxAdded = new TextBox();
            _lastTextBoxAdded.KeyUp += OnTOnTextInput;
            Controls.Add(_lastTextBoxAdded);
            _lastTextBoxAdded.Focus();
        }
    }
}