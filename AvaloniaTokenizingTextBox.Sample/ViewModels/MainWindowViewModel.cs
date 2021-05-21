using AvaloniaTokenizingTextBox.Controls;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace AvaloniaTokenizingTextBox.Sample.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<TokenizingTextBoxItem> tokens;

        public ObservableCollection<TokenizingTextBoxItem> Tokens
        {
            get { return tokens; }
            set { this.RaiseAndSetIfChanged(ref tokens, value); }
        }

        public ICommand TestCommand { get; }

        public MainWindowViewModel()
        {
            Tokens = new ObservableCollection<TokenizingTextBoxItem>();
            TestCommand = ReactiveCommand.Create(Test);

        }

        private void Test()
        {
            Debug.WriteLine(Tokens.Count);
        }
    }
}
