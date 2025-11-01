using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Enums.Custom;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Exchanges.Paddock;
using Stump.Server.WorldServer.Game.Guilds;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Handlers.Mounts;
using Stump.Server.WorldServer.Handlers.Paddock;

namespace Stump.Server.WorldServer.Game.Maps.Paddocks
{
    public class Paddock
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool IsPublicPaddock() => IsPublic == true && OnSale == false;

        //Bebedouros
        private List<int> drinking = new List<int>() { 7590, 7591, 7592, 7593, 7594, 7595, 7596, 7597, 7598, 7599, 7600, 7601, 7602, 7603, 7604, 7605, 7673, 7674, 7675, 7676, 7677, 7678, 7679, 7682, 14707, 14708, 14709, 14710, 14711, 14712, 14713, 14714, 14715, 14716, 14717, 14718, 14789, 15709, 16287, 19917, 19918, 19919, 19927 };

        //Manjedouras
        private List<int> manger = new List<int>() { 7606, 7607, 7608, 7609, 7610, 7611, 7612, 7613, 7614, 7615, 7616, 7617, 7618, 7619, 7620, 7621, 7683, 7684, 7685, 7686, 7687, 7688, 7689, 7690, 14772, 14773, 14775, 14776, 1477, 14778, 14779, 14780, 14781, 14782, 14783, 14784, 14788, 14794, 16288, 17740, 19931, 19932, 21518 };

        //Esbofeteador
        private List<int> slapper = new List<int>() { 7625, 7626, 7627, 7629, 7755, 7756, 7757, 7758, 7759, 7760, 7761, 7762, 7763, 7765, 7766, 7767, 7768, 7769, 7770, 7771, 7772, 7773, 7774, 14732, 14733, 14734, 14735, 14736, 14737, 14738, 14739, 14740, 14741, 14742, 14743, 14786, 14791, 15096, 15277, 15708, 16291, 17038, 19925, 19926 };

        //Acariciador
        private List<int> patter = new List<int>() { 7622, 7623, 7624, 7628, 7733, 7734, 7735, 7736, 7737, 7738, 7739, 7740, 7741, 7742, 7743, 7744, 7745, 7746, 14758, 14759, 14760, 14761, 14762, 14763, 14764, 14765, 14766, 14767, 14768, 14769, 14770, 14771, 14793, 15707, 16289, 17416, 17739, 19930, 21519 };

        //Fulminador
        private List<int> lightning = new List<int>() { 7775, 7776, 7777, 7778, 7779, 7780, 7781, 7782, 7783, 7784, 7785, 7786, 7787, 7788, 7789, 7790, 7791, 7792, 7793, 7794, 7795, 7796, 7797, 7798, 14719, 14721, 14722, 14723, 14724, 14725, 14726, 14727, 14728, 14729, 14730, 14731, 14785, 14790, 15276, 17738, 19922, 19923, 19924 };

        //Dragobunda
        private List<int> dragobutt = new List<int>() { 7634, 7635, 7636, 7637, 7691, 7692, 7693, 7694, 7695, 7696, 7697, 7698, 7699, 7700, 14744, 14745, 14746, 14747, 14748, 14749, 14750, 14751, 14752, 14753, 14754, 14755, 14756, 14757, 14787, 14792, 14976, 15275, 15363, 16290, 19928, 19929, 21516 };

        public Paddock(WorldMapPaddockRecord record)
        {
            Record = record;

            if (record.Map == null)
                throw new Exception(string.Format("Paddock's map({0}) not found", record.MapId));
        }

        #region >> Properties

        private WorldMapPaddockRecord Record { get; }

        public Guild Guild
        {
            get
            {
                return Record.Guild;
            }
            set
            {
                IsRecordDirty = true;
                Record.Guild = value;
            }
        }

        public Map Map
        {
            get
            {
                return Record.Map;
            }
            protected set
            {
                IsRecordDirty = true;
                Record.Map = value;
            }
        }

        public int Id => Record.Id;

        public bool IsPublic
        {
            get { return Record.IsPublic; }
        }

        public uint MaxOutdoorMount
        {
            get
            {
                return Record.MaxOutdoorMount;
            }
            protected set
            {
                IsRecordDirty = true;
                Record.MaxOutdoorMount = value;
            }
        }

        public uint MaxItems
        {
            get
            {
                return Record.MaxItems;
            }
            protected set
            {
                IsRecordDirty = true;
                Record.MaxItems = value;
            }
        }

