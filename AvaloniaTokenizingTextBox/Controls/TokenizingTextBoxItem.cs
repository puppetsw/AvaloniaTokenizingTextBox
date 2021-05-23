using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;

namespace AvaloniaTokenizingTextBox.Controls
{
    /// <summary>
    /// Class TokenizingTextBoxItem.
    /// Implements the <see cref="Avalonia.Controls.ContentControl" />
    /// Implements the <see cref="Avalonia.Controls.ISelectable" />
    /// </summary>
    /// <seealso cref="Avalonia.Controls.ContentControl" />
    /// <seealso cref="Avalonia.Controls.ISelectable" />
    [PseudoClasses(":pressed", ":selected")]
    public class TokenizingTextBoxItem : ContentControl, ISelectable
    {
        /// <summary>
        /// Defines the <see cref="IsSelected"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<TokenizingTextBoxItem, bool>(nameof(IsSelected));

        /// <summary>
        /// Gets or sets the selection state of the item.
        /// </summary>
        public bool IsSelected
        {
            get { return GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        static TokenizingTextBoxItem()
        {
            SelectableMixin.Attach<TokenizingTextBoxItem>(IsSelectedProperty);
            PressedMixin.Attach<TokenizingTextBoxItem>();
            FocusableProperty.OverrideDefaultValue<TokenizingTextBoxItem>(true);
        }
    }
}
