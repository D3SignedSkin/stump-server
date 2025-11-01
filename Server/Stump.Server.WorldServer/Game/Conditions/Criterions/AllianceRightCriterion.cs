using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class AllianceRightsCriterion : Criterion
    {
        public override bool Eval(Character character)
        {
            return true;
        }

        public override void Build()
        { }
    }
}
