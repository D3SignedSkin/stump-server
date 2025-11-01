using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database;
using Stump.Server.WorldServer.Database.Accounts;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Database.World.Maps;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Merchants;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Actors.RolePlay.TaxCollectors;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells.Triggers;
using Stump.Server.WorldServer.Database.World.Database.World;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.BaseServer.Logging;
using System.Globalization;
using MongoDB.Bson;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Server.BaseServer.IPC.Messages;
using Stump.Server.WorldServer.Game.Maps.Paddocks;
using Stump.ORM.SubSonic.Extensions;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Handlers.Context.RolePlay;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using Stump.Server.WorldServer.Game.Actors.RolePlay;
using Stump.Server.WorldServer.Database.Npcs;
using Stump.Server.WorldServer.Discord;

namespace Stump.Server.WorldServer.Game
{
    public class World : DataManager<World>
    {
        readonly Logger logger = LogManager.GetCurrentClassLogger();
        readonly ConcurrentDictionary<int, WorldAccount> m_connectedAccounts = new ConcurrentDictionary<int, WorldAccount>();
        readonly ConcurrentDictionary<int, Character> m_charactersById = new ConcurrentDictionary<int, Character>(Environment.ProcessorCount, ClientManager.MaxConcurrentConnections);
        readonly ConcurrentDictionary<string, Character> m_charactersByName = new ConcurrentDictionary<string, Character>(Environment.ProcessorCount, ClientManager.MaxConcurrentConnections, StringComparer.OrdinalIgnoreCase);

        Dictionary<int, Area> m_areas = new Dictionary<int, Area>();

        int m_characterCount;

        private Dictionary<double, MapScrollActionRecord> _mapScroll = new Dictionary<double, MapScrollActionRecord>();
        Dictionary<long, Map> m_maps = new Dictionary<long, Map>();
        Dictionary<double, double[]> m_maps_Coordinate = new Dictionary<double, double[]>();
        Dictionary<int, SubArea> m_subAreas = new Dictionary<int, SubArea>();
        Dictionary<int, SuperArea> m_superAreas = new Dictionary<int, SuperArea>();
        Dictionary<int, WorldMapGraveyardRecord> m_graveyards = new Dictionary<int, WorldMapGraveyardRecord>();

        readonly object m_saveLock = new object();
        readonly ConcurrentBag<ISaveable> m_saveablesInstances = new ConcurrentBag<ISaveable>();

        public event Action<Character> CharacterJoined;

        private List<Tuple<int, int>> MonstersRelouinEvent = new List<Tuple<int, int>>
        {
            Tuple.Create(98, 794),
            Tuple.Create(101, 793),
        };

        void OnCharacterEntered(Character character)
        {
            CharacterJoined?.Invoke(character);
        }

        public event Action<Character> CharacterLeft;

        void OnCharacterLeft(Character character)
        {
            CharacterLeft?.Invoke(character);
        }

        public int CharacterCount
        {
            get { return m_characterCount; }
        }

        public object SaveLock
        {
            get { return m_saveLock; }
        }

        private ServerStatusEnum Status
        {
            get;
            set;
        }

        public ServerStatusEnum GetWorldStatus() => Status;

        #region Initialization

        private bool m_spacesLoaded;
        private bool m_spacesSpawned;

        [Initialization(InitializationPass.Seventh)]
        public override void Initialize()
        {
            // maps
            LoadSpaces();
            SpawnSpaces();
        }

        public void Reload()
        {
            SpawnSpaces();
        }

        public void LoadSpaces()
        {
            if (m_spacesLoaded)
            {
                UnSetLinks();
            }

            m_spacesLoaded = true;

            logger.Info("Load maps...");
            m_maps = Database.Query<MapRecord, MapPositionRecord, MapRecord>(new MapRecordRelator().Map, MapRecordRelator.FetchQuery).ToDictionary(entry => entry.Id, entry => new Map(entry));

            logger.Info("Load sub areas...");
            m_subAreas = Database.Query<SubAreaRecord>(SubAreaRecordRelator.FetchQuery).ToDictionary(entry => entry.Id, entry => new SubArea(entry));

            logger.Info("Load areas...");
            m_areas = Database.Query<AreaRecord>(AreaRecordRelator.FetchQuery).ToDictionary(entry => entry.Id, entry => new Area(entry));

            logger.Info("Load super areas...");
            m_superAreas = Database.Query<SuperAreaRecord>(SuperAreaRecordRelator.FetchQuery).ToDictionary(entry => entry.Id, entry => new SuperArea(entry));

            logger.Info("Load graveyards...");
            m_graveyards = Database.Query<WorldMapGraveyardRecord>(WorldMapGraveyardRelator.FetchQuery).ToDictionary(entry => entry.Id, entry => entry);

            SetLinks();
        }

