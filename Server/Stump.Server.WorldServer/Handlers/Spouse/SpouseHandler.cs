using System.Linq;
using Stump.DofusProtocol.Types;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Handlers;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Guilds;

namespace Handlers.Spouse
{
    public class SpouseHandler : WorldHandlerContainer
    {
        [WorldHandler(SpouseGetInformationsMessage.Id)]
        public static void HandleSpouseInformation(WorldClient client, SpouseGetInformationsMessage message)
        {
            if (client.Character.CurrentSpouse != 0)
            {
                var spouse = CharacterManager.Instance.GetCharacterById(client.Character.CurrentSpouse);

                if (spouse == null)
                {
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            client.Character.SendServerMessage("Votre compagnon n'existe pas!");
                            break;
                        case "es":
                            client.Character.SendServerMessage("¡Su compañero no existe!");
                            break;
                        case "en":
                            client.Character.SendServerMessage("Your mate dont exist!");
                            break;
                        default:
                            client.Character.SendServerMessage("Seu companheiro não existe!");
                            break;
                    }
                }
                else
                {
                    if (spouse.Id == client.Character.CurrentSpouse && spouse.SpouseID == client.Character.Id)
                    {
                        int level = ExperienceManager.Instance.GetCharacterLevel(spouse.Experience);

                        GuildMember guildmember = null;
                        guildmember = GuildManager.Instance.TryGetGuildMember(spouse.Id);

                        Character spouseon = null;
                        spouseon = World.Instance.GetCharacter(spouse.Id);

                        client.Send(new SpouseStatusMessage(true));

                        if (spouseon != null)
                        {
                            if (spouseon.Level > 200)
                                client.Send(new SpouseInformationsMessage(new FriendSpouseInformations(spouseon.Account.Id, (ulong)spouseon.Id, "° " + spouseon.Name + " °", (ushort)(spouseon.Level - 200), (sbyte)spouseon.Breed.Id, (sbyte)spouseon.Sex, spouseon.Look.GetEntityLook(), spouseon.GuildMember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : spouseon.GuildMember.Guild.GetGuildInformations(), (sbyte)spouseon.AlignmentSide)));
                            else
                                client.Send(new SpouseInformationsMessage(new FriendSpouseInformations(spouseon.Account.Id, (ulong)spouseon.Id, spouseon.Name, (ushort)spouseon.Level, (sbyte)spouseon.Breed.Id, (sbyte)spouseon.Sex, spouseon.Look.GetEntityLook(), spouseon.GuildMember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : spouseon.GuildMember.Guild.GetGuildInformations(), (sbyte)spouseon.AlignmentSide)));
                        }
                        else
                        {
                            if (level > 200)
                                client.Send(new SpouseInformationsMessage(new FriendSpouseInformations(spouse.AccountId, (ulong)spouse.Id, "° " + spouse.Name + " °", (ushort)(level - 200), (sbyte)spouse.Breed, (sbyte)spouse.Sex, spouse.LastLook?.GetEntityLook() ?? spouse.DefaultLook.GetEntityLook(), guildmember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : guildmember.Guild.GetGuildInformations(), (sbyte)spouse.AlignmentSide)));
                            else
                                client.Send(new SpouseInformationsMessage(new FriendSpouseInformations(spouse.AccountId, (ulong)spouse.Id, spouse.Name, (ushort)level, (sbyte)spouse.Breed, (sbyte)spouse.Sex, spouse.LastLook?.GetEntityLook() ?? spouse.DefaultLook.GetEntityLook(), guildmember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : guildmember.Guild.GetGuildInformations(), (sbyte)spouse.AlignmentSide)));
                        }

                        bool follow = false;

                        if (spouseon != null)
                        {
                            if (client.Character.FollowingCharacters.FirstOrDefault(x => x == spouseon) != null)
                            {
                                follow = true;
                            }
                        }
                        else
                        {
                            if (client.Character.FollowingCharacters.FirstOrDefault(x => x.Id == spouse.Id) != null)
                            {
                                follow = true;
                            }
                        }

                        if (spouseon != null)
                        {
                            if (spouseon.Level > 200)
                                client.Send(new SpouseInformationsMessage(new FriendSpouseOnlineInformations(spouseon.Account.Id, (ulong)spouseon.Id, spouseon.Name, (ushort)(spouseon.Level - 200), (sbyte)spouseon.Breed.Id, (sbyte)spouseon.Sex, spouseon.Look.GetEntityLook(), spouseon.GuildMember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : spouseon.GuildMember.Guild.GetGuildInformations(), (sbyte)spouseon.AlignmentSide, spouseon.IsInFight(), follow, spouseon.Map.Id, (ushort)spouseon.SubArea.Id)));
                            else
                                client.Send(new SpouseInformationsMessage(new FriendSpouseOnlineInformations(spouseon.Account.Id, (ulong)spouseon.Id, spouseon.Name, (ushort)spouseon.Level, (sbyte)spouseon.Breed.Id, (sbyte)spouseon.Sex, spouseon.Look.GetEntityLook(), spouseon.GuildMember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : spouseon.GuildMember.Guild.GetGuildInformations(), (sbyte)spouseon.AlignmentSide, spouseon.IsInFight(), follow, spouseon.Map.Id, (ushort)spouseon.SubArea.Id)));
                        }
                        else
                        {
                            if (level > 200)
                                client.Send(new SpouseInformationsMessage(new FriendSpouseOnlineInformations(spouse.AccountId, (ulong)spouse.Id, spouse.Name, (ushort)(level - 200), (sbyte)spouse.Breed, (sbyte)spouse.Sex, spouse.LastLook?.GetEntityLook() ?? spouse.DefaultLook.GetEntityLook(), guildmember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : guildmember.Guild.GetGuildInformations(), (sbyte)spouse.AlignmentSide, false, follow, spouse.MapId, (ushort)World.Instance.GetMap(spouse.MapId).SubArea.Id)));// todo maybe recconect in fight count as fight idk
                            else
                                client.Send(new SpouseInformationsMessage(new FriendSpouseOnlineInformations(spouse.AccountId, (ulong)spouse.Id, spouse.Name, (ushort)level, (sbyte)spouse.Breed, (sbyte)spouse.Sex, spouse.LastLook?.GetEntityLook() ?? spouse.DefaultLook.GetEntityLook(), guildmember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : guildmember.Guild.GetGuildInformations(), (sbyte)spouse.AlignmentSide, false, follow, spouse.MapId, (ushort)World.Instance.GetMap(spouse.MapId).SubArea.Id)));// todo maybe recconect in fight count as fight idk
                        }
                    }
                }
            }
        }

