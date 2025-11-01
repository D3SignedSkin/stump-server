using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

using Stump.Server.WorldServer.Game.Maps.Pathfinding;
using System;
using System.Linq;

namespace Stump.Server.WorldServer.Commands.Commands
{
    public class FollowLeadCommand : InGameCommand
    {
        Character Lead;
        public FollowLeadCommand()
        {
            Aliases = new[] { "follow" };
            Description = "Permite que você acompanhe os movimentos do lider do seu grupo, e faça com que eles se juntem à luta do lider.";
            Description_en = "Allows you to track your party's leader's movements, and have them join the leader's fight.";
            Description_es = "Te permite seguir los movimientos del líder de tu grupo y hacer que se unan a la lucha del líder.";
            Description_fr = "Vous permet de suivre les mouvements du chef de votre groupe et de les faire rejoindre le combat du chef.";

            RequiredRole = RoleEnum.Vip;
        }

        public override void Execute(GameTrigger trigger)
        {
            var character = trigger.Character;

            if (!character.IsInParty())
            {
                #region Menssagem Infor
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Vous devez être dans un groupe pour utiliser cette fonctionnalité.");
                        break;
                    case "es":
                        character.SendServerMessage("Debes estar en un grupo para usar esta característica.");
                        break;
                    case "en":
                        character.SendServerMessage("You must be in a group to use this feature.");
                        break;
                    default:
                        character.SendServerMessage("Você deve estar em um grupo para poder usar essa função.");
                        break;
                }
                #endregion

                return;
            }
            if (!character.IsPartyLeader())
            {
                #region Menssagem Infor
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Vous devez être le chef de groupe pour pouvoir utiliser cette fonction.");
                        break;
                    case "es":
                        character.SendServerMessage("Debe ser el líder del grupo para poder utilizar esta función.");
                        break;
                    case "en":
                        character.SendServerMessage("You must be the group leader to be able to use this function.");
                        break;
                    default:
                        character.SendServerMessage("Você deve ser o lider do grupo para poder usar essa função.");
                        break;
                }
                #endregion

                return;
            }

