using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Items.Player.Custom.LivingObjects;
using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Core.Extensions;
using Stump.Core.Reflection;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps.Cells.Shapes;
using Stump.Server.WorldServer.Game.HavenBags;
using Stump.Server.WorldServer.Game.Items.Player.Custom;
using Stump.Server.WorldServer.Game.Fairy;
using Database.Mandatory;
using MongoDB.Bson;
using Stump.Server.BaseServer.Logging;
using System.Globalization;
using Stump.Server.WorldServer.Game.Formulas;
using Stump.Server.WorldServer.Game.Maps.Paddocks;
using System.Web.UI;
using Accord.Statistics.Kernels;
using Stump.Server.WorldServer.Game.Items.Player.Custom.CeremonialRings;

namespace Stump.Server.WorldServer.Handlers.Inventory
{
    public partial class InventoryHandler
    {
        public const int MimicryId = 14485;
        static readonly int[] BworkerItens = { 6813, 6812, 6811, 6807, 6805, 6804, 6799 };
        static readonly int[] itemsToGift = { 6887, 6811, 6807, 6805, 6804, 6799, 6813, 6812 };
        static readonly int[] PresenteNataw = { 1546, 1550, 1548, 11654, 11650, 11660, 11666, 11658, 11662, 1547, 1545, 1549, 7941, 7942, 7943, 7944, 7945, 7946, 7947, 7948, 7949, 7950, 7951, 7953, 7954, 7955, 7956, 7957, 7958, 7959, 7960, 7961, 7962, 7963, 312, 313, 287, 304, 289, 381, 393, 407, 411, 412, 413, 414, 415, 416, 417, 418, 420, 424, 426, 429, 430, 400, 394, 395, 391, 384, 286, 306, 361, 362, 363, 364, 365, 366 };
        static readonly int[] PresenteDofusiano = { 7991, 7989, 7987, 7985, 7983, 7982, 7979, 7978, 7949, 7948, 7945, 7980, 7972, 7968, 7964, 7965, 7941, 7954, 7953, 7951, 7950, 7946, 7947, 16537, 16538, 16539, 17864, 17995, 16536, 16535, 16534, 16533, 16532, 16530, 16529, 16528, 11113, 11112, 7943, 7944, 7952, 7955, 7957, 7959, 7960, 7966, 7967, 7969, 7970, 7973, 7974, 7975, 7976, 7977, 7984, 7984, 7986, 7988, 7989, 7990, 7992, 7993, 7994, 7995, 7996, 8081 };
        static readonly int[] PresenteSurpreendente = { 14635, 13665, 12737, 13366, 12738, 13367, 720, 721, 9200, 10604, 14048, 719, 718, 9201, 1575, 9690, 798, 683, 686, 802, 809, 806, 13665, 12737, 13366, 12738, 13367, 720, 721, 9200, 10604, 14048, 719, 718, 9201, 1575, 9690, 798, 683, 686, 802, 809, 806, 18523, 16073, 12132, 6962, 8334, 17274, 17275, 1557, 1558, 7508, 30349, 30347, 15237, 30348, 17864, 17995 };
        static readonly int[] PresenteSuperpoderoso = { 17621, 17624, 17567, 17626, 13157, 15219, 15271, 15181, 15177, 15182, 15176, 13982, 13168, 15219, 17987, 17986, 17988, 17989, 17982, 17864, 17995, 17979, 17977, 17981, 17976, 14864, 17994, 15689, 17993, 17991, 17992, 15445, 17980, 17975, 15178, 15179, 17973, 15177, 15180, 13154, 13923, 17118, 17976, 19237, 19232, 19233, 17021, 15039, 17022, 13936, 13995, 18734, 14921, 16460, 17620, 11943, 15718, 15719, 15733, 15715, 15720, 15717, 13935, 15811, 15716, 18539, 18541, 18543, 18542, 12740, 18540, 18537, 18538, 13156, 13947, 13155 };
        private static readonly int[] SpellsFatalId = { 7261, 7262, 7355, 7356, 7357, 7358, 10118, 10119, 10120, 10121, 10122, 10123, 10124, 10125, 10126, 10127, 10128, 10129 };

        [WorldHandler(ObjectSetPositionMessage.Id)]
        public static void HandleObjectSetPositionMessage(WorldClient client, ObjectSetPositionMessage message)
        {
            if (!Enum.IsDefined(typeof(CharacterInventoryPositionEnum), (int)message.position))
                return;

            var item = client.Character.Inventory.TryGetItem((int)message.objectUID);

            if (item == null)
                return;

            //Bloqueio de Equipar Pedras de Almas em Lutas - A Equipagem da mesma em luta faz não capturar a alma.
            if (client.Character.IsInFight() && item.Template.TypeId == 83)
                return;

            // Bloqueio para Não equipar Companheiros - TODO
            if (item.Template.TypeId == 169)
            {
                switch (client.Character.Account.Lang)
                {
                    case "fr":
                        client.Character.SendServerMessage("Nous améliorons le système compagnon. Veuillez attendre une nouvelle mise à jour pour l'utiliser à nouveau.", System.Drawing.Color.Red);
                        break;
                    case "es":
                        client.Character.SendServerMessage("Estamos mejorando el sistema complementario. Espere una nueva actualización para volver a usarlo.", System.Drawing.Color.Red);
                        break;
                    case "en":
                        client.Character.SendServerMessage("We are improving the companion system.. please wait for a new update to use again.", System.Drawing.Color.Red);
                        break;
                    default:
                        client.Character.SendServerMessage("Estamos melhorando o sistema dos companheiro.. por favor aguarde uma nova atualização para voltar a usar.", System.Drawing.Color.Red);
                        break;
                }

                return;
            }

            client.Character.Inventory.MoveItem(item, (CharacterInventoryPositionEnum)message.position);
        }

