AvaloniaTokenizingTextBox
============
### Current Progress
![Example1](https://user-images.githubusercontent.com/79826944/121011702-0e31b700-c7d6-11eb-86a3-45856847c105.gif)

A Tokenizing TextBox for [Avalonia](https://github.com/AvaloniaUI/Avalonia), similar to the one created by [Marcus Perryman](https://github.com/marcpems) for [WindowsCommunityToolkit](https://github.com/windows-toolkit/WindowsCommunityToolkit)

Based on work by [Jöran Malek](https://github.com/iterate-ch/tokenizingtextbox)

## Usage

```xml
<DockPanel>
  <c:TokenizingTextBox
    TokenDelimiter=";"
    Items="{Binding Tokens}"
    SelectedItem="{Binding SelectedItem}"
    SearchSource="{Binding SearchSourceList}"/>
</DockPanel>
```

## TODO
* Cleanup main control file
* Remove Listbox inheritence
* Added TemplatedControl inheritence
* Create Tokens Enumerable property
* Create SelectedToken property
* Re-organise OnKeyDown events
* ...pretty much an overhaul of everything

## Licence

AvaloniaTokenizingTextBox is licensed under the [MIT license](https://github.com/puppetsw/AvaloniaTokenizingTextBox/blob/master/LICENSE).

## Contributing

All contributions and improvements are welcome!
