using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Exchanges.Paddock;
using Stump.Server.WorldServer.Game.Maps.Paddocks;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    [Discriminator("Paddock", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillPaddock : CustomSkill
    {
        private Paddock m_paddock;

        public SkillPaddock(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject) : base(id, record, interactiveObject)
        { }

        private Paddock Paddock
        {
            get { return PaddockManager.Instance.GetPaddockByMap(InteractiveObject.Map.Id); }
            set { m_paddock = value; }
        }

        public override bool IsEnabled(Character character)
        {
            if (Paddock == null)
                return false;

            if (Paddock.OnSale)
                return false;

            return base.IsEnabled(character);
        }

        public override bool CanUse(Character character)
        {
            if (Paddock == null)
                return false;

            if (!Paddock.IsPaddockOwner(character))
                return false;

            if (Paddock.Abandonned == true)
                return false;

            if (Paddock.OnSale)
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

            if (Paddock == null)
                return -1;

            var exchange = new PaddockExchange(character, Paddock, InteractiveObject);
            exchange.Open();

            return base.StartExecute(character);
        }
    }
}
