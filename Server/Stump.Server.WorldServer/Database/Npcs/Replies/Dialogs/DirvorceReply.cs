using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Npcs;
using Stump.Server.WorldServer.Database.Npcs.Replies;
using System;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using System.Drawing;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer;
using NLog;

namespace Database.Npcs.Replies.Mariage
    {
        [Discriminator("Divorce", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
        class Divorce_Reply : NpcReply
        {
        //private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public Divorce_Reply(NpcReplyRecord record) : base(record)
            {
            }
            public override bool Execute(Npc npc, Character character)
            {
                if (character.CurrentSpouse != 0)
                {
                    Character spouse = World.Instance.GetCharacter(character.CurrentSpouse);
                // character.Spouse.StopFollowSpouse(spouse);
            


                var spouseoff = CharacterManager.Instance.GetCharacterById(character.CurrentSpouse);
                character.CurrentSpouse = 0;
                character.Client.Send(new Stump.DofusProtocol.Messages.SpouseStatusMessage(false));
                if (spouse != null)//spouse on
                {
                    if (spouse.CurrentSpouse == character.Id)//spouse on
                    {

                        switch (spouse.Account.Lang)
                        {
                            case "fr":
                                spouse.SendServerMessage(string.Format("Vous venez de vous faire larguer par {0} !", character.Name), Color.Red);
                                break;
                            case "es":
                                spouse.SendServerMessage(string.Format("¡Usted acaba de ser abandonado por {0} !", character.Name), Color.Red);
                                break;
                            case "en":
                                spouse.SendServerMessage(string.Format("You have just been dumped by {0} !", character.Name), Color.Red);
                                break;
                            default:
                                spouse.SendServerMessage(string.Format("Você acabou de ser largado por {0} !", character.Name), Color.Red);
                                break;
                        }
                        spouse.CurrentSpouse = 0;
                        spouse.Client.Send(new Stump.DofusProtocol.Messages.SpouseStatusMessage(false));
                       
        //logger.Info("save from in queue {0}", "Divorce_Reply1");
                        spouse.SaveLater();
                    }//else wtf
                }
                else if (spouseoff.SpouseID == character.Id)
                {
                    spouseoff.SpouseID = 0;

                    try
                    {
                        WorldServer.Instance.DBAccessor.Database.Update(spouseoff);
                        Console.WriteLine("Update successful. Spouse account has been successfully updated.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during database update for spouse account: {ex.Message}. The spouse account could not be updated.");
                    }
                }//else wtf


                //logger.Info("save from in queue {0}", "Divorce_Reply2");

                character.SaveLater();
               
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Vous venez de divorcer !", Color.Red);
                        break;
                    case "es":
                        character.SendServerMessage("¡Usted acaba de divorciarse !", Color.Red);
                        break;
                    case "en":
                        character.SendServerMessage("You have just divorced !", Color.Red);
                        break;
                    default:
                        character.SendServerMessage("Você acaba de divorciar-se !", Color.Red);
                        break;
                }
            }
                else                   
            switch (character.Account.Lang)
            {
                case "fr":
                        character.SendServerMessage("Vous ne pouvez pas divorcer, vous n'êtes pas marié !", Color.Red);
                        break;
                case "es":
                        character.SendServerMessage("¡Usted no puedes divorciarte, usted no estás casado !", Color.Red);
                        break;
                case "en":
                        character.SendServerMessage("You can not divorce, you are not married !", Color.Red);
                        break;
                default:
                        character.SendServerMessage("Você não pode se divorciar, você não é casado !", Color.Red);
                        break;
            }
            return true;
            }
        }
    }
