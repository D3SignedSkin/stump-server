using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Drawing;

namespace Commands.Commands
{
    public class ParchoCommands : TargetCommand
    {
        public ParchoCommands()
        {
            base.Aliases = new string[]
            {
                "Parcho"
            };
            RequiredRole = RoleEnum.Developer;
            Description = "Permet de vous parcho.";
        }
        public override void Execute(TriggerBase trigger)
        {
            Character[] targets = GetTargets(trigger);
            for (int i = 0; i < targets.Length; i++)
            {
                Character character = targets[i];
                character.Stats.Vitality.Additional = 101;
                character.Stats.Chance.Additional = 101;
                character.Stats.Intelligence.Additional = 101;
                character.Stats.Wisdom.Additional = 101;
                character.Stats.Agility.Additional = 101;
                character.Stats.Strength.Additional = 101;
                character.SpellsPoints = 999;
                character.RefreshStats();
                character.SendServerMessage("Votre personnage" + "<b> " + character.Name + " </b>" + "à obtenu <b>101 points de caractéristiques additionnels</b>.");
            }
        }
    }
}