        public void SpawnSpaces()
        {
            if (m_spacesSpawned)
            {
                UnSpawnSpaces();
            }

            m_spacesSpawned = true;

            logger.Info("Spawn npcs ...");
            SpawnNpcs();

            if (Settings.ModoPVM)
            {
                logger.Info("Spawn npcs watends ...");
                SpawnWatendNpcs();
            }

            logger.Info("Spawn interactives ...");
            SpawnInteractives();

            logger.Info("Spawn cell triggers ...");
            SpawnCellTriggers();

            if (Settings.ModoPVM)
            {
                logger.Info("Spawn monsters ...");
                SpawnMonsters();
            }

            if (Settings.ModoPVM)
            {
                logger.Info("Spawn merchants ...");
                SpawnMerchants();
            }
        }

        private void SetLinks()
        {
            foreach (var map in m_maps.Values.Where(map => map.Record.Position != null))
            {
                SubArea subArea;
                if (m_subAreas.TryGetValue(map.Record.Position.SubAreaId, out subArea))
                {
                    subArea.AddMap(map);
                }
            }

            foreach (var subArea in m_subAreas.Values)
            {
                Area area;
                if (m_areas.TryGetValue(subArea.Record.AreaId, out area))
                {
                    area.AddSubArea(subArea);
                }
            }

            foreach (var area in m_areas.Values)
            {
                SuperArea superArea;
                if (m_superAreas.TryGetValue(area.Record.SuperAreaId, out superArea))
                {
                    superArea.AddArea(area);
                }
            }
        }

        private void UnSetLinks()
        {
            foreach (var map in m_maps.Values.Where(map => map.Record.Position != null))
            {
                SubArea subArea;
                if (m_subAreas.TryGetValue(map.Record.Position.SubAreaId, out subArea))
                {
                    subArea.RemoveMap(map);
                }
            }

            foreach (var subArea in m_subAreas.Values)
            {
                Area area;
                if (m_areas.TryGetValue(subArea.Record.AreaId, out area))
                {
                    area.RemoveSubArea(subArea);
                }
            }

            foreach (var area in m_areas.Values)
            {
                SuperArea superArea;
                if (m_superAreas.TryGetValue(area.Record.SuperAreaId, out superArea))
                {
                    superArea.RemoveArea(area);
                }
            }
        }

        public static void SpawnWatendNpcs()
        {
            var watendsNpcs = WatendManager.Instance.GetNpcWatendSpawns();

            foreach (var watend in watendsNpcs)
            {
                NpcSpawn npcSpawn = new NpcSpawn()
                {
                    NpcId = watend.NpcId,
                    MapId = (uint)watend.MapId,
                    CellId = watend.CellId,
                    Direction = watend.Direction
                };

                var position = npcSpawn.GetPosition();

                position.Map.SpawnWatendNpc(watend, npcSpawn.Template, position, npcSpawn.Template.Look);
            }
        }

        public static void SpawnNpcs()
        {
            foreach (var npcSpawn in NpcManager.Instance.GetNpcSpawns().Where(x => x.Active >= 1))
            {
                var position = npcSpawn.GetPosition();

                position.Map.SpawnNpc(npcSpawn);
            }
        }

        //Função para Spawn de apenas um único mapa by: Kenshin
        public static void SpawnNpcsMap(Character character)
        {
            foreach (var npcSpawn in NpcManager.Instance.GetNpcSpawns().Where(x => x.Active >= 1))
            {
                if (npcSpawn.MapId == character.Map.Id)
                {
                    var position = npcSpawn.GetPosition();

                    position.Map.SpawnNpc(npcSpawn);
                }
            }
        }

