using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.KoliLog;
using System;
using System.Linq;
using FightLoot = Stump.Server.WorldServer.Game.Fights.Results.FightLoot;

namespace Stump.Server.WorldServer.Game.Arena
{
    public class ArenaFightResult : FightResult<CharacterFighter>
    {
        public ArenaFightResult(CharacterFighter fighter, FightOutcomeEnum outcome, FightLoot loot, int rank, bool showLoot = true) : base(fighter, outcome, loot)
        {
            Rank = rank;
            ShowLoot = showLoot;
        }

        public override bool CanLoot(FightTeam team) => Outcome == FightOutcomeEnum.RESULT_VICTORY && !Fighter.HasLeft() && ShowLoot;

        public int Rank { get; }

        public bool ShowLoot { get; }

        public override FightResultListEntry GetFightResultListEntry()
        {
            int amount = 0;
            ulong kamas = 0;

            if (CanLoot(Fighter.Team))
            {
                amount = Fighter.Character.ComputeWonArenaTokens(Rank);
                kamas = Fighter.Character.ComputeWonArenaKamas();
                var lastfights_hdr = KoliLog_manager.Instance.GetHardwareRecord(Fighter.Character.Account.LastHardwareId).Where(x => DateTime.Now - x.Time > TimeSpan.FromSeconds(3));
                var lastfights_ip = KoliLog_manager.Instance.GetIpRecords(Fighter.Character.Client.IP).Where(x => DateTime.Now - x.Time > TimeSpan.FromSeconds(3));
                var lastfights = lastfights_hdr.Concat(lastfights_ip.Where(x => !lastfights_hdr.Contains(x))).ToList();

                if (lastfights.Count() > 0)
                {
                    int opponentcount = 0;

                    foreach (var opponent in Fighter.OpposedTeam.GetAllFightersWithLeavers().OfType<CharacterFighter>())
                    {
                        if (lastfights.Any(x => x.Ip_opponent == opponent.Character.Client.IP && x.Koli_Type == Fighter.Character.ArenaMode) || lastfights.Any(x => x.Hardware_opponent == opponent.Character.Account.LastHardwareId && x.Koli_Type == Fighter.Character.ArenaMode))
                        {
                            opponentcount++;
                        }
                    }
                    if (opponentcount > 0)
                    {
                        if (Fighter.Character.ArenaMode == 1)
                        {
                            amount = 0;
                            kamas = 0;
                        }
                        else
                        {
                            amount = (int)Math.Floor((double)(opponentcount / 3) * amount);
                            kamas = (ulong)Math.Floor((double)(opponentcount / 3) * kamas);
                        }
                    }
                }
            }

            var items = amount > 0 ? new[] { (short)ItemIdEnum.KOLIZETON_12736, (short)amount } : new short[0];
            var loot = new DofusProtocol.Types.FightLoot((System.Collections.Generic.IEnumerable<FightLootObject>)items.Select(x => (uint)x), kamas);

            return new FightResultPlayerListEntry((ushort)Outcome, 0, loot, Id, Alive, (ushort)Level, new FightResultAdditionalData[0]);
        }

        public override void Apply()
        {
            Fighter.Character.UpdateArenaProperties(Rank, Outcome == FightOutcomeEnum.RESULT_VICTORY, Fighter.Character.ArenaMode);
        }

        private int ApplyTokens(Character owner)
        {
            int amountTokens = 0;

            if (owner.ArenaTokensMax < 500)
            {
                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                {
                    amountTokens = owner.ComputeWonArenaTokens(Rank);
                }
                else
                {
                    Random random = new Random();
                    int randomNumber = random.Next(101);

                    if (randomNumber >= 70)
                        amountTokens = owner.ComputeWonArenaTokens(Rank);
                }

                if (amountTokens > 0 && World.Instance.GetWorldStatus() != ServerStatusEnum.SAVING)
                {
                    if (owner.Inventory.CreateTokenItem(amountTokens, "ArenaFightResult"))
                    {
                        //TODO Protection OverDROP
                        owner.ArenaTokensMax += amountTokens;
                        owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 21, amountTokens, Settings.TokenTemplateId);

                        return amountTokens;
                    }
                }
            }

            return 0;
        }
    }
}