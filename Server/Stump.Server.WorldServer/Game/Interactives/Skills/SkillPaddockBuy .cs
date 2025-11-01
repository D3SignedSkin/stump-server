using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs.Paddock;
using Stump.Server.WorldServer.Game.Maps.Paddocks;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    [Discriminator("PaddockBuy", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillPaddockBuy : CustomSkill
    {
        
        public SkillPaddockBuy(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
            m_interactiveObject = interactiveObject;
        }
        private InteractiveObject m_interactiveObject
        {
            get;
            set;
        }
        public override bool AreConditionsFilled(Character character)
        {
            var paddock = PaddockManager.Instance.GetPaddockByMap(InteractiveObject.Map.Id);
            if (paddock == null)
                return false;
            if (paddock.IsPublicPaddock())
                return false;
            //if (character.Guild == null)
            //   return false;
            if (paddock.Guild != null)
            {
                if (character?.Guild == paddock.Guild)
                    return false;
                if (character?.Guild?.Boss?.Id != character.Id)
                    return false;
            }
            //if (paddock.Price == 0)
            //    return false;
            if (paddock.OnSale == false)
                   return false;
                if (paddock.Locked)
                return false;




            return base.AreConditionsFilled(character);
        }
        public override int StartExecute(Character character)
        {
            if (character.IsBusy())
                return -1;

            if (!Record.AreConditionsFilled(character))
            {
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 1);
                return -1;
            }

            var paddock = PaddockManager.Instance.GetPaddockByMap(InteractiveObject.Map.Id);
            if (paddock == null)
                return -1;
            //character.Client.Send(new PaddockSellBuyDialogMessage(false, (uint)character.Id,1));

            var dialog = new PaddockBuySell(character, paddock,false, (ulong)paddock.Price, m_interactiveObject);
            dialog.Open();

            return base.StartExecute(character);
        }
    }
}
