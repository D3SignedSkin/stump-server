//using System;
//using System.Linq;
//using Stump.DofusProtocol.D2oClasses;
//using Stump.DofusProtocol.Enums;
//using Stump.Server.WorldServer.Database.Items;
//using Stump.Server.WorldServer.Game.Actors.RolePlay;
//using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
//using Stump.Server.WorldServer.Game.Effects.Instances;
//using Stump.Server.WorldServer.Game.Maps.Cells;

//namespace Stump.Server.WorldServer.Game.Items.Player.Custom
//{
//    //[ItemHasEffect(EffectsEnum.Effect_148)]
//    public sealed class Follower : BasePlayerItem
//    {
//        public Follower(Character owner, PlayerItemRecord record)
//            : base(owner, record)
//        {
//            short type =(Effects.FirstOrDefault(x => x.EffectId == EffectsEnum.Effect_148) as EffectInteger).Value;//Dicenum IF are 2 are Monster
//            int follower = (Template.PossibleEffects.FirstOrDefault(x => x.EffectId == 148) as EffectInstanceDice).Value;
//            AddFollower(owner,type, follower);
//            Owner.Inventory.MoveItem(this, CharacterInventoryPositionEnum.INVENTORY_POSITION_FOLLOWER);
//            //Initialize();

//        }

//        public override uint Stack
//        {
//            get { return Math.Min(Record.Stack, 1); }
//            set { Record.Stack = Math.Min(value, 1); }
//        }
//        //private void Initialize()
//        //{
//        //    var integerEffect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

//        //    if (integerEffect == null)
//        //        return false;

//        //    if (Operation == HandlerOperation.APPLY)
//        //        AddFollower(Target);
//        //}
//        private void AddFollower(Character target,short type,int follower)
//        {
//            var position_char = target.Position.Clone();
//            var excludedCells = position_char.Map.GetActors<RolePlayActor>().Select(entry => entry.Cell.Id);
//            var position = new ObjectPosition(position_char.Map, position_char.Point.GetAdjacentCells(true).Where(x => x.IsInMap()).OrderBy(x => x.ManhattanDistanceTo(position_char.Point)).Where(x => position_char.Map.Cells[x.CellId].Walkable && !excludedCells.Contains(x.CellId)).FirstOrDefault().CellId, position_char.Direction); 


//            var followerActor = new FollowerActor(position.Map.GetNextContextualId(), position, target, type, follower);
//            target.following.Add(followerActor);
//            followerActor.Map.Enter(followerActor);

//        }

//    }
//}