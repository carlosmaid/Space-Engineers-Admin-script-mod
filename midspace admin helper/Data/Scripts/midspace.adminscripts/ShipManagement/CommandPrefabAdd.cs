﻿namespace midspace.adminscripts
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using midspace.adminscripts.Messages.Sync;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;

    public class CommandPrefabAdd : ChatCommand
    {
        public CommandPrefabAdd()
            : base(ChatCommandSecurity.Admin, "addprefab", new[] { "/addprefab" })
        {
        }

        public override void Help(ulong steamId, bool brief)
        {
            MyAPIGateway.Utilities.ShowMessage("/addprefab <#>", "Add the specified <#> prefab. Spawns the specified a ship 2m directly in front of player.");
        }

        public override bool Invoke(ulong steamId, long playerId, string messageText)
        {
            var match = Regex.Match(messageText, @"/addprefab\s{1,}(?<Key>.+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var prefabName = match.Groups["Key"].Value;
                var prefabKvp = MyDefinitionManager.Static.GetPrefabDefinitions().FirstOrDefault(kvp => kvp.Key.Equals(prefabName, StringComparison.InvariantCultureIgnoreCase));
                MyPrefabDefinition prefab = null;

                if (prefabKvp.Value != null)
                    prefab = prefabKvp.Value;

                int index;
                if (prefabName.Substring(0, 1) == "#" && Int32.TryParse(prefabName.Substring(1), out index) && index > 0 && index <= CommandListPrefabs.PrefabCache.Count)
                    prefab = CommandListPrefabs.PrefabCache[index - 1];

                if (prefab != null)
                {
                    if (!MyAPIGateway.Multiplayer.MultiplayerActive)
                    {
                        if (MessageSyncCreatePrefab.AddPrefab(prefab.Id.SubtypeName, MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.EntityId, SyncCreatePrefabType.Stock))
                            return true;
                    }
                    else
                        ConnectionHelper.SendMessageToServer(new MessageSyncCreatePrefab()
                        {
                            PrefabName = prefab.Id.SubtypeName,
                            PositionEntityId = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.EntityId,
                            Type = SyncCreatePrefabType.Stock,
                        });
                    return true;
                }

                MyAPIGateway.Utilities.ShowMessage("Failed", "Could not create the specified prefab.");
                return true;
            }

            return false;
        }
    }
}
