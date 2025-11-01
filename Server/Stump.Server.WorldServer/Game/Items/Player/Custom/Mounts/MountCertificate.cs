using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Database.Mounts;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using Stump.Server.WorldServer.Game.Effects.Instances;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    [ItemType(ItemTypeEnum.CERTIFICAT_DE_MULDO_196)]
    [ItemType(ItemTypeEnum.CERTIFICAT_DE_VOLKORNE_207)]
    [ItemType(ItemTypeEnum.CERTIFICAT_DE_DRAGODINDE_97)]
    public sealed class MountCertificate : BasePlayerItem
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private EffectMount m_mountEffect;
        private EffectString m_nameEffect;
        private EffectString m_belongsToEffect;
        private EffectDuration m_validityEffect;

        public int? MountId => (m_mountEffect ?? (m_mountEffect = Effects.OfType<EffectMount>().FirstOrDefault()))?.MountId;

        public MountCertificate(Character owner, PlayerItemRecord record) : base(owner, record)
        {
            if (Template.Id != MountTemplate.DEFAULT_SCROLL_ITEM) //Default template is used to apply montage effects
                Initialize();

            if (record.Stack > 1)
            {
                for (int i = 0; i < record.Stack - 1; i++)
                {
                    Owner.Inventory.AddItem(Template);
                }

                record.Stack = 1;
            }
        }

        public override uint Stack
        {
            get { return Math.Min(Record.Stack, 1); }
            set { Record.Stack = Math.Min(value, 1); }
        }

        public Mount Mount
        {
            get;
            private set;
        }

        private void Initialize()
        {
            if (Effects.Count > 0)
            {
                if (Effects.Any(x => x.Id == -1))
                    return;

                m_mountEffect = Effects.OfType<EffectMount>().FirstOrDefault();
                m_nameEffect = Effects.OfType<EffectString>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_Name);
                m_belongsToEffect = Effects.OfType<EffectString>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_BelongsTo);
                m_validityEffect = Effects.OfType<EffectDuration>().FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_Validity);

                if (m_mountEffect == null)
                {
                    logger.Error($"Invalid certificate mount effect absent");
                    CreateMount();
                    return;
                }

                if (m_mountEffect.Date < DateTime.Now - MountManager.MountStorageValidity) //Invalid certificate
                    return;

                var record = MountManager.Instance.GetMount(m_mountEffect.MountId);

                if (record == null) //If you turn off the server and do not save, it will be buggy
                {
                    logger.Error($"If you turn off the server and do not save, it will be buggy");
                    CreateMount();
                    return;
                }

                Mount = new Mount(Owner, record);
            }
            else
            {
                CreateMount();
            }
        }

        public void InitializeEffects(Mount mount)
        {
            if (Effects.Count > 0)
                Effects.Clear();

            m_mountEffect = new EffectMount(
                effectId: EffectsEnum.Effect_ViewMountCharacteristics,
                id: mount.Id,
                expirationDate: DateTimeOffset.Now.AddDays(MountManager.MountStorageValidityDays).ToUnixTimeMilliseconds(),
                model: (uint)mount.Template.Id,
                name: mount.Name,
                owner: mount.OwnerName,
                level: mount.Level,
                sex: mount.Sex,
                isRideable: true,
                isFeconded: false,
                isFecondationReady: false,
                reproductionCount: this.GetNewReproductionCount(mount),
                reproductionCountMax: (uint)mount.ReproductionCountMax,
                effects: mount.Effects.ToList(),
                capacities: new List<uint>());

            Effects.Add(m_mountEffect);

            mount.Owner = Owner;

            if (mount.Owner != null)
            {
                Effects.Add(m_belongsToEffect = new EffectString(EffectsEnum.Effect_BelongsTo, mount.Owner.Name));
            }

            Mount = mount;
            mount.StoredSince = DateTime.Now;
        }

        private void CreateMount()
        {
            var template = MountManager.Instance.GetTemplateByCertificateId(Template.Id);

            if (template == null)
            {
                logger.Error($"Cannot generate mount associated to scroll {Template.Name} ({Template.Id}) in owner {Owner.Namedefault} there is no matching mount template");
                Owner.Inventory.RemoveItem(this);
                return;
            }

            Mount mount = MountManager.Instance.CreateMount(Owner, template);
            MountManager.Instance.SaveMount(mount.Record);

            InitializeEffects(mount);
        }

        public bool CanConvert()
        {
            return Mount != null && m_mountEffect != null && m_mountEffect.Date + MountManager.MountStorageValidity > DateTime.Now;
        }

        public override ObjectItem GetObjectItem()
        {
            if (m_validityEffect != null && m_mountEffect != null)
            {
                var validity = m_mountEffect.Date + MountManager.MountStorageValidity - DateTime.Now;

                m_validityEffect.Update(validity > TimeSpan.Zero ? validity : TimeSpan.Zero);
            }

            return base.GetObjectItem();
        }

        public override void OnPersistantItemAdded()
        {
            if (Mount != null)
            {
                MountManager.Instance.SaveMount(Mount.Record);
            }
        }

        public override bool OnRemoveItem()
        {
            if (Mount != null)
                Mount.StoredSince = null;

            return base.OnRemoveItem();
        }

        private int GetNewReproductionCount(Mount mount)
        {
            if (mount.Template.Id == 88 || mount.Template.Id == 89)
            {
                return -1;
            }

            return mount.ReproductionCount;
        }
    }
}