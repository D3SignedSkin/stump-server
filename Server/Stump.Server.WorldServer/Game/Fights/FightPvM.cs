using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Discord;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights.Challenges;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Formulas;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Handlers.Context;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Fights
{
    public class FightPvM : Fight<FightMonsterTeam, FightPlayerTeam>
    {
        private bool m_ageBonusDefined;

        public FightPvM(int id, Map fightMap, FightMonsterTeam defendersTeam, FightPlayerTeam challengersTeam) : base(id, fightMap, defendersTeam, challengersTeam)
        { }

        public override void StartPlacement()
        {
            base.StartPlacement();

            m_placementTimer = Map.Area.CallDelayed(FightConfiguration.PlacementPhaseTime, StartFighting);
        }

        public override void StartFighting()
        {
            if (m_placementTimer != null)
            {
                m_placementTimer.Dispose();
                m_placementTimer = null;
            }

            int challengeAmount = ContextHandler.GetChallengeCount(this);

            ForEach(entry => ContextHandler.SendChallengeFightValidated(entry.Client, this, challengeAmount), false);

            base.StartFighting();
        }

        protected override void OnFightStarted()
        {
            base.OnFightStarted();
        }

        protected override void OnFighterAdded(FightTeam team, FightActor actor)
        {
            base.OnFighterAdded(team, actor);

            if (!(team is FightMonsterTeam) || m_ageBonusDefined)
                return;

            MonsterFighter monsterFighter = team.Leader as MonsterFighter;

            if (monsterFighter != null)
                AgeBonus = monsterFighter.Monster.Group.AgeBonus;

            m_ageBonusDefined = true;
        }

        public FightPlayerTeam PlayerTeam => Teams.FirstOrDefault(x => x.TeamType == TeamTypeEnum.TEAM_TYPE_PLAYER) as FightPlayerTeam;

        public FightMonsterTeam MonsterTeam => Teams.FirstOrDefault(x => x.TeamType == TeamTypeEnum.TEAM_TYPE_MONSTER) as FightMonsterTeam;

        public override FightTypeEnum FightType => FightTypeEnum.FIGHT_TYPE_PvM;

        public override bool IsPvP => false;

        public bool IsPvMArenaFight { get; set; }

        protected override List<IFightResult> GetResults()
        {
            var results = new List<IFightResult>();
            results.AddRange(GetFightersAndLeavers().Where(entry => entry.HasResult).Select(entry => entry.GetFightResult()));

            try
            {
                if (Map.IsDungeon())
                {
                    // var taxCollectors = Map.SubArea.Maps.Select(x => x.TaxCollector).Where(x => x != null && x.CanGatherLoots()).FirstOrDefault();

                    var taxCollectors = Map.TaxCollector;

                    if (taxCollectors != null)
                        results.Add(new TaxCollectorProspectingResult(taxCollectors, this));

                }
                else
                {
                    //var taxCollectors = Map.SubArea.Maps.Select(x => x.TaxCollector).Where(x => x != null && x.CanGatherLoots()).FirstOrDefault();

                    var taxCollectors = Map.TaxCollector; // the players choise this!

                    if (taxCollectors != null)
                        results.Add(new TaxCollectorProspectingResult(taxCollectors, this));
                }

                foreach (var team in m_teams)
                {
                    IEnumerable<FightActor> droppers = team.OpposedTeam.GetAllFighters(entry => entry.IsDead() && entry.CanDrop()).ToList();
                    var looters = results.Where(x => x.CanLoot(team) && !(x is TaxCollectorProspectingResult)).OrderByDescending(entry => entry.Prospecting).Concat(results.OfType<TaxCollectorProspectingResult>().Where(x => x.CanLoot(team)).OrderByDescending(x => x.Prospecting)); // tax collector loots at the end
                    var teamPP = team.GetAllFighters<CharacterFighter>().Sum(entry => (entry.Stats[PlayerFields.Prospecting].Total >= 100) ? 100 : entry.Stats[PlayerFields.Prospecting].Total);
                    var kamas = Winners == team ? droppers.Sum(entry => entry.GetDroppedKamas()) : 0;

                    foreach (var looter in looters)
                    {
                        looter.Loot.Kamas = teamPP > 0 ? FightFormulas.AdjustDroppedKamas(looter, teamPP, kamas) : 0;

                        if (team == Winners)
                        {
                            foreach (var item in droppers.SelectMany(dropper => dropper.RollLoot(looter)))
                            {
                                looter.Loot.AddItem(item);
                            }

                            if (team == Winners && looter is FightPlayerResult)
                            {
                                Character character = (looter as FightPlayerResult).Character;

                                foreach (MonsterFighter monster in this.DefendersTeam.GetAllFighters().Where(x => x is MonsterFighter))
                                {
                                    if (monster is MonsterFighter)
                                    {
                                        BasePlayerItem Arme = character.Inventory.TryGetItem(CharacterInventoryPositionEnum.ACCESSORY_POSITION_WEAPON);
                                        var items = character.Inventory.GetEquipedItems();

                                        #region // ----------------- Sistema de Drop Profissão Caça By:Kenshin (DESATIVADO) ---------------- //
                                        //if (Arme != null)
                                        //{
                                        //    foreach (var item in items.Where(x => x.Effects.Exists(y => y.EffectId == EffectsEnum.Effect_795)))
                                        //    {
                                        //        if (item != null)
                                        //            JobHunting(character, looter);
                                        //    }
                                        //}
                                        #endregion

                                        #region // ----------------- Drop de Broches Skonk By:Kenshin ---------------- //
                                        if (Map.IsDungeon() && Map.SubArea.Id == 254)
                                        {
                                            if (monster.Monster.Template.Id == 675 && !character.Inventory.Any(entry => entry.Template.Id == 7935)) //Mob Safira
                                            {
                                                looter.Loot.AddItem(new DroppedItem(7935, 1));
                                            }
                                            else if (monster.Monster.Template.Id == 681 && !character.Inventory.Any(entry => entry.Template.Id == 7938)) //Mob Diamantina
                                            {
                                                looter.Loot.AddItem(new DroppedItem(7938, 1));
                                            }
                                            else if (monster.Monster.Template.Id == 673 && !character.Inventory.Any(entry => entry.Template.Id == 7936)) //Mob Esmeralda
                                            {
                                                looter.Loot.AddItem(new DroppedItem(7936, 1));
                                            }
                                            else if (monster.Monster.Template.Id == 677 && !character.Inventory.Any(entry => entry.Template.Id == 7937)) //Mob Rubina
                                            {
                                                looter.Loot.AddItem(new DroppedItem(7937, 1));
                                            }
                                        }
                                        #endregion

                                        #region // ----------------- DG AscensionBasic By:Kenshin ---------------- //
                                        if (monster.SubArea.Id == 71)
                                        {
                                            int floorMap = AscensionMonoEnum.getAscensionFloorMapBasic(character.GetAscensionBasicStair())[0];

                                            if (floorMap == character.Map.Id)
                                            {
                                                ////Desativado para melhoria e adaptação de recompensas
                                                //foreach (var item in AscensionMonoEnum.getAscensionFloorLootsBasic(character.GetAscensionBasicStair()))
                                                //{
                                                //    looter.Loot.AddItem(new DroppedItem(item[0], (uint)item[1])); //Se sim, Irá dropar os itens.
                                                //}

                                                if (character.GetAscensionBasicStair() < 99)
                                                {
                                                    string _webHookString = null;
                                                    #region MSG - Você completou o andar ##
                                                    switch (character.Account.Lang)
                                                    {
                                                        case "fr":
                                                            character.SendServerMessage("Tu as terminé le sol" + (character.AscensionBasicStair + 1));
                                                            break;
                                                        case "es":
                                                            character.SendServerMessage("Completaste el piso " + (character.AscensionBasicStair + 1));
                                                            break;
                                                        case "en":
                                                            character.SendServerMessage("You completed the floor " + (character.AscensionBasicStair + 1));
                                                            break;
                                                        default:
                                                            character.SendServerMessage("Você completou o andar " + (character.AscensionBasicStair + 1));
                                                            break;
                                                    }
                                                    #endregion

                                                    character.AddAscensionBasicStair(1);

                                                    #region MSG - Completou todos os andares
                                                    if (character.GetAscensionBasicStair() == 99)
                                                    {
                                                        switch (character.Account.Lang)
                                                        {
                                                            case "fr":
                                                                character.SendServerMessage("Vous avez terminé tous les étages de la Tour de l'Ascension.");
                                                                World.Instance.SendAnnounce("<b>" + character.Name + "</b> Vous avez terminé tous les étages de la Ascension Tower.", Color.Yellow);
                                                                _webHookString = "**" + character.Name + "** Vous avez terminé tous les étages de la Ascension Tower.";
                                                                break;
                                                            case "es":
                                                                character.SendServerMessage("Has completado todos los pisos de la Torre de la Ascensión.");
                                                                World.Instance.SendAnnounce("<b>" + character.Name + "</b> Has completado todos los pisos de la Ascension Tower.", Color.Yellow);
                                                                _webHookString = "**" + character.Name + "** Has completado todos los pisos de la Ascension Tower.";
                                                                break;
                                                            case "en":
                                                                character.SendServerMessage("You have completed all floors of the Ascension Tower.");
                                                                World.Instance.SendAnnounce("<b>" + character.Name + "</b> Completed all floors of the Ascension Tower.", Color.Yellow);
                                                                _webHookString = "**" + character.Name + "** Completed all floors of the Ascension Tower.";
                                                                break;
                                                            default:
                                                                character.SendServerMessage("Você completou todos os andares da torre da ascensão.");
                                                                World.Instance.SendAnnounce("<b>" + character.Name + "</b> Completou todos os andares da Torre da Ascensão.", Color.Yellow);
                                                                _webHookString = "**" + character.Name + "** Completou todos os andares da Torre da Ascensão.";
                                                                break;
                                                        }

                                                        if (DiscordIntegration.EnableDiscordWebHook)
                                                            PlainText.SendWebHook(DiscordIntegration.DiscordChatGlobalUrl, _webHookString, DiscordIntegration.DiscordWHUsername);
                                                    }
                                                    #endregion
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }

                                #region // ----------------- Sistema de Drop Dungeon Kolosso By:Kenshin ---------------- //
                                if (Map.IsDungeon() && character.SubArea.Id == 624)
                                {
                                    if (character.Map.Id == 61998084)
                                    {
                                        if (character.Inventory.Count(item => item.Template.Id == 11960) == 0)
                                            looter.Loot.AddItem(new DroppedItem(11960, 1));
                                    }
                                    else if (character.Map.Id == 61998082)
                                    {
                                        if (character.Inventory.Count(item => item.Template.Id == 11961) == 0)
                                            looter.Loot.AddItem(new DroppedItem(11961, 1));
                                    }
                                    else if (character.Map.Id == 61998338)
                                    {
                                        if (character.Inventory.Count(item => item.Template.Id == 11963) == 0)
                                            looter.Loot.AddItem(new DroppedItem(11963, 1));
                                    }
                                    else if (character.Map.Id == 61998340)
                                    {
                                        if (character.Inventory.Count(item => item.Template.Id == 11964) == 0)
                                            looter.Loot.AddItem(new DroppedItem(11964, 1));
                                    }
                                }
                                #endregion
                            }

                            #region // ----------------- Drop Orbes Desatiado By:Kenshin (DESATIVADO) ---------------- //
                            //Não utilizo mais os drops de Orbes porem o código pode ser usado no futuro
                            //int sum = 0;
                            //foreach (var monster in this.DefendersTeam.GetAllFighters())
                            //{
                            //    if (monster is MonsterFighter)
                            //    {
                            //        if ((monster as MonsterFighter).Monster.Template.Id != 494)
                            //        {
                            //            sum += monster.Level;
                            //        }
                            //    }
                            //}
                            //int maxLootNumber = (int)((sum / (double)this.DefendersTeam.GetAllFighters().Count()));
                            //looter.Loot.AddItem(new DroppedItem(8374, ((uint)new Random().Next((int)(maxLootNumber / 4.0 < 1 ? 1 : maxLootNumber / 4.0), (int)(maxLootNumber < 1 ? 1 : maxLootNumber))) * (uint)Rates.OrbsRate));
                            #endregion

                            #region // ----------------- Points Boutique Desativado By:Kenshin (DESATIVADO) ---------------- //

                            //Não utilizo o drop de Ogrines dentro do servidor porem o código pode ser no futuro

                            //if (looter is FightPlayerResult && this.DefendersTeam.GetAllFighters().Where(x => x is MonsterFighter).ToList().Exists(x => (x as MonsterFighter).Monster.Template.IsBoss))
                            //{
                            //    Character character = (looter as FightPlayerResult).Character;
                            //    BasePlayerItem BPsearcher = character.Inventory.TryGetItem(CharacterInventoryPositionEnum.ACCESSORY_POSITION_PETS);
                            //    if (BPsearcher != null && BPsearcher.Template.Id == 30350)
                            //    {
                            //        MonsterFighter boss = this.DefendersTeam.GetAllFighters().Where(x => x is MonsterFighter).ToList().FirstOrDefault(x => (x as MonsterFighter).Monster.Template.IsBoss) as MonsterFighter;
                            //        if (boss != null && boss.Monster.Template.Id == 1194 || boss.Monster.Template.Id == 872)
                            //        {
                            //            uint bp = ((uint)new CryptoRandom().Next((int)1, (int)Math.Ceiling(boss.Level / 15 * 1.09)));
                            //            looter.Loot.AddItem(new DroppedItem(Settings.TokenTemplateId, bp));
                            //            IPCAccessor.Instance.Send(new UpdateTokensMessage(character.Client.Account.Tokens + (int)bp, character.Client.Account.Id));
                            //        }
                            //    }
                            //}
                            #endregion                        
                        }

                        if (looter is IExperienceResult)
                        {
                            var winXP = FightFormulas.CalculateWinExp(looter, team.GetAllFighters<CharacterFighter>(), droppers);
                            var total = team == Winners ? winXP : (double)Math.Round(winXP * 0.10);

                            (looter as IExperienceResult).AddEarnedExperience(total);
                        }
                    }
                }

                //if (Winners == null || Draw)
                //{
                //    return results;
                //}
                //else if (DefendersTeam.Fighters.Any(x => x is MonsterFighter && (x as MonsterFighter).Monster.Nani))
                //{
                //    var NaniMonster = Map.NaniMonster;

                //    if (NaniMonster == null)
                //        return results;

                //    MonsterNaniManager.Instance.ResetSpawn(Map.NaniMonster);
                //    Map.NaniMonster = null;

                //    var characters = Winners.Fighters.OfType<CharacterFighter>();

                //    if (characters.Count() < 1)
                //        return results;

                //    if (Winners.TeamType == TeamTypeEnum.TEAM_TYPE_PLAYER)
                //    {
                //        World.Instance.SendAnnounceLang(
                //            "<b>" + string.Join(",", characters.Select(x => x.Name)) + "</b> ganhou : <b>" + NaniMonster.Template.Name + "</b>.",
                //            "<b>" + string.Join(",", characters.Select(x => x.Name)) + "</b> has won : <b>" + NaniMonster.Template.Name + "</b>.",
                //            "<b>" + string.Join(",", characters.Select(x => x.Name)) + "</b> ha ganado : <b>" + NaniMonster.Template.Name + "</b>.",
                //            "<b>" + string.Join(",", characters.Select(x => x.Name)) + "</b> a gagné : <b>" + NaniMonster.Template.Name + "</b>.",
                //            Color.OrangeRed
                //            );
                //    }
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return results;
        }

        protected override void SendGameFightJoinMessage(CharacterFighter fighter)
        {
            ContextHandler.SendGameFightJoinMessage(fighter.Character.Client, true, true, IsStarted, IsStarted ? 0 : (int)GetPlacementTimeLeft().TotalMilliseconds / 100, FightType);
        }

        protected override bool CanCancelFight() => false;

        public override TimeSpan GetPlacementTimeLeft()
        {
            var timeleft = FightConfiguration.PlacementPhaseTime - (DateTime.Now - CreationTime).TotalMilliseconds;

            if (timeleft < 0)
                timeleft = 0;

            return TimeSpan.FromMilliseconds(timeleft);
        }

        protected override void OnDisposed()
        {
            if (m_placementTimer != null)
                m_placementTimer.Dispose();

            base.OnDisposed();
        }
    }
}