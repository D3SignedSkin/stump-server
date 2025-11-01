using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Ecaflip
{
    [SpellCastHandler(SpellIdEnum.HELPING_WORD_14446)]
    public class HelpingHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public HelpingHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            if (Caster.Cell != TargetedCell)
            {
                Handlers[0].Apply();
            }
            else
            {
                Handlers[1].Apply();
            }
        }
    }

    [SpellCastHandler(SpellIdEnum.HELPING_WORD_14449)]
    public class HelpingWordHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public HelpingWordHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            Handlers[0].Apply();
            Handlers[1].Apply();
            Handlers[2].Apply();
            //Handlers[3].Apply();
            Handlers[4].Apply();
            Handlers[5].Apply();
        }
    }
}