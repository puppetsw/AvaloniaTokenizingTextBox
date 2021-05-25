using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;

namespace AvaloniaTokenizingTextBox.Controls
{
    /// <summary>
    /// <inheritdoc/>
    /// <para>Implements <see cref="Avalonia.Controls.TextBox" />,
    /// Implements <see cref="Avalonia.Styling.IStyleable" /></para>
    /// </summary>
    /// <remarks>
    /// As the KeyDown event doesn't seem to fire if the caret position
    /// of the TextBox is at the start of the control. Created inherited
    /// control that fires that extra event for us.
    /// </remarks>
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
