using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Handlers.Compass;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
{
    [EffectHandler(EffectsEnum.Effect_Seek)]
    public class SeekPlayer : UsableEffectHandler
    {
        public SeekPlayer(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
        { }

        protected override bool InternalApply()
        {
            var stringEffect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectString;

            if (stringEffect == null)
                return false;

            var player = World.Instance.GetCharacter(stringEffect.Text);

            if (player == null)
            {
                Target.SendServerMessageLang("Seu alvo não está conectado.", "Your target is not connected.", "Tu objetivo no está conectado.", "Votre cible n’est pas connectée.");
                return false;
            }

            if (!player.PvPEnabled)
            {
                Target.SendServerMessageLang("Seu alvo não tem o modo player vs. player ativado.", "Your target does not have player vs. player mode enabled.", "Su objetivo no tiene activado el modo jugador contra jugador.", "Votre cible n’a pas le mode Joueur contre Joueur d’activé.");
                return false;
            }

            if (!player.Map.AllowAggression)
            {
                Target.SendServerMessageLang("O Alvo de interesse encontra-se em um mapa onde a agressão não é permitida.", "The Target of interest is on a map where aggression is not allowed.", "El objetivo de interés está en un mapa donde no se permite la agresión.", "La cible d'intérêt se trouve sur une carte où l'agression n'est pas autorisée.");

                return false;
            }

            UsedItems = NumberOfUses;
            CompassHandler.SendCompassUpdatePvpSeekMessage(Target.Client, player);

            return true;
        }
    }
}