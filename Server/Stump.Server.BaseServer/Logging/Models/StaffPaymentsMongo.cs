using MongoDB.Bson;
using System;

namespace Stump.Server.BaseServer.Logging.Models
{
    public class StaffPaymentsMongo
    {
        public ObjectId Id { get; set; }

        public String Name { get; set; }

        public String LastName { get; set; }

        public int AccountId { get; set; }

        public String StaffName { get; set; }

        public String Email { get; set; }

        public int Valor { get; set; }

        public DateTime Date { get; set; }
    }
}