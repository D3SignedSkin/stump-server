using System;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Npcs;
using Stump.Server.WorldServer.Database.Npcs.Actions;
using Stump.Server.WorldServer.Database.Quests;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using System.Linq;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using System.Collections.Generic;

namespace Stump.Server.WorldServer.Game.Quests.Objectives
{
    public class BringSoulsToObjective : QuestObjective
    {
        public BringSoulsToObjective(Character character, QuestObjectiveTemplate template, bool finished, int npcId, int monsterId, int amount) : base(character, template, finished)
        {
            Npc = NpcManager.Instance.GetNpcTemplate(npcId);
            Amount = amount;
            Monster = MonsterManager.Instance.GetTemplate(monsterId);
        }

        public BringSoulsToObjective(Character character, QuestObjectiveTemplate template, QuestObjectiveStatus status, int npcId, int monsterId, int amount) : base(character, template, status)
        {
            Npc = NpcManager.Instance.GetNpcTemplate(npcId);
            Amount = amount;
            Monster = MonsterManager.Instance.GetTemplate(monsterId);
        }

        public NpcTemplate Npc
        {
            get;
            set;
        }

        public int Amount
        {
            get;
            set;
        }

        public MonsterTemplate Monster
        {
            get;
            set;
        }

        public override void EnableObjective()
        {
            Character.InteractingWith += CharacterOnInteractingWith;
        }

        public override void DisableObjective()
        {
            Character.InteractingWith -= CharacterOnInteractingWith;
        }

        private void CharacterOnInteractingWith(Character character, Npc npc, NpcActionTypeEnum actionType, NpcAction action)
        {
            if (!(action is NpcTalkAction) || npc.Template.Id != Template.Parameter0)
                return;

            var soulStone = character.Inventory.GetItems().Where(x => x.Template.TypeId == (int)ItemTypeEnum.PIERRE_D_ME_PLEINE_85);

            if (soulStone != null)
            {
                var monstersToRemove = new List<int>();

                foreach (var item in soulStone)
                {
                    var soulStoneSummonEffects = item.Effects.OfType<EffectDice>().Where(x => x is EffectDice effectDice && x.EffectId == EffectsEnum.Effect_SoulStoneSummon && !monstersToRemove.Contains(x.Value) && x.Value == Monster.Id).ToList();

                    if (soulStoneSummonEffects.Any())
                    {
                        foreach (var effect in soulStoneSummonEffects)
                        {
                            if (!monstersToRemove.Contains(effect.Value))
                            {
                                monstersToRemove.Add(effect.Value);
                                item.Effects.Remove(effect);
                            }
                        }

                        if (item.Effects.Count() == 0)
                        {
                            character.Inventory.RemoveItem(item);
                            character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 21, Amount, item.Template.Id);
                        }
                        else
                        {
                            character.Inventory.RefreshItem(item);
                        }

                        CompleteObjective();
                    }
                }
            }
        }

        public override QuestObjectiveInformations GetQuestObjectiveInformations()
        {
            return new QuestObjectiveInformations((ushort)Template.Id, ObjectiveRecord.Status ? false : true, new string[0]);
        }

        public override bool CanSee()
        {
            var soulStone = Character.Inventory.GetItems().Where(x => x.Template.TypeId == 85);

            if (soulStone != null)
            {
                foreach (var item in soulStone)
                {
                    if (item.Effects.OfType<EffectDice>().Any(x => x.EffectId == EffectsEnum.Effect_SoulStoneSummon && x.Value == Monster.Id))
                        return true;
                }
            }

            return false;
        }

        public override int Completion()
        {
            return 0;
        }
    }
}