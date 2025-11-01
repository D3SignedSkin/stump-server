using System.Collections.Generic;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Payments;

namespace Stump.Server.WorldServer.Game.Lottery
{
    class StaffPaymentsManager : DataManager<StaffPaymentsManager>
    {
        private readonly Dictionary<int, StaffPayments> m_payments = new Dictionary<int, StaffPayments>();

        [Initialization(InitializationPass.Any)]
        public override void Initialize()
        {
            foreach (var payments in Database.Query<StaffPayments>(StaffPaymentsTableRelator.FetchQuery))
            {
                m_payments.Add(payments.Id, payments);
            }
        }

        public Dictionary<int, StaffPayments> GetPayments()
        {
            return m_payments;
        }
    }
}
