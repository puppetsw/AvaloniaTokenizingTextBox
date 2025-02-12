using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace AvaloniaTokenizingTextBox.Sample.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<string> SelectableTokens { get; }

    private string? _selectedItem;
    public string? SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public ObservableCollection<string> Tokens { get; }

    public ICommand TestCommand { get; }
    
    public MainWindowViewModel()
    {
        SelectableTokens = ["anothertest@gmail.com", "anothertest2@gmail.com", "test@gmail.com", "john.mcclane@hotmail.com"];
        Tokens = [];
        TestCommand = ReactiveCommand.Create(Test);
    }

    private void Test()
    {
        Debug.WriteLine("Selected tokens: " + string.Join(',', Tokens));
        //throw new System.NotImplementedException();
    }
}
