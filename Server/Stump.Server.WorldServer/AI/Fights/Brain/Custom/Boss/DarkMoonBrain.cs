using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.AI.Fights.Actions;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Spells;
using System.Linq;
using TreeSharp;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.DARKLI_MOON_225)]
    public class MoonDarkBrain : Brain
    {
        Spell spellTotem = null;

        public MoonDarkBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
            Fighter.GetAlive += OnGetAlive;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.Stats[PlayerFields.SummonLimit].Additional = 1;

            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DARK_POWER_3276, 1), Fighter.Cell);
        }

        private void OnGetAlive(FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.DARK_POWER_3276, 1), Fighter.Cell);
        }

        public override void Play()
        {
            foreach (var spell in Fighter.Spells.Values)
            {
                if (spell.Id == (int)SpellIdEnum.DARK_MONKEY_BUSINESS_920 && Fighter.AP > 6)
                {
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.DARK_MONKEY_BUSINESS_920, 1), Fighter.Cell);
                }

                var target = Environment.GetNearestEnemy();
                var freeCell = Environment.GetFreeAdjacentCell();

                if (freeCell != null && spell.Id == (int)SpellIdEnum.AIR_TOTEM_300)
                {
                    spellTotem = new Spell((int)SpellIdEnum.AIR_TOTEM_6166, 4);
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.AIR_TOTEM_300, 2), freeCell);
                }
                else if (freeCell != null && spell.Id == (int)SpellIdEnum.FIRE_TOTEM_298)
                {
                    spellTotem = new Spell((int)SpellIdEnum.FIRE_TOTEM_6163, 4);
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.FIRE_TOTEM_298, 2), freeCell);
                }
                else if (freeCell != null && spell.Id == (int)SpellIdEnum.WATER_TOTEM_299)
                {
                    spellTotem = new Spell((int)SpellIdEnum.WATER_TOTEM_6167, 4);
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.WATER_TOTEM_299, 2), freeCell);
                }
                else if (freeCell != null && spell.Id == (int)SpellIdEnum.EARTH_TOTEM_301)
                {
                    spellTotem = new Spell((int)SpellIdEnum.EARTH_TOTEM_6165, 4);
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.EARTH_TOTEM_301, 2), freeCell);
                }

                var selector = new PrioritySelector();

                selector.AddChild(new Decorator(ctx => target == null, new DecoratorContinue(new RandomMove(Fighter))));
                selector.AddChild(new Decorator(ctx => spell == null, new DecoratorContinue(new FleeAction(Fighter))));

                if (target != null && spell != null)
                {
                    selector.AddChild(new PrioritySelector(
                        new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK, new Sequence(new SpellCastAction(Fighter, spell, target.Cell, true), new Decorator(new MoveNearTo(Fighter, target)))),
                        new Sequence(new MoveNearTo(Fighter, target), new Decorator(ctx => Fighter.CanCastSpell(spell, target.Cell) == SpellCastResult.OK, new Sequence(new SpellCastAction(Fighter, spell, target.Cell, true))))));
                }

                foreach (var action in selector.Execute(this))
                {

                }
            }
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.TOTEM_DE_LAIR_2817)]
    public class TotemAire : Brain
    {
        public TotemAire(AIFighter fighter) : base(fighter)
        {
            Fighter.GetAlive += OnGetAlive;
        }

        private void OnGetAlive(FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            Fighter.Summoner.Summoner.Stats[PlayerFields.AirResistPercent].Base = 200;

            var freeCell = Environment.GetFreeAdjacentCell();
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.AIR_TOTEM_6166, 4), freeCell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.TOTEM_DE_LEAU_2816)]
    public class TotemAgua : Brain
    {
        public TotemAgua(AIFighter fighter) : base(fighter)
        {
            Fighter.GetAlive += OnGetAlive;
        }

        private void OnGetAlive(FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            Fighter.Summoner.Summoner.Stats[PlayerFields.WaterResistPercent].Base = 200;

            var freeCell = Environment.GetFreeAdjacentCell();
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.WATER_TOTEM_6167, 4), freeCell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.TOTEM_DU_FEU_2815)]
    public class TotemFuego : Brain
    {
        public TotemFuego(AIFighter fighter) : base(fighter)
        {
            Fighter.GetAlive += OnGetAlive;
        }

        private void OnGetAlive(FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            Fighter.Summoner.Summoner.Stats[PlayerFields.FireResistPercent].Base = 200;

            var freeCell = Environment.GetFreeAdjacentCell();
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.FIRE_TOTEM_6163, 4), freeCell);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.TOTEM_DE_LA_TERRE_2818)]
    public class TotemTierra : Brain
    {
        public TotemTierra(AIFighter fighter) : base(fighter)
        {
            Fighter.GetAlive += OnGetAlive;
        }

        private void OnGetAlive(FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            Fighter.Summoner.Summoner.Stats[PlayerFields.EarthResistPercent].Base = 200;

            var freeCell = Environment.GetFreeAdjacentCell();
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.EARTH_TOTEM_6165, 4), freeCell);
        }
    }
}