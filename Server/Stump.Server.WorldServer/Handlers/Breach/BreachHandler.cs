using Stump.Core.Extensions;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Breach;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Handlers.Breach
{
    public partial class BreachHandler : WorldHandlerContainer
    {
        private BreachHandler()
        {
        }

        [WorldHandler(BreachInvitationAnswerMessage.Id)]
        public static void HandleBreachInvitationAnswerMessage(WorldClient client, BreachInvitationAnswerMessage message)
        {
            if (client.Character.BreachGroupInvitation != null)
            {
                if (message.accept)
                {
                    client.Character.BreachOwner = client.Character.BreachGroupInvitation.Host;
                    client.Character.BreachGroupInvitation = null;

                    if (client.Character.BreachOwner.BreachGroup == null)
                    {
                        client.Character.BreachOwner.BreachGroup = new long[] { client.Character.Id };
                    }
                    else
                    {
                        client.Character.BreachOwner.BreachGroup = client.Character.BreachOwner.BreachGroup.Add(client.Character.Id);
                    }

                    client.Send(new BreachInvitationCloseMessage(client.Character.BreachOwner.GetCharacterMinimalInformations()));
                    client.Character.BreachOwner.Client.Send(new BreachInvitationResponseMessage(client.Character.GetCharacterMinimalInformations(), true));
                    client.Character.Teleport(client.Character.BreachOwner.Position);
                }
            }
        }

        [WorldHandler(BreachInvitationRequestMessage.Id)]
        public static void HandleBreachInvitationRequestMessage(WorldClient client, BreachInvitationRequestMessage message)
        {
            if (client.Character.BreachGroup == null || client.Character.BreachGroup.Length <= 3)
            {
                foreach (var guest in message.guests)
                {
                    Character target = World.Instance.GetCharacter((int)guest);
                    BreachInvitationOfferMessage BreachInvitationOfferMessage = new BreachInvitationOfferMessage(client.Character.GetCharacterMinimalInformations(), 60);
                    
                    if (target != client.Character)
                    {
                        target.BreachGroupInvitation = new BreachGroupInvitation(client.Character, BreachInvitationOfferMessage);
                        target.Client.Send(BreachInvitationOfferMessage);
                    }
                }
            }
        }

        [WorldHandler(BreachTeleportRequestMessage.Id)]
        public static void HandleBreachTeleportRequestMessage(WorldClient client, BreachTeleportRequestMessage message)
        {
            if (client.Character.Map.SubArea.Id == 904 || client.Character.Map.Id == 195559424)
            {
                client.Character.Teleport(new ObjectPosition(Singleton<World>.Instance.GetMap(client.Character.Record.MapBeforeBreachId), (short)client.Character.Record.CellBeforeBreachId));
                client.Character.Record.IsInBreach = false;
                client.Character.Record.CellBeforeBreachId = 0;
                client.Character.Record.MapBeforeBreachId = 0;
            }
            else
            {
                client.Character.Record.IsInBreach = true;
                client.Character.Record.CellBeforeBreachId = client.Character.Cell.Id;
                client.Character.Record.MapBeforeBreachId = client.Character.Map.Id;
                client.Character.Teleport(new ObjectPosition(Singleton<World>.Instance.GetMap(195559424), 382, DirectionsEnum.DIRECTION_SOUTH_EAST));
            }
        }

        [WorldHandler((BreachExitRequestMessage.Id))]
        public static void HandleBreachExitRequestMessage(WorldClient client, BreachExitRequestMessage message)
        {
            client.Character.Teleport(new ObjectPosition(Singleton<World>.Instance.GetMap(195559424), 382, DirectionsEnum.DIRECTION_NORTH));
            client.Character.BreachGroup = null;
            client.Character.BreachOwner = null;
            client.Character.SendServerMessageLang("Deixando os sonhos, o grupo foi dissolvido.", "Leaving Dreams, the group was disbanded.", "Dejando Dreams, el grupo se disolvió", "En quittant Dreams, le groupe a été dissous.");
        }

        [WorldHandler(BreachRewardBuyMessage.Id)]
        public static void HandleBreachRewardBuyMessage(WorldClient client, BreachRewardBuyMessage message)
        {
            if (client.Character.BreachBuyables != null)
            {
                for (int index = 0; index < client.Character.BreachBuyables.Length; index++)
                {
                    BreachReward buyable = client.Character.BreachBuyables[index];

                    if (buyable.id == message.id)
                    {
                        if (client.Character.BreachBudget >= buyable.price)
                        {
                            client.Send(new BreachRewardBoughtMessage(message.id, true));
                            var buyables = new List<BreachReward>(client.Character.BreachBuyables);
                            buyables.RemoveAt(index);
                            client.Character.BreachBuyables = buyables.ToArray();
                           
                            switch (buyable.id)
                            {
                                //MINOR INTELLIGENCY
                                case 6:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)126, 10));
                                    client.Character.BreachBudget -= 300;
                                    break;

                                //MINOR LUCK
                                case 7:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)123, 10));
                                    client.Character.BreachBudget -= 300;
                                    break;

                                //MINOR AGILITY
                                case 91:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)119, 10));
                                    client.Character.BreachBudget -= 300;
                                    break;

                                //MINOR STRENGTH
                                case 92:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)118, 10));
                                    client.Character.BreachBudget -= 300;
                                    break;

                                //MINOR VITALITY
                                case 93:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)125, 25));
                                    client.Character.BreachBudget -= 300;
                                    break;
                                
                                //MINOR POWER
                                case 94:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)138, 10));
                                    client.Character.BreachBudget -= 300;
                                    break;
                                
                                //MINOR AP ATTACK
                                case 95:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)410, 1));
                                    client.Character.BreachBudget -= 300;
                                    break;

                                //MINOR AM ATTACK
                                case 96:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)412, 1));
                                    client.Character.BreachBudget -= 300;
                                    break;

                                //MINOR TACKLE LEAK
                                case 99:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)752, 1));
                                    client.Character.BreachBudget -= 300;
                                    break;

                                //MINOR TACKLE BLOCK
                                case 100:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)753, 1));
                                    client.Character.BreachBudget -= 300;
                                    break;

                                ///MEDIUM INTELLIGENCY
                                case 103:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)126, 25));
                                    client.Character.BreachBudget -= 600;
                                    break;

                                ///MEDIUM LUCK
                                case 104:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)123, 25));
                                    client.Character.BreachBudget -= 600;
                                    break;

                                ///MEDIUM AGILITY
                                case 105:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)119, 25));
                                    client.Character.BreachBudget -= 600;
                                    break;

                                ///MEDIUM STRENGTH
                                case 106:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)118, 25));
                                    client.Character.BreachBudget -= 600;
                                    break;

                                ///MEDIUM VITALITY
                                case 107:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)125, 50));
                                    client.Character.BreachBudget -= 600;
                                    break;

                                ///MEDIUM POWER
                                case 108:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)138, 20));
                                    client.Character.BreachBudget -= 600;
                                    break;

                                ///MEDIUM AP ATTACK
                                case 109:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)410, 2));
                                    client.Character.BreachBudget -= 600;
                                    break;

                                ///MEDIUM AM ATTACK
                                case 110:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)412, 2));
                                    client.Character.BreachBudget -= 600;
                                    break;

                                ///MEDIUM TACKLE LEAK
                                case 113:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)752, 2));
                                    client.Character.BreachBudget -= 600;
                                    break;

                                ///MEDIUM TACKLE BLOCK
                                case 114:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)753, 2));
                                    client.Character.BreachBudget -= 600;
                                    break;

                                ///MAJOR INTELLIGENCY
                                case 117:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)126, 50));
                                    client.Character.BreachBudget -= 900;
                                    break;

                                ///MAJOR LUCK
                                case 118:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)123, 50));
                                    client.Character.BreachBudget -= 900;
                                    break;

                                ///MAJOR AGILITY
                                case 119:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)119, 50));
                                    client.Character.BreachBudget -= 900;
                                    break;

                                ///MAJOR STRENGTH
                                case 120:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)118, 50));
                                    client.Character.BreachBudget -= 900;
                                    break;

                                ///MAJOR VITALITY
                                case 121:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)125, 100));
                                    client.Character.BreachBudget -= 900;
                                    break;

                                ///MAJOR POWER
                                case 122:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)138, 40));
                                    client.Character.BreachBudget -= 900;
                                    break;

                                ///MAJOR AP ATTACK
                                case 123:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)410, 4));
                                    client.Character.BreachBudget -= 900;
                                    break;

                                ///MAJOR AM ATTACK
                                case 124:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)412, 4));
                                    client.Character.BreachBudget -= 900;
                                    break;

                                ///MAJOR TACKLE LEAK
                                case 127:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)752, 4));
                                    client.Character.BreachBudget -= 900;
                                    break;

                                ///MAJOR TACKLE BLOCK
                                case 128:
                                    client.Character.BreachBoosts = client.Character.BreachBoosts.Add(new ObjectEffectInteger((ushort)753, 4));
                                    client.Character.BreachBudget -= 900;
                                    break;

                                default:
                                    break;
                            }

                            client.Send(new BreachStateMessage(client.Character.GetCharacterMinimalInformations(),
                                client.Character.BreachBoosts.ToArray(), (uint)client.Character.BreachBudget, true));
                        }
                        else
                        {
                            client.Send(new BreachRewardBoughtMessage(message.id, false));
                        }

                        break;
                    }
                }
            }
        }
    }
}