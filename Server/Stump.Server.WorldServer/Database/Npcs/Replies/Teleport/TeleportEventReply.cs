using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Maps.Cells;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("TeleportEvent", typeof(NpcReply), typeof(NpcReplyRecord))]
    public class TeleportEventReply : NpcReply
    {
        private bool m_mustRefreshPosition;
        private ObjectPosition m_position;

        public TeleportEventReply()
        {
            Record.Type = "TeleportEvent";
        }

        public TeleportEventReply(NpcReplyRecord record)
            : base(record)
        {
        }

        private void RefreshPosition()
        {
            var map = Game.World.Instance.GetMap(139724293);

            if (map == null)
                throw new Exception(string.Format("Cannot load SkillTeleport id={0}, map {1} isn't found", Id, 139724293));

            var cell = map.Cells[271];

            m_position = new ObjectPosition(map, cell, DirectionsEnum.DIRECTION_SOUTH_EAST);
        }

        public ObjectPosition GetPosition()
        {
            if (m_position == null || m_mustRefreshPosition)
                RefreshPosition();

            m_mustRefreshPosition = false;

            return m_position;
        }

        public override bool Execute(Npc npc, Character character)
        {
            List<WorldClient> listcharacters = new List<WorldClient>();

            if (!base.Execute(npc, character))
            {
                return false;
            }
            else if (character.isMultiLeadder)
            {
                #region MSG
                character.SendServerMessageLang(
                    "Você deve está sozinho para poder entrar no mapa de eventos.",
                    "You must be alone to enter the event map.",
                    "Debes estar solo para ingresar al mapa de eventos.",
                    "Vous devez être seul pour accéder à la carte de l'événement.");
                #endregion

                return false;
            }
            else
            {
                foreach (var Character in WorldServer.Instance.FindClients(x => x.Character.Map.SubArea.Id == 84))
                {
                    if (Character != null && Character.Account.Email == character.Account.Email || Character.IP == character.Client.IP)
                    {
                        listcharacters.Add(Character);
                    }
                }

                if (listcharacters.Count() >= 1)
                {
                    #region MSG
                    character.SendServerMessageLang(
                        "Você já possui personagem participando do evento.",
                        "You already have a character participating in the event.",
                        "Ya tienes un personaje participando en el evento.",
                        "Vous avez déjà un personnage participant à l'événement.");
                    #endregion
                    return false;
                }
                else
                {
                    #region MSG
                    character.SendServerMessageLang(
                       "Você foi teleportado para dentro do mapa evento.",
                       "You have been teleported into the event map.",
                       "Has sido teletransportado al mapa de eventos.",
                       "Vous avez été téléporté sur la carte de l'événement.");
                    #endregion
                    return character.Teleport(GetPosition());
                }
            }
        }
    }
}