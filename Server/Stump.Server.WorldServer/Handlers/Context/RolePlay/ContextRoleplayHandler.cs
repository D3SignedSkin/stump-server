using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Arena;
using Stump.Server.WorldServer.Game.Breach;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Interactives.Skills;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Paddocks;
using Stump.Server.WorldServer.Handlers.Interactives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Handlers.Context.RolePlay
{
    public partial class ContextRoleplayHandler
    {
        static List<Tuple<long, long>> correctPositions = new List<Tuple<long, long>>
        {
            new Tuple<long, long>(189400321, 193201153),
            new Tuple<long, long>(189268225, 189399809),
            new Tuple<long, long>(189269249, 206307331),
            new Tuple<long, long>(108926977, 108925440),
        };

        [WorldHandler(GuidedModeReturnRequestMessage.Id)]
        public static void HandleGuidedModeReturnRequestMessage(WorldClient client, GuidedModeReturnRequestMessage message)
        {
            //if (message.MessageId == 6088)
            //{
            //    #region >> Message Information
            //    client.Character.SendServerMessageLang(
            //        "Informações: Para obter mais detalhes sobre o projeto, convidamos você a visitar nosso website. Acesse-o através deste link: <a href=\"http://serverhydra.com/pt\"><b>Clique Aqui!.</b></a> Lá você encontrará todas as informações necessárias e poderá explorar em profundidade o conteúdo relacionado ao projeto.",
            //        "Information: For more details about the project, we invite you to visit our website. Access it through this link: <a href=\"http://serverhydra.com/en\"><b>Click here!.</b></a> There you will find all the necessary information and you can explore in depth the content related to the project.",
            //        "Información: Para más detalles sobre el proyecto, lo invitamos a visitar nuestro sitio web. Accede a través de este enlace: <a href=\"http://serverhydra.com/es\"><b>Haz clic aquí!</b></a> Allí encontrarás toda la información necesaria y podrás explorar en profundidad el contenido relacionado con el proyecto.",
            //        "Information : Pour plus de détails sur le projet, nous vous invitons à visiter notre site Internet. Accédez-y via ce lien : <a href=\"http://serverhydra.com/fr\"><b>Cliquez ici !</b></a> Vous y trouverez toutes les informations nécessaires et vous pourrez explorer en profondeur le contenu lié au projet."
            //        );
            //    #endregion
            //}
            //else 
            //{
            //    var Map = World.Instance.GetMap(152305664);
            //    client.Character.Teleport(Map, Map.GetCell(342));
            //}

            //client.Character.StartQuest(1042);

            //var Map = World.Instance.GetMap(152305664);
            //client.Character.Teleport(Map, Map.GetCell(342));
        }

        [WorldHandler(GuidedModeQuitRequestMessage.Id)]
        public static void HandleGuidedModeQuitRequestMessage(WorldClient client, GuidedModeQuitRequestMessage message)
        {
            var map = World.Instance.GetMap(153092354);
            client.Character.Teleport(map, map.GetCell(328));
        }

        [WorldHandler(MapRunningFightListRequestMessage.Id)]
        public static void HandleMapRunningFightListRequestMessage(WorldClient client, MapRunningFightListRequestMessage message)
        {
            if (client.Character.Map.Id == ArenaManager.KolizeumMapId || client.Character.Map.Id == ArenaManager.AstrubMapId)
            {
                var mapList1vs1 = ArenaManager.Instance.Arenas_1vs1.SelectMany(x => x.Value.Map.Fights);
                var mapList3vs3 = ArenaManager.Instance.Arenas_3vs3.SelectMany(x => x.Value.Map.Fights);

                if (client.Character.Map.Id == ArenaManager.KolizeumMapId)
                    SendMapRunningFightListMessage(client, mapList1vs1.Concat(mapList3vs3), client.Character);
                else
                    SendMapRunningFightListMessage(client, mapList1vs1.Concat(mapList3vs3).Concat(client.Character.Map.Fights), client.Character);
            }
            else
            {
                var mapListFights = client.Character.Map.Fights;
                SendMapRunningFightListMessage(client, mapListFights, client.Character);
            }
        }

        [WorldHandler(MapRunningFightDetailsRequestMessage.Id)]
        public static void HandleMapRunningFightDetailsRequestMessage(WorldClient client, MapRunningFightDetailsRequestMessage message)
        {
            var fight = FightManager.Instance.GetFight(message.fightId);

            if (fight == null || (fight.Map != client.Character.Map && client.Character.Map.Id != ArenaManager.KolizeumMapId && client.Character.Map.Id != ArenaManager.AstrubMapId))
                return;

            SendMapRunningFightDetailsMessage(client, fight);
        }

        [WorldHandler(GameRolePlayFreeSoulRequestMessage.Id)]
        public static void HandleGameRoleplayFreeSoulRequestMessage(WorldClient client, GameRolePlayFreeSoulRequestMessage message)
        {
            client.Character.FreeSoul();
        }

        #region >> Information Maps
        [WorldHandler(MapInformationsRequestMessage.Id)]
        public static void HandleMapInformationsRequestMessage(WorldClient client, MapInformationsRequestMessage message)
        {
            if (client.Character.Map.Id != 195559424 && client.Character.Map.SubArea.Id == 904) //Iniciando em mapas dos Sonhos Infinito menos mapa Home
            {
                SendMapComplementaryInformationsBreachMessage(client);
            }
            else
            {
                SendMapComplementaryInformationsDataMessage(client);
            }

            if (client.Character.Map.Id == ArenaManager.KolizeumMapId || client.Character.Map.Id == ArenaManager.AstrubMapId)
            {
                var arenaCount = ArenaManager.Instance.Arenas_1vs1.Where(x => x.Value?.Map != null).Sum(x => x.Value.Map.GetFightCount()) + ArenaManager.Instance.Arenas_3vs3.Where(x => x.Value?.Map != null).Sum(x => x.Value.Map.GetFightCount());

                if (arenaCount > 0)
                {
                    SendMapFightCountMessage(client, (short)arenaCount);
                }
            }
            else if (client.Character.Map.GetFightCount() > 0)
            {
                SendMapFightCountMessage(client, client.Character.Map.GetFightCount());
            }

            var objectItems = client.Character.Map.GetObjectItems();

            foreach (var objectItem in objectItems.ToArray())
            {
                SendObjectGroundAddedMessage(client, objectItem);
            }

            var paddock = PaddockManager.Instance.GetPaddockByMap((uint)message.mapId);

            if (paddock != null)
            {
                if (paddock.Guild != null)
                {
                    client.Send(paddock.GetPaddockPropertiesGuildMessage());
                }
                else
                {
                    client.Send(paddock.GetPaddockPropertiesMessage());
                }
            }

            if (client.Character.Vip && client.Character?.KnownZaaps?.Any() == true)
            {
                List<double> zaaps = new List<double>();

                foreach (var map in from map in client.Character.SuperArea.Maps from interactive in map.GetInteractiveObjects().Where(interactive => interactive.Template != null && interactive.Template.Type == InteractiveTypeEnum.TYPE_ZAAP && interactive.Spawn.ElementId != 71355) select map)
                {
                    if (map.Position.X == -1 && map.Position.Y == 0) //Zaap Fantasma
                        continue;

                    zaaps.Add(map.Id);
                }

                client.Send(new KnownZaapListMessage(zaaps));
            }
            else
            {
                client.Send(new KnownZaapListMessage(client.Character.KnownZaaps.Select(zaap => (double)zaap.Id).ToList()));
            }

            foreach (var interactiveObject in client.Character.Map.GetInteractiveObjects())
            {
                InteractiveHandler.SendStatedElementUpdatedMessage(client, interactiveObject.Id, interactiveObject.Cell.Id, (int)interactiveObject.State);
            }

            var skills = client.Character.Map.GetInteractiveObjects().SelectMany(x => x.GetSkills().Where(y => (y is SkillCraft || y is SkillRuneCraft) && y.IsEnabled(client.Character)).Select(y => y.SkillTemplate)).Distinct();

            SendJobMultiCraftAvailableSkillsMessage(client, client.Character, skills, true);
            client.Character.OnKoh();
        }

        [WorldHandler(ChangeMapMessage.Id)]
        public static void HandleChangeMapMessage(WorldClient client, ChangeMapMessage message)
        {
            //if (client.UserGroup.Role >= RoleEnum.Moderator_Helper)
            //{
            //    client.Character.SendServerMessage($"Map Actual: {client.Character.Map.Id} Target MSG MapId: {message.mapId}");
            //}

            long mapMessageId = (long)message.mapId;

            //Correção Ponte de Pandala
            if (client.Character.Map.Id == 189399809 && mapMessageId == 189268225)
            {
                mapMessageId = 189268737;
            }
            //Frighots 3 - Passagem para Illizaely
            else if (client.Character.Map.Id == 108926976 && mapMessageId == 108926977)
            {
                List<short> listCells = new List<short>()
                {
                    547,
                    534,
                    520,
                    548,
                    535,
                };

                if (listCells.Contains(client.Character.Cell.Id))
                {
                    Map tagertmap = World.Instance.GetMap(183632898);
                    client.Character.Teleport(new ObjectPosition(tagertmap, tagertmap.GetCell(142), client.Character.Direction));

                    return;
                }
                else
                {
                    Map tagertmap = World.Instance.GetMap(108925440);
                    client.Character.Teleport(new ObjectPosition(tagertmap, tagertmap.GetCell(280), client.Character.Direction));

                    return;
                }
            }
            //Correção Ponte de Frighost 3
            else if (client.Character.Map.Id == 108926976 && mapMessageId == 108926464)
            {
                List<short> listCells = new List<short>()
                {
                    119,
                    133,
                    148,
                    120,
                    134,
                    121,
                };

                if (listCells.Contains(client.Character.Cell.Id))
                {
                    Map tagertmap = World.Instance.GetMap(108927235);
                    client.Character.Teleport(new ObjectPosition(tagertmap, tagertmap.GetCell(532), client.Character.Direction));
                }
                else
                {
                    Map tagertmap = World.Instance.GetMap(108927235);
                    client.Character.Teleport(new ObjectPosition(tagertmap, tagertmap.GetCell(549), client.Character.Direction));
                }

                return;
            }
            else if (correctPositions.Any(x => x.Item1 == mapMessageId))
            {
                mapMessageId = correctPositions.FirstOrDefault(x => x.Item1 == mapMessageId).Item2;
            }

            var neighbourState = client.Character.Map.GetClientMapRelativePosition(mapMessageId);
            var scrollAction = WorldMapScrollActionManager.Instance.GetWorldMapScroll(client.Character.Map);

            if (scrollAction != null)
            {
                Map newDestination = null;

                if (neighbourState == MapNeighbour.Top && scrollAction.TopMapId != 0)
                    newDestination = World.Instance.GetMap(scrollAction.TopMapId);

                if (neighbourState == MapNeighbour.Bottom && scrollAction.BottomMapId != 0)
                    newDestination = World.Instance.GetMap(scrollAction.BottomMapId);

                if (neighbourState == MapNeighbour.Left && scrollAction.LeftMapId != 0)
                    newDestination = World.Instance.GetMap(scrollAction.LeftMapId);

                if (neighbourState == MapNeighbour.Right && scrollAction.RightMapId != 0)
                    newDestination = World.Instance.GetMap(scrollAction.RightMapId);

                if (newDestination != null)
                {
                    client.Character.Teleport(newDestination, neighbourState);
                    return;
                }
            }

            // todo : check with MapChangeData the neighbour validity
            if (neighbourState != MapNeighbour.None && client.Character.Position.Cell.MapChangeData != 0)
                client.Character.Teleport(neighbourState);
        }

        public static void SendMapComplementaryInformationsBreachMessage(WorldClient client)
        {
            if (client.Character.BreachOwner == null)
            {
                if (client.Character?.BreachBranches == null)
                    client.Character.BreachBranches = BreachBranche.generateBreachBranches(client.Character);

                List<BreachBranch> breachBranches = new List<BreachBranch>();

                foreach (var breachBranch in client.Character.BreachBranches)
                {
                    BreachBranch branch = new BreachBranch(room: breachBranch.room, element: breachBranch.element, bosses: breachBranch.bosses, map: breachBranch.map, score: 1, relativeScore: 2, breachBranch.monsters);
                }

                client.Send(new BreachEnterMessage((ulong)client.Character.Id));
                client.Send(new BreachStateMessage(client.Character.GetCharacterMinimalInformations(), client.Character.BreachBoosts.ToArray(), (uint)client.Character.BreachBudget, true));

                foreach (var character in client.Character.Map.GetAllCharacters())
                {
                    client.Send(new BreachCharactersMessage(client.Character.Map.GetAllCharacters().Select(x => (ulong)x.Id).ToArray()));
                }

                client.Send(new MapComplementaryInformationsBreachMessage(
                    subAreaId: (ushort)client.Character.Map.SubArea.Id,
                    mapId: client.Character.Map.Id,
                    houses: new HouseInformations[0],
                    actors: client.Character.Map.Actors.Where(entry => entry.CanBeSee(client.Character)).Select(entry => entry.GetGameContextActorInformations(client.Character) as GameRolePlayActorInformations).ToArray(),
                    interactiveElements: client.Character.Map.GetInteractiveObjects().Where(entry => entry.CanBeSee(client.Character)).Select(entry => entry.GetInteractiveElementWithBonus(client.Character)).ToArray(),
                    statedElements: client.Character.Map.GetInteractiveObjects().Where(entry => entry.CanBeSee(client.Character)).Where(x => x.Animated).Select(entry => entry.GetStatedElement()).ToArray(),
                    obstacles: client.Character.Map.GetMapObstacles().ToArray(),
                    fights: client.Character.Map.Fights.Where(entry => entry.BladesVisible).Select(entry => entry.GetFightCommonInformations()).ToArray(),
                    hasAggressiveMonsters: false,
                    fightStartPositions: new FightStartingPositions(client.Character.Map.GetBlueFightPlacement().Select(x => (ushort)x.Id), client.Character.Map.GetRedFightPlacement().Select(x => (ushort)x.Id)),
                    floor: (uint)(200 + client.Character.BreachStep),
                    room: 0,
                    infinityMode: 0,
                    branches: breachBranches.ToArray()
                ));

                var cm = client.Character.Map.GetMapComplementaryInformationsDataMessage(client.Character);

                if (cm is MapComplementaryInformationsBreachMessage)
                    client.Send(cm);
            }
            else
            {
                List<BreachBranch> breachBranches = new List<BreachBranch>();

                foreach (var breachBranch in client.Character.BreachOwner.BreachBranches)
                {
                    BreachBranch branch = new BreachBranch(room: breachBranch.room, element: breachBranch.element, bosses: breachBranch.bosses, map: breachBranch.map, score: 1, relativeScore: 2, breachBranch.monsters);
                }

                client.Send(new BreachEnterMessage((ulong)client.Character.BreachOwner.Id));
                client.Send(new BreachStateMessage(client.Character.BreachOwner.GetCharacterMinimalInformations(), client.Character.BreachOwner.BreachBoosts.ToArray(), (uint)client.Character.BreachOwner.BreachBudget, true));

                foreach (var character in client.Character.Map.GetAllCharacters())
                {
                    client.Send(new BreachCharactersMessage(client.Character.Map.GetAllCharacters().Select(x => (ulong)x.Id).ToArray()));
                }

                client.Send(new MapComplementaryInformationsBreachMessage(
                    subAreaId: (ushort)client.Character.Map.SubArea.Id,
                    mapId: client.Character.Map.Id,
                    houses: new HouseInformations[0],
                    actors: client.Character.Map.Actors.Where(entry => entry.CanBeSee(client.Character)).Select(entry => entry.GetGameContextActorInformations(client.Character) as GameRolePlayActorInformations).ToArray(),
                    interactiveElements: client.Character.Map.GetInteractiveObjects().Where(entry => entry.CanBeSee(client.Character)).Select(entry => entry.GetInteractiveElementWithBonus(client.Character)).ToArray(),
                    statedElements: client.Character.Map.GetInteractiveObjects().Where(entry => entry.CanBeSee(client.Character)).Where(x => x.Animated).Select(entry => entry.GetStatedElement()).ToArray(),
                    obstacles: client.Character.Map.GetMapObstacles().ToArray(),
                    fights: client.Character.Map.Fights.Where(entry => entry.BladesVisible).Select(entry => entry.GetFightCommonInformations()).ToArray(),
                    hasAggressiveMonsters: false,
                    fightStartPositions: new FightStartingPositions(client.Character.Map.GetBlueFightPlacement().Select(x => (ushort)x.Id), client.Character.Map.GetRedFightPlacement().Select(x => (ushort)x.Id)),
                    floor: (uint)(200 + client.Character.BreachStep),
                    room: client.Character.CurrentBreachRoom.room,
                    infinityMode: 0,
                    branches: breachBranches.ToArray()
                ));

                var cm = client.Character.Map.GetMapComplementaryInformationsDataMessage(client.Character);

                if (cm is MapComplementaryInformationsBreachMessage)
                    client.Send(cm);
            }
        }
        #endregion

        public static void SendMapRunningFightListMessage(IPacketReceiver client, IEnumerable<IFight> fights, Character character)
        {
            client.Send(new MapRunningFightListMessage(fights.Select(entry => entry.GetFightExternalInformations(character))));
        }

        public static void SendGameRolePlayShowActorMessage(IPacketReceiver client, Character character, RolePlayActor actor)
        {
            client.Send(new GameRolePlayShowActorMessage(actor.GetGameContextActorInformations(character) as GameRolePlayActorInformations));
        }

        public static void SendObjectGroundAddedMessage(IPacketReceiver client, WorldObjectItem objectItem)
        {
            client.Send(new ObjectGroundAddedMessage((ushort)objectItem.Cell.Id, (ushort)objectItem.Item.Id));
        }

        public static void SendObjectGroundRemovedMessage(IPacketReceiver client, WorldObjectItem objectItem)
        {
            client.Send(new ObjectGroundRemovedMessage((ushort)objectItem.Cell.Id));
        }

        public static void SendMapRunningFightDetailsMessage(IPacketReceiver client, IFight fight)
        {
            var redFighters = fight.ChallengersTeam.GetAllFighters(x => !(x is SummonedFighter) && !(x is SummonedBomb)).ToArray();
            var blueFighters = fight.DefendersTeam.GetAllFighters(x => !(x is SummonedFighter) && !(x is SummonedBomb)).ToArray();
            var partiesName = fight.GetPartiesName().ToArray();

            if (partiesName.Length > 0)
            {
                client.Send(new MapRunningFightDetailsExtendedMessage((ushort)fight.Id, redFighters.Select(entry => entry.GetGameFightFighterLightInformations()).ToArray(), blueFighters.Select(entry => entry.GetGameFightFighterLightInformations()).ToArray(), partiesName));
            }
            else
            {
                client.Send(new MapRunningFightDetailsMessage((ushort)fight.Id, redFighters.Select(entry => entry.GetGameFightFighterLightInformations()).ToArray(), blueFighters.Select(entry => entry.GetGameFightFighterLightInformations()).ToArray()));
            }
        }

        public static void SendMapFightCountMessage(IPacketReceiver client, short fightsCount)
        {
            client.Send(new MapFightCountMessage((ushort)fightsCount));
        }

        public static void SendMapComplementaryInformationsDataMessage(WorldClient client)
        {
            var cm = client.Character.Map.GetMapComplementaryInformationsDataMessage(client.Character);
            client.Send(cm);// resend because dont show when login

            if (cm is MapComplementaryInformationsDataInHavenBagMessage)
            {
                var havenbag = Game.HavenBags.HavenBagManager.Instance.GetHavenBagByOwner(client.Character.Record.HavenBagOwnerId);
                var havenbagfurnitures = Game.HavenBags.HavenBagManager.Instance.GetHavenBagFurnitures().Where(x => x.HavenBagId == havenbag.HavenBagId).Select(v => v.HavenBagFurnitureInformation).ToArray();

                ContextHandler.SendHavenBagFurnituresMessage(client, havenbagfurnitures);
                ContextHandler.SendHavenBagPermissionsUpdateMessage(client, Game.HavenBags.HavenBagManager.Instance.GetHavenBagPermissions(havenbag.FriendsAllowed, havenbag.GuildAllowed));
            }
        }

        public static void SendCurrentMapMessage(IPacketReceiver client, long mapId)
        {
            client.Send(new CurrentMapMessage(mapId));
        }
    }
}