using System;
using Stump.Core.Attributes;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Handlers.Inventory;
using Stump.Core.Mathematics;
using Stump.Core.Timers;
using Stump.Server.WorldServer.Game.Jobs;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Core.Reflection;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Handlers.Context;
using Stump.DofusProtocol.Messages;
using System.Linq;
using MongoDB.Bson;
using Stump.Server.BaseServer.Logging;
using System.Globalization;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    public class SkillHarvest : Skill, ISkillWithAgeBonus
    {
        [Variable(true)]
        public static int StarsBonusRate = 1800;

        [Variable(true)]
        public static short StarsBonusLimit = 200;

        public const short ClientStarsBonusLimit = 200;

        [Variable]
        public static int HarvestTime = 3000;

        [Variable]
        public static int RegrowTime = 60000;

        ItemTemplate m_harvestedItem;
        private TimedTimerEntry m_regrowTimer;

        public SkillHarvest(int id, InteractiveSkillTemplate skillTemplate, InteractiveObject interactiveObject) : base(id, skillTemplate, interactiveObject)
        {
            m_harvestedItem = ItemManager.Instance.TryGetTemplate(SkillTemplate.GatheredRessourceItem);
            CreationDate = DateTime.Now;

            if (m_harvestedItem == null)
                throw new Exception($"Harvested item {SkillTemplate.GatheredRessourceItem} doesn't exist");
        }

        public bool Harvested => HarvestedSince.HasValue && (DateTime.Now - HarvestedSince).Value.TotalMilliseconds < RegrowTime;

        public DateTime CreationDate
        {
            get;
            private set;
        }

        public DateTime EnabledSince => HarvestedSince + TimeSpan.FromMilliseconds(RegrowTime) ?? CreationDate;

        public DateTime? HarvestedSince
        {
            get;
            private set;
        }

        public short AgeBonus
        {
            get
            {
                var bonus = (DateTime.Now - EnabledSince).TotalSeconds / (StarsBonusRate);

                if (bonus > StarsBonusLimit)
                    bonus = StarsBonusLimit;

                return (short)bonus;
            }
            set
            {
                HarvestedSince = DateTime.Now - TimeSpan.FromMilliseconds(RegrowTime) - TimeSpan.FromSeconds(value * StarsBonusRate);
            }
        }

        public override int GetDuration(Character character, bool forNetwork = false) => HarvestTime;

        public override bool IsEnabled(Character character) => base.IsEnabled(character) && !Harvested && character.Jobs[SkillTemplate.ParentJobId].Level >= SkillTemplate.LevelMin;

        public override int StartExecute(Character character)
        {
            InteractiveObject.SetInteractiveState(InteractiveStateEnum.STATE_ANIMATED);

            base.StartExecute(character);

            return GetDuration(character);
        }

        public override void EndExecute(Character character)
        {
            var count = RollHarvestedItemCount(character);
            var bonus = (int)Math.Floor(count * (AgeBonus / 100d));

            SetHarvested();

            InteractiveObject.SetInteractiveState(InteractiveStateEnum.STATE_ACTIVATED);

            if (character.Inventory.IsFull(m_harvestedItem, count))
            {
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 964); //Seu inventário está cheio. Você não pode adicionar mais nenhum item.
                base.EndExecute(character);
                return;
            }

            character.Inventory.AddItem(m_harvestedItem, count + bonus);
            InventoryHandler.SendObtainedItemWithBonusMessage(character.Client, m_harvestedItem, count, bonus);

            if (SkillTemplate.ParentJobId != 1)
            {
                var xp = JobManager.Instance.GetHarvestJobXp((int)SkillTemplate.LevelMin);
                character.Jobs[SkillTemplate.ParentJobId].Experience += xp;
            }

            character.OnHarvestItem(m_harvestedItem, count + bonus);

            base.EndExecute(character);

            #region Protetores de Recursos by: Kenshin
            var random = new Random();
            int[] allowedParentJobIds = { 26, 28, 24, 36, 2 };
            long jobLevel = character.Jobs[SkillTemplate.ParentJobId].Experience;

            if (jobLevel > 3799 && allowedParentJobIds.Contains(SkillTemplate.ParentJobId) && SkillTemplate.MonsterId != 0)
            {
                if (character.Map.Record.BlueFightCells.Length == 0 || character.Map.Record.RedFightCells.Length == 0)
                {
                    #region // ----------------- Sistema de Logs MongoDB Erro Fight by: Kenshin ---------------- //
                    try
                    {
                        var document = new BsonDocument
                        {
                                { "CharacterId", character.Id },
                                { "CharacterName", character.Name },
                                { "WorldMapId", character.Map.Id },
                                { "BlueFightCells", character.Map.Record.BlueFightCells.Length },
                                { "RedFightCells", character.Map.Record.RedFightCells.Length },
                                { "Modo", "Harvest" },
                                { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                            };

                        MongoLogger.Instance.Insert("World_FightCells", document);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Erro no Mongologs Erro de Celulas no Mapa : " + e.Message);
                    }
                    #endregion
                }
                else
                {
                    const int monsterGradeCount = 5;
                    const int levelThreshold = 10;
                    const int monsterGradeMultiplier = 40;
                    const int randomThreshold = 1000; // Valor máximo para gerar a probabilidade precisa de 10% (1 a 1000)

                    var monsterGradeId = 1;

                    while (monsterGradeId < monsterGradeCount && character.Level <= monsterGradeId * monsterGradeMultiplier + levelThreshold)
                    {
                        monsterGradeId++;

                        if (character.Level > monsterGradeId * monsterGradeMultiplier + levelThreshold)
                        {
                            break;
                        }
                    }

                    var monsterId = SkillTemplate.MonsterId;
                    var randomNumber = random.Next(1, randomThreshold); // Gerar número entre 1 e randomThreshold

                    if (randomNumber <= 100) // Verificar se o número está dentro do intervalo de 1 a 100 (10%)
                    {
                        var grade = Singleton<MonsterManager>.Instance.GetMonsterGrade(monsterId, monsterGradeId);
                        var position = new ObjectPosition(character.Map, character.Cell, (DirectionsEnum)5);
                        var monster = new Monster(grade, new MonsterGroup(0, position));

                        var fight = Singleton<FightManager>.Instance.CreatePvMFight(character.Map);
                        fight.ChallengersTeam.AddFighter(character.CreateFighter(fight.ChallengersTeam));
                        fight.DefendersTeam.AddFighter(new MonsterFighter(fight.DefendersTeam, monster));
                        fight.StartPlacement();

                        fight.HideBlades();

                        var message = new GameFightJoinRequestMessage(character.Fighter.Id, (ushort)fight.Id);
                        ContextHandler.HandleGameFightJoinRequestMessage(character.Client, message);

                        character.SaveLater();
                    }

                }
            }
            #endregion
        }

        public void SetHarvested()
        {
            HarvestedSince = DateTime.Now;
            InteractiveObject.Map.Refresh(InteractiveObject);
            m_regrowTimer = InteractiveObject.Area.CallDelayed(RegrowTime, Regrow);
        }

        public void Regrow()
        {
            if (m_regrowTimer != null)
            {
                m_regrowTimer.Stop();
                m_regrowTimer = null;
            }

            InteractiveObject.Map.Refresh(InteractiveObject);
            InteractiveObject.SetInteractiveState(InteractiveStateEnum.STATE_NORMAL);
        }

        int RollHarvestedItemCount(Character character)
        {
            var job = character.Jobs[SkillTemplate.ParentJobId];
            var minMax = JobManager.Instance.GetHarvestItemMinMax(job.Template, job.Level, SkillTemplate);
            return new CryptoRandom().Next(minMax.First, minMax.Second + 1);
        }
    }
}