using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient.Memcached;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.ORM.SubSonic.Extensions;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Breach
{
    public class BreachBranchesDialog
    {
        private readonly Character character;

        public BreachBranchesDialog(Character character)
        {
            this.character = character;
        }

        public void Open()
        {
            openBranches();
        }

        public void openBranches()
        {
            if (character.BreachBranches == null)
                character.BreachBranches = BreachBranche.generateBreachBranches(character);

            ExtendedBreachBranch BreachBranches = new ExtendedBreachBranch();
            var SourceBranches = character.BreachBranches[0];

            BreachBranches.bosses = SourceBranches.bosses;
            BreachBranches.element = SourceBranches.element;
            BreachBranches.map = SourceBranches.map;
            BreachBranches.modifier = SourceBranches.modifier;
            BreachBranches.prize = SourceBranches.prize;
            BreachBranches.rewards = SourceBranches.rewards;
            BreachBranches.monsters = SourceBranches.monsters;
            BreachBranches.room = SourceBranches.room;

            Task.Delay(200).ContinueWith(t =>
            {
                character.Client.Send(new BreachBranchesMessage(new List<ExtendedBreachBranch> { BreachBranches }));

                if (character.BreachBuyables != null && character.BreachBuyables.Length >= 1)
                {
                    List<BreachReward> breachReward = new List<BreachReward>();

                    for (int i = 0; i < character.BreachBuyables.Count(); i++)
                    {
                        breachReward.Add(character.BreachBuyables[i]);
                    }

                    character.Client.Send(new BreachRewardsMessage(breachReward));
                }
            });
        }
    }
}