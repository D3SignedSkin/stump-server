//using Stump.Core.Collections;
//using Stump.Core.Extensions;
//using Stump.Core.Reflection;
//using Stump.DofusProtocol.Enums;
//using Stump.Server.BaseServer.Database;
//using Stump.Server.WorldServer.Database.Arena;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
//using Stump.Server.WorldServer.Game.Arena;
//using Stump.Server.WorldServer.Game.Effects.Instances;
//using Stump.Server.WorldServer.Game.Fights;
//using Stump.Server.WorldServer.Game.Items;
//using Stump.Server.WorldServer.Game.Maps;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Threading;

//namespace Stump.Server.WorldServer.Database.Npcs.Replies
//{
//    [Discriminator("PvPSeekkk", typeof(NpcReply), typeof(NpcReplyRecord))]
//    public class PvPSeekReply : NpcReply
//    {
//        private TimedStack<Pair<int, int>> m_pvpSeekHistory = new TimedStack<Pair<int, int>>(60 * 5);
//        private Timer BattlefieldTimer;
//        private int retryBattleField;

//        public PvPSeekReply(NpcReplyRecord record)
//            : base(record)
//        {
//        }

//        public void startBattleField(Character first, Character second)
//        {
            
//            switch (first.Account.Lang)
//            {
//                case "fr":
//                    first.SendServerMessage("Un adversaire a été trouvé: <b>" + second.Name + "</b> de rang <b>" + second.GetCharacterRankName() + " (" + second.CharacterRankExp + " CP)" + "</b>, commencer le combat...", Color.Chartreuse);
//                    break;
//                case "es":
//                    first.SendServerMessage("Un oponente ha sido encontrado: <b>" + second.Name + "</b> de rango <b>" + second.GetCharacterRankName() + " (" + second.CharacterRankExp + " CP)" + "</b>, comenzando el combate...", Color.Chartreuse);
//                    break;
//                case "en":
//                    first.SendServerMessage("An opponent has been found: <b>" + second.Name + "</b> of rank <b>" + second.GetCharacterRankName() + " (" + second.CharacterRankExp + " CP)" + "</b>, beginning the combat...", Color.Chartreuse);
//                    break;
//                default:
//                    first.SendServerMessage("Um oponente foi encontrado: <b>" + second.Name + "</b> de rank <b>" + second.GetCharacterRankName() + " (" + second.CharacterRankExp + " CP)" + "</b>, começando o combate...", Color.Chartreuse);
//                    break;
//            }

            
//            switch (second.Account.Lang)
//            {
//                case "fr":
//                    second.SendServerMessage("Un adversaire vous défie sur le champ de bataille! <b>" + first.Name + "</b> votre rang <b>" + first.GetCharacterRankName() + " (" + first.CharacterRankExp + " CP)" + "</b>, commencer le combat...", Color.Chartreuse);
//                    break;
//                case "es":
//                    second.SendServerMessage("Un oponente te desafía en el campo de batalla ! <b>" + first.Name + "</b> su rango <b>" + first.GetCharacterRankName() + " (" + first.CharacterRankExp + " CP)" + "</b>, comenzando el combate...", Color.Chartreuse);
//                    break;
//                case "en":
//                    second.SendServerMessage("An opponent challenges you on the battlefield! <b>" + first.Name + "</b> your rank <b>" + first.GetCharacterRankName() + " (" + first.CharacterRankExp + " CP)" + "</b>, beginning the combat...", Color.Chartreuse);
//                    break;
//                default:
//                    second.SendServerMessage("Um oponente desafia você no campo de batalha! <b>" + first.Name + "</b> seu rank <b>" + first.GetCharacterRankName() + " (" + first.CharacterRankExp + " CP)" + "</b>, começando o combate...", Color.Chartreuse);
//                    break;
//            }
//            first.updateBattleFieldPosition();
//            second.updateBattleFieldPosition();

//            /* Save map/cell before fight was started */

//            var random = ArenaManager.Instance.Arenas_1vs1.RandomElementOrDefault();
//            Map m_pvpmap = null;
//            if (random.Value != null && random.Value.Map != null)
//                m_pvpmap = random.Value.Map;
//            if (m_pvpmap != null)
//            {
//                var preFight = FightManager.Instance.CreateAgressionFight(m_pvpmap, second.AlignmentSide, first.AlignmentSide, true);
//                try
//                {
//                    second.Area.AddMessage(() =>
//                    {
//                        second.LeaveDialog();
//                        lock (preFight.m_playersMaps)
//                            preFight.m_playersMaps.Add(second, second.Map);
//                        second.Teleport(m_pvpmap, second.Cell);

//                        m_pvpmap.Area.ExecuteInContext(() =>
//                        {
//                            preFight.ChallengersTeam.AddFighter(second.CreateFighter(preFight.ChallengersTeam));
//                        });
//                    });

