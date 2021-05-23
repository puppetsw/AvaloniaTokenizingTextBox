using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using System;

namespace AvaloniaTokenizingTextBox.Controls
{
    /// <summary>
    /// Class TokenTextBox.
    /// Implements the <see cref="Avalonia.Controls.TextBox" />
    /// Implements the <see cref="Avalonia.Styling.IStyleable" />
    /// <para></para>
    /// </summary>
    /// <seealso cref="Avalonia.Controls.TextBox" />
    /// <seealso cref="Avalonia.Styling.IStyleable" />
    public class TokenTextBox : TextBox, IStyleable
    {
        Type IStyleable.StyleKey => typeof(TextBox);

        public event EventHandler<KeyEventArgs>? MyKeyDown;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //If backspace is pressed while the caret position of the textbox is 0
            //the event doesn't fire? So we raise our own event.
            OnMyKeyDown(e);
            base.OnKeyDown(e);
        }

        protected virtual void OnMyKeyDown(KeyEventArgs e) => MyKeyDown?.Invoke(this, e);
    }
}
