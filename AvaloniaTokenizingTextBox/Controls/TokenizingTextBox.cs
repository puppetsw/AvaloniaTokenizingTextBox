using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Utils;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace AvaloniaTokenizingTextBox.Controls;

public class TokenDelimiter
{
    private readonly TokenDelimiterType _delimiterType;

    public TokenDelimiter(TokenDelimiterType delimiterType)
    {
        _delimiterType = delimiterType;
    }

    public string? GetTypeAsString() =>
        _delimiterType switch
        {
            TokenDelimiterType.Semicolon => ";",
            TokenDelimiterType.Comma => ",",
            TokenDelimiterType.Pipe => "|",
            TokenDelimiterType.ForwardSlash => "//",
            TokenDelimiterType.BackSlash => "\\",
            TokenDelimiterType.Custom => null,
            _ => throw new ArgumentOutOfRangeException(nameof(_delimiterType), _delimiterType, null),
        };

    public Key? GetTypeAsKeycode() =>
        _delimiterType switch
        {
            TokenDelimiterType.Semicolon => (Key?)Key.OemSemicolon,
            TokenDelimiterType.Comma => (Key?)Key.OemComma,
            TokenDelimiterType.Pipe => (Key?)Key.OemPipe,
            TokenDelimiterType.ForwardSlash => null,
            TokenDelimiterType.BackSlash => (Key?)Key.OemBackslash,
            TokenDelimiterType.Custom => null,
            _ => throw new ArgumentOutOfRangeException(nameof(_delimiterType), _delimiterType, null),
        };
}

public enum TokenDelimiterType
{
    Semicolon = 0,
    Comma = 1,
    Pipe = 2,
    ForwardSlash = 3,
    BackSlash = 4,
    Custom = 5
}

/// <summary>
/// A text input control that displays tokens.
/// </summary>
public class TokenizingTextBox : SelectingItemsControl
{
    private const string PART_TEXT_BOX = "PART_TextBox";
    private const string PART_WRAP_PANEL = "PART_WrapPanel";
    private const string PART_POPUP = "PART_Popup";
    private const string PART_SEARCH_LIST_BOX = "PART_SearchListBox";

    private CancellationTokenSource? _cancellationTokenSource;

    private ObservableCollection<string> _searchResults;
    private IEnumerable<string> _searchSource;

    private TokenTextBox _textBox;
    private WrapPanel _wrapPanel;
    private Popup _popup;

    private string _tempText = string.Empty;

    private ISelectionAdapter? _adapter;
    private object? _selectedSearchResult;

    /// <summary>
    /// The default value for the <see cref="ItemsControl.ItemsPanel"/> property.
    /// </summary>
    private static readonly FuncTemplate<Panel> DefaultPanel =
        new(() => new StackPanel {Orientation = Orientation.Horizontal});

    /// <summary>
    /// Defines the <see cref="TokenDelimiter"/> property.
    /// </summary>
    public static readonly StyledProperty<TokenDelimiter> TokenDelimiterProperty =
        AvaloniaProperty.Register<TokenizingTextBox, TokenDelimiter>(
            nameof(TokenDelimiter), 
            new TokenDelimiter(TokenDelimiterType.Semicolon));

