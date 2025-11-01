//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Game.Actors;
//using Stump.Server.WorldServer.Game.Actors.Fight;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
//using Stump.Server.WorldServer.Game.Effects.Handlers.Usables;
//using Stump.Server.WorldServer.Game.Effects.Instances;
//using Stump.Server.WorldServer.Game.Items.Player;
//using Stump.Server.WorldServer.Game.Maps.Pathfinding;

//namespace Stump.Server.WorldServer.Game.Effects.Handlers.Items
//{
//    [EffectHandler(EffectsEnum.Effect_Follow)]
//    public class Follow : UsableEffectHandler
//    {
//        public Follow(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
//        {
//        }

//        Character Lead;

//        protected override bool InternalApply()
//        {
//            var integerEffect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

//            if (integerEffect == null)
//                return false;

//            if (Target.IsInFight())
//                return false;

//            if (!Target.Account.IsSubscribe)
//                return false;

//            UsedItems = NumberOfUses;

//            if (!Target.IsInParty())
//            {
//                #region Menssagem Infor
//                switch (Target.Account.Lang)
//                {
//                    case "fr":
//                        Target.SendServerMessage("Vous devez être dans un groupe pour utiliser cette fonctionnalité.");
//                        break;
//                    case "es":
//                        Target.SendServerMessage("Debes estar en un grupo para usar esta característica.");
//                        break;
//                    case "en":
//                        Target.SendServerMessage("You must be in a group to use this feature.");
//                        break;
//                    default:
//                        Target.SendServerMessage("Você deve estar em um grupo para poder usar essa função.");
//                        break;
//                }
//                #endregion

//                return false;
//            }

//            if (!Target.IsPartyLeader())
//            {
//                #region Menssagem Infor
//                switch (Target.Account.Lang)
//                {
//                    case "fr":
//                        Target.SendServerMessage("Vous devez être le chef de groupe pour pouvoir utiliser cette fonction.");
//                        break;
//                    case "es":
//                        Target.SendServerMessage("Debe ser el líder del grupo para poder utilizar esta función.");
//                        break;
//                    case "en":
//                        Target.SendServerMessage("You must be the group leader to be able to use this function.");
//                        break;
//                    default:
//                        Target.SendServerMessage("Você deve ser o lider do grupo para poder usar essa função.");
//                        break;
//                }
//                #endregion

//                return false;
//            }

//            if (!Target.isMultiLeadder)
//            {
//                Target.isMultiLeadder = true;
//                Lead = Target;

//                foreach (var perso in WorldServer.Instance.FindClients(x => x.IP == Target.Client.IP && x.Account.LastHardwareId == Target.Account.LastHardwareId && x.Character != Target))
//                {
//                    if (perso == null)
//                        return false;

//                    if (Target.Map.IsDungeon())
//                        break;

//                    if (Target.Account.UserGroupId >= 4 && Target.Account.UserGroupId <= 7)
//                        break;

//                    if (perso.Account.UserGroupId >= 4 && perso.Account.UserGroupId <= 7)
//                        break;

//                    if (perso.Character.Map.Id == Target.Map.Id)
//                    {
//                        perso.Character.Teleport(Target.Map, Target.Cell);
//                    }
//                }

//                Target.StartMoving += OnStartMoving;
//                Target.EnterFight += OnEnterFight;
//                Target.ReadyStatusChanged += OnReadyStatusChanged;

//                #region Menssagem Infor
//                switch (Target.Account.Lang)
//                {
//                    case "fr":
//                        Target.SendServerMessage("Le suivi des personnages est désormais activé, tous vos personnages rejoindront automatiquement les combats et bougeront en même temps que le leadder !");
//                        break;
//                    case "es":
//                        Target.SendServerMessage("El seguimiento de personajes ahora está habilitado, ¡todos tus personajes se unirán automáticamente a las peleas y se moverán al mismo tiempo que el líder!");
//                        break;
//                    case "en":
//                        Target.SendServerMessage("Character tracking is now enabled, all your characters will automatically join fights and move at the same time as the leader!");
//                        break;
//                    default:
//                        Target.SendServerMessage("O rastreamento de personagens agora está ativado, todos os seus personagens se juntarão automaticamente às lutas e se moverão ao mesmo tempo que o líder!");
//                        break;
//                }
//                #endregion
//            }
//            else
//            {
//                Target.isMultiLeadder = false;
//                Lead = null;
//                Target.StartMoving -= OnStartMoving;
//                Target.EnterFight -= OnEnterFight;
//                Target.ReadyStatusChanged -= OnReadyStatusChanged;

