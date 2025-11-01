using NLog;
using System;
using System.Linq;
using System.Collections.Generic;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Maps.Paddocks;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game
{
    public class PaddockCreationManager : DataManager<PaddockCreationManager>, ISaveable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static Dictionary<int, CharactersItemsPaddock> m_charactersPaddocksItems = new Dictionary<int, CharactersItemsPaddock>();

        [Initialization(InitializationPass.Sixth)]
        public override void Initialize()
        {
            m_charactersPaddocksItems = Database.Fetch<CharactersItemsPaddock>(CharactersItemsPaddockRelator.FetchQuery).ToDictionary(x => x.Id);

            World.Instance.RegisterSaveableInstance(this);
        }

        public static Boolean HasItemInCellId(int cellid, long mapId) => m_charactersPaddocksItems.Values.Any(entry => entry.MapId == mapId && entry.CellId == cellid && !entry.IsDeleted);

        public static bool PaddockLimitItemsReached(Character character, Paddock paddock)
        {
            //Se for paddock particular
            if (!paddock.IsPublicPaddock())
            {
                return m_charactersPaddocksItems.Values.Count(entry => entry.PaddockId == paddock.Id && !entry.IsDeleted) >= paddock.MaxItems;
            }
            //Se for paddock pública
            else
            {
                return m_charactersPaddocksItems.Values.Count(entry => entry.PaddockId == paddock.Id && entry.OwnerId == character.Id && !entry.IsDeleted) >= paddock.MaxItems;
            }
        }

        public void AddItemRecord(int ownerId, int paddockId, long mapId, ushort itemId, ushort cellId, short itemEffect, short itemUsed, short itemUsedMax)
        {
            CharactersItemsPaddock newItem = new CharactersItemsPaddock
            {
                Id = CharactersItemsPaddock.PopNextId(),
                OwnerId = ownerId,
                PaddockId = paddockId,
                MapId = mapId,
                ItemId = itemId,
                CellId = cellId,
                ItemEffectiveness = itemEffect,
                ItemUseds = itemUsed,
                ItemUsedsMax = itemUsedMax,
                IsNew = true
            };

            m_charactersPaddocksItems.Add(newItem.Id, newItem);
        }

        public List<CharactersItemsPaddock> GetCharacterItemsPaddock(int paddockId, int characterId)
        {
            List<CharactersItemsPaddock> _itemsPaddock = m_charactersPaddocksItems.Values.Where(x => x.PaddockId == paddockId && x.OwnerId == characterId && !x.IsDeleted).ToList();
            return _itemsPaddock;
        }

        public List<CharactersItemsPaddock> GetMapItemsPaddock(int paddockId, long mapId)
        {
            List<CharactersItemsPaddock> _itemsPaddock = m_charactersPaddocksItems.Values.Where(x => x.PaddockId == paddockId && x.MapId == mapId && !x.IsDeleted).ToList();
            return _itemsPaddock;
        }

        public void Save()
        {
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                try
                {
                    var itemsToRemove = m_charactersPaddocksItems
                        .Where(pair => pair.Value.IsDeleted)
                        .Select(pair => pair.Key)
                        .ToList();

                    var itemsToUpdate = m_charactersPaddocksItems
                        .Where(pair => pair.Value.IsUpdate && !pair.Value.IsNew)
                        .ToList();

                    var itemsToInsert = m_charactersPaddocksItems
                        .Where(pair => pair.Value.IsNew)
                        .ToList();

                    foreach (var key in itemsToRemove)
                    {
                        var item = m_charactersPaddocksItems[key];

                        m_charactersPaddocksItems.Remove(key);
                        Database.Delete(item);
                    }

                    foreach (var pair in itemsToUpdate)
                    {
                        var item = pair.Value;

                        Database.Update(item);
                        item.IsUpdate = false;
                    }

                    foreach (var pair in itemsToInsert)
                    {
                        var item = pair.Value;

                        Database.Insert(item);
                        item.IsNew = false;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving Character ItemPaddock: {ex.Message}");
                }
            });
        }
    }
}