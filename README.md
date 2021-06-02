AvaloniaTokenizingTextBox
============
![Example](https://user-images.githubusercontent.com/79826944/120430465-37a5a980-c3b6-11eb-9501-3ea1022c64f4.gif)

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
