//using Stump.Core.Threading;
//using Stump.Server.BaseServer.Database;
//using Stump.Server.BaseServer.Initialization;
//using Stump.Server.WorldServer.Database.Monsters;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
//using Stump.Server.WorldServer.Game.Maps;
//using Stump.Server.WorldServer.Game.Maps.Cells;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;

//namespace Stump.Server.WorldServer.Game.Misc
//{
//    public class MonsterNani
//    {
//        public MonsterNani(MonsterNaniRecord record)
//        {
//            Record = record;
//            Spawned = false;

//        }

//        public MonsterNaniRecord Record
//        {
//            get;
//            set;
//        }
//        public bool Spawned
//        {
//            get;
//            set;
//        }

//        public string ZoneName
//        {
//            get;
//            set;
//        }
//    }

//    public class MonsterNaniManager : DataManager<MonsterNaniManager>
//    {
//        public static int NaniUpdateInterval = 14400000; //Refresh de 4 em 4 horas
//        public static int NaniMatchmakingInterval = 900; //900 INTERVALE EN SECONDES
//        //private List<int> MapIdLock;
//        private List<MonsterNani> monsterNaniRecords = new List<MonsterNani>();
//        readonly SelfRunningTaskPool m_NaniTaskPool = new SelfRunningTaskPool(NaniUpdateInterval, "MonstersNani");

//        [Initialization(InitializationPass.Fifth)]
//        public override void Initialize()
//        {
//            monsterNaniRecords = Database.Query<MonsterNaniRecord>(MonsterNaniRelator.FetchQuery).Select(x => new MonsterNani(x)).ToList();
//            WorldServer.Instance.IOTaskPool.CallDelayed(60000, UsualCHeck);
//            WorldServer.Instance.IOTaskPool.CallPeriodically(NaniUpdateInterval * 1000, UsualCHeck);
//            m_NaniTaskPool.Start();
//        }

//        public void UsualCHeck()
//        {
//            CheckMonstersSpawns();
//        }

//        public void CheckMonstersSpawns(bool Silent = false)
//        {
//            foreach (var nani in monsterNaniRecords.OrderBy(x => x.Record.MonsterGradeId))
//            {
                
//                var monster = MonsterManager.Instance.GetMonsterGrade(nani.Record.MonsterGradeId);

//                if (monster == null)
//                    continue;

//                var map = PickRandomMapInSUbArea(nani.Record.SubAreas);
//                //MapIdLock.Add(map.Id);

//                if (nani.Spawned && monster.Template.Id == 2819)
//                {
//                    if (!Silent)
//                        World.Instance.SendAnnounceLang(
//                        "<b>" + monster.Template.Name + " - Level: " + monster.Level + "</b> está disponível na área : <b>" + nani.ZoneName + "</b>.",
//                        "<b>" + monster.Template.Name + " - Level: " + monster.Level + "</b> is available in the area : <b>" + nani.ZoneName + "</b>.",
//                        "<b>" + monster.Template.Name + " - Level: " + monster.Level + "</b> esta disponible en la zona : <b>" + nani.ZoneName + "</b>.",
//                        "<b>" + monster.Template.Name + " - Level: " + monster.Level + "</b> est disponible dans la région : <b>" + nani.ZoneName + "</b>.",
//                        Color.OrangeRed
//                        ); ;

//                    continue;
//                }

//                var pos = new ObjectPosition(map, map.GetRandomFreeCell().Id);
//                var group = map.SpawnMonsterGroup(monster, pos, true);

//                nani.Spawned = true;
//                nani.ZoneName = map.SubArea.Record.Name;

//                if (monster.Template.Id == 2819)
//                {
//                    World.Instance.SendAnnounceLang(
//                    "<b>" + monster.Template.Name + " - Level: " + monster.Level + "</b> está disponível na área : <b>" + nani.ZoneName + "</b>.",
//                    "<b>" + monster.Template.Name + " - Level: " + monster.Level + "</b> is available in the area : <b>" + nani.ZoneName + "</b>.",
//                    "<b>" + monster.Template.Name + " - Level: " + monster.Level + "</b> esta disponible en la zona : <b>" + nani.ZoneName + "</b>.",
//                    "<b>" + monster.Template.Name + " - Level: " + monster.Level + "</b> est disponible dans la région : <b>" + nani.ZoneName + "</b>.",
//                    Color.OrangeRed
//                    );
//                }
//            }
//        }

//        public Map PickRandomMapInSUbArea(SubArea[] areas)
//        {
//            Random random = new Random();
//            int pickedareaIndex = random.Next(0, (areas.Count() - 1));
//            var pickedarea = areas[pickedareaIndex];
//            var AreaMaps = pickedarea.Maps.Where(x => x.HasPriorityOnWorldmap && x.AllowFightChallenges).ToArray();
//            int pickedMap = random.Next(0, (AreaMaps.Count() - 1));

//            return AreaMaps[pickedMap];
//        }

//        public void ResetSpawn(MonsterGrade Nani)
//        {
//            var mn = monsterNaniRecords.FirstOrDefault(x => x.Record.MonsterGradeId == Nani.Id || x.Record.MonsterGradeId == Nani.GradeId);

//            if (mn == null)
//                return;

//            mn.Spawned = false;
//        }
//    }
//}