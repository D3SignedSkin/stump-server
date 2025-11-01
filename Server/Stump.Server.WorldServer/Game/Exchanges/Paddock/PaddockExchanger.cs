using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Handlers.Inventory;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using Stump.Server.WorldServer.Game.Items.Player.Custom;
using MapPaddock = Stump.Server.WorldServer.Game.Maps.Paddocks.Paddock;
using Mount = Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts.Mount;

namespace Stump.Server.WorldServer.Game.Exchanges.Paddock
{
    public class PaddockExchanger : Exchanger
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        public PaddockExchanger(Character character, MapPaddock paddock, PaddockExchange paddockExchange, InteractiveObject interactiveObject) : base(paddockExchange)
        {
            Paddock = paddock;
            Character = character;
            InteractivePaddock = interactiveObject;
        }

        public Character Character
        {
            get;
        }

        public MapPaddock Paddock
        {
            get;
        }

        public InteractiveObject InteractivePaddock
        {
            get;
            private set;
        }

        #region >> Paddock Handlers

        public bool EquipToPaddock(int mountId) // Equipado para o Paddock
        {
            if (!Character.HasEquippedMount())
                return false;

            if (!HasMountRight(Character.EquippedMount))
                return false;

            if (Character.EquippedMount.Id != mountId)
                return false;

            if (!Character.IsMountInventoryEmpty())
                return false;

            Mount mount = Character.EquippedMount;
            Mount mountSave = MountManager.Instance.GetMount(entry => entry.Id == mount.Id);

            if (mountSave.OwnerId != Character.Id)
            {
                mountSave.setMountOwner(Character);
            }

            if (Paddock.AddMountToPaddock(Character, Character.EquippedMount, mountSave) && Character.UnEquipMount())
            {
                InventoryHandler.SendExchangeMountPaddockAddMessage(Character.Client, mount);
                return true;
            }

            Character.SendServerMessageLang(
                "Não foi possível realizar a ação no momento. Por favor, tente novamente mais tarde.",
                "Unable to perform action at this time. Please try again later.",
                "No se puede realizar la acción en este momento. Por favor, inténtelo de nuevo más tarde.",
                "Impossible d'effectuer l'action pour le moment. Veuillez réessayer plus tard.");

            log.Error($"Error EquipToPaddock - Mountid: {mountId} CharacterName: {Character.NameClean} MapId: {Character.Map.Id}");

            return false;
        }

        public bool EquipToStable(int mountId) // Equipado para o Estabulo
        {
            if (!Character.HasEquippedMount())
                return false;

            if (!HasMountRight(Character.EquippedMount))
                return false;

            if (Character.EquippedMount.Id != mountId)
                return false;

            if (!Character.IsMountInventoryEmpty())
                return false;

            Mount mount = Character.EquippedMount;
            Mount mountSave = MountManager.Instance.GetMount(entry => entry.Id == mount.Id);

            if (mountSave.OwnerId != Character.Id)
            {
                mountSave.setMountOwner(Character);
            }

            if (Character.UnEquipMount())
            {
                mountSave.AddStabledMount();
                InventoryHandler.SendExchangeMountStableAddMessage(Character.Client, mount);

                return true;
            }

            Character.SendServerMessageLang(
                "Não foi possível realizar a ação no momento. Por favor, tente novamente mais tarde.",
                "Unable to perform action at this time. Please try again later.",
                "No se puede realizar la acción en este momento. Por favor, inténtelo de nuevo más tarde.",
                "Impossible d'effectuer l'action pour le moment. Veuillez réessayer plus tard.");

            log.Error($"Error EquipToStable - Mountid: {mountId} CharacterName: {Character.NameClean} MapId: {Character.Map.Id}");

            return false;
        }

        public bool PaddockToEquip(int mountId) // Paddock para Equipar
        {
            if (Character.Level < Mount.RequiredLevel)
            {
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 227, Mount.RequiredLevel);
                return false;
            }

            Mount mount = MountManager.Instance.GetMountPaddock(mountId, this.Paddock.Id);

            if (mount == null)
                return false;

            if (!HasMountRight(mount, true))
                return false;

