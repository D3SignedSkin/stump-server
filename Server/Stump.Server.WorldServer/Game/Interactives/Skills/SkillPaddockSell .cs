using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs.Paddock;
using Stump.Server.WorldServer.Game.Maps.Paddocks;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    [Discriminator("PaddockSell", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class PaddockSell : CustomSkill
    {
   
        public PaddockSell(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
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
            if (paddock.Guild == null)
                return false;
            if (character.Guild != paddock.Guild)
                return false;
            if (character.Guild.Boss.Id != character.Id)
                return false;
            // if (paddock.Price != 0)//just modyif
            //return false;
             if (paddock.OnSale ==true)
            return false;
            if (!paddock.Locked && paddock.OnSale)//already selling , no?!
                return false;

            //guild permission !??!




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
            if(paddock.Guild!=null)
                if(character.Guild != paddock.Guild)
                    return -1;
            if(paddock.Abandonned==true)
                return -1;

          
            var dialog = new PaddockBuySell(character, paddock, true, (ulong)paddock.Price, m_interactiveObject);
            dialog.Open();

            return base.StartExecute(character);
        }
    }
}