        [WorldHandler(FriendSpouseJoinRequestMessage.Id)]
        public static void HandleJoinSpouse(WorldClient client, FriendSpouseJoinRequestMessage message)
        {
            var character = client.Character;
            var spouse = World.Instance.GetCharacter(x => x.Id == client.Character.CurrentSpouse);

            if (spouse == null)
            {
                switch (client.Character.Account.Lang)
                {
                    case "fr":
                        client.Character.SendServerMessage("Votre compagnon est hors ligne!");
                        break;
                    case "es":
                        client.Character.SendServerMessage("¡Su compañero no está conectado!");
                        break;
                    case "en":
                        client.Character.SendServerMessage("Your mate is offline!");
                        break;
                    default:
                        client.Character.SendServerMessage("Seu companheiro não está conectado!");
                        break;
                }
            }
            else if (spouse.Map.IsDungeon())
            {
                switch (client.Character.Account.Lang)
                {
                    case "fr":
                        client.Character.SendServerMessage("Personnage en donjon!");
                        break;
                    case "es":
                        client.Character.SendServerMessage("¡Personaje en calabozo!");
                        break;
                    case "en":
                        client.Character.SendServerMessage("Character in dugeon!");
                        break;
                    default:
                        client.Character.SendServerMessage("Personagem em calabouço!");
                        break;
                }
            }
            else
            {
                character.Teleport(spouse.Map, spouse.Cell);
            }
        }

        public static void SendSpouseInformationMessage(WorldClient client, Character spouseon)
        {
            bool follow = false;

            if (client.Character.FollowingCharacters.FirstOrDefault(x => x == spouseon) != null)
                follow = true;

            client.Send(new SpouseStatusMessage(true));

            if (spouseon.Level > 200)
                client.Send(new SpouseInformationsMessage(new FriendSpouseInformations(spouseon.Account.Id, (ulong)spouseon.Id, spouseon.Name, (ushort)(spouseon.Level - 200), (sbyte)spouseon.Breed.Id, (sbyte)spouseon.Sex, spouseon.Look.GetEntityLook(), spouseon.GuildMember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : spouseon.GuildMember.Guild.GetGuildInformations(), (sbyte)spouseon.AlignmentSide)));
            else
                client.Send(new SpouseInformationsMessage(new FriendSpouseInformations(spouseon.Account.Id, (ulong)spouseon.Id, spouseon.Name, (ushort)spouseon.Level, (sbyte)spouseon.Breed.Id, (sbyte)spouseon.Sex, spouseon.Look.GetEntityLook(), spouseon.GuildMember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : spouseon.GuildMember.Guild.GetGuildInformations(), (sbyte)spouseon.AlignmentSide)));

            if (spouseon.Level > 200)
                client.Send(new SpouseInformationsMessage(new FriendSpouseOnlineInformations(spouseon.Account.Id, (ulong)spouseon.Id, spouseon.Name, (ushort)(spouseon.Level - 200), (sbyte)spouseon.Breed.Id, (sbyte)spouseon.Sex, spouseon.Look.GetEntityLook(), spouseon.GuildMember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : spouseon.GuildMember.Guild.GetGuildInformations(), (sbyte)spouseon.AlignmentSide, spouseon.IsInFight(), follow, spouseon.Map.Id, (ushort)spouseon.SubArea.Id)));
            else
                client.Send(new SpouseInformationsMessage(new FriendSpouseOnlineInformations(spouseon.Account.Id, (ulong)spouseon.Id, spouseon.Name, (ushort)spouseon.Level, (sbyte)spouseon.Breed.Id, (sbyte)spouseon.Sex, spouseon.Look.GetEntityLook(), spouseon.GuildMember == null ? new GuildInformations(0, "", 0, new Stump.DofusProtocol.Types.SocialEmblem(0, 0, 0, 0)) : spouseon.GuildMember.Guild.GetGuildInformations(), (sbyte)spouseon.AlignmentSide, spouseon.IsInFight(), follow, spouseon.Map.Id, (ushort)spouseon.SubArea.Id)));

            client.Send(new AllianceLeftMessage());
        }

        [WorldHandler(FriendSpouseFollowWithCompassRequestMessage.Id)]
        public static void SendFriendSpouseFollowWithCompassRequestMessage(WorldClient client, FriendSpouseFollowWithCompassRequestMessage message)
        {
            var spouse = World.Instance.GetCharacter(client.Character.CurrentSpouse);

            if (spouse != null)
            {
                if (message.enable)
                    spouse.FollowSpousee(client.Character);
                else
                    spouse.StopFollowSpouse();
            }
        }
    }
}