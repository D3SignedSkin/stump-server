using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Spells;
using System.Linq;

namespace Stump.Server.WorldServer.Handlers.Context.RolePlay
{
    public partial class ContextRoleplayHandler : WorldHandlerContainer
    {
        [WorldHandler(SpellVariantActivationRequestMessage.Id)]
        public static void HandleSpellVariantActivationMessage(WorldClient client, SpellVariantActivationRequestMessage message)
        {
            if (client.Character.IsInFight() && client.Character.Fight.State != Game.Fights.FightState.Placement)
            {
                return;
            }

            var breedSpell = SpellManager.GetSpellVariant(message.spellId);

            if (breedSpell == null)
                return;

            if (client.Character.Breed.Id == breedSpell.BreedId)
            {
                var variant = breedSpell.VariantId == message.spellId;
                var required = variant ? breedSpell.Spell : breedSpell.VariantId;

                if (Game.Actors.RolePlay.Characters.Character.SpellsBlock.Contains(message.spellId))
                {
                    client.Character.OpenPopupLang(
                        "O feitiço está desabilitado para correção. Aguarde nosso anúncio de correção em nosso discord oficial.",
                        "The spell is disabled for correction. Wait for our correction announcement on our official discord.",
                        "El hechizo está desactivado para su corrección. Espere nuestro anuncio de corrección en nuestro discord oficial.",
                        "Le sort est désactivé pour correction. Attendez notre annonce de correction sur notre discord officiel.", "Server", 1);

                    client.Send(new SpellVariantActivationMessage(message.spellId, false));
                    return;
                }

                if (client.Character.Spells.GetSpell(required) == null)
                {
                    client.Send(new SpellVariantActivationMessage(message.spellId, false));
                    return;
                }

                if (client.Character.Level < (variant ? breedSpell.VariantLevel : breedSpell.ObtainLevel))
                {
                    client.Send(new SpellVariantActivationMessage(message.spellId, false));
                    return;
                }

                client.Character.Shortcuts.SwapSpellShortcuts((short)required, (short)message.spellId);
                client.Character.Spells.GetSpell(required).Record.Selected = false;
                client.Character.Spells.GetSpell(message.spellId).Record.Selected = true;
                client.Send(new ShortcutBarContentMessage((sbyte)ShortcutBarEnum.SPELL_SHORTCUT_BAR, client.Character.Shortcuts.GetShortcuts(ShortcutBarEnum.SPELL_SHORTCUT_BAR).Select(entry => entry.GetNetworkShortcut())));
                client.Send(new SpellVariantActivationMessage(message.spellId, true));
            }
            else
            {
                client.Send(new SpellVariantActivationMessage(message.spellId, false));
            }
        }
    }
}