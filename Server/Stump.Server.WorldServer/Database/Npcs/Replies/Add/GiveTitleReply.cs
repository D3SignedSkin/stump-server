using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Npcs;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using System;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("GiveTitle", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class GiveTitleReply : NpcReply
    {
        public GiveTitleReply(NpcReplyRecord record)
            : base(record)
        {
        }
        public short titleid
        {
            get
            {
                return base.Record.GetParameter<short>(0u, false);
            }
            set
            {
                base.Record.SetParameter<short>(0u, value);
            }
        }

        public int KamasParameter
        {
            get
            {
                return Record.GetParameter<int>(1, true);
            }
            set
            {
                Record.SetParameter(1, value);
            }
        }

        public override bool Execute(Npc npc, Character character)
        {
            bool result;

            if (!base.Execute(npc, character))
            {
                result = false;
            }
            else
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

                if (character.HasTitle((ushort)titleid))
                {
                    switch (character.Account.Lang)
                    {
                        case "fr":
                            character.SendServerMessage("Vous avez déjà ce titre dans votre personnage !");
                            break;
                        case "es":
                            character.SendServerMessage("Ya tienes este título en tu personaje!");
                            break;
                        case "en":
                            character.SendServerMessage("You already have this title in your character!");
                            break;
                        default:
                            character.SendServerMessage("Você já possui esse titulo em seu personagem!");
                            break;
                    }

                    return false;
                }

                character.AddTitle((ushort)titleid);
                character.SelectTitle((ushort)titleid);
                character.SaveLater();

                result = true;
            }

            return result;
        }
    }
}