    /// <summary>
    /// Defines the <see cref="Text"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        TextBlock.TextProperty.AddOwner<TokenizingTextBox>(new(
            coerce: (sender, value) => value,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true));

    /// <summary>
    /// Defines the <see cref="SearchSource"/> property. 
    /// </summary>
    public static readonly DirectProperty<TokenizingTextBox, IEnumerable<string>> SearchSourceProperty =
        AvaloniaProperty.RegisterDirect<TokenizingTextBox, IEnumerable<string>>(
            nameof(SearchSource),
            o => o.SearchSource,
            (o, v) => o.SearchSource = v);

    /// <summary>
    /// Defines the <see cref="SearchResults"/> property. 
    /// </summary>
    public static readonly DirectProperty<TokenizingTextBox, IList<string>> SearchResultsProperty =
        AvaloniaProperty.RegisterDirect<TokenizingTextBox, IList<string>>(
            nameof(SearchResults),
            o => o.SearchResults);

    /// <summary>
    /// Defines the <see cref="SelectedSearchResult"/> property.
    /// </summary>
    public static readonly DirectProperty<TokenizingTextBox, object?> SelectedSearchResultProperty =
        AvaloniaProperty.RegisterDirect<TokenizingTextBox, object?>(
            nameof(SelectedSearchResult),
            o => o.SelectedSearchResult,
            (o, v) => o.SelectedSearchResult = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Gets or sets the search source.
    /// </summary>
    public IEnumerable<string> SearchSource
    {
        get => _searchSource;
        set => SetAndRaise(SearchSourceProperty, ref _searchSource, value);
    }

    /// <summary>
    /// Gets the search results.
    /// </summary>
    public ObservableCollection<string> SearchResults
    {
        get => _searchResults;
        //set => SetAndRaise(SearchResultsProperty, ref _searchResults, value);
    }

    /// <summary>
    /// Gets or sets the selected search result.
    /// </summary>
    public object? SelectedSearchResult
    {
        get => _selectedSearchResult;
        private set => SetAndRaise(SelectedSearchResultProperty, ref _selectedSearchResult, value);
    }

    /// <summary>
    /// Gets or sets the input text.
    /// </summary>
    [Content]
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the token delimiter.
    /// </summary>
    public TokenDelimiter TokenDelimiter
    {
        get => GetValue(TokenDelimiterProperty);
        set => SetValue(TokenDelimiterProperty, value);
    }

    static TokenizingTextBox()
    {
        ItemsPanelProperty.OverrideDefaultValue<TokenizingTextBox>(DefaultPanel!);
        TextProperty.Changed.AddClassHandler<TokenizingTextBox>((x, e) => x.OnTextPropertyChanged(e));
        SelectedSearchResultProperty.Changed.AddClassHandler<TokenizingTextBox>((x, e) => x.OnSelectedSearchResultChanged(e));
    }

    public TokenizingTextBox()
    {
        // these null!s are because those fields are set after the constructor,
        // but they can be considered non-nullable :-)
        _adapter = null;
        _searchResults = []; // this must be non-null
        _searchSource = null!;
        _textBox = null!;
        _wrapPanel = null!;
        _popup = null!;
        SelectionChanged += OnSelectionChanged;
        Selection = new SelectionModel<string>();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Debug.WriteLine($"Selection added items: {string.Join(',', e.AddedItems.Cast<string>())}");
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TokenizingTextBoxItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TokenizingTextBoxItem>(item, out recycleKey);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_textBox != null && _wrapPanel != null)
        {
            _textBox.RemoveHandler(TextInputEvent, TextBox_TextChanged);
            _textBox.BackKeyDown -= TextBox_KeyDown;
            _textBox.GotFocus -= TextBox_GotFocus;
            _wrapPanel.KeyDown -= WrapPanel_KeyDown;
        }

        _textBox = e.NameScope.Get<TokenTextBox>(PART_TEXT_BOX);
        _wrapPanel = e.NameScope.Get<WrapPanel>(PART_WRAP_PANEL);
        _popup = e.NameScope.Get<Popup>(PART_POPUP);
        SelectionAdapter = GetSelectionAdapterPart(e.NameScope);

        if (_textBox != null && _wrapPanel != null)
        {            
            _textBox.AddHandler(TextInputEvent, TextBox_TextChanged, RoutingStrategies.Tunnel);
            _textBox.BackKeyDown += TextBox_KeyDown;
            _textBox.GotFocus += TextBox_GotFocus;
            _wrapPanel.KeyDown += WrapPanel_KeyDown;
        }
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.Source is Visual source)
        {
            var point = e.GetCurrentPoint(source);

            if (point.Properties.IsLeftButtonPressed || point.Properties.IsRightButtonPressed)
            {
                e.Handled = UpdateSelectionFromEventSource(
                    e.Source,
                    true,
                    e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                    e.KeyModifiers.HasFlag(KeyModifiers.Control),
                    point.Properties.IsRightButtonPressed);
            }
        }
    }
    
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);

        if (e.NavigationMethod == NavigationMethod.Directional)
        {
            e.Handled = UpdateSelectionFromEventSource(
                e.Source,
                true,
                e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                e.KeyModifiers.HasFlag(KeyModifiers.Control));
        }
    }

    private void OnSelectedSearchResultChanged(AvaloniaPropertyChangedEventArgs e)
    {
        object? newItem = e.NewValue;
        
        if (newItem != null)
        {
            _tempText = (string) newItem;
        }
    }

    private void OnAdapterSelectionComplete(object? sender, RoutedEventArgs e)
    {
        _textBox.Focus();
        string? selectedSearchItem =
            SelectionAdapter is SelectingItemsControlSelectionAdapter sicsa ?
            sicsa.SelectorControl!.SelectedValue as string :
            SelectionAdapter?.SelectedItem as string;
        if (selectedSearchItem != null)
        {
            _textBox.Text = string.Empty;
            _popup.IsOpen = false;
            AddToken(selectedSearchItem);
        }
        e.Handled = true;
        Debug.WriteLine("Adapter selection complete: " + selectedSearchItem);
    }

    private void OnAdapterSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedSearchResult = SelectionAdapter?.SelectedItem;
        Debug.WriteLine("Adapter selection changed.");
    }

    private void OnTextPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue != null)
        {
            OnTextChanged((string) e.NewValue);
        }
    }

    private void OnTextChanged(string searchText)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _cancellationTokenSource = new CancellationTokenSource();

        // TODO: add delay, add async search via callback / delegate
        var currentResults = new List<string>(SearchResults);
        var newResults = FilterText(searchText, _cancellationTokenSource.Token);

        foreach (var str in newResults.Except(currentResults))
        {
            SearchResults.Add(str);
        }
        foreach (var str in currentResults.Except(newResults))
        {
            SearchResults.Remove(str);
        }

        _popup.IsOpen = SearchResults.Count > 0; // If we have results show the popup
    }

    private List<string> FilterText(string searchText, CancellationToken cancellationToken)
    {
        List<string> items = new(SearchSource); //create local copy of SearchSource property.
        List<string> tokens = new((IEnumerable<string>) ItemsSource!); //get local list of tokens already added.
        List<string> results = new();

        foreach (string item in items)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            //TODO: Filter out items already in the tokens list.
            //TODO: Change with better method for matching
            if (!string.IsNullOrEmpty(searchText) && !tokens.Contains(item) && item.Contains(searchText))
            {
                results.Add(item);
            }
        }
        return results;
    }

    private ISelectionAdapter? SelectionAdapter
    {
        get => _adapter;
        set
        {
            if (_adapter != null)
            {
                _adapter.SelectionChanged -= OnAdapterSelectionChanged;
                _adapter.Commit -= OnAdapterSelectionComplete;
                _adapter.Cancel -= OnAdapterSelectionComplete;
                _adapter.ItemsSource = null;
            }

            _adapter = value;

            if (_adapter != null)
            {
                _adapter.SelectionChanged += OnAdapterSelectionChanged;
                _adapter.Commit += OnAdapterSelectionComplete;
                _adapter.Cancel += OnAdapterSelectionComplete;
                _adapter.ItemsSource = SearchResults;
            }
        }
    }

    private static ISelectionAdapter GetSelectionAdapterPart(INameScope nameScope)
    {
        SelectingItemsControl? selector = nameScope.Find<SelectingItemsControl>(PART_SEARCH_LIST_BOX);
        if (selector != null)
        {
            // Check if it is already an IItemsSelector
            // Built in support for wrapping a Selector control
            return (selector as ISelectionAdapter) ?? new SelectingItemsControlSelectionAdapter(selector);
        }
        else
        {
            return nameScope.Find<ISelectionAdapter>(PART_SEARCH_LIST_BOX)!;
        }
    }

    private void TextBox_GotFocus(object? sender, GotFocusEventArgs e)
    {
        if (!string.IsNullOrEmpty(_tempText))
        {
            Text = _tempText;
            _tempText = string.Empty; //Clear temp string
            _popup.Focus();
            _popup.IsOpen = false;
            ClearTextBoxSelection();
        }
        
        //Clear selection of tokens.
        ClearTokenSelection();
    }

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        int currentCursorPosition = _textBox.SelectionStart;
        int selectionLength = currentCursorPosition + _textBox.SelectionEnd;
        switch (e.Key)
        {            
            case Key.Left when currentCursorPosition == 0 && selectionLength == 0 && ItemCount > 0:
            case Key.Back when currentCursorPosition == 0 && selectionLength == 0 && ItemCount > 0:
                var container = ContainerFromIndex(ItemCount - 1);
                if (container is TokenizingTextBoxItem element)
                {
                    element.Focus();
                    SelectedIndex = ItemCount - 1;
                }

                e.Handled = true;
                break;
            case Key.Escape:
            case Key.Back when currentCursorPosition == 1:
                _popup.IsOpen = false;
                e.Handled = true;
                break;
            // hacky solution for backspace not triggering TextBox_TextChanged
            case Key.Back:
                TextBox_TextChanged(null, new TextInputEventArgs { Text = "\b" });
                break;
        }
    }

    private void TextBox_TextChanged(object? sender, TextInputEventArgs e)
    {
        string textBoxText = _textBox.Text ?? string.Empty;
        string text = e.Text switch
        {
            "\b" => (textBoxText.Length > 0 ? textBoxText[0..^1] : string.Empty),
            null => textBoxText,
            _ => textBoxText + e.Text
        };
        string? tokenDelimiterStr = TokenDelimiter.GetTypeAsString();

        if (string.IsNullOrEmpty(text))
        {
            _popup.IsOpen = false;
            return;
        }

        if (tokenDelimiterStr != null && !text.Contains(tokenDelimiterStr))
        {
            // perform search and ignore tokenization
            OnTextChanged(text);
            return;
        }

        bool lastDelimited = text[^1] == tokenDelimiterStr![0];

        string[] tokens = text.Split(tokenDelimiterStr, StringSplitOptions.RemoveEmptyEntries);
        int numberToProcess = lastDelimited ? tokens.Length : tokens.Length - 1;

        for (var position = 0; position < numberToProcess; position++)
        {
            AddToken(tokens[position]);
        }

        if (lastDelimited)
        {
            _textBox.Text = string.Empty;
        }
        else
        {
            _textBox.Text = tokens[^1];
            _textBox.CaretIndex = _textBox.Text.Length;
        }

        e.Handled = true; //handle the event so the delimiter doesn't display
    }

    private void WrapPanel_KeyDown(object? sender, KeyEventArgs e)
    {
        SelectionAdapter?.HandleKeyDown(e);
        if (e.Handled)
        {
            return;
        }
        
        if (e.Key == TokenDelimiter.GetTypeAsKeycode())
        {
            e.Handled = true;
            _popup.Focus();
            _popup.IsOpen = false;
            _textBox.Focus();
            TextBox_TextChanged(null, new TextInputEventArgs { Text = TokenDelimiter.GetTypeAsString() } );
            return;
        }
        
        switch (e.Key)
        {
            case Key.Back when ItemCount > 0:
            case Key.Delete when ItemCount > 0 && SelectedItem != null:
                string selectedItem = (SelectedItem as string)!;
                int index = ((IList<string>)ItemsSource!).IndexOf(selectedItem);
                (ItemsSource as IList)?.RemoveAt(index);
                _textBox.Focus();
                break;
            case Key.End when ItemCount > 0:
                _textBox.Focus();
                break;
            case Key.Right when ItemCount - 1 == SelectedIndex:
                _textBox.Focus();
                break;
            case Key.Tab:
                e.Handled = true;
                _popup.Focus();
                _popup.IsOpen = false;
                _textBox.Focus();
                TextBox_TextChanged(null, new TextInputEventArgs { Text = TokenDelimiter.GetTypeAsString() } );
                break;
        }
    }

    private void AddToken(string token)
    {
        if (token.Length > 0)
        {
            (ItemsSource as IList)?.Add(token);
        }
    }

    private void ClearTextBoxSelection()
    {
        if (_textBox != null)
        {
            int length = _textBox.Text?.Length ?? 0;
            _textBox.SelectionStart = length;
            _textBox.SelectionEnd = length;
        }
    }

    private void ClearTokenSelection()
    {
        SelectedIndex = -1;
    }

    /// <summary>
    /// Deletes a token.
    /// </summary>
    public void DeleteToken(object obj)
    {
        if (obj is string token)
        {
            // The removal should be by index, 
            // in case there are repeated tokens.
            // TODO: find out how to do that
            ((IList<string>)ItemsSource!).Remove(token);
            _textBox?.Focus();
        }
    }
}