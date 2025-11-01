//using Stump.Core.Attributes;
//using Stump.Core.I18N;
//using Stump.DofusProtocol.Enums;
//using Stump.DofusProtocol.Types;
//using Stump.Server.WorldServer.Database.I18n;
//using Stump.Server.WorldServer.Game.Actors.Fight;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
//using Stump.Server.WorldServer.Game.Breach;
//using Stump.Server.WorldServer.Game.Fights;
//using Stump.Server.WorldServer.Game.Items;
//using Stump.Server.WorldServer.Game.Items.Player;
//using Stump.Server.WorldServer.Game.Parties;
//using Stump.Server.WorldServer.Handlers.Idols;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Stump.Server.WorldServer.Game.Idols
//{
//    public sealed class IdolInventory
//    {
//        [Variable]
//        const int MaxActiveIdols = 6;

//        public IdolInventory(Character owner)
//        {
//            Owner = owner;
//            ActiveIdols = owner.Record.Idols.Select(x => IdolManager.Instance.CreatePlayerIdol(Owner, x)).Where(x => x != null).ToList();

//            Owner.Inventory.ItemRemoved += OnInventoryItemRemoved;
//        }

//        public IdolInventory(Party party)
//        {
//            Party = party;
//            ActiveIdols = new List<PlayerIdol>();

//            BindEvents(party);
//        }

//        private void OnPartyDeleted(Party party)
//        {
//            UnBindEvents(party);
//        }

//        public Party Party
//        {
//            get;
//        }

//        public bool IsPartyIdols
//        {
//            get { return Party != null; }
//        }

//        public Character Owner
//        {
//            get;
//            private set;
//        }

//        private List<PlayerIdol> ActiveIdols
//        {
//            get;
//            set;
//        }

//        public bool HasIdol(int templateId) => ActiveIdols.Any(x => x.Template.Id == templateId);

//        public void Add(short idolId)
//        {
//            var owner = Owner;

//            if (IsPartyIdols)
//            {
//                var partyIdol = GetPartyIdol(idolId);

//                if (partyIdol == null)
//                    return;

//                owner = Party.GetMember((int)partyIdol.ownersIds.First());
//            }

//            var idol = IdolManager.Instance.CreatePlayerIdol(owner, idolId);

//            if (idol == null)
//                return;

//            if (HasIdol(idol.Id))
//                return;

//            if (ActiveIdols.Count >= MaxActiveIdols)
//                return;

//            if (!owner.Inventory.HasItem(idol.Template.IdolItem))
//                return;

//            ActiveIdols.Add(idol);
//            IdolHandler.SendIdolSelectedMessage(IsPartyIdols ? Party.Clients : Owner.Client, true, IsPartyIdols, (short)idol.Id);

//            if (!owner.IsInFight() || owner.Fight.State != FightState.Placement)
//                return;

//            IdolHandler.SendIdolFightPreparationUpdate(Owner.Fight.Clients, ActiveIdols.Select(x => x.GetNetworkIdol()));
//        }

//        public bool Remove(short idolId)
//        {
//            var idol = ActiveIdols.FirstOrDefault(x => x.Id == idolId);

//            if (idol == null)
//                return false;

//            return Remove(idol);
//        }

//        public bool Remove(PlayerIdol idol)
//        {
//            var result = ActiveIdols.Remove(idol);

//            if (!result)
//                return false;

//            IdolHandler.SendIdolSelectedMessage(IsPartyIdols ? Party.Clients : Owner.Client, false, IsPartyIdols, (short)idol.Id);

//            if (!Owner.IsInFight() || Owner.Fight.State != FightState.Placement)
//                return true;

//            IdolHandler.SendIdolFightPreparationUpdate(Owner.Fight.Clients, ActiveIdols.Select(x => x.GetNetworkIdol()));

//            return true;
//        }

//        public IEnumerable<PlayerIdol> GetIdols()
//        {
//            return ActiveIdols.ToArray();
//        }

//        public IEnumerable<PartyIdol> GetPartyIdols()
//        {
//            var idolItems = Party.Members.SelectMany(x => x.Inventory.GetItems(y => y.Template.TypeId == (uint)ItemTypeEnum.IDOLE)).ToArray();
//            var partyIdols = new Dictionary<int, PartyIdol>();

//            foreach (var idolItem in idolItems)
//            {
//                var template = IdolManager.Instance.GetTemplateByItemId(idolItem.Template.Id);
//                var ownerIds = new List<long>();

