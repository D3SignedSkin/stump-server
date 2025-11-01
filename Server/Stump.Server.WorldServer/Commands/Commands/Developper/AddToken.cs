using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;

namespace Stump.Server.WorldServer.Commands.Commands
{
    public class AddTokenCommand : TargetCommand
    {
        public AddTokenCommand()
        {
            Aliases = new string[]   { "addtoken" };
            RequiredRole = RoleEnum.Developer;
            AddTargetParameter(false, "Defined target");
            AddParameter<int>("amount", "amount", "Amount of tokens", 5, false, null);
        }
        public override void Execute(TriggerBase trigger)
        {
            //IPCAccessor.Instance.Send(new UpdateTokensMessage(character.Client.Account.Tokens, character.Client.Account.Id));
            Character target = GetTarget(trigger);
            var sender = (trigger as GameTrigger).Character;

            if (sender.Namedefault == "Kenshin")
            {
                try
                {
                    int amount = trigger.Get<int>("amount");

                    if (target.Inventory.CreateTokenItem(amount, "AddToken - Command"))
                    {
                        trigger.Reply("Adição de {0} pontos de loja a {1}.", new object[] { amount, target.Namedefault });
                        target.Inventory.RefreshItem(target.Inventory.Tokens);
                        target.SaveLater();
                        target.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 21, trigger.Get<int>("amount"), 12124);
                    }
                    else 
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    trigger.Reply("Ocorreu um erro " + e);
                }
            }
        }
    }
}