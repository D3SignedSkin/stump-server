using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Handlers.Dialogs;
using Stump.Server.WorldServer.Game.Dialogs;
using Stump.DofusProtocol.Types;

namespace Stump.Server.WorldServer.Game.Misc
{
    public class DonjonZaapDialog : IDialog
    {
        readonly List<Map> m_destinations = new List<Map>(){
                    World.Instance.GetMap(96994817), //Calabouço das Larvas
                    World.Instance.GetMap(87295489), //Calabouço dos Ferreiros
                    World.Instance.GetMap(152829952),//Cripta do Kardorim 
                    World.Instance.GetMap(190449664),//Granja do Girassol Faminto
                    World.Instance.GetMap(193725440),//Castelo Arenoso
                    World.Instance.GetMap(121373185),//Pátio do Papatudo Real
                    World.Instance.GetMap(163578368),//Casa Fantasma
                    World.Instance.GetMap(94110720), //Calabouço dos Scarafolhas
                    World.Instance.GetMap(87033344), //Calabouço dos Skeletos
                    World.Instance.GetMap(96338946), //Calabouço dos Tofus
                    World.Instance.GetMap(146675712),//Esconderijo do Karoblatta
                    World.Instance.GetMap(17564931), //Caverna dos Bulbos
                    World.Instance.GetMap(104595969),//Calabouço dos Bworks
                    World.Instance.GetMap(5243139),  //Gruta Hesca
                    World.Instance.GetMap(64749568), //Ninho do Kwakwa
                    World.Instance.GetMap(166986752),//Claustro dos Blops
                    World.Instance.GetMap(98566657), //Gelatésima Dimensão
                    World.Instance.GetMap(157548544),//Vilarejo Kanibola
                    World.Instance.GetMap(116392448),//Castelo do Wei Wabbit
                    World.Instance.GetMap(106954752),//Picos Rochosos dos Smagadores
                    World.Instance.GetMap(176947200),//Laboratório de Brumen Tinctorias
                    World.Instance.GetMap(22282240), //Porão da Arca de Otomai
                    World.Instance.GetMap(116654593),//Toca do Wei Wabbit
                    World.Instance.GetMap(79430145), //Toca do Daigoro
                    World.Instance.GetMap(174326272),//Cemitério dos Mastodontes
                    World.Instance.GetMap(149684224),//Domínio Ancestral
                    World.Instance.GetMap(149160960),//Antro da Rainha Donarânia
                    World.Instance.GetMap(157024256),//Barco do Tchuke
                    World.Instance.GetMap(181665792),//Tenda dos Riktus Mágikus
                    World.Instance.GetMap(72352768), //Antro do Dragão-Porco
                    World.Instance.GetMap(155713536),//Toca do Mulobo
                    World.Instance.GetMap(107216896),//Caverna do Tronkoso
                    World.Instance.GetMap(118226944),//Teatro de Dramak
                    World.Instance.GetMap(130286592),//Fábrica do Malefisko
                    World.Instance.GetMap(157286400),//Árvore de Moon
                    World.Instance.GetMap(159125512),//Biblioteca do Mestre Corvoc
                    World.Instance.GetMap(22808576), //Canal do Rasgabola
                    World.Instance.GetMap(27000832), //Calabouço dos Ratos de Bonta
                    World.Instance.GetMap(40108544), //Calabouço dos Ratos de Brakmar
                    World.Instance.GetMap(161743872),//Miausoléu do Repulgnante
                    World.Instance.GetMap(34473474), //Centro do Labirinto do Minotoror
                    World.Instance.GetMap(198968320),//Calabouço dos Dragovos
                    World.Instance.GetMap(17302528), //Toca dos Pandikazes
                    World.Instance.GetMap(107481088),//Toca do Skonk
                    World.Instance.GetMap(96338948), //Tofuleiro Real
                    World.Instance.GetMap(55050240), //Estufa do Papamute Real
                    World.Instance.GetMap(143138823),//Megálito de Fraktal
                    World.Instance.GetMap(18088960), //Calabouço dos Kitsunes
                    World.Instance.GetMap(132907008),//Viveiro do Altostruz
                    World.Instance.GetMap(136578048),//Ringue do Capitão Skarlat
                    World.Instance.GetMap(174064128),//Caverna de El Piko
                    World.Instance.GetMap(149423104),//Clareira do Carvalho Mole
                    World.Instance.GetMap(16516867), //Calabouço dos Fogofox
                    World.Instance.GetMap(89391104), //Laboratório do Tynril
                    World.Instance.GetMap(56098816), //Escavação do Pingwin Real
                    World.Instance.GetMap(102760961),//Calabouço dos Ratos do Castelo de Amakna
                    World.Instance.GetMap(56360960), //Destroços do Ogrolandês Avoado
                    World.Instance.GetMap(130548736),//Galeria do Perfuror
                    World.Instance.GetMap(21495808), //Dossel do Kimbo
                    World.Instance.GetMap(34472450), //Sala do Minotot
                    World.Instance.GetMap(57148161), //Hipogeu do Obsidemônio
                    World.Instance.GetMap(125831681),//Gruta da Kanígrula
                    World.Instance.GetMap(162004992),//Platô do Ush
                    World.Instance.GetMap(59511808), //Toca Gelifox
                    World.Instance.GetMap(143917569),//Relogium de XLII
                    World.Instance.GetMap(176030208),//Tripa do Ver Medo
                    World.Instance.GetMap(104333825),//Gruta do Bworker
                    World.Instance.GetMap(182327297),//Templo do Grande Ugah
                    World.Instance.GetMap(26738688), //Antro do Kralamor Gigante
                    World.Instance.GetMap(62915584), //Antro do Kwentro
                    World.Instance.GetMap(136840192),//Porão do Toxolias
                    World.Instance.GetMap(182453248),//Templo maldito de Araknas
                    World.Instance.GetMap(61865984), //Cavernas do Kolosso
                    World.Instance.GetMap(62130696), //Antecâmara dos Glursos
                    World.Instance.GetMap(57934593), //Calabouço da Mina de Kifril
                    World.Instance.GetMap(123207680),//Pirâmide do Sombra
                    World.Instance.GetMap(179568640),//Acampamento do Conde Razof
                    World.Instance.GetMap(110100480),//Transportador de Sylargh
                    World.Instance.GetMap(110362624),//Salões privados de Klim
                    World.Instance.GetMap(109838849),//Forjafria de Missiz Frizz
                    World.Instance.GetMap(109576705),//Laboratório de Nileza
                    World.Instance.GetMap(112201217),//Calabouço do Conde Traspafrent
                    World.Instance.GetMap(119277057),//Aquódromo do Merkator
                    World.Instance.GetMap(129500160),//Palácio do Rei Nidas
                    World.Instance.GetMap(137102336),//Trono da Corte Sombria
                    World.Instance.GetMap(140771328),//Barriga da Baleia
                    World.Instance.GetMap(143393281),//Olho de Vórtex
                    World.Instance.GetMap(160564224),//Desafio do Gatolho
                    World.Instance.GetMap(169869312),//Navio do Capitão Meno
                    World.Instance.GetMap(169345024),//Templo de Katulu
                    World.Instance.GetMap(169607168),//Palácio de Dentinea
                    World.Instance.GetMap(176160768),//Câmara de Tal Kasha
                    World.Instance.GetMap(182714368),//Mansão de Dezist
                    World.Instance.GetMap(184690945),//Mirante de Ilyzaelle
                    World.Instance.GetMap(187432960),//Torre de Bethel
                    World.Instance.GetMap(187957506),//Torre de Solar
                    World.Instance.GetMap(195035136),//Cervejaria do rei Dazak
                    World.Instance.GetMap(106430464),//Forja dos Waddicts
                    World.Instance.GetMap(199491584),//Covil do Kharnossauro
                    World.Instance.GetMap(199753728),//Prova de Draegnerys
                    World.Instance.GetMap(198705152),//Santuário de Torkelônia
        };

