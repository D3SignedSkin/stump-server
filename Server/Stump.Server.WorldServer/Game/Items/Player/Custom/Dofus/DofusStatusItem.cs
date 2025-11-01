using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    [ItemId(ItemIdEnum.DOFUS_OCRE_7754)]
    [ItemId(ItemIdEnum.DOFUS_EBENE_7114)]
    [ItemId(ItemIdEnum.DOFUS_IVOIRE_7115)]
    [ItemId(ItemIdEnum.DOFUS_POURPRE_694)]
    [ItemId(ItemIdEnum.DOFUS_EMERAUDE_737)]
    [ItemId(ItemIdEnum.DOFUS_TURQUOISE_739)]
    public class DofusStatusItem : BasePlayerItem
    {
        public DofusStatusItem(Character owner, PlayerItemRecord record) : base(owner, record)
        { }

        public override bool OnEquipItem(bool unequip)
        {
            if (unequip)
            {
                if (this.Template.Id == (int)ItemIdEnum.DOFUS_OCRE_7754)
                {
                    //if (Owner.HasEmote((EmotesEnum)185))
                    //    Owner.RemoveEmote((EmotesEnum)185);

                    if (Owner.HasOrnament(253))
                        Owner.RemoveOrnament(253);
                }
                else if (this.Template.Id == (int)ItemIdEnum.DOFUS_EBENE_7114)
                {
                    //if (Owner.HasEmote((EmotesEnum)182))
                    //    Owner.RemoveEmote((EmotesEnum)182);

                    if (Owner.HasOrnament(251))
                        Owner.RemoveOrnament(251);
                }
                else if (this.Template.Id == (int)ItemIdEnum.DOFUS_IVOIRE_7115)
                {
                    //if (Owner.HasEmote((EmotesEnum)181))
                    //    Owner.RemoveEmote((EmotesEnum)181);

                    if (Owner.HasOrnament(250))
                        Owner.RemoveOrnament(250);
                }
                else if (this.Template.Id == (int)ItemIdEnum.DOFUS_POURPRE_694)
                {
                    //if (Owner.HasEmote((EmotesEnum)186))
                    //    Owner.RemoveEmote((EmotesEnum)186);

                    if (Owner.HasOrnament(252))
                        Owner.RemoveOrnament(252);
                }
                else if (this.Template.Id == (int)ItemIdEnum.DOFUS_EMERAUDE_737)
                {
                    //if (Owner.HasEmote((EmotesEnum)183))
                    //    Owner.RemoveEmote((EmotesEnum)183

                    if (Owner.HasOrnament(245))
                        Owner.RemoveOrnament(245);
                }
                else if (this.Template.Id == (int)ItemIdEnum.DOFUS_TURQUOISE_739)
                {
                    //if (Owner.HasEmote((EmotesEnum)184))
                    //    Owner.RemoveEmote((EmotesEnum)184);

                    if (Owner.HasOrnament(249))
                        Owner.RemoveOrnament(249);
                }
            }
            else
            {
                if (this.Template.Id == (int)ItemIdEnum.DOFUS_OCRE_7754)
                {
                    //if (!Owner.HasEmote((EmotesEnum)185))
                    //    Owner.AddEmote((EmotesEnum)185);

                    if (!Owner.HasOrnament(253))
                        Owner.AddOrnament(253);
                }
                else if (this.Template.Id == (int)ItemIdEnum.DOFUS_EBENE_7114)
                {
                    //if (!Owner.HasEmote((EmotesEnum)182))
                    //    Owner.AddEmote((EmotesEnum)182);

                    if (!Owner.HasOrnament(251))
                        Owner.AddOrnament(251);
                }
                else if (this.Template.Id == (int)ItemIdEnum.DOFUS_IVOIRE_7115)
                {
                    //if (!Owner.HasEmote((EmotesEnum)181))
                    //    Owner.AddEmote((EmotesEnum)181);

                    if (!Owner.HasOrnament(250))
                        Owner.AddOrnament(250);
                }
                else if (this.Template.Id == (int)ItemIdEnum.DOFUS_POURPRE_694)
                {
                    //if (!Owner.HasEmote((EmotesEnum)186))
                    //    Owner.AddEmote((EmotesEnum)186);

                    if (!Owner.HasOrnament(252))
                        Owner.AddOrnament(252);
                }
                else if (this.Template.Id == (int)ItemIdEnum.DOFUS_EMERAUDE_737)
                {
                    //if (!Owner.HasEmote((EmotesEnum)183))
                    //    Owner.AddEmote((EmotesEnum)183

                    if (!Owner.HasOrnament(245))
                        Owner.AddOrnament(245);
                }
                else if (this.Template.Id == (int)ItemIdEnum.DOFUS_TURQUOISE_739)
                {
                    //if (!Owner.HasEmote((EmotesEnum)184))
                    //    Owner.AddEmote((EmotesEnum)184);

                    if (!Owner.HasOrnament(249))
                        Owner.AddOrnament(249);
                }
            }

            return base.OnEquipItem(unequip);
        }
    }
}