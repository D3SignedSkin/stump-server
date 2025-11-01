using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts
{
    [SpellCastHandler((int)SpellIdEnum.ABYSSAL_DOFUS_6828)]
    public class DofusAbyssal : DefaultSpellCastHandler
    {
        public DofusAbyssal(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (Fight.TimeLine.RoundNumber == 1 && !Caster.Abyssal)
            {
                Caster.Abyssal = true;
                EffectDice dice = new EffectDice(EffectsEnum.Effect_AddMP, 1, 0, 0);
                var cells = Caster.Position.Point.GetAdjacentCells();

                foreach (var cell in cells)
                {
                    var f = Fight.GetOneFighter(Map.GetCell(cell.CellId));

                    if (f != null)
                    {
                        if (f.IsEnnemyWith(Caster)) dice = new EffectDice(EffectsEnum.Effect_AddAP_111, 1, 0, 0);
                    }
                }

                var hand = EffectManager.Instance.GetSpellEffectHandler(dice, Caster, this, Caster.Cell, false);

                hand.DefaultDispellableStatus = FightDispellableEnum.REALLY_NOT_DISPELLABLE; // tocheck
                hand.Apply();
            }

            foreach (var handler in Handlers)
            {
                handler.DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
                handler.Apply();
            }
        }
    }
}