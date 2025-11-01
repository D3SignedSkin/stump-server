using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;

using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Linq;

namespace Stump.Server.WorldServer.Commands.Commands.Players
{
    public class CharCommand : SubCommandContainer
    {
        public CharCommand()
        {
            Aliases = new[] { "actor", ""};
            RequiredRole = RoleEnum.Administrator;
            Description = "Provides many commands to view statistics.";
        }
    }

    public class StatsViewommand : TargetSubCommand
    {
        public StatsViewommand()
        {
            Aliases = new[] { "view" };
            RequiredRole = RoleEnum.Administrator;
            Description = "Provides many commands to view statistics.";
            ParentCommandType = typeof(CharCommand);
            AddParameter("target", "t", "Character whose stats will be viewed.", converter: ParametersConverter.CharacterConverter, isOptional: true);
        }

        public override void Execute(TriggerBase trigger)
        {
            if (trigger.IsArgumentDefined("target"))
            {
                var target = trigger.Get<Character>("target");
                var source = ((GameTrigger)trigger).Character.Client;

                source.Send(new CharacterStatsListMessage(target.GetCharacterCharacteristicsInformations()));

                //Todo - Verificar BUG de passar asa para outro.
                //source.Send(new CharacterSelectedSuccessMessage(target.GetCharacterBaseInformations(), false));

                source.Send(new InventoryContentMessage(target.Inventory.Select(entry => entry.GetObjectItem()), (ulong)target.Inventory.Kamas));
                source.Send(new SpellListMessage(false, target.Spells.GetSpells().Select(entry => entry.GetSpellItem())));                
                source.Character.OpenPopupLang(
                    "Você está agora visualizando as características do jogador " + target.Namedefault + ".", 
                    "You are now viewing the characteristics of player " + target.Namedefault + ".", 
                    "Ahora estás viendo las características del jugador " + target.Namedefault + ".", 
                    "Vous visualisez maintenant les caractéristiques du joueur " + target.Namedefault + ".",
                    "Stats Comand: ",
                    0);
            }
            else
            {
                trigger.ReplyError("Please define a target");
            }
        }
    }
}