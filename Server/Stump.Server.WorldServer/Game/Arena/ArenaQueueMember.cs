using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Core.Attributes;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Arena
{
    public class ArenaQueueMember
    {
        [Variable]
        public static int ArenaMargeIncreasePerMinutes = 30;

        [Variable(true, DefinableRunning = true)]
        public static bool ArenaCheckIP = true;

        public ArenaQueueMember(Character character, int mode)
        {
            Character = character;
            InQueueSince = DateTime.Now;
            character.ArenaMode = mode;
        }

        public ArenaQueueMember(ArenaParty party)
        {
            Party = party;
            InQueueSince = DateTime.Now;
        }

        public Character Character
        {
            get;
            private set;
        }

        public ArenaParty Party
        {
            get;
            private set;
        }

        public int Level
        {
            get { return Party != null ? Party.GroupLevelAverage : Character.Level > 200 ? 200 : Character.Level; }
        }

        public int ArenaRank
        {
            get { return Party != null ? Party.GroupRankAverage : Character.ArenaMode == 1 ? Character.ArenaPointsRank_1vs1 : Character.ArenaMode == 2 ? Character.ArenaPointsRank_3vs3_Solo : Character.ArenaPointsRank_3vs3_Solo; }
        }

        public int MaxMatchableRank
        {
            get { return (int)(ArenaRank + ArenaMargeIncreasePerMinutes * (DateTime.Now - InQueueSince).TotalMinutes); }
        }

        public int MinMatchableRank
        {
            get { return (int)(ArenaRank - ArenaMargeIncreasePerMinutes * (DateTime.Now - InQueueSince).TotalMinutes); }
        }

        public DateTime InQueueSince
        {
            get;
            set;
        }

        public int MembersCount
        {
            get { return Party != null ? Party.MembersCount : 1; }
        }

        public bool IsBusy()
        {
            return EnumerateCharacters().Any(x => !x.CanEnterArena(false));
        }

        public IEnumerable<Character> EnumerateCharacters()
        {
            return Party != null ? Party.Members : Enumerable.Repeat(Character, 1);
        }

        public bool IsCompatibleWith(ArenaQueueMember member, bool Is1vs1Alone = false, bool Is3vs3Alone = false)
        {
            if (member.IsBusy())
                return false;

            if (member.EnumerateCharacters().Any(x => EnumerateCharacters().Any(y => y.UserGroup.Role < DofusProtocol.Enums.RoleEnum.Moderator_Helper && y.IsGameMaster() && x.IsGameMaster())))
            {
                Console.WriteLine("Arena Block: Jogador é Staff");
                return false;
            }

            if (ArenaCheckIP && member.EnumerateCharacters().Any(x => EnumerateCharacters().Any(y => y.Client.IP == x.Client.IP)))
            {
                Console.WriteLine("Arena Block: Team contém o mesmo IP");
                return false;
            }

            if (member.EnumerateCharacters().Any(x => EnumerateCharacters().Any(y => y.Client.Account.LastHardwareId == x.Client.Account.LastHardwareId)))
            {
                Console.WriteLine("Arena Block: Team contém o mesmo MACID");
                return false;
            }

            //Koliseu 1vs1 e 3vs3 Solo
            if (Is1vs1Alone || Is3vs3Alone)
            {
                if (member.Character?.ArenaMode != Character?.ArenaMode)
                    return false;

                if (Math.Abs(member.Level - Level) > ArenaManager.ArenaMaxLevelDifference)
                    return false;

                if (Is1vs1Alone)
                {
                    int Koli1vs1 = ArenaManager.Instance.Fila(1);

                    if (Koli1vs1 > 2 && member.EnumerateCharacters().Any(x => EnumerateCharacters().Any(y => y.CharacterToSeekName == x.Account.Nickname)))
                        return false;

                    //Math.Max(member.MinMatchableRank, MinMatchableRank) <= Math.Max(member.MaxMatchableRank, MaxMatchableRank)
                }
            }

            if (!Is1vs1Alone && !Is3vs3Alone && Math.Abs(member.Party.ArenaGroupLevelSum - this.Party.ArenaGroupLevelSum) > ArenaManager.ArenaMaxLevelDifference)
            {
                Console.WriteLine("Arena Block: Team 3vs3 possui diferença de level total maior que " + ArenaManager.ArenaMaxLevelDifference + ".");
                return false;
            }

            return true;
        }
    }
}