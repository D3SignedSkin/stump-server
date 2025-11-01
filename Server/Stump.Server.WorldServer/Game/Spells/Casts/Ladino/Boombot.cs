using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Roublard
{
    [SpellCastHandler(SpellIdEnum.BOOMBOT_430)]
    [SpellCastHandler(SpellIdEnum.BOOMBOT_13429)]
    public class RoublabotCastHandler : DefaultSpellCastHandler
    {
        public RoublabotCastHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            base.Execute();

            var slave = Fight.GetOneFighter<SummonedFighter>(x => x.Cell == TargetedCell && x.Controller == Caster);

            if (slave == null)
                return;

            if (Spell.Template.Id == (int)SpellIdEnum.BOOMBOT_430)
                slave.CastAutoSpell(new Spell((int)SpellIdEnum.INITIALISATION_2821, 1), TargetedCell);
            else
                slave.CastAutoSpell(new Spell((int)SpellIdEnum.INITIALISATION_13454, 1), TargetedCell);
        }
    }
}