//                    first.Area.AddMessage(() =>
//                    {
//                        first.LeaveDialog();
//                        lock (preFight.m_playersMaps)
//                            preFight.m_playersMaps.Add(first, first.Map);
//                        first.Teleport(m_pvpmap, first.Cell);

//                        m_pvpmap.Area.ExecuteInContext(() =>
//                        {
//                            preFight.DefendersTeam.AddFighter(first.CreateFighter(preFight.DefendersTeam));
//                            preFight.StartPlacement();
//                        });
//                    });
//                }
//#pragma warning disable CS0168 // La variable 'ex' se ha declarado pero nunca se usa
//                catch (Exception ex)
//#pragma warning restore CS0168 // La variable 'ex' se ha declarado pero nunca se usa
//                {
//                    preFight.EndFight();
//                    second.Teleport(Stump.Server.WorldServer.Game.World.Instance.GetMap(100270593), second.Cell);
//                    first.Teleport(Stump.Server.WorldServer.Game.World.Instance.GetMap(100270593), first.Cell);
//                }
//            }
//            else
//            {
                
//                switch (first.Account.Lang)
//                {
//                    case "fr":
//                        first.SendServerMessage("La bataille sur le champ de bataille a été annulée.", Color.DarkOrange);
//                        break;
//                    case "es":
//                        first.SendServerMessage("La pelea en el campo de batalla ha sido cancelada.", Color.DarkOrange);
//                        break;
//                    case "en":
//                        first.SendServerMessage("The fight on the battlefield has been canceled.", Color.DarkOrange);
//                        break;
//                    default:
//                        first.SendServerMessage("A luta no campo de batalha foi cancelada.", Color.DarkOrange);
//                        break;
//                }
//                switch (second.Account.Lang)
//                {
//                    case "fr":
//                        second.SendServerMessage("La bataille sur le champ de bataille a été annulée.", Color.DarkOrange);
//                        break;
//                    case "es":
//                        second.SendServerMessage("La pelea en el campo de batalla ha sido cancelada.", Color.DarkOrange);
//                        break;
//                    case "en":
//                        second.SendServerMessage("The fight on the battlefield has been canceled.", Color.DarkOrange);
//                        break;
//                    default:
//                        second.SendServerMessage("A luta no campo de batalha foi cancelada.", Color.DarkOrange);
//                        break;
//                }
               
//            }
//        }

//        private static void OnQueueBattleField(Character character, PvPSeekReply obj)
//        {
//            if (character.IsInFight())
//                return;
//            Character target = null;

//            target = Game.World.Instance.GetCharacters(x => character.CanBattlefield(x) == true).RandomElementOrDefault();
//            if (target != null && target.Account.Nickname == character.CharacterToSeekName && !character.IsGameMaster())
//                target = null;
//            obj.retryBattleField++;
           

//            switch (character.Account.Lang)
//            {
//                case "fr":
//                    character.SendServerMessage("Recherche d'adversaire en cours... (" + obj.retryBattleField + " tentatives)", Color.Aqua);
//                    break;
//                case "es":
//                    character.SendServerMessage("Busqueda de oponente en curso... (" + obj.retryBattleField + " intentos)", Color.Aqua);
//                    break;
//                case "en":
//                    character.SendServerMessage("Search of opponent in progress... (" + obj.retryBattleField + " attempts)", Color.Aqua);
//                    break;
//                default:
//                    character.SendServerMessage("Busca do oponente em progresso... (" + obj.retryBattleField + " tentativas)", Color.Aqua);
//                    break;
//            }
//            if (target != null && !character.IsBusy())
//            {
//                character.CharacterToSeekName = target.Account.Nickname;
//                obj.startBattleField(character, target);
//                return;
//            }
//            else if (target != null)
//            {
               
//                switch (character.Account.Lang)
//                {
//                    case "fr":
//                        character.SendServerMessage("Un adversaire a été trouvé sur le champ de bataille, mais vous étiez occupé, le combat a été annulé.", Color.Red);
//                        break;
//                    case "es":
//                        character.SendServerMessage("Un oponente fue encontrado para el campo de batalla, pero estabas ocupado, la pelea fue cancelada.", Color.Red);
//                        break;
//                    case "en":
//                        character.SendServerMessage("An opponent was found for the battlefield, but you were busy, the fight was canceled.", Color.Red);
//                        break;
//                    default:
//                        character.SendServerMessage("Um adversário foi encontrado para o campo de batalha, mas você estava ocupado, a luta foi cancelada.", Color.Red);
//                        break;
//                }
//                return;
//            }
//            if (obj.retryBattleField == 6)
//            {
                
