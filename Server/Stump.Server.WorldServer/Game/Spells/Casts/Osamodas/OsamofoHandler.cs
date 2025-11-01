using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Handlers.Actions;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Osamodas
{
    [SpellCastHandler((int)SpellIdEnum.FIRST_13998)]
    [SpellCastHandler((int)SpellIdEnum.SECOND_14006)]
    [SpellCastHandler((int)SpellIdEnum.THIRD_14007)]
    [SpellCastHandler((int)SpellIdEnum.FOURTH_14008)]
    [SpellCastHandler((int)SpellIdEnum.FIFTH_14009)]
    [SpellCastHandler((int)SpellIdEnum.SIXTH_14010)]
    public class OsamofoHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public OsamofoHandler(SpellCastInformations cast) : base(cast)
        { }

        //Lista de estados para cada Spell
        private static readonly List<(int, int)> spellstates = new List<(int, int)>
        {
            ((int)SpellIdEnum.FIRST_13998, 1235),
            ((int)SpellIdEnum.SECOND_14006, 1236),
            ((int)SpellIdEnum.THIRD_14007, 1237),
            ((int)SpellIdEnum.FOURTH_14008, 1238),
            ((int)SpellIdEnum.FIFTH_14009, 1239),
            ((int)SpellIdEnum.SIXTH_14010, 1240),
        };

        //Estados das Cargas de Tofu do 1 ao 4
        private static readonly List<int> statesTofu = new List<int>
        {
            // Tofu
            (int)SpellStatesEnum.TOFUCHARGE_14_1213,
            (int)SpellStatesEnum.TOFUCHARGE_24_1214,
            (int)SpellStatesEnum.TOFUCHARGE_34_1215,
            (int)SpellStatesEnum.TOFUCHARGE_44_1216,
        };

        //Estados das Cargas de gobbal do 1 ao 4
        private static readonly List<int> statesGobbal = new List<int>
        {
            // Gobbal
            (int)SpellStatesEnum.GOBBACHARGE_14_1217,
            (int)SpellStatesEnum.GOBBACHARGE_24_1218,
            (int)SpellStatesEnum.GOBBACHARGE_34_1219,
            (int)SpellStatesEnum.GOBBACHARGE_44_1220,
        };

        //Estados das Cargas de Sapo do 1 ao 4
        private static readonly List<int> statesSapo = new List<int>
        {
            // Sapo
            (int)SpellStatesEnum.TOACHARGE_14_1221,
            (int)SpellStatesEnum.TOACHARGE_24_1222,
            (int)SpellStatesEnum.TOACHARGE_34_1223,
            (int)SpellStatesEnum.TOACHARGE_44_1224,
        };

        //Estados das Cargas de Dragão do 1 ao 4
        private static readonly List<int> statesDagro = new List<int>
        {
            // Dragão
            (int)SpellStatesEnum.WYRMLICHARGE_14_1225,
            (int)SpellStatesEnum.WYRMLICHARGE_24_1226,
            (int)SpellStatesEnum.WYRMLICHARGE_34_1227,
            (int)SpellStatesEnum.WYRMLICHARGE_44_1228
        };

        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            //Se o handler for invocação, pega os dados de quem invocou.. se não for pega os dados do jogador ativo,
            var fightPlaying = Fight.FighterPlaying is SummonedMonster ? Fight.FighterPlaying.Summoner : Fight.FighterPlaying;

            //Verifica se o jogador possui os estados e de qual invocação os estado pertence
            int TofuState = GetActiveState(fightPlaying, statesTofu);
            int GobbalState = GetActiveState(fightPlaying, statesGobbal);
            int SapoState = GetActiveState(fightPlaying, statesSapo);
            int DagroState = GetActiveState(fightPlaying, statesDagro);

            //Verificar se o jogador possui alguma invocação já invocada com o estado da spell utilizada
            int spellStateActualSpell = spellstates.FirstOrDefault(x => x.Item1 == Spell.Template.Id).Item2;

            //Verificando se já existe alguma invocação com o estado da spell acima
            SummonedFighter fightInvocation = Caster.Summons.FirstOrDefault(summoner => summoner.GetStates().Any(buff => buff.State.Id == spellStateActualSpell));

            //Verificando se a celula target está vazia para teleportar
            bool cellfree = !Fight.GetAllFighters().Any(x => x.Cell == TargetedCell);

            //Se existir ele teleporta a invocação para a nova celula se a celular não estiver ocupada
            if (fightInvocation != null && cellfree)
            {
                fightInvocation.Position.Cell = TargetedCell;
                Fight.ForEach(entry => ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(entry.Client, fightInvocation, fightInvocation, TargetedCell), true);
            }
            //Se não existir ele invoca uma nova
            else
            {
                //Verificando qual é a invocação pelas cargas do invocador
                if (TofuState > 0)
                {
                    setInvocationTofu();
                    Handlers[3].Dice.EffectId = EffectsEnum.Effect_Summon;
                }
                else if (GobbalState > 0)
                {
                    setInvocationGobbal();
                    Handlers[3].Dice.EffectId = EffectsEnum.Effect_Summon;
                }
                else if (SapoState > 0)
                {
                    setInvocationSapo();
                    Handlers[3].Dice.EffectId = EffectsEnum.Effect_Summon;
                }
                else if (DagroState > 0)
                {
                    setInvocationDrago();
                    Handlers[3].Dice.EffectId = EffectsEnum.Effect_Summon;
                }
                else
                {
                    Handlers[3].Dice.EffectId = EffectsEnum.Effect_SummonSlave;
                    Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.INFORMO_5813;
                }

                this.setInovcation();
            }

            if (fightPlaying.GetStates().Any(playerState => !statesTofu.Contains(playerState.State.Id) || !statesGobbal.Contains(playerState.State.Id) || !statesSapo.Contains(playerState.State.Id) || !statesDagro.Contains(playerState.State.Id)))
            {
                EffectDice dice = new EffectDice(EffectsEnum.Effect_AddState, (int)SpellStatesEnum.UNCHARGED_1212, 0, 0);
                dice.ZoneShape = SpellShapeEnum.P;
                dice.ZoneSize = 1;
                dice.IsDirty = true;
                dice.Triggers = "I";
                dice.Duration = -1;
                dice.TargetMask = "C";
                var handler = EffectManager.Instance.GetSpellEffectHandler(dice, fightPlaying, new DefaultSpellCastHandler(new SpellCastInformations(fightPlaying, new Spell((int)SpellIdEnum.OSAMODASS_WHIP_14155, 1), fightPlaying.Cell)), fightPlaying.Cell, false);
                handler.Apply();
            }
        }

        private int GetActiveState(FightActor fightPlaying, List<int> stateIds)
        {
            return fightPlaying.GetStates().FirstOrDefault(playerState => stateIds.Contains(playerState.State.Id) && !playerState.IsDisabled)?.State.Id ?? -1;
        }
    
        private void setInovcation()
        {
            //Effect_Summon
            Handlers[3].AddAffectedActor(Caster);
            Handlers[3].Apply();

            FightActor _target = Fight.GetOneFighter(TargetedCell);

            //Effect_AddState
            Handlers[0].AddAffectedActor(_target);
            Handlers[0].Apply();

            //Effect_AddState
            Handlers[1].AddAffectedActor(_target);
            Handlers[1].Apply();

            //Effect_2794
            //Handlers[2].Apply();
            //Handlers[2].AddAffectedActor(m_affectedactors);

            //Effect_2794
            //Handlers[4].Apply();

            //Caster.CastAutoSpell(new Spell((int)SpellIdEnum.OSAMODASS_WHIP_14155, (short)Caster.Level), Caster.Cell);
        }

        private void setInvocationTofu()
        {
            //Tofucarga 1/4
            if (Caster.HasState((int)SpellStatesEnum.TOFUCHARGE_14_1213))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.TOFU_MLANIQUE_5789;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOFUCHARGE_14_1213);
            }
            //Tofucarga 2/4
            else if (Caster.HasState((int)SpellStatesEnum.TOFUCHARGE_24_1214))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.TOFU_ALBINOS_5790;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOFUCHARGE_14_1213);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOFUCHARGE_24_1214);
            }
            //Tofucarga 3/4
            else if (Caster.HasState((int)SpellStatesEnum.TOFUCHARGE_34_1215))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.TOFU_DOR_5791;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOFUCHARGE_14_1213);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOFUCHARGE_24_1214);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOFUCHARGE_34_1215);
            }
            //Tofucarga 4/4
            else
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.TOFU_DOR_5791;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOFUCHARGE_14_1213);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOFUCHARGE_24_1214);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOFUCHARGE_34_1215);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOFUCHARGE_44_1216);
            }
        }

        private void setInvocationGobbal()
        {
            //Gobbal 1/4
            if (Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_14_1217) && !Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_24_1218) && !Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_34_1219) && !Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_44_1220))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.BOUFTOU_MLANIQUE_5795;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.GOBBACHARGE_14_1217);
            }
            //Gobbal 2/4
            else if (Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_14_1217) && Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_24_1218) && !Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_34_1219) && !Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_44_1220))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.BOUFTOU_ALBINOS_5797;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.GOBBACHARGE_14_1217);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.GOBBACHARGE_24_1218);
            }
            //Gobbal 3/4
            else if (Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_14_1217) && Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_24_1218) && Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_34_1219) && !Caster.HasState((int)SpellStatesEnum.GOBBACHARGE_44_1220))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.BOUFTOU_CHTAIN_5796;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.GOBBACHARGE_14_1217);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.GOBBACHARGE_24_1218);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.GOBBACHARGE_34_1219);
            }
            //Gobbal 4/4
            else
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.BOUFTOU_CHTAIN_5796;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.GOBBACHARGE_14_1217);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.GOBBACHARGE_24_1218);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.GOBBACHARGE_34_1219);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.GOBBACHARGE_44_1220);
            }
        }

        private void setInvocationSapo()
        {
            //Sapocarga 1/4
            if (Caster.HasState((int)SpellStatesEnum.TOACHARGE_14_1221))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.CRAPAUD_MLANIQUE_5792;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOACHARGE_14_1221);
            }
            //Sapocarga 2/4
            else if (Caster.HasState((int)SpellStatesEnum.TOACHARGE_24_1222))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.CRAPAUD_ALBINOS_5794;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOACHARGE_14_1221);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOACHARGE_24_1222);
            }
            //Sapocarga 3/4
            else if (Caster.HasState((int)SpellStatesEnum.TOACHARGE_34_1223))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.CRAPAUD_VERDOYANT_5793;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOACHARGE_14_1221);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOACHARGE_24_1222);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOACHARGE_34_1223);
            }
            //Sapocarga 4/4
            else
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.CRAPAUD_VERDOYANT_5793;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOACHARGE_14_1221);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOACHARGE_24_1222);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOACHARGE_34_1223);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.TOACHARGE_44_1224);
            }
        }

        private void setInvocationDrago()
        {
            //Dragocarga 1/4
            if (Caster.HasState((int)SpellStatesEnum.WYRMLICHARGE_14_1225))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.DRAGONNET_MLANIQUE_5799;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.WYRMLICHARGE_14_1225);
            }
            //Dragocarga 2/4
            else if (Caster.HasState((int)SpellStatesEnum.WYRMLICHARGE_24_1226))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.DRAGONNET_ALBINOS_5800;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.WYRMLICHARGE_14_1225);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.WYRMLICHARGE_24_1226);
            }
            //Dragocarga 3/4
            else if (Caster.HasState((int)SpellStatesEnum.WYRMLICHARGE_34_1227))
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.DRAGONNET_ROUGE_5798;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.WYRMLICHARGE_14_1225);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.WYRMLICHARGE_24_1226);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.WYRMLICHARGE_34_1227);
            }
            //Dragocarga 4/4
            else
            {
                Handlers[3].Dice.DiceNum = (int)MonsterIdEnum.DRAGONNET_ROUGE_5798;
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.WYRMLICHARGE_14_1225);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.WYRMLICHARGE_24_1226);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.WYRMLICHARGE_34_1227);
                SpellEffectHandler.RemoveStateBuff(Caster, SpellStatesEnum.WYRMLICHARGE_44_1228);
            }
        }
    }
}
