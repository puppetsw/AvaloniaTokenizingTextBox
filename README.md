AvaloniaTokenizingTextBox
============
![TextBoxExample](https://user-images.githubusercontent.com/79826944/119290212-42ac5b80-bc8b-11eb-8518-a5a545c68705.gif)

A Tokenizing TextBox for [Avalonia](https://github.com/AvaloniaUI/Avalonia), similar to the one created by [Marcus Perryman](https://github.com/marcpems). 

Ported from [WindowsCommunityToolkit](https://github.com/windows-toolkit/WindowsCommunityToolkit)

Based on work from [Jöran Malek](https://github.com/iterate-ch/tokenizingtextbox)

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
