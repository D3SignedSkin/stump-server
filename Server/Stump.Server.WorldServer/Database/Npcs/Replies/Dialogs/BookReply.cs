using System;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Dialogs.Book;
using Stump.Server.WorldServer.Game.Exchanges.Bank;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("BookReply", typeof(NpcReply), new Type[] { typeof(NpcReplyRecord) })]
    public class BookReply : NpcReply
    {
        public BookReply(NpcReplyRecord record)
          : base(record)
        {
        }

        public short DocumentId
        {
            get
            {
                return this.Record.GetParameter<short>(0U, false);
            }
            set
            {
                this.Record.SetParameter<short>(0U, value);
            }
        }

        public override bool Execute(Npc npc, Character character)
        {
            var bookDialog = new BookDialog(character, DocumentId);
            bookDialog.Open();

            return true;
        }
    }
}
