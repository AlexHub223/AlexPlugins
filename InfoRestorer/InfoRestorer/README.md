# InfoRestorer

**InfoRestorer** is an advanced Unturned RocketMod plugin that automatically backups player inventories before death, allowing administrators to restore them easily.

## 🚀 Features

- **Automatic Backups**: Captures complete inventory, clothing, and equipped items exactly before a player dies.
- **Multiple Versions**: Stores multiple backups per player (configurable limit).
- **Easy Restoration**: Simple command to restore any backup version to a player.
- **Optimized**: Clean implementation focused on performance and reliability.
- **Clean Configuration**: Control how many saves to keep and whether to clear inventory on restore.

## 📦 Installation

1. Download `InfoRestorer.dll`.
2. Place it in your `Plugins` folder.
3. Configure the settings in `Plugins/InfoRestorer/InfoRestorer.configuration.xml`.

## ⚙️ Commands

- `/restore <player> [version]`
  - Restores the inventory of the specified player.
  - `[version]` is optional (defaults to the most recent death).

## 🛡️ Permissions

- `inforestorer.restore`: Allows using the `/restore` command.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
