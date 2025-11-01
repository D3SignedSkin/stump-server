using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Database.Lottery;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Stump.Server.WorldServer.Game.Lottery;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    [Discriminator("Loterie", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class LoterieSkill : CustomSkill
    {
        public LoterieSkill(int id, InteractiveCustomSkillRecord skillTemplate, InteractiveObject interactiveObject) : base(id, skillTemplate, interactiveObject)
        { }

        public override int StartExecute(Character character)
        {
            var ticket = character.Inventory.GetItems().FirstOrDefault(x => x.Template.Id == 30018);

            if (ticket != null)
            {
                character.Inventory.RemoveItem(ticket, 1);
                Start(character);
            }
            else
            {
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.OpenPopup("Vous n'avez pas de ticket de loterie. Revenez plus tard avec un billet de loterie pour pouvoir jouer.");
                        break;
                    case "es":
                        character.OpenPopup("No tienes billete de lotería. Vuelve más tarde con un billete de lotería para que puedas jugar.");
                        break;
                    case "en":
                        character.OpenPopup("You do not have a Lottery Ticket. Come back later with a lottery ticket so you can play.");
                        break;
                    default:
                        character.OpenPopup("Você não possui Ticket de Loteria. Volte mais tarde com um ticket de loteria para poder jogar.");
                        break;
                }
            }

            return base.StartExecute(character);
        }

        private void Start(Character character)
        {
            int itemSelect = Select(LotteryManager.Instance.GetItemsLottery());
            character.Inventory.AddItem(ItemManager.Instance.TryGetTemplate(itemSelect), LotteryManager.Instance.GetLotteryItemById(itemSelect).Stack);
        }

        public Random r = new Random();

        private int Select(Dictionary<int, LotteryRewards> Items)
        {
            int poolSize = 0;

            foreach (var itemList in Items)
            {
                poolSize += itemList.Value.Probability;
            }

            int randomNumber = r.Next(0, poolSize) + 1;
            int probability = 0;

            foreach (var itemList in Items)
            {
                probability += itemList.Value.Probability;

                if (randomNumber <= probability)
                {
                    return itemList.Value.ItemID;
                }
            }

            return 0;
        }
    }
}