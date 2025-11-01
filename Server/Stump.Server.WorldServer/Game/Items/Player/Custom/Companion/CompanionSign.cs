using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom.Companion
{
    [ItemType(ItemTypeEnum.COMPAGNON_169)]
    public class CompanionSign : BasePlayerItem
    {
        public CompanionSign(Character owner, PlayerItemRecord record) : base(owner, record)
        { }

        public override bool OnEquipItem(bool unequip)
        {
            if (Owner.IsInFight() && Owner.Fight.State == FightState.Placement && !Owner.Team.IsFull())
            {
                if (!unequip)
                {
                    if (Owner.CreateCompanion(Owner.Team) != null)
                        Owner.Team.AddFighter(Owner.CreateCompanion(Owner.Team));
                }
                else
                {
                    Owner.Companion?.LeaveFight();
                }
            }

            Owner.Party?.UpdateMember(Owner);
            return base.OnEquipItem(unequip);
        }
    }
}