            if (!character.isMultiLeadder)
            {
                character.isMultiLeadder = true;
                Lead = character;

                foreach (var perso in WorldServer.Instance.FindClients(x => x.IP == character.Client.IP && x.Account.LastHardwareId == character.Account.LastHardwareId && x.Character != character))
                {
                    if (character.Map.IsDungeon())
                        break;

                    if (character.Account.UserGroupId >= 4 && character.Account.UserGroupId <= 7)
                        break;

                    if (perso.Account.UserGroupId >= 4 && perso.Account.UserGroupId <= 7)
                        break;

                    if (perso.Character.Map.Id == character.Map.Id)
                    {
                        perso.Character.Teleport(character.Map, character.Cell);
                    }
                }

                character.StartMoving += OnStartMoving;
                character.EnterFight += OnEnterFight;
                character.ReadyStatusChanged += OnReadyStatusChanged;

                #region Menssagem Infor
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Le suivi des personnages est désormais activé, tous vos personnages rejoindront automatiquement les combats et bougeront en même temps que le leadder !");
                        break;
                    case "es":
                        character.SendServerMessage("El seguimiento de personajes ahora está habilitado, ¡todos tus personajes se unirán automáticamente a las peleas y se moverán al mismo tiempo que el líder!");
                        break;
                    case "en":
                        character.SendServerMessage("Character tracking is now enabled, all your characters will automatically join fights and move at the same time as the leader!");
                        break;
                    default:
                        character.SendServerMessage("O rastreamento de personagens agora está ativado, todos os seus personagens se juntarão automaticamente às lutas e se moverão ao mesmo tempo que o líder!");
                        break;
                }
                #endregion
            }
            else
            {
                character.isMultiLeadder = false;
                Lead = null;
                character.StartMoving -= OnStartMoving;
                character.EnterFight -= OnEnterFight;
                character.ReadyStatusChanged -= OnReadyStatusChanged;

                #region Menssagem Infor
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.SendServerMessage("Le suivi des personnages est maintenant désactivé !");
                        break;
                    case "es":
                        character.SendServerMessage("¡El seguimiento de personajes ahora está deshabilitado!");
                        break;
                    case "en":
                        character.SendServerMessage("Character tracking is now disabled!");
                        break;
                    default:
                        character.SendServerMessage("O rastreamento de personagens agora está desativado!");
                        break;
                }
                #endregion
            }
        }

        private void OnReadyStatusChanged(CharacterFighter fighter)
        {
            if (fighter == null)
                return;

            if (fighter.Map.IsDungeon())
                return;

            try
            {
                foreach (var perso in WorldServer.Instance.FindClients(x => x.IP == fighter.Character.Client.IP && x.Account.LastHardwareId == fighter.Character.Account.LastHardwareId && x.Character != fighter.Character))
                {
                    if (perso.Character.Map.Id == fighter.Map.Id && perso.Character.IsInFight())
                    {
                        if (perso.Account.UserGroupId >= 4 && perso.Account.UserGroupId <= 7)
                            break;

                        if (perso.Character.Map.IsDungeon())
                            continue;

                        perso.Character.Fighter.ToggleReady(fighter.IsReady);
                    }
                }
            }
            catch (Exception ex)
            {
                // Trata a exceção adequadamente de acordo com o tipo e informa o erro
                if (ex is InvalidOperationException)
                {
                    // Ocorreu uma operação inválida, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
                else if (ex is ArgumentNullException)
                {
                    // Faltando argumento, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
                else if (ex is Exception)
                {
                    // Erro genérico, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
            }
        }


        private void OnStartMoving(ContextActor actor, Path path)
        {
            try
            {
                var character = (actor as Character);

                if (character.Map.IsDungeon())
                    return;

                if (character.IsInFight())
                    return;

                if (character.Account.UserGroupId >= 4 && character.Account.UserGroupId <= 7)
                    return;

                character.EnterMap += OnEnterMap;

                foreach (var perso in WorldServer.Instance.FindClients(x => x.IP == character.Client.IP && x.Account.LastHardwareId == character.Account.LastHardwareId && x.Character != character))
                {
                    if (perso.Character.Map.Id == character.Map.Id)
                    {
                        if (perso.Account.UserGroupId >= 4 && perso.Account.UserGroupId <= 7)
                            break;

                        if (perso.Character.Map.IsDungeon())
                            continue;

                        perso.Character.Teleport(character.Map, character.Cell);
                        perso.Character.StartMove(path);
                    }
                }
            }
            catch (Exception ex)
            {
                // Trata a exceção adequadamente de acordo com o tipo e informa o erro
                if (ex is InvalidOperationException)
                {
                    // Ocorreu uma operação inválida, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
                else if (ex is ArgumentNullException)
                {
                    // Faltando argumento, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
                else if (ex is Exception)
                {
                    // Erro genérico, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
            }
        }


        private void OnEnterMap(ContextActor actor, Game.Maps.Map map)
        {
            var character = (actor as Character);

            if (character.IsInFight() || character.Map.IsDungeon())
                return;

            try
            {
                character.EnterMap -= OnEnterMap;

                foreach (var perso in WorldServer.Instance.FindClients(x => x.IP == character.Client.IP && x.Account.LastHardwareId == character.Account.LastHardwareId && x.Character != character))
                {
                    if (perso.Account.UserGroupId >= 4 && perso.Account.UserGroupId <= 7)
                        break;

                    if (perso.Character.Map.IsDungeon())
                        continue;

                    perso.Character.Teleport(character.Map, character.Cell);
                }
            }
            catch (Exception ex)
            {
                // Trata a exceção adequadamente de acordo com o tipo e informa o erro
                if (ex is InvalidOperationException)
                {
                    // Ocorreu uma operação inválida, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
                else if (ex is ArgumentNullException)
                {
                    // Faltando argumento, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
                else if (ex is Exception)
                {
                    // Erro genérico, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
            }
        }

        private void OnEnterFight(CharacterFighter fighter)
        {
            if (fighter == null) // || fighter.Map.IsDungeon() - Retirei para poder entrar em lutas dentro das DGs
                return;

            if (fighter.Character.Account.UserGroupId >= 4 && fighter.Character.Account.UserGroupId <= 7)
                return;

            try
            {
                foreach (var perso in WorldServer.Instance.FindClients(x => x.IP == fighter.Character.Client.IP && x.Account.LastHardwareId == fighter.Character.Account.LastHardwareId && x.Character != fighter.Character))
                {
                    if (perso.Character.Map.Id == fighter.Map.Id && !perso.Character.IsInFight())
                    {
                        if (perso.Account.UserGroupId >= 4 && perso.Account.UserGroupId <= 7)
                            break;

                        if (perso.Character.Map.IsDungeon())
                            continue;

                        var fighterr = perso.Character.CreateFighter(fighter.Team);
                        fighter.Team.AddFighter(fighterr);
                    }
                }
            }
            catch (Exception ex)
            {
                // Trata a exceção adequadamente de acordo com o tipo e informa o erro
                if (ex is InvalidOperationException)
                {
                    // Ocorreu uma operação inválida, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
                else if (ex is ArgumentNullException)
                {
                    // Faltando argumento, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
                else if (ex is Exception)
                {
                    // Erro genérico, informa o erro e continua a execução
                    Console.WriteLine("Erro: " + ex.Message);
                }
            }
        }
    }
}