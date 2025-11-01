using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using Stump.Core.Cache;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Npcs;
using Stump.Server.WorldServer.Database.Npcs.Actions;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Pathfinding;
using Stump.Server.WorldServer.Game.Quests;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs
{
    public sealed class Npc : RolePlayActor, IInteractNpc, IContextDependant, IAutoMovedEntity
    {
        private readonly List<NpcAction> m_actions = new List<NpcAction>();

        List<Tuple<short, int>> DoppleQuests = new List<Tuple<short, int>>
        {
            Tuple.Create((short) 979,168),
            Tuple.Create((short) 982,165),
            Tuple.Create((short) 983,167),
            Tuple.Create((short) 984,161),
            Tuple.Create((short) 985,455),
            Tuple.Create((short) 986,455),
            Tuple.Create((short) 987,166),
            Tuple.Create((short) 988,169),
            Tuple.Create((short) 989,2691),
            Tuple.Create((short) 990,163),
            Tuple.Create((short) 991,164),
            Tuple.Create((short) 992,160),
            Tuple.Create((short) 995,995),
            Tuple.Create((short) 1324,3111),
            Tuple.Create((short) 1330,3132),
            Tuple.Create((short) 1557,3286),
            Tuple.Create((short) 2236,3976),
            Tuple.Create((short) 2314,4290),
            Tuple.Create((short) 2483,4777)
        };

        public Npc(int id, NpcTemplate template, ObjectPosition position, ActorLook look)
        {
            Id = id;
            Template = template;
            Position = position;
            Look = look;

            m_gameContextActorInformations = new ObjectValidator<GameContextActorInformations>(BuildGameContextActorInformations);
            m_actions.AddRange(Template.Actions);
        }

        public int TemplateId => Template.Id;

        public List<NpcAction> Actions => m_actions;

        public event Action<Npc, NpcActionTypeEnum, NpcAction, Character> Interacted;

        public override string ToString() => string.Format("{0} ({1}) [{2}]", Template.Name, Id, TemplateId);

        public Npc(int id, NpcSpawn spawn) : this(id, spawn.Template, spawn.GetPosition(), spawn.Look)
        {
            Spawn = spawn;
        }

        public NpcTemplate Template
        {
            get;
        }

        public NpcSpawn Spawn
        {
            get;
        }

        public override int Id
        {
            get;
            protected set;
        }

        public override ActorLook Look
        {
            get;
            set;
        }

        public override bool CanBeSee(WorldObject byObj)
        {
            Character character = byObj as Character;

            //NPC is under development.
            if (this.Spawn != null && this.Spawn.IsPending)
            { 
                return base.CanBeSee(byObj) && character != null && character.IsGameMaster() ? true : false;
            }

            return base.CanBeSee(byObj);
        }

        private void OnInteracted(NpcActionTypeEnum actionType, NpcAction action, Character character)
        {
            character.OnInteractingWith(this, actionType, action);

            var handler = Interacted;

            if (handler != null)
                handler(this, actionType, action, character);
        }

        public void Refresh()
        {
            m_gameContextActorInformations.Invalidate();

            if (Map != null)
                Map.Refresh(this);
        }

        public void InteractWith(NpcActionTypeEnum actionType, Character dialoguer)
        {
            if (!CanInteractWith(actionType, dialoguer))
                return;

            var action = Actions.Where(entry => entry.ActionType.Contains(actionType) && entry.CanExecute(this, dialoguer)).OrderBy(x => x.Priority).First();

            action.Execute(this, dialoguer);
            OnInteracted(actionType, action, dialoguer);
        }

        public bool CanInteractWith(NpcActionTypeEnum action, Character dialoguer)
        {
            if (dialoguer.Map != Position.Map || dialoguer.IsFighting() || dialoguer.IsInRequest() || dialoguer.IsGhost())
                return false;

            if (dialoguer.IsBusy())
                dialoguer.Dialog.Close();

            return Actions.Count > 0 && Actions.Any(entry => entry.ActionType.Contains(action) && entry.CanExecute(this, dialoguer));
        }

        public void SpeakWith(Character dialoguer)
        {
            if (!CanInteractWith(NpcActionTypeEnum.ACTION_TALK, dialoguer))
                return;

            InteractWith(NpcActionTypeEnum.ACTION_TALK, dialoguer);
        }

        #region >> Movement
        public DateTime NextMoveDate
        {
            get;
            set;
        }

        public DateTime LastMoveDate
        {
            get;
            private set;
        }

        public override bool StartMove(Path movementPath)
        {
            if (this.Spawn is null)
                return false;

            if (!this.Spawn.IsMovable)
                return false;

            if (!CanMove() || movementPath.IsEmpty())
                return false;

            Position = movementPath.EndPathPosition;
            var keys = movementPath.GetServerPathKeys();

            Map.ForEach(entry => ContextHandler.SendGameMapMovementMessage(entry.Client, keys, this));

            StopMove();
            LastMoveDate = DateTime.Now;

            return true;
        }
        #endregion Movement

        #region >> GameContextActorInformations
        private readonly ObjectValidator<GameContextActorInformations> m_gameContextActorInformations;

        private GameContextActorInformations BuildGameContextActorInformations()
        { 
            return new GameRolePlayNpcInformations(Id,
                                                   GetEntityDispositionInformations(),
                                                   Look.GetEntityLook(),
                                                   (ushort)Template.Id,
                                                   Template.Gender != 0,
                                                   (ushort)Template.SpecialArtworkId);
        }

        public override GameContextActorInformations GetGameContextActorInformations(Character character)
        {          
            if (this.Template.Id == 606 || this.Template.Id == 2188 || this.Template.Id == 2185) //NPC de Procurados de Astrub/Justiceiros/Amakna
            {
                List<BasePlayerItem> items = character.Inventory.GetItems(x => x.Position == CharacterInventoryPositionEnum.INVENTORY_POSITION_FOLLOWER).ToList();

                foreach (var item in items)
                {
                    if (item.Effects.Any(effect => effect.EffectId == EffectsEnum.Effect_RateXP || effect.EffectId == EffectsEnum.Effect_RateDrop || effect.EffectId == EffectsEnum.Effect_RateKamas))
                        continue;

                    var watend = WatendManager.Instance.GetWantendByItemId(item.Template.Id);

                    if (watend is null)
                        break;

                    if (character.Quests.Any(x => !x.Finished && x.CurrentStep.Objectives.Any(y => !y.ObjectiveRecord.Status && y.CanSee() && y.Template.Parameter0 == watend.MonsterId)))
                    {
                        var characterQuest = character.Quests.Where(x => !x.Finished && x.CurrentStep.Objectives.Any(y => !y.ObjectiveRecord.Status && y.CanSee() && y.Template.Parameter0 == watend.MonsterId));

                        foreach (var quest in characterQuest)
                        {
                            return new GameRolePlayNpcWithQuestInformations(Id, GetEntityDispositionInformations(), Look.GetEntityLook(), (ushort)Template.Id, Template.Gender != 0, (ushort)Template.SpecialArtworkId, new GameRolePlayNpcQuestFlag(new ushort[] { quest.Id }, new ushort[0]));
                        }
                    }
                }
            }
            else
            {
                #region >> Default

                if (character.Quests.Any(x => !x.Finished && x.CurrentStep.Objectives.Any(y => !y.ObjectiveRecord.Status && y.CanSee() && y.Template.Parameter0 == Template.Id)))
                {
                    foreach (var test in character.Quests.Where(x => !x.Finished && x.CurrentStep.Objectives.Any(y => y.Template.Parameter0 == Template.Id)))
                    {
                        return new GameRolePlayNpcWithQuestInformations(Id, GetEntityDispositionInformations(), Look.GetEntityLook(), (ushort)Template.Id, Template.Gender != 0, (ushort)Template.SpecialArtworkId, new GameRolePlayNpcQuestFlag(new ushort[] { test.Id }, new ushort[0]));
                    }

                    return BuildGameContextActorInformations();
                }
                else if (GiveQuestCanActive(character).Count() > 0)
                {
                    return new GameRolePlayNpcWithQuestInformations(Id, GetEntityDispositionInformations(), Look.GetEntityLook(), (ushort)Template.Id, Template.Gender != 0, (ushort)Template.SpecialArtworkId, new GameRolePlayNpcQuestFlag(new ushort[0], GiveQuestCanActive(character).ToArray()));
                }

                #endregion
            }

            return BuildGameContextActorInformations();
        }

        private IEnumerable<ushort> GiveQuestCanActive(Character character)
        {
            List<NpcActionRecord> actionsTalk = NpcManager.Instance.GetNpcActionsRecords(Template.Id).Where(x => x.Type == "Talk").ToList();

            for (int i = 0; i < actionsTalk.Count; i++)
            {
                var replys = NpcManager.Instance.GetMessageRepliesRecords(actionsTalk[i].NpcId);

                foreach (var reply in replys.Where(x => x.Type == "Quest").Select(x => short.Parse(x.Parameter0)))
                {
                    if (DoppleQuests.Any(x => x.Item1 == reply) && character.Quests.Any(x => x.Template.StepIds.Contains(reply) && x.Finished))
                    {
                        int QuestDoppleId = DoppleQuests.FirstOrDefault(tuple => tuple.Item1 == reply)?.Item2 ?? 0;
                        bool result = CheckDoppleTime(character, QuestDoppleId);

                        if (result)
                            yield return (ushort)QuestManager.Instance.GetQuestTemplateWithStepId(reply).Id;
                    }
                    else
                    {
                        if (!character.Quests.Any(x => x.Template.StepIds.Contains(reply)))
                        {
                            yield return (ushort)QuestManager.Instance.GetQuestTemplateWithStepId(reply).Id;
                        }
                    }
                }
            }
        }

        private bool CheckDoppleTime(Character character, int DoppleId)
        {
            if (DoppleId == 995)
                return true;

            var foundDopeul = character.DoppleCollection.Dopeul.FirstOrDefault(dopeul => dopeul.DopeulId == DoppleId);

            if (foundDopeul == null)
                return true;

            return foundDopeul.Time <= DateTime.Now;
        }
        #endregion
    }
}