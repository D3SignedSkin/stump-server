using Stump.Server.WorldServer.Database.Monsters;
using System;

namespace Stump.Server.WorldServer.Game.Dungs.Rushers
{
    public class Rusher
    {
        public int Id 
        {
            get { return Record.Id; }
            set { Record.Id = value; }
        }

        public int DungeonId 
        {
            get { return Record.DungeonId; }
            set { Record.DungeonId = value; }
        }

        public string DungeonName 
        {
            get { return Record.DungeonName; }
            set { Record.DungeonName = value; }
        }

        public int OwnerId 
        {
            get { return Record.OwnerId; }
            set { Record.OwnerId = value; }
        }

        public string OwnerName 
        {
            get { return Record.OwnerName; }
            set { Record.OwnerName = value; }
        }

        public int OwnerLevel 
        {
            get { return Record.OwnerLevel; }
            set { Record.OwnerLevel = value; }
        }

        public double FightTime 
        {
            get { return Record.FightTime; }
            set { Record.FightTime = value; }
        }

        public bool IsNew
        {
            get { return Record.IsNew; }
            set { Record.IsNew = value; }
        }

        public bool IsUpdate
        {
            get { return Record.IsUpdate; }
            set { Record.IsUpdate = value; }
        }

        public DungRusherRecord Record
        {
            get;
            set;
        }

        public Rusher(int id, int dungeonId, string dungeonName, int ownerId, string ownerName, int ownerLevel, double fightTime)
        {
            Record = new DungRusherRecord();

            Id = id;
            DungeonId = dungeonId;
            DungeonName = dungeonName;
            OwnerId = ownerId;
            OwnerName = ownerName;
            OwnerLevel = ownerLevel;
            FightTime = fightTime;
            IsNew = true;
        }
    }
}