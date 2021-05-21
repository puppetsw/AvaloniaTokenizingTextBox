using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaTokenizingTextBox.Controls
{
    public class TokenizingTextBoxItem : ContentControl, ISelectable
    {
        public TokenizingTextBoxItem()
        {
            
        }

        public bool IsSelected { get; set; }
    }
}
