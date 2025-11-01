//using Stump.Server.BaseServer.Database;
//using Stump.Server.WorldServer.Database.Interactives;
//using Stump.Server.WorldServer.Database.Lottery;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
//using Stump.Server.WorldServer.Game.Items;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using Stump.Server.WorldServer.Game.Lottery;

//namespace Stump.Server.WorldServer.Game.Interactives.Skills
//{
//    [Discriminator("Loterie", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
//    public class LoterieSkill : CustomSkill
//    {
//        public LoterieSkill(int id, InteractiveCustomSkillRecord skillTemplate, InteractiveObject interactiveObject)
//            : base(id, skillTemplate, interactiveObject)
//        {
//        }

//        public override int StartExecute(Character character)
//        {
//            if (character.Vip == false || character.WorldAccount.LastLoterieDateVip != null && character.WorldAccount.LastLoterieDateVip.Date == DateTime.Now.Date)
//            {
//                if (character.WorldAccount.LastLoterieDate != null && character.WorldAccount.LastLoterieDate.Date == DateTime.Now.Date)
//                {
//                    var ticket = character.Inventory.GetItems().FirstOrDefault(x => x.Template.Id == 17745);
//                    if (ticket != null)
//                    {
//                        character.Inventory.RemoveItem(ticket, 1);
//                        Start(character);
//                    }
//                    else
//                        switch (character.Account.Lang)
//                        {
//                            case "fr":
//                                character.OpenPopup("La loterie n'est gratuite qu'une fois par jour, si vous souhaitez recommencer, attendez demain ou gagnez un ticket de loterie lors d'événements.");
//                                break;
//                            case "es":
//                                character.OpenPopup("La lotería solo es gratuita una vez al día, si desea comenzar de nuevo, espere hasta mañana o gane un boleto de lotería en los eventos.");
//                                break;
//                            case "en":
//                                character.OpenPopup("The lottery is only free once a day, if you want to start over, wait until tomorrow or win a lottery ticket at events.");
//                                break;
//                            default:
//                                character.OpenPopup("A loteria é gratuita apenas uma vez por dia, se você quiser começar de novo, espere até amanhã ou ganhe um bilhete de loteria em eventos.");
//                                break;
//                        }
//                }
//                else
//                {
//                    Start(character);
//                    character.WorldAccount.LastLoterieDate = DateTime.Now;
//                }
//            }
//            else
//            {
//                Start(character);
//                character.WorldAccount.LastLoterieDateVip = DateTime.Now;
//            }

//            return base.StartExecute(character);
//        }

//        private void Start(Character character)
//        {
//            int itemSelect = Select(LotteryManager.Instance.GetItemsLottery());
//            character.Inventory.AddItem(ItemManager.Instance.TryGetTemplate(itemSelect), LotteryManager.Instance.GetLotteryItemById(itemSelect).Stack);
//            switch (character.Account.Lang)
//            {
//                case "fr":
//                    character.SendServerMessage("Tu as gagné: " + LotteryManager.Instance.GetLotteryItemById(itemSelect).Comment + "!", Color.Green);
//                    break;
//                case "es":
//                    character.SendServerMessage("Ganaste: " + LotteryManager.Instance.GetLotteryItemById(itemSelect).Comment + "!", Color.Green);
//                    break;
//                case "en":
//                    character.SendServerMessage("You won: " + LotteryManager.Instance.GetLotteryItemById(itemSelect).Comment + "!", Color.Green);
//                    break;
//                default:
//                    character.SendServerMessage("Você ganhou: " + LotteryManager.Instance.GetLotteryItemById(itemSelect).Comment + "!", Color.Green);
//                    break;
//            }
//        }

//        public Random r = new Random();

//        private int Select(Dictionary<int, LotteryRewards> Items)
//        {
//            int poolSize = 0;
//            foreach (var itemList in Items)
//            {
//                poolSize += itemList.Value.Probability;
//            }
//            int randomNumber = r.Next(0, poolSize) + 1;

//            int probability = 0;
//            foreach (var itemList in Items)
//            {
//                probability += itemList.Value.Probability;
//                if (randomNumber <= probability)
//                {
//                    return itemList.Value.ItemID;
//                }
//            }
//            return 0;
//        }
//    }
//}
