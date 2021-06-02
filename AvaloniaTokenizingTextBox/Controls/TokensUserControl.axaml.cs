using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaTokenizingTextBox.Controls
{
    public class TokensUserControl : UserControl
    {
        public TokensUserControl()
        {
            InitializeComponent();

            //this.DataContext = new TokensControlViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}