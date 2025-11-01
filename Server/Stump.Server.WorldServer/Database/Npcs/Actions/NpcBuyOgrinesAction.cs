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
    public class NpcBuyOgrinesAction : NpcActionDatabase
    {
        public const string Discriminator = "ShopOgrines";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private List<NpcItem> m_items;
        private ItemTemplate m_token;

        public NpcBuyOgrinesAction(NpcActionRecord record) : base(record)
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
            get { return new[] { NpcActionTypeEnum.ACTION_SELL_OGRINES }; }
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
                    #region >> Popup de MSG com NPC Exo Ativo
                    switch (character.Account.Lang)
                    {
                        case "fr":
                            character.OpenPopup("Tous les articles vendus dans cette boutique sont livrés avec : \n 1 - Statut aléatoire \n 2 - Exos aléatoires \n VIP : 1 Exo \n VIP Gold : 2 Exos", "Boutique des Ogrines", 1);
                            break;
                        case "es":
                            character.OpenPopup("Todos los artículos vendidos en esta tienda se entregan con: \n 1 - Estado aleatorio \n 2 - Exos aleatorios \n VIP: 1 Exo \n Gold VIP: 2 Exos", "Tienda de Ogrinas", 1);
                            break;
                        case "en":
                            character.OpenPopup("All items sold in this store are delivered with: \n 1 - Random Status \n 2 - Random Exos \n VIP: 1 Exo \n Gold VIP: 2 Exos", "Ogrines Store", 1);
                            break;
                        default:
                            character.OpenPopup("Todos itens vendidos nessa loja são entregues com: \n 1 - Status Aleatórios \n 2 - Exos Aleátorios \n VIP : 1 Exo \n Gold VIP: 2 Exos", "Loja Ogrines", 1);
                            break;
                    }
                    #endregion
                }
                else
                {
                    #region >> Popup de MSG sem NPC Exo Ativo
                    switch (character.Account.Lang)
                    {
                        case "fr":
                            character.OpenPopup("Tous les articles vendus dans cette boutique sont livrés avec : \n 1 - Statut Aléatoire", "Boutique des Ogrines", 1);
                            break;
                        case "es":
                            character.OpenPopup("Todos los artículos vendidos en esta tienda se entregan con: \n 1 - Estado Aleatorio", "Tienda de Ogrinas", 1);
                            break;
                        case "en":
                            character.OpenPopup("All items sold in this store are delivered with: \n 1 - Random Status", "Ogrines Store", 1);
                            break;
                        default:
                            character.OpenPopup("Todos itens vendidos nessa loja são entregues com: \n 1 - Status Aleatórios", "Loja Ogrines", 1);
                            break;
                    }
                    #endregion
                }
            }
            #endregion
        }
    }
}