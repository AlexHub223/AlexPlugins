using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using System.Linq;
using InfoRestorer;

namespace InfoRestorer.Commands
{
    public class RestoreCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "restore";
        public string Help => "Restores a player's inventory from a backup.";
        public string Syntax => "<player> [version]";
        public List<string> Aliases => new List<string> { "invrestore" };
        public List<string> Permissions => new List<string> { "inforestorer.restore" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var plugin = InfoRestorerPlugin.Instance;

            if (command.Length < 1)
            {
                UnturnedChat.Say(caller, plugin.Translate("usage"), plugin.MessageColor);
                return;
            }

            var target = UnturnedPlayer.FromName(command[0]);
            if (target == null)
            {
                UnturnedChat.Say(caller, plugin.Translate("player_not_found"), plugin.MessageColor);
                return;
            }

            if (!plugin.Backups.TryGetValue(target.CSteamID, out var backups) || backups.Count == 0)
            {
                UnturnedChat.Say(caller, plugin.Translate("no_backups", target.CharacterName), plugin.MessageColor);
                return;
            }

            int version = 1;
            if (command.Length >= 2)
            {
                if (!int.TryParse(command[1], out version))
                {
                    UnturnedChat.Say(caller, plugin.Translate("not_a_number", command[1]), plugin.MessageColor);
                    return;
                }
            }

            if (version < 1 || version > backups.Count)
            {
                UnturnedChat.Say(caller, plugin.Translate("out_of_range", target.CharacterName, backups.Count), plugin.MessageColor);
                return;
            }

            var backup = backups[version - 1];
            backup.ApplyTo(target.Player, plugin.Configuration.Instance.ShouldClearInventory);

            UnturnedChat.Say(caller, plugin.Translate("restored", target.CharacterName, version), plugin.MessageColor);
        }
    }
}
