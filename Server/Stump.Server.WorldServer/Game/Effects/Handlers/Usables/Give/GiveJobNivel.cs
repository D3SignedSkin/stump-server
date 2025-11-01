using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Jobs;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Game.Jobs;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
{
    [EffectHandler(EffectsEnum.Effect_1050)]
    public class GiveJobNivel : UsableEffectHandler
    {
        public GiveJobNivel(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
        { }

        protected override bool InternalApply()
        {
            var effect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

            if (effect == null)
                return false;

            int level = 200;
            int JobId = effect.Value;
            JobTemplate jobTemplate;

            if (level == 0 || JobId == 0)
                return false;

            try
            {
                jobTemplate = JobManager.Instance.GetJobTemplate(JobId);
                var job = Target.Jobs[JobId];
                int CharLevel = job.Level;
                int CharFinalLevel = level;

                if (CharFinalLevel == 0)
                    return false;

                if (job.Level == 200)
                    return false;

                else if (level + job.Level < 1)
                    job.Experience = 0;
                else
                    job.Experience = ExperienceManager.Instance.GetJobLevelExperience((byte)CharFinalLevel);
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