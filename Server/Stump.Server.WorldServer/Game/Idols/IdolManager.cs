using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Idols;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Idols
{
    public class IdolManager : DataManager<IdolManager>
    {
        private Dictionary<int, IdolTemplate> m_idolTemplates;

        [Initialization(InitializationPass.Fourth)]
        public override void Initialize()
        {
            #region >> Limpando as variaveis para que o comando Reload não duplique elas.

            if (m_idolTemplates != null)
                m_idolTemplates.Clear();

            #endregion

            m_idolTemplates = Database.Query<IdolTemplate>(IdolTemplateRelator.FetchQuery).ToDictionary(entry => entry.Id);
        }

        public IdolTemplate[] GetTemplates()
        {
            return m_idolTemplates.Values.ToArray();
        }

        public IdolTemplate GetTemplate(int id)
        {
            IdolTemplate result;
            return !m_idolTemplates.TryGetValue(id, out result) ? null : result;
        }

        public IdolTemplate GetTemplateByItemId(int itemId)
        {
            return m_idolTemplates.FirstOrDefault(x => x.Value.IdolItemId == itemId).Value;
        }
    }
}