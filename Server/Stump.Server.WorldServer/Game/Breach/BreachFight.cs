using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stump.Core.Extensions;
using Stump.Core.Mathematics;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Challenges;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Formulas;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Handlers.Characters;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.Game.Breach
{
    public class BreachFight : Fight<FightMonsterTeam, FightPlayerTeam>
    {
        private int step;
        private Character leader;

        public BreachFight(int id, Character leader, Map fightMap, FightMonsterTeam defendersTeam, FightPlayerTeam challengersTeam, int step) : base(id, fightMap, defendersTeam, challengersTeam)
        {
            this.leader = leader;
            this.step = step;
        }

        public FightPlayerTeam PlayerTeam => Teams.FirstOrDefault(x => x.TeamType == TeamTypeEnum.TEAM_TYPE_PLAYER) as FightPlayerTeam;

        public FightMonsterTeam MonsterTeam => Teams.FirstOrDefault(x => x.TeamType == TeamTypeEnum.TEAM_TYPE_MONSTER) as FightMonsterTeam;

        public override FightTypeEnum FightType => FightTypeEnum.FIGHT_TYPE_BREACH;

        public override bool IsPvP => false;

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

        public static Dictionary<short, byte> OrnamentsEarnablesSonge = new Dictionary<short, byte>()
        {
            {50, 139},
            {75, 141},
            {100, 143},
            {150, 145},
            {200, 147},
            {350, 149},
            {400, 151}
        };

        protected override void OnFightEnded()
        {
            base.OnFightEnded();

            Task.Delay(3000).ContinueWith(t =>
            {
                leader.Teleport(new ObjectPosition(Singleton<World>.Instance.GetMap(195561472), 300, DirectionsEnum.DIRECTION_SOUTH_EAST));

                foreach (var entry in this.ChallengersTeam.Fighters)
                {
                    if (entry is CharacterFighter)
                    {
                        Character character = (entry as CharacterFighter).Character;

                        if (character.Id != leader.Id)
                        {
                            character.Teleport(new ObjectPosition(Singleton<World>.Instance.GetMap(195561472), 300, DirectionsEnum.DIRECTION_SOUTH_EAST));
                        }
                    }
                }
            });

            foreach (var team in m_teams)
            {
                if (team == Winners && Winners.Fighters.Contains(ChallengersTeam.Leader))
                {
                    leader.BreachStep++;
                    leader.BreachBranches = BreachBranche.generateBreachBranches(leader);
                    leader.BreachBuyables = new BreachReward[] { };

                    foreach (var boost in leader.CurrentBreachRoom.rewards)
                    {
                        leader.BreachBuyables = leader.BreachBuyables.Add(boost);
                    }

                    if (leader.BreachStep >= 201)
                    {
                        leader.BreachBuyables = new BreachReward[] { };
                        leader.OpenPopup("Vous venez de terminer votre run Breach ! Retour à la salle 1 !");
                        leader.BreachStep = 1;
                        leader.BreachBoosts = new ObjectEffectInteger[] { };
                        leader.BreachBranches = BreachBranche.generateBreachBranches(leader);
                        leader.BreachBudget = 0;
                        leader.BreachBuyables = new BreachReward[] { };
                    }

                    foreach (var earnable in OrnamentsEarnablesSonge)
                    {
                        if (leader.BreachStep >= earnable.Key)
                        {
                            if (!leader.Ornaments.Contains(earnable.Value))
                            {
                                leader.AddOrnament(earnable.Value);
                                leader.SendServerMessage("Vous avez obtenu un nouvel ornement !");
                            }
                        }
                    }
                }
            }

            foreach (var entry in this.GetAllFighters())
            {
                if (entry is CharacterFighter)
                {
                    Character character = (entry as CharacterFighter).Character;
                    character.Stats.Health.DamageTaken = 0;
                    CharacterHandler.SendCharacterStatsListMessage(character.Client);
                }
            }
        }

        protected override void OnFightStarted()
        {
            base.OnFightStarted();
            initChallenge();

            void initChallenge()
            {
                for (int i = 0; i < 2; i++)
                {
                    var challenge = ChallengeManager.Instance.GetRandomChallenge(this);

                    if (challenge == null)
                        return;

                    challenge.Initialize();
                    AddChallenge(challenge);
                }

                foreach (var characterFighter in this.ChallengersTeam.Fighters)
                {
                    if (characterFighter is CharacterFighter)
                    {
                        Character character = (characterFighter as CharacterFighter).Character;

                        foreach (var boost in leader.BreachBoosts)
                        {
                            switch (boost.actionId)
                            {
                                case 752:
                                    character.Stats[PlayerFields.TackleEvade].Context += (int)boost.value;
                                    break;
                                case 753:
                                    character.Stats[PlayerFields.TackleBlock].Context += (int)boost.value;
                                    break;
                                case 126:
                                    character.Stats[PlayerFields.Intelligence].Context += (int)boost.value;
                                    break;
                                case 123:
                                    character.Stats[PlayerFields.Chance].Context += (int)boost.value;
                                    break;
                                case 119:
                                    character.Stats[PlayerFields.Agility].Context += (int)boost.value;
                                    break;
                                case 118:
                                    character.Stats[PlayerFields.Strength].Context += (int)boost.value;
                                    break;
                                case 125:
                                    character.Stats[PlayerFields.Vitality].Context += (int)boost.value;
                                    break;
                                case 138:
                                    character.Stats[PlayerFields.DamageBonusPercent].Context += (int)boost.value;
                                    break;
                                case 410:
                                    character.Stats[PlayerFields.APAttack].Context += (int)boost.value;
                                    break;
                                case 412:
                                    character.Stats[PlayerFields.MPAttack].Context += (int)boost.value;
                                    break;
                                default:
                                    break;
                            }
                        }

                        CharacterHandler.SendCharacterStatsListMessage(character.Client);
                    }
                }
            }
        }

        protected override void OnFighterAdded(FightTeam team, FightActor actor)
        {
            base.OnFighterAdded(team, actor);

            if (!(team is FightMonsterTeam))
                return;
        }

        protected override List<IFightResult> GetResults()
        {
            var cryptoRandom = new CryptoRandom();
            var results = new List<IFightResult>();
            Character leader = this.leader;

            results.AddRange(GetFightersAndLeavers().Where(entry => entry.HasResult).Select(entry => entry.GetFightResult()));
            {
                foreach (var team in m_teams)
                {
                    IEnumerable<FightActor> droppers = team.OpposedTeam.GetAllFighters(entry => entry.IsDead() && entry.CanDrop()).ToList();

                    var looters = results.Where(x => x.CanLoot(team) && !(x is TaxCollectorProspectingResult))
                        .OrderByDescending(entry => entry.Prospecting).Concat(results.OfType<TaxCollectorProspectingResult>().Where(x => x.CanLoot(team)).OrderByDescending(x => x.Prospecting));

                    int random = cryptoRandom.Next(2);

                    foreach (var looter in looters)
                    {
                        if (team == Winners && looter is FightPlayerResult)
                        {
                            if (looter is IExperienceResult)
                            {
                                var winXP = FightFormulas.CalculateWinExp(looter, team.GetAllFighters<CharacterFighter>(), droppers);
                                var biggestwave = DefendersTeam.m_wavesFighters.OrderByDescending(x => x.WaveNumber).FirstOrDefault();

                                //if (biggestwave != null)
                                //    winXP = FightFormulas.CalculateWinExp(looter, team.GetAllFighters<CharacterFighter>(), droppers, biggestwave.WaveNumber + 1);

                                (looter as IExperienceResult).AddEarnedExperience(team == Winners ? winXP : (long)Math.Round(winXP * 0.10));
                            }
                        }
                    }

                    if (Winners == null || Draw)
                        return results;
                }
            }

            this.leader.BreachBudget += 100;

            return results;
        }

        protected override void SendGameFightJoinMessage(CharacterFighter fighter)
        {
            ContextHandler.SendGameFightJoinMessage(fighter.Character.Client, true, true, IsStarted, IsStarted ? 0 : (int)GetPlacementTimeLeft().TotalMilliseconds / 100, FightType);
        }

        protected override bool CanCancelFight()
        {
            return false;
        }

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
