AvaloniaTokenizingTextBox
============
![TextBoxExample1](https://user-images.githubusercontent.com/79826944/119514941-9797c100-bdb4-11eb-8314-d2e26861a8a6.gif)

A Tokenizing TextBox for [Avalonia](https://github.com/AvaloniaUI/Avalonia), similar to the one created by [Marcus Perryman](https://github.com/marcpems) for [WindowsCommunityToolkit](https://github.com/windows-toolkit/WindowsCommunityToolkit)

Based on work by [Jöran Malek](https://github.com/iterate-ch/tokenizingtextbox)

## Usage

```xml
<DockPanel>
  <c:TokenizingTextBox DockPanel.Dock="Top"
    TokenDelimiter=";"
    Items="{Binding Tokens}"
    SelectedItem="{Binding SelectedItem}" />
</DockPanel>
```

## Licence

AvaloniaTokenizingTextBox is licensed under the [MIT license](https://github.com/puppetsw/AvaloniaTokenizingTextBox/blob/master/LICENSE).

## Contributing

All contributions and improvements are welcome!
