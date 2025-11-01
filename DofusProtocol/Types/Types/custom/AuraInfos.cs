

// Generated on 12/21/2022 00:00:00
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stump.Core.IO;

namespace Stump.DofusProtocol.Types
{
    public class AuraInfos
    {
        public const short Id = 8890;

        public uint id;
        public string hexColor;

        public virtual short TypeId
        {
            get { return Id; }
        }


        public AuraInfos()
        {
        }

        public AuraInfos(uint id, string hexColor)
        {
            this.id = id;
            this.hexColor = hexColor;
        }


        public virtual void Serialize(IDataWriter writer)
        {
            writer.WriteUInt(id);
            writer.WriteUTF(hexColor);
        }

        public virtual void Deserialize(IDataReader reader)
        {
            id = reader.ReadUInt();
            hexColor = reader.ReadUTF();
        }
    }
}