        public bool Abandonned
        {
            get
            {
                return Record.Abandonned;
            }
            protected set
            {
                IsRecordDirty = true;
                Record.Abandonned = value;
            }
        }

        public bool OnSale
        {
            get
            {
                return Record.OnSale;
            }
            set
            {
                IsRecordDirty = true;
                Record.OnSale = value;
            }
        }

        public bool Locked
        {
            get
            {
                return Record.Locked;
            }
            set
            {
                IsRecordDirty = true;
                Record.Locked = value;
            }
        }

        public ulong Price
        {
            get
            {
                return Record.Price;
            }
            set
            {
                IsRecordDirty = true;
                Record.Price = value;
            }
        }

        public bool IsRecordDirty
        {
            get;
            private set;
        }
        #endregion

        #region >> Handlers

        public bool IsPaddockOwner(Character character)
        {
            if (IsPublicPaddock())
            {
                return true;
            }
            if (character.Guild?.Id == Guild?.Id && Guild != null)
            {
                return character.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_USE_PADDOCKS);
            }
            else
            {
                return false;
            }
        }

        public bool CanOrganizePaddock(Character character)
        {
            if (character.Guild != null && this.Record.GuildId > 0)
            {
                if (character.Guild?.Id == this.Record.GuildId)
                {
                    return character.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_ORGANIZE_PADDOCKS);
                }
            }

            return false;
        }

        public bool CanTakeOthersMounts(Character character)
        {
            if (character.Guild != null && this.Record.GuildId > 0)
            {
                if (character.Guild?.Id == this.Record.GuildId)
                {
                    return character.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_TAKE_OTHERS_MOUNTS_IN_PADDOCKS);
                }
            }

            return false;
        }

        public bool AddMountToPaddock(Character character, Mount mount, Mount mountSave = null)
        {
            if (!(character.Dialoger is PaddockExchanger paddockExchanger))
            {
                Console.WriteLine("Error: Character's dialoger is not a PaddockExchanger.");
                return false;
            }

            var spawnCell = this.Map.GetInteractiveObject(paddockExchanger.InteractivePaddock.Id)
                                    .Position.Point.GetAdjacentCells()
                                    .Select(entry => this.Map.GetCell(entry.CellId))
                                    .FirstOrDefault(cell => cell.FarmCell && cell.Walkable);

            if (spawnCell == null)
            {
                Console.WriteLine("Error: No valid spawn cell found.");
                return false;
            }

            mount.AddPaddockMount(character, this, spawnCell.Id);

            if (mountSave != null)
            {
                mountSave.AddPaddockMount(character, this, spawnCell.Id);
            }

            return true;

        }

        #endregion

        #region >> Network
        public PaddockPropertiesMessage GetPaddockPropertiesMessage()
        {
            PaddockInstancesInformations _properties = new PaddockInstancesInformations(
                (ushort)MaxOutdoorMount,
                (ushort)MaxItems,
                this.IsPublicPaddock() ? new PaddockBuyableInformations[0] : new[] { new PaddockBuyableInformations(Price, Locked) });

            return new PaddockPropertiesMessage(_properties);
        }

        public PaddockPropertiesMessage GetPaddockPropertiesGuildMessage()
        {
            PaddockInstancesInformations _properties = new PaddockInstancesInformations(
                (ushort)MaxOutdoorMount,
                (ushort)MaxItems,
                this.IsPublicPaddock() ? new PaddockBuyableInformations[0] : new[] { new PaddockGuildedInformations(Price, Locked, Abandonned, Guild.GetGuildInformations()) });

            return new PaddockPropertiesMessage(_properties);
        }

        public PaddockContentInformations GetPaddockContentInformations()
        {
            //abandoned ? how?
            PaddockContentInformations properties = new PaddockContentInformations(
                (ushort)MaxOutdoorMount,
                (ushort)MaxItems,
                Id,
                (short)Map.Position.X,
                (short)Map.Position.Y,
                Map.Id,
                (ushort)Map.SubArea.Id,
                Abandonned,
                MountManager.Instance.GetMounts().Select(x => x.GetMountInformationsForPaddock()));

            return properties;
        }
        #endregion

        #region >> World Save

        //Salvando modificações na Paddock Particular de Guilds
        public void Save(ORM.Database database)
        {
            try
            {
                if (IsRecordDirty)
                {
                    database.Update(Record);
                    IsRecordDirty = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error saving Paddock: {ex.Message}");
            }
        }

        #endregion
    }
}