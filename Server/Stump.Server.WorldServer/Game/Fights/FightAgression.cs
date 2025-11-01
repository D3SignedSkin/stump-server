using Database.Seeklog;
using MongoDB.Bson;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Seeklog;
using Stump.Server.WorldServer.Handlers.Context;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Fights
{
    public class FightAgression : Fight<FightPlayerTeam, FightPlayerTeam>
    {
        public bool battleField;

        public FightAgression(int id, Map fightMap, FightPlayerTeam defendersTeam, FightPlayerTeam challengersTeam, bool isBattlefield = false) : base(id, fightMap, defendersTeam, challengersTeam)
        {
            m_placementTimer = Map.Area.CallDelayed(FightConfiguration.PlacementPhaseTime, StartFighting);
            this.battleField = isBattlefield;
        }

        public override void StartPlacement()
        {
            base.StartPlacement();
            m_placementTimer = Map.Area.CallDelayed(FightConfiguration.PlacementPhaseTime, StartFighting);
        }

        public override void StartFighting()
        {
            m_placementTimer.Dispose();

            base.StartFighting();
        }

        public override FightTypeEnum FightType
        {
            get { return FightTypeEnum.FIGHT_TYPE_AGRESSION; }
        }

        public override bool IsPvP
        {
            get { return true; }
        }

        public override bool IsMultiAccountRestricted
        {
            get { return true; }
        }

        protected override void ApplyResults()
        {
            foreach (var fightResult in Results)
            {
                fightResult.Apply();
            }
        }

        private int[,] earnedEXPMatrix = new int[,]{
            { 35000000, 5500000, 9500000 },
            { 9500000, 13000000, 16500000 },
            { 16500000, 20000000, 29000000 },
        };

        private int[,] earnedKamasMatrix = new int[,]{
           { 200000, 300000, 500000 },
           { 300000, 500000, 700000 },
           { 500000, 600000, 900000 },
           { 600000, 800000, 110000 },
        };

        private int[,] earnedPvMatrix = new int[,]{
           { 3, 3, 3 },
           { 3, 3, 3 },
           { 3, 3, 3 },
           { 3, 4, 4 },
        };

        private int[,] earnedKolMatrix = new int[,]{
           { 1, 1, 1 },
           { 1, 1, 1 },
           { 1, 1, 1 },
           { 1, 2, 2 },
        };

        protected override void SendGameFightJoinMessage(CharacterFighter fighter)
        {
            ContextHandler.SendGameFightJoinMessage(fighter.Character.Client, CanCancelFight(), true, IsStarted, (int)GetPlacementTimeLeft().TotalMilliseconds / 100, FightType);
        }

        public override TimeSpan GetPlacementTimeLeft()
        {
            var timeleft = FightConfiguration.PlacementPhaseTime - (DateTime.Now - CreationTime).TotalMilliseconds;

            if (timeleft < 0)
                timeleft = 0;

            return TimeSpan.FromMilliseconds(timeleft);
        }

        protected override bool CanCancelFight() => false;

        public Dictionary<Character, Map> m_playersMaps = new Dictionary<Character, Map>();

        /* - - - Variáveis - - - */
        private List<Character> _allWinners = new List<Character>();
        private List<CharacterFighter> _noHonor = new List<CharacterFighter>();
        private List<Character> _allLosers = new List<Character>();
        private int TotalNivelVencedores, TotalNivAsaVencedores, QtdVencedores;
        private int TotalNivelPerdedores, TotalNivAsaPerdedores, QtdPerdedores;
        List<string> _ipsWinners = new List<string>();
        List<string> _ipLosers = new List<string>();

        protected override List<IFightResult> GetResults()
        {
            var results = GetFightersAndLeavers().Where(entry => entry.HasResult).Select(fighter => fighter.GetFightResult()).ToList();
            var resultLeavers = GethDcFighters()?.Where(entry => entry.HasResult).Select(fighter => fighter.GetFightResult()).ToList();

            GetInformations();

            if (GetFightDuration().TotalMinutes < 2)
                SendFightInformationsOnFinish(false);
            else
                SendFightInformationsOnFinish(true);

            if (resultLeavers != null && resultLeavers.Count() >= 1)
                results.Add(resultLeavers.FirstOrDefault());

            foreach (var playerResult in results.OfType<FightPlayerResult>())
            {
                short m_honor = CalculateEarnedHonor(playerResult.Fighter);
                double RatePercent = playerResult.Character.Client.UserGroup.Role >= RoleEnum.Gold_Vip ? Rates.GoldHonrorRate : playerResult.Character.Client.UserGroup.Role == RoleEnum.Vip ? Rates.VipHonrorRate : Rates.HonrorRate;

                if (m_honor > 0 && m_honor < 120)
                    m_honor = 120;

                if (GetFightDuration().TotalMinutes < 2)
                {
                    if (m_honor > 0)
                    {
                        if (playerResult.Vip)
                            m_honor = (short)(m_honor * (RatePercent / 100));

                        if (m_honor > 228 / 2)
                            m_honor = 228 / 2;

                        playerResult.SetEarnedHonor((short)((m_honor * 1.75) / 2), CalculateEarnedDishonor(playerResult.Fighter));
                    }
                    else
                    {
                        if (m_honor < -228)
                            m_honor = -228;

                        playerResult.SetEarnedHonor((short)(m_honor * 2.20), CalculateEarnedDishonor(playerResult.Fighter));
                    }
                }
                else
                {
                    if (m_honor > 0)
                    {
                        if (playerResult.Vip)
                            m_honor = (short)(m_honor * (RatePercent / 100));

                        if (m_honor > 228)
                            m_honor = 228;

                        playerResult.SetEarnedHonor((short)(m_honor * 1.75), CalculateEarnedDishonor(playerResult.Fighter));
                    }
                    else
                    {
                        if (m_honor < -228)
                            m_honor = -228;

                        playerResult.SetEarnedHonor((short)(m_honor * 2.20), CalculateEarnedDishonor(playerResult.Fighter));
                    }
                }

                if (this.battleField)
                {
                    CalculateEarnedRank(playerResult);
                }
                else
                {
                    if (playerResult.Fight.FightType == FightTypeEnum.FIGHT_TYPE_AGRESSION)
                    {
                        CalculateEarned(playerResult);
                    }
                }

                #region MongoDB
                var document = new BsonDocument
                    {
                        { "FightId", UniqueId.ToString() },
                        { "Duration", GetFightDuration().TotalSeconds },
                        { "Team", Enum.GetName(typeof(TeamEnum), playerResult.Fighter.Team.Id) },
                        { "isWinner", Winners.Id == playerResult.Fighter.Team.Id},
                        { "AccountId", playerResult.Character.Account.Id },
                        { "AccountName", playerResult.Character.Account.Login },
                        { "CharacterId", playerResult.Character.Id },
                        { "CharacterName", playerResult.Character.Name},
                        { "IPAddress", playerResult.Character.Client.IP },
                        { "ClientKey", playerResult.Character.Account.LastHardwareId },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Fight_Agress", document);
                #endregion
            }
            return results;
        }

        #region Receber Informações
        public void GetInformations()
        {
            var m_losers = this.GetLosersAndLeaversWithDc().OfType<CharacterFighter>().ToList();
            var m_winners = Winners.GetAllFightersWithLeavers().OfType<CharacterFighter>().ToList();

            if (m_losers != null && m_losers.Count() > 0)
            {
                //Armazenando informações sobre os perdedores
                foreach (var _perso in m_losers)
                {
                    if (_perso.Character.AlignmentSide == AlignmentSideEnum.ALIGNMENT_NEUTRAL)
                        _noHonor.Add(_perso);

                    try
                    {
                        _allLosers.Add(_perso.Character);
                        _ipLosers.Add(_perso.Character.Client.IP);
                    }
                    catch (Exception e) { }

                    TotalNivelPerdedores += _perso.Level;
                    TotalNivAsaPerdedores += _perso.Character.AlignmentGrade;
                    QtdPerdedores++;
                }
            }

            if (m_winners != null && m_winners.Count() > 0)
            {
                //Armazenando informações sobre os vencedores
                foreach (var _perso in m_winners)
                {
                    var lastfights_hdr = SeekLog_manager.Instance.GetHardwareRecord(_perso.Character.Account.LastHardwareId);
                    var lastfights_ip = SeekLog_manager.Instance.GetIpRecords(_perso.Character.Client.IP);
                    var lastfights = lastfights_hdr.Concat(lastfights_ip.Where(x => !lastfights_hdr.Contains(x))).ToList();

                    if (_perso.Character.AlignmentSide == AlignmentSideEnum.ALIGNMENT_NEUTRAL || _ipLosers.Contains(_perso.Character.Client.IP))
                        _noHonor.Add(_perso);

                    if (m_losers != null && m_losers.Count() > 0)
                    {
                        foreach (var loser in m_losers)
                        {
                            if (lastfights.Any(x => x.Ip_opponent == loser.Character.Client.IP))
                            {
                                _noHonor.Add(_perso);
                                break;
                            }
                            else if (lastfights.Any(x => x.Hardware_opponent == loser.Character.Account.LastHardwareId))
                            {
                                _noHonor.Add(_perso);
                                break;
                            }
                        }
                    }

                    try
                    {
                        _ipsWinners.Add((_perso as CharacterFighter).Character.Client.IP);
                        _allWinners.Add((_perso as CharacterFighter).Character);
                    }
                    catch (Exception e) { }

                    TotalNivelVencedores += _perso.Level;
                    TotalNivAsaVencedores += (_perso as CharacterFighter).Character.AlignmentGrade;
                    QtdVencedores++;
                }
            }

            if (m_winners.Count() > 0 && m_losers.Count() > 0)
                RecordLog();
        }
        #endregion

        #region Record Log PVP
        private void RecordLog()
        {
            var m_winners = Winners.GetAllFightersWithLeavers().OfType<CharacterFighter>().ToList();
            var m_losers = this.GetLosersAndLeaversWithDc().OfType<CharacterFighter>().ToList();

            try
            {
                if (m_winners.Count() > 0 && m_losers.Count() > 0)
                {
                    foreach (var winner in m_winners.Where(x => x != null))
                    {
                        foreach (var loser in m_losers.Where(y => y != null))
                        {
                            int winner_honor = CalculateEarnedHonor(winner);
                            int loser_honor = CalculateEarnedHonor(loser);

                            SeekLog_manager.Instance.AddRecord(new seek_logRecord
                            {
                                Time = DateTime.Now,
                                IsNew = true,
                                Name_own = winner.Character.NameClean,
                                Honor_own = winner_honor,
                                Ip_own = winner.Character.Client.IP,
                                Hardware_own = winner.Character.Account.LastHardwareId,
                                Name_opponent = loser.Character.NameClean,
                                Honor_opponent = loser_honor,
                                Ip_opponent = loser.Character.Client.IP,
                                Hardware_opponent = loser.Character.Account.LastHardwareId,
                            });

                            SeekLog_manager.Instance.Save();
                        }
                    }
                }
            }
            catch (Exception e)
            { }
        }
        #endregion

        #region Enviar informações término da batalha
        private void SendFightInformationsOnFinish(bool real)
        {
            var m_winnerscount = Winners.GetAllFighters().OfType<CharacterFighter>().Count();
            var m_loserscount = Losers.GetAllFighters().OfType<CharacterFighter>().Count();
            var m_leaverscount = Leavers.Count;
            bool MSG_Active = false;

            if (!real)
            {
                #region Devido à duração da luta muito curta, algumas estatísticas mudaram.
                foreach (var fighter in this.GetAllCharacters())
                {
                    switch (fighter.Account.Lang)
                    {
                        case "fr":
                            fighter.SendServerMessage("En raison de la durée du combat trop courte, certaines statistiques ont changé.", System.Drawing.Color.DarkOrange);
                            break;
                        case "es":
                            fighter.SendServerMessage("Debido a la duración de la pelea demasiado corta, algunas estadísticas han cambiado.", System.Drawing.Color.DarkOrange);
                            break;
                        case "en":
                            fighter.SendServerMessage("Due to the duration of the fight too short, some statistics have changed.", System.Drawing.Color.DarkOrange);
                            break;
                        default:
                            fighter.SendServerMessage("Devido à duração da luta muito curta, algumas estatísticas mudaram.", System.Drawing.Color.DarkOrange);
                            break;
                    }
                }
                #endregion
            }

            if (MSG_Active == true)
            {
                if (m_winnerscount == 1 && m_loserscount == 1)
                {
                    #region A batalha durou ### minutos ....
                    if (battleField)
                    {
                        World.Instance.SendAnnounceLang($"O jogador <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> derrotou <b>{(Losers.Fighters[0] as CharacterFighter).Character.Name}</b> no campo de batalha." +
                            $" A batalha durou <b>{this.GetFightDuration().Minutes}</b> minuto(s) e <b>{this.GetFightDuration().Seconds}</b> segundo(s).", $"The player <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> defeated <b>{(Losers.Fighters[0] as CharacterFighter).Character.Name}</b> on the battlefield." +
                            $" The battle lasted <b>{this.GetFightDuration().Minutes}</b> minute(s) and <b>{this.GetFightDuration().Seconds}</b> second(s).", $"El jugador <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> derrotado <b>{(Losers.Fighters[0] as CharacterFighter).Character.Name}</b> en el campo de batalla." +
                            $" La batalla duró <b>{this.GetFightDuration().Minutes}</b> minuto(s) y <b>{this.GetFightDuration().Seconds}</b> segundo(s).", $"Le joueur <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> a battu <b>{(Losers.Fighters[0] as CharacterFighter).Character.Name}</b> sur le champ de bataille." +
                            $" La bataille a duré <b>{this.GetFightDuration().Minutes}</b> minute(s) et <b>{this.GetFightDuration().Seconds}</b> seconde(s).", Color.YellowGreen);
                    }
                    else
                    {
                        World.Instance.SendAnnounceLang($"O jogador <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> derrotou <b>{(Losers.Fighters[0] as CharacterFighter).Character.Name}</b> em combate PvP." +
                            $" A batalha durou <b>{this.GetFightDuration().Minutes}</b> minuto(s) e <b>{this.GetFightDuration().Seconds}</b> segundo(s).", $"The player <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> defeated <b>{(Losers.Fighters[0] as CharacterFighter).Character.Name}</b> in PvP combat." +
                            $" The battle lasted <b>{this.GetFightDuration().Minutes}</b> minute(s) and <b>{this.GetFightDuration().Seconds}</b> second(s).", $"El jugador <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> derrotado <b>{(Losers.Fighters[0] as CharacterFighter).Character.Name}</b> en combate PvP." +
                            $" La batalla duró <b>{this.GetFightDuration().Minutes}</b> minuto(s) y <b>{this.GetFightDuration().Seconds}</b> segundo(s).", $"Le joueur <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> a battu <b>{(Losers.Fighters[0] as CharacterFighter).Character.Name}</b> en combat PvP." +
                            $" La bataille a duré <b>{this.GetFightDuration().Minutes}</b> minute(s) et <b>{this.GetFightDuration().Seconds}</b> seconde(s).", Color.YellowGreen);

                    }
                    #endregion
                }
                else if ((m_winnerscount == 1 && m_leaverscount == 1) || (m_loserscount == 1 && m_leaverscount == 1))
                {
                    #region A batalha durou ### minutos ...
                    if (battleField)
                    {
                        World.Instance.SendAnnounceLang($"O jogador <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> derrotou <b>{(Leavers[0] as CharacterFighter).Character.Name}</b> no campo de batalha." +
                            $" A batalha durou <b>{this.GetFightDuration().Minutes}</b> minuto(s) e <b>{this.GetFightDuration().Seconds}</b> segundo(s).", $"The player <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> defeated <b>{(Leavers[0] as CharacterFighter).Character.Name}</b> on the battlefield." +
                            $" The battle lasted <b>{this.GetFightDuration().Minutes}</b> minute(s) and <b>{this.GetFightDuration().Seconds}</b> second(s).", $"El jugador <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> derrotado <b>{(Leavers[0] as CharacterFighter).Character.Name}</b> en el campo de batalla." +
                            $" La batalla duró <b>{this.GetFightDuration().Minutes}</b> minuto(s) y <b>{this.GetFightDuration().Seconds}</b> segundo(s).", $"Le joueur <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> a battu <b>{(Leavers[0] as CharacterFighter).Character.Name}</b> sur le champ de bataille." +
                            $" La bataille a duré <b>{this.GetFightDuration().Minutes}</b> minute(s) et <b>{this.GetFightDuration().Seconds}</b> seconde(s).", Color.YellowGreen);
                    }
                    else
                    {
                        World.Instance.SendAnnounceLang($"O jogador <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> derrotou <b>{(Leavers[0] as CharacterFighter).Character.Name}</b> em combate PvP." +
                            $" A batalha durou <b>{this.GetFightDuration().Minutes}</b> minuto(s) e <b>{this.GetFightDuration().Seconds}</b> segundo(s).", $"The player <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> defeated <b>{(Leavers[0] as CharacterFighter).Character.Name}</b> in PvP combat." +
                            $" The battle lasted <b>{this.GetFightDuration().Minutes}</b> minute(s) and <b>{this.GetFightDuration().Seconds}</b> second(s).", $"El jugador <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> derrotado <b>{(Leavers[0] as CharacterFighter).Character.Name}</b> en combate PvP." +
                            $" La batalla duró <b>{this.GetFightDuration().Minutes}</b> minuto(s) y <b>{this.GetFightDuration().Seconds}</b> segundo(s).", $"Le joueur <b>{(Winners.Fighters[0] as CharacterFighter).Character.Name}</b> a battu <b>{(Leavers[0] as CharacterFighter).Character.Name}</b> en combat PvP." +
                            $" La bataille a duré <b>{this.GetFightDuration().Minutes}</b> minute(s) et <b>{this.GetFightDuration().Seconds}</b> seconde(s).", Color.YellowGreen);
                    }
                    #endregion
                }
            }
        }
        #endregion

        #region Calcular Ganhos
        private void CalculateEarned(FightPlayerResult result)
        {
            var pvpSeek = result.Character.Inventory.GetItems(x => x.Template.Id == (int)ItemIdEnum.ORDRE_DEXECUTION_10085).FirstOrDefault();
            double fightTime = this.GetFightDuration().TotalSeconds;

            if (Winners == result.Fighter.Team && pvpSeek == null)
            {
                //Calculando Ganho de XP
                CalculateCharacterEarnedExp(result, fightTime);
                //Calculando o Ganho de Kamas
                CalculateCharacterEarnedKamas(result, fightTime);

                //Voltando o Player Para o Mapa Antigo
                if (m_playersMaps.Keys.Contains(result.Character))
                    result.Character.NextMap = m_playersMaps[result.Character];
            }
            else if (pvpSeek != null)
            {
                if (pvpSeek.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_Seek) is EffectString seekEffect)
                {
                    var target = result.Fighter.OpposedTeam.GetAllFightersWithLeavers<CharacterFighter>().FirstOrDefault(x => x.Name == seekEffect.Text);

                    if (target != null)
                    {
                        result.Character.Inventory.RemoveItem(pvpSeek);

                        if (result.Character.Account.Email != target.Character.Account.Email && result.Character.Account.LastConnectionIp != target.Character.Account.LastConnectionIp && result.Character.Account.LastHardwareId != target.Character.Account.LastHardwareId)
                        {
                            if (Winners == result.Fighter.Team) /* Character Winner */
                            {
                                //Calculando Ganho de XP
                                CalculateCharacterEarnedExp(result, fightTime);
                                //Calculando o Ganho de Kamas
                                CalculateCharacterEarnedKamas(result, fightTime);
                                //Calculando o Ganho de Pevefichas
                                CharacterWinnerPevetons(result, fightTime);

                                //Voltando o Player Para o Mapa Antigo
                                if (m_playersMaps.Keys.Contains(result.Character))
                                    result.Character.NextMap = m_playersMaps[result.Character];
                            }
                            else
                            {
                                //Calculando Ganho de XP
                                CalculateTargetEarnedExp(target, fightTime);
                                //Calculando o Ganho de Kamas
                                CalculateTargetEarnedKamas(target, fightTime);
                                //Calculando o Ganho de Pevefichas
                                TargetWinnerPevetons(target, fightTime);

                                //Voltando o Player Para o Mapa Antigo
                                if (m_playersMaps.Keys.Contains(target.Character))
                                    target.Character.NextMap = m_playersMaps[target.Character];
                            }
                        }
                    }
                }
                else
                    result.Character.Inventory.RemoveItem(pvpSeek);
            }
        }
        #endregion

        #region Loot PVP
        #region Calcular Ganho de Pevefichas
        private bool CharacterWinnerPevetons(FightPlayerResult PlayerFight, double FightTime)//Character Winner By:Kenshin
        {
            bool result;
            int targeLevel = PlayerFight.Character.Level;

            if (Draw)
            {
                result = false;
            }
            else
            {
                if (PlayerFight.Character.AlignmentSide == AlignmentSideEnum.ALIGNMENT_NEUTRAL || _noHonor.Contains(PlayerFight.Fighter))
                {
                    result = false;
                }
                else if (QtdVencedores == 0 || QtdPerdedores == 0)
                {
                    result = false;
                }
                else
                {

                    #region Level PVP  <= 50
                    if (targeLevel <= 50)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[0, 0]);
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[0, 1]);
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[0, 2]);
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 51 ~ <= 100
                    else if (targeLevel >= 51 && targeLevel <= 100)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[1, 0]);
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[1, 1]);
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[1, 2]);
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 101 ~ <= 150
                    else if (targeLevel >= 101 && targeLevel <= 150)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[2, 0]);
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[2, 1]);
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[2, 2]);
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 151
                    else if (targeLevel >= 151)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[3, 0]);
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[3, 1]);
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[3, 2]);
                        }

                        result = true;
                    }
                    #endregion
                    else
                        result = false;
                }
            }

            return result;
        }

        private bool TargetWinnerPevetons(CharacterFighter PlayerFight, double FightTime) //Target Winner By:Kenshin
        {
            bool result;
            int targeLevel = PlayerFight.Character.Level;

            if (Draw)
            {
                result = false;
            }
            else
            {
                if (PlayerFight.OpposedTeam.AlignmentSide == AlignmentSideEnum.ALIGNMENT_NEUTRAL || _noHonor.Contains(PlayerFight))
                {
                    result = false;
                }
                else if (QtdVencedores == 0 || QtdPerdedores == 0)
                {
                    result = false;
                }
                else
                {
                    #region Level PVP  <= 50
                    if (targeLevel <= 50)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[0, 0]);
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[0, 1]);
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[0, 2]);
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 51 ~ <= 100
                    else if (targeLevel >= 51 && targeLevel <= 100)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[1, 0]);
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[1, 1]);
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[1, 2]);
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 101 ~ <= 150
                    else if (targeLevel >= 101 && targeLevel <= 150)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[2, 0]);
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[2, 1]);
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[2, 2]);
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 151
                    else if (targeLevel >= 151)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[3, 0]);
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[3, 1]);
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.AddItem((int)ItemIdEnum.PEVETON_10275, (uint)earnedPvMatrix[3, 2]);
                        }

                        result = true;
                    }
                    #endregion
                    else
                        result = false;
                }
            }

            return result;
        }
        #endregion

        #region Calcular Ganhos de Kamas
        private bool CalculateCharacterEarnedKamas(FightPlayerResult PlayerFight, double FightTime)//Character Winner By:Kenshin
        {
            bool result;
            int targeLevel = PlayerFight.Character.Level;

            if (Draw)
            {
                result = false;
            }
            else
            {
                if (PlayerFight.Character.AlignmentSide == AlignmentSideEnum.ALIGNMENT_NEUTRAL || _noHonor.Contains(PlayerFight.Fighter))
                {
                    result = false;
                }
                else if (QtdVencedores == 0 || QtdPerdedores == 0)
                {
                    result = false;
                }
                else
                {

                    #region Level PVP  <= 50
                    if (targeLevel <= 50)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[0, 0];
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[0, 1];
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[0, 2];
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 51 ~ <= 100
                    else if (targeLevel >= 51 && targeLevel <= 100)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[1, 0];
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[1, 1];
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[1, 2];
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 101 ~ <= 150
                    else if (targeLevel >= 101 && targeLevel <= 150)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[2, 0];
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[2, 1];
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[2, 2];
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 151
                    else if (targeLevel >= 151)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[3, 0];
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[3, 1];
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[3, 2];
                        }

                        result = true;
                    }
                    #endregion
                    else
                        result = false;
                }
            }

            return result;
        }

        private bool CalculateTargetEarnedKamas(CharacterFighter PlayerFight, double FightTime) //Target Winner By:Kenshin
        {
            bool result;
            int targeLevel = PlayerFight.Character.Level;

            if (Draw)
            {
                result = false;
            }
            else
            {
                if (PlayerFight.OpposedTeam.AlignmentSide == AlignmentSideEnum.ALIGNMENT_NEUTRAL || _noHonor.Contains(PlayerFight))
                {
                    result = false;
                }
                else if (QtdVencedores == 0 || QtdPerdedores == 0)
                {
                    result = false;
                }
                else
                {

                    #region Level PVP  <= 50
                    if (targeLevel <= 50)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[0, 0];
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[0, 1];
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[0, 2];
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 51 ~ <= 100
                    else if (targeLevel >= 51 && targeLevel <= 100)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[1, 0];
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[1, 1];
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[1, 2];
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 101 ~ <= 150
                    else if (targeLevel >= 101 && targeLevel <= 150)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[2, 0];
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[2, 1];
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[2, 2];
                        }

                        result = true;
                    }
                    #endregion

                    #region Level PVP  >= 151
                    else if (targeLevel >= 151)
                    {
                        if (FightTime <= 300)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[3, 0];
                        }
                        else if (FightTime > 301 && FightTime <= 600)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[3, 1];
                        }
                        else if (FightTime >= 601)
                        {
                            PlayerFight.Loot.Kamas += earnedKamasMatrix[3, 2];
                        }

                        result = true;
                    }
                    #endregion
                    else
                        result = false;
                }
            }

            return result;
        }
        #endregion

        #region Calcular Ganho de Honra
        public short CalculateEarnedHonor(CharacterFighter character)
        {
            short result = 0;

            if (Draw)
            {
                result = 0;
            }
            else
            {
                if (character.OpposedTeam.AlignmentSide == AlignmentSideEnum.ALIGNMENT_NEUTRAL || _noHonor.Contains(character))
                {
                    result = 0;
                }
                else if (QtdVencedores == 0 || QtdPerdedores == 0)
                {
                    result = 0;
                }
                else
                {
                    int somaGanhador = (TotalNivAsaVencedores * (QtdPerdedores - 1)) + TotalNivelVencedores;
                    int somaPerdedor = (TotalNivAsaPerdedores * (QtdVencedores - 1)) + TotalNivelPerdedores;
                    bool passar = false;

                    /* - - - Geração de honra aos vencedores - - - */
                    if (_allWinners.Contains(character.Character))
                    {
                        if (QtdVencedores == 1 && QtdPerdedores == 1 && TotalNivelVencedores - TotalNivelPerdedores < (TotalNivelVencedores > 200 ? 50 : 20))
                        {
                            passar = true;
                        }

                        else if (somaPerdedor >= somaGanhador || somaGanhador / somaPerdedor < 1.255)
                        {
                            passar = true;
                        }

                        if (!passar)
                        {
                            return 0;
                        }
                        else if (QtdPerdedores == 1)
                        {
                            var _loser = _allLosers[0];

                            if (_loser != null)
                            {
                                if (_loser.Honor < 60)
                                {
                                    return 60;
                                }
                                else
                                {
                                    result = ((short)(_loser.Honor * 0.05));
                                }
                            }
                        }
                        else
                        {
                            double _sumHonor = 0;

                            foreach (var _actor in _allLosers)
                            {
                                _sumHonor += _actor.Honor;
                            }

                            result = ((short)((_sumHonor * 0.05) / QtdPerdedores));
                        }
                    } //Fim da checagem de vencedores
                    else /* - - - Geração de honra aos perdedores - - - */
                    {
                        if (QtdVencedores == 1 && QtdPerdedores == 1 && TotalNivelVencedores - TotalNivelPerdedores < (TotalNivelVencedores > 200 ? 50 : 20))
                        {
                            passar = true;
                        }
                        else if (somaPerdedor >= somaGanhador || somaGanhador / somaPerdedor < 1.255)
                        {
                            passar = true;
                        }

                        if (!passar)
                        {
                            return 0;
                        }

                        result = ((short)-((character.Character.Honor * 10) / 100));

                    }//Fim da checagem de perdedores
                }
            }
            return result;
        }
        #endregion

        #region Calcular Ganho de Rank
        private void CalculateEarnedRank(FightPlayerResult result)
        {
            if (Winners == result.Fighter.Team)
            {
                double fightTime = this.GetFightDuration().TotalSeconds;
                bool leaver = false;
                CharacterFighter loser = null;

                if (Losers.Fighters.Count > 0)
                {
                    loser = Losers.Fighters[0] as CharacterFighter;
                }
                else if (Leavers.Count > 0)
                {
                    loser = Leavers[0] as CharacterFighter;
                    leaver = true;
                }

                int bonusFight = 0;
                int winPoints = 0;

                if (loser != null)
                    winPoints += loser.Character.GetCharacterRankBonus();

                if (fightTime > 200)
                    bonusFight += 2;
                if (fightTime > 300) // 5 minutes
                    bonusFight += 5;
                else if (fightTime > 600) // 10 minutes
                    bonusFight += 10;
                else if (fightTime > 900) // 15 minutes
                    bonusFight += 15;

                winPoints += bonusFight;

                if (GetFightDuration().TotalMinutes < 2)
                    winPoints = 10;

                if (leaver && GetFightDuration().TotalMinutes < 2)
                {
                    winPoints = 5;

                    #region MSG
                    switch (result.Character.Account.Lang)
                    {
                        case "fr":
                            result.Character.SendServerMessage("Votre adversaire est parti très vite, vous ne gagnerez que <b>" + winPoints + "</b> CP dans ce combat.", Color.Chartreuse);
                            break;
                        case "es":
                            result.Character.SendServerMessage("Tu oponente abandonó muy rapido, solamente ganaras <b>" + winPoints + "</b> CP en este combate.", Color.Chartreuse);
                            break;
                        case "en":
                            result.Character.SendServerMessage("Your opponent left very fast, you will only win <b>" + winPoints + "</b> CP in this fight.", Color.Chartreuse);
                            break;
                        default:
                            result.Character.SendServerMessage("Seu oponente saiu muito rápido, você só ganhará <b>" + winPoints + "</b> CP nessa luta.", Color.Chartreuse);
                            break;
                    }
                    #endregion
                }
                else
                {
                    //Pevefichas
                    switch (result.Character.CharacterRankId)
                    {
                        case 1:
                            result.Loot.AddItem(10275, 5);
                            break;
                        case 2:
                            result.Loot.AddItem(10275, 25);
                            break;
                        case 3:
                            result.Loot.AddItem(10275, 50);
                            break;
                        case 4:
                            result.Loot.AddItem(10275, 10);
                            break;
                        case 5:
                            result.Loot.AddItem(10275, 150);
                            break;
                        case 6:
                            result.Loot.AddItem(10275, 250);
                            break;
                        default:
                            result.Loot.AddItem(10275, 5);
                            break;
                    }

                    #region MSG
                    switch (result.Character.Account.Lang)
                    {
                        case "fr":
                            result.Character.SendServerMessage("Vous avez gagné <b>" + winPoints + "</b CP dans ce combat.", Color.Chartreuse);
                            break;
                        case "es":
                            result.Character.SendServerMessage("Has ganado <b>" + winPoints + "</b> CP en este combate.", Color.Chartreuse);
                            break;
                        case "en":
                            result.Character.SendServerMessage("You have won <b>" + winPoints + "</b CP in this combat.", Color.Chartreuse);
                            break;
                        default:
                            result.Character.SendServerMessage("Você ganhou <b>" + winPoints + "</b CP neste combate.", Color.Chartreuse);
                            break;
                    }
                    #endregion
                }

                result.Character.CharacterRankWin += 1;
                result.Character.CharacterRankExp += winPoints;
            }
            else
            {
                CharacterFighter winner = Winners.Fighters[0] as CharacterFighter;
                double fightTime = this.GetFightDuration().TotalSeconds;

                if (winner != null)
                {
                    int lostPoint = result.Character.GetCharacterRankBonus();

                    if (winner.Character.CharacterRankId < result.Character.CharacterRankId)
                        lostPoint += (int)(winner.Character.GetCharacterRankBonus());

                    if (fightTime > 200)
                        lostPoint += 2;
                    if (fightTime > 300) // 5 minutes
                        lostPoint += 5;
                    else if (fightTime > 600) // 10 minutes
                        lostPoint += 10;
                    else if (fightTime > 900) // 15 minutes
                        lostPoint += 15;

                    result.Character.CharacterRankLose += 1;

                    #region MSG
                    switch (result.Character.Account.Lang)
                    {
                        case "fr":
                            result.Character.SendServerMessage("Vous avez perdu <b>" + lostPoint + "</b> CP dans ce combat.", Color.Chartreuse);
                            break;
                        case "es":
                            result.Character.SendServerMessage("Has perdido <b>" + lostPoint + "</b> CP en este combate.", Color.Chartreuse);
                            break;
                        case "en":
                            result.Character.SendServerMessage("You have lost <b>" + lostPoint + "</b> CP in this combat.", Color.Chartreuse);
                            break;
                        default:
                            result.Character.SendServerMessage("Você perdeu <b>" + lostPoint + "</b> CP neste combate.", Color.Chartreuse);
                            break;
                    }
                    #endregion

                    result.Character.CharacterRankExp -= lostPoint;
                }
            }
        }
        #endregion

        #region Calcular Ganho de XP
        private bool CalculateCharacterEarnedExp(FightPlayerResult PlayerFight, double fightTime)
        {
            bool result;
            int targeLevel = PlayerFight.Character.Level;

            if (Draw)
            {
                result = false;
            }
            else
            {
                if (PlayerFight.Character.AlignmentSide == AlignmentSideEnum.ALIGNMENT_NEUTRAL || _noHonor.Contains(PlayerFight.Fighter))
                {
                    result = false;
                }
                else if (QtdVencedores == 0 || QtdPerdedores == 0)
                {
                    result = false;
                }
                else
                {
                    #region Level 51 ~ 100
                    /* Level 51 ~ 100 */
                    if (targeLevel >= 51 && targeLevel <= 100)
                    {
                        if (fightTime <= 300)
                        {
                            PlayerFight.AddEarnedExperience(earnedEXPMatrix[0, 0]);
                        }

                        else if (fightTime > 301 && fightTime <= 600)
                        {
                            PlayerFight.AddEarnedExperience(earnedEXPMatrix[0, 1]);
                        }

                        else if (fightTime >= 601)
                        {
                            PlayerFight.AddEarnedExperience(earnedEXPMatrix[0, 2]);
                        }
                        result = true;
                    }
                    #endregion

                    #region Level 101 ~ 150
                    /* Level 51 ~ 100 */
                    else if (targeLevel >= 101 && targeLevel <= 150)
                    {
                        if (fightTime <= 300)
                        {
                            PlayerFight.AddEarnedExperience(earnedEXPMatrix[1, 0]);
                        }

                        else if (fightTime > 301 && fightTime <= 600)
                        {
                            PlayerFight.AddEarnedExperience(earnedEXPMatrix[1, 1]);
                        }
                        else if (fightTime >= 601)
                        {
                            PlayerFight.AddEarnedExperience(earnedEXPMatrix[1, 2]);
                        }
                        result = true;
                    }
                    #endregion

                    #region Level 151 ~ 200
                    /* Level 151 ~ 200 */
                    else if (targeLevel >= 151)
                    {
                        if (fightTime <= 300)
                        {
                            PlayerFight.AddEarnedExperience(earnedEXPMatrix[2, 0]);
                        }

                        else if (fightTime > 301 && fightTime <= 600)
                        {
                            PlayerFight.AddEarnedExperience(earnedEXPMatrix[2, 1]);
                        }

                        else if (fightTime >= 601)
                        {
                            PlayerFight.AddEarnedExperience(earnedEXPMatrix[2, 2]);
                        }
                        result = true;
                    }
                    #endregion
                    else
                        result = false;
                }
            }

            return result;
        }

        private bool CalculateTargetEarnedExp(CharacterFighter PlayerFight, double fightTime)
        {
            bool result;
            int targeLevel = PlayerFight.Character.Level;

            if (Draw)
            {
                result = false;
            }
            else
            {
                if (PlayerFight.OpposedTeam.AlignmentSide == AlignmentSideEnum.ALIGNMENT_NEUTRAL || _noHonor.Contains(PlayerFight))
                {
                    result = false;
                }
                else if (QtdVencedores == 0 || QtdPerdedores == 0)
                {
                    result = false;
                }
                else
                {
                    #region Level 51 ~ 100
                    /* Level 51 ~ 100 */
                    if (targeLevel >= 51 && targeLevel <= 100)
                    {
                        if (fightTime <= 300)
                        {
                            PlayerFight.Loot.Experience += earnedEXPMatrix[0, 0];
                        }
                        else if (fightTime > 301 && fightTime <= 600)
                        {
                            PlayerFight.Loot.Experience += earnedEXPMatrix[0, 1];
                        }
                        else if (fightTime >= 601)
                        {
                            PlayerFight.Loot.Experience += earnedEXPMatrix[0, 2];
                        }
                        result = true;
                    }
                    #endregion

                    #region Level 101 ~ 150
                    /* Level 51 ~ 100 */
                    else if (targeLevel >= 101 && targeLevel <= 150)
                    {
                        if (fightTime <= 300)
                        {
                            PlayerFight.Loot.Experience += earnedEXPMatrix[1, 0];
                        }
                        else if (fightTime > 301 && fightTime <= 600)
                        {
                            PlayerFight.Loot.Experience += earnedEXPMatrix[1, 1];
                        }
                        else if (fightTime >= 601)
                        {
                            PlayerFight.Loot.Experience += earnedEXPMatrix[1, 2];
                        }
                        result = true;
                    }
                    #endregion

                    #region Level 151 ~ 200
                    /* Level 151 ~ 200 */
                    else if (targeLevel >= 151)
                    {
                        if (fightTime <= 300)
                        {
                            PlayerFight.Loot.Experience += earnedEXPMatrix[2, 0];
                        }
                        else if (fightTime > 301 && fightTime <= 600)
                        {
                            PlayerFight.Loot.Experience += earnedEXPMatrix[2, 1];
                        }
                        else if (fightTime >= 601)
                        {
                            PlayerFight.Loot.Experience += earnedEXPMatrix[2, 2];
                        }
                        result = true;
                    }
                    #endregion
                    else
                        result = false;
                }
            }

            return result;
        }
        #endregion

        #region Calcular Desonra
        public short CalculateEarnedDishonor(CharacterFighter character)
        {
            if (Draw)
                return 0;

            return character.OpposedTeam.AlignmentSide != AlignmentSideEnum.ALIGNMENT_NEUTRAL ? (short)0 : (short)1;
        }
        #endregion
        #endregion
    }
}
