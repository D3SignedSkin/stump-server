using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Misc;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    [ItemId(ItemIdEnum.REPUESTRE_KREKER_30348)] //Dung Manager
    public class DungManager : BasePlayerItem
    {
        public DungManager(Character owner, PlayerItemRecord record) : base(owner, record)
        {
        }
        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {

            var dialog = new DonjonZaapDialog(Owner);
            dialog.Open();

            if (Owner.Vip == true)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}