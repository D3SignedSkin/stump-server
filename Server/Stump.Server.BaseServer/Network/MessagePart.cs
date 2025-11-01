//using System;
//using Stump.Core.IO;

//namespace Stump.Server.BaseServer.Network
//{
//    public class MessagePart
//    {
//        private byte[] m_data;
//        private byte[] m_completeData;

//        public bool IsValid
//        {
//            get
//            {
//                int num;
//                if (this.Header.HasValue)
//                {
//                    int? length1 = this.Length;
//                    if (length1.HasValue)
//                    {
//                        length1 = this.Length;
//                        int length2 = this.Data.Length;
//                        num = length1.GetValueOrDefault() == length2 ? (length1.HasValue ? 1 : 0) : 0;
//                        goto label_4;
//                    }
//                }
//                num = 0;
//                label_4:
//                return num != 0;
//            }
//        }

//        public int? Header { get; private set; }

//        public int? MessageId
//        {
//            get
//            {
//                if (!this.Header.HasValue)
//                    return new int?();
//                int? header = this.Header;
//                return header.HasValue ? new int?(header.GetValueOrDefault() >> 2) : new int?();
//            }
//        }

//        public int? LengthBytesCount
//        {
//            get
//            {
//                if (!this.Header.HasValue)
//                    return new int?();
//                int? header = this.Header;
//                return header.HasValue ? new int?(header.GetValueOrDefault() & 3) : new int?();
//            }
//        }

//        public int? Length { get; private set; }

//        public byte[] Data
//        {
//            get
//            {
//                return this.m_data;
//            }
//            private set
//            {
//                this.m_data = value;
//            }
//        }

//        public byte[] CompleteData
//        {
//            get
//            {
//                return this.m_completeData;
//            }
//            private set
//            {
//                this.m_completeData = value;
//            }
//        }

