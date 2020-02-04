# Causym

Causym is a discord bot built using https://github.com/Quahu/Disqord

## Current Features
- Custom guild prefixes
- Message translation via Yandex API
- Guild/channel statistics tracking including snapshots of message frequency, members online/dnd/idle and total member counts

## Requirements
- Running on Debian Linux
The statistics module runs with ScottPlot, currently .net core 3 does not fully support `System.Drawing`, as such you may run into the following issue: https://github.com/dotnet/core/issues/2746
To solve this install the following:
```
sudo apt install libc6-dev 
sudo apt install libgdiplus
```
