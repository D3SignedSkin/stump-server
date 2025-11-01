using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using System;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("AddKamas", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class AddKamasReply : NpcReply
    {
        public AddKamasReply(NpcReplyRecord record)
                : base(record)
        {
        }

        public int Amount
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

        public int MaxAmount
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

        public override bool Execute(Npc npc, Character character)
        {
            bool flag;

            if (!base.Execute(npc, character))
            {
                flag = false;
            }
            else if (character.Inventory.Kamas > MaxAmount || character.Bank.Kamas > MaxAmount)
            {
                #region MSG
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Vous avez déjà assez de kamas !", System.Drawing.Color.Red);
                        break;
                    case "es":
                        character.SendServerMessage("Ya tienes suficientes kamas!", System.Drawing.Color.Red);
                        break;
                    case "en":
                        character.SendServerMessage("You already have enough kamas!", System.Drawing.Color.Red);
                        break;
                    default:
                        character.SendServerMessage("Você já possui kamas suficientes !", System.Drawing.Color.Red);
                        break;
                }
                #endregion
                flag = false;
            }
            else if (ReplyId == 34213)
            {
                #region NPC Kenshin Add Kamas MSG
                if (character.Account.UserGroupId >= 4 && character.Kamas <= 100000000)
                {
                    character.Inventory.AddKamas((int)Amount);
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 45, Amount);
                    flag = true;
                }
                else
                {
                    #region MSG
                    switch (character.Account.Lang)
                    {
                        case "fr":
                            character.SendServerMessage("Vous avez déjà assez de kamas !", System.Drawing.Color.Red);
                            break;
                        case "es":
                            character.SendServerMessage("Ya tienes suficientes kamas!", System.Drawing.Color.Red);
                            break;
                        case "en":
                            character.SendServerMessage("You already have enough kamas!", System.Drawing.Color.Red);
                            break;
                        default:
                            character.SendServerMessage("Você já possui kamas suficientes !", System.Drawing.Color.Red);
                            break;
                    }
                    #endregion
                    flag = false;
                }
                #endregion
            }
            else
            {
                character.Inventory.AddKamas((int)Amount);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 45, Amount);
                flag = true;
            }
            return flag;
        }
    }
}

