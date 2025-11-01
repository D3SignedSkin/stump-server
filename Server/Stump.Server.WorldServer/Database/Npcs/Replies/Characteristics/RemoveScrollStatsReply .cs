using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Items;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("RemoveScrollStats", typeof(NpcReply), new System.Type[]
    {
        typeof(NpcReplyRecord)
    })]
    public class RemoveScrollStatsReply : NpcReply
    {

        public RemoveScrollStatsReply(NpcReplyRecord record) : base(record)
        {
        }

        public int KamasParameter
        {
            get
            {
                return Record.GetParameter<int>(0, true);
            }
            set
            {
                Record.SetParameter(0, value);
            }
        }

        public int OgrinesParameter
        {
            get
            {
                return Record.GetParameter<int>(1, true);
            }
            set
            {
                Record.SetParameter(1, value);
            }
        }

        public override bool Execute(Npc npc, Character character)
        {
            if (!base.Execute(npc, character))
            {
                return false;
            }
            else
            {
                bool KamasActive = false;
                bool OgrinesActive = false;

                var ogrines = character.Inventory.TryGetItem(Settings.TokenTemplateId);
                int KamasAmount = 0;
                int OgrinesAmount = 0;

                if (KamasParameter != 0)
                {
                    if (character.Kamas < KamasParameter)
                    {
                        //Não possui kamas o suficiente para da sequencia
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 82);
                        return false;
                    }

                    KamasActive = true;
                }

                if (OgrinesParameter != 0)
                {
                    if (ogrines == null)
                    {
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 4);
                        return false;
                    }
                    else if (ogrines.Stack < OgrinesParameter)
                    {
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 252);
                        return false;
                    }

                    OgrinesActive = true;
                }

                if (character.Stats.Vitality.Additional != 0)
                {
                    var CharStats = character.Stats.Vitality.Additional;

                    character.Stats.Vitality.Additional = 0;
                    AddScrollPerga(character, CharStats, 808, 810);

                    if (KamasActive == true)
                        KamasAmount += KamasParameter;

                    if (OgrinesActive == true)
                        OgrinesAmount += OgrinesParameter;
                }

                if (character.Stats.Agility.Additional != 0)
                {
                    var CharStats = character.Stats.Agility.Additional;

                    character.Stats.Agility.Additional = 0;
                    AddScrollPerga(character, CharStats, 800, 801);

                    if (KamasActive == true)
                        KamasAmount += KamasParameter;

                    if (OgrinesActive == true)
                        OgrinesAmount += OgrinesParameter;
                }

                if (character.Stats.Chance.Additional != 0)
                {
                    var CharStats = character.Stats.Chance.Additional;

                    character.Stats.Chance.Additional = 0;
                    AddScrollPerga(character, CharStats, 812, 814);

                    if (KamasActive == true)
                        KamasAmount += KamasParameter;

                    if (OgrinesActive == true)
                        OgrinesAmount += OgrinesParameter;
                }

                if (character.Stats.Strength.Additional != 0)
                {
                    var CharStats = character.Stats.Strength.Additional;

                    character.Stats.Strength.Additional = 0;
                    AddScrollPerga(character, CharStats, 796, 797);

                    if (KamasActive == true)
                        KamasAmount += KamasParameter;

                    if (OgrinesActive == true)
                        OgrinesAmount += OgrinesParameter;
                }

                if (character.Stats.Intelligence.Additional != 0)
                {
                    var CharStats = character.Stats.Intelligence.Additional;

                    character.Stats.Intelligence.Additional = 0;
                    AddScrollPerga(character, CharStats, 816, 817);

                    if (KamasActive == true)
                        KamasAmount += KamasParameter;

                    if (OgrinesActive == true)
                        OgrinesAmount += OgrinesParameter;
                }

                if (character.Stats.Wisdom.Additional != 0)
                {
                    var CharStats = character.Stats.Wisdom.Additional;

                    character.Stats.Wisdom.Additional = 0;
                    AddScrollPerga(character, CharStats, 804, 805);

                    if (KamasActive == true)
                        KamasAmount += KamasParameter;

                    if (OgrinesActive == true)
                        OgrinesAmount += OgrinesParameter;
                }

                if (KamasParameter != 0 && KamasActive == true && KamasAmount != 0)
                {
                    if (character.Kamas < KamasAmount)
                    {
                        //Não possui kamas o suficiente para da sequencia
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 82);
                        return false;
                    }

                    character.Inventory.SubKamas(KamasAmount);
                    character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 46, KamasAmount);
                }

                if (OgrinesParameter != 0 && OgrinesActive == true && OgrinesAmount != 0)
                {
                    if (ogrines.Stack < OgrinesAmount)
                    {
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 252);
                        return false;
                    }

                    if (character.Inventory.RemoveTokenItem(OgrinesAmount, "Remove Scrolls Reply"))
                    {
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 22, OgrinesAmount, Settings.TokenTemplateId);

                        character.RefreshStats();
                        character.SaveLater();
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        public void AddScrollPerga(Character character, int QuantElement, int Scroll1, int Scroll2)
        {
            #region Formula by Kenshin

            if (QuantElement >= 1 && QuantElement <= 80)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, QuantElement);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, QuantElement, Scroll1);
            }
            else if (QuantElement == 82 || QuantElement == 84 || QuantElement == 86 || QuantElement == 88 || QuantElement == 90 || QuantElement == 92 || QuantElement == 94 || QuantElement == 96 || QuantElement == 98 || QuantElement == 100)
            {
                int Calc = QuantElement / 2;
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, Calc);

                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, Calc, Scroll2);
            }
            else if (QuantElement == 81)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, 1);
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, 40);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 1, Scroll1);
                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 80, Scroll2);
            }
            else if (QuantElement == 83)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, 3);
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, 40);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 3, Scroll1);
                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 40, Scroll2);
            }
            else if (QuantElement == 85)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, 5);
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, 40);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 5, Scroll1);
                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 40, Scroll2);
            }
            else if (QuantElement == 87)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, 7);
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, 40);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 7, Scroll1);
                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 40, Scroll2);
            }
            else if (QuantElement == 89)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, 9);
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, 40);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 9, Scroll1);
                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 40, Scroll2);
            }
            else if (QuantElement == 91)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, 11);
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, 40);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 10, Scroll1);
                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 40, Scroll2);
            }
            else if (QuantElement == 93)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, 13);
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, 40);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 13, Scroll1);
                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 40, Scroll2);
            }
            else if (QuantElement == 95)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, 15);
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, 40);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 15, Scroll1);
                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 40, Scroll2);
            }
            else if (QuantElement == 97)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, 17);
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, 40);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 17, Scroll1);
                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 40, Scroll2);
            }
            else if (QuantElement == 99)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, 19);
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, 40);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 19, Scroll1);
                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 40, Scroll2);
            }
            else if (QuantElement == 101)
            {
                var ItemScroll1 = ItemManager.Instance.CreatePlayerItem(character, Scroll1, 21);
                var ItemScroll2 = ItemManager.Instance.CreatePlayerItem(character, Scroll2, 40);

                character.Inventory.AddItem(ItemScroll1);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 21, 21, Scroll1);
                character.Inventory.AddItem(ItemScroll2);
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, (short)21, 40, Scroll2);
            }
            else
            {
                switch (character.Account.Lang)
                {
                    case "fr":
                        character.OpenPopup("Je n'ai pas pu identifier sa véritable puissance.");
                        break;
                    case "es":
                        character.OpenPopup("No pude identificar su poder real.");
                        break;
                    case "en":
                        character.OpenPopup("I couldn't identify its real power.");
                        break;
                    default:
                        character.OpenPopup("Não consegui identificar o seu poder real.");
                        break;
                }
            }
            #endregion
        }
    }
}
