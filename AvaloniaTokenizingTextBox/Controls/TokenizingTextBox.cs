using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Utils;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AvaloniaTokenizingTextBox.Controls
{
    public class TokenDelimiter
    {
        private readonly TokenDelimiterType _delimiterType;

        public TokenDelimiter(TokenDelimiterType delimiterType)
        {
            _delimiterType = delimiterType;
        }
        
        public string? GetTypeAsString()
        {
            switch (_delimiterType)
            {
                case TokenDelimiterType.Semicolon:
                    return ";";
                case TokenDelimiterType.Comma:
                    return ",";
                case TokenDelimiterType.Pipe:
                    return "|";
                case TokenDelimiterType.ForwardSlash:
                    return "//";
                case TokenDelimiterType.BackSlash:
                    return "\\";
                case TokenDelimiterType.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_delimiterType), _delimiterType, null);
            }

            return null;
        }

        public Key? GetTypeAsKeycode()
        {
            switch (_delimiterType)
            {
                case TokenDelimiterType.Semicolon:
                    return Key.OemSemicolon;
                case TokenDelimiterType.Comma:
                    return Key.OemComma;
                case TokenDelimiterType.Pipe:
                    return Key.OemPipe;
                case TokenDelimiterType.ForwardSlash:
                    break;
                case TokenDelimiterType.BackSlash:
                    return Key.OemBackslash;
                case TokenDelimiterType.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_delimiterType), _delimiterType, null);
            }

            return null;
        }
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

        private CancellationTokenSource? _cancellationTokenSource = new();

        private IEnumerable<string> _searchResults = new AvaloniaList<string>();
        private IEnumerable<string> _searchSource = new AvaloniaList<string>();

        private TokenTextBox? _textBox;
        private WrapPanel? _wrapPanel;
        private Popup? _popup;

        private string _text = string.Empty;
        private string _tempText = string.Empty;

        private ISelectionAdapter _adapter;
        private object _selectedSearchResult;

        /// <summary>
        /// The default value for the <see cref="ItemsControl.ItemsPanel"/> property.
        /// </summary>
        private static readonly FuncTemplate<IPanel> DefaultPanel =
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
        public static readonly DirectProperty<TokenizingTextBox, string> TextProperty =
            TextBlock.TextProperty.AddOwnerWithDataValidation<TokenizingTextBox>(
                o => o.Text,
                (o, v) => o.Text = v,
                defaultBindingMode: BindingMode.TwoWay,
                enableDataValidation: true);

        /// <summary>
        /// Defines the <see cref="SearchResults"/> property. 
        /// </summary>
        public static readonly DirectProperty<TokenizingTextBox, IEnumerable<string>> SearchResultsProperty =
            AvaloniaProperty.RegisterDirect<TokenizingTextBox, IEnumerable<string>>(
                nameof(SearchResults),
                o => o.SearchResults);

        /// <summary>
        /// Defines the <see cref="SearchSource"/> property. 
        /// </summary>
        public static readonly DirectProperty<TokenizingTextBox, IEnumerable<string>> SearchSourceProperty =
            AvaloniaProperty.RegisterDirect<TokenizingTextBox, IEnumerable<string>>(
                nameof(SearchSource),
                o => o.SearchSource,
                (o, v) => o.SearchSource = v);

        /// <summary>
        /// Defines the <see cref="SelectedSearchResult"/> property.
        /// </summary>
        public static readonly DirectProperty<TokenizingTextBox, object> SelectedSearchResultProperty =
            AvaloniaProperty.RegisterDirect<TokenizingTextBox, object>(
                nameof(SelectedSearchResult),
                o => o.SelectedSearchResult,
                (o, v) => o.SelectedSearchResult = v,
                defaultBindingMode: BindingMode.TwoWay,
                enableDataValidation: true);

        public IEnumerable<string> SearchResults
        {
            get => _searchResults;
            set => SetAndRaise(SearchResultsProperty, ref _searchResults, value);
        }

        public IEnumerable<string> SearchSource
        {
            get => _searchSource;
            set => SetAndRaise(SearchSourceProperty, ref _searchSource, value);
        }

        /// <summary>
        /// Gets or sets the input text.
        /// </summary>
        public string Text
        {
            get => _text;
            set => SetAndRaise(TextProperty, ref _text, value);
        }

        /// <summary>
        /// Gets or sets the token delimiter.
        /// </summary>
        public TokenDelimiter TokenDelimiter
        {
            get => GetValue(TokenDelimiterProperty);
            set => SetValue(TokenDelimiterProperty, value);
        }

        /// <summary>
        /// Gets of sets the selected search result.
        /// </summary>
        public object SelectedSearchResult
        {
            get => _selectedSearchResult;
            set => SetAndRaise(SelectedSearchResultProperty, ref _selectedSearchResult, value);
        }
        
        static TokenizingTextBox()
        {
            ItemsPanelProperty.OverrideDefaultValue<TokenizingTextBox>(DefaultPanel);

            TextProperty.Changed.AddClassHandler<TokenizingTextBox>((x, e) => x.OnTextPropertyChanged(e));
            SelectedSearchResultProperty.Changed.AddClassHandler<TokenizingTextBox>((x, e) => x.OnSelectedSearchResultChanged(e));
        }

        public TokenizingTextBox()
        {
            SelectionChanged += OnSelectionChanged;
            Selection = new SelectionModel<string>();
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine($"Selection changed {e.AddedItems}");
        }

        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            return new ItemContainerGenerator<TokenizingTextBoxItem>(
                this,
                TokenizingTextBoxItem.ContentProperty,
                TokenizingTextBoxItem.ContentTemplateProperty);
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

            if (e.Source is IVisual source)
            {
                var point = e.GetCurrentPoint(source);

                if (point.Properties.IsLeftButtonPressed || point.Properties.IsRightButtonPressed)
                {
                    e.Handled = UpdateSelectionFromEventSource(
                        e.Source,
                        true,
                        e.KeyModifiers.HasAllFlags(KeyModifiers.Shift),
                        e.KeyModifiers.HasAllFlags(KeyModifiers.Control),
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
                    e.KeyModifiers.HasAllFlags(KeyModifiers.Shift),
                    e.KeyModifiers.HasAllFlags(KeyModifiers.Control));
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

        private void OnAdapterSelectionComplete(object sender, RoutedEventArgs e)
        {
            _textBox?.Focus();
            //Console.WriteLine("Selection complete.");
        }

        private void OnAdapterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedSearchResult = SelectionAdapter.SelectedItem;
            //Console.WriteLine("Selection changed.");
        }

        private void OnTextPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                OnTextChanged((string) e.NewValue);
            }
        }

        private async void OnTextChanged(string searchText)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _cancellationTokenSource = new CancellationTokenSource();

            //TODO: add delay
            var results = await FilterTextAsync(searchText, _cancellationTokenSource.Token);

            if (results.Count > 0)
            {
                SearchResults = new List<string>(results);
                _popup.IsOpen = true; // If we have results show the popup
            }
        }

        private Task<List<string>> FilterTextAsync(string searchText, CancellationToken cancellationToken)
        {
            List<string> items = new(SearchSource); //create local copy of SearchSource property.
            List<string> tokens = new((IEnumerable<string>) Items); //get local list of tokens already added.
            List<string> results = new();

            foreach (string item in items)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                //TODO: Filter out items already in the tokens list.
                //TODO: Change with better method for matching
                if (item.StartsWith(searchText) && !string.IsNullOrEmpty(searchText) && !tokens.Contains(item))
                {
                    results.Add(item);
                }
            }
            return Task.FromResult(results);
        }

        private ISelectionAdapter SelectionAdapter
        {
            get => _adapter;
            set
            {
                if (_adapter != null)
                {
                    _adapter.SelectionChanged -= OnAdapterSelectionChanged;
                    _adapter.Commit -= OnAdapterSelectionComplete;
                    _adapter.Cancel -= OnAdapterSelectionComplete;
                    _adapter.Items = null;
                }

                _adapter = value;

                if (_adapter != null)
                {
                    _adapter.SelectionChanged += OnAdapterSelectionChanged;
                    _adapter.Commit += OnAdapterSelectionComplete;
                    _adapter.Cancel += OnAdapterSelectionComplete;
                    _adapter.Items = SearchResults;
                }
            }
        }

        private static ISelectionAdapter GetSelectionAdapterPart(INameScope nameScope)
        {
            ISelectionAdapter? adapter = null;
            SelectingItemsControl selector = nameScope.Find<SelectingItemsControl>(PART_SEARCH_LIST_BOX);
            if (selector != null)
            {
                // Check if it is already an IItemsSelector
                adapter = selector as ISelectionAdapter;
                if (adapter == null)
                {
                    // Built in support for wrapping a Selector control
                    adapter = new SelectingItemsControlSelectionAdapter(selector);
                }
            }
            if (adapter == null)
            {
                adapter = nameScope.Find<ISelectionAdapter>(PART_SEARCH_LIST_BOX);
            }
            return adapter;
        }

        private void TextBox_GotFocus(object? sender, GotFocusEventArgs e)
        {
            if (!string.IsNullOrEmpty(_tempText))
            {
                SetAndRaise(TextProperty, ref _text, _tempText);
                _tempText = string.Empty; //Clear temp string
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
                    var container = ItemContainerGenerator.ContainerFromIndex(ItemCount - 1);
                    if (container is TokenizingTextBoxItem element)
                    {
                        element.Focus();
                        SelectedIndex = ItemCount - 1;
                    }

                    e.Handled = true;
                    break;
            }
        }

        private void TextBox_TextChanged(object? sender, TextInputEventArgs e)
        {
            if (_textBox == null)
            {
                return;
            }

            string? textBoxText = _textBox?.Text;
            string text = textBoxText + e.Text;

            if (e.Text != null && (string.IsNullOrEmpty(TokenDelimiter.GetTypeAsString()) || !e.Text.Contains(TokenDelimiter.GetTypeAsString())))
            {
                return;
            }

            bool lastDelimited = text[^1] == TokenDelimiter.GetTypeAsString()[0];

            string[] tokens = text.Split(new[] { TokenDelimiter.GetTypeAsString() }, StringSplitOptions.RemoveEmptyEntries);
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
            SelectionAdapter.HandleKeyDown(e);
            if (e.Handled)
            {
                return;
            }
            
            if (e.Key == TokenDelimiter.GetTypeAsKeycode())
            {
                e.Handled = true;
                _popup?.Focus();
                _popup.IsOpen = false;
                _textBox?.Focus();
                TextBox_TextChanged(null, new TextInputEventArgs { Text = TokenDelimiter.GetTypeAsString() } );
                return;
            }
            
            switch (e.Key)
            {
                case Key.Back when ItemCount > 0:
                case Key.Delete when ItemCount > 0:
                    if (SelectedItem == null)
                        break;

                    int index = IndexOf(Items, SelectedItem);
                    (Items as IList)?.RemoveAt(index);
                    _textBox?.Focus();
                    break;
                case Key.End when ItemCount > 0:
                    _textBox?.Focus();
                    break;
                case Key.Right when ItemCount - 1 == SelectedIndex:
                    _textBox?.Focus();
                    break;
                case Key.Tab:
                    e.Handled = true;
                    _popup?.Focus();
                    _popup.IsOpen = false;
                    _textBox?.Focus();
                    TextBox_TextChanged(null, new TextInputEventArgs { Text = TokenDelimiter.GetTypeAsString() } );
                    break;
            }
        }

        private void AddToken(string token)
        {
            if (token.Length > 0)
                (Items as IList)?.Add(token);
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
        /// Deletes the currently selected token.
        /// </summary>
        public void DeleteSelected()
        {
            if (SelectedItem == null)
                return;
            int index = IndexOf(Items, SelectedItem);
            (Items as IList)?.RemoveAt(index);
            _textBox?.Focus();
        }
    }
}