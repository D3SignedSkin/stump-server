using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    [ItemId(ItemIdEnum.POTION_DE_CHANGEMENT_DE_NOM_10860)]
    public class NameChangePotion : BasePlayerItem
    {
        public NameChangePotion(Character owner, PlayerItemRecord record) : base(owner, record)
        { }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {
            if ((Owner.Record.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME) == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME)
            {
                Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_POPUP, 43);
                return 0;
            }

            Owner.Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME;
            Owner.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_NAME;

            Owner.SendSystemMessage(41, false);

            return 1;
        }
    }

    [ItemId(ItemIdEnum.POTION_DE_CHANGEMENT_DES_COULEURS_10861)]
    public class ColourChangePotion : BasePlayerItem
    {
        public ColourChangePotion(Character owner, PlayerItemRecord record) : base(owner, record)
        { }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {
            if ((Owner.Record.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS) == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS)
            {
                Owner.SendSystemMessage(43, false);
                return 0;
            }

            Owner.Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS;
            Owner.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS;

            Owner.SendSystemMessage(42, false);

            return 1;
        }
    }

    [ItemId(ItemIdEnum.POTION_DE_CHANGEMENT_DE_VISAGE_13518)]
    public class LookChangePotion : BasePlayerItem
    {
        public LookChangePotion(Character owner, PlayerItemRecord record) : base(owner, record)
        { }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {
            if ((Owner.Record.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC) == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC)
            {
                Owner.SendSystemMessage(43, false);
                return 0;
            }

            Owner.Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC;
            Owner.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC;

            Owner.SendSystemMessage(58, false);

            return 1;
        }
    }

    [ItemId(ItemIdEnum.POTION_DE_CHANGEMENT_DE_SEXE_10862)]
    public class SexChangePotion : BasePlayerItem
    {
        public SexChangePotion(Character owner, PlayerItemRecord record) : base(owner, record)
        { }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {
            if ((Owner.Record.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER)
                == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER)
            {
                Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_POPUP, 43);
                Owner.SendSystemMessage(43, false);
                return 0;
            }

            Owner.Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER;
            Owner.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER;

            Owner.SendSystemMessage(44, false);

            return 1;
        }
    }

    [ItemId(ItemIdEnum.POTION_DE_CHANGEMENT_DE_CLASSE_16147)]
    public class ClassChangePotion : BasePlayerItem
    {
        public ClassChangePotion(Character owner, PlayerItemRecord record) : base(owner, record)
        { }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {
            if ((Owner.Record.MandatoryChanges & (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED) == (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED)
            {
                Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_POPUP, 43);
                Owner.SendSystemMessage(43, false);
                return 0;
            }

            Owner.Record.MandatoryChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED;
            Owner.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_BREED;
            Owner.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_GENDER;
            Owner.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COSMETIC;
            Owner.Record.PossibleChanges |= (sbyte)CharacterRemodelingEnum.CHARACTER_REMODELING_COLORS;

            Owner.SendSystemMessage(63, false);

            return 1;
        }
    }

    [ItemId(30005)] //Poção Camaleão
    public class PotionCameleon : BasePlayerItem
    {
        public PotionCameleon(Character owner, PlayerItemRecord record) : base(owner, record)
        { }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {
            if (Owner.EquippedMount != null)
            {
                if (!Owner.EquippedMount.Behaviors.Contains((int)MountBehaviorEnum.Caméléone))
                {
                    Owner.EquippedMount.AddBehavior(MountBehaviorEnum.Caméléone);
                    Owner.EquippedMount.Save(MountManager.Instance.Database);
                    Owner.UpdateLook();
                    Owner.EquippedMount.RefreshMount();
                    return 1;
                }
                else
                {

                    switch (Owner.Account.Lang)
                    {
                        case "fr":
                            Owner.OpenPopup("Vous ne pouvez pas utiliser cette potion, votre dragodinde est déjà caméléone !");
                            break;
                        case "es":
                            Owner.OpenPopup("¡No puedes usar esta poción, tu dragopavo ya es camaleón!");
                            break;
                        case "en":
                            Owner.OpenPopup("You can't use this potion, your dragoturkey is already chameleon!");
                            break;
                        default:
                            Owner.OpenPopup("Você não pode usar esta poção, seu dragossauro já é camaleão!");
                            break;
                    }
                    return 0;
                }
            }
            else
            {
                switch (Owner.Account.Lang)
                {
                    case "fr":
                        Owner.OpenPopup("Vous ne pouvez pas utiliser cette potion, vous n'avez pas de dragodinde équippée !");
                        break;
                    case "es":
                        Owner.OpenPopup("¡No puedes usar esta poción, no tienes un dragopavo equipado!");
                        break;
                    case "en":
                        Owner.OpenPopup("You cannot use this potion, you do not have a dragosaur equipped!");
                        break;
                    default:
                        Owner.OpenPopup("Você não pode usar esta poção, você não tem um dragossauro equipado!");
                        break;
                }

                return 0;
            }
        }
    }

    // Poção Auto Pilotada Mod By Kenshin
    [ItemId(ItemIdEnum.BOUTEILLE_POUSSIEREUSE_18412)]
    [ItemId(ItemIdEnum.POTION_AUTOPILOTEE_19349)]
    public class PotionAutoPiloto : BasePlayerItem
    {
        public PotionAutoPiloto(Character owner, PlayerItemRecord record) : base(owner, record)
        { }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {
            if (!Owner.HasEquippedMount() && Owner.EquippedMount == null)
            {
                Owner.SendServerMessageLang(
                    "Você não possui uma montaria equipada. Equipe uma montaria e utilize novamente a poção.",
                    "You don't have a mount equipped. Equip a mount and use the potion again.",
                    "No tienes una montura equipada. Equipa una montura y vuelve a usar la poción.",
                    "Vous n'avez pas de monture équipée. Équipez une monture et utilisez à nouveau la potion.");

                return 0;
            }

            if (!Owner.EquippedMount.Behaviors.Contains((int)MountBehaviorEnum.Poussiereuse))
            {
                Owner.EquippedMount.AddBehavior(MountBehaviorEnum.Poussiereuse);
                Owner.EquippedMount.Save(MountManager.Instance.Database);
                Owner.UpdateLook();
                Owner.EquippedMount.RefreshMount();

                return 1;
            }
            else
            {
                Owner.SendServerMessageLang(
                    "Você não pode usar esse feitiço em uma montaria que já possui o efeito Auto Pilotada.",
                    "You can't use this spell on a mount that already has the Self-Piloted effect.",
                    "No puedes usar este hechizo en una montura que ya tenga el efecto Autopilotado.",
                    "Vous ne pouvez pas utiliser ce sort sur une monture qui a déjà l'effet Autopilote.");

                return 0;
            }
        }
    }

    [ItemId(ItemIdEnum.ELIXIR_GROUPER_30351)]
    public class ElixirGrouperPotion : BasePlayerItem
    {
        public ElixirGrouperPotion(Character owner, PlayerItemRecord record) : base(owner, record)
        { }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {
            var character = Owner;

            foreach (var perso in WorldServer.Instance.FindClients(x => x.IP == character.Client.IP && x.Character != character))
            {
                if (character.Party != null && character.Party.Members.Contains(perso.Character))
                {
                    switch (Owner.Account.Lang)
                    {
                        case "fr":
                            Owner.OpenPopup("Vous ne pouvez pas utiliser cette potion, vous êtes dans un groupe ou vous n'avez plus de compte connecté.");
                            break;
                        case "es":
                            Owner.OpenPopup("No puedes usar esta poción, estás en un grupo o ya no tienes ninguna cuenta conectada.");
                            break;
                        case "en":
                            Owner.OpenPopup("You cannot use this potion, you are in a group or you no longer have any accounts logged in.");
                            break;
                        default:
                            Owner.OpenPopup("Você não pode usar esta poção, você está em um grupo ou não tem mais contas logadas.");
                            break;
                    }
                }
                else
                {

                    continue;
                }

                character.Invite(perso.Character, PartyTypeEnum.PARTY_TYPE_CLASSICAL, true);
            }

            if (Owner.Vip == true)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
