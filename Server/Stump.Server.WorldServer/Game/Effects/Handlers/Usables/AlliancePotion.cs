using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Alliances;
using Stump.Server.WorldServer.Game.Dialogs.Guilds;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
{
    [EffectHandler(EffectsEnum.Effect_ChangeAllianceName)]
    public class AlliancePotionName : UsableEffectHandler
    {
        public AlliancePotionName(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item) { }

        protected override bool InternalApply()
        {
            var alliance = Singleton<AllianceManager>.Instance.TryGetAlliance((int)Target.Client.Character.Guild.Alliance.Id);

            if (Target.GuildMember == null)
                return false;

            if (!Target.GuildMember.IsBoss)
                return false;

            if (Target.Client.Character.Guild.Alliance == null)
                return false;

            if (alliance.Boss != Target.Client.Character.Guild)
                return false;

            var panel = new AllianceModificationPanel(Target) { ChangeName = true, ChangeTag = true, ChangeEmblem = false };
            panel.Open();

            UsedItems = 0;

            return true;
        }
    }


    [EffectHandler(EffectsEnum.Effect_ChangeAllianceEmblem)]
    public class AlliancePotionBlazon : UsableEffectHandler
    {
        public AlliancePotionBlazon(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item) { }

        protected override bool InternalApply()
        {
            var alliance = Singleton<AllianceManager>.Instance.TryGetAlliance((int)Target.Client.Character.Guild.Alliance.Id);

            if (Target.GuildMember == null)
                return false;

            if (!Target.GuildMember.IsBoss)
                return false;

            if (Target.Client.Character.Guild.Alliance == null)
                return false;

            if (alliance.Boss != Target.Client.Character.Guild)
                return false;

            var panel = new AllianceModificationPanel(Target) { ChangeName = false, ChangeTag = false, ChangeEmblem = true };
            panel.Open();

            UsedItems = 0;

            return true;
        }
    }
}