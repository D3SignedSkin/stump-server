using System.Linq;
using System.Threading.Tasks;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Breach;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    [Discriminator("PortalBreachBranches", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class PortalBreachBranches : CustomSkill
    {
        public PortalBreachBranches(int id, InteractiveCustomSkillRecord skillTemplate, InteractiveObject interactiveObject) : base(id, skillTemplate, interactiveObject)
        {
        }

        public override int StartExecute(Character character)
        {
            if (character.BreachOwner == null)
            {
                if (character.Level >= 180)
                {
                    if (character.BreachStep < 201)
                    {
                        if (character.BreachBranches != null)
                        {
                            ExtendedBreachBranch extendedBreachBranch = character.BreachBranches[0];
                            switch (Id)
                            {
                                case 104589:
                                    extendedBreachBranch = character.BreachBranches[2];
                                    break;
                                case 104590:
                                    extendedBreachBranch = character.BreachBranches[1];
                                    break;
                                case 104591:
                                    extendedBreachBranch = character.BreachBranches[0];
                                    break;
                                default:
                                    break;
                            }

                            Map map = World.Instance.GetMap((uint)extendedBreachBranch.map);

                            character.Teleport(map, character.Cell);

                            if (character.BreachGroup != null)
                            {
                                foreach (long guest in character.BreachGroup)
                                {
                                    World.Instance.GetCharacter((int)guest).Teleport(map, character.Cell);
                                }
                            }

                            character.CurrentBreachRoom = extendedBreachBranch;

                            Task.Delay(3000).ContinueWith(t =>
                            {
                                var group = new MonsterGroup(map.GetNextContextualId(), new ObjectPosition(map, map.GetRandomFreeCell(), map.GetRandomDirection()));

                                foreach (var monster in extendedBreachBranch.monsters)
                                {
                                    MonsterGrade monsterGrade = MonsterManager.Instance.GetMonsterGrades()
                                        .Where(x => x.MonsterId == monster.genericId)
                                        .Where(x => x.GradeId == monster.grade)
                                        .First();

                                    group.AddMonster(new Monster(monsterGrade, group));
                                }

                                //revisar
                                MonsterGrade bossGrade = MonsterManager.Instance.GetMonsterGrades()
                                    .Where(x => x.MonsterId == extendedBreachBranch.bosses.ToArray()[0].genericId)
                                    .Where(x => x.GradeId == extendedBreachBranch.bosses.ToArray()[0].grade).First();

                                group.AddMonster(new Monster(bossGrade, group));

                                var BreachFight = Singleton<FightManager>.Instance.CreateBreachFight(character.Map, character);
                                BreachFight.ChallengersTeam.AddFighter(character.CreateFighter(BreachFight.ChallengersTeam));

                                if (character.BreachGroup != null)
                                {
                                    foreach (long guest in character.BreachGroup)
                                    {
                                        BreachFight.ChallengersTeam.AddFighter(World.Instance.GetCharacter((int)guest).CreateFighter(BreachFight.ChallengersTeam));
                                    }
                                }

                                foreach (var monster in group.GetMonsters())
                                {
                                    BreachFight.DefendersTeam.AddFighter(monster.CreateFighter(BreachFight.DefendersTeam));
                                }

                                BreachFight.StartPlacement();

                                ContextHandler.HandleGameFightJoinRequestMessage(character.Client, new GameFightJoinRequestMessage(character.Fighter.Id, (ushort)BreachFight.Id));
                                character.SaveLater();

                                if (character.BreachGroup != null)
                                {
                                    foreach (long guest in character.BreachGroup)
                                    {
                                        Character guestCharacter = World.Instance.GetCharacter((int)guest);
                                        ContextHandler.HandleGameFightJoinRequestMessage(guestCharacter.Client, new GameFightJoinRequestMessage(guestCharacter.Fighter.Id, (ushort)BreachFight.Id));
                                        guestCharacter.SaveLater();
                                    }
                                }

                            });
                        }
                        else
                        {
                            character.SendServerMessageLang
                                (
                                "Abra o globo dos sonhos para ativar o seu andar!",
                                "Open the dream globe to activate your floor!",
                                "Abre el globo de los sueños para activar tu suelo!",
                                "Ouvrez le globe de rêve pour activer votre sol !"
                                );
                        }
                    }
                    else
                    {
                        character.SendServerMessageLang
                            (
                            "Você terminou a carreira dos seus sonhos, por favor comece outra!",
                            "You've finished your dream career, please start another one!",
                            "Has terminado la carrera de tus sueños, ¡comienza otra!",
                            "Vous avez terminé la carrière de vos rêves, s'il vous plaît, commencez-en une autre !"
                            );
                    }
                }
                else
                {
                    character.SendServerMessageLang
                        (
                        "Você deve estar no nível 180 para acessar os sonhos!",
                        "You must be level 180 to access the dreams!",
                        "Debes tener el nivel 180 para acceder a los sueños!",
                        "Vous devez être niveau 180 pour accéder aux rêves !"
                        );
                }
            }
            else
            {
                character.SendServerMessageLang
                    (
                    "Você não é o dono desses sonhos!",
                    "You are not the owner of these dreams!",
                    "Tú no eres el dueño de estos sueños!",
                    "Vous n'êtes pas le propriétaire de ces rêves!"
                    );
            }

            return base.StartExecute(character);
        }
    }
}
