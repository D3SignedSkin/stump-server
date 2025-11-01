using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.game.spells.Casts.Forjalanza
{
    [SpellCastHandler(23262)]
    public class LanzaInmortal:  DefaultSpellCastHandler
    {
        public LanzaInmortal(SpellCastInformations cast) : base(cast)
        {
        }
        
        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            //if (Handlers.Length > 3)
                //Handlers[4].SetAffectedActor(Caster);
           

            base.Execute();
        }
    }
    
    [SpellCastHandler(23279)]
    public class LanzaMortal:  DefaultSpellCastHandler
    {
        public LanzaMortal(SpellCastInformations cast) : base(cast)
        {
        }
        
        public override void Execute()
        {
            if (!m_initialized)
                Initialize();
            
            //eccotrar al invocador de quien lanzo el echizo entre los fighters 
            var caster = Fight.Fighters.FirstOrDefault(x => Caster.Summoner == x);
            if (caster != null && Handlers.FirstOrDefault(x => x.Effect.EffectId == EffectsEnum.Effect_AddState) != null)
                Handlers[0].SetAffectedActor(caster);



            base.Execute();
        }
    }
}