using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Stump.Core.Attributes;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs;
using Stump.Server.WorldServer.Game.Dialogs.TaxCollector;
using Stump.Server.WorldServer.Game.Exchanges.TaxCollector;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Guilds;
using Stump.Server.WorldServer.Game.Items.TaxCollector;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Pathfinding;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.TaxCollectors
{
    public class TaxCollectorNpc : NamedActor, IInteractNpc, IContextDependant, IAutoMovedEntity
    {
        [Variable]
        public static int BaseAP = 6;

        [Variable]
        public static int BaseMP = 5;

        [Variable]
        public static int BaseResistance = 25;

        [Variable]
        public static int MaxGatheredXPTotal = 2000000;

        [Variable]
        public static int MaxGatheredXPFight = 150000;

        [Variable]
        public static int MaxTaxCollectorsPercentPerArea = 25;

        public const int TAXCOLLECTOR_BONES = 714;
        public const int TAXCOLLECTOR_BONES_KATULU = 3638;

        readonly WorldMapTaxCollectorRecord m_record;
        readonly List<IDialog> m_openedDialogs = new List<IDialog>();
        string m_name;
        ActorLook m_look;
        readonly int m_contextId;
        private readonly int CallerId;

        /// <summary>
        /// Create a new tax collector with a new record (no IO)
        /// </summary>
        public TaxCollectorNpc(int globalId, int contextId, ObjectPosition position, Guild guild, Character caller) //Version 2.61 by Kenshin
        {
            m_contextId = contextId;
            Position = position;
            Guild = guild;
            Bag = new TaxCollectorBag(this);
            CallerId = caller.Id;
            m_record = new WorldMapTaxCollectorRecord
            {
                Id = globalId,
                Map = Position.Map,
                Cell = Position.Cell.Id,
                Direction = (int)Position.Direction,
                FirstNameId = (short)TaxCollectorManager.Instance.GetRandomTaxCollectorFirstname(),
                LastNameId = (short)TaxCollectorManager.Instance.GetRandomTaxCollectorName(),
                GuildId = guild.Id,
                CallerName = caller.Name,
                CallerId = caller.Id,
                Date = DateTime.Now
            };

            IsRecordDirty = true;
        }

        /// <summary>
        /// Create and load the tax collector (IO)
        /// </summary>
        public TaxCollectorNpc(WorldMapTaxCollectorRecord record, int contextId)
        {
            m_record = record;
            m_contextId = contextId;
            Bag = new TaxCollectorBag(this);

            if (record.MapId == null)
                throw new Exception("TaxCollector's map not found");

            Position = new ObjectPosition(
                record.Map,
                record.Map.Cells[m_record.Cell],
                (DirectionsEnum)m_record.Direction);


            Guild = GuildManager.Instance.TryGetGuild(Record.GuildId);
            LoadRecord();
        }

        #region Properties

        public WorldMapTaxCollectorRecord Record
        {
            get
            {
                return m_record;
            }
        }

        public ReadOnlyCollection<IDialog> OpenDialogs
        {
            get { return m_openedDialogs.AsReadOnly(); }
        }

        /// <summary>
        /// Context id
        /// </summary>
        public override int Id
        {
            get
            {
                return m_contextId;
            }
        }

        /// <summary>
        /// Unique id among all tax collectors
        /// </summary>
        public int GlobalId
        {
            get { return m_record.Id; }
            protected set { m_record.Id = value; }
        }

        public override string Name
        {
            get
            {
                return m_name ?? (m_name = string.Format("{0} {1}", TaxCollectorManager.Instance.GetTaxCollectorFirstName(FirstNameId), TaxCollectorManager.Instance.GetTaxCollectorName(LastNameId)));
            }
        }

        public ushort Level
        {
            get { return Guild.Level; }
        }

        public Guild Guild
        {
            get;
            protected set;
        }

        public TaxCollectorBag Bag
        {
            get;
            protected set;
        }

        public override ActorLook Look
        {
            get
            {
                return m_look ?? RefreshLook();
            }
        }

        public short FirstNameId
        {
            get { return m_record.FirstNameId; }
            protected set
            {
                m_record.FirstNameId = value;
                m_name = null;
            }
        }

        public short LastNameId
        {
            get { return m_record.LastNameId; }
            protected set { m_record.LastNameId = value; m_name = null; }
        }

        public int GatheredExperience
        {
            get { return m_record.GatheredExperience; }
            set
            {
                m_record.GatheredExperience = value;
                IsRecordDirty = true;
            }
        }

        public long GatheredKamas
        {
            get { return m_record.GatheredKamas; }
            set
            {
                m_record.GatheredKamas = value;
                IsRecordDirty = true;
            }
        }

        public int AttacksCount
        {
            get { return m_record.AttacksCount; }
            set
            {
                m_record.AttacksCount = value;
                IsRecordDirty = true;
            }
        }

        public TaxCollectorFighter Fighter
        {
            get;
            private set;
        }

        public bool IsFighting
        {
            get
            {
                return Fighter != null;
            }
        }

        public TaxCollectorStateEnum GetState()
        {
            if (IsFighting)
            {
                if (Fighter.Fight.State != FightState.Fighting)
                    return TaxCollectorStateEnum.STATE_WAITING_FOR_HELP;

                return TaxCollectorStateEnum.STATE_FIGHTING;
            }

            return TaxCollectorStateEnum.STATE_COLLECTING;
        }
        #endregion

        #region Look

        public ActorLook RefreshLook()
        {
            short Bones = TAXCOLLECTOR_BONES;

            if (Map.Area.Id == 63)
            {
                Bones = TAXCOLLECTOR_BONES_KATULU;
            }

            m_look = new ActorLook { BonesID = Bones };

            if (Guild.Emblem.Template != null)
                m_look.AddSkin((short)Guild.Emblem.Template.SkinId);
            m_look.AddColor(8, Guild.Emblem.SymbolColor);
            m_look.AddColor(7, Guild.Emblem.BackgroundColor);

            return m_look;
        }

        #endregion

        #region Dialogs

        public void InteractWith(NpcActionTypeEnum actionType, Character dialoguer)
        {
            if (!CanInteractWith(actionType, dialoguer))
                return;

            var dialog = new TaxCollectorInfoDialog(dialoguer, this);
            dialog.Open();
        }

        public bool CanInteractWith(NpcActionTypeEnum action, Character dialoguer)
        {
            return CanBeSee(dialoguer) && action == NpcActionTypeEnum.ACTION_TALK;
        }

        public void OnDialogOpened(IDialog dialog)
        {
            m_openedDialogs.Add(dialog);
        }

        public void OnDialogClosed(IDialog dialog)
        {
            m_openedDialogs.Remove(dialog);
        }

        public void CloseAllDialogs()
        {
            foreach (var dialog in OpenDialogs.ToArray())
            {
                dialog.Close();
            }

            m_openedDialogs.Clear();
        }

        #endregion

        #region Movement

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
            if (!CanMove() || movementPath.IsEmpty())
                return false;

            Position = movementPath.EndPathPosition;
            var keys = movementPath.GetServerPathKeys();

            Map.ForEach(entry => ContextHandler.SendGameMapMovementMessage(entry.Client, keys, this));

            StopMove();
            LastMoveDate = DateTime.Now;

            return true;
        }

        #endregion

        #region Fight
        public TaxCollectorFighter CreateFighter(FightTeam team)
        {
            if (IsFighting)
                throw new Exception("Tax collector is already fighting !");

            Fighter = new TaxCollectorFighter(team, this);

            Map.Refresh(this); // get invisible
            CloseAllDialogs();

            return Fighter;
        }

        public void RejoinMap()
        {
            if (!IsFighting)
                return;

            Fighter = null;

            Map.Refresh(this);
            AttacksCount++;
        }

        public override bool CanBeSee(Maps.WorldObject byObj)
        {
            return base.CanBeSee(byObj) && !IsFighting;
        }
        public bool CanGatherLoots()
        {
            return !IsFighting;
        }

        #endregion

        #region Database

        public bool IsRecordDirty
        {
            get;
            private set;
        }

        public void LoadRecord()
        {
            Bag.LoadRecord();
        }

        public void Save()
        {
            try
            {
                WorldServer.Instance.IOTaskPool.EnsureContext();

                if (Bag != null && Bag.IsDirty)
                    Bag.Save(GuildManager.Instance.Database);

                if (m_record != null)
                {
                    WorldServer.Instance.IOTaskPool.AddMessage(() => WorldServer.Instance.DBAccessor.Database.Update(m_record));
                }

                Console.WriteLine("Save operation completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during the save operation: {ex.Message}");
            }
        }

        #endregion

        public bool IsBagEmpty()
        {
            return Bag.Count == 0;
        }

        public bool IsTaxCollectorOwner(Guilds.GuildMember member)
        {
            return member.Guild.Id == m_record.GuildId;
        }

        public bool IsBusy()
        {
            return OpenDialogs.Any(x => x is TaxCollectorExchange);
        }

        protected override void OnDisposed()
        {
            CloseAllDialogs();
            Guild.RemoveTaxCollector(this);
            base.OnDisposed();
        }

        #region Network

        public override GameContextActorInformations GetGameContextActorInformations(Character character)
        {
            return new GameRolePlayTaxCollectorInformations(
                contextualId: Id,
                look: Look.GetEntityLook(),
                disposition: GetEntityDispositionInformations(),
                identification: GetTaxCollectorStaticInformations(),
                taxCollectorAttack: character == null || character.CanAttack(this) == FighterRefusedReasonEnum.FIGHTER_ACCEPTED ? 0 : 1);
        }

        //public TaxCollectorInformations GetNetworkTaxCollector()
        //{
        //    return new TaxCollectorInformations(
        //        uniqueId: GlobalId,
        //        firstNameId: (ushort)FirstNameId,
        //        lastNameId: (ushort)LastNameId,
        //        allianceIdentity: new AllianceInformation(),
        //        additionalInfos: GetAdditionalTaxCollectorInformations(),
        //        worldX: (short)Position.Map.Position.X,
        //        worldY: (short)Position.Map.Position.Y,
        //        subAreaId: (ushort)Position.Map.SubArea.Id,
        //        state: (sbyte)GetState(),
        //        look: Look.GetEntityLook(),
        //        complements: GetTaxCollectorComplementaryInformations().ToArray(),
        //        characteristics: new CharacterCharacteristics(),
        //        equipments: new ObjectItem[0],
        //        spells: new TaxCollectorOrderedSpell[0]);
        //}

        public TaxCollectorStaticInformations GetTaxCollectorStaticInformations()
        {
            return new TaxCollectorStaticInformations(
                firstNameId: (ushort)FirstNameId,
                lastNameId: (ushort)LastNameId,
                allianceIdentity: Guild.Alliance.GetAllianceInformations(),
                callerId: (ulong)CallerId);
        }

        //public AdditionalTaxCollectorInformations GetAdditionalTaxCollectorInformations()
        //{
        //    return new AdditionalTaxCollectorInformations(Record.CallerName, Record.Date.GetUnixTimeStamp());
        //}

        //public IEnumerable<TaxCollectorComplementaryInformations> GetTaxCollectorComplementaryInformations()
        //{
        //    var informations = new List<TaxCollectorComplementaryInformations>();

        //    if (IsFighting && (Fighter.Fight is FightPvT))
        //    {
        //        var fight = Fighter.Fight as FightPvT;

        //        if (fight.State == FightState.Placement)
        //        {
        //            informations.Add(new TaxCollectorWaitingForHelpInformations(
        //                    new ProtectedEntityWaitingForHelpInfo(
        //                        (int)(fight.GetAttackersPlacementTimeLeft().TotalMilliseconds / 100),
        //                        (int)(FightPvT.GetDefendersWaitTimeForPlacement().TotalMilliseconds / 100),
        //                        (sbyte)fight.GetDefendersLeftSlot())));
        //        }

        //    }
        //    informations.Add(new TaxCollectorLootInformations((ulong)GatheredKamas, (ulong)GatheredExperience, (uint)Bag.BagWeight, (ulong)Bag.BagValue));

        //    return informations;
        //}

        public TaxCollectorBasicInformations GetTaxCollectorBasicInformations()
        {
            return new TaxCollectorBasicInformations((ushort)FirstNameId, (ushort)LastNameId, (short)Position.Map.Position.X, (short)Position.Map.Position.Y, Position.Map.Id, (ushort)Position.Map.SubArea.Id);
        }

        public StorageInventoryContentMessage GetStorageInventoryContent()
        {
            return new StorageInventoryContentMessage(Bag.Select(x => x.GetObjectItem()).ToArray(), (ulong)GatheredKamas);
        }

        #endregion
    }
}