using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Quests;
using Stump.Server.WorldServer.Handlers.Context.RolePlay;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("Quest", typeof(NpcReply), typeof(NpcReplyRecord))]
    public class StartQuestReply : NpcReply
    {
        private List<int> QuestsRepeats = new List<int> { 470, 469, 468, 467, 466, 465, 464, 463, 462, 461, 460, 459, 458, 708, 715, 940, 1617, 1679, 1843 };

        public StartQuestReply(NpcReplyRecord record) : base(record)
        { }

        public int StepId
        {
            get { return Record.GetParameter<int>(0); }
            set { Record.SetParameter(0, value); }
        }

        public string ItemsParameter
        {
            get
            {
                return Record.GetParameter<string>(1U, true);
            }
            set
            {
                Record.SetParameter(1U, value);
            }
        }

        public override bool CanExecute(Npc npc, Character character)
        {
            return base.CanExecute(npc, character) && !character.Quests.Any(x => x.Template.StepIds.Contains(StepId) && !QuestsRepeats.Contains(x.Template.Id));
        }

        public override bool Execute(Npc npc, Character character)
        {
            if (base.Execute(npc, character))
            {
                character.StartQuest(StepId);
                ContextRoleplayHandler.SendRefreshMapQuestWithout(character.Client, QuestManager.Instance.GetQuestTemplateWithStepId(StepId).Id);

                if (ItemsParameter != null)
                {
                    var parameter = ItemsParameter.Split(',');
                    var itemsToDelete = new Dictionary<BasePlayerItem, int>();

                    foreach (var itemParameter in parameter.Select(x => x.Split('_')))
                    {

                        int itemId;
                        if (!int.TryParse(itemParameter[0], out itemId))
                            return false;
                        int amount;
                        if (!int.TryParse(itemParameter[1], out amount))
                            return false;

                        var template = ItemManager.Instance.TryGetTemplate(itemId);
                        if (template == null)
                            return false;

                        var item = character.Inventory.TryGetItem(template);

                        if (item == null)
                        {
                            //Vous ne possédez pas l'objet nécessaire.
                            character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 4);
                            return false;
                        }

                        if (item.Stack < amount)
                        {
                            //Vous ne possédez pas l'objet en quantité suffisante.
                            character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 252);
                            return false;
                        }

                        itemsToDelete.Add(item, amount);
                    }

                    foreach (var itemToDelete in itemsToDelete)
                    {
                        character.Inventory.RemoveItem(itemToDelete.Key, itemToDelete.Value);
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 22, itemToDelete.Value, itemToDelete.Key.Template.Id);
                    }
                }

                return true;
            }

            return false;
        }
    }
}