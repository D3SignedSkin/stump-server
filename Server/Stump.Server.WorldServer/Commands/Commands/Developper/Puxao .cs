using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Stump.Server.WorldServer.Commands.Commands.GameMaster
{
    public class Puxao : CommandBase

    {
        private static Regex m_numberRegex = new Regex("^[0-9]+$", RegexOptions.Compiled);
        private static Regex m_numberRangeRegex = new Regex("^([0-9]+)-([0-9]+)$", RegexOptions.Compiled);

        public Puxao()
        {
            base.Aliases = new string[] { "puxao" };
            base.RequiredRole = RoleEnum.Developer;
            base.Description = "Teletransporta todos os player para você!!!";
            base.AddParameter<string>("parameters", "params", "Level (exact or range x-y), Breed, Area or Name (partial)", null, true, null);
        }

        public override void Execute(TriggerBase trigger)
        {
            PlayableBreedEnum playableBreedEnum;
            Predicate<Character> breedId = (Character x) => true;
            if (trigger.IsArgumentDefined("params"))
            {
                string str = trigger.Get<string>("params");
                if (!Puxao.m_numberRegex.IsMatch(str))
                {
                    Match match = Puxao.m_numberRangeRegex.Match(str);
                    if (match.Success)
                    {
                        int num = int.Parse(match.Groups[1].Value);
                        int num1 = int.Parse(match.Groups[2].Value);
                        breedId = (Character x) => (x.Level < num ? false : x.Level <= num1);
                    }
                    else if (!Enum.TryParse<PlayableBreedEnum>(str, true, out playableBreedEnum))
                    {
                        Area area = Singleton<World>.Instance.GetArea(str);
                        breedId = (area == null ? new Predicate<Character>((Character x) => x.Name.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) != -1) : new Predicate<Character>((Character x) => x.Area == area));
                    }
                    else
                    {
                        breedId = (Character x) => x.BreedId == playableBreedEnum;
                    }
                }
                else
                {
                    int num2 = int.Parse(str);
                    breedId = (Character x) => x.Level == num2;
                }
            }
            IEnumerable<Character> characters = Singleton<World>.Instance.GetCharacters(breedId);
            int num3 = 0;
            Character character = ((GameTrigger)trigger).Character;
            foreach (Character character1 in characters)
            {
                character1.Area.ExecuteInContext(() => character1.Teleport(character.Position, true));
            }
            if (num3 == 0)
            {
                trigger.ReplyError("No results found");
            }
        }
    }
}