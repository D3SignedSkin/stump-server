using System.Drawing;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Maps.Cells.Shapes;
using Stump.Server.WorldServer.Game.Spells;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;

namespace Stump.Server.WorldServer.Game.Fights.Triggers
{
    public class Trap : MarkTrigger
    {
        public Trap(short id, FightActor caster, Spell spell, EffectDice originEffect, Spell trapSpell, Cell centerCell, SpellShapeEnum shape, byte minSize, byte size) : base(id, caster, spell, originEffect, centerCell, new MarkShape(caster.Fight, centerCell, shape, GetMarkShape(shape), minSize, size, GetTrapColorBySpell(spell)))
        {
            TrapSpell = trapSpell;
            VisibleState = GameActionFightInvisibilityStateEnum.INVISIBLE;
        }

        public bool HasBeenTriggered
        {
            get;
            private set;
        }

        public bool WillBeTriggered
        {
            get;
            set;
        }

        public Spell TrapSpell
        {
            get;
        }

        public GameActionFightInvisibilityStateEnum VisibleState
        {
            get;
            set;
        }

        public override bool StopMovement => !WillBeTriggered;

        public override GameActionMarkTypeEnum Type => GameActionMarkTypeEnum.TRAP;

        public override TriggerType TriggerType => TriggerType.MOVE;

        public override bool DoesSeeTrigger(FightActor fighter) => VisibleState != GameActionFightInvisibilityStateEnum.INVISIBLE || fighter.IsFriendlyWith(Caster);

        public override bool DecrementDuration() => false;

        public override void Trigger(FightActor trigger, Cell triggerCell)
        {
            if (HasBeenTriggered)
                return;

            HasBeenTriggered = true;
            NotifyTriggered(trigger, TrapSpell);

            var handler = SpellManager.Instance.GetSpellCastHandler(Caster, TrapSpell, Shape.Cell, false);

            handler.MarkTrigger = this;
            handler.TriggerCell = triggerCell;
            handler.Initialize();

            foreach (var effectHandler in handler.GetEffectHandlers())
            {
                effectHandler.EffectZone = new Zone(effectHandler.Effect.ZoneShape, Shape.Size);

                if (!effectHandler.GetAffectedActors().Any() && effectHandler.IsValidTarget(trigger))
                    effectHandler.SetAffectedActors(new[] { trigger });
            }

            Remove();
            handler.Execute();

            if (TrapSpell.Id == (int)SpellIdEnum.PORTAL_5359 || TrapSpell.Id == (int)SpellIdEnum.PORTAL_14574 || TrapSpell.Id == (int)SpellIdEnum.FLEXIBLE_PORTAL_14604 && Fight.GetOneFighter(CenterCell) != null)
            {
                Fight.TriggerMarks(CenterCell, Fight.GetOneFighter(CenterCell), TriggerType.MOVE);
            }

            //Debug Sram by Kenshin v2.61.10
            //if (trigger.GetBuffs().Any(x => x.Spell.Template.Id == (int)SpellIdEnum.TOXINES_12965))
            //{
            //    Caster.CastAutoSpell(new Spell((int)SpellIdEnum.TOXINES_12975, 1), Caster.Cell);
            //}

            Fight.PortalsManager.RefreshClientsPortals();
        }

        public override GameActionMark GetHiddenGameActionMark() => new GameActionMark(Caster.Id, (sbyte)Caster.Team.Id, CastedSpell.Template.Id, (sbyte)CastedSpell.CurrentLevel, Id, (sbyte)Type, -1, new GameActionMarkedCell[0], true);

        public override GameActionMark GetGameActionMark() => new GameActionMark(Caster.Id, (sbyte)Caster.Team.Id, CastedSpell.Template.Id, (sbyte)CastedSpell.CurrentLevel, Id, (sbyte)Type, CenterCell.Id, Shape.GetGameActionMarkedCells(), true);

        public override bool CanTrigger(FightActor actor) => !HasBeenTriggered && !WillBeTriggered;

        private static Color GetTrapColorBySpell(Spell spell)
        {
            switch (spell.Id)
            {
                //Create by Kenshin - 2.61
                //Neutre
                case (int)SpellIdEnum.PARALYSING_TRAP_12910:
                    return Color.FromArgb(0, 34, 117, 28);

                //STR
                case (int)SpellIdEnum.MASS_TRAP_12920:
                case (int)SpellIdEnum.LETHAL_TRAP_12921:
                case (int)SpellIdEnum.MALEVOLENT_TRAP_12948:
                    return Color.FromArgb(255, 89, 60, 38);

                //INT
                case (int)SpellIdEnum.DRIFT_TRAP_12942:
                case (int)SpellIdEnum.TRICKY_TRAP_12906:
                case (int)SpellIdEnum.FRAGMENTATION_TRAP_12941:
                    return Color.FromArgb(230, 36, 8);

                //AGI
                case (int)SpellIdEnum.INSIDIOUS_TRAP_12918:
                case (int)SpellIdEnum.REPELLING_TRAP_12914:
                    return Color.FromArgb(53, 200, 120);

                //Chance
                case (int)SpellIdEnum.MIRY_TRAP_12916:
                case (int)SpellIdEnum.SICKRAT_TRAP_12931:
                    return Color.FromArgb(255, 41, 107, 168);

                //Verification
                case (int)SpellIdEnum.LETHAL_TRAP_80:
                    return Color.FromArgb(0, 0, 0, 0);
                case (int)SpellIdEnum.REPELLING_TRAP_73:
                    return Color.FromArgb(0, 155, 240, 237);
                case (int)SpellIdEnum.POISONED_TRAP_71:
                    return Color.FromArgb(0, 105, 28, 117);
                case (int)SpellIdEnum.TRAP_OF_SILENCE_7713:
                    return Color.FromArgb(0, 49, 45, 134);
                case (int)SpellIdEnum.PARALYSING_TRAP_69:
                    return Color.FromArgb(0, 34, 117, 28);
                case (int)SpellIdEnum.TRICKY_TRAP_65:
                case (int)SpellIdEnum.MASS_TRAP_79:
                    return Color.FromArgb(0, 90, 52, 28);
                default:
                    return Color.Brown;
            }
        }
    }
}