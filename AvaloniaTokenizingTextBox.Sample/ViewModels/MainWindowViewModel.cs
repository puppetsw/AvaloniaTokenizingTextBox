using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AvaloniaTokenizingTextBox.Sample.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<string> _tokens = new();

        public object? SelectedItem { get; set; }
        
        public ObservableCollection<string> Tokens
        {
            get { return _tokens; }
            set { this.RaiseAndSetIfChanged(ref _tokens, value); }
        }

        public MainWindowViewModel()
        {
        }
    }
}
