using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AvaloniaTokenizingTextBox.Sample.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<string> tokens;

        public object SelectedItem { get; set; }
        public ObservableCollection<string> Tokens
        {
            get { return tokens; }
            set { this.RaiseAndSetIfChanged(ref tokens, value); }
        }

        public ICommand TestCommand { get; }

        public MainWindowViewModel()
        {
            Tokens = new ObservableCollection<string>();
        }
    }
}
