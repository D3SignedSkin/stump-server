using System.Collections.Generic;
using System.Linq;
using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors.RolePlay.TaxCollectors;
using Stump.Server.WorldServer.Game.Actors.Stats;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Actors.Fight
{
    public sealed class TaxCollectorFighter : AIFighter
    {
        private readonly StatsFields m_stats;

        public TaxCollectorFighter(FightTeam team, TaxCollectorNpc taxCollector) : base(team, taxCollector.Guild.GetTaxCollectorSpells(), taxCollector.GlobalId)
        {
            Id = Fight.GetNextContextualId();
            TaxCollectorNpc = taxCollector;
            Look = TaxCollectorNpc.Look.Clone();
            Items = TaxCollectorNpc.Bag.SelectMany(x => Enumerable.Repeat(x.Template.Id, (int)x.Stack)).Shuffle().ToList();
            Kamas = TaxCollectorNpc.GatheredKamas;
            m_stats = new StatsFields(this);
            m_stats.Initialize(TaxCollectorNpc);
            Cell cell;

            if (!Fight.FindRandomFreeCell(this, out cell, false))
                return;

            Position = new ObjectPosition(TaxCollectorNpc.Map, cell, TaxCollectorNpc.Direction);
        }

        public TaxCollectorNpc TaxCollectorNpc
        {
            get;
        }

        public override string Name => TaxCollectorNpc.Name;

        public override ObjectPosition MapPosition => TaxCollectorNpc.Position;

        public override ushort Level => TaxCollectorNpc.Level;

        public override bool Vip => false;

        public override RoleEnum Role => RoleEnum.Player;

        public override Character Owner => (Summoner as CharacterFighter).Character;


        public override StatsFields Stats => m_stats;

        public List<int> Items
        {
            get;
        }

        public long Kamas
        {
            get;
        }

        public override string GetMapRunningFighterName() => TaxCollectorNpc.Name;

        public override IFightResult GetFightResult(FightOutcomeEnum outcome) => new TaxCollectorFightResult(this, outcome, Loot);

        //public TaxCollectorFightersInformation GetTaxCollectorFightersInformation()
        //{
        //    FightPvT pvtFight = Fight as FightPvT;
        //    var allies = Fight.State == FightState.Placement && pvtFight != null ? pvtFight.DefendersQueue.Select(x => x.GetCharacterMinimalPlusLookInformations()) : Team.Fighters.OfType<CharacterFighter>().Select(x => x.Character.GetCharacterMinimalPlusLookInformations());

        //    return new TaxCollectorFightersInformation(TaxCollectorNpc.GlobalId, allies, OpposedTeam.Fighters.OfType<CharacterFighter>().Select(x => x.Character.GetCharacterMinimalPlusLookInformations()));
        //}

        public override GameFightFighterLightInformations GetGameFightFighterLightInformations(WorldClient client = null) => new GameFightFighterTaxCollectorLightInformations(
                true,
                IsAlive(),
                Id,
                0,
                Level,
                (sbyte)BreedEnum.TAX_COLLECTOR,
                (ushort)TaxCollectorNpc.FirstNameId,
                (ushort)TaxCollectorNpc.LastNameId);

        public override FightTeamMemberInformations GetFightTeamMemberInformations()
        {
            return new FightTeamMemberTaxCollectorInformations(
                id: Id,
                firstNameId: (ushort)TaxCollectorNpc.FirstNameId,
                lastNameId: (ushort)TaxCollectorNpc.LastNameId,
                groupId: (uint)TaxCollectorNpc.Guild.Id,
                uid: TaxCollectorNpc.GlobalId);
        }

        public override GameFightFighterInformations GetGameFightFighterInformations(WorldClient client = null)
        {
            return new GameFightTaxCollectorInformations(
                contextualId: Id,
                disposition: GetEntityDispositionInformations(client),
                look: Look.GetEntityLook(),
                spawnInfo: GetGameContextBasicSpawnInformation(),
                wave: 0,
                stats: GetGameFightMinimalStats(client),
                previousPositions: new ushort[0],
                firstNameId: (ushort)TaxCollectorNpc.FirstNameId,
                lastNameId: (ushort)TaxCollectorNpc.LastNameId);
        }
    }
}
