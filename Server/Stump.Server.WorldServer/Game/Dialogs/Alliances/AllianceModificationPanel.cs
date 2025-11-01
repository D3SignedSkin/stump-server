using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Alliances;
using Stump.Server.WorldServer.Handlers.Alliances;
using Stump.Server.WorldServer.Handlers.Dialogs;
using Stump.Server.WorldServer.Handlers.Guilds;
using GuildEmblem = Stump.DofusProtocol.Types.SocialEmblem;

namespace Stump.Server.WorldServer.Game.Dialogs.Guilds
{
    public class AllianceModificationPanel : IDialog
    {
        public AllianceModificationPanel(Character character)
        {
            Character = character;
        }

        public Character Character
        {
            get;
        }

        public bool ChangeName
        {
            get;
            set;
        }
        public bool ChangeTag
        {
            get;
            set;
        }

        public bool ChangeEmblem
        {
            get;
            set;
        }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_ALLIANCE_RENAME;

        public void Open()
        {
            Character.SetDialog(this);
            AllianceHandler.SendAllianceModificationStartedMessage(Character.Client, ChangeName, ChangeTag, ChangeEmblem);
        }

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void ModifyAllianceName(string allianceName, string allianceTag)
        {
            var alliance = Singleton<AllianceManager>.Instance.TryGetAlliance((int)Character.Guild.Alliance.Id);

            if (!ChangeName)
                return;

            if (Character.GuildMember == null)
                return;

            if (!Character.GuildMember.IsBoss)
                return;

            if (Character.Guild.Alliance == null)
                return;

            if (alliance.Boss != Character.Guild)
                return;

            var result = Character.Guild.Alliance.SetAllianceName(Character, allianceName, allianceTag);

            AllianceHandler.SendAllianceCreationResultMessage(Character.Client, result);

            if (result == SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK)
                Close();
        }

        public void ModifyAllianceEmblem(GuildEmblem emblem)
        {
            var alliance = Singleton<AllianceManager>.Instance.TryGetAlliance((int)Character.Guild.Alliance.Id);

            if (!ChangeEmblem)
                return;

            if (Character.GuildMember == null)
                return;

            if (!Character.GuildMember.IsBoss)
                return;

            if (Character.Guild.Alliance == null)
                return;

            if (alliance.Boss != Character.Guild)
                return;

            var result = Character.Guild.Alliance.SetAllianceEmblem(Character, emblem);

            AllianceHandler.SendAllianceCreationResultMessage(Character.Client, result);

            if (result == SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK)
                Close();
        }
    }
}