        public DonjonZaapDialog(Character character, InteractiveObject zaap)
        {
            Character = character;
            Zaap = zaap;
        }

        public DonjonZaapDialog(Character character)
        {
            Character = character;
        }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_TELEPORTER;

        public Character Character
        {
            get;
        }

        public bool UseTp
        {
            get;
            set;
        }


        public InteractiveObject Zaap
        {
            get;
        }

        public void Open()
        {
            Character.SetDialog(this);
            SendZaapListMessage(Character.Client);
        }

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void Teleport(Map map, int cell)
        {
            if (Character.Account.IsSubscribe == false && Character.WorldAccount.LastDungeonDate != null && Character.WorldAccount.LastDungeonDate.Date == DateTime.Now.Date)
            {
                Character.OpenPopup("Você não pode usar o teletransporte novamente hoje");
                return;
            }

            if (Character.Map.IsDungeon())
            {
                Character.OpenPopup("Você não pode usar o teletransporte dentro de uma Dungeon.");
                return;
            }

            Character.Record.MapBeforeDungeonId = Character.Map.Id;
            Character.Record.CellBeforeDungeonId = Character.Cell.Id;
            Character.Teleport(map, map.GetCell(cell));

            if (!UseTp)
            {
                var cost = GetCostTo(map);

                if (Character.Kamas < cost)
                    return;

                Character.Inventory.SubKamas(cost);
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, cost);//Você perdeu 1% kamas
            }
            Close();
        }

        public void SendZaapListMessage(IPacketReceiver client)
        {
            client.Send(new ZaapDestinationsMessage((sbyte)TeleporterTypeEnum.TELEPORTER_ZAAP,
                m_destinations.Select
                (entry => new TeleportDestination((sbyte)TeleporterTypeEnum.TELEPORTER_ZAAP,
                entry.Id,
                (ushort)entry.SubArea.Id,
                (ushort)entry.SubArea.Record.Level,
                (ushort)GetCostTo(entry))).ToArray(),
                Character.Record.SpawnMapId ?? 0));
        }

        public short GetCostTo(Map map)
        {
            var pos = map.Position;
            var pos2 = Character.Map.Position;

            return (short)Math.Floor(Math.Sqrt(((pos2.X - pos.X) * (pos2.X - pos.X) + (pos2.Y - pos.Y) * (pos2.Y - pos.Y)) * 10) + 95000);
        }
    }
}