using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Benchmark;
using Stump.Server.BaseServer.Commands;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps;

namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    public class InfoIP : InGameCommand
    {
        public InfoIP()
        {
            Aliases = new[]
            {
                "infoip",
            };
            RequiredRole = RoleEnum.Developer;
            Description = "Numero de jugadores conectados x IP";
        }

        public override void Execute(GameTrigger trigger)
        {
            trigger.Reply("Ip connectés: " + WorldServer.Instance.ClientManager.Clients.GroupBy(x => x.IP).Count());
        }
    }
    public class InfoCommand : SubCommandContainer
    {
        public InfoCommand()
        {
            Aliases = new[] { "infoserver" };
            RequiredRole = RoleEnum.Developer;
            Description = "Exibe informações a respeito do servidor!";
            Description_en = "Displays information about the server!";
            Description_es = "Muestra información acerca del servidor!";
            Description_fr = "Affiche des informations sur le serveur!";
        }

        public override void Execute(TriggerBase trigger)
        {
            if (trigger.Args.HasNext)
                base.Execute(trigger);
            else
            {
                string spell_text = ""; 
                string spell_text_bugs = "";
                var spellsblok = ((GameTrigger)trigger).Character.SpellsBlock_temp;
                var spellsbugs = ((GameTrigger)trigger).Character.SpellsBugs_temp;
                foreach (var spellid in spellsblok)
                {
                    spell_text += ",{spell," + spellid + ",1}";
                }
                foreach (var spellid in spellsbugs)
                {
                    spell_text_bugs += ",{spell," + spellid + ",1}";
                }
                switch (((GameTrigger)trigger).Character.Account.Lang)
                {
                    case "fr":
                        trigger.Reply("Temps : " + trigger.Bold("{0}") + " Joueurs en Ligne : " + trigger.Bold("{1}") + " Record en Ligne: " + trigger.Bold("{2}"), WorldServer.Instance.UpTime.ToString(@"dd\.hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count, Game.Misc.ServerInfoManager.Instance.GetRecord());
                        if (spellsblok.Count() == 0)
                            spell_text = " Aucun";
                        if (spellsbugs.Count() == 0)
                            spell_text_bugs = " Aucun";
                        trigger.Reply("Sorts Désactivés: " + trigger.Bold("{0}") + ".", spell_text.Substring(1));
                        trigger.Reply("Sorts en analyses: " + trigger.Bold("{0}") + ".", spell_text_bugs.Substring(1));
                        break;
                    case "es":
                        trigger.Reply("Tiempo : " + trigger.Bold("{0}") + " Jugadores Online : " + trigger.Bold("{1}") + " Record Online: " + trigger.Bold("{2}"), WorldServer.Instance.UpTime.ToString(@"dd\.hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count, Game.Misc.ServerInfoManager.Instance.GetRecord());
                        if (spellsblok.Count() == 0)
                            spell_text = " Ninguna";
                        if (spellsbugs.Count() == 0)
                            spell_text_bugs = " Ninguna";
                        trigger.Reply("Hechizos Deshabilitados: " + trigger.Bold("{0}") + ".", spell_text.Substring(1));
                        trigger.Reply("Hechizos en análisis: " + trigger.Bold("{0}") + ".", spell_text_bugs.Substring(1));
                        break;
                    case "en":
                        trigger.Reply("Time : " + trigger.Bold("{0}") + " Players Online : " + trigger.Bold("{1}") + " Record Online: " + trigger.Bold("{2}"), WorldServer.Instance.UpTime.ToString(@"dd\.hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count, Game.Misc.ServerInfoManager.Instance.GetRecord());
                        if (spellsblok.Count() == 0)
                            spell_text = " None";
                        if (spellsbugs.Count() == 0)
                            spell_text_bugs = " None";
                        trigger.Reply("Disabled Spells: " + trigger.Bold("{0}") + ".", spell_text.Substring(1));
                        trigger.Reply("Spells in analysis: " + trigger.Bold("{0}") + ".", spell_text_bugs.Substring(1));
                        break;
                    default:
                        trigger.Reply("Tempo : " + trigger.Bold("{0}") + " Jogadores Online : " + trigger.Bold("{1}") + " Record Online: " + trigger.Bold("{2}"), WorldServer.Instance.UpTime.ToString(@"dd\.hh\:mm\:ss"), WorldServer.Instance.ClientManager.Count, Game.Misc.ServerInfoManager.Instance.GetRecord());
                        if (spellsblok.Count() == 0)
                            spell_text = " Nenhuma";
                        if (spellsbugs.Count() == 0)
                            spell_text_bugs = " Nenhuma";
                        trigger.Reply("Feitiços Desativados: " + trigger.Bold("{0}") + ".", spell_text.Substring(1));
                        trigger.Reply("Feitiços em análises: " + trigger.Bold("{0}") + ".", spell_text_bugs.Substring(1));
                        break;
                }
            }
        }
    }
    public class InfoMapCommand : SubCommand
    {
        public InfoMapCommand()
        {
            Aliases = new[] { "map" };
            RequiredRole = RoleEnum.Developer;
            Description = "Da informaciones del mapa";
            ParentCommandType = typeof(InfoCommand);
            AddParameter("map", "map", "Target map", isOptional: true, converter: ParametersConverter.MapConverter);
        }

        public override void Execute(TriggerBase trigger)
        {
            Map map = null;
            if (trigger.IsArgumentDefined("map"))
            {
                map = trigger.Get<Map>("map");
            }
            else if (trigger is GameTrigger)
            {
                map = (trigger as GameTrigger).Character.Map;
            }

            if (map == null)
            {
                trigger.ReplyError("Map not defined");
                return;
            }

            trigger.ReplyBold("Map {0} (relative : {1})", map.Id, map.RelativeId);
            trigger.ReplyBold("X:{0}, Y:{1}", map.Position.X, map.Position.Y);
            trigger.ReplyBold("SubArea:{0}, Area:{1}, SuperArea:{2}", map.SubArea.Id, map.Area.Id, map.SuperArea.Id);
            var actors = map.GetActors<RolePlayActor>().ToArray();
            trigger.ReplyBold("Actors ({0}) :", actors.Length);

            foreach (var actor in actors)
            {
                trigger.ReplyBold("- {0} : {1}", actor.GetType().Name, actor);
            }

            trigger.ReplyBold("SpawningPools ({0}) :", map.SpawningPools.Count);

            foreach (var pool in map.SpawningPools)
            {
                trigger.ReplyBold("- {0} : State : {1}, Next : {2}s", pool.GetType().Name, pool.State, pool.RemainingTime / 1000);
            }
            trigger.ReplyBold("Mapas Cordenadas : {0}", string.Join(",", World.Instance.GetMapsCordinates(map.Id)));
            
        }
    }

    public class InfoAreaCommand : SubCommand
    {
        public InfoAreaCommand()
        {
            Aliases = new[] { "area" };
            RequiredRole = RoleEnum.Developer;
            Description = "Give informations about an area";
            ParentCommandType = typeof(InfoCommand);
            AddParameter("area", "area", "Target area", isOptional: true, converter: ParametersConverter.AreaConverter);
        }

        public override void Execute(TriggerBase trigger)
        {
            Area area = null;
            if (trigger.IsArgumentDefined("area"))
            {
                area = trigger.Get<Area>("area");
            }
            else if (trigger is GameTrigger)
            {
                area = (trigger as GameTrigger).Character.Area;
            }

            if (area == null)
            {
                trigger.ReplyError("Area not defined");
                return;
            }

            trigger.ReplyBold("Area {0} ({1})", area.Name, area.Id);
            trigger.ReplyBold("Enabled : {0}", area.IsRunning);
            trigger.ReplyBold("Objects : {0}", area.ObjectCount);
            trigger.ReplyBold("Timers : {0}", area.TimersCount);
            trigger.ReplyBold("MsgQueue : {0}", area.MsgQueueCount);
            trigger.ReplyBold("Update interval : {0}ms", area.UpdateDelay);
            trigger.ReplyBold("AvgUpdateTime : {0}ms", area.AverageUpdateTime);
            trigger.ReplyBold("LastUpdate : {0}", area.LastUpdateTime);
            trigger.ReplyBold("Is Updating : {0}", area.IsUpdating);
            trigger.ReplyBold("Is Disposed : {0}", area.IsDisposed);
            trigger.ReplyBold("Current Thread : {0}", area.CurrentThreadId);

            var entries =
                BenchmarkManager.Instance.Entries.Where(
                    x => x.AdditionalProperties.ContainsKey("area") && x.AdditionalProperties["area"].Equals(area.Id));

            var groups = entries.GroupBy(x => x.MessageType);

            foreach (var group in groups.Take(5))
            {
                var average = group.Average(x => x.Timestamp.TotalMilliseconds);
                var errors = group.Where(x => x.AdditionalProperties.ContainsKey("exception"));

                trigger.ReplyBold("- {0} {1} ms {2} entries {3} errors", group.Key, average, group.Count(), errors.Count());
            }
        }
    }

    public class InfoCharacterCommand : TargetSubCommand
    {
        public InfoCharacterCommand()
        {
            Aliases = new[] { "character", "char", "player" };
            RequiredRole = RoleEnum.Developer;
            Description = "Give informations about a player";
            ParentCommandType = typeof(InfoCommand);
            AddTargetParameter();
            AddParameter<bool>("stats", "s", "Gives informations about his stats too", isOptional: true);
        }

        public override void Execute(TriggerBase trigger)
        {
            foreach (var character in GetTargets(trigger))
            {
                trigger.ReplyBold("{0} ({1})", character.Name, character.Id);
                trigger.ReplyBold("Account : {0}/{1} ({2}) - {3}", character.Account.Login, character.Account.Nickname,
                    character.Account.Id, character.UserGroup.Name);
                trigger.ReplyBold("HardwareId : {0}", character.Account.LastHardwareId);
                trigger.ReplyBold("Ip : {0}", character.UserGroup.Role == RoleEnum.Administrator ? "127.0.0.1" : character.Client.IP);
                trigger.ReplyBold("Level : {0}", character.Level);
                trigger.ReplyBold("Map : {0}, Cell : {1}, Direction : {2} ({3})", character.Map.Id, character.Cell.Id,
                    character.Direction,character.Direction.GetHashCode());
                trigger.ReplyBold("Kamas : {0}", character.Inventory.Kamas);
                trigger.ReplyBold("Items : {0}", character.Inventory.Count);
                trigger.ReplyBold("Spells : {0}", character.Spells.Count());               
                trigger.ReplyBold("Tokens : {0}", character.Inventory.Tokens?.Stack == null ? 0 : character.Inventory.Tokens?.Stack);
                trigger.ReplyBold("Fight : {0}",
                    character.IsFighting() ? character.Fight.Id.ToString(CultureInfo.InvariantCulture) : "Not fighting");
                trigger.ReplyBold("{0},{1},{2}", character.Map.Id, character.Cell.Id,
                    (int)character.Direction);

                if (!trigger.Get<bool>("stats"))
                    return;

                trigger.ReplyBold("Spells Points : {0}, Stats Points : {1}", character.SpellsPoints,
                    character.StatsPoints);
                trigger.ReplyBold("Health : {0}/{1}", character.Stats.Health.Total, character.Stats.Health.TotalMax);
                trigger.ReplyBold("AP : {0}, PM : {1}", character.Stats.AP, character.Stats.MP);
                trigger.ReplyBold("Vitality : {0}, Wisdom : {1}", character.Stats.Vitality, character.Stats.Wisdom);
                trigger.ReplyBold("Strength : {0}, Intelligence : {1}", character.Stats.Strength,
                    character.Stats.Intelligence);
                trigger.ReplyBold("Agility : {0}, Chance : {1}", character.Stats.Agility, character.Stats.Chance);
            }
        }
    }

    public class InfoFightCommand : SubCommand
    {
        public InfoFightCommand()
        {
            Aliases = new[] { "fight" };
            RequiredRole = RoleEnum.Developer;
            Description = "Give informations about a fight";
            ParentCommandType = typeof(InfoCommand);
            AddParameter("fight", "f", "Gives informations about the given fight", converter: ParametersConverter.FightConverter);
        }


        public override void Execute(TriggerBase trigger)
        {
            var fight = trigger.Get<IFight>("fight");

            trigger.ReplyBold("Fight {0}", fight.Id);
            trigger.ReplyBold("State : {0} Started Since : {1}", fight.State,
                              fight.IsStarted ? (DateTime.Now - fight.StartTime).ToString(@"m\mss\s") : "not");
            trigger.ReplyBold("Blue team ({0}) :", fight.DefendersTeam.Fighters.Count);

            foreach (var fighter in fight.DefendersTeam.Fighters)
            {
                trigger.ReplyBold(" - {0} : {1} Level : {2}{3}", fighter.GetType().Name, fighter, fighter.Level,
                    fighter.IsDead() ? " DEAD" : string.Empty);
            }

            trigger.ReplyBold("Red team ({0}) :", fight.ChallengersTeam.Fighters.Count);

            foreach (var fighter in fight.ChallengersTeam.Fighters)
            {
                trigger.ReplyBold(" - {0} : {1} Level : {2}{3}", fighter.GetType().Name, fighter, fighter.Level,
                    fighter.IsDead() ? " DEAD" : string.Empty);
            }

            trigger.ReplyBold("Spectators ({0}) :", fight.Spectators.Count);

            foreach (var spectator in fight.Spectators)
            {
                trigger.ReplyBold(" - {0}", spectator.Character);
            }
        }
    }

    public class InfoIPCommand : SubCommand
    {
        public InfoIPCommand()
        {
            Aliases = new[] { "ip" };
            RequiredRole = RoleEnum.Developer;
            Description = "Give informations about an ip";
            ParentCommandType = typeof(InfoCommand);
            AddParameter<string>("ip", "i", "Gives informations about the given ip");
        }


        public override void Execute(TriggerBase trigger)
        {
            var ip = trigger.Get<string>("ip");

            try
            {
                IPAddressRange.Parse(ip);
            }
            catch
            {
                trigger.ReplyError("IP format '{0}' incorrect", ip);
                return;
            }

            var characters = World.Instance.GetCharacters(x => x.Client.IP == ip).ToArray();

            if (!characters.Any())
            {
                trigger.ReplyError("No results found !", ip);
                return;
            }

            trigger.ReplyBold("Connected characters:");
            foreach (var character in characters)
            {
                trigger.Reply(string.Format("Player: {0} - Account: {1}({2})", character.Name, character.Account.Login, character.Account.LastHardwareId));
            }
        }
    }
}