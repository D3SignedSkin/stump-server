using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.D2oClasses;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Dialogs;
using Stump.Server.WorldServer.Handlers.Dialogs;

namespace Stump.Server.WorldServer.game.Dialogs.Breach
{
    public class BreachDialog : IDialog
    {
        public BreachDialog(Character character)
        {
            Character = character;
        }

        public Character Character { get; }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_DIALOG;

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void Open()
        {
            Character.ResetDialog();

            var random = new Random();
            var maps = new List<uint> { 191104002 };
            var map = World.Instance.GetMap(maps[new Random().Next(maps.Count())]);
            var branchMonsters = new List<List<int>>();
            var branchBosses = new List<int>();

            if (Character.BreachStep < 201)
            {
                for (var branches = 0; branches < 3; branches++)
                {
                    if (Character.BreachStep < 51)
                    {
                        var monsters = new List<int>();

                        branchBosses.Add(MonsterManager.Instance.GetMonsterGrades().Where(x => x.GradeId == MonsterManager.Instance.GetMonsterGradeByIdAndSelection(x.MonsterId, GradeSelection.Last)).Where(x => MonsterManager.Instance.GetTemplate(x.MonsterId).IsBoss).ToArray()[
                            random.Next(MonsterManager.Instance.GetMonsterGrades().Where(x => x.GradeId == MonsterManager.Instance.GetMonsterGradeByIdAndSelection(x.MonsterId, GradeSelection.Last)).Where(x => MonsterManager.Instance.GetTemplate(x.MonsterId).IsBoss).Count())].Id);

                        for (var i = 0; i < 3; i++)
                        {
                            monsters.Add(MonsterManager.Instance.GetMonsterGrades().Where(x => x.GradeId == MonsterManager.Instance.GetMonsterGradeByIdAndSelection(x.MonsterId, GradeSelection.Last)).Where(x => !MonsterManager.Instance.GetTemplate(x.MonsterId).IsBoss).ToArray()[
                                random.Next(MonsterManager.Instance.GetMonsterGrades().Where(x => x.GradeId == MonsterManager.Instance.GetMonsterGradeByIdAndSelection(x.MonsterId, GradeSelection.Last)).Where(x => !MonsterManager.Instance.GetTemplate(x.MonsterId).IsBoss).Count())].Id);
                        }

                        branchMonsters.Add(monsters);
                    }
                }
            }
            else
            {
                //Character.SetAscensionStair(0);
            }

            //ROOM: I // II // III etc dans le menu
            //MODIFIER: Modificateur de combat (Paradoxe/Rêve/Cauchemard)

            var monsterBranches = new List<MonsterInGroupLightInformations[]>();

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    monsterBranches.Add(new[] { new MonsterInGroupLightInformations(MonsterManager.Instance.GetMonsterGrade(branchMonsters[i][j]).Template.Id, 1, 1) });
                }
            }

            var bossBranches = new List<MonsterInGroupLightInformations[]>();

            bossBranches.Add(new[]
            {
                new MonsterInGroupLightInformations(MonsterManager.Instance.GetMonsterGrade(branchBosses[0]).Template.Id, 1, 1)
            });
            bossBranches.Add(new[]
            {
                new MonsterInGroupLightInformations(MonsterManager.Instance.GetMonsterGrade(branchBosses[1]).Template.Id, 1, 1)
            });
            bossBranches.Add(new[]
            {
                new MonsterInGroupLightInformations(MonsterManager.Instance.GetMonsterGrade(branchBosses[2]).Template.Id, 1, 1)
            });

            BreachReward[] breachRewards =
            {
                new BreachReward(93, new sbyte[0], "", 0, 100), //VITALITY
                new BreachReward(94, new sbyte[0], "", 0, 100), //POWER
                new BreachReward(92, new sbyte[0], "", 0, 100), //STRENGH
                new BreachReward(6, new sbyte[0], "", 0, 100), //INTELLIGENCY
                new BreachReward(91, new sbyte[0], "", 0, 100), //AGILITY
                new BreachReward(7, new sbyte[0], "", 0, 100), //CHANCE
            };

            ExtendedBreachBranch[] extendedBreachBranchs =
            {
                //new ExtendedBreachBranch(1, 1, bossBranches[0], 188486931, monsterBranches[0], breachRewards, (uint)random.Next(155, 547), 100),

                //new ExtendedBreachBranch(2, 2, bossBranches[1], 188486931, monsterBranches[1], breachRewards, (uint)random.Next(155, 547), 100),

                //new ExtendedBreachBranch(3, 3, bossBranches[2], 188486931, monsterBranches[2], breachRewards, (uint)random.Next(155, 547), 100)
            };

            Character.Client.Send(new BreachBranchesMessage(extendedBreachBranchs));
        }
    }
}