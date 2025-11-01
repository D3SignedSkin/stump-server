using System.Linq;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Database.Quests;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Quests.Objectives
{
    public class EscortMonstersObjective : QuestObjective
    {
        private int m_completion;

        public EscortMonstersObjective(Character character, QuestObjectiveTemplate template, bool finished, int monsterId, int amount) : base(character, template, finished)
        {
            Monster = MonsterManager.Instance.GetTemplate(monsterId);
            Amount = amount;
        }

        public EscortMonstersObjective(Character character, QuestObjectiveTemplate template, QuestObjectiveStatus finished, int monsterId, int amount) : base(character, template, finished)
        {
            Monster = MonsterManager.Instance.GetTemplate(monsterId);
            Amount = amount;
        }

        public MonsterTemplate Monster
        {
            get;
        }

        public int Amount
        {
            get;
        }

        public int Completions
        {
            get { return ObjectiveRecord.Completion; }
            private set
            {
                m_completion = value;
                ObjectiveRecord.Completion = value;
            }
        }

        public override void EnableObjective()
        {
            Character.FightEnded += OnFightEnded;
        }

        private void OnFightEnded(Character character, CharacterFighter fighter)
        {
            if (!(fighter.Fight is FightPvM))
                return;

            if ((fighter.Fight as FightPvM).IsPvMArenaFight)
                return;

            if (Character.Fighter != null && character.Fighter.Fight.Winners != character.Fighter.Team)
                return;

            if (Monster is null)
                return;

            bool hasWatend = fighter.OpposedTeam.Fighters.Any(x => x is MonsterFighter && (x as MonsterFighter).Monster.Template.Id == Monster.Id);

            if (hasWatend)
            {
                Character.AddFollow(Monster.Id);
                Completions += 1;

                //CompleteObjective();
            }
        }

        public override void DisableObjective()
        {
            Character.FightEnded -= OnFightEnded;
        }

        public override QuestObjectiveInformations GetQuestObjectiveInformations()
        {
            return new QuestObjectiveInformationsWithCompletion((ushort)Template.Id, ObjectiveRecord.Status ? false : true, new string[0], (ushort)Completions, (ushort)Amount);
        }

        public override bool CanSee()
        {
            return true;
        }

        public override int Completion()
        {
            return Completions;
        }
    }
}