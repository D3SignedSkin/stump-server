using System;
using System.Drawing;
using Stump.Core.Attributes;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
{
    [EffectHandler(EffectsEnum.Effect_AddNivel_1049)]
    public class GiveNivel : UsableEffectHandler
    {
        [Variable]
        public static short NivelLimit = 199;

        public GiveNivel(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
        {
        }

        protected override bool InternalApply()
        {
            var effectInt = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

            if (effectInt == null)
                return false;

            if (Target.Level >= NivelLimit)
                return false;

            if (Item.Template.Id == 12332 && Target.Level > 25)
                return false;

            if (Item.Template.Id == 12333 && Target.Level > 50)
                return false;

            if (Item.Template.Id == 30358 && Target.Level > 150)
                return false;

            if (Item.Template.Id == 30359 && Target.Level > 198)
                return false;

            var addnivel = AdjustNivelAdd((short)(effectInt.Value * NumberOfUses));

            UsedItems = NumberOfUses;

            Target.LevelUp((ushort)addnivel);
            return true;
        }

        short AdjustNivelAdd(short addnivel)
        {
            if (Target.Level >= NivelLimit)
                return 0;

            return Target.Level + addnivel > NivelLimit ? NivelLimit : addnivel;
        }
    }
}