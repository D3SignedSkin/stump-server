using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Items;
using System;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("AddFollow", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class AddFollowReply : NpcReply
    {
        public AddFollowReply(NpcReplyRecord record) : base(record)
        { }

        public int FollowId
        {
            get { return this.Record.GetParameter<int>(0U, false); }
            set { this.Record.SetParameter(0U, value); }
        }

        public override bool Execute(Npc npc, Character character)
        {
            bool flag;

            if (!base.Execute(npc, character))
            {
                flag = false;
            }
            else
            {
                ItemTemplate _itemTemplate = ItemManager.Instance.TryGetTemplate(FollowId);
                character.Inventory.MoveItem(character.Inventory.AddItem(_itemTemplate, _itemTemplate.Effects), CharacterInventoryPositionEnum.INVENTORY_POSITION_FOLLOWER);
                character.RefreshActor();
                flag = true;
            }

            return flag;
        }
    }
}