        public void UnSpawnSpaces()
        {
            foreach (var map in m_maps.Values)
            {
                foreach (var interactive in map.GetInteractiveObjects().ToArray())
                {
                    map.UnSpawnInteractive(interactive);
                }

                foreach (var pool in map.SpawningPools.ToArray())
                {
                    map.RemoveSpawningPool(pool);
                }

                foreach (var trigger in map.GetTriggers().ToArray())
                {
                    map.RemoveTrigger(trigger);
                }
            }

            foreach (var subArea in m_subAreas)
            {
                foreach (var monsterSpawn in subArea.Value.MonsterSpawns.ToArray())
                {
                    subArea.Value.RemoveMonsterSpawn(monsterSpawn);
                }
            }
        }
        public void UnSpawnNpcs()
        {
            foreach (var current in m_maps.Values)
            {
                List<Npc> npcsToDelete = new List<Npc>();
                var npcs = current.Actors.Where(x => x is Npc);
                foreach (var npc in npcs)
                {
                    npcsToDelete.Add(npc as Npc);
                }
                foreach (var npc in npcsToDelete)
                {
                    current.UnSpawnNpc(npc);
                }
            }
        }

        //Função para UnSpawn de Apenas um mapa by: Kenshin
        public void UnSpawnNpcsMap(Character character)
        {
            foreach (var current in m_maps.Values)
            {
                List<Npc> npcsToDelete = new List<Npc>();
                var npcs = current.Actors.Where(x => x is Npc && x.Map.Id == character.Map.Id);

                foreach (var npc in npcs)
                {
                    npcsToDelete.Add(npc as Npc);
                }

                foreach (var npc in npcsToDelete)
                {
                    current.UnSpawnNpc(npc);
                }
            }
        }

        public void SpawnInteractives()
        {
            foreach (var interactive in InteractiveManager.Instance.GetInteractiveSpawns())
            {
                var map = interactive.GetMap();

                if (map == null)
                {
                    logger.Error("Cannot spawn interactive id={0} : map {1} doesn't exist", interactive.Id, interactive.MapId);
                    continue;
                }

                map.SpawnInteractive(interactive);
                map.ForEach(entry => ContextRoleplayHandler.SendMapComplementaryInformationsDataMessage(entry.Client));
            }

            foreach (var map in m_maps)
                map.Value.UpdateAvailableJobs();
        }

        //Função Criada para Spawn apenas Interativos da Area
        public void SpawnInteractivesArea(Character character)
        {
            foreach (var interactive in InteractiveManager.Instance.GetInteractiveSpawns())
            {
                var map = interactive.GetMap();

                if (map == null)
                {
                    logger.Error("Cannot spawn interactive id={0} : map {1} doesn't exist", interactive.Id, interactive.MapId);
                    continue;
                }

                if (map.Area.Id == character.Map.Area.Id)
                {
                    map.SpawnInteractive(interactive);
                    map.ForEach(entry => ContextRoleplayHandler.SendMapComplementaryInformationsDataMessage(entry.Client));
                }
            }

            foreach (var map in m_maps)
            {
                if (map.Value.Area.Id == character.Map.Area.Id)
                {
                    map.Value.UpdateAvailableJobs();
                }
            }
        }

        public void UnSpawnInteractives()
        {
            foreach (var map in m_maps.Values)
            {
                var interactives = map.GetInteractiveObjects().ToArray();

                foreach (var interactive in interactives)
                {
                    map.UnSpawnInteractive(interactive);
                }
            }
        }

        //Função para UnSpawn de Interativos da Area by: Kenshin
        public void UnSpawnInteractivesArea(Character character)
        {
            foreach (var map in m_maps.Values)
            {
                var interactives = map.GetInteractiveObjects().ToArray();

                foreach (var interactive in interactives)
                {
                    if (interactive.Map.Area.Id == character.Map.Area.Id)
                        map.UnSpawnInteractive(interactive);
                }
            }
        }

        public void SpawnCellTriggers()
        {
            foreach (var trigger in CellTriggerManager.Instance.GetCellTriggers().Select(cellTrigger => cellTrigger.GenerateTrigger()))
            {
                trigger.Position.Map.AddTrigger(trigger);
            }
        }

        public void UnSpawnCellTriggers()
        {
            foreach (var map in m_maps.Values)
            {
                var triggers = map.GetTriggers().ToArray();

                foreach (var trigger in triggers)
                {
                    map.RemoveTrigger(trigger);
                }
            }
        }

