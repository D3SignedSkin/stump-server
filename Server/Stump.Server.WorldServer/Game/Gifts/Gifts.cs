using NLog;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.I18n;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.Startup;

namespace Stump.Server.WorldServer.Game.Gifts
{
    public class Gifts : DataManager<Gifts>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Adicionar um Presente Offline/Online
        public void SetGift(int accountId, string title, ItemTemplate item, uint amount, bool isOgrines = false, bool isMax = false, bool isLinkedAccount = false, bool isLinkedCharacter = false, bool isActive = false, string Sender = null)
        {
            if (item is null)
                return;

            var itemName = TextManager.Instance.GetText(item.NameId) ?? string.Empty;

            if (item.Id == Settings.TokenTemplateId)
                itemName = "Token Ogrines";

            //Crear datos relacionados con la descripción de la tarea
            var startupActionRecord = new StartupActionRecord
            {
                Title = title,
                Text = itemName,
                DescUrl = "",
                PictureUrl = "http://serverhydra.com/forge/image2/0/0/0/dofus/www/game/items/200/" + item.IconId + ".png"
            };
            World.Instance.Database.Insert(startupActionRecord);

            //Registrar el enlace de regalo y otras tablas
            var startupActionRecordItems = new StartupActionRecordItems
            {
                Id = startupActionRecord.Id,
                OwnerId = accountId,
                StartupActionId = (uint)startupActionRecord.Id,
                Active = isActive,
                GiftSenderName = Sender != null ? Sender : "Empty"
            };
            World.Instance.Database.Insert(startupActionRecordItems);

            //Registrar los artículos que se entregarán con la función anterior
            var startupActionItemRecord = new StartupActionItemRecord
            {
                ActionId = (uint)startupActionRecord.Id,
                ItemTemplate = item.Id,
                Amount = amount,
                Ogrines = isOgrines,
                MaxEffects = isMax,
                LinkedAccount = isLinkedAccount,
                LinkedCharacter = isLinkedCharacter
            };

            World.Instance.Database.Insert(startupActionItemRecord);
        }
        #endregion
    }
}