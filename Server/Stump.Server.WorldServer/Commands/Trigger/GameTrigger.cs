using System;
using System.Globalization;
using MongoDB.Bson;
using Stump.Core.IO;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Commands.Trigger
{
    public abstract class GameTrigger : TriggerBase
    {
        protected GameTrigger(StringStream args, Character character)
            : base(args, character.UserGroup.Role)
        {
            Character = character;
        }


        protected GameTrigger(string args, Character character)
            : base(args, character.UserGroup.Role)
        {
            Character = character;
        }

        public override RoleEnum UserRole => Character.UserGroup.Role;

        public override bool CanFormat => true;

        public Character Character
        {
            get;
            protected set;
        }

        public override bool CanAccessCommand(CommandBase command) => Character.UserGroup.IsCommandAvailable(command);

        public override void Log()
        {
            //if (BoundCommand.RequiredRole <= RoleEnum.Player)
            //    return;

            var CharacterRank = "Player";

            if (Character.Account.UserGroupId >= 4 && Character.Account.UserGroupId <= 9)
                CharacterRank = "Staff";

            var document = new BsonDocument
              {
                    { "HardwareId", Character.Client.Account.LastHardwareId },
                    { "AccountId", Character.Account.Id },
                    { "AccountName", Character.Account.Login },
                    { "AccountUserGroup", CharacterRank },
                    { "CharacterId", Character.Id },
                    { "CharacterName", Character.Name },
                    { "Command", BoundCommand.Aliases[0] },
                    { "Parameters", Args.String },
                    { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
              };
                  MongoLogger.Instance.Insert("Player_Commands", document);
        }
    }
}