        private void SpawnMonsters()
        {
            Boolean isRelouinActive = DateTime.Now >= Settings.StartHelloween && DateTime.Now <= Settings.EndHelloween;

            foreach (var spawn in MonsterManager.Instance.GetMonsterSpawns())
            {
                if (spawn.Template is null)
                {
                    logger.Error($"Error SpawnMonsters ID: {spawn.Id}");
                    continue;
                }

                Boolean isMonsterRelouinEvent = isRelouinActive && MonstersRelouinEvent.Any(x => x.Item1 == spawn.MonsterId);

                if (isMonsterRelouinEvent)
                {
                    MonsterSpawn monsterEvent = MonsterManager.Instance.GetMonsterSpawn((int)spawn.SubAreaId, spawn.Frequency, MonstersRelouinEvent.FirstOrDefault(x => x.Item1 == spawn.MonsterId).Item2);

                    if (monsterEvent.MonsterId > 0 && monsterEvent.SubArea != null)
                    {
                        spawn.SubArea.AddMonsterSpawn(monsterEvent);
                    }
                }
                else
                {
                    if (spawn.Map != null)
                    {
                        spawn.Map.AddMonsterSpawn(spawn);
                    }
                    else if (spawn.SubArea != null)
                    {
                        spawn.SubArea.AddMonsterSpawn(spawn);
                    }
                }
            }

            foreach (var spawn in MonsterManager.Instance.GetMonsterDungeonsSpawns().Where(spawn => spawn.Map != null))
            {
                spawn.Map.AddMonsterDungeonSpawn(spawn);
            }

            foreach (var spawn in MonsterManager.Instance.GetMonsterStaticSpawns().Where(spawn => spawn.Map != null))
            {
                spawn.Map.AddMonsterStaticSpawn(spawn);
            }

            foreach (var map in m_maps.Where(map => map.Value.MonsterSpawnsCount > 0))
            {
                map.Value.EnableClassicalMonsterSpawns();
            }
        }

        static void SpawnMerchants()
        {
            foreach (var merchant in from spawn in MerchantManager.Instance.GetMerchantSpawns() where spawn.Map != null select new Merchant(spawn))
            {
                merchant.LoadRecord();
                MerchantManager.Instance.ActiveMerchant(merchant);
                merchant.Map.Enter(merchant);
            }
        }

        void UnSpawnMerchants()
        {
            foreach (var merchant in MerchantManager.Instance.Merchants)
            {
                MerchantManager.Instance.UnActiveMerchant(merchant);
                merchant.Map.Leave(merchant);
            }
        }

        public void SpawnTaxCollectors()
        {
            foreach (var taxcollector in from spawn in TaxCollectorManager.Instance.GetTaxCollectorSpawns() where spawn.Map != null select new TaxCollectorNpc(spawn, spawn.Map.GetNextContextualId()))
            {
                taxcollector.Guild.AddTaxCollector(taxcollector);
                taxcollector.Map.Enter(taxcollector);
            }
        }

        public void SpawnPaddocksGuild()
        {
            foreach (var paddock in from spawn in PaddockManager.Instance.GetPaddocks() where spawn.Map != null && spawn.Guild != null select spawn)
            {
                paddock.Guild.AddPaddock(paddock);

            }
        }
        #endregion

        public void SpawnPrisms()
        {
            //logger.Info("Spawn Prisms ...");
            //foreach (var current in
            //    from spawn in PrismManager.Instance.GetPrismSpawns()
            //    where spawn.Map != null
            //    select new PrismNpc(spawn, spawn.Map.GetNextContextualId()))
            //{
            //    current.Alliance.AddPrism(current);
            //    current.Map.Enter(current);
            //    current.Map.SubArea.HasPrism = true;
            //    PrismManager.Instance.AddActivePrism(current);
            //}
        }

        #region Maps

        public double[] GetMapsCordinates(long mapId)
        {
            return m_maps_Coordinate.ContainsKey(mapId) ? m_maps_Coordinate[mapId] : null;
        }

        public MapScrollActionRecord GetMapScroll(long mapId)
        {
            return _mapScroll.ContainsKey(mapId) ? _mapScroll[mapId] : null;
        }

        public Map GetMap(long id)
        {
            Map map;
            m_maps.TryGetValue(id, out map);

            return map;
        }

        public Map GetMap(int x, int y, bool outdoor = true)
        {
            return m_maps.Values.FirstOrDefault(entry => entry.Position.X == x && entry.Position.Y == y && entry.Outdoor == outdoor);
        }

