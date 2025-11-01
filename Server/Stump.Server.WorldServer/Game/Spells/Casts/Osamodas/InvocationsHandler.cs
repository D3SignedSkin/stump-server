using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Osamodas
{
    [SpellCastHandler(13998)] //primero
    [SpellCastHandler(14006)] //segundo
    [SpellCastHandler(14007)] //tercero
    public class InvocationsHandler: DefaultSpellCastHandler
    {
        public InvocationsHandler(SpellCastInformations cast) : base(cast)
        {
        }
        
        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            if (Caster.HasState(1212))//1212	Descargado
                Handlers[3].Dice.DiceNum = 5813;//amorfo
            else if (Caster.HasState(1213))//1213	Tofucarga 1/4
                Handlers[3].Dice.DiceNum = 5789;//Tofu melánico
            else if (Caster.HasState(1214))//1214	Tofucarga 2/4
                Handlers[3].Dice.DiceNum = 5790;//Tofu albino
            else if (Caster.HasState(1215)) //1215	Tofucarga 3/4
                Handlers[3].Dice.DiceNum = 5791;//Tofu dorado
            else if (Caster.HasState(1216)) //1216	Tofucarga 4/4
                Handlers[3].Dice.DiceNum = 5791;//Tofu dorado
            else if (Caster.HasState(1217)) //1217	Jalacarga 1/4
                Handlers[3].Dice.DiceNum = 5795;//Jalató melánico
            else if (Caster.HasState(1218))//1218	Jalacarga 2/4
                Handlers[3].Dice.DiceNum = 5797;//Jalató albino
            else if (Caster.HasState(1219))//1219	Jalacarga 3/4
                Handlers[3].Dice.DiceNum = 5796;//Jalató castaño
            else if (Caster.HasState(1220))//1220	Jalacarga 4/4
                Handlers[3].Dice.DiceNum = 5796;//Jalató castaño
            else if (Caster.HasState(1221)) //1221	Sapocarga 1/4
                Handlers[3].Dice.DiceNum = 5792;//Sapo melánico
            else if (Caster.HasState(1222)) //1222	Sapocarga 2/4
                Handlers[3].Dice.DiceNum = 5794;//Sapo albino
            else if (Caster.HasState(1223)) //1223	Sapocarga 3/4
                Handlers[3].Dice.DiceNum = 5793;//Sapo verde
            else if (Caster.HasState(1224)) //1224	Sapocarga 4/4
                Handlers[3].Dice.DiceNum = 5793;//Sapo verde
            else if (Caster.HasState(1225)) //1225	Dragocarga 1/4
                Handlers[3].Dice.DiceNum = 5799;//Dragonito melánico
            else if (Caster.HasState(1226)) //1226	Dragocarga 2/4
                Handlers[3].Dice.DiceNum = 5800;//Dragonito albino
            else if (Caster.HasState(1227)) //1227	Dragocarga 3/4
                Handlers[3].Dice.DiceNum = 5798;//Dragonito rojo
            else if (Caster.HasState(1228)) //1228	Dragocarga 4/4
                Handlers[3].Dice.DiceNum = 5798;//Dragonito rojo
            
            Handlers[3].Dice.EffectId = EffectsEnum.Effect_SummonSlave;
            base.Execute();
        }
    }
}