//                #region Menssagem Infor
//                switch (Target.Account.Lang)
//                {
//                    case "fr":
//                        Target.SendServerMessage("Le suivi des personnages est maintenant désactivé !");
//                        break;
//                    case "es":
//                        Target.SendServerMessage("¡El seguimiento de personajes ahora está deshabilitado!");
//                        break;
//                    case "en":
//                        Target.SendServerMessage("Character tracking is now disabled!");
//                        break;
//                    default:
//                        Target.SendServerMessage("O rastreamento de personagens agora está desativado!");
//                        break;
//                }
//                #endregion
//            }

//            return true;
//        }

//        private void OnReadyStatusChanged(CharacterFighter fighter)
//        {
//            if (fighter == null)
//                return;

//            if (fighter.Map.Id == 165153537 || fighter.Map.SubArea.Id == 84)
//                return;

//            foreach (var perso in WorldServer.Instance.FindClients(x => x.IP == fighter.Character.Client.IP && x.Account.LastHardwareId == fighter.Character.Account.LastHardwareId && x.Character != fighter.Character))
//            {
//                if (perso == null)
//                    return;

//                if (perso.Character.Map.Id == fighter.Map.Id && perso.Character.IsInFight())
//                {
//                    if (perso.Account.UserGroupId >= 4 && perso.Account.UserGroupId <= 7)
//                        break;

//                    perso.Character.Fighter.ToggleReady(fighter.IsReady);
//                }
//            }
//        }

//        private void OnStartMoving(ContextActor actor, Path path)
//        {
//            var character = (actor as Character);

//            if (character == null)
//                return;

//            if (character.IsInFight())
//                return;

//            if (character.Map.Id == 165153537 || character.Map.SubArea.Id == 84)
//                return;

//            if (character.Account.UserGroupId >= 4 && character.Account.UserGroupId <= 7)
//                return;

//            character.EnterMap += OnEnterMap;

//            foreach (var perso in WorldServer.Instance.FindClients(x => x.IP == character.Client.IP && x.Account.LastHardwareId == character.Account.LastHardwareId && x.Character != character))
//            {
//                if (perso == null)
//                    continue;

//                if (perso.Character.Map.Id == character.Map.Id)
//                {
//                    if (perso.Account.UserGroupId >= 4 && perso.Account.UserGroupId <= 7)
//                        continue;

//                    perso.Character.Teleport(character.Map, character.Cell);
//                    perso.Character.StartMove(path);
//                }
//            }
//        }

//        private void OnEnterMap(ContextActor actor, Game.Maps.Map map)
//        {
//            var character = (actor as Character);

//            if (character == null)
//                return;

//            if (character.IsInFight() || character.Map.IsDungeon())
//                return;

//            if (character.Map.Id == 165153537 || character.Map.SubArea.Id == 84)
//                return;

//            character.EnterMap -= OnEnterMap;

//            foreach (var perso in WorldServer.Instance.FindClients(x => x.IP == character.Client.IP && x.Account.LastHardwareId == character.Account.LastHardwareId && x.Character != character))
//            {
//                if (perso == null)
//                    return;

//                if (perso.Account.UserGroupId >= 4 && perso.Account.UserGroupId <= 7)
//                    break;

//                perso.Character.Teleport(character.Map, character.Cell);
//            }
//        }

//        private void OnEnterFight(CharacterFighter fighter)
//        {
//            if (fighter == null || fighter.Map.IsDungeon())
//                return;

//            if (fighter.Character.Account.UserGroupId >= 4 && fighter.Character.Account.UserGroupId <= 7)
//                return;

//            if (fighter.Character.Map.Id == 165153537 || fighter.Character.Map.SubArea.Id == 84)
//                return;

//            foreach (var perso in WorldServer.Instance.FindClients(x => x.IP == fighter.Character.Client.IP && x.Account.LastHardwareId == fighter.Character.Account.LastHardwareId && x.Character != fighter.Character))
//            {
//                if (perso == null)
//                    return;

//                if (perso.Character.Map.Id == fighter.Map.Id && !perso.Character.IsInFight())
//                {
//                    if (perso.Account.UserGroupId >= 4 && perso.Account.UserGroupId <= 7)
//                        break;

//                    var fighterr = perso.Character.CreateFighter(fighter.Team);
//                    fighter.Team.AddFighter(fighterr);
//                }
//            }
//        }
//    }
//}
