using NLog;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Conditions.Criterions;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    [ItemHasEffect(EffectsEnum.Effect_621)]
    public class SpawnMonster : BasePlayerItem
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SpawnMonster(Character owner, PlayerItemRecord record)
            : base(owner, record)
        {
        }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {

            var level = (Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_621) as EffectInteger).Value;
            var monster_id = (Template.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_621) as EffectDice).Max;
            var grade_id = (Template.Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_621) as EffectDice).Min;

            if (level == 0 || monster_id == 0 || grade_id == 0)
            {
                return base.UseItem(amount, targetCell, target);
            }
            try
            {

                List<Monster> Monsters = new List<Monster>();
                var grade = Singleton<MonsterManager>.Instance.GetMonsterGrade(monster_id, grade_id);
                var position = new ObjectPosition(Owner.Map, Owner.Cell, (DirectionsEnum)5);
                var monster_group = new MonsterGroup(position.Map.GetNextContextualId(), position);
                var quantity = 1;
                while (quantity < 12)
                {
                    if (level > quantity * 20 + 10)
                        quantity++;
                    else
                        break;
                }if (quantity > 8) quantity = 8;
                for (int i = 0; i< quantity; i++)
                    Monsters.Add(new Monster(grade, monster_group));
                var fight = Singleton<FightManager>.Instance.CreatePvMFight(Owner.Map);
                fight.ChallengersTeam.AddFighter(Owner.CreateFighter(fight.ChallengersTeam));
                Monsters.ForEach(x => fight.DefendersTeam.AddFighter(new MonsterFighter(fight.DefendersTeam, x)));
                fight.StartPlacement();
            }
            catch(System.Exception ex)
            {
                logger.Error(string.Format("Spawnmonster {0} error: {1}",
                    Template.Id, ex));
                return 0;
            }           

            return 1;
        }
    }
}