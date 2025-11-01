using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Osamodas
{
    [SpellCastHandler(SpellIdEnum.OSAMODASS_WHIP_13992)]
    public class WhipHandler : DefaultSpellCastHandler // By Kenshin Version 2.61.10
    {
        // Osamodas States
        private static readonly List<int> states = new List<int>
        {
            // Tofu
            (int)SpellStatesEnum.TOFUCHARGE_14_1213,
            (int)SpellStatesEnum.TOFUCHARGE_24_1214,
            (int)SpellStatesEnum.TOFUCHARGE_34_1215,
            (int)SpellStatesEnum.TOFUCHARGE_44_1216,

            // Gobbal
            (int)SpellStatesEnum.GOBBACHARGE_14_1217,
            (int)SpellStatesEnum.GOBBACHARGE_24_1218,
            (int)SpellStatesEnum.GOBBACHARGE_34_1219,
            (int)SpellStatesEnum.GOBBACHARGE_44_1220,

            // Sapo
            (int)SpellStatesEnum.TOACHARGE_14_1221,
            (int)SpellStatesEnum.TOACHARGE_24_1222,
            (int)SpellStatesEnum.TOACHARGE_34_1223,
            (int)SpellStatesEnum.TOACHARGE_44_1224,

            // Dragão
            (int)SpellStatesEnum.WYRMLICHARGE_14_1225,
            (int)SpellStatesEnum.WYRMLICHARGE_24_1226,
            (int)SpellStatesEnum.WYRMLICHARGE_34_1227,
            (int)SpellStatesEnum.WYRMLICHARGE_44_1228
        };

        public WhipHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            var fighter = Fight.FighterPlaying is SummonedMonster ? Fight.FighterPlaying.Summoner : Fight.FighterPlaying;
            var uncharged = fighter.GetStates().FirstOrDefault(playerState => states.Contains(playerState.State.Id) && !playerState.IsDisabled);

            if (Spell.CurrentLevel == 1)
            {
                ApplyTofuCharges(fighter, uncharged);
            }
            else if (Spell.CurrentLevel == 2)
            {
                ApplyGobbaCharges(fighter, uncharged);
            }
            else if (Spell.CurrentLevel == 3)
            {
                ApplyToaCharges(fighter, uncharged);
            }
            else if (Spell.CurrentLevel == 4)
            {
                ApplyWyrmliCharges(fighter, uncharged);
            }
        }

        //Tofu Carga
        private void ApplyTofuCharges(FightActor fighter, StateBuff uncharged)
        {
            //Adiciona a primeira Carga de Tofu se estiver descarregado
            if (fighter.HasState((int)SpellStatesEnum.UNCHARGED_1212))
            {
                //Tofucarga 1/4
                ApplyHandler(4);
                SpellEffectHandler.RemoveStateBuff(fighter, SpellStatesEnum.UNCHARGED_1212);

                if (uncharged != null)
                {
                    SpellEffectHandler.RemoveStateBuff(fighter, (SpellStatesEnum)uncharged.State.Id);
                }
            }
            else
            {
                if (fighter.HasState((int)SpellStatesEnum.TOFUCHARGE_44_1216) && !fighter.HasState((int)SpellStatesEnum.UNCHARGED_1212))
                {
                    //Descarregado
                    ApplyHandler(0);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.TOFUCHARGE_44_1216);
                }
                else if (fighter.HasState((int)SpellStatesEnum.TOFUCHARGE_34_1215) && !fighter.HasState((int)SpellStatesEnum.TOFUCHARGE_44_1216))
                {
                    //Tofucarga 4/4 and Descarregado
                    ApplyHandler(1);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.TOFUCHARGE_34_1215);
                }
                else if (fighter.HasState((int)SpellStatesEnum.TOFUCHARGE_24_1214) && !fighter.HasState((int)SpellStatesEnum.TOFUCHARGE_34_1215))
                {
                    //Tofucarga 3/4
                    ApplyHandler(2);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.TOFUCHARGE_24_1214);
                }
                else if (fighter.HasState((int)SpellStatesEnum.TOFUCHARGE_14_1213) && !fighter.HasState((int)SpellStatesEnum.TOFUCHARGE_24_1214))
                {
                    //Tofucarga 2/4
                    ApplyHandler(3);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.TOFUCHARGE_14_1213);
                }
            }
        }

        //Papatudo Carga
        private void ApplyGobbaCharges(FightActor fighter, StateBuff uncharged)
        {
            if (fighter.HasState((int)SpellStatesEnum.UNCHARGED_1212))
            {
                ApplyHandler(4);
                SpellEffectHandler.RemoveStateBuff(fighter, SpellStatesEnum.UNCHARGED_1212);

                if (uncharged != null)
                {
                    SpellEffectHandler.RemoveStateBuff(fighter, (SpellStatesEnum)uncharged.State.Id);
                }
            }
            else
            {
                if (fighter.HasState((int)SpellStatesEnum.GOBBACHARGE_44_1220) && !fighter.HasState((int)SpellStatesEnum.UNCHARGED_1212))
                {
                    ApplyHandler(0);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.GOBBACHARGE_44_1220);
                }
                else if (fighter.HasState((int)SpellStatesEnum.GOBBACHARGE_34_1219) && !fighter.HasState((int)SpellStatesEnum.GOBBACHARGE_44_1220))
                {
                    ApplyHandler(1);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.GOBBACHARGE_34_1219);
                }
                else if (fighter.HasState((int)SpellStatesEnum.GOBBACHARGE_24_1218) && !fighter.HasState((int)SpellStatesEnum.GOBBACHARGE_34_1219))
                {
                    ApplyHandler(2);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.GOBBACHARGE_24_1218);
                }
                else if (fighter.HasState((int)SpellStatesEnum.GOBBACHARGE_14_1217) && !fighter.HasState((int)SpellStatesEnum.GOBBACHARGE_24_1218))
                {
                    ApplyHandler(3);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.GOBBACHARGE_14_1217);
                }
            }
        }

        //Sapo Carga
        private void ApplyToaCharges(FightActor fighter, StateBuff uncharged)
        {
            if (fighter.HasState((int)SpellStatesEnum.UNCHARGED_1212))
            {
                ApplyHandler(4);
                SpellEffectHandler.RemoveStateBuff(fighter, SpellStatesEnum.UNCHARGED_1212);

                if (uncharged != null)
                {
                    SpellEffectHandler.RemoveStateBuff(fighter, (SpellStatesEnum)uncharged.State.Id);
                }
            }
            else
            {
                if (fighter.HasState((int)SpellStatesEnum.TOACHARGE_44_1224) && !fighter.HasState((int)SpellStatesEnum.UNCHARGED_1212))
                {
                    ApplyHandler(0);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.TOACHARGE_44_1224);
                }
                else if (fighter.HasState((int)SpellStatesEnum.TOACHARGE_34_1223) && !fighter.HasState((int)SpellStatesEnum.TOACHARGE_44_1224))
                {
                    ApplyHandler(1);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.TOACHARGE_34_1223);
                }
                else if (fighter.HasState((int)SpellStatesEnum.TOACHARGE_24_1222) && !fighter.HasState((int)SpellStatesEnum.TOACHARGE_34_1223))
                {
                    ApplyHandler(2);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.TOACHARGE_24_1222);
                }
                else if (fighter.HasState((int)SpellStatesEnum.TOACHARGE_14_1221) && !fighter.HasState((int)SpellStatesEnum.TOACHARGE_24_1222))
                {
                    ApplyHandler(3);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.TOACHARGE_14_1221);
                }
            }
        }

        //Drago Carga
        private void ApplyWyrmliCharges(FightActor fighter, StateBuff uncharged)
        {
            if (fighter.HasState((int)SpellStatesEnum.UNCHARGED_1212))
            {
                ApplyHandler(4);
                SpellEffectHandler.RemoveStateBuff(fighter, SpellStatesEnum.UNCHARGED_1212);

                if (uncharged != null)
                {
                    SpellEffectHandler.RemoveStateBuff(fighter, (SpellStatesEnum)uncharged.State.Id);
                }
            }
            else
            {
                if (fighter.HasState((int)SpellStatesEnum.WYRMLICHARGE_44_1228) && !fighter.HasState((int)SpellStatesEnum.UNCHARGED_1212))
                {
                    ApplyHandler(0);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.WYRMLICHARGE_44_1228);
                }
                else if (fighter.HasState((int)SpellStatesEnum.WYRMLICHARGE_34_1227) && !fighter.HasState((int)SpellStatesEnum.WYRMLICHARGE_44_1228))
                {
                    ApplyHandler(1);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.WYRMLICHARGE_34_1227);
                }
                else if (fighter.HasState((int)SpellStatesEnum.WYRMLICHARGE_24_1226) && !fighter.HasState((int)SpellStatesEnum.WYRMLICHARGE_34_1227))
                {
                    ApplyHandler(2);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.WYRMLICHARGE_24_1226);
                }
                else if (fighter.HasState((int)SpellStatesEnum.WYRMLICHARGE_14_1225) && !fighter.HasState((int)SpellStatesEnum.WYRMLICHARGE_24_1226))
                {
                    ApplyHandler(3);
                    RemoveStateIfUncharged(fighter, uncharged, SpellStatesEnum.WYRMLICHARGE_14_1225);
                }
            }
        }

        private void RemoveStateIfUncharged(FightActor fighter, StateBuff uncharged, SpellStatesEnum stateToRemove)
        {
            if (uncharged != null)
            {
                SpellEffectHandler.RemoveStateBuff(fighter, stateToRemove);
            }
        }

        private void ApplyHandler(short number)
        {
            Handlers[number].AddAffectedActor(Fight.FighterPlaying);
            Handlers[number].Apply();
        }
    }
}