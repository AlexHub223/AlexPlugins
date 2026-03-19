using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Adam.InfoRestorer.Commands
{
    public class CommandRestore : IRocketCommand
    {
        private InfoRestorerPlugin pluginInstance => InfoRestorerPlugin.Instance;

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("invalid_syntax"), pluginInstance.MessageColor);
                return;
            }

            var found = UnturnedPlayer.FromName(command[0]);
            if (found == null)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("player_not_found"), pluginInstance.MessageColor);
                return;
            }

            if (!int.TryParse(command[1], out int timesAgo) || timesAgo <= 0)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("not_number", command[1]), pluginInstance.MessageColor);
                return;
            }

            var saves = pluginInstance.Players[found.CSteamID];

            var index = saves.Count - timesAgo;
            if (index < 0 || index >= saves.Count)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("too_much", found.CharacterName), pluginInstance.MessageColor);
                return;
            }

            var inventory = saves[index];
            inventory.LoadToPlayer(found.Player, pluginInstance.Configuration.Instance.ShouldClearInventory);
            UnturnedChat.Say(caller, pluginInstance.Translate("restored", found.CharacterName), pluginInstance.MessageColor);
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "restore";

        public string Help => "";

        public string Syntax => "<player> <timeAgo>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();
    }
}
