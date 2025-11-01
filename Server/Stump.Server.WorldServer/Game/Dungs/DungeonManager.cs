using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dungs.Rushers;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Dungs
{
    public class DungeonManager : DataManager<DungeonManager>, ISaveable
    {
        Dictionary<int, DungeonsRecord> m_dungeons;
        Dictionary<int, DungRusherRecord> m_dungeonsRushers;

        [Initialization(InitializationPass.Sixth)]
        public override void Initialize()
        {
            m_dungeons = Database.Query<DungeonsRecord>(DungeonsRelator.FetchQuery).ToDictionary(entry => entry.Id);
            m_dungeonsRushers = Database.Query<DungRusherRecord>(DungRusherRelator.FetchQuery).ToDictionary(entry => entry.Id);

            World.Instance.RegisterSaveableInstance(this);
        }

        public void CreateDataRusher(Character character, int dungId, string dungName, double fightTime)
        {
            Rusher rusher = new Rusher
            (
                id: DungRusherRecord.PopNextId(),
                dungeonId: dungId,
                dungeonName: dungName,
                ownerId: character.Id,
                ownerName: character.Name,
                ownerLevel: character.Level,
                fightTime: fightTime
            );

            if (m_dungeonsRushers == null)
                m_dungeonsRushers = new Dictionary<int, DungRusherRecord>();

            m_dungeonsRushers.Add(rusher.Id, rusher.Record);
        }

        public DungeonsRecord GetDungeonByMapId(long mapId)
        {
            return m_dungeons.Values.FirstOrDefault(x => x.Maps != null && x.Maps.Any(map => map == mapId));
        }

        public void SetDungeonMapId(Character character)
        {
            DungeonsRecord dungeonRecord = GetDungeonByMapId(character.Map.Id);

            if (dungeonRecord != null)
            {
                if (character.DungeonReturn != null && character.DungeonReturn.Count > 0)
                {
                    bool hasOwnerDungeon = character.DungeonReturn.Any(array => array != null && array.Contains(dungeonRecord.Id));

                    if (hasOwnerDungeon)
                    {
                        List<long[]> ownerListReturn = character.DungeonReturn;

                        long[] dungeon = ownerListReturn.FirstOrDefault(x => x.Contains(dungeonRecord.Id));

                        if (dungeon != null)
                        {
                            int index = ownerListReturn.IndexOf(dungeon);

                            dungeon[1] = character.Map.Id;

                            ownerListReturn[index] = dungeon;
                        }

                        character.DungeonReturn = ownerListReturn;
                    }
                    else
                    {
                        List<long[]> ownerDungeons = character.DungeonReturn;
                        ownerDungeons.Add(new long[] { dungeonRecord.Id, character.Map.Id });

                        character.DungeonReturn = ownerDungeons;
                    }
                }
                else
                {
                    List<long[]> dungeonList = new List<long[]>()
                    {
                        new long[] { dungeonRecord.Id, character.Map.Id }
                    };

                    character.DungeonReturn = dungeonList;
                }
            }
        }

        public DungRusherRecord GetRusherByOwnerAndDungeonId(int ownerId, int dungeonId)
        {
            if (m_dungeonsRushers.Values.Count() <= 0)
                return null;

            return m_dungeonsRushers.Values.FirstOrDefault(x => x.OwnerId == ownerId && x.DungeonId == dungeonId);
        }

        public void Save()
        {
            foreach (var record in m_dungeonsRushers.Values.Where(x => x.IsUpdate || x.IsNew))
            {
                if (!record.IsUpdate && record.IsNew)
                {
                    Database.Insert(record);
                    record.IsNew = false;
                }

                if (record.IsUpdate)
                {
                    Database.Update(record);
                    record.IsUpdate = false;
                }
            }
        }
    }
}