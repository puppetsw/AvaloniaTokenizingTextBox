using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using System;

namespace AvaloniaTokenizingTextBox.Controls
{
    public class TokenTextBox : TextBox, IStyleable
    {
        Type IStyleable.StyleKey => typeof(TextBox);

        public event EventHandler<KeyEventArgs> MyKeyDown;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //bubble up event here for backspace when at start of textbox
            OnMyKeyDown(e);
            base.OnKeyDown(e);
        }

        protected virtual void OnMyKeyDown(KeyEventArgs e)
        {
            MyKeyDown?.Invoke(this, e);
        }
    }
}
