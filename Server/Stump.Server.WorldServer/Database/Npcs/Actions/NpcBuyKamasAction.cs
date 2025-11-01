using System.Collections.Generic;
using System.ComponentModel;
using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Items.Shops;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Dialogs.Npcs;
using Stump.Server.WorldServer.Game.Items;

namespace Stump.Server.WorldServer.Database.Npcs.Actions
{
    [Discriminator(Discriminator, typeof(NpcActionDatabase), typeof(NpcActionRecord))]
    public class NpcBuyKamasAction : NpcActionDatabase
    {
        private ItemTemplate m_token;
        private List<NpcItem> m_items;
        public const string Discriminator = "ShopKamas";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public NpcBuyKamasAction(NpcActionRecord record) : base(record)
        { }

        /// <summary>
        /// Read-only
        /// </summary>
        public List<NpcItem> Items
        {
            get { return m_items ?? (m_items = ItemManager.Instance.GetNpcShopItems(Record.Id)); }
        }
        /// <summary>
        /// Parameter 0
        /// </summary>
        public int TokenId
        {
            get { return Record.GetParameter<int>(0, true); }
            set { Record.SetParameter(0, value); }
        }
        public ItemTemplate Token
        {
            get { return TokenId > 0 ? m_token ?? (m_token = ItemManager.Instance.TryGetTemplate(TokenId)) : null; }
            set
            {
                m_token = value;
                TokenId = value == null ? 0 : m_token.Id;
            }
        }
        /// <summary>
        /// Parameter 1
        /// </summary>
        [DefaultValue(1)]
        public bool CanSell
        {
            get { return Record.GetParameter<int>(1, true) == 1; }
            set { Record.SetParameter(1, value); }
        }
        /// <summary>
        /// Parameter 2
        /// </summary>
        public bool MaxStats
        {
            get { return Record.GetParameter<int>(2, true) == 1; }
            set { Record.SetParameter(2, value); }
        }
        public override NpcActionTypeEnum[] ActionType
        {
            get
            {
                return new[]
                {
                    NpcActionTypeEnum.ACTION_SELL_KAMAS,
                };
            }
        }

        public override void Execute(Npc npc, Character character)
        {
            var dialog = new NpcShopDialogLogger(character, npc, Items, Token)
            {
                CanSell = CanSell,
                MaxStats = MaxStats
            };

            dialog.Open();

            //PopUp de Alerta com informações dos itens apresentados na loja.
            #region >> Npcs : Armas, Sapateiro, Joalheiro, Alfaiate, Escudos
            if (NpcId == 7005 || NpcId == 7007 || NpcId == 7008 || NpcId == 7009 || NpcId == 7010 && npc.Spawn.MapId == 192413698)
            {
                if (Settings.NpcExoEffects == true)
                {
                    switch (character.Account.Lang)
                    {
                        case "fr":
                            character.OpenPopup("Tous les articles vendus dans cette boutique sont livrés avec : \n 1 - Statut aléatoire \n 2 - Verrouillé sur ForgeMagic \n 3 - Verrouillé sur le personnage que vous achetez.", "Boutique des Kamas", 1);
                            break;
                        case "es":
                            character.OpenPopup("Todos los artículos vendidos en esta tienda se entregan con: \n 1 - Estado aleatorio \n 2 - Bloqueado para ForgeMagic \n 3 - Bloqueado para el personaje que compras.", "Tienda de Kamas", 1);
                            break;
                        case "en":
                            character.OpenPopup("All items sold in this store are delivered with: \n 1 - Random Status \n 2 - Locked to ForgeMagic \n 3 - Locked to the Character you Buy.", "Kamas Store", 1);
                            break;
                        default:
                            character.OpenPopup("Todos itens vendidos nessa loja são entregues com: \n 1 - Status Aleatórios \n 2 - Bloqueados para ForjaMagia \n 3 - Bloqueados no Personagem que Comprar.", "Loja Kamas", 1);
                            break;
                    }
                }
            }
            #endregion
        }
    }
}