using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Database.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Characters
{
    public class CustomManager : DataManager<CustomManager>
    {
        readonly Dictionary<int, AuraColorsRecord> m_recorsAuras = new Dictionary<int, AuraColorsRecord>();





        public AuraColorsRecord GetAura(int id)
        {
            return m_recorsAuras.First((aura) => aura.Value.Id == id).Value;
        }

        public List<AuraInfos> GetAuras()
        {
            var auraInfos = new List<AuraInfos>();
            foreach (var aura in m_recorsAuras.Values)
            {
                auraInfos.Add(new AuraInfos((uint)aura.Id, aura.hexColor));
            }
            return auraInfos;
        }





        [Initialization(InitializationPass.Eleven)]
        public override void Initialize()
        {
            foreach (
                var record in Database.Query<AuraColorsRecord>(AuraColorsRelator.FetchQuery))
            {

                m_recorsAuras.Add((ushort)record.Id, record);

            }

        }

    }
}
