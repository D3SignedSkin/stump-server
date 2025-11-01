using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.game.spells.Casts.Forjalanza
{
    [SpellCastHandler(24391)]
    public class Estandarte: DefaultSpellCastHandler
    {
        public Estandarte(SpellCastInformations cast) : base(cast)
        {
        }
        
        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            var target = Handlers;
            base.Execute();
        }
    }
}