//        public bool Build(BigEndianReader reader)
//        {
//            if (this.IsValid)
//                return true;
//            this.m_completeData = reader.Data;
//            int? nullable1;
//            int num1;
//            if (reader.BytesAvailable >= 2L)
//            {
//                nullable1 = this.Header;
//                num1 = !nullable1.HasValue ? 1 : 0;
//            }
//            else
//                num1 = 0;
//            if (num1 != 0)
//                this.Header = new int?((int)reader.ReadShort());
//            reader.ReadUInt();
//            nullable1 = this.LengthBytesCount;
//            long? nullable2;
//            int num2;
//            if (nullable1.HasValue)
//            {
//                long bytesAvailable = reader.BytesAvailable;
//                nullable1 = this.LengthBytesCount;
//                nullable2 = nullable1.HasValue ? new long?((long)nullable1.GetValueOrDefault()) : new long?();
//                long valueOrDefault = nullable2.GetValueOrDefault();
//                if ((bytesAvailable >= valueOrDefault ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
//                {
//                    nullable1 = this.Length;
//                    num2 = !nullable1.HasValue ? 1 : 0;
//                    goto label_11;
//                }
//            }
//            num2 = 0;
//            label_11:
//            if (num2 != 0)
//            {
//                nullable1 = this.LengthBytesCount;
//                int num3 = 0;
//                int num4;
//                if ((nullable1.GetValueOrDefault() < num3 ? (nullable1.HasValue ? 1 : 0) : 0) == 0)
//                {
//                    nullable1 = this.LengthBytesCount;
//                    int num5 = 3;
//                    num4 = nullable1.GetValueOrDefault() > num5 ? (nullable1.HasValue ? 1 : 0) : 0;
//                }
//                else
//                    num4 = 1;
//                if (num4 != 0)
//                    throw new Exception("Malformated Message Header, invalid bytes number to read message length (inferior to 0 or superior to 3)");
//                this.Length = new int?(0);
//                nullable1 = this.LengthBytesCount;
//                for (int index = nullable1.Value - 1; index >= 0; --index)
//                {
//                    nullable1 = this.Length;
//                    int num5 = (int)reader.ReadByte() << index * 8;
//                    this.Length = nullable1.HasValue ? new int?(nullable1.GetValueOrDefault() | num5) : new int?();
//                }
//            }
//            int num6;
//            if (this.Data == null)
//            {
//                nullable1 = this.Length;
//                num6 = nullable1.HasValue ? 1 : 0;
//            }
//            else
//                num6 = 0;
//            if (num6 != 0)
//            {
//                nullable1 = this.Length;
//                int num3 = 0;
//                if (nullable1.GetValueOrDefault() == num3 && nullable1.HasValue)
//                    this.Data = new byte[0];
//                long bytesAvailable1 = reader.BytesAvailable;
//                nullable1 = this.Length;
//                nullable2 = nullable1.HasValue ? new long?((long)nullable1.GetValueOrDefault()) : new long?();
//                long valueOrDefault = nullable2.GetValueOrDefault();
//                if (bytesAvailable1 >= valueOrDefault && nullable2.HasValue)
//                {
//                    IDataReader bigEndianReader = reader;
//                    nullable1 = this.Length;
//                    int n = nullable1.Value;
//                    this.Data = bigEndianReader.ReadBytes(n);
//                }
//                else
//                {
//                    nullable1 = this.Length;
//                    nullable2 = nullable1.HasValue ? new long?((long)nullable1.GetValueOrDefault()) : new long?();
//                    long bytesAvailable2 = reader.BytesAvailable;
//                    if (nullable2.GetValueOrDefault() > bytesAvailable2 && nullable2.HasValue)
//                        this.Data = reader.ReadBytes((int)reader.BytesAvailable);
//                }
//            }
//            int num7;
//            if (this.Data != null)
//            {
//                nullable1 = this.Length;
//                if (nullable1.HasValue)
//                {
//                    int length = this.Data.Length;
//                    nullable1 = this.Length;
//                    int valueOrDefault = nullable1.GetValueOrDefault();
//                    num7 = length < valueOrDefault ? (nullable1.HasValue ? 1 : 0) : 0;
//                    goto label_35;
//                }
//            }
//            num7 = 0;
//            label_35:
//            if (num7 != 0)
//            {
//                int num3 = 0;
//                long num4 = (long)this.Data.Length + reader.BytesAvailable;
//                nullable1 = this.Length;
//                nullable2 = nullable1.HasValue ? new long?((long)nullable1.GetValueOrDefault()) : new long?();
//                long valueOrDefault1 = nullable2.GetValueOrDefault();
//                if (num4 < valueOrDefault1 && nullable2.HasValue)
//                {
//                    num3 = (int)reader.BytesAvailable;
//                }
//                else
//                {
//                    long num5 = (long)this.Data.Length + reader.BytesAvailable;
//                    nullable1 = this.Length;
//                    nullable2 = nullable1.HasValue ? new long?((long)nullable1.GetValueOrDefault()) : new long?();
//                    long valueOrDefault2 = nullable2.GetValueOrDefault();
//                    if (num5 >= valueOrDefault2 && nullable2.HasValue)
//                    {
//                        nullable1 = this.Length;
//                        num3 = nullable1.Value - this.Data.Length;
//                    }
//                }
//                if ((uint)num3 > 0U)
//                {
//                    int length = this.Data.Length;
//                    Array.Resize<byte>(ref this.m_data, this.Data.Length + num3);
//                    Array.Copy((Array)reader.ReadBytes(num3), 0, (Array)this.Data, length, num3);
//                }
//            }
//            return this.IsValid;
//        }
//    }
//}
using System;
using Stump.Core.IO;

namespace Stump.Server.BaseServer.Network
{
    public class MessagePart
    {
        private readonly bool m_readData;
        private long m_availableBytes;

        public MessagePart(bool readData)
        {
            m_readData = readData;
        }

        private bool m_dataMissing;

        /// <summary>
        /// Set to true when the message is whole
        /// </summary>
        public bool IsValid
        {
            get
            {
                return Header.HasValue && Length.HasValue && (!ReadData || Data != null) &&
                    Length <= (ReadData ? Data.Length : m_availableBytes);
            }
        }

