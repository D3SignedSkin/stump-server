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
    [Discriminator("Objective", typeof(NpcReply), typeof(NpcReplyRecord))]
    public class CompleteQuestObjectiveReply : NpcReply
    {
        public CompleteQuestObjectiveReply(NpcReplyRecord record) : base(record)
        { }

        public int ObjectiveId
        {
            get { return Record.GetParameter<int>(0); }
            set { Record.SetParameter(0, value); }
        }

        public string ItemsDeleteParameter
        {
            get { return Record.GetParameter<string>(1, true); }
            set { Record.SetParameter(1, value); }
        }

        public override bool CanExecute(Npc npc, Character character)
        {
            return base.CanExecute(npc, character) && character.Quests.Any(x => x.CurrentStep.Objectives.Any(y => y.Template.Id == ObjectiveId));
        }

        public override bool Execute(Npc npc, Character character)
        {
            if (!string.IsNullOrEmpty(ItemsDeleteParameter))
            {
                if (!StartItemsDeleteParameter(character))
                    return false;
            }

            var objective = character.Quests.SelectMany(x => x.CurrentStep.Objectives).FirstOrDefault(x => x.Template.Id == ObjectiveId);

            if (objective == null || objective.Finished)
                return false;

            objective.CompleteObjective();

            return base.Execute(npc, character);
        }

        private bool StartItemsDeleteParameter(Character character)
        {
            if (string.IsNullOrEmpty(ItemsDeleteParameter))
                return false;

            var _itemsDeleteParameter = ItemsDeleteParameter.Split('_');
            var _itemsToDelete = new Dictionary<BasePlayerItem, int>();

            foreach (var itemDeleteParameter in _itemsDeleteParameter.Select(x => x.Split(',')))
            {
                int itemId;
                if (!int.TryParse(itemDeleteParameter[0], out itemId))
                    return false;

                int amount;
                if (!int.TryParse(itemDeleteParameter[1], out amount))
                    return false;

                var template = ItemManager.Instance.TryGetTemplate(itemId);

                if (template == null)
                    return false;

                var item = character.Inventory.TryGetItem(template);

                if (item == null)
                {
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 4);
                    return false;
                }

                if (item.Stack < amount)
                {
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 252);
                    return false;
                }

                _itemsToDelete.Add(item, amount);
            }

            foreach (var itemToDelete in _itemsToDelete)
            {
                character.Inventory.RemoveItem(itemToDelete.Key, itemToDelete.Value);
            }

            return true;
        }
    }
}