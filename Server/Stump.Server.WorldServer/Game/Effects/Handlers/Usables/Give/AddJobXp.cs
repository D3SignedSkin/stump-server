using System;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Jobs;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Jobs;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
{
    [EffectHandler(EffectsEnum.Effect_AddJobXp)]
    public class AddJobXp : UsableEffectHandler
    {
        public AddJobXp(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
        { }

        protected override bool InternalApply()
        {
            var effect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

            if (effect == null)
                return false;

            int JobId = effect.Value;
            int xp = (Item.Template.Effects.Where(x => x.EffectId == EffectsEnum.Effect_AddJobXp).FirstOrDefault() as EffectDice).Value;

            JobTemplate jobTemplate;

            if (xp == 0 || JobId == 0)
                return false;

            try
            {
                jobTemplate = JobManager.Instance.GetJobTemplate(JobId);

                var ownerJob = Target.Jobs[JobId];

                if (ownerJob is null)
                    return false;

                if (ownerJob.Level == 200)
                {
                    return false;
                }
                else if (xp + ownerJob.Experience < 1)
                {
                    ownerJob.Experience = 0;
                }
                else
                {
                    ownerJob.Experience += xp;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            UsedItems = NumberOfUses;

            return true;
        }
    }
}