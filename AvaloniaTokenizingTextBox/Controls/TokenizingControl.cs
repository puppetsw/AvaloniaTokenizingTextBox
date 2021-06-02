using System.Collections;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace AvaloniaTokenizingTextBox.Controls
{
    /// <summary>
    /// Tokenizing Control
    /// </summary>
    public class TokenizingControl : ItemsControl
    {
        /// <summary>
        /// The default value for the <see cref="ItemsControl.ItemsPanel"/> property.
        /// </summary>
        private static readonly FuncTemplate<IPanel> s_defaultPanel =
            //new(() => new StackPanel { Orientation = Orientation.Horizontal });
            new(() => new TokenizingWrapPanel() { StretchChild = StretchChild.Last });

        static TokenizingControl() => ItemsPanelProperty.OverrideDefaultValue<TokenizingControl>(s_defaultPanel);

        public TokenizingControl()
        {
            
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsCollectionChanged(sender, e);
        }
    }
}