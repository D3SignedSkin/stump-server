
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;


namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    public class GradeModificator : TargetCommand
    {
        public GradeModificator()
        {
            base.Aliases = new string[]{ "grade" };
            base.RequiredRole = RoleEnum.Developer;
            base.AddTargetParameter(false, "Defined target");
            base.AddParameter<int>("NewUserGroupId", null, null, 1, true);
            Description = "Modifier le grade du joueur ciblé.";
        }
        public override void Execute(TriggerBase trigger)
        {
            Character[] targets = base.GetTargets(trigger);

            for (int i = 0; i < targets.Length; i++)
            {
                Character character = targets[i];
                trigger.Reply("", new object[]
                {
                    character.Name,
                });

                int grade = (int)trigger.Get<int>("NewUserGroupId");

                GameTrigger gameTrigger = trigger as GameTrigger;

                if (gameTrigger.Character.Account.UserGroupId < (int)trigger.Get<int>("NewUserGroupId"))
                {
                    gameTrigger.Character.OpenPopup("Vous ne pouvez pas attribuer un grade supérieur au votre !");
                }
                else
                {
                    character.Account.UserGroupId = grade;
                    character.SaveLater();
                    character.OpenPopup("Votre grade à étè modifié en grade : " + character.Account.UserGroupId + "\n Merci de déconnecter / reconnecter pour mettre à jour votre grade.");
                    gameTrigger.Character.SendServerMessage("Grade du joueur " + character.Namedefault + " modifié en grade " + (int)trigger.Get<int>("NewUserGroupId") + " !");
                }
            }
        }
    }
}