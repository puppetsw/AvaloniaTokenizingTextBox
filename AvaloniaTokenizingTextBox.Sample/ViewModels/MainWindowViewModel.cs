using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Controls;
using AvaloniaTokenizingTextBox.Controls;

namespace AvaloniaTokenizingTextBox.Sample.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<string> tokens;

        public TokensControlViewModel TestViewModel { get; } = new TokensControlViewModel();
        
        public object SelectedItem { get; set; }
        public ObservableCollection<string> Tokens
        {
            get { return tokens; }
            set { this.RaiseAndSetIfChanged(ref tokens, value); }
        }

        public ICommand TestCommand { get; }
        public ICommand AddCommand { get; }

        public MainWindowViewModel()
        {
            Tokens = new ObservableCollection<string>();

            AddCommand = ReactiveCommand.Create(Add);
        }

        private void Add()
        {
            TestViewModel.Controls.Add(new TextBlock { Text = "Test" });
        }
    }
}
