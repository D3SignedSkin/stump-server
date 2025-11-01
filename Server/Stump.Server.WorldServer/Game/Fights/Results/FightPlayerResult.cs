using System;
using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights.Results.Data;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Guilds;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Handlers.Characters;
using Stump.Server.WorldServer.Handlers.Inventory;

namespace Stump.Server.WorldServer.Game.Fights.Results
{
    public class FightPlayerResult : FightResult<CharacterFighter>, IExperienceResult, IPvpResult
    {
        public FightPlayerResult(CharacterFighter fighter, FightOutcomeEnum outcome, FightLoot loot) : base(fighter, outcome, loot)
        { }

        public Character Character => Fighter.Character;

        public new ushort Level => Character.Level;

        public override bool CanLoot(FightTeam team) => Fighter.Team == team && (!Fighter.HasLeft() || Fighter.IsDisconnected);

        public FightExperienceData ExperienceData
        {
            get;
            private set;
        }

        public FightPvpData PvpData
        {
            get;
            private set;
        }

        public override FightResultListEntry GetFightResultListEntry()
        {
            var additionalDatas = new List<DofusProtocol.Types.FightResultAdditionalData>();

            if (ExperienceData != null)
                additionalDatas.Add(ExperienceData.GetFightResultAdditionalData());

            if (PvpData != null)
                additionalDatas.Add(PvpData.GetFightResultAdditionalData());

            return new FightResultPlayerListEntry((ushort)Outcome, 0, Loot.GetFightLoot(), Id, Alive, (ushort)Level,
                additionalDatas);
        }

        public override void Apply()
        {
            Character.Inventory.AddKamas(Loot.Kamas);

            foreach (var drop in Loot.Items.Values)
            {
                if (drop == null)
                    continue;

                if (drop.IgnoreGeneration)
                    continue;

                var template = ItemManager.Instance.TryGetTemplate(drop.ItemId);

                if (template != null)
                {
                    if (template.Effects.Count > 0)
                    {
                        for (var i = 0; i < drop.Amount; i++)
                        {
                            var item = ItemManager.Instance.CreatePlayerItem(Character, drop.ItemId, 1);

                            Character.Inventory.AddItem(item, false);
                        }
                    }
                    else
                    {
                        var item = ItemManager.Instance.CreatePlayerItem(Character, drop.ItemId, (int)drop.Amount);
                        Character.Inventory.AddItem(item, false);
                    }
                }
                else
                {
                    Console.WriteLine("DROP Item Error : " + drop.ItemId);
                    continue;
                }
            }

            if (ExperienceData != null)
                ExperienceData.Apply();

            if (PvpData != null)
                PvpData.Apply();

            CharacterHandler.SendCharacterStatsListMessage(Character.Client);
            InventoryHandler.SendInventoryContentMessage(Character.Client);
        }

        public void AddEarnedExperience(double experience)
        {
            if (Fighter.HasLeft() && !Fighter.IsDisconnected)
                return;

            if (ExperienceData == null)
                ExperienceData = new FightExperienceData(Character);

            if (Character.IsRiding && Character.EquippedMount.GivenExperience > 0)
            {
                var xp = (double)(experience * (Character.EquippedMount.GivenExperience * 0.01));
                var mountXp = (double)Character.EquippedMount.AdjustGivenExperience(Character, (long)xp);

                experience -= xp;

                if (mountXp > 0)
                {
                    ExperienceData.ShowExperienceForMount = true;
                    ExperienceData.ExperienceForMount += (int)mountXp;
                }
            }

            if (Character.GuildMember != null && Character.GuildMember.GivenPercent > 0)
            {
                var xp = (double)(experience * (Character.GuildMember.GivenPercent * 0.01));
                var guildXp = (double)Character.Guild.AdjustGivenExperience(Character, (float)xp);

                guildXp = guildXp * 10;//bonus
                experience -= xp;
                guildXp = guildXp > Guild.MaxGuildXP ? Guild.MaxGuildXP : guildXp;

                if (guildXp > 0)
                {
                    ExperienceData.ShowExperienceForGuild = true;
                    ExperienceData.ExperienceForGuild += (int)guildXp;
                }
            }

            //if(Character.Account.CreationDate.AddDays(2) > System.DateTime.Now)
            //{
            //    experience *= 2;
            //}

            ExperienceData.ShowExperienceFightDelta = true;
            ExperienceData.ShowExperience = true;
            ExperienceData.ShowExperienceLevelFloor = true;
            ExperienceData.ShowExperienceNextLevelFloor = true;

            if (experience < 0)
                experience = 0;

            ExperienceData.ExperienceFightDelta += experience;

            if (ExperienceData.ExperienceFightDelta < 0)
                ExperienceData.ExperienceFightDelta = 0;

            CharacterHandler.SendCharacterExperienceGainMessage(this.Character.Client, (long)ExperienceData.ExperienceFightDelta, ExperienceData.ExperienceForMount, ExperienceData.ExperienceForGuild, 0);
        }

        public void SetEarnedHonor(short honor, short dishonor)
        {
            if (PvpData == null)
                PvpData = new FightPvpData(Character);

            PvpData.HonorDelta = honor;
            PvpData.DishonorDelta = dishonor;
            PvpData.Honor = Character.Honor;
            PvpData.Dishonor = Character.Dishonor;
            PvpData.Grade = (byte)Character.AlignmentGrade;
            PvpData.MinHonorForGrade = Character.LowerBoundHonor;
            PvpData.MaxHonorForGrade = Character.UpperBoundHonor;
        }
    }
}