using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Sadida
{
    [SpellCastHandler(SpellIdEnum.LEAFY_TREE_13560)]
    public class LeafyTreeHandler : DefaultSpellCastHandler
    {
        public LeafyTreeHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            //Effect_Summon
            Handlers[0].Apply();

            //Effect_TriggerBuff
            Handlers[1].Apply();
        }
    }
}
