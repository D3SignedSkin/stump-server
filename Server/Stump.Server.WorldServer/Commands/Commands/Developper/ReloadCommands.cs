using System;
using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Database.I18n;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Breeds;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Social;
using Stump.Server.WorldServer.Game.Idols;
using Stump.Server.WorldServer.Game.Jobs;
using Stump.Server.WorldServer.Game.Mounts;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Misc;
using Stump.Server.WorldServer.Game.Fairy;

namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    public class ReloadCommands : CommandBase
    {
        public Dictionary<string, Action> m_entries = new Dictionary<string, Action>
            {
                {"npcs", NpcManager.Instance.Initialize},
                {"npcsmap", NpcManager.Instance.Initialize},
                {"monsters", MonsterManager.Instance.Initialize},
                {"items", ItemManager.Instance.Initialize},
                {"world", World.Instance.Reload},
                {"effects", EffectManager.Instance.Initialize},
                {"interactives", InteractiveManager.Instance.Initialize},
                {"interactivesarea", InteractiveManager.Instance.Initialize},
                {"breeds", BreedManager.Instance.Initialize},
                {"experiences", ExperienceManager.Instance.Initialize},
                {"langs", TextManager.Instance.Initialize},
                {"badwords", ChatManager.Instance.Initialize},
                {"idols", IdolManager.Instance.Initialize},
                {"muldos", CaptureMuldoManager.Instance.Initialize},
                {"dragodindes", CaptureDragodindeManager.Instance.Initialize},
                {"vulk", CaptureVulkManager.Instance.Initialize},
                {"hunter", HunterManager.Instance.Initialize},
                {"announces", AutoManager.Instance.Initialize},
                {"fairy", ItemsFairyManager.Instance.Initialize},
                //{"zaaps", CustomZaapManager.Instance.Initialize},
            };

        public ReloadCommands()
        {
            Aliases = new[] {"reload"};
            RequiredRole=RoleEnum.Developer;
            Description = "Reload manager";
            AddParameter<string>("name", "n", "Name of the manager to reload", isOptional:true);
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = (trigger as GameTrigger).Character;

            if (!trigger.IsArgumentDefined("name"))
            {
                trigger.Reply("Entries : " + string.Join(", ", m_entries.Keys));
                return;
            }

            var name = trigger.Get<string>("name").ToLower();

            if (!m_entries.TryGetValue(name, out var entry))
            {
                trigger.ReplyError("{0} not a valid name.", name);
                trigger.ReplyError("Entries : " + string.Join(", ", m_entries.Keys));
                return;
            }
            var method = entry;

            if (method == null)
            {
                trigger.ReplyError("Cannot reload {0} : method not found", name);
                return;
            }

            trigger.ReplyBold($"[RELOAD] Reloading {name} ... WORLD PAUSED");

            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
                {
                    try
                    {
                        switch (name)
                        {
                            case "npcs":
                                World.Instance.UnSpawnNpcs();
                                ItemManager.Instance.Initialize();
                                break;
                            case "npcsmap":
                                World.Instance.UnSpawnNpcsMap(character);
                                ItemManager.Instance.Initialize();
                                break;

                            case "interactives":
                                World.Instance.UnSpawnInteractives();
                                break;
                            case "interactivesarea":
                                World.Instance.UnSpawnInteractivesArea(character);
                                break;
                        }

                        method.Invoke();

                        switch (name)
                        {
                            case "npcs":
                                World.SpawnNpcs();
                                AutoManager.Instance.Initialize();
                                break;
                            case "npcsmap":
                                World.SpawnNpcsMap(character);
                                AutoManager.Instance.Initialize();
                                break;
                            case "interactives":
                                World.Instance.SpawnInteractives();
                                break;
                            case "interactivesarea":
                                World.Instance.SpawnInteractivesArea(character);
                                break;
                        }
                    }
                    finally
                    {
                    }

                    trigger.ReplyBold($"[RELOAD] {name} reloaded ... WORLD RESUMED");
                });
        }
    }
}
