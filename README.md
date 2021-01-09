# Causym

Causym is a discord bot built using https://github.com/Quahu/Disqord

<a href="https://www.buymeacoffee.com/Jaquesy"><img src="https://img.buymeacoffee.com/button-api/?text=Buy me a coffee&emoji=&slug=Jaquesy&button_colour=BD5FFF&font_colour=ffffff&font_family=Cookie&outline_colour=000000&coffee_colour=FFDD00"></a>

## Current Features
- Custom guild prefixes
- Message translation
- Guild/channel statistics tracking including snapshots of message frequency, members online/dnd/idle and total member counts

## Requirements
- Running on Debian Linux
The statistics module runs with ScottPlot, currently .net core 3 does not fully support `System.Drawing`, as such you may run into the following issue: https://github.com/dotnet/core/issues/2746
To solve this install the following:
```
sudo apt install libc6-dev 
sudo apt install libgdiplus
```