            if (mount.RemoveMountFromPaddock())
            {
                if (mount.OwnerId != Character.Id)
                {
                    mount.setMountOwner(Character);
                }

                Character.EquipMount(mount);
                InventoryHandler.SendExchangeMountPaddockRemoveMessage(Character.Client, mount);

                return true;
            }

            Character.SendServerMessageLang(
                "Não foi possível realizar a ação no momento. Por favor, tente novamente mais tarde.",
                "Unable to perform action at this time. Please try again later.",
                "No se puede realizar la acción en este momento. Por favor, inténtelo de nuevo más tarde.",
                "Impossible d'effectuer l'action pour le moment. Veuillez réessayer plus tard.");

            log.Error($"Error PaddockToEquip - Mountid: {mountId} CharacterName: {Character.NameClean} MapId: {Character.Map.Id}");

            return false;
        }

        public bool PaddockToStable(int mountId) // Paddock para o Estabulo
        {
            Mount mount = MountManager.Instance.GetMountPaddock(mountId, this.Paddock.Id);

            if (mount == null)
                return false;

            if (!HasMountRight(mount))
                return false;

            if (mount.OwnerId != Character.Id)
            {
                mount.setMountOwner(Character);
            }

            mount.RemoveMountFromPaddock();
            mount.AddStabledMount();

            InventoryHandler.SendExchangeMountStableAddMessage(Character.Client, mount);
            InventoryHandler.SendExchangeMountPaddockRemoveMessage(Character.Client, mount);

            return true;
        }

        public bool StableToPaddock(int mountId) // Estabulo para o Paddock
        {
            Mount mount = MountManager.Instance.GetMountStable(mountId);

            if (mount == null)
                return false;

            if (!HasMountRight(mount))
                return false;

            if (mount.OwnerId != Character.Id)
            {
                mount.setMountOwner(Character);
            }

            if (Paddock.AddMountToPaddock(Character, mount))
            {
                InventoryHandler.SendExchangeMountPaddockAddMessage(Character.Client, mount);
                InventoryHandler.SendExchangeMountStableRemoveMessage(Character.Client, mount);

                return true;
            }

            Character.SendServerMessageLang(
                "Não foi possível realizar a ação no momento. Por favor, tente novamente mais tarde.",
                "Unable to perform action at this time. Please try again later.",
                "No se puede realizar la acción en este momento. Por favor, inténtelo de nuevo más tarde.",
                "Impossible d'effectuer l'action pour le moment. Veuillez réessayer plus tard.");

            log.Error($"Error StableToPaddock - Mountid: {mountId} CharacterName: {Character.NameClean} MapId: {Character.Map.Id}");

            return false;
        }

        public bool StableToEquip(int mountId) // Estabulo para Equipar
        {
            Mount mount = MountManager.Instance.GetMountStable(mountId);

            if (Character.Level < Mount.RequiredLevel)
            {
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 227, Mount.RequiredLevel);
                return false;
            }

            if (mount == null)
                return false;

            if (!HasMountRight(mount, true))
                return false;

            if (mount.OwnerId != Character.Id)
            {
                mount.setMountOwner(Character);
            }

            mount.RemoveStabledMount();
            Character.EquipMount(mount);

            InventoryHandler.SendExchangeMountStableRemoveMessage(Character.Client, mount);

            return true;
        }

        public bool StableToInventory(int mountId) // Estabulo para o Inventario
        {
            Mount mount = MountManager.Instance.GetMountStable(mountId);

            if (mount == null)
                return false;

            if (!HasMountRight(mount))
                return false;

            if (mount.OwnerId != Character.Id)
            {
                mount.setMountOwner(Character);
            }

            mount.RemoveStabledMount();

            MountManager.Instance.StoreMount(Character, mount);
            InventoryHandler.SendExchangeMountStableRemoveMessage(Character.Client, mount);

            return true;
        }

        public bool PaddockToInventory(int mountId) // Paddock para o Inventario
        {
            Mount mount = MountManager.Instance.GetMountPaddock(mountId, this.Paddock.Id);

            if (mount == null)
                return false;

            if (!HasMountRight(mount))
                return false;

            if (mount.RemoveMountFromPaddock())
            {
                MountManager.Instance.StoreMount(Character, mount);
                InventoryHandler.SendExchangeMountPaddockRemoveMessage(Character.Client, mount);

                return true;
            }

            return false;
        }

        public bool EquipToInventory(int mountId) // Equipado para o Inventario
        {
            if (!Character.HasEquippedMount())
                return false;

            if (!HasMountRight(Character.EquippedMount))
                return false;

            if (Character.EquippedMount.Id != mountId)
                return false;

            if (!Character.IsMountInventoryEmpty())
                return false;

            Mount mount = Character.EquippedMount;

            if (Character.UnEquipMount())
            {
                if (mount.OwnerId != Character.Id)
                {
                    mount.setMountOwner(Character);
                }

                MountManager.Instance.StoreMount(Character, mount);
                return true;
            }

            return false;
        }

        public bool InventoryToStable(int itemId) // Inventario para o Estabulo
        {
            var item = Character.Inventory.TryGetItem(itemId) as MountCertificate;

            if (item == null || !item.CanConvert())
                return false;

            if (item.Mount == null)
                return false;

            if (!Character.Inventory.RemoveItem(item))
                return false;

            if (item.Mount.OwnerId != Character.Id)
            {
                item.Mount.setMountOwner(Character);
            }

            item.Mount.AddStabledMount();
            InventoryHandler.SendExchangeMountStableAddMessage(Character.Client, item.Mount);

            return true;
        }

        public bool InventoryToPaddock(int itemId) // Inventario para o Paddock
        {
            var item = Character.Inventory.TryGetItem(itemId) as MountCertificate;

            if (item == null || !item.CanConvert())
                return false;

            if (item.Mount == null)
                return false;

            if (Paddock.AddMountToPaddock(Character, item.Mount))
            {
                if (item.Mount.OwnerId != Character.Id)
                {
                    item.Mount.setMountOwner(Character);
                }

                Character.Inventory.RemoveItem(item);

                InventoryHandler.SendExchangeMountPaddockAddMessage(Character.Client, item.Mount);
                return true;
            }

            Character.SendServerMessageLang(
                "Não foi possível realizar a ação no momento. Por favor, tente novamente mais tarde e informe a equipe via Ticket Discord com um PrintScreen.",
                "Unable to perform action at this time. Please try again later and let the team know via Ticket Discord with a PrintScreen.",
                "No se puede realizar la acción en este momento. Inténtelo de nuevo más tarde e infórmeselo al equipo a través de Ticket Discord con una PrintScreen.",
                "Impossible d'effectuer l'action pour le moment. Veuillez réessayer plus tard et informer l'équipe via Ticket Discord avec un PrintScreen.");

            log.Error($"Error InventoryToPaddock - ItemId: {itemId} CharacterName: {Character.NameClean} MapId: {Character.Map.Id}");

            return false;
        }

        public bool InventoryToEquip(int itemId) // Inventario para Equipar
        {
            if (Character.HasEquippedMount())
                return false;

            if (Character.Level < Mount.RequiredLevel)
            {
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 227, Mount.RequiredLevel);
                return false;
            }

            var item = Character.Inventory.TryGetItem(itemId) as MountCertificate;

            if (item == null || !item.CanConvert())
                return false;

            if (item.Mount == null)
                return false;

            if (!Character.Inventory.RemoveItem(item))
                return false;

            if (item.Mount.OwnerId != Character.Id)
            {
                item.Mount.setMountOwner(Character);
            }

            Character.EquipMount(item.Mount);

            return true;
        }

        #endregion

        public bool HasMountRight(Mount mount, bool equip = false)
        {
            if (equip && Character.HasEquippedMount())
                return false;

            if (mount.Owner != null && Character != mount.Owner)
                return false;

            if (!equip || Character.Level >= Mount.RequiredLevel)
                return true;

            Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 227, Mount.RequiredLevel);

            return false;
        }

        public override bool MoveItem(int id, int quantity)
        {
            return false;
        }

        public override bool SetKamas(long amount)
        {
            return false;
        }
    }
}