        public Map GetMap(int x, int y, int worldPos, bool HaspriorityWorldMap = false)
        {
            if (
                !m_maps.Values.Any(
                    entry =>
                        entry.Position.X == x && entry.Position.Y == y && entry.WorldMap == worldPos &&
                        entry.Record.Position.HasPriorityOnWorldmap == HaspriorityWorldMap))
            {
                var position =
                    Database.FirstOrDefault<MapPositionRecord>("SELECT * FROM world_maps_positions WHERE PosX=" + x +
                                                               " AND PosY=" + y + " AND WorldMap=" + worldPos +
                                                               " AND HasPriorityOnWorldmap=" +
                                                               Convert.ToInt16(HaspriorityWorldMap));
                if (position != null)
                    GetMap(position.Id);
            }
            return
                m_maps.Values.FirstOrDefault(
                    entry => entry.Position.X == x && entry.Position.Y == y && entry.WorldMap == worldPos &&
                        entry.Record.Position.HasPriorityOnWorldmap == HaspriorityWorldMap);
        }

        public IEnumerable<Map> GetAllMapsWhereIsAZaap()
        {
            return GetMaps().Where(x => x.Zaap != null);
        }
        public IEnumerable<Map> GetAllMapsWhereIsAZaapiByArea(int subArea)
        {
            return GetMaps().Where(x => x.Area.Id == subArea && x.Zaapi != null);
        }
        public IEnumerable<Map> GetMaps()
        {
            return m_maps.Values;
        }

        public Map[] GetMaps(int x, int y)
        {
            return m_maps.Values.Where(entry => entry.Position.X == x && entry.Position.Y == y).ToArray();
        }

        public Map[] GetMaps(int x, int y, bool outdoor)
        {
            return m_maps.Values.Where(entry => entry.Position.X == x && entry.Position.Y == y && entry.Outdoor == outdoor).ToArray();
        }

        public Map[] GetMaps(Map reference, int x, int y)
        {
            var maps = reference.SubArea.GetMaps(x, y);
            if (maps.Length > 0)
                return maps;

            maps = reference.Area.GetMaps(x, y);
            if (maps.Length > 0)
                return maps;

            maps = reference.SuperArea.GetMaps(x, y);
            return maps.Length > 0 ? maps : new Map[0];
        }

        public Map[] GetMaps(Map reference, int x, int y, bool outdoor)
        {
            var maps = reference.SubArea.GetMaps(x, y, outdoor);
            if (maps.Length > 0)
                return maps;

            maps = reference.Area.GetMaps(x, y, outdoor);
            if (maps.Length > 0)
                return maps;

            maps = reference.SuperArea.GetMaps(x, y, outdoor);
            return maps.Length > 0 ? maps : new Map[0];
        }

        public IEnumerable<Map> GetMapsByArea(ushort AreaId)
        {
            return m_maps.Values.Where(x => x.Area.Id == AreaId).ToList();
        }

        public SubArea GetSubArea(int id)
        {
            SubArea subArea;
            m_subAreas.TryGetValue(id, out subArea);

            return subArea;
        }

        public SubArea GetSubArea(string name) => m_subAreas.Values.FirstOrDefault(entry => entry.Record.Name == name);

        public Area GetArea(int id)
        {
            Area area;
            m_areas.TryGetValue(id, out area);

            return area;
        }
        public List<SubArea> GetSubAreas()
        {
            return m_subAreas.Values.ToList();
        }
        public Area GetArea(string name) => m_areas.Values.FirstOrDefault(entry => entry.Name == name);

        public SuperArea GetSuperArea(int id)
        {
            SuperArea superArea;
            m_superAreas.TryGetValue(id, out superArea);

            return superArea;
        }
        public SuperArea GetSuperArea(string name) => m_superAreas.Values.FirstOrDefault(entry => entry.Name == name);

        public IEnumerable<WorldMapGraveyardRecord> GetGraveyards() => m_graveyards.Values.ToArray();
        public void SetGraveyards(Dictionary<int, WorldMapGraveyardRecord> graveyards)
        {
            m_graveyards = graveyards;
        }
        public WorldMapGraveyardRecord GetNearestGraveyard(Map currentMap)
        {
            var actualPoint = new MapPoint(currentMap.Position);
            return GetGraveyards().OrderBy(x => actualPoint.EuclideanDistanceTo(new MapPoint(x.Map.Position)))
                .OrderByDescending(x => x.Map.Area.Id == currentMap.Area.Id)
                .OrderByDescending(x => x.SubAreaId == currentMap.SubArea.Id).FirstOrDefault();
        }

