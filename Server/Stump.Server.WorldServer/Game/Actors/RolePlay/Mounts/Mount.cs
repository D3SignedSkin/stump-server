using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.Mounts;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Handlers.Items;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Items.Player.Custom;
using Stump.Server.WorldServer.Game.Maps.Paddocks;
using Stump.Server.WorldServer.Game.Maps.Pathfinding;
using Stump.Server.WorldServer.Handlers.Context;
using Stump.Server.WorldServer.Handlers.Mounts;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts
{
    public class Mount : NamedActor, IContextDependant, IAutoMovedEntity
    {
        private static new readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly double[][] XP_PER_GAP =
        {
            new double[] {0, 10},
            new double[] {10, 8},
            new double[] {20, 6},
            new double[] {30, 4},
            new double[] {40, 3},
            new double[] {50, 2},
            new double[] {60, 1.5},
            new double[] {70, 1}
        };

        [Variable(true)]
        public static int RequiredLevel = 60;

        #region >> Properties

        #region >> Properties World

        public MountRecord Record
        {
            get;
        }

        public Character Owner
        {
            get { return m_owner = m_owner is null ? World.Instance.GetCharacter(this.OwnerId) : m_owner; }

            set
            {
                m_owner = value;
            }
        }

        public Paddock Paddock
        {
            get { return m_paddock; }
            set
            {
                m_paddock = value;
            }
        }
        #endregion

        public int Id
        {
            get { return Record.Id; }
            private set { Record.Id = value; }
        }

        public int OwnerId
        {
            get { return (int)Record.OwnerId; }
            set { Record.OwnerId = value; }
        }

        public string OwnerName
        {
            get { return Record.OwnerName; }
            set
            {
                Record.OwnerName = value;
            }
        }

        public string Name
        {
            get { return Record.Name; }
            private set
            {
                Record.Name = value;
            }
        }

        public int TemplateId
        {
            get { return Record.TemplateId; }
        }

        public ushort Level
        {
            get;
            protected set;
        }

        public DateTime? StoredSince
        {
            get { return Record.StoredSince; }
            set
            {
                Record.StoredSince = value;
            }
        }

        public int? PaddockId
        {
            get { return Record.PaddockId; }
            protected set
            {
                Record.PaddockId = value;
            }
        }

        public long? PaddockMapId
        {
            get { return Record.PaddockMapId; }
            set
            {
                Record.PaddockMapId = value;
            }
        }

        public int PaddockCellId
        {
            get { return Record.PaddockCellId; }
            set
            {
                Record.PaddockCellId = value;
            }
        }

        public int PaddockMountDirection
        {
            get { return Record.PaddockMountDirection; }
            set
            {
                Record.PaddockMountDirection = value;
            }
        }

        public bool IsInStable
        {
            get { return Record.IsInStable; }
            set
            {
                Record.IsInStable = value;
            }
        }

        public bool IsEquipped
        {
            get { return Record.IsEquipped; }
            set
            {
                Record.IsEquipped = value;
            }
        }

        #region >> Mount Status

        public bool IsFecondationReady
        {
            get { return Record.IsFecondationReady; }
            private set
            {
                Record.IsFecondationReady = value;
            }
        }

        public bool Sex
        {
            get { return Record.Sex; }
            private set
            {
                Record.Sex = value;
            }
        }

        public long Experience // TODO Error certificado
        {
            get { return Record.Experience; }
            protected set
            {
                Record.Experience = value;
            }
        }

        public long ExperienceLevelFloor
        {
            get;
            protected set;
        }

        public long ExperienceNextLevelFloor
        {
            get;
            protected set;
        }

        public sbyte GivenExperience
        {
            get { return Record.GivenExperience; }
            protected set
            {
                Record.GivenExperience = value;
            }
        }

        public int Stamina
        {
            get { return Record.Stamina; }
            protected set
            {
                Record.Stamina = value;
            }
        }

        public int StaminaMax
        {
            get { return 10000; }
        }

        public int Maturity
        {
            get { return Record.Maturity; }
            protected set
            {
                Record.Maturity = value;
            }
        }

        public int Energy
        {
            get { return Record.Energy; }
            protected set
            {
                Record.Energy = value;
            }
        }

        public int EnergyMax
        {
            get { return 7400; }
        }

        public int Serenity
        {
            get { return Record.Serenity; }
            protected set
            {
                Record.Serenity = value;
            }
        }

        public int SerenityMax
        {
            get { return 10000; }
        }


        public int ReproductionCount
        {
            get { return Record.ReproductionCount; }
            protected set
            {
                Record.ReproductionCount = value;
            }
        }

        public int ReproductionCountMax
        {
            get { return 80; }
        }

        public int Love
        {
            get { return Record.Love; }
            protected set
            {
                Record.Love = value;
            }
        }

        public int LoveMax
        {
            get { return 10000; }
        }

        public int AggressivityMax
        {
            get { return -10000; }
        }

        public int PodsMax
        {
            get { return Record.Template.PodsBase + Record.Template.PodsPerLevel * Level; }
        }

        public int MaturityForAdult
        {
            get { return 10000; }
        }

        public int FecondationTime
        {
            get { return 0; }
        }

        public int Tiredness
        {
            get { return Record.Tiredness; }
            protected set
            {
                Record.Tiredness = value;
                IsUpdate = true;
            }
        }

        public int MaxTired
        {
            get { return 1000; }
        }

        #endregion

        public bool UseHarnessColors
        {
            get { return Owner?.Record.UseHarnessColor ?? false; }
            set
            {
                if (Owner != null)
                {
                    Owner.Record.UseHarnessColor = value;
                    Owner.UpdateLook();
                    RefreshMount();
                }
            }
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

        #region >> Inventory
        public MountInventory Inventory
        {
            get;
            set;
        }
        #endregion Inventory

        #region >> Look
        public HarnessItem Harness => Owner?.Inventory.TryGetItem(CharacterInventoryPositionEnum.ACCESSORY_POSITION_RIDE_HARNESS) as HarnessItem;

        public ActorLook Look
        {
            get
            {
                var result = new ActorLook();

                try
                {
                    var look = Template.EntityLook.Clone();
                    var harness = Harness;

                    if (harness != null)
                        look.AddSkin((short)harness.HarnessTemplate.SkinId);


                    if (harness != null && UseHarnessColors)
                    {
                        look.SetColors(harness.HarnessTemplate.Colors);
                    }
                    else if (Behaviors.Contains((int)MountBehaviorEnum.Caméléone) && Owner != null)
                    {
                        Color color1;
                        Color color2;
                        Color color3;

                        if (Owner.DefaultLook.Colors.TryGetValue(3, out color1) &&
                            Owner.DefaultLook.Colors.TryGetValue(4, out color2) &&
                            Owner.DefaultLook.Colors.TryGetValue(5, out color3))
                            look.SetColors(color1, color2, color3);
                    }

                    result = look;
                }
                catch
                { }

                return result;
            }
        }
        #endregion

        private List<EffectInteger> m_effects;
        private Paddock m_paddock;
        private Character m_owner;

        public ReadOnlyCollection<EffectInteger> Effects => m_effects.AsReadOnly();

        public ReadOnlyCollection<int> Ancestors => Record.Ancestors.AsReadOnly();

        public ReadOnlyCollection<int> Behaviors => Record.Behaviors.AsReadOnly();

        public MountTemplate Template => Record.Template;

        public ItemTemplate certificateItem => Template.certificateItem;

        #endregion

        #region >> Constructions
        public Mount(Character character, MountRecord record)
        {
            Record = record;
            Level = ExperienceManager.Instance.GetMountLevel(Experience);
            ExperienceLevelFloor = ExperienceManager.Instance.GetMountLevelExperience(Level);
            ExperienceNextLevelFloor = ExperienceManager.Instance.GetMountNextLevelExperience(Level);

            if (Record.PaddockId != null)
            {
                Paddock = PaddockManager.Instance.GetPaddock(Record.PaddockId.Value);
            }

            m_effects = MountManager.Instance.GetMountEffects(this);

            Owner = character;
        }

        public Mount(MountRecord record)
        {
            Record = record;
            Level = ExperienceManager.Instance.GetMountLevel(Experience);
            ExperienceLevelFloor = ExperienceManager.Instance.GetMountLevelExperience(Level);
            ExperienceNextLevelFloor = ExperienceManager.Instance.GetMountNextLevelExperience(Level);

            if (Record.PaddockId != null)
            {
                Paddock = PaddockManager.Instance.GetPaddock(Record.PaddockId.Value);
            }

            m_effects = MountManager.Instance.GetMountEffects(this);
        }
        #endregion

        #region >> Paddock Manager

        public bool RemoveMountFromPaddock()
        {
            if (this.Paddock != null && !this.IsInStable)
            {
                this.PaddockId = null;
                this.PaddockMapId = 0;
                this.PaddockCellId = 0;
                this.PaddockMountDirection = 0;
                this.IsUpdate = true;

                return true;
            }
            else
            {
                Console.WriteLine("Error ao remover montaria.");
                return false;
            }
        }

        public void AddPaddockMount(Character character, Paddock paddock, short spawnCellId)
        {
            this.OwnerName = character.NameClean;
            this.Record.Map = character.Position.Map;
            this.Paddock = paddock;
            this.PaddockId = paddock.Id;
            this.PaddockMapId = paddock.Map.Id;
            this.PaddockCellId = spawnCellId;
            this.IsInStable = false;
            this.IsUpdate = true;
        }

        public void AddStabledMount()
        {
            this.IsInStable = true;
            this.PaddockId = null;
            this.PaddockMapId = 0;
            this.PaddockCellId = 0;
            this.IsUpdate = true;
        }

        public void RemoveStabledMount()
        {
            this.IsInStable = false;
            this.PaddockId = null;
            this.PaddockMapId = 0;
            this.PaddockCellId = 0;
            this.IsUpdate = true;
        }

        public void setMountOwner(Character character)
        {
            this.Owner = character;
            this.OwnerId = character.Id;
            this.OwnerName = character.NameClean;
            this.IsUpdate = true;
        }

        public void SetMountEquipped()
        {
            this.IsEquipped = true;
            this.IsUpdate = true;
        }

        public void RemoveMountEquipped()
        {
            this.IsEquipped = false;
            this.IsUpdate = true;
        }

        public void setNewMount()
        {
            this.IsNew = true;
        }

        public void setUpdateMount()
        {
            this.IsUpdate = true;
        }

        #region // ----------------- Breeding Items By:Kenshin ---------------- //

        public void setStamina(int quantity)
        {
            this.Stamina += quantity;
            this.IsUpdate = true;
        }

        public void setMaturity(int quantity)
        {
            this.Maturity += quantity;
            this.IsUpdate = true;
        }

        public void setEnergy(int quantity)
        {
            this.Energy += quantity;
            this.IsUpdate = true;
        }

        public void setSerenity(int quantity)
        {
            this.Serenity += quantity;
            this.IsUpdate = true;
        }

        public void setLove(int quantity)
        {
            this.Love += quantity;
            this.IsUpdate = true;
        }

        public void setTiredness(int quantity)
        {
            this.Tiredness += quantity;
            this.IsUpdate = true;
        }

        public void setAggressivity(int quantity)
        {
            //this.AggressivityMax += quantity;
        }

        #endregion

        #endregion

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

        #region >> Handlers
        public void ApplyMountEffects(bool send = true)
        {
            if (Owner == null)
                return;

            // dummy item
            var item = ItemManager.Instance.CreatePlayerItem(Owner, MountTemplate.DEFAULT_SCROLL_ITEM, 1);
            item.Effects.AddRange(Effects);

            Owner.Inventory.ApplyItemEffects(item, send, ItemEffectHandler.HandlerOperation.APPLY);
        }

        public void UnApplyMountEffects()
        {
            if (Owner == null)
                return;

            // dummy item
            var item = ItemManager.Instance.CreatePlayerItem(Owner, MountTemplate.DEFAULT_SCROLL_ITEM, 1);
            item.Effects.AddRange(Effects);

            Owner.Inventory.ApplyItemEffects(item);
        }

        public void RenameMount(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || Owner == null)
                return;

            this.IsUpdate = true;
            Name = name.EscapeString();

            MountHandler.SendMountRenamedMessage(Owner.Client, Id, name);
        }

        public void Sterelize(Character character)
        {
            this.IsUpdate = true;
            character.EquippedMount.ReproductionCount = -1;
            MountHandler.SendMountSterelizeMessage(character.Client, character.EquippedMount.Id);
        }

        public void SetGivenExperience(Character character, sbyte xp)
        {
            this.IsUpdate = true;
            GivenExperience = xp > 90 ? (sbyte)90 : (xp < 0 ? (sbyte)0 : xp);

            MountHandler.SendMountXpRatioMessage(character.Client, GivenExperience);
        }

        public void AddXP(Character character, long experience)
        {
            Experience += experience;

            var level = ExperienceManager.Instance.GetMountLevel(Experience);

            if (level == Level)
                return;

            Level = level;
            this.IsUpdate = true;
            OnLevelChanged(character);
        }

        public void RefreshMount()
        {
            MountHandler.SendMountSetMessage(Owner.Client, GetMountClientData());
        }

        #region // ----------------- Adicionar Behavior By:Kenshin ---------------- //
        public void AddBehavior(MountBehaviorEnum behavior)
        {
            var behaviors = Record.Behaviors.ToList();
            behaviors.Add((int)behavior);

            Record.Behaviors = behaviors;

            IsUpdate = true;
        }
        #endregion

        #region // ----------------- Remover Behavior By:Kenshin ---------------- //
        public void RemoveBehavior(MountBehaviorEnum behavior)
        {
            var behaviors = Record.Behaviors.ToList();
            behaviors.Remove((int)behavior);

            Record.Behaviors = behaviors;

            IsUpdate = true;
        }
        #endregion

        protected virtual void OnLevelChanged(Character character)
        {
            ExperienceLevelFloor = ExperienceManager.Instance.GetMountLevelExperience(Level);
            ExperienceNextLevelFloor = ExperienceManager.Instance.GetMountNextLevelExperience(Level);

            UnApplyMountEffects();
            m_effects = MountManager.Instance.GetMountEffects(this);
            ApplyMountEffects();

            MountHandler.SendMountSetMessage(character.Client, GetMountClientData());
        }

        public long AdjustGivenExperience(Character giver, long amount)
        {
            var gap = giver.Level - Level;

            for (var i = XP_PER_GAP.Length - 1; i >= 0; i--)
            {
                if (gap > XP_PER_GAP[i][0])
                    return (long)(amount * XP_PER_GAP[i][1] * 0.01);
            }

            return (long)(amount * XP_PER_GAP[0][1] * 0.01);
        }
        #endregion

        #region >> Network

        public MountClientData GetMountClientData()
        {
            return new MountClientData
            {
                sex = Sex,
                isRideable = true,
                isWild = false,
                isFecondationReady = false,
                id = Id,
                model = (uint)Template.Id,
                ancestor = Ancestors,
                behaviors = Behaviors,
                name = Name,
                ownerId = Record.OwnerId ?? -1,
                experience = (ulong)Experience,
                experienceForLevel = (ulong)ExperienceLevelFloor,
                experienceForNextLevel = ExperienceNextLevelFloor,
                level = (sbyte)Level,
                maxPods = (uint)PodsMax,
                stamina = (uint)Stamina,
                staminaMax = (uint)StaminaMax,
                maturity = (uint)Maturity,
                maturityForAdult = (uint)MaturityForAdult,
                energy = (uint)Energy,
                energyMax = (uint)EnergyMax,
                serenity = Serenity,
                serenityMax = (uint)SerenityMax,
                aggressivityMax = AggressivityMax,
                love = (uint)Love,
                loveMax = (uint)LoveMax,
                fecondationTime = FecondationTime,
                boostLimiter = Tiredness,
                boostMax = 1000,
                reproductionCount = ReproductionCount,
                reproductionCountMax = (uint)ReproductionCountMax,
                effectList = Effects.Select(x => x.GetObjectEffect() as ObjectEffectInteger).Concat(Harness != null ? new[] { new EffectInteger(EffectsEnum.Effect_HarnessGID, (short)Harness.Template.Id).GetObjectEffect() as ObjectEffectInteger } : new ObjectEffectInteger[0]),
                harnessGID = (ushort)(Harness?.Template.Id ?? 0),
                useHarnessColors = UseHarnessColors,
            };
        }

        public MountInformationsForPaddock GetMountInformationsForPaddock() => new MountInformationsForPaddock((byte)TemplateId, Name, Record.OwnerName);
        #endregion

        #region >> World Save
        public void Save(ORM.Database database)
        {
            try
            {
                WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
                {
                    if (this.IsNew)
                    {
                        database.Insert(this.Record);
                    }
                    else if (this.IsUpdate)
                    {
                        database.Update(this.Record);
                    }

                    this.IsNew = false;
                    this.IsUpdate = false;
                });
            }
            catch (Exception ex)
            {
                logger.Error($"Error saving Character Mount (ID: {this.Record.Id}): {ex.Message}", ex);
            }
        }
        #endregion
    }
}