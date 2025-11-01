using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
	[Discriminator("RemoveBehavior", typeof(NpcReply), new System.Type[]
	{
		typeof(NpcReplyRecord)
	})]
	public class RemoveBehaviorReply : NpcReply
	{
        private ItemTemplate m_itemTemplate;

        public RemoveBehaviorReply(NpcReplyRecord record) : base(record)
		{
		}

		public ushort BehaviorId
		{
			get
			{
				return Record.GetParameter<ushort>(0);
			}
			set
			{
				Record.SetParameter(0, value);
			}
		}

        public int ItemId
        {
            get
            {
                return this.Record.GetParameter<int>(1U, false);
            }
            set
            {
                this.Record.SetParameter<int>(1U, value);
            }
        }

        public ItemTemplate Item
        {
            get
            {
                ItemTemplate itemTemplate;
                if ((itemTemplate = this.m_itemTemplate) == null)
                    itemTemplate = this.m_itemTemplate = Singleton<ItemManager>.Instance.TryGetTemplate(this.ItemId);
                return itemTemplate;
            }
            set
            {
                this.m_itemTemplate = value;
                this.ItemId = value.Id;
            }
        }

        public int Amount
        {
            get
            {
                return this.Record.GetParameter<int>(2U, false);
            }
            set
            {
                this.Record.SetParameter<int>(2U, value);
            }
        }

        public int KamasParameter
        {
            get
            {
                return Record.GetParameter<int>(3, true);
            }
            set
            {
                Record.SetParameter(2, value);
            }
        }

        public override bool Execute(Npc npc, Character character)
		{
            if (character.EquippedMount != null)
            {
                if (character.EquippedMount.Behaviors.Contains((int)(MountBehaviorEnum)BehaviorId))
                {
                    if (KamasParameter != 0)
                    {
                        if (character.Kamas < KamasParameter)
                        {
                            character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 82);
                            return false;
                        }

                        character.Inventory.SubKamas(KamasParameter);
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, KamasParameter);
                    }


                    character.EquippedMount.RemoveBehavior((MountBehaviorEnum)BehaviorId);
                    character.EquippedMount.Save(MountManager.Instance.Database);
                    character.SaveLater();
                    character.UpdateLook();
                    character.EquippedMount.RefreshMount();

                    if (ItemId != 0)
                    {
                        BasePlayerItem playerItem = Singleton<ItemManager>.Instance.CreatePlayerItem(character, Item, Amount, false);

                        if (playerItem == null)
                        {
                            return false;
                        }
                        else
                        {
                            ((ItemsCollection<BasePlayerItem>)character.Inventory).AddItem(playerItem);
                            character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 21, Amount, playerItem.Template.Id);
                        }
                    }

                    return true;
                }
                else
                {

                    switch (character.Account.Lang)
                    {
                        case "fr":
                            character.OpenPopup("Vous ne pouvez pas utiliser cette potion, votre dragodinde a déjà un pilote automatique !");
                            break;
                        case "es":
                            character.OpenPopup("¡No puedes usar esta poción, tu dragopavo ya tiene piloto automático!");
                            break;
                        case "en":
                            character.OpenPopup("You can't use this potion, your dragoturkey already has auto pilot!");
                            break;
                        default:
                            character.OpenPopup("Você não pode usar esta poção, seu dragossauro não tem o efeito escolhido!");
                            break;
                    }
                    return false;
                }
            }
            else
            {
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.OpenPopup("Vous ne pouvez pas utiliser cette potion, vous n'avez pas de dragodinde équippée !");
                        break;
                    case "es":
                        character.OpenPopup("¡No puedes usar esta poción, no tienes un dragopavo equipado!");
                        break;
                    case "en":
                        character.OpenPopup("You cannot use this potion, you do not have a dragosaur equipped!");
                        break;
                    default:
                        character.OpenPopup("Você não pode usar esta poção, você não tem um dragossauro equipado!");
                        break;
                }

                return false;
            }
        }
	}
}