        #endregion

        #region Actors

        public void Enter(Character character)
        {
            // note : to delete
            if (m_charactersById.ContainsKey(character.Id))
                Leave(character);

            if (m_charactersById.TryAdd(character.Id, character) && m_charactersByName.TryAdd(character.Name, character))
            {
                Interlocked.Increment(ref m_characterCount);
                OnCharacterEntered(character);
            }
            else
                logger.Error("Cannot add character {0} to the World", character);

            if (!m_connectedAccounts.ContainsKey(character.Account.Id))
                m_connectedAccounts.TryAdd(character.Account.Id, character.Client.WorldAccount);
        }

        public void Leave(Character character)
        {
            Character dummy;
            if (m_charactersById.TryRemove(character.Id, out dummy) && m_charactersByName.TryRemove(character.Name, out dummy))
            {
                Interlocked.Decrement(ref m_characterCount);
                OnCharacterLeft(character);
            }
            else
                logger.Error("Cannot remove character {0} from the World", character);

            WorldAccount dAccount;
            m_connectedAccounts.TryRemove(character.Account.Id, out dAccount);
        }

        public bool IsConnected(int id)
        {
            return m_charactersById.ContainsKey(id);
        }

        public bool IsConnected(string name)
        {
            return m_charactersByName.ContainsKey(name);
        }

        public bool IsAccountConnected(int id)
        {
            return m_connectedAccounts.ContainsKey(id);
        }

        public WorldAccount GetConnectedAccount(int id)
        {
            WorldAccount account;
            return m_connectedAccounts.TryGetValue(id, out account) ? account : null;
        }

        public Character GetCharacter(int id)
        {
            Character character;
            return m_charactersById.TryGetValue(id, out character) ? character : null;
        }

        public Character GetCharacter(string name)
        {
            Character character;
            m_charactersByName.TryGetValue(name, out character);

            if (character != null)
                return character;

            try
            {
                character = WorldServer.Instance.ClientManager.FindAll<WorldClient>(entry => entry.Character.Name == name).Select(entry => entry.Character).SingleOrDefault();
                return character;
            }
            catch
            { }

            return null;
        }

        public Character GetCharacter(Predicate<Character> predicate)
        {
            return m_charactersById.FirstOrDefault(k => predicate(k.Value)).Value;
        }

        public IEnumerable<Character> GetCharacters(Predicate<Character> predicate)
        {
            return m_charactersById.Values.Where(k => predicate(k));
        }

        /// <summary>
        /// Get a spell by a search pattern. *account = current spell used by account, name = spell by his name.
        /// </summary>
        /// <returns></returns>
        public Character GetCharacterByPattern(string pattern)
        {
            if (pattern[0] != '*')
                return GetCharacter(pattern);

            var name = pattern.Remove(0, 1);


            return WorldServer.Instance.ClientManager.FindAll<WorldClient>(entry => entry.Account.Login == name).Select(entry => entry.Character).SingleOrDefault();
        }
        public IEnumerable<Character> GetCharacters()
        {
            return m_charactersById.Values;
        }

        /// <summary>
        /// Get a spell by a search pattern. * = caller, *account = current spell used by account, name = spell by his name.
        /// </summary>
        /// <returns></returns>
        public Character GetCharacterByPattern(Character caller, string pattern)
        {
            return pattern == "*" ? caller : GetCharacterByPattern(pattern);
        }

        public void ForEachCharacter(Action<Character> action)
        {
            foreach (var key in m_charactersById)
                action(key.Value);
        }

        public void ForEachCharacter(Predicate<Character> predicate, Action<Character> action)
        {
            foreach (var key in m_charactersById.Where(k => predicate(k.Value)))
                action(key.Value);
        }

        public void SendAnnounce(string announce)
        {
            ForEachCharacter(character => character.SendServerMessage(announce));
        }

        public void SendAnnounce(string announce, Color color)
        {
            ForEachCharacter(character => character.SendServerMessage(announce, color));
        }

        public void SendAnnounceLang(string announce, string announceen, string announcees, string announcefr, Color color)
        {
            ForEachCharacter(character =>
            {
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage(announcefr, color);
                        break;
                    case "es":
                        character.SendServerMessage(announcees, color);
                        break;
                    case "en":
                        character.SendServerMessage(announceen, color);
                        break;
                    default:
                        character.SendServerMessage(announce, color);
                        break;
                }
            });
        }

