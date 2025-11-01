using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Stats;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;
using System.Linq;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.Game.Actors.Fight
{
    public class SummonedClone : SummonedFighter
    {
        protected int spellUsedId = 0;
        protected readonly StatsFields m_stats;

        public SummonedClone(int id, FightActor caster, Cell cell, int SpellId = 0) : base(id, caster.Team, new List<Spell>(), caster, cell)
        {
            Caster = caster;
            Look = caster.Look.Clone();
            m_stats = new StatsFields(this);
            m_stats.InitializeFromStats(caster.Stats);
            spellUsedId = SpellId;

            if (Caster is CharacterFighter && (Caster as CharacterFighter).Character.BreedId == PlayableBreedEnum.Sram)
                Stats.Health.DamageTaken = 0;

            ResetUsedPoints();
        }

        public FightActor Caster { get; }

        public override ObjectPosition MapPosition => Position;

        public override string GetMapRunningFighterName() => Name;

        public override ushort Level => Caster.Level;

        public override bool Vip => false;

        public override RoleEnum Role => RoleEnum.Player;

        public override Character Owner => (Caster as CharacterFighter).Owner;

        public override string Name => (Caster is NamedFighter) ? ((NamedFighter)Caster).Name : "(no name)";

        public override StatsFields Stats => m_stats;

        protected override void OnDead(FightActor killedBy, bool passTurn = true, bool isKillEffect = false)
        {
            if (Caster is CharacterFighter && (Caster as CharacterFighter).Character.BreedId == PlayableBreedEnum.Sram)
            {
                if (spellUsedId == (int)SpellIdEnum.DOUBLE_12915)
                {
                    var spellHandler = SpellManager.Instance.GetSpellCastHandler(this, new Spell((int)SpellIdEnum.EXPLOSION_OF_THE_PLOTTER_19725, 1), Caster.Cell, false);

                    spellHandler.Initialize();

                    var handlers = spellHandler.GetEffectHandlers().ToArray();

                    using (this.Fight.StartSequence(SequenceTypeEnum.SEQUENCE_SPELL))
                    {
                        spellHandler.Execute();
                    }

                    Caster.RemoveSpellBuffs((int)SpellIdEnum.PLOT_12966);
                    //(Caster as CharacterFighter).Character.Client.Send(new SlaveNoLongerControledMessage(this.Owner.Id, this.Id));
                }
                else if (spellUsedId == (int)SpellIdEnum.PLOTTER_12936)
                {
                    var spellHandler = SpellManager.Instance.GetSpellCastHandler(this, new Spell((int)SpellIdEnum.EXPLOSION_OF_THE_PLOTTER_19725, 1), this.Position.Cell, false);

                    spellHandler.Initialize();

                    var handlers = spellHandler.GetEffectHandlers().ToArray();

                    using (this.Fight.StartSequence(SequenceTypeEnum.SEQUENCE_SPELL))
                    {
                        spellHandler.Execute();
                    }

                    Caster.RemoveSpellBuffs((int)SpellIdEnum.PLOT_19724);
                    //(Caster as CharacterFighter).Character.Client.Send(new SlaveNoLongerControledMessage(this.Owner.Id, this.Id));
                }
            }

            Controller = null;
            Summoner.RemoveSummon(this);
            base.OnDead(killedBy, passTurn);
        }

        //Version 2.61 by Kenshin
        public override GameFightFighterInformations GetGameFightFighterInformations(WorldClient client = null)
        {
            var casterInfos = Caster.GetGameFightFighterInformations();

            if (casterInfos is GameFightCharacterInformations)
            {
                var characterInfos = casterInfos as GameFightCharacterInformations;

                return new GameFightCharacterInformations(
                    contextualId: Id,
                    disposition: GetEntityDispositionInformations(),
                    look: casterInfos.look,
                    spawnInfo: GetGameContextBasicSpawnInformation(client),
                    wave: 0,
                    stats: GetGameFightMinimalStats(),
                    previousPositions: MovementHistory.GetEntries(2).Select(x => x.Cell.Id).Select(x => (ushort)x).ToArray(),
                    name: characterInfos.name,
                    status: characterInfos.status,
                    leagueId: characterInfos.leagueId,
                    ladderPosition: characterInfos.ladderPosition,
                    hiddenInPrefight: false,
                    level: characterInfos.level,
                    alignmentInfos: characterInfos.alignmentInfos,
                    breed: characterInfos.breed,
                    sex: characterInfos.sex);
            }

            return new GameFightFighterInformations(
                contextualId: Id,
                disposition: GetEntityDispositionInformations(),
                look: casterInfos.look,
                spawnInfo: GetGameContextBasicSpawnInformation(client),
                wave: 0,
                stats: GetGameFightMinimalStats(),
                previousPositions: MovementHistory.GetEntries(2).Select(x => x.Cell.Id).Select(x => (ushort)x).ToArray());
        }

        public override FightTeamMemberInformations GetFightTeamMemberInformations() => new FightTeamMemberInformations(Id);
    }
}