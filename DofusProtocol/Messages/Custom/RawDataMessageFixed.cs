using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Core.IO;
using Stump.DofusProtocol.Types;

namespace Stump.DofusProtocol.Messages.Custom
{
    [OverrideMessage]
    public class RawDataMessageFixed : Message
    {
        public const uint Id = 3396;
        public byte[] content;
        public override uint MessageId
        {
            get { return Id; }
        }

        public RawDataMessageFixed()
        {

        }

        public RawDataMessageFixed(byte[] _content)
        {
            content = _content;
        }

        public override void Serialize(IDataWriter writer)
        {
            writer.WriteVarInt(content.Length);
            writer.WriteBytes(content);
        }

        public override void Deserialize(IDataReader reader)
        {
            var len = reader.ReadVarInt();
            content = reader.ReadBytes(len);
        }
    }
}