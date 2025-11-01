using Database;
using Stump.Server.WorldServer;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.UtilisationTokens
{
    class UtilisationTokenManager
    {
        private UtilisationTokenRecord record;

        public UtilisationTokenManager(Character character, string transactionType = "", int itemId = 0, int itemStack = 0, int tokenAvantAchat = 0, int tokenApreAchat = 0, int token = 0)
        {
            try
            {
                string tokenName = "Ogrina";

                if (character != null)
                {
                    if (token > 0)
                    {
                        if (token == 10275)
                            tokenName = "Chapa";
                        else if (token == 12736)
                            tokenName = "Kolificha";
                    }
                    else if (tokenApreAchat == 0)
                    {
                        tokenApreAchat = (int)(character.Inventory.Tokens?.Stack ?? 0);
                    }

                    var record = new UtilisationTokenRecord
                    {
                        OwnerName = character.Name,
                        OwnerId = character.Id,
                        Time = DateTime.Now,
                        TransactionType = transactionType,
                        itemId = itemId,
                        ItemStack = itemStack,
                        TokenAvantAchat = tokenAvantAchat,
                        TokenApreAchat = tokenApreAchat,
                        TokenName = tokenName
                    };

                    WorldServer.Instance.DBAccessor.Database.Insert(record);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UtilisationTokenManager: {ex.Message}");
            }
        }
    }
}