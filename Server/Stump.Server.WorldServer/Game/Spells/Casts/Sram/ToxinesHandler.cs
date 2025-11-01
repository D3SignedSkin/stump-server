using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.TOXINES_12933)]
    public class ToxinesHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public ToxinesHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            //Handlers[0].Apply();

            //if (Caster.GetBuffs(x => x.Spell.Template.Id == (int)SpellIdEnum.TOXINES_12965).Any())
            //{
            //    Handlers[1].Apply();
            //}

            //if (Caster.GetBuffs(x => x.Spell.Template.Id == (int)SpellIdEnum.TOXINES_12975).Any())
            //{
            //    Handlers[2].Apply();
            //}

            //Handlers[3].Apply();
        }
    }

    [SpellCastHandler(SpellIdEnum.TOXINES_12965)]
    public class ToxinesPasiveHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public ToxinesPasiveHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            Handlers[0].Apply();
            //Handlers[1].Apply();
            Handlers[2].Apply();
        }
    }
}