        [WorldHandler(ObjectDeleteMessage.Id)]
        public static void HandleObjectDeleteMessage(WorldClient client, ObjectDeleteMessage message)
        {
            var item = client.Character.Inventory.TryGetItem((int)message.objectUID);

            if (item == null)
                return;

            client.Character.Inventory.RemoveItem(item, (int)message.quantity);
        }
        private static void Lang(WorldClient client, int id)
        {
            string saco = "";
            switch (id)
            {

                case 1:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Kerubim";
                            break;
                        case "es":
                            saco = "Kerubim";
                            break;
                        case "en":
                            saco = "Kerub";
                            break;
                        default:
                            saco = "Kerubim";
                            break;
                    }
                    break;
                case 2:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Amakna";
                            break;
                        case "es":
                            saco = "Amakna";
                            break;
                        case "en":
                            saco = "Amakna";
                            break;
                        default:
                            saco = "Amakna";
                            break;
                    }
                    break;
                case 3:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Nowel";
                            break;
                        case "es":
                            saco = "Nawidad";
                            break;
                        case "en":
                            saco = "Kwismas";
                            break;
                        default:
                            saco = "Nataw";
                            break;
                    }
                    break;
                case 4:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Allister";
                            break;
                        case "es":
                            saco = "Allister";
                            break;
                        case "en":
                            saco = "Allister";
                            break;
                        default:
                            saco = "Allister";
                            break;
                    }
                    break;
                case 5:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Printemps";
                            break;
                        case "es":
                            saco = "Primavera";
                            break;
                        case "en":
                            saco = "Spring";
                            break;
                        default:
                            saco = "Primavera";
                            break;
                    }
                    break;
                case 6:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Sain Ballotin";
                            break;
                        case "es":
                            saco = "San Valentón";
                            break;
                        case "en":
                            saco = "Saint Ballotwine";
                            break;
                        default:
                            saco = "São Rosadim";
                            break;
                    }
                    break;
                case 7:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Hiver";
                            break;
                        case "es":
                            saco = "Invierno";
                            break;
                        case "en":
                            saco = "Winter";
                            break;
                        default:
                            saco = "Inverno";
                            break;
                    }
                    break;
                case 9:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Wabbit";
                            break;
                        case "es":
                            saco = "Wabbit";
                            break;
                        case "en":
                            saco = "Wabbit";
                            break;
                        default:
                            saco = "Wabbit";
                            break;
                    }
                    break;
                case 10:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Taverne";
                            break;
                        case "es":
                            saco = "Taberna";
                            break;
                        case "en":
                            saco = "Inn";
                            break;
                        default:
                            saco = "Taberna";
                            break;
                    }
                    break;
                case 11:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Casino";
                            break;
                        case "es":
                            saco = "Casino";
                            break;
                        case "en":
                            saco = "Casino";
                            break;
                        default:
                            saco = "Cassino";
                            break;
                    }
                    break;
                case 12:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Princesse";
                            break;
                        case "es":
                            saco = "Princesa";
                            break;
                        case "en":
                            saco = "Princess";
                            break;
                        default:
                            saco = "Princesa";
                            break;
                    }
                    break;
                case 14:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Pwâk";
                            break;
                        case "es":
                            saco = "Pazcuak";
                            break;
                        case "en":
                            saco = "Fleaster";
                            break;
                        default:
                            saco = "Paskwak";
                            break;
                    }
                    break;
                case 15:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Fin Patraque";
                            break;
                        case "es":
                            saco = "San Patírico";
                            break;
                        case "en":
                            saco = "Saint Potrick";
                            break;
                        default:
                            saco = "Pão Satrício";
                            break;
                    }
                    break;
                case 16:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Fanfon";
                            break;
                        case "es":
                            saco = "Do de Pecho";
                            break;
                        case "en":
                            saco = "Gobbstock";
                            break;
                        default:
                            saco = "Fanfão";
                            break;
                    }
                    break;
                case 17:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Été";
                            break;
                        case "es":
                            saco = "Verano";
                            break;
                        case "en":
                            saco = "Summer";
                            break;
                        default:
                            saco = "Verão";
                            break;
                    }
                    break;
                case 18:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Steamer";
                            break;
                        case "es":
                            saco = "Steamer";
                            break;
                        case "en":
                            saco = "Foggernaut";
                            break;
                        default:
                            saco = "Steamer";
                            break;
                    }
                    break;
                case 19:
                    switch (client.Character.Account.Lang)
                    {
                        case "fr":
                            saco = "Sidimote";
                            break;
                        case "es":
                            saco = "Sidimote";
                            break;
                        case "en":
                            saco = "Sidimote";
                            break;
                        default:
                            saco = "Sidimote";
                            break;
                    }
                    break;

            }
            if (id != 0)
            {
                switch (client.Character.Account.Lang)
                {
                    case "fr":
                        client.Character.SendServerMessage("Vous avez désormais accès au thème havre-sac " + saco + ".", System.Drawing.Color.DarkOrange);
                        break;
                    case "es":
                        client.Character.SendServerMessage("Ahora tienes acceso al tema de la merkasako de " + saco + ".", System.Drawing.Color.DarkOrange);
                        break;
                    case "en":
                        client.Character.SendServerMessage("You now have access to the " + saco + " haven bag theme.", System.Drawing.Color.DarkOrange);
                        break;
                    default:
                        client.Character.SendServerMessage("Agora você tem acesso ao tema do saco de viagem de " + saco + ".", System.Drawing.Color.DarkOrange);
                        break;
                }
            }
            else
            {
                switch (client.Character.Account.Lang)
                {
                    case "fr":
                        client.Character.SendServerMessage("Ce personnage a déjà ce thème.", System.Drawing.Color.DarkOrange);
                        break;
                    case "es":
                        client.Character.SendServerMessage("Este personaje ya tiene este tema.", System.Drawing.Color.DarkOrange);
                        break;
                    case "en":
                        client.Character.SendServerMessage("This character already has this theme.", System.Drawing.Color.DarkOrange);
                        break;
                    default:
                        client.Character.SendServerMessage("Esse personagem ja possuí este tema.", System.Drawing.Color.DarkOrange);
                        break;
                }
            }
        }

        [WorldHandler(ObjectUseMessage.Id)]
        public static void HandleObjectUseMessage(WorldClient client, ObjectUseMessage message)
        {
            try
            {
                var basePlayerItem = client.Character.Inventory.TryGetItem((int)message.objectUID);

                if (basePlayerItem != null)
                {
                    if (basePlayerItem is TeleportPotion)
                    {
                        var timePotion = DateTime.Now.AddSeconds(5).GetUnixTimeStampLong();

                        client.Character.Area.CallDelayed(5000, () => client.Character.Inventory.UseItem(basePlayerItem));
                        client.Character.Map.Clients.Send(new GameRolePlayDelayedObjectUseMessage(client.Character.Id, (sbyte)DelayedActionTypeEnum.DELAYED_ACTION_OBJECT_USE, timePotion, (ushort)basePlayerItem.Template.Id));
                    }
                    else
                    {
                        client.Character.Inventory.UseItem(basePlayerItem);
                    }

                    switch (basePlayerItem.Template.Id)
                    {
                        #region// ----------------- Sacos de Viagens ---------------- //
                        case 27464:
                            client.Character.AddEmote(EmotesEnum.EMOTE_APPLAUDIR);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 20038:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 1) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 1);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 1);
                            }

                            break;
                        case 20039:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 2) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 2);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 2);
                            }

                            break;
                        case 20040:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 3) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 3);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 3);
                            }

                            break;
                        case 20042:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 4) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 4);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 4);
                            }

                            break;
                        case 20043:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 5) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 5);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 5);
                            }

                            break;
                        case 20044:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 6) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 6);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 6);
                            }

                            break;
                        case 20046:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 7) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 7);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 7);
                            }

                            break;
                        case 20047:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 9) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 9);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 9);
                            }

                            break;
                        case 20048:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 10) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 10);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 10);
                            }

                            break;
                        case 20049:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 11) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 11);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 11);
                            }

                            break;
                        case 20051:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 12) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 12);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 12);
                            }

                            break;
                        case 20052:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 14) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 14);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 14);
                            }

                            break;
                        case 20053:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 15) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 15);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 15);
                            }

                            break;
                        case 20054:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 16) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 16);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 16);
                            }

                            break;
                        case 20055:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 17) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 17);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 17);
                            }

                            break;
                        case 20056:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 18) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 18);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 18);
                            }

                            break;
                        case 20057:
                            if (HavenBagManager.Instance.HaveHavenBag(client.Character, 19) == true)
                            {
                                Lang(client, 0);
                            }
                            else
                            {
                                HavenBagManager.Instance.AddHavenBag(client.Character, 19);
                                client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                                Lang(client, 19);
                            }

                            break;
                        #endregion

                        #region // ---------------- Bolsitas ------------------//
                        case 16819:
                            var itemTemplate6 = Singleton<ItemManager>.Instance.TryGetTemplate(2331); //Berenjena
                            client.Character.Inventory.AddItem(itemTemplate6, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16820:
                            var itemTemplate13 = Singleton<ItemManager>.Instance.TryGetTemplate(1984); //Cenizas Eternas
                            client.Character.Inventory.AddItem(itemTemplate13, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16821:
                            var itemTemplate14 = Singleton<ItemManager>.Instance.TryGetTemplate(1734); //Cerezas
                            client.Character.Inventory.AddItem(itemTemplate14, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16822:
                            var itemTemplate = Singleton<ItemManager>.Instance.TryGetTemplate(1736); //Limones
                            client.Character.Inventory.AddItem(itemTemplate, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16825:
                            var itemTemplate4 = Singleton<ItemManager>.Instance.TryGetTemplate(1977); //Especias
                            client.Character.Inventory.AddItem(itemTemplate4, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16826:
                            var itemTemplate7 = Singleton<ItemManager>.Instance.TryGetTemplate(1974); //Enchalada (Lechuga e-e)
                            client.Character.Inventory.AddItem(itemTemplate7, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16827:
                            var itemTemplate9 = Singleton<ItemManager>.Instance.TryGetTemplate(1983); //Grasa Gelatinosa
                            client.Character.Inventory.AddItem(itemTemplate9, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16828:
                            var itemTemplate11 = Singleton<ItemManager>.Instance.TryGetTemplate(6671); //Alubias
                            client.Character.Inventory.AddItem(itemTemplate11, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16830:
                            var itemTemplate2 = Singleton<ItemManager>.Instance.TryGetTemplate(1978); //Pimienta
                            client.Character.Inventory.AddItem(itemTemplate2, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16831:
                            var itemTemplate3 = Singleton<ItemManager>.Instance.TryGetTemplate(1730); //Sal
                            client.Character.Inventory.AddItem(itemTemplate3, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16832:
                            var itemTemplate5 = Singleton<ItemManager>.Instance.TryGetTemplate(1975); //Cebolla
                            client.Character.Inventory.AddItem(itemTemplate5, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16833:
                            var itemTemplate8 = Singleton<ItemManager>.Instance.TryGetTemplate(519); //Polvos Magicos
                            client.Character.Inventory.AddItem(itemTemplate8, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16834:
                            var itemTemplate10 = Singleton<ItemManager>.Instance.TryGetTemplate(1986); //Polvo Temporal
                            client.Character.Inventory.AddItem(itemTemplate10, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16835:
                            var itemTemplate12 = Singleton<ItemManager>.Instance.TryGetTemplate(1985); //Resina
                            client.Character.Inventory.AddItem(itemTemplate12, 10);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;

                        #endregion

                        #region // ----------------- Sacola de Recursos By:Kenshin ---------------- //

                        case 7941: // Sacola de Trigo
                            var itemTemplate20 = Singleton<ItemManager>.Instance.TryGetTemplate(289);
                            client.Character.Inventory.AddItem(itemTemplate20, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7942: // Sacola de Cevada
                            var itemTemplate21 = Singleton<ItemManager>.Instance.TryGetTemplate(400);
                            client.Character.Inventory.AddItem(itemTemplate21, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7943: // Sacola de Aveia
                            var itemTemplate22 = Singleton<ItemManager>.Instance.TryGetTemplate(533);
                            client.Character.Inventory.AddItem(itemTemplate22, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7944: // Sacola de Lúpulo
                            var itemTemplate23 = Singleton<ItemManager>.Instance.TryGetTemplate(401);
                            client.Character.Inventory.AddItem(itemTemplate23, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7945: // Sacola de Linho
                            var itemTemplate24 = Singleton<ItemManager>.Instance.TryGetTemplate(423);
                            client.Character.Inventory.AddItem(itemTemplate24, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7946: // Sacola de Centeio
                            var itemTemplate25 = Singleton<ItemManager>.Instance.TryGetTemplate(532);
                            client.Character.Inventory.AddItem(itemTemplate25, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7947: // Sacola de Arroz
                            var itemTemplate26 = Singleton<ItemManager>.Instance.TryGetTemplate(7018);
                            client.Character.Inventory.AddItem(itemTemplate26, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7948: // Sacola de Malte
                            var itemTemplate27 = Singleton<ItemManager>.Instance.TryGetTemplate(405);
                            client.Character.Inventory.AddItem(itemTemplate27, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7949: // Sacola de Cânhamo
                            var itemTemplate28 = Singleton<ItemManager>.Instance.TryGetTemplate(425);
                            client.Character.Inventory.AddItem(itemTemplate28, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7950: // Sacola de Freixo
                            var itemTemplate29 = Singleton<ItemManager>.Instance.TryGetTemplate(303);
                            client.Character.Inventory.AddItem(itemTemplate29, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7951: // Sacola de Castanheiro
                            var itemTemplate30 = Singleton<ItemManager>.Instance.TryGetTemplate(473);
                            client.Character.Inventory.AddItem(itemTemplate30, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7952: // Sacola de Nogueira
                            var itemTemplate31 = Singleton<ItemManager>.Instance.TryGetTemplate(476);
                            client.Character.Inventory.AddItem(itemTemplate31, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7953: // Sacola de Carvalho
                            var itemTemplate32 = Singleton<ItemManager>.Instance.TryGetTemplate(460);
                            client.Character.Inventory.AddItem(itemTemplate32, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7954: // Sacola de Bombu
                            var itemTemplate33 = Singleton<ItemManager>.Instance.TryGetTemplate(2358);
                            client.Character.Inventory.AddItem(itemTemplate33, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7955: // Sacola de Olivioleta
                            var itemTemplate34 = Singleton<ItemManager>.Instance.TryGetTemplate(2357);
                            client.Character.Inventory.AddItem(itemTemplate34, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7956: // Sacola de Bordo
                            var itemTemplate35 = Singleton<ItemManager>.Instance.TryGetTemplate(471);
                            client.Character.Inventory.AddItem(itemTemplate35, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7957: // Sacola de Teixo
                            var itemTemplate36 = Singleton<ItemManager>.Instance.TryGetTemplate(461);
                            client.Character.Inventory.AddItem(itemTemplate36, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7958: // Sacola de Bambu
                            var itemTemplate37 = Singleton<ItemManager>.Instance.TryGetTemplate(7013);
                            client.Character.Inventory.AddItem(itemTemplate37, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7959: // Sacola de Cerejeira
                            var itemTemplate38 = Singleton<ItemManager>.Instance.TryGetTemplate(474);
                            client.Character.Inventory.AddItem(itemTemplate38, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7960: // Sacola de Ébano
                            var itemTemplate39 = Singleton<ItemManager>.Instance.TryGetTemplate(449);
                            client.Character.Inventory.AddItem(itemTemplate39, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7961: // Sacola de Bambu Sombrio
                            var itemTemplate40 = Singleton<ItemManager>.Instance.TryGetTemplate(7016);
                            client.Character.Inventory.AddItem(itemTemplate40, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7962: // Sacola de Olmo
                            var itemTemplate41 = Singleton<ItemManager>.Instance.TryGetTemplate(470);
                            client.Character.Inventory.AddItem(itemTemplate41, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7963: // Sacola de Bambu Sagrado
                            var itemTemplate42 = Singleton<ItemManager>.Instance.TryGetTemplate(7014);
                            client.Character.Inventory.AddItem(itemTemplate42, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7964: // Sacola de Urtigas
                            var itemTemplate43 = Singleton<ItemManager>.Instance.TryGetTemplate(421);
                            client.Character.Inventory.AddItem(itemTemplate43, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7965: // Sacola de Sálvias
                            var itemTemplate44 = Singleton<ItemManager>.Instance.TryGetTemplate(428);
                            client.Character.Inventory.AddItem(itemTemplate44, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7966: // Sacola de Trevo 5 Folhas
                            var itemTemplate45 = Singleton<ItemManager>.Instance.TryGetTemplate(395);
                            client.Character.Inventory.AddItem(itemTemplate45, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7967: // Sacola de Menta Selvagem
                            var itemTemplate46 = Singleton<ItemManager>.Instance.TryGetTemplate(380);
                            client.Character.Inventory.AddItem(itemTemplate46, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7968: // Sacola de Freyescas
                            var itemTemplate47 = Singleton<ItemManager>.Instance.TryGetTemplate(593);
                            client.Character.Inventory.AddItem(itemTemplate47, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7969: // Sacola de Edelvais
                            var itemTemplate48 = Singleton<ItemManager>.Instance.TryGetTemplate(594);
                            client.Character.Inventory.AddItem(itemTemplate48, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7970: // Sacola de Sementes Pandóbora
                            var itemTemplate49 = Singleton<ItemManager>.Instance.TryGetTemplate(7059);
                            client.Character.Inventory.AddItem(itemTemplate49, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7971: // Sacola de Ferro
                            var itemTemplate50 = Singleton<ItemManager>.Instance.TryGetTemplate(312);
                            client.Character.Inventory.AddItem(itemTemplate50, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7972: // Sacola de Cobre
                            var itemTemplate51 = Singleton<ItemManager>.Instance.TryGetTemplate(441);
                            client.Character.Inventory.AddItem(itemTemplate51, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7973: // Sacola de Bronze
                            var itemTemplate52 = Singleton<ItemManager>.Instance.TryGetTemplate(442);
                            client.Character.Inventory.AddItem(itemTemplate52, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7974: // Sacola de Kobalto
                            var itemTemplate53 = Singleton<ItemManager>.Instance.TryGetTemplate(443);
                            client.Character.Inventory.AddItem(itemTemplate53, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7975: // Sacola de Manganês
                            var itemTemplate54 = Singleton<ItemManager>.Instance.TryGetTemplate(445);
                            client.Character.Inventory.AddItem(itemTemplate54, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7976: // Sacola de Estanho
                            var itemTemplate55 = Singleton<ItemManager>.Instance.TryGetTemplate(444);
                            client.Character.Inventory.AddItem(itemTemplate55, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7977: // Sacola de Silicato
                            var itemTemplate56 = Singleton<ItemManager>.Instance.TryGetTemplate(7032);
                            client.Character.Inventory.AddItem(itemTemplate56, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7978: // Sacola de Prata
                            var itemTemplate57 = Singleton<ItemManager>.Instance.TryGetTemplate(350);
                            client.Character.Inventory.AddItem(itemTemplate57, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7979: // Sacola de Bauxita
                            var itemTemplate58 = Singleton<ItemManager>.Instance.TryGetTemplate(446);
                            client.Character.Inventory.AddItem(itemTemplate58, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7980: // Sacola de Ouro
                            var itemTemplate59 = Singleton<ItemManager>.Instance.TryGetTemplate(313);
                            client.Character.Inventory.AddItem(itemTemplate59, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7981: // Sacola de Dolomita
                            var itemTemplate60 = Singleton<ItemManager>.Instance.TryGetTemplate(7033);
                            client.Character.Inventory.AddItem(itemTemplate60, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7982: // Sacola de Gobião
                            var itemTemplate61 = Singleton<ItemManager>.Instance.TryGetTemplate(1782);
                            client.Character.Inventory.AddItem(itemTemplate61, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7983: // Sacola de Truta
                            var itemTemplate62 = Singleton<ItemManager>.Instance.TryGetTemplate(1844);
                            client.Character.Inventory.AddItem(itemTemplate62, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7984: // Sacola de Peixe Gatinho
                            var itemTemplate63 = Singleton<ItemManager>.Instance.TryGetTemplate(603);
                            client.Character.Inventory.AddItem(itemTemplate63, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7985: // Sacola de Gramarões
                            var itemTemplate64 = Singleton<ItemManager>.Instance.TryGetTemplate(598);
                            client.Character.Inventory.AddItem(itemTemplate64, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7986: // Sacola de Surimi
                            var itemTemplate65 = Singleton<ItemManager>.Instance.TryGetTemplate(1757);
                            client.Character.Inventory.AddItem(itemTemplate65, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7987: // Sacola de Empanado
                            var itemTemplate66 = Singleton<ItemManager>.Instance.TryGetTemplate(1750);
                            client.Character.Inventory.AddItem(itemTemplate66, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7988: // Sacola de Lúcio
                            var itemTemplate67 = Singleton<ItemManager>.Instance.TryGetTemplate(1847);
                            client.Character.Inventory.AddItem(itemTemplate67, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7989: // Sacola de Carpa
                            var itemTemplate68 = Singleton<ItemManager>.Instance.TryGetTemplate(1794);
                            client.Character.Inventory.AddItem(itemTemplate68, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7990: // Sacola de Sardinha
                            var itemTemplate69 = Singleton<ItemManager>.Instance.TryGetTemplate(1805);
                            client.Character.Inventory.AddItem(itemTemplate69, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7991: // Sacola de Kralamor
                            var itemTemplate70 = Singleton<ItemManager>.Instance.TryGetTemplate(600);
                            client.Character.Inventory.AddItem(itemTemplate70, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7992: // Sacola de Robalo
                            var itemTemplate71 = Singleton<ItemManager>.Instance.TryGetTemplate(1779);
                            client.Character.Inventory.AddItem(itemTemplate71, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7993: // Sacola de Arraia Farle
                            var itemTemplate72 = Singleton<ItemManager>.Instance.TryGetTemplate(1788);
                            client.Character.Inventory.AddItem(itemTemplate72, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7994: // Sacola de Perca
                            var itemTemplate73 = Singleton<ItemManager>.Instance.TryGetTemplate(1801);
                            client.Character.Inventory.AddItem(itemTemplate73, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7995: // Sacola de Martelifoice
                            var itemTemplate74 = Singleton<ItemManager>.Instance.TryGetTemplate(602);
                            client.Character.Inventory.AddItem(itemTemplate74, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 7996: // Sacola de Kaliptus
                            var itemTemplate75 = Singleton<ItemManager>.Instance.TryGetTemplate(7925);
                            client.Character.Inventory.AddItem(itemTemplate75, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 8081: // Sacola de Cárpino
                            var itemTemplate76 = Singleton<ItemManager>.Instance.TryGetTemplate(472);
                            client.Character.Inventory.AddItem(itemTemplate76, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 11103: // Sacola de Camânula Branca
                            var itemTemplate77 = Singleton<ItemManager>.Instance.TryGetTemplate(11102);
                            client.Character.Inventory.AddItem(itemTemplate77, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 11111: // Sacola de Peixelado
                            var itemTemplate78 = Singleton<ItemManager>.Instance.TryGetTemplate(11106);
                            client.Character.Inventory.AddItem(itemTemplate78, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 11112: // Sacola de Álamo
                            var itemTemplate79 = Singleton<ItemManager>.Instance.TryGetTemplate(11107);
                            client.Character.Inventory.AddItem(itemTemplate79, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 11113: // Sacola de Frostiz
                            var itemTemplate80 = Singleton<ItemManager>.Instance.TryGetTemplate(11109);
                            client.Character.Inventory.AddItem(itemTemplate80, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 11114: // Sacola de Obsidiana
                            var itemTemplate81 = Singleton<ItemManager>.Instance.TryGetTemplate(11110);
                            client.Character.Inventory.AddItem(itemTemplate81, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16528: // Sacola de Ginseng
                            var itemTemplate82 = Singleton<ItemManager>.Instance.TryGetTemplate(16385);
                            client.Character.Inventory.AddItem(itemTemplate82, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16529: // Sacola de Beladona
                            var itemTemplate83 = Singleton<ItemManager>.Instance.TryGetTemplate(16387);
                            client.Character.Inventory.AddItem(itemTemplate83, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16530: // Sacola de Mandrágora
                            var itemTemplate84 = Singleton<ItemManager>.Instance.TryGetTemplate(16389);
                            client.Character.Inventory.AddItem(itemTemplate84, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16531: // Sacola de Aveleira
                            var itemTemplate85 = Singleton<ItemManager>.Instance.TryGetTemplate(16488);
                            client.Character.Inventory.AddItem(itemTemplate85, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16532: // Sacola de Milho
                            var itemTemplate86 = Singleton<ItemManager>.Instance.TryGetTemplate(16454);
                            client.Character.Inventory.AddItem(itemTemplate86, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16533: // Sacola de Milheto
                            var itemTemplate87 = Singleton<ItemManager>.Instance.TryGetTemplate(16456);
                            client.Character.Inventory.AddItem(itemTemplate87, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16534: // Sacola de Enguias
                            var itemTemplate88 = Singleton<ItemManager>.Instance.TryGetTemplate(16461);
                            client.Character.Inventory.AddItem(itemTemplate88, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16535: // Sacola de Dourados Cinza
                            var itemTemplate89 = Singleton<ItemManager>.Instance.TryGetTemplate(16463);
                            client.Character.Inventory.AddItem(itemTemplate89, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16536: // Sacola de Pacamão
                            var itemTemplate90 = Singleton<ItemManager>.Instance.TryGetTemplate(16465);
                            client.Character.Inventory.AddItem(itemTemplate90, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16537: // Sacola de Bacalhau
                            var itemTemplate91 = Singleton<ItemManager>.Instance.TryGetTemplate(16467);
                            client.Character.Inventory.AddItem(itemTemplate91, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16538: // Sacola de Tenca
                            var itemTemplate92 = Singleton<ItemManager>.Instance.TryGetTemplate(16469);
                            client.Character.Inventory.AddItem(itemTemplate92, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16539: // Sacola de Peixe Espada
                            var itemTemplate93 = Singleton<ItemManager>.Instance.TryGetTemplate(16471);
                            client.Character.Inventory.AddItem(itemTemplate93, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 18058: // Sacola de Aquaju
                            var itemTemplate94 = Singleton<ItemManager>.Instance.TryGetTemplate(17991);
                            client.Character.Inventory.AddItem(itemTemplate94, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 18059: // Sacola de Salikrônia
                            var itemTemplate95 = Singleton<ItemManager>.Instance.TryGetTemplate(17992);
                            client.Character.Inventory.AddItem(itemTemplate95, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 18060: // Sacola de Quisnoa
                            var itemTemplate96 = Singleton<ItemManager>.Instance.TryGetTemplate(17993);
                            client.Character.Inventory.AddItem(itemTemplate96, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 18061: // Sacola de Lapa
                            var itemTemplate97 = Singleton<ItemManager>.Instance.TryGetTemplate(17994);
                            client.Character.Inventory.AddItem(itemTemplate97, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 18062: // Sacola de Sepiolita
                            var itemTemplate98 = Singleton<ItemManager>.Instance.TryGetTemplate(17995);
                            client.Character.Inventory.AddItem(itemTemplate98, 50);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;

                        #endregion

                        #region// ----------------- Baús ---------------- //
                        case 17255:
                            var random = new Random().Next(300000, 1000000); // Baú de Kamas Lottery
                            client.Character.Inventory.AddKamas(random);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 45, random);
                            break;

                        case 30389: //Sherajah
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 25796, 1);
                            UseBox(client.Character, 25795, 1);
                            UseBox(client.Character, 25797, 1);
                            UseBox(client.Character, 25798, 1);
                            UseBox(client.Character, 25807, 1);
                            break;

                        case 30341: //Pergamancia
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 25773, 1);
                            UseBox(client.Character, 25774, 1);
                            UseBox(client.Character, 25775, 1);
                            UseBox(client.Character, 25776, 1);
                            UseBox(client.Character, 25941, 1);
                            break;

                        case 30388: //Hecate
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 27376, 1);
                            UseBox(client.Character, 27377, 1);
                            UseBox(client.Character, 27378, 1);
                            UseBox(client.Character, 27379, 1);
                            break;

                        case 30346: //Okre Ball Zy
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 30342, 1);
                            UseBox(client.Character, 30343, 1);
                            UseBox(client.Character, 30344, 1);
                            UseBox(client.Character, 21479, 1);
                            UseBox(client.Character, 30345, 1);
                            break;

                        case 30390: //Reforssado
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 25978, 1);
                            UseBox(client.Character, 25979, 1);
                            UseBox(client.Character, 25981, 1);
                            UseBox(client.Character, 25990, 1);
                            UseBox(client.Character, 25293, 1);
                            break;

                        case 30391: //Paskwadores
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 26813, 1);
                            UseBox(client.Character, 26814, 1);
                            UseBox(client.Character, 26815, 1);
                            UseBox(client.Character, 26808, 1);
                            break;

                        case 30392: //Oiro
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 26743, 1);
                            UseBox(client.Character, 26744, 1);
                            UseBox(client.Character, 26745, 1);
                            UseBox(client.Character, 26746, 1);
                            UseBox(client.Character, 26748, 1);
                            break;

                        case 30393: //Ovivmet
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 9234, 1);
                            UseBox(client.Character, 9233, 1);
                            UseBox(client.Character, 9255, 1);
                            UseBox(client.Character, 9256, 1);
                            break;

                        case 30394: //Parabiotico
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 12424, 1);
                            UseBox(client.Character, 12425, 1);
                            UseBox(client.Character, 12426, 1);
                            UseBox(client.Character, 12427, 1);
                            break;

                        case 30395: //Rolevivo
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 13213, 1);
                            UseBox(client.Character, 13211, 1);
                            UseBox(client.Character, 13212, 1);
                            UseBox(client.Character, 13210, 1);
                            break;

                        case 30396: //Shushuvivo
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 18154, 1);
                            UseBox(client.Character, 18155, 1);
                            break;

                        case 30397: //Colorvivo v1
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 19526, 1);
                            UseBox(client.Character, 19524, 1);
                            UseBox(client.Character, 19525, 1);
                            break;

                        case 30398: //Colorvivo v2
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 30332, 1);
                            UseBox(client.Character, 30333, 1);
                            UseBox(client.Character, 30334, 1);
                            break;

                        case 30399: //Colorvivo v3
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 26260, 1);
                            UseBox(client.Character, 26261, 1);
                            UseBox(client.Character, 26262, 1);
                            UseBox(client.Character, 26289, 1);
                            break;

                        case 30403: //Presença Aquoso
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 25121, 1);
                            UseBox(client.Character, 27395, 1);
                            UseBox(client.Character, 25374, 1);
                            UseBox(client.Character, 27730, 1);
                            break;

                        case 30404: //Presença Vegetal
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 25123, 1);
                            UseBox(client.Character, 27393, 1);
                            UseBox(client.Character, 25375, 1);
                            UseBox(client.Character, 27732, 1);
                            break;

                        case 30405: //Presença Volatil
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 25124, 1);
                            UseBox(client.Character, 27394, 1);
                            UseBox(client.Character, 25376, 1);
                            UseBox(client.Character, 27729, 1);
                            break;

                        case 30406: //Presença Ignea
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 25122, 1);
                            UseBox(client.Character, 27396, 1);
                            UseBox(client.Character, 25373, 1);
                            UseBox(client.Character, 27731, 1);
                            break;

                        case 30410: //Guardião das Profundeza
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 30407, 1);
                            UseBox(client.Character, 30408, 1);
                            UseBox(client.Character, 19725, 1);
                            UseBox(client.Character, 30315, 1);
                            UseBox(client.Character, 10861, 1);
                            break;

                        case 30412: //Brumario
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 27685, 1);
                            UseBox(client.Character, 27684, 1);
                            UseBox(client.Character, 27683, 1);
                            UseBox(client.Character, 27682, 1);
                            break;

                        case 30413: //Cavaleiro de astrub
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 27758, 1);
                            UseBox(client.Character, 27761, 1);
                            UseBox(client.Character, 27762, 1);
                            UseBox(client.Character, 27801, 1);
                            UseBox(client.Character, 27719, 1);
                            break;

                        case 30414: //Set Djaul
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            UseBox(client.Character, 27699, 1);
                            UseBox(client.Character, 27702, 1);
                            UseBox(client.Character, 27701, 1);
                            UseBox(client.Character, 27668, 1);
                            break;
                        #endregion

                        #region  // ----------------- Tonel ---------------- //
                        case 16823:
                            var itemTemplate18 = Singleton<ItemManager>.Instance.TryGetTemplate(1731); //Zumo Sabroso
                            client.Character.Inventory.AddItem(itemTemplate18, 15);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16824:
                            var itemTemplate15 = Singleton<ItemManager>.Instance.TryGetTemplate(311); //Agua
                            client.Character.Inventory.AddItem(itemTemplate15, 15);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16829:
                            var itemTemplate17 = Singleton<ItemManager>.Instance.TryGetTemplate(1973); //Aceite para freir
                            client.Character.Inventory.AddItem(itemTemplate17, 15);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 16836:
                            var itemTemplate16 = Singleton<ItemManager>.Instance.TryGetTemplate(2012); //Sangre de Scorbuto
                            client.Character.Inventory.AddItem(itemTemplate16, 15);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 684:
                            var itemTemplate19 = Singleton<ItemManager>.Instance.TryGetTemplate(12736); //Pergaminho de kolifichas
                            client.Character.Inventory.AddItem(itemTemplate19, 1000);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;

                        #endregion

                        #region// ----------------- Pociones ---------------- //
                        case 6965:
                            tpPlayer(client.Character, 5506048, 359, DirectionsEnum.DIRECTION_EAST);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 6964:
                            tpPlayer(client.Character, 13631488, 370, DirectionsEnum.DIRECTION_EAST);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        case 996: //Multigly
                            tpPlayer(client.Character, 98566657, 43, DirectionsEnum.DIRECTION_EAST);
                            client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                            break;
                        //case 548:  HAD Effect_TeleportToSavePoint
                        //    var objectPosition = client.Character.GetSpawnPoint();
                        //    var NextMap = objectPosition.Map;
                        //    var Cell = objectPosition.Cell;
                        //    client.Character.Teleport(NextMap, Cell);
                        //    client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                        //    break;
                        case MimicryId:
                            client.Character.Inventory.Save();
                            client.Send(new ClientUIOpenedByObjectMessage(3, (uint)basePlayerItem.Guid));
                            break;
                        case 14426: // KoH Agression
                            var timePotion = DateTime.Now.AddSeconds(5).GetUnixTimeStampLong();
                            client.Character.Area.CallDelayed(5000, () => UseItem(client));
                            client.Character.Map.Clients.Send(new GameRolePlayDelayedObjectUseMessage(client.Character.Id, (sbyte)DelayedActionTypeEnum.DELAYED_ACTION_OBJECT_USE, timePotion, (ushort)basePlayerItem.Template.Id));
                            break;
                            #endregion
                    }

                    #region// ----------------- Itens Surpresas ---------------- //
                    if (itemsToGift.Contains(basePlayerItem.Template.Id))
                    {
                        var random = new Random().Next(0, 8);
                        var ItemAdd = Singleton<ItemManager>.Instance.TryGetTemplate(BworkerItens[random]); //Suprise set Browk for tokens
                        client.Character.Inventory.AddItem(ItemAdd, 1);
                        client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                    }

                    if (basePlayerItem.Template.Id == 12132)
                    {
                        var random = new Random().Next(0, 8);
                        var ItemAdd = Singleton<ItemManager>.Instance.TryGetTemplate(PresenteNataw[random]); //Presente de Nataw
                        client.Character.Inventory.AddItem(ItemAdd, 1);
                        client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                    }

                    if (basePlayerItem.Template.Id == 6962)
                    {
                        var random = new Random().Next(0, 8);
                        var ItemAdd = Singleton<ItemManager>.Instance.TryGetTemplate(PresenteDofusiano[random]); //Presente Dofusiano
                        client.Character.Inventory.AddItem(ItemAdd, 1);
                        client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                    }

                    if (basePlayerItem.Template.Id == 10912)
                    {
                        var random = new Random().Next(0, 8);
                        var ItemAdd = Singleton<ItemManager>.Instance.TryGetTemplate(PresenteSurpreendente[random]); //Presente de Nataw surpreendente
                        client.Character.Inventory.AddItem(ItemAdd, 1);
                        client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                    }

                    if (basePlayerItem.Template.Id == 10914)
                    {
                        var random = new Random().Next(0, 8);
                        var ItemAdd = Singleton<ItemManager>.Instance.TryGetTemplate(PresenteSuperpoderoso[random]); //Presente de Nataw potencialmente superpoderoso
                        client.Character.Inventory.AddItem(ItemAdd, 1);
                        client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                    }
                    #endregion

                    #region// ----------------- Itens De Fadas ---------------- //
                    if (basePlayerItem.Template.TypeId == 74)
                    {
                        BasePlayerItem MontaPet = client.Character.Inventory.TryGetItem(CharacterInventoryPositionEnum.ACCESSORY_POSITION_PETS);
                        long TimeFairy = DateTime.Now.AddSeconds(ItemsFairyManager.Instance.GetFairyItemById(basePlayerItem.Template.Id).DelaySeconds).GetUnixTimeStampLong();
                        int CallTime = ItemsFairyManager.Instance.GetFairyItemById(basePlayerItem.Template.Id).CallDelay;
                        int SpellId = ItemsFairyManager.Instance.GetFairyItemById(basePlayerItem.Template.Id).SpellId;
                        int SpellLevel = ItemsFairyManager.Instance.GetFairyItemById(basePlayerItem.Template.Id).SpellLevel;

                        if (TimeFairy == 0)
                            return;

                        if (SpellId == 0)
                            return;

                        if (SpellsFatalId.Contains(SpellId) && client.Character.HasEquippedMount())
                            client.Character.Area.CallDelayed((int)(CallTime - (0.03 * CallTime)), () => client.Character.Dismount());

                        if (MontaPet != null && SpellsFatalId.Contains(SpellId) && MontaPet.Template.TypeId == 121)
                            client.Character.Area.CallDelayed((int)(CallTime - (0.03 * CallTime)), () => client.Character.Inventory.MoveItem(MontaPet, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED));

                        client.Character.Area.CallDelayed(CallTime, () => UseItemFairy(client, client.Character.Cell.Id, SpellId, (short)SpellLevel));
                        client.Character.Map.Clients.Send(new GameRolePlayDelayedObjectUseMessage(client.Character.Id, (sbyte)DelayedActionTypeEnum.DELAYED_ACTION_OBJECT_USE, TimeFairy, (ushort)basePlayerItem.Template.Id));
                        client.Character.Inventory.RemoveItem(basePlayerItem, 1);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no Uso de um Objeto : " + ex.Message);
            }
        }

        private static void UseBox(Character character, int ItemTemplate, int Amount)
        {
            var Item = Singleton<ItemManager>.Instance.TryGetTemplate(ItemTemplate);

            character.Inventory.AddItem(Item, Amount);
        }

        private static void UseItemFairy(WorldClient client, short cellid, int spell, short spelllevel)
        {
            client.Character.CancelEmote();
            client.Character.Look.RemoveAuras();
            client.Character.Map.Clients.Send(new GameRolePlayDelayedActionFinishedMessage(client.Character.Id, (sbyte)DelayedActionTypeEnum.DELAYED_ACTION_OBJECT_USE));
            client.Character.Map.Clients.Send(new GameRolePlaySpellAnimMessage((ulong)client.Character.Id, (ushort)cellid, (ushort)spell, spelllevel, (short)client.Character.Direction));
        }

        private static void UseItem(WorldClient client)
        {
            client.Character.Map.Clients.Send(new GameRolePlayDelayedActionFinishedMessage(client.Character.Id, (sbyte)DelayedActionTypeEnum.DELAYED_ACTION_OBJECT_USE));
            var circle = new Circle(6);
            var cells = circle.GetCells(client.Character.Cell, client.Character.Map);
            var actor = client.Character.Map.GetActors<Character>(x => cells.Contains(x.Cell) && x.AvaState == AggressableStatusEnum.AvA_ENABLED_AGGRESSABLE && client.Character.CanAgressAvA(x) == FighterRefusedReasonEnum.FIGHTER_ACCEPTED).FirstOrDefault();

            if (actor != null)
            {
                actor.StopMove();
                var fight = FightManager.Instance.CreateAgressionFightKoh(actor.Map);
                fight.ChallengersTeam.AddFighter(client.Character.CreateFighter(fight.ChallengersTeam));
                fight.DefendersTeam.AddFighter(actor.CreateFighter(fight.DefendersTeam));
                fight.StartPlacement();
                return;
            }
        }

        [WorldHandler(ObjectUseMultipleMessage.Id)]
        public static void HandleObjectUseMultipleMessage(WorldClient client, ObjectUseMultipleMessage message)
        {
            var item = client.Character.Inventory.TryGetItem((int)message.objectUID);

            if (item == null)
                return;
            client.Character.Inventory.UseItem(item, (int)message.quantity);

        }

        [WorldHandler(ObjectUseOnCellMessage.Id)]
        public static void HandleObjectUseOnCellMessage(WorldClient client, ObjectUseOnCellMessage message)
        {
            try
            {
                var item = client.Character.Inventory.TryGetItem((int)message.objectUID);

                if (item == null)
                    return;

                var cell = client.Character.Map.GetCell(message.cells);

                if (cell == null)
                    return;

                client.Character.Inventory.UseItem(item, cell);

                #region// ----------------- Itens De Fadas ---------------- //
                if (item.Template.Type.Id == 74)
                {
                    BasePlayerItem MontaPet = client.Character.Inventory.TryGetItem(CharacterInventoryPositionEnum.ACCESSORY_POSITION_PETS);
                    long TimeFairy = DateTime.Now.AddSeconds(ItemsFairyManager.Instance.GetFairyItemById(item.Template.Id).DelaySeconds).GetUnixTimeStampLong();
                    int CallTime = ItemsFairyManager.Instance.GetFairyItemById(item.Template.Id).CallDelay;
                    int SpellId = ItemsFairyManager.Instance.GetFairyItemById(item.Template.Id).SpellId;
                    int SpellLevel = ItemsFairyManager.Instance.GetFairyItemById(item.Template.Id).SpellLevel;

                    if (TimeFairy == 0)
                        return;

                    if (SpellId == 0)
                        return;

                    if (SpellsFatalId.Contains(SpellId) && client.Character.HasEquippedMount())
                        client.Character.Area.CallDelayed((int)(CallTime - (0.03 * CallTime)), () => client.Character.Dismount());

                    if (MontaPet != null && SpellsFatalId.Contains(SpellId) && MontaPet.Template.TypeId == 121)
                        client.Character.Area.CallDelayed((int)(CallTime - (0.03 * CallTime)), () => client.Character.Inventory.MoveItem(MontaPet, CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED));

                    client.Character.Area.CallDelayed(CallTime, () => UseItemFairy(client, cell.Id, SpellId, (short)SpellLevel));
                    client.Character.Map.Clients.Send(new GameRolePlayDelayedObjectUseMessage(client.Character.Id, (sbyte)DelayedActionTypeEnum.DELAYED_ACTION_OBJECT_USE, TimeFairy, (ushort)item.Template.Id));
                    client.Character.Inventory.RemoveItem(item, 1);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no Uso de um Objeto : " + ex.Message);
            }
        }

        [WorldHandler(ObjectUseOnCharacterMessage.Id)]
        public static void HandleObjectUseOnCharacterMessage(WorldClient client, ObjectUseOnCharacterMessage message)
        {
            var item = client.Character.Inventory.TryGetItem((int)message.objectUID);

            if (item == null)
                return;

            if (!item.Template.Targetable)
                return;

            var character = client.Character.Map.GetActor<Character>((int)message.characterId);

            if (character == null)
                return;

            client.Character.Inventory.UseItem(item, character);
        }


        [WorldHandler(ObjectFeedMessage.Id)]
        public static void HandleObjectFeedMessage(WorldClient client, ObjectFeedMessage message)
        {
            if (client.Character.IsInFight())
            {
                return;
            }

            var item = client.Character.Inventory.TryGetItem((int)message.objectUID);
            var food = client.Character.Inventory.TryGetItem((int)message.meal.First().objectUID);

            if (item == null || food == null)
            {
                return;
            }

            var quantityToFeed = Math.Min(message.meal.First().quantity, food.Stack);
            var remainingQuantity = quantityToFeed;

            if (item.Stack > 1)
            {
                item.Owner.Inventory.CutItem(item, (int)item.Stack - 1);
            }

            for (int i = 0; i < quantityToFeed; i++)
            {
                if (item.Feed(food))
                {
                    remainingQuantity--;
                }
                else
                {
                    break;
                }
            }

            client.Character.Inventory.RemoveItem(food, (int)(quantityToFeed - remainingQuantity));
            client.Character.Inventory.CheckItemsCriterias();
        }

        [WorldHandler(LivingObjectChangeSkinRequestMessage.Id)]
        public static void HandleLivingObjectChangeSkinRequestMessage(WorldClient client, LivingObjectChangeSkinRequestMessage message)
        {
            if (client.Character.IsInFight())
                return;

            var item = client.Character.Inventory.TryGetItem((int)message.livingUID);

            if (!(item is CommonLivingObject))
                return;

            ((CommonLivingObject)item).SelectedLevel = (short)message.skinId;
        }

        [WorldHandler(LivingObjectDissociateMessage.Id)]
        public static void HandleLivingObjectDissociateMessage(WorldClient client, LivingObjectDissociateMessage message)
        {
            if (client.Character.IsInFight())
                return;

            var item = client.Character.Inventory.TryGetItem((int)message.livingUID);

            (item as BoundLivingObjectItem)?.Dissociate();
        }

        [WorldHandler(ObjectDropMessage.Id)]
        public static void HandleObjectDropMessage(WorldClient client, ObjectDropMessage message)
        {
            if (client.Character.IsInFight() || client.Character.IsInExchange())
                return;

            //Restrição Staff Hydra By:Kenshin
            if (client.Character.UserGroup.Role >= RoleEnum.Moderator_Helper && client.Character.UserGroup.Role <= RoleEnum.Administrator || client.Character.Invisible)
            {
                #region MSG
                switch (client.Character.Account.Lang)
                {
                    case "fr":
                        client.Character.SendServerMessage("Vous n'êtes pas autorisé à déposer des articles. Vérifiez vos droits avec STAFF", System.Drawing.Color.Red);
                        break;
                    case "es":
                        client.Character.SendServerMessage("No se le permite dejar artículos. Verifique sus derechos con STAFF", System.Drawing.Color.Red);
                        break;
                    case "en":
                        client.Character.SendServerMessage("You are not allowed to drop items. Check your rights with STAFF", System.Drawing.Color.Red);
                        break;
                    default:
                        client.Character.SendServerMessage("Você não tem permissão para dropar itens. Consulte seus direitos com a STAFF", System.Drawing.Color.Red);
                        break;
                }
                #endregion

                #region MongoDB Logs Staff
                var document = new BsonDocument
                    {
                        { "AccountId", client.Account.Id },
                        { "AccountName", client.Account.Login },
                        { "CharacterId", client.Character.Id },
                        { "CharacterName", client.Character.Namedefault},
                        { "AbuseReason", "Object Drop Message"},
                        { "IPAddress", client.IP },
                        { "ClientKey", client.Character.Account.LastHardwareId },
                        { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                    };

                MongoLogger.Instance.Insert("Staff_AbuseRights", document);
                #endregion
                return;
            }
            else
            {
                client.Character.DropItem((int)message.objectUID, (int)message.quantity);
            }
        }

        [WorldHandler(MimicryObjectFeedAndAssociateRequestMessage.Id)]
        public static void HandleMimicryObjectFeedAndAssociateRequestMessage(WorldClient client, MimicryObjectFeedAndAssociateRequestMessage message)
        {
            if (client.Character.IsInFight())
                return;

            var character = client.Character;

            var host = character.Inventory.TryGetItem((int)message.hostUID);
            var food = character.Inventory.TryGetItem((int)message.foodUID);
            var mimisymbic = character.Inventory.TryGetItem(ItemIdEnum.MIMIBIOTE_14485);

            if (host == null || food == null)
            {
                SendMimicryObjectErrorMessage(client, host == null ? MimicryErrorEnum.NO_VALID_HOST : MimicryErrorEnum.NO_VALID_FOOD);
                return;
            }

            if (mimisymbic == null)
            {
                SendMimicryObjectErrorMessage(client, MimicryErrorEnum.NO_VALID_MIMICRY);
                return;
            }

            if (host.Effects.Any(x => x.EffectId == EffectsEnum.Effect_LivingObjectId || x.EffectId == EffectsEnum.Effect_Appearance || x.EffectId == EffectsEnum.Effect_Apparence_Wrapper)
                || !host.Template.Type.Mimickable)
            {
                SendMimicryObjectErrorMessage(client, MimicryErrorEnum.NO_VALID_HOST);
                return;
            }

            if (food.Effects.Any(x => x.EffectId == EffectsEnum.Effect_LivingObjectId || x.EffectId == EffectsEnum.Effect_Appearance || x.EffectId == EffectsEnum.Effect_Apparence_Wrapper)
                || !food.Template.Type.Mimickable)
            {
                SendMimicryObjectErrorMessage(client, MimicryErrorEnum.NO_VALID_FOOD);
                return;
            }

            if (food.Template.Id == host.Template.Id)
            {
                SendMimicryObjectErrorMessage(client, MimicryErrorEnum.SAME_SKIN);
                return;
            }

            if (food.Template.Level > host.Template.Level)
            {
                SendMimicryObjectErrorMessage(client, MimicryErrorEnum.FOOD_LEVEL);
                return;
            }

            if (food.Template.TypeId != host.Template.TypeId)
            {
                SendMimicryObjectErrorMessage(client, MimicryErrorEnum.FOOD_TYPE);
                return;
            }

            var modifiedItem = ItemManager.Instance.CreatePlayerItem(character, host);
            DateTime Date = DateTime.Now.AddDays(7);
            int Month = Date.Month - 1;

            modifiedItem.Effects.Add(new EffectInteger(EffectsEnum.Effect_Appearance, (short)food.Template.Id));
            modifiedItem.Stack = 1;

            if (message.preview)
            {
                SendMimicryObjectPreviewMessage(client, modifiedItem);
            }
            else
            {
                character.Inventory.UnStackItem(food, 1);

                if (mimisymbic.Effects.Exists(y => y.EffectId == EffectsEnum.Effect_BlockItemNpcShop))
                {
                    modifiedItem.Effects.Add(new EffectDate((short)EffectsEnum.Effect_Exchangeable, (short)Date.Year, (short)Month, (short)Date.Day, (short)Date.Hour, (short)Date.Minute, new EffectBase()));
                    CreateMandatory(character, 6);
                    character.Inventory.UnStackItem(mimisymbic, 1);
                }
                else
                {
                    character.Inventory.UnStackItem(mimisymbic, 1);
                }

                character.Inventory.UnStackItem(host, 1);
                character.Inventory.AddItem(modifiedItem);

                SendMimicryObjectAssociatedMessage(client, modifiedItem);
            }
        }

        private static void CreateMandatory(Character character, int MandatoryId)
        {
            try
            {
                var DeleteMandatory = new MandatoryRecord();
                var CompareTime = DateTime.Now;
                int Time = character.Client.UserGroup.Role >= RoleEnum.Gold_Vip ? 3 : 24;

                foreach (var Mandatory in character.MandatoryCollection.Mandatory.Where(Mandatory => Mandatory.MandatoryId == Mandatory.MandatoryId && Mandatory.OwnerId == character.Id))
                {
                    DeleteMandatory = Mandatory;
                    CompareTime = Mandatory.Time;
                    break;
                }

                character.MandatoryCollection.Mandatory.Add(new MandatoryRecord
                {
                    OwnerId = character.Id,
                    MandatoryId = MandatoryId,
                    Time = DateTime.Now.AddHours(Time),
                    IsNew = true,
                    Ip = character.Client.IP
                });

                character.SaveLater();
            }
            catch { }
        }

        [WorldHandler(MimicryObjectEraseRequestMessage.Id)]
        public static void HandleMimicryObjectEraseRequestMessage(WorldClient client, MimicryObjectEraseRequestMessage message)
        {
            if (client.Character.IsInFight())
                return;

            var host = client.Character.Inventory.TryGetItem((int)message.hostUID);

            if (host == null)
                return;

            host.Effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_Appearance);

            if (host.Effects.Exists(x => x.EffectId == EffectsEnum.Effect_Exchangeable))
                host.Effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_Exchangeable);

            host.Invalidate();

            client.Character.Inventory.RefreshItem(host);
            client.Character.UpdateLook();

            SendInventoryWeightMessage(client);
        }

        [WorldHandler(WrapperObjectDissociateRequestMessage.Id)]
        public static void HandleWrapperObjectDissociateRequestMessage(WorldClient client, WrapperObjectDissociateRequestMessage message)
        {
            if (client.Character.IsInFight() || client.Character.IsInExchange())
                return;

            var host = client.Character.Inventory.TryGetItem((int)message.hostUID);

            if (host == null)
                return;

            var apparenceWrapper = host.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_Apparence_Wrapper) as EffectInteger;

            if (apparenceWrapper == null)
                return;

            var wrapperItemTemplate = ItemManager.Instance.TryGetTemplate(apparenceWrapper.Value);

            host.Effects.RemoveAll(x => x.EffectId == EffectsEnum.Effect_Apparence_Wrapper);

            if (wrapperItemTemplate.Effects.Any(x => x.EffectId == EffectsEnum.Effect_148))
            {
                int followerValue = (wrapperItemTemplate.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_148) as EffectDice).Value;

                if (followerValue > 0)
                {
                    CeremonialRingsHandlers.RemoveItemFollow(client.Character, followerValue, true);
                }
            }

            host.Invalidate();
            client.Character.Inventory.RefreshItem(host);
            host.OnObjectModified();

            var wrapperItem = ItemManager.Instance.CreatePlayerItem(client.Character, wrapperItemTemplate, 1);

            client.Character.Inventory.AddItem(wrapperItem);
            client.Character.UpdateLook();

            SendInventoryWeightMessage(client);
        }

        public static void SendWrapperObjectAssociatedMessage(IPacketReceiver client, BasePlayerItem host)
        {
            client.Send(new WrapperObjectAssociatedMessage((uint)host.Guid));
        }

        public static void SendMimicryObjectAssociatedMessage(IPacketReceiver client, BasePlayerItem host)
        {
            client.Send(new MimicryObjectAssociatedMessage((uint)host.Guid));
        }

        public static void SendMimicryObjectPreviewMessage(IPacketReceiver client, BasePlayerItem host)
        {
            client.Send(new MimicryObjectPreviewMessage(host.GetObjectItem()));
        }

        public static void SendMimicryObjectErrorMessage(IPacketReceiver client, MimicryErrorEnum error)
        {
            client.Send(new MimicryObjectErrorMessage((sbyte)ObjectErrorEnum.SYMBIOTIC_OBJECT_ERROR, (sbyte)error, true));
        }

        public static void SendGameRolePlayPlayerLifeStatusMessage(IPacketReceiver client)
        {
            client.Send(new GameRolePlayPlayerLifeStatusMessage());
        }

        public static void SendGameRolePlayPlayerLifeStatusMessage(IPacketReceiver client, sbyte state, int phxMap)
        {
            client.Send(new GameRolePlayPlayerLifeStatusMessage(state, phxMap));
        }

        public static void SendInventoryContentMessage(WorldClient client)
        {
            var itemPrices = new Dictionary<uint, ulong>();

            foreach (var item in client.Character.Inventory)
            {
                if (!itemPrices.ContainsKey(item.GetObjectItem().objectGID))
                {
                    itemPrices.Add(item.GetObjectItem().objectGID, PriceFormulas.getItemPrice(item.Template.Id));
                }
            }

            ObjectItem[] ownerItems = client.Character.Inventory.Select(entry => entry.GetObjectItem()).ToArray();

            client.Send(new ObjectAveragePricesMessage(itemPrices.Keys.ToArray(), itemPrices.Values.ToArray()));
            client.Send(new InventoryContentMessage(ownerItems, (ulong)client.Character.Inventory.Kamas));
        }

        public static void SendInventoryContentAndPresetMessage(WorldClient client)
        {
            var itemPrices = new Dictionary<uint, ulong>();

            foreach (var item in client.Character.Inventory)
            {
                if (!itemPrices.ContainsKey(item.GetObjectItem().objectGID))
                    itemPrices.Add(item.GetObjectItem().objectGID, PriceFormulas.getItemPrice(item.Template.Id));
            }

            client.Send(new ObjectAveragePricesMessage(itemPrices.Keys.ToArray(), itemPrices.Values.ToArray()));
            client.Send(new InventoryContentMessage(client.Character.Inventory.Select(entry => entry.GetObjectItem()).ToArray(), (ulong)client.Character.Inventory.Kamas));
        }

        public static void SendInventoryWeightMessage(WorldClient client)
        {
            client.Send(new InventoryWeightMessage((uint)client.Character.Inventory.Weight, (uint)client.Character.Inventory.WeightTotal));
        }

        public static void SendExchangeKamaModifiedMessage(IPacketReceiver client, bool remote, long kamasAmount)
        {
            client.Send(new ExchangeKamaModifiedMessage(remote, (ulong)kamasAmount));
        }

        public static void SendObjectAddedMessage(IPacketReceiver client, IItem addedItem)
        {
            client.Send(new ObjectAddedMessage(addedItem.GetObjectItem(), 0));
        }

        public static void SendObjectsAddedMessage(IPacketReceiver client, IEnumerable<ObjectItem> addeditems)
        {
            client.Send(new ObjectsAddedMessage(addeditems));
        }

        public static void SendObjectsQuantityMessage(IPacketReceiver client, IEnumerable<ObjectItemQuantity> itemQuantity)
        {
            client.Send(new ObjectsQuantityMessage(itemQuantity.ToArray()));
        }

        public static void SendObjectDeletedMessage(IPacketReceiver client, int guid)
        {
            client.Send(new ObjectDeletedMessage((uint)guid));
        }

        public static void SendObjectsDeletedMessage(IPacketReceiver client, IEnumerable<int> guids)
        {
            client.Send(new ObjectsDeletedMessage(guids.Select(entry => (uint)entry).ToList()));
        }

        public static void SendObjectModifiedMessage(IPacketReceiver client, IItem item)
        {
            client.Send(new ObjectModifiedMessage(item.GetObjectItem()));
        }

        public static void SendObjectMovementMessage(IPacketReceiver client, BasePlayerItem movedItem)
        {
            client.Send(new ObjectMovementMessage((uint)movedItem.Guid, (sbyte)movedItem.Position));
        }

        public static void SendObjectQuantityMessage(IPacketReceiver client, BasePlayerItem item)
        {
            client.Send(new ObjectQuantityMessage((uint)item.Guid, (uint)item.Stack, 0));
        }

        public static void SendObjectErrorMessage(IPacketReceiver client, ObjectErrorEnum error)
        {
            client.Send(new ObjectErrorMessage((sbyte)error));
        }

        public static void SendSetUpdateMessage(WorldClient client, ItemSetTemplate itemSet)
        {
            client.Send(new SetUpdateMessage((ushort)itemSet.Id,
                client.Character.Inventory.GetItemSetEquipped(itemSet).Select(entry => (uint)entry.Template.Id),
                client.Character.Inventory.GetItemSetEffects(itemSet).Select(entry => entry.GetObjectEffect())));
        }

        public static void SendExchangeShopStockMovementUpdatedMessage(IPacketReceiver client, MerchantItem item)
        {
            //client.Send(new ExchangeShopStockMovementUpdatedMessage(item.GetObjectItemToSell()));
        }

        public static void SendExchangeShopStockMovementRemovedMessage(IPacketReceiver client, MerchantItem item)
        {
            //client.Send(new ExchangeShopStockMovementRemovedMessage((uint)item.Guid));
        }

        public static void SendObtainedItemMessage(IPacketReceiver client, ItemTemplate item, int count)
        {
            client.Send(new ObtainedItemMessage((ushort)item.Id, (uint)count));
        }

        public static void SendObtainedItemWithBonusMessage(IPacketReceiver client, ItemTemplate item, int count, int bonus)
        {
            client.Send(new ObtainedItemWithBonusMessage((ushort)item.Id, (uint)count, (uint)bonus));
        }

        public static void SendExchangeObjectPutInBagMessage(IPacketReceiver client, bool remote, IItem item)
        {
            client.Send(new ExchangeObjectPutInBagMessage(remote, item.GetObjectItem()));
        }

        public static void SendExchangeObjectModifiedInBagMessage(IPacketReceiver client, bool remote, IItem item)
        {
            client.Send(new ExchangeObjectModifiedInBagMessage(remote, item.GetObjectItem()));
        }

        public static void SendExchangeObjectRemovedFromBagMessage(IPacketReceiver client, bool remote, int guid)
        {
            client.Send(new ExchangeObjectRemovedFromBagMessage(remote, (uint)guid));
        }
        public static void tpPlayer(Character player, uint mapId, short cellId, DirectionsEnum playerDirection)
        {
            player.Teleport(new ObjectPosition(Singleton<World>.Instance.GetMap(mapId), cellId, playerDirection));
        }
    }
}