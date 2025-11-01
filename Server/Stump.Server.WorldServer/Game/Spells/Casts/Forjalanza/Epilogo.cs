using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Handler;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.game.spells.Casts.Forjalanza
{
    [SpellCastHandler(23801)]
    public class Epilogo : DefaultSpellCastHandler
    {
        public Epilogo(SpellCastInformations cast)
            : base(cast)
        {
        }

        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            var target = Fight.GetOneFighter<SummonedFighter>(TargetedCell);

            if (target != null && ((SummonedMonster)target).Monster.Template.Id == 7139)
            {
                Handlers[0].Apply(); // wait
                Handlers[1].SetAffectedActors(new[] { target});

                Handlers[1].Apply();

               

                /*foreach (var handler in Handlers.OrderBy(x => x.Priority))
                {
                    handler.Apply();
                }*/
                target.Die();
                if (Caster.HasState(3472) || Caster.HasState(3361))
                {
                    var buffs = Caster.GetBuffs().Where(x => x.Dice.Id == 3472 || x.Dice.Id == 3361);
                    foreach (var buff in buffs)
                    {
                        Caster.RemoveBuff(buff);
                    }
                    var spell = new Spell(24387, 1);
                    Caster.CastAutoSpell(spell, Caster.Cell);
                    //Caster.RemoveBuff(new StateBuff());
                }
            }

            // start without action BeforeDead
        }
    }
}