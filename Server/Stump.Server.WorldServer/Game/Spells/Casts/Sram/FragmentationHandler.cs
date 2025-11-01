using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Spells.Casts.Sram
{
    [SpellCastHandler(SpellIdEnum.FRAGMENTATION_TRAP_12954)]
    public class ToxinesHandler : DefaultSpellCastHandler //By Kenshin Version 2.61.10
    {
        public ToxinesHandler(SpellCastInformations cast) : base(cast)
        { }

        public override void Execute()
        {
            if (!base.Initialize())
                Initialize();

            //Acionador da Bomba
            //Ponto central da bomba
            FightActor _target = Fight.GetOneFighter(TargetedCell);
            Cell[] cells = _target.Position.Point.getCellsAdjacentsZone(_target.Map, 1, 3);

            var allFightersInZone = Fight.GetAllFighters().Where(x => cells.Contains(x.Cell));

            // Verifica o personagem que ativou a trap é inimigo
            if (!Caster.IsFriendlyWith(_target))
            {
                //Principal 
                //Effect_DamageFire - 4/7
                Handlers[0].Apply();
            }

            // Loop nos personagens dentro do raio de efeito da spell
            foreach (var fighter in allFightersInZone)
            {
                uint distance = fighter.Position.Point.DistanceTo(new MapPoint(TargetedCell));

                //Principal
                //Effect_DamageFire - 4/7
                //Handlers[0].AddAffectedActor(fighter);
                Handlers[0].Apply();

                if (distance == 1)
                {
                    //Effect_DamageFire - 8/10
                    //Handlers[1].AddAffectedActor(fighter);
                    Handlers[1].Apply();
                }
                else if (distance == 2)
                {
                    //Effect_DamageFire - 18/20
                    //Handlers[2].AddAffectedActor(fighter);
                    Handlers[2].Apply();
                }
                else if (distance == 3)
                {
                    //Effect_DamageFire - 28/30
                    //Handlers[3].AddAffectedActor(fighter);
                    Handlers[3].Apply();
                }
            }

            //Effect_CastSpell_1160 - 12967 LVL 1
            //Handlers[4].Apply();

            //Effect_TriggerBuff - 12968 LVL 1
            //Handlers[5].Apply();
        }
    }
}