//                switch (character.Account.Lang)
//                {
//                    case "fr":
//                        character.SendServerMessage("Actuellement, personne n'est disponible sur le champ de bataille. Réessayez plus tard...", Color.DarkOrange);
//                        break;
//                    case "es":
//                        character.SendServerMessage("Actualmente, nadie está disponible en el campo de batalla. Inténtalo de nuevo más tarde...", Color.DarkOrange);
//                        break;
//                    case "en":
//                        character.SendServerMessage("Currently, nobody is available on the battlefield. Try again later...", Color.DarkOrange);
//                        break;
//                    default:
//                        character.SendServerMessage("Atualmente, ninguém está disponível no campo de batalha. Tente novamente mais tarde...", Color.DarkOrange);
//                        break;
//                }
//                return;
//            }
//            obj.BattlefieldTimer = new Timer(_ => OnQueueBattleField(character, obj), null, 1000 * 10, Timeout.Infinite);
//        }

//        public override bool Execute(Npc npc, Character character)
//        {
//            if (!base.Execute(npc, character))
//                return false;

//            m_pvpSeekHistory.Clean();
//            if (m_pvpSeekHistory.Any(x => x.First.First == character.Id))
//            {
                
//                switch (character.Account.Lang)
//                {
//                    case "fr":
//                        character.SendServerMessage("Vous devez attendre 5 minutes entre chaque combat.", Color.DarkOrange);
//                        break;
//                    case "es":
//                        character.SendServerMessage("Tienes que esperar 5 minutos entre cada combate.", Color.DarkOrange);
//                        break;
//                    case "en":
//                        character.SendServerMessage("You have to wait 5 minutes between each fight.", Color.DarkOrange);
//                        break;
//                    default:
//                        character.SendServerMessage("Você tem que esperar 5 minutos entre cada luta.", Color.DarkOrange);
//                        break;
//                }
//                return false;
//            }

//            if (character.AgressionPenality >= DateTime.Now)
//            {
               
//                switch (character.Account.Lang)
//                {
//                    case "fr":
//                        character.SendServerMessage("Vous devez attendre que vous ne soyez plus interdit de chercher un combat sur le champ de bataille.", Color.DarkOrange);
//                        break;
//                    case "es":
//                        character.SendServerMessage("Debes esperar hasta que ya no estés prohibido para buscar una pelea en el campo de batalla.", Color.DarkOrange);
//                        break;
//                    case "en":
//                        character.SendServerMessage("You must wait until you are no longer forbidden to search for a fight on the battlefield.", Color.DarkOrange);
//                        break;
//                    default:
//                        character.SendServerMessage("Você deve esperar até que não seja mais proibido procurar uma luta no campo de batalha.", Color.DarkOrange);
//                        break;
//                }
//                return false;
//            }
            
//                if (!character.battleFieldOn)
//                character.battleFieldOn = true;
//            this.retryBattleField = 0;
          

//            switch (character.Account.Lang)
//            {
//                case "fr":
//                    character.SendServerMessage("Recherche de l'adversaire... (60 secondes)", Color.Aqua);
//                    break;
//                case "es":
//                    character.SendServerMessage("Busqueda de oponente... (60 segundos)", Color.Aqua);
//                    break;
//                case "en":
//                    character.SendServerMessage("Search of opponent... (60 secondes)", Color.Aqua);
//                    break;
//                default:
//                    character.SendServerMessage("Busca do oponente... (60 segundos)", Color.Aqua);
//                    break;
//            }
//            this.BattlefieldTimer = new Timer(_ => OnQueueBattleField(character, this), null, 1000 * 10, Timeout.Infinite);

//            /*
//             * OLDER SYSTEM WITH CONTRACT
//             * 
//             */

//            /*foreach (var contract in character.Inventory.GetItems(x => x.Template.Id == (int)ItemIdEnum.ORDRE_DEXECUTION_10085))
//                character.Inventory.RemoveItem(contract);
//            var item = ItemManager.Instance.CreatePlayerItem(character, (int)ItemIdEnum.ORDRE_DEXECUTION_10085, 25);
//            var seekEffect = item.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_Seek);
//            if (seekEffect != null)
//                item.Effects.Remove(seekEffect);
//            item.Effects.Add(new EffectString(EffectsEnum.Effect_Seek, target.Name));
//            item.Effects.Add(new EffectInteger(EffectsEnum.Effect_Alignment, (short)target.AlignmentSide));
//            item.Effects.Add(new EffectInteger(EffectsEnum.Effect_Grade, target.AlignmentGrade));
//            item.Effects.Add(new EffectInteger(EffectsEnum.Effect_Level, target.Level));
//            item.Effects.Add(new EffectInteger(EffectsEnum.Effect_NonExchangeable_981, 0));
//            character.Inventory.AddItem(item);
//            character.CharacterToSeekName = target.Account.Nickname;
//            m_pvpSeekHistory.Push(new Pair<int, int>(character.Id, target.Id));
//            character.SendServerMessage($"Você encontrou o jogador {target.Name} como alvo, e recebeu 25 pergaminhos de busca.", Color.DarkOrange);
//            */

//            return true;
//        }
//    }
//}