        public void SendAnnounce(TextInformationTypeEnum type, short messageId, params object[] parameters)
        {
            ForEachCharacter(character => character.SendInformationMessage(type, messageId, parameters));
        }

        public void SendAnnounceDisplayLang(string announce, string announceen, string announcees, string announcefr)
        {
            ForEachCharacter(character =>
            {
                character.SendServerDisplayLang(announce, announceen, announcees, announcefr);
            });
        }

        public void SendServerMessageGoldVips(string message, string messageen, string messagees, string messagefr, Color color)
        {
            ForEachCharacter(character => character.SendServerMessageLangColor(message, messageen, messagees, messagefr, color));
        }
        #endregion

        public void RegisterSaveableInstance(ISaveable instance)
        {
            if (!m_saveablesInstances.Contains(instance))
                m_saveablesInstances.Add(instance);
        }

        public void Save()
        {
            lock (SaveLock)
            {
                if (IPCAccessor.Instance.IsConnected)
                {
                    IPCAccessor.Instance.SendRequest(new ServerUpdateMessage(WorldServer.Instance.ClientManager.Count, ServerStatusEnum.SAVING), delegate (Server.BaseServer.IPC.Messages.CommonOKMessage message)
                    {
                        Console.WriteLine("Sended State World => " + ServerStatusEnum.SAVING);
                        Status = ServerStatusEnum.SAVING;
                    });
                }

                logger.Info("Saving world ...");

                this.SendAnnounceLang("Server salvando...", "Saving Server...", "Server salvando...", "Server sauvegarder...", Color.Orange); //this.SendAnnounce(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 164, new object[0]);

                var sw = Stopwatch.StartNew();
                var clients = WorldServer.Instance.ClientManager.FindAll<WorldClient>().ToList();

                logger.Info("Saving clients ...");

                foreach (var client in clients)
                {
                    try
                    {
                        if (client.Character != null) //&& WorldServer.Clients.Contains(client))
                        {
                            logger.Info("saving character {0}...", client.Character.Name);

                            client.Character.SaveNow();

                            logger.Info("character {0} saved in {1} ", client.Character.Name, sw.ElapsedMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Cannot save character {0} : {1}", client, ex);
                    }
                }

                logger.Info("Saving instance ...");

                foreach (var instance in m_saveablesInstances.ToList())
                {
                    try
                    {
                        logger.Info("Saving instance {0} ...", instance);

                        instance.Save();

                        logger.Info("instance {0} saved in {1} ", instance, sw.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Cannot save instance {0} : {1}", instance, ex);
                    }
                }

                var document = new BsonDocument
                {
                    { "Id", WorldServer.ServerInformation.Id },
                    { "Name", WorldServer.ServerInformation.Name },
                    { "Players", WorldServer.Instance.ClientManager.Count },
                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                };

                MongoLogger.Instance.Insert("World_Stats", document);

                logger.Info("changing ipc status...");

                if (IPCAccessor.Instance.IsConnected)
                {
                    logger.Info("ipc connected...");

                    IPCAccessor.Instance.SendRequest(new ServerUpdateMessage(WorldServer.Instance.ClientManager.Count, ServerStatusEnum.FINISH_SAVE), delegate (CommonOKMessage message)
                    {
                        Status = ServerStatusEnum.FINISH_SAVE;

                        logger.Info("ipc first request ok...");

                        if (IPCAccessor.Instance.IsConnected)
                        {
                            logger.Info("ipc connected again...");

                            IPCAccessor.Instance.SendRequest(new ServerUpdateMessage(WorldServer.Instance.ClientManager.Count, ServerStatusEnum.ONLINE), delegate (CommonOKMessage messagee)
                            {
                                Status = ServerStatusEnum.ONLINE;

                                logger.Info("ipc second request ok...");
                                Console.WriteLine("Sended State World => " + ServerStatusEnum.ONLINE);
                            });
                        }
                    });
                }

                this.SendAnnounceLang("Server salvo!", "Saved Server...", "¡Server salvo!", "Server sauvé!", Color.Orange); //this.SendAnnounce(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 165, new object[0]);

                logger.Info("World server saved ! ({0} ms)", sw.ElapsedMilliseconds);
            }
        }

        public void Stop(bool wait = false)
        {
            foreach (var area in m_areas)
            {
                area.Value.Stop(wait);
            }
        }

        private readonly List<Area> m_pausedAreas = new List<Area>();
        /// <summary>
        /// Has to be called from another thread !!
        /// </summary>
        public void Pause()
        {
            logger.Info("World Paused !!");

            foreach (var area in m_areas.Where(x => x.Value.IsRunning))
            {
                if (area.Value.IsInContext)
                    throw new Exception("Has to be called from another thread !!");

                area.Value.Stop(true);
                m_pausedAreas.Add(area.Value);
            }

            if (WorldServer.Instance.IOTaskPool.IsInContext)
                throw new Exception("Has to be called from another thread !!");

            WorldServer.Instance.IOTaskPool.Stop(true);
        }

        public void Resume()
        {
            logger.Info("World Resumed");

            foreach (var pausedArea in m_pausedAreas)
            {
                pausedArea.Start();
            }

            m_pausedAreas.Clear();

            WorldServer.Instance.IOTaskPool.Start();
        }

        public void SendAnnounceAllPlayersShutDown() // 
        {
            var Joueurco = World.Instance.GetCharacters().ToList(); // Le serveur vérifie les personnages qui sont connecté dans la Base de donnée

            foreach (var character in Joueurco)
            {
                character.SendSystemMessage(3, true); // Le serveur envoie  à tout les joueurs connecté un message de redémarrage serveur
            }
        }

        public void SendAnnounceAllPlayersEvent() // Nom au hasard (Mettre un nom reconnaissable quand même...)
        {
            var connectedcharacter = World.Instance.GetCharacters().ToList(); // Le serveur vérifie les personnages qui sont connecté dans la Base de donnée

            foreach (var character in connectedcharacter)
            {
                character.DisplayNotification("<br>Un évent est en préparation! Rendez-vous en .tp event !"); // Affichage temporaire (Notification)
            }
        }

        public String FormatAnnounce(String lang, Character player)
        {
            if (lang == "fr")
                return "VIP " + player.Account.Lang.ToUpper() + " - <b>" + player.Namedefault + "</b> est en ligne";
            else if (lang == "es")
                return "VIP " + player.Account.Lang.ToUpper() + " - <b>" + player.Namedefault + "</b> está online";
            else if (lang == "en")
                return "VIP " + player.Account.Lang.ToUpper() + " - <b>" + player.Namedefault + "</b> is online";
            else
                return "VIP " + player.Account.Lang.ToUpper() + " - <b>" + player.Namedefault + "</b> conectou-se";
        }

        public String FormatAnnounceDiscord(String lang, Character player)
        {
            if (lang == "fr")
                return "VIP " + player.Account.Lang.ToUpper() + " - **" + player.Namedefault + "** est en ligne";
            else if (lang == "es")
                return "VIP " + player.Account.Lang.ToUpper() + " - **" + player.Namedefault + "** está online";
            else if (lang == "en")
                return "VIP " + player.Account.Lang.ToUpper() + " - **" + player.Namedefault + "** is online";
            else
                return "VIP " + player.Account.Lang.ToUpper() + " - **" + player.Namedefault + "** conectou-se";
        }

        public void SendAnnounceAuto(Character player)
        {
            var connectedcharacter = World.Instance.GetCharacters().ToList();

            foreach (var all in connectedcharacter)
            {
                if (player.Account.UserGroupId <= Settings.AnnouncePlayerOnlineGroup)
                {
                    if (all.Account.Lang == player.Account.Lang)
                    {
                        if (player.Client.UserGroup.Role == RoleEnum.Gold_Vip)
                            all.SendServerMessage(FormatAnnounce(all.Account.Lang, player), Color.Yellow);
                        else
                            all.SendServerMessage(FormatAnnounce(all.Account.Lang, player), Color.PaleGoldenrod);
                    }
                    else
                    {
                        all.SendServerMessage(FormatAnnounce(all.Account.Lang, player), Color.Gray);
                    }
                }
            }

            if (player.Account.UserGroupId <= Settings.AnnouncePlayerOnlineGroup)
                PlainText.SendWebHook(DiscordIntegration.DiscordChatVipUrl, FormatAnnounceDiscord(player.Account.Lang, player), DiscordIntegration.DiscordWHUsername);
        }
    }
}