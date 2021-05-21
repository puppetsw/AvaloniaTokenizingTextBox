using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace AvaloniaTokenizingTextBox.Sample.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<string> tokens;

        public ObservableCollection<string> Tokens
        {
            get { return tokens; }
            set { this.RaiseAndSetIfChanged(ref tokens, value); }
        }

        public ICommand TestCommand { get; }

        public MainWindowViewModel()
        {
            Tokens = new ObservableCollection<string>();
            TestCommand = ReactiveCommand.Create(Test);

        }

        private void Test()
        {
            Debug.WriteLine(Tokens.Count);
        }
    }
}
