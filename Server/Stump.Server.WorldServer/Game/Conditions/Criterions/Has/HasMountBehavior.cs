using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class HasMountBehavior : Criterion
    {
        public const string Identifier = "HMEB";

        public int MountBehavior
        {
            get;
            set;
        }

        public override bool Eval(Character character) => character.HasEquippedMount() && character.EquippedMount.Behaviors.Contains((int)(MountBehaviorEnum)MountBehavior);

        public override void Build()
        {
            int mountbehavior;
            if (!int.TryParse(Literal, out mountbehavior))
                throw new Exception(string.Format("Cannot build HasMountEquipped, {0} is not behavior", Literal));

            MountBehavior = mountbehavior;
        }

        public override string ToString() => FormatToString(Literal);
    }
}