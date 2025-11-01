using Stump.Server.BaseServer.Database;
using System;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Server.BaseServer.IPC.Messages;

namespace Stump.Server.WorldServer.Database.Npcs.Replies.AlignReplies
{
    [Discriminator("Mercenary", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class MercenaryReply : NpcReply
    {
        public MercenaryReply(NpcReplyRecord record) : base(record)
        {
        }

        public override bool Execute(Npc npc, Character character)
        {
            int Cost = 100;
            var item = character.Inventory.TryGetItem(ItemManager.Instance.TryGetTemplate(Settings.TokenTemplateId));
            int TokensAmount = character.Client.Account.Tokens;

            if (character.Inventory.CanTokenBlock())
            {
                //Servidor: La interacción con las ogrinas está en mantenimiento. Vuelve a intentarlo más tarde.
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 244);
                return false;
            }
            else if (item != null)
            {
                if (item.Stack >= Cost && TokensAmount >= Cost)
                {
                    if (character.Inventory.RemoveTokenItem(Cost, "Mercenary Wing"))
                    {
                        character.ChangeAlignementSide(AlignmentSideEnum.ALIGNMENT_MERCENARY);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    character.SendServerMessage("No tienes los suficientes puntos boutique! ");
                }
            }
            else
            {
                character.SendServerMessage("No tienes los suficientes puntos boutique!");
            }
            return true;
        }
    }
}
