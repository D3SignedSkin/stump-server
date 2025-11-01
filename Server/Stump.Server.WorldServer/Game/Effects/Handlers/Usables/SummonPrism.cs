using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Handlers;
using Stump.Server.WorldServer.Game.Effects.Handlers.Usables;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Prisms;
using System;

namespace Stump.Server.WorldServer.Game.Effects.Usables
{
    [EffectHandler(EffectsEnum.Effect_Summon_Prism)]
    public class SummonPrism : UsableEffectHandler
    {
        public SummonPrism(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
        { }

        protected override bool InternalApply()
        {
            return Target.Guild?.Alliance != null && Singleton<PrismManager>.Instance.TryAddPrism(Target);
        }
    }
}
