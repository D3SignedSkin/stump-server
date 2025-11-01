using NLog;
using System;
using System.Linq;
using Stump.Core.Attributes;
using Stump.Core.Mathematics;
using Stump.DofusProtocol.Enums;
using System.Collections.Generic;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Mounts;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Items.Player.Custom;
using Stump.Server.WorldServer.Game.Maps.Paddocks;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts
{
    public class MountManager : DataManager<MountManager>, ISaveable
    {
        [Variable]
        public static int MountStorageValidityDays = 40;

        readonly object m_lock = new object();

        private Dictionary<int, MountTemplate> m_mountTemplates;
        private Dictionary<int, MountRecord> m_mounts;
        private Dictionary<int, HarnessRecord> m_harness;

        private List<MountRecord> m_mountsRecordDelete = new List<MountRecord>();

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static TimeSpan MountStorageValidity => TimeSpan.FromDays(MountStorageValidityDays);

        [Initialization(InitializationPass.Sixth)]
        public override void Initialize()
        {
            //Carrega os templates das montarias.
            m_mountTemplates = Database.Query<MountTemplate, MountBonus, MountTemplate>(new MountTemplateRelator().Map, MountTemplateRelator.FetchQuery).ToDictionary(entry => entry.Id);
            Database.Execute(string.Format(MountRecordRelator.DeleteStoredSince, (DateTime.Now - MountStorageValidity).ToString("yyyy-MM-dd HH:mm:ss.fff")));

            //Carrega as montarias dos personagens.
            m_mounts = Database.Query<MountRecord>(MountRecordRelator.FetchQuery).ToDictionary(x => x.Id);

            //Carrega as cores das montarias.
            m_harness = Database.Query<HarnessRecord>(HarnessRelator.FetchQuery).ToDictionary(x => x.ItemId);

            World.Instance.RegisterSaveableInstance(this);
        }

        #region >> Mount Manager

        public Mount CreateMount(Character owner, MountTemplate template)
        {
            var rand = new CryptoRandom();

            return CreateMount(owner, template, rand.Next(2) == 1);
        }

        public Mount CreateMount(Character owner, MountTemplate template, bool sex)
        {
            var record = new MountRecord()
            {
                TemplateId = template.Id,
                OwnerId = owner.Id,
                OwnerName = owner.NameClean,
                Name = "Anonyme",
                Sex = sex,
                Energy = template.EnergyBase,
            };

            record.AssignIdentifier();
            record.IsNew = true;

            AddMount(record);

            return new Mount(owner, record);
        }

        public BasePlayerItem StoreMount(Character character, Mount mount)
        {
            var item = ItemManager.Instance.CreatePlayerItem(character, mount.certificateItem, 1, new List<EffectBase> { new EffectBase(-1, new EffectBase()) }) as MountCertificate;

            if (item == null)
                throw new Exception($"Item {mount.certificateItem} type isn't MountCertificate");

            mount.IsUpdate = true;
            item.InitializeEffects(mount);
            return character.Inventory.AddItem(item);
        }

        public HarnessRecord GetHarness(int id)
        {
            return !m_harness.TryGetValue(id, out HarnessRecord record) ? null : record;
        }

        public MountTemplate[] GetTemplates()
        {
            return m_mountTemplates.Values.ToArray();
        }

        public MountTemplate GetTemplate(int id)
        {
            return !m_mountTemplates.TryGetValue(id, out var result) ? null : result;
        }

        public MountTemplate GetTemplateByCertificateId(int certificateId)
        {
            return m_mountTemplates.FirstOrDefault(x => x.Value.CertificateId == certificateId).Value;
        }

        public void AddMount(MountRecord record)
        {
            if (!m_mounts.ContainsKey(record.Id))
            {
                m_mounts.Add(record.Id, record);
            }
        }

        public void DeleteMount(MountRecord record)
        {
            m_mountsRecordDelete.Add(record);
            m_mounts.Remove(record.Id);
        }

        public MountRecord GetMount(int mountId)
        {
            if (!m_mounts.TryGetValue(mountId, out var record))
                return null;

            return record;
        }

        private static short GetBonusByLevel(int finalBonus, int level)
        {
            return (short)Math.Floor(finalBonus * level / 100d);
        }

        public List<EffectInteger> GetMountEffects(Mount mount)
        {
            return mount.Template.Bonuses.Select(x => new EffectInteger((EffectsEnum)x.EffectId, GetBonusByLevel(x.Amount, mount.Level))).ToList();
        }

        public Mount GetEquippedMountByMountId(int mountId)
        {
            if (m_mounts == null || m_mounts.Count == 0)
                return null;

            MountRecord mountEntry = m_mounts.FirstOrDefault(entry => entry.Value.Id == mountId && entry.Value.IsEquipped).Value;

            if (mountEntry == null)
                return null;

            return new Mount(mountEntry);
        }

        #endregion

        #region >> Paddock Manager

        public Mount GetMount(Predicate<Mount> predicate)
        {
            var mount = m_mounts.FirstOrDefault(entry => predicate(new Mount(entry.Value)));

            return mount.Equals(default(KeyValuePair<int, Mount>)) ? null : new Mount(mount.Value);
        }

        public List<Mount> GetMounts()
        {
            return m_mounts.Select(entry => new Mount(entry.Value)).ToList();
        }

        public List<Mount> GetMounts(Predicate<Mount> predicate)
        {
            return m_mounts.Where(entry => predicate(new Mount(entry.Value))).Select(entry => new Mount(entry.Value)).ToList();
        }

        public Mount GetMountStable(int mountId)
        {
            return GetMount(mount => mount.IsInStable && mount.Id == mountId);
        }

        public List<Mount> GetMountsStable(Character character)
        {
            return GetMounts(mount => mount.IsInStable && mount.OwnerId == character.Id);
        }

        public Mount GetMountPaddock(int mountId, int paddockId)
        {
            return GetMount(mount => mount.Id == mountId && mount.Paddock.Id == paddockId);
        }

        public List<Mount> GetPaddockMounts()
        {
            return GetMounts(mount => mount.PaddockId > 0 && !mount.IsInStable).ToList();
        }

        public List<Mount> GetPaddockMounts(Paddock paddock)
        {
            return GetMounts(mount => mount.PaddockId > 0 && !mount.IsInStable && mount.PaddockId == paddock.Id);
        }

        public List<Mount> GetPaddockMounts(Character character, Paddock paddock)
        {
            return GetMounts(mount => mount.PaddockId > 0 && !mount.IsInStable && mount.PaddockId == paddock.Id && mount.OwnerId == character.Id);
        }

        #endregion

        public void SaveMount(MountRecord record)
        {
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                #region >> Mount Save

                if (record.IsNew)
                {
                    Database.Insert(record);
                    record.IsNew = false;
                }
                else if (record.IsUpdate)
                {
                    Database.Update(record);
                    record.IsUpdate = false;
                }

                #endregion
            });
        }

        public void Save()
        {
            lock (m_lock)
            {
                WorldServer.Instance.IOTaskPool.AddMessage(() =>
                {
                    #region >> Mounts Save

                    var _saveMount = m_mounts.Where(entry => entry.Value.IsNew || entry.Value.IsUpdate).ToList();

                    foreach (var mount in _saveMount)
                    {
                        try
                        {
                            if (mount.Value.IsNew)
                            {
                                Database.Insert(mount.Value);
                            }
                            else if (mount.Value.IsUpdate)
                            {
                                Database.Update(mount.Value);
                            }

                            mount.Value.IsNew = false;
                            mount.Value.IsUpdate = false;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao salvar a montaria {mount.Value.Id}: {ex.Message}");
                        }
                    }

                    if (m_mountsRecordDelete.Count > 0)
                    {
                        foreach (var mount in m_mountsRecordDelete)
                        {
                            try
                            {
                                Database.Delete(mount);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Erro ao deletar a montaria {mount.Id}: {ex.Message}");
                            }
                        }

                        m_mountsRecordDelete.Clear();
                    }

                    #endregion
                });
            }
        }
    }
}