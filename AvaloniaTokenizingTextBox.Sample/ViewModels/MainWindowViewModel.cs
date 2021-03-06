using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AvaloniaTokenizingTextBox.Sample.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<string> _tokens = new();

        public ObservableCollection<string> SelectableTokens { get; } = new() { "anothertest@gmail.com", "anothertest2@gmail.com", "test@gmail.com" };

        public object? SelectedItem { get; set; }
        
        public ObservableCollection<string> Tokens
        {
            get => _tokens;
            set => this.RaiseAndSetIfChanged(ref _tokens, value);
        }

        public ICommand TestCommand { get; }
        
        public MainWindowViewModel()
        {
            TestCommand = ReactiveCommand.Create(Test);
        }

        private void Test()
        {
            throw new System.NotImplementedException();
        }
    }
}
