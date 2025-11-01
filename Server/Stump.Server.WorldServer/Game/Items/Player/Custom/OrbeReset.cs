using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    [ItemId(ItemIdEnum.PAQUET_ORBE_RECONSTITUANT_10564)]
    public class OrbeReset : BasePlayerItem
    {
        public OrbeReset(Character owner, PlayerItemRecord record)//orbe are disabled in 2.46 above
            : base(owner, record)
        {
        }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {

            Owner.ResetStats(true);//false are per menu!
            return 1;
        }
    }
}