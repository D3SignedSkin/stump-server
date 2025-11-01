using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.IPC.Messages;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Items.Player;
using System;
using System.Linq;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("OgrinesToKamas", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class OgrinesToKamas : NpcReply
    {
        public OgrinesToKamas(NpcReplyRecord record) : base(record)
        { }

        public int AmountKamas
        {
            get
            {
                return this.Record.GetParameter<int>(0U, false);
            }
            set
            {
                this.Record.SetParameter<int>(0U, value);
            }
        }

        public uint AmountOgrines
        {
            get
            {
                return this.Record.GetParameter<uint>(1U, false);
            }
            set
            {
                this.Record.SetParameter<uint>(1U, value);
            }
        }

        public override bool Execute(Npc npc, Character character)
        {
            bool flag;

            if (!base.Execute(npc, character))
            {
                flag = false;
            }
            else if (character.Inventory.CanTokenBlock() == true)
            {
                //Servidor: La interacción con las ogrinas está en mantenimiento. Vuelve a intentarlo más tarde.
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 244);
                return false;
            }
            else
            {
                var ogri = character.Account.Tokens;

                if (ogri == 0)
                {
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 4);
                    flag = false;
                }
                else if (WorldServer.ServerInformation.Id == 2 && character.Account.UserGroupId <= 3) //Bloqueio de Utilização de Ogrines em Servidor BETA
                {
                    switch (character.Account.Lang)
                    {
                        case "fr":
                            character.SendServerMessage("L'utilisation des ogrines sur le serveur BETA TEST est bloquée");
                            break;
                        case "es":
                            character.SendServerMessage("El uso de ogrinas en el servidor BETA TEST está bloqueado");
                            break;
                        case "en":
                            character.SendServerMessage("The use of ogrines on the BETA TEST server is blocked");
                            break;
                        default:
                            character.SendServerMessage("A utilização de ogrines no servidor BETA TESTE está bloqueada");
                            break;
                    }
                    flag = false;
                }
                else if (ogri < AmountOgrines)
                {
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 252);
                    flag = false;
                }
                else
                {
                    var items_ = character.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED);
                    var amountogrines = AmountOgrines;

                    character.Inventory.UnStackItem(character.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED).First(x => x.Template.Id == Settings.TokenTemplateId), (int)amountogrines);
                    BasePlayerItem tokens = character.Inventory.GetItems(CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED).FirstOrDefault(x => x.Template.Id == Settings.TokenTemplateId);
                    character.Inventory.AddKamas(AmountKamas);
                    character.RefreshActor();
                    character.SaveLater();
                    flag = true;

                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 22, (int)AmountOgrines, Settings.TokenTemplateId);
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 45, AmountKamas);

                    if (tokens != null)
                    {
                        if (IPCAccessor.Instance.IsConnected)
                        {
                            character.Client.Account.Tokens = (int)tokens.Stack;
                            IPCAccessor.Instance.Send(new UpdateTokensMessage(character.Client.Account.Tokens, character.Client.Account.Id));
                        }
                    }
                }
            }
            return flag;
        }
    }
}