//                PartyIdol partyIdol;

//                if (partyIdols.TryGetValue(template.Id, out partyIdol))
//                {
//                    ownerIds.AddRange(partyIdol.ownersIds.Select(x => (long)x));
//                    partyIdols.Remove(partyIdol.id);
//                }

//                ownerIds.Add(idolItem.Owner.Id);
//                partyIdol = new PartyIdol((ushort)template.Id, (ushort)template.ExperienceBonus, (ushort)template.DropBonus, ownerIds.Select(x => (ulong)x).ToArray());
//                partyIdols.Add(template.Id, partyIdol);
//            }

//            return partyIdols.Values;
//        }

//        private bool CanUseIdol(PlayerIdol idol, FightPvM fight)
//        {
//            #region CanUseIdols
//            if (!idol.Template.IdolSpellId.HasValue)
//            {
//                return false;
//            }
//            else if (ActiveIdols.Count(x => x.Id == idol.Id) > 1)
//            {
//                return false;
//            }
//            else if (fight.MonsterTeam.Fighters.OfType<MonsterFighter>().Any(x => idol.Template.IncompatibleMonsters.Contains(x.Monster.Template.Id)))
//            {
//                Owner.SendServerMessageLang
//                    (
//                    "O ídolo " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.Portugese) + " foi desequipado por conter monstro incompatível na luta.",
//                    "The idol " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.English) + " has been unequipped because it contains an incompatible monster in the fight.",
//                    "El ídolo " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.Spanish) + " ha sido desequipado porque contiene un monstruo incompatible en la pelea.",
//                    "L'idole " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.French) + " a été déséquipée car elle contient un monstre incompatible dans le combat."
//                    );
//                return false;
//            }
//            else if (fight.MonsterTeam.Fighters.OfType<MonsterFighter>().Any(x => x.Monster.Template.AllIdolsDisabled))
//            {
//                Owner.SendServerMessageLang
//                    (
//                    "Os ídolos foram desequipados por conter monstros incompatíveis na luta.",
//                    "Idols have been unequipped for containing incompatible monsters in the fight.",
//                    "Los ídolos no han sido equipados para contener monstruos incompatibles en la lucha.",
//                    "Les idoles n'ont pas été équipées pour contenir des monstres incompatibles dans le combat."
//                    );
//                return false;
//            }
//            else if (idol.Template.GroupOnly && (fight.PlayerTeam.Fighters.Count < 4 || fight.MonsterTeam.Fighters.Count < 4))
//            {
//                Owner.SendServerMessageLang
//                    (
//                    "O ídolo " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.Portugese) + " foi desequipado por não conter a quantidade de monstros necessária.",
//                    "The idol " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.English) + " has been unequipped because it does not contain the required amount of monsters.",
//                    "El ídolo " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.English) + " ha sido desequipado porque no contiene la cantidad requerida de monstruos.",
//                    "L'idole " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.English) + " a été déséquipée car elle ne contient pas le nombre requis de monstres."
//                    );
//                return false;
//            }
//            else if (!idol.Owner.Inventory.HasItem(idol.Template.IdolItem))
//            {
//                return false;
//            }
//            else if (!idol.Owner.IsInFight() || idol.Owner.Fight.Id != fight.Id)
//            {
//                return false;
//            }
//            #endregion CanUseIdols

//            return true;
//        }

