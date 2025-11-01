using Stump.Core.Collections;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Handlers.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("AscensionBasic", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class AscensionReply : NpcReply
    {
        //Recompensas
        const int Mount01 = 22986; //Grifforis, o Bebê
        const int Mount02 = 22988; //Grifforis, o Imaturo
        const int Mount03 = 22990; //Grifforis, o Corajoso
        const int Mount04 = 22992; //Grifforis, o Sábio
        const int Mount05 = 22994; //Grifforis, o Campeão

        //Certificados
        const int Certi01 = 30011; //Certificado da Ascensão 1/6
        const int Certi02 = 30012; //Certificado da Ascensão 2/6
        const int Certi03 = 30013; //Certificado da Ascensão 3/6
        const int Certi04 = 30014; //Certificado da Ascensão 4/6
        const int Certi05 = 30015; //Certificado da Ascensão 5/6

        private readonly ConcurrentList<Character> m_members = new ConcurrentList<Character>();

        public AscensionReply(NpcReplyRecord record) : base(record)
        { }

        public IEnumerable<Character> Members
        {
            get { return m_members; }
        }

        private long[] maps_safe = { 176818181, 176818435, 176819201, 176819203, 176819205, 176819457, 176820225, 176820227, 176820229, 176820481, 176820483, 172492290 };

        public override bool Execute(Npc npc, Character character)
        {
            #region // ----------------- DG AscensionBasic - Controle de Recompensas By:Kenshin ---------------- //
            if (character.IsInParty())
            {
                foreach (var TeamCharacter in character.Party.Members.Take(8))
                {
                    // Aqui Verificamos se o Membro do grupo possui a recompensa e se não possuir ele irá ganhar.
                    if (!TeamCharacter.IsInFight() && TeamCharacter != character && maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter.AscensionBasicStair == 25 && !TeamCharacter.Inventory.Any(entry => entry.Template.Id == Certi01))
                    {
                        var itemTemplateRecomp01 = Singleton<ItemManager>.Instance.TryGetTemplate(Mount01);
                        var itemTemplateRecomp02 = Singleton<ItemManager>.Instance.TryGetTemplate(Certi01);

                        TeamCharacter.Inventory.AddItem(itemTemplateRecomp01, 1);
                        TeamCharacter.Inventory.AddItem(itemTemplateRecomp02, 1);
                    }
                    else if (!TeamCharacter.IsInFight() && TeamCharacter != character && maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter.AscensionBasicStair == 50 && !TeamCharacter.Inventory.Any(entry => entry.Template.Id == Certi02))
                    {
                        var itemTemplateRecomp01 = Singleton<ItemManager>.Instance.TryGetTemplate(Mount02);
                        var itemTemplateRecomp02 = Singleton<ItemManager>.Instance.TryGetTemplate(Certi02);

                        TeamCharacter.Inventory.AddItem(itemTemplateRecomp01, 1);
                        TeamCharacter.Inventory.AddItem(itemTemplateRecomp02, 1);
                    }
                    else if (!TeamCharacter.IsInFight() && TeamCharacter != character && maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter.AscensionBasicStair == 75 && !TeamCharacter.Inventory.Any(entry => entry.Template.Id == Certi03))
                    {
                        var itemTemplateRecomp01 = Singleton<ItemManager>.Instance.TryGetTemplate(Mount03);
                        var itemTemplateRecomp02 = Singleton<ItemManager>.Instance.TryGetTemplate(Certi03);

                        TeamCharacter.Inventory.AddItem(itemTemplateRecomp01, 1);
                        TeamCharacter.Inventory.AddItem(itemTemplateRecomp02, 1);
                    }
                    else if (!TeamCharacter.IsInFight() && TeamCharacter != character && maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter.AscensionBasicStair == 85 && !TeamCharacter.Inventory.Any(entry => entry.Template.Id == Certi04))
                    {
                        var itemTemplateRecomp01 = Singleton<ItemManager>.Instance.TryGetTemplate(Mount04);
                        var itemTemplateRecomp02 = Singleton<ItemManager>.Instance.TryGetTemplate(Certi04);

                        TeamCharacter.Inventory.AddItem(itemTemplateRecomp01, 1);
                        TeamCharacter.Inventory.AddItem(itemTemplateRecomp02, 1);
                    }
                    else if (!TeamCharacter.IsInFight() && TeamCharacter != character && maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter.AscensionBasicStair == 97 && !TeamCharacter.Inventory.Any(entry => entry.Template.Id == Certi05))
                    {
                        var itemTemplateRecomp01 = Singleton<ItemManager>.Instance.TryGetTemplate(Mount05);
                        var itemTemplateRecomp02 = Singleton<ItemManager>.Instance.TryGetTemplate(Certi05);

                        TeamCharacter.Inventory.AddItem(itemTemplateRecomp01, 1);
                        TeamCharacter.Inventory.AddItem(itemTemplateRecomp02, 1);
                    }

                    // Aqui Verificamos se por algum motivo o membro não possuir os certificados.. ele terá que pegar para iniciar a partida
                    if (maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter != character && TeamCharacter.AscensionBasicStair == 25 && !TeamCharacter.Inventory.Any(entry => entry.Template.Id == Certi01))
                    {
                        MensagemCertificado(character);
                        return false;
                    }
                    else if (maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter != character && TeamCharacter.AscensionBasicStair == 50 && !TeamCharacter.Inventory.Any(entry => entry.Template.Id == Certi02))
                    {
                        MensagemCertificado(character);
                        return false;
                    }
                    else if (maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter != character && TeamCharacter.AscensionBasicStair == 75 && !TeamCharacter.Inventory.Any(entry => entry.Template.Id == Certi03))
                    {
                        MensagemCertificado(character);
                        return false;
                    }
                    else if (maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter != character && TeamCharacter.AscensionBasicStair == 85 && !TeamCharacter.Inventory.Any(entry => entry.Template.Id == Certi04))
                    {
                        MensagemCertificado(character);
                        return false;
                    }
                    else if (maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter != character && TeamCharacter.AscensionBasicStair == 97 && !TeamCharacter.Inventory.Any(entry => entry.Template.Id == Certi05))
                    {
                        MensagemCertificado(character);
                        return false;
                    }
                }
            }
            #endregion

            if (character.GetAscensionBasicStair() == 99)
            {
                Map mapExit = Game.World.Instance.GetMap(172492290);

                character.SetAscensionBasicStair(0);
                character.Teleport(mapExit, mapExit.GetCell(326));

                return true;
            }

            var actualStairMap = AscensionMonoEnum.getAscensionFloorMapBasic(character.AscensionBasicStair)[0];

            Map map = Game.World.Instance.GetMap((uint)actualStairMap);

            var actualStairCell = AscensionMonoEnum.getAscensionFloorMapBasic(character.AscensionBasicStair)[1];

            int[] actualStairMonsters = AscensionMonoEnum.getAscensionFloorMonstersBasic(character.AscensionBasicStair);

            StartTeleport(character, map, actualStairCell);
            StartFight(character, map, actualStairCell, actualStairMonsters);

            return true;
        }

        public void MensagemCertificado(Character character)
        {
            switch (character.Account.Lang)
            {
                case "fr":
                    character.SendServerMessage("Le Membre : " + character.Namedefault + " n'a pas le certificat pour passer l'étage de la tour.");
                    break;
                case "es":
                    character.SendServerMessage("El Socio: " + character.Namedefault + " no tiene el certificado para pasar el piso de la torre.");
                    break;
                case "en":
                    character.SendServerMessage("The Member: " + character.Namedefault + " does not have the certificate to pass the tower floor.");
                    break;
                default:
                    character.SendServerMessage("O Membro: " + character.Namedefault + " não possui o certificado para passar o andar da torre.");
                    break;
            }
        }

        public void StartTeleport(Character character, Map map, int cell)
        {
            if (character.AscensionBasicStair == 99)
                return;

            if (!Game.World.Instance.GetMap(map.Id).Area.IsRunning) Game.World.Instance.GetMap(map.Id).Area.Start();
            {
                if (character.IsInParty())
                {
                    foreach (var TeamCharacter in character.Party.Members)
                    {
                        if (!TeamCharacter.IsInFight() && maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter.AscensionBasicStair == character.AscensionBasicStair)
                        {
                            TeamCharacter.Teleport(map, map.GetCell(cell));
                            #region Text Information
                            switch (character.Account.Lang)
                            {
                                case "fr":
                                    TeamCharacter.DisplayNotification($"Vous avez été téléporté à l'étage " + (TeamCharacter.AscensionBasicStair + 1) + " de la Tour de l'Ascension.", NotificationEnum.INFORMATION);
                                    break;
                                case "es":
                                    TeamCharacter.DisplayNotification($"Has sido teletransportado arriba " + (TeamCharacter.AscensionBasicStair + 1) + " de la Torre de la Ascensión.", NotificationEnum.INFORMATION);
                                    break;
                                case "en":
                                    TeamCharacter.DisplayNotification($"You have been teleported upstairs " + (TeamCharacter.AscensionBasicStair + 1) + " of the Tower of Ascension.", NotificationEnum.INFORMATION);
                                    break;
                                default:
                                    TeamCharacter.DisplayNotification($"Você foi teletransportado para o andar de cima " + (TeamCharacter.AscensionBasicStair + 1) + " da Torre da Ascensão.", NotificationEnum.INFORMATION);
                                    break;
                            }
                            #endregion
                        }
                        else
                        {
                            character.Teleport(map, map.GetCell(cell));
                            #region Text Information
                            switch (character.Account.Lang)
                            {
                                case "fr":
                                    character.DisplayNotification($"Vous avez été téléporté à l'étage " + (character.AscensionBasicStair + 1) + " de la Tour de l'Ascension.", NotificationEnum.INFORMATION);
                                    break;
                                case "es":
                                    character.DisplayNotification($"Has sido teletransportado arriba " + (character.AscensionBasicStair + 1) + " de la Torre de la Ascensión.", NotificationEnum.INFORMATION);
                                    break;
                                case "en":
                                    character.DisplayNotification($"You have been teleported upstairs " + (character.AscensionBasicStair + 1) + " of the Tower of Ascension.", NotificationEnum.INFORMATION);
                                    break;
                                default:
                                    character.DisplayNotification($"Você foi teletransportado para o andar de cima " + (character.AscensionBasicStair + 1) + " da Torre da Ascensão.", NotificationEnum.INFORMATION);
                                    break;
                            }
                            #endregion
                        }
                    }
                }
                else
                {
                    character.Teleport(map, map.GetCell(cell));
                    #region Text Information
                    switch (character.Account.Lang)
                    {
                        case "fr":
                            character.DisplayNotification($"Vous avez été téléporté à l'étage " + (character.AscensionBasicStair + 1) + " de la Tour de l'Ascension.", NotificationEnum.INFORMATION);
                            break;
                        case "es":
                            character.DisplayNotification($"Has sido teletransportado arriba " + (character.AscensionBasicStair + 1) + " de la Torre de la Ascensión.", NotificationEnum.INFORMATION);
                            break;
                        case "en":
                            character.DisplayNotification($"You have been teleported upstairs " + (character.AscensionBasicStair + 1) + " of the Tower of Ascension.", NotificationEnum.INFORMATION);
                            break;
                        default:
                            character.DisplayNotification($"Você foi teletransportado para o andar de cima " + (character.AscensionBasicStair + 1) + " da Torre da Ascensão.", NotificationEnum.INFORMATION);
                            break;
                    }
                    #endregion
                }
            }
        }

        public void StartFight(Character character, Map map, int cell, int[] monsters)
        {
            if (character.AscensionBasicStair == 99)
                return;

            Task.Delay(1000).ContinueWith(t =>
            {
                var clients = Members.Where(x => x.Fight != character.Fight).ToClients();
                var fight = Singleton<FightManager>.Instance.CreatePvMFight(character.Map);
                int ModularCount = 4;
                int ModularGrade = 1;

                fight.ChallengersTeam.AddFighter(character.CreateFighter(fight.ChallengersTeam));

                #region // ----------------- DG AscensionBasic - Sistema de Moldulagem da Luta By:Kenshin ---------------- //
                if (!character.IsInParty())
                {
                    if (character.Level <= 50)
                        ModularGrade = 1;
                    else if (character.Level >= 51 && character.Level <= 100)
                        ModularGrade = 2;
                    else if (character.Level >= 101 && character.Level <= 150)
                        ModularGrade = 3;
                    else if (character.Level >= 151 && character.Level <= 190)
                        ModularGrade = 4;
                    else
                        ModularGrade = 5;
                }
                else
                {
                    var GroupLevelSum = character.Party.GroupLevelSum;
                    var MembersCount = character.Party.Members.Where(x => x.AscensionBasicStair == character.AscensionBasicStair && maps_safe.Contains(x.Map.Id)).ToList();
                    int GroupLevelAverage = 200;

                    GroupLevelAverage = GroupLevelSum / MembersCount.Count;

                    if (GroupLevelAverage <= 50)
                        ModularGrade = 1;
                    else if (GroupLevelAverage >= 51 && GroupLevelAverage <= 100)
                        ModularGrade = 2;
                    else if (GroupLevelAverage >= 101 && GroupLevelAverage <= 150)
                        ModularGrade = 3;
                    else if (GroupLevelAverage >= 151 && GroupLevelAverage <= 190)
                        ModularGrade = 4;
                    else
                        ModularGrade = 5;
                }

                if (character.IsInParty())
                {
                    var clientsgroup = character.Party.Members.Where(x => x.AscensionBasicStair == character.AscensionBasicStair && maps_safe.Contains(x.Map.Id)).ToList();

                    if (clientsgroup.Count <= 4)
                        ModularCount = 4;
                    else if (clientsgroup.Count == 5)
                        ModularCount = 5;
                    else if (clientsgroup.Count == 6)
                        ModularCount = 6;
                    else if (clientsgroup.Count == 7)
                        ModularCount = 7;
                    else
                        ModularCount = 8;
                }
                #endregion

                foreach (int m in monsters.Take(ModularCount))
                {
                    var grade = Singleton<MonsterManager>.Instance.GetMonsterGrade((int)m, ModularGrade);
                    var position = new ObjectPosition(map, map.GetCell(cell), (DirectionsEnum)5);
                    var monster = new Monster(grade, new MonsterGroup(0, position));

                    fight.DefendersTeam.AddFighter(new MonsterFighter(fight.DefendersTeam, monster));
                }

                fight.StartPlacement();


                if (character.IsInParty())
                {
                    foreach (var TeamCharacter in character.Party.Members)
                    {
                        if (!TeamCharacter.IsInFight() && maps_safe.Contains(TeamCharacter.Map.Id) && TeamCharacter.AscensionBasicStair == character.AscensionBasicStair)
                        {
                            ContextHandler.HandleGameFightJoinRequestMessage(TeamCharacter.Client, new GameFightJoinRequestMessage(character.Fighter.Id, (ushort)fight.Id));
                            TeamCharacter.SaveLater();
                        }
                        else
                        {
                            ContextHandler.HandleGameFightJoinRequestMessage(character.Client, new GameFightJoinRequestMessage(character.Fighter.Id, (ushort)fight.Id));
                            character.SaveLater();
                        }
                    }
                }
                else
                {
                    ContextHandler.HandleGameFightJoinRequestMessage(character.Client, new GameFightJoinRequestMessage(character.Fighter.Id, (ushort)fight.Id));
                    character.SaveLater();
                }

                return true;
            });
        }
    }
}