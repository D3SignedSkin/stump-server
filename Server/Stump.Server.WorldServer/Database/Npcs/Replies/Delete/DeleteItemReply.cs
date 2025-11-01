using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("DeleteItems", typeof(NpcReply), typeof(NpcReplyRecord))]
    public class DeleteItemReply : NpcReply
    {
        public DeleteItemReply()
        {
            Record.Type = "DeleteItems";
        }

        public DeleteItemReply(NpcReplyRecord record) : base(record)
        { }

        /// <summary>
        /// Parameter 0
        /// </summary>
        public string ItemsDeleteParameter
        {
            get
            {
                return Record.GetParameter<string>(0, true);
            }
            set
            {
                Record.SetParameter(0, value);
            }
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
                StartItemsDeleteParameter(character);

                flag = true;
            }

            return flag;
        }

        public void StartItemsDeleteParameter(Character character)
        {
            bool flag;
            bool isWatend = false;

            #region // ---------------- Delete Item By: Kenshin ------------------//
            if (ItemsDeleteParameter != null)
            {
                var parameter = ItemsDeleteParameter.Split(',');
                var itemsToDelete = new Dictionary<BasePlayerItem, int>();

                foreach (var itemDeleteParameter in parameter.Select(x => x.Split('_')))
                {

                    int itemId;
                    if (!int.TryParse(itemDeleteParameter[0], out itemId))
                        flag = false;

                    int amount;
                    if (!int.TryParse(itemDeleteParameter[1], out amount))
                        flag = false;

                    var template = ItemManager.Instance.TryGetTemplate(itemId);

                    if (template == null)
                        flag = false;

                    var item = character.Inventory.TryGetItem(template);

                    if (item == null)
                    {
                        //Vous ne possédez pas l'objet nécessaire.
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 4);
                        flag = false;
                    }

                    if (item.Template.Type.ItemType == ItemTypeEnum.PERSONNAGE_SUIVEUR_32)
                        isWatend = true;

                    if (item.Stack < amount)
                    {
                        //Vous ne possédez pas l'objet en quantité suffisante.
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 252);
                        flag = false;
                    }

                    itemsToDelete.Add(item, amount);
                }

                foreach (var itemToDelete in itemsToDelete)
                {
                    character.Inventory.RemoveItem(itemToDelete.Key, itemToDelete.Value);
                }

                if (isWatend)
                {
                    character.RefreshActor();
                }

                flag = true;
            }
            #endregion
        }
    }
}