//        private bool CanUseIdol(PlayerIdol idol, BreachFight fight)
//        {
//            #region CanUseIdols
//            if (!idol.Template.IdolSpellId.HasValue)
//            {
//                return false;
//            }
//            else if (ActiveIdols.Count(x => x.Id == idol.Id) > 1)
//            {
//                return false;
//            }
//            else if (fight.MonsterTeam.Fighters.OfType<MonsterFighter>().Any(x => idol.Template.IncompatibleMonsters.Contains(x.Monster.Template.Id)))
//            {
//                Owner.SendServerMessageLang
//                    (
//                    "O ídolo " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.Portugese) + " foi desequipado por conter monstro incompatível na luta.",
//                    "The idol " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.English) + " has been unequipped because it contains an incompatible monster in the fight.",
//                    "El ídolo " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.Spanish) + " ha sido desequipado porque contiene un monstruo incompatible en la pelea.",
//                    "L'idole " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.French) + " a été déséquipée car elle contient un monstre incompatible dans le combat."
//                    );
//                return false;
//            }
//            else if (fight.MonsterTeam.Fighters.OfType<MonsterFighter>().Any(x => x.Monster.Template.AllIdolsDisabled))
//            {
//                Owner.SendServerMessageLang
//                    (
//                    "Os ídolos foram desequipados por conter monstros incompatíveis na luta.",
//                    "Idols have been unequipped for containing incompatible monsters in the fight.",
//                    "Los ídolos no han sido equipados para contener monstruos incompatibles en la lucha.",
//                    "Les idoles n'ont pas été équipées pour contenir des monstres incompatibles dans le combat."
//                    );
//                return false;
//            }
//            else if (idol.Template.GroupOnly && (fight.PlayerTeam.Fighters.Count < 4 || fight.MonsterTeam.Fighters.Count < 4))
//            {
//                Owner.SendServerMessageLang
//                    (
//                    "O ídolo " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.Portugese) + " foi desequipado por não conter a quantidade de monstros necessária.",
//                    "The idol " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.English) + " has been unequipped because it does not contain the required amount of monsters.",
//                    "El ídolo " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.English) + " ha sido desequipado porque no contiene la cantidad requerida de monstruos.",
//                    "L'idole " + TextManager.Instance.GetText(idol.Template.IdolItem.NameId, Languages.English) + " a été déséquipée car elle ne contient pas le nombre requis de monstres."
//                    );
//                return false;
//            }
//            else if (!idol.Owner.Inventory.HasItem(idol.Template.IdolItem))
//            {
//                return false;
//            }
//            else if (!idol.Owner.IsInFight() || idol.Owner.Fight.Id != fight.Id)
//            {
//                return false;
//            }
//            #endregion CanUseIdols

//            return true;
//        }

//        public IEnumerable<PlayerIdol> ComputeIdols(FightPvM fight)
//        {
//            foreach (var idol in ActiveIdols.ToArray())
//            {
//                if (CanUseIdol(idol, fight))
//                    continue;

//                Remove(idol);
//            }

//            if (ActiveIdols.Count > MaxActiveIdols)
//                return new PlayerIdol[0];

//            return GetIdols();
//        }

//        public IEnumerable<PlayerIdol> ComputeIdols(BreachFight fight)
//        {
//            foreach (var idol in ActiveIdols.ToArray())
//            {
//                if (CanUseIdol(idol, fight))
//                    continue;

//                Remove(idol);
//            }

//            if (ActiveIdols.Count > MaxActiveIdols)
//                return new PlayerIdol[0];

//            return GetIdols();
//        }

//        #region Events

//        private void BindEvents(Party party)
//        {
//            party.LeaderChanged += OnPartyLeaderChanged;
//            party.MemberRemoved += OnPartyMemberRemoved;
//            party.GuestPromoted += OnPartyGuestPromoted;
//            party.PartyDeleted += OnPartyDeleted;
//        }

//        private void UnBindEvents(Party party)
//        {
//            party.LeaderChanged -= OnPartyLeaderChanged;
//            party.MemberRemoved -= OnPartyMemberRemoved;
//            party.GuestPromoted -= OnPartyGuestPromoted;
//            party.PartyDeleted -= OnPartyDeleted;
//        }

//        private void OnPartyGuestPromoted(Party party, Character member)
//        {
//            member.Inventory.ItemRemoved += OnInventoryItemRemoved;
//        }

//        private void OnPartyMemberRemoved(Party party, Character member, bool kicked)
//        {
//            member.Inventory.ItemRemoved -= OnInventoryItemRemoved;
//        }

//        private void OnPartyLeaderChanged(Party party, Character leader)
//        {
//            Owner = leader;
//        }

//        private void OnInventoryItemRemoved(ItemsCollection<BasePlayerItem> sender, BasePlayerItem item)
//        {
//            foreach (var idol in ActiveIdols.Where(x => x.Template.IdolItemId == item.Template.Id).ToArray())
//            {
//                if (idol.Owner.Inventory.HasItem(idol.Template.IdolItem))
//                    continue;

//                Remove(idol);

//                if (IsPartyIdols)
//                    IdolHandler.SendIdolPartyLostMessage(Party.Clients, (short)idol.Id);
//            }
//        }

//        #endregion Events

//        #region Save
//        public void Save()
//        {
//            Owner.Record.Idols = ActiveIdols.Select(x => x.Id).ToList();
//        }

//        #endregion Save
//    }
//}