        public int? Header
        {
            get;
            private set;
        }

        public int? MessageId
        {
            get
            {
                if (!Header.HasValue)
                    return null;

                return Header >> 2; // xxxx xx??
            }
        }

        public int? LengthBytesCount
        {
            get
            {
                if (!Header.HasValue)
                    return null;

                return Header & 0x3; // ???? ??xx
            }
        }

        public int? Length
        {
            get;
            private set;
        }

        private byte[] m_data;

        /// <summary>
        /// Set only if ReadData or ExceedBufferSize is true
        /// </summary>
        public byte[] Data
        {
            get { return m_data; }
            private set { m_data = value; }
        }

        public bool ReadData
        {
            get { return m_readData; }
        }

        /// <summary>
        /// Build or continue building the message. Returns true if the resulted message is valid and ready to be parsed
        /// </summary>
        public bool Build(IDataReader reader)
        {
            if (reader.BytesAvailable <= 0)
                return false;

            if (IsValid)
                return true;

            if (!Header.HasValue && reader.BytesAvailable < 2)
                return false;

            if (reader.BytesAvailable >= 2 && !Header.HasValue)
            {
                Header = reader.ReadUShort();
            }

            reader.ReadUInt(); // sequence id but who cares ?

            if (LengthBytesCount.HasValue &&
                reader.BytesAvailable >= LengthBytesCount && !Length.HasValue)
            {
                if (LengthBytesCount < 0 || LengthBytesCount > 3)
                    throw new Exception("Malformated Message Header, invalid bytes number to read message length (inferior to 0 or superior to 3)");

                Length = 0;

                // 3..0 or 2..0 or 1..0
                for (var i = LengthBytesCount.Value - 1; i >= 0; i--)
                {
                    Length |= reader.ReadByte() << (i * 8);
                }
            }

            // first case : no data read
            if (Length.HasValue && !m_dataMissing)
            {
                if (Length == 0)
                {
                    if (ReadData)
                        Data = new byte[0];
                    return true;
                }

                // enough bytes in the buffer to build a complete message
                if (reader.BytesAvailable >= Length)
                {
                    if (ReadData)
                        Data = reader.ReadBytes(Length.Value);
                    else
                        m_availableBytes = reader.BytesAvailable;

                    return true;
                }

                // not enough bytes, so we read what we can
                if (!(Length > reader.BytesAvailable))
                    return IsValid;

                if (ReadData)
                    Data = reader.ReadBytes((int)reader.BytesAvailable);
                else
                    m_availableBytes = reader.BytesAvailable;

                m_dataMissing = true;
                return false;
            }

            //second case : the message was split and it missed some bytes
            if (!Length.HasValue || !m_dataMissing)
                return IsValid;

            // still miss some bytes ...
            if ((ReadData ? Data.Length : 0) + reader.BytesAvailable < Length)
            {
                if (ReadData)
                {
                    var lastLength = m_data.Length;
                    Array.Resize(ref m_data, (int)(Data.Length + reader.BytesAvailable));
                    var array = reader.ReadBytes((int)reader.BytesAvailable);

                    Array.Copy(array, 0, Data, lastLength, array.Length);
                }
                else
                    m_availableBytes = reader.BytesAvailable;

                m_dataMissing = true;
            }

            // there is enough bytes in the buffer to complete the message :)
            if (!((ReadData ? Data.Length : 0) + reader.BytesAvailable >= Length))
                return IsValid;

            if (ReadData)
            {
                var bytesToRead = Length.Value - Data.Length;

                Array.Resize(ref m_data, Data.Length + bytesToRead);
                var array = reader.ReadBytes(bytesToRead);

                Array.Copy(array, 0, Data, Data.Length - bytesToRead, bytesToRead);
            }
            else
                m_availableBytes = reader.BytesAvailable;

            return IsValid;
        }
    }
}