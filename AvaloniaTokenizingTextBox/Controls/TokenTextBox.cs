using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;

namespace AvaloniaTokenizingTextBox.Controls
{
    /// <summary>
    /// <inheritdoc cref="Avalonia.Controls.TextBox" />
    /// <para>Implements <see cref="Avalonia.Controls.TextBox" />,
    /// Implements <see cref="Avalonia.Styling.IStyleable" /></para>
    /// </summary>
    /// <remarks>
    /// As the KeyDown event doesn't seem to fire if the caret position
    /// of the TextBox is at the start of the control. Created inherited
    /// TextBox control that fires that extra event for us.
    /// </remarks>
    /// <seealso cref="Avalonia.Controls.TextBox" />
    /// <seealso cref="Avalonia.Styling.IStyleable" />
    public sealed class TokenTextBox : TextBox, IStyleable
    {
        Type IStyleable.StyleKey => typeof(TextBox);

        public event EventHandler<KeyEventArgs>? BackKeyDown;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //If backspace is pressed while the caret position of the TextBox is 0
            //the event doesn't fire? So we raise our own event.
            OnBackKeyDown(e);
            base.OnKeyDown(e);
        }

        private void OnBackKeyDown(KeyEventArgs e)
        {
            BackKeyDown?.Invoke(this, e);
        }
    }
}