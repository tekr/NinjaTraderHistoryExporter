using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NinjaTraderHistoryExporter
{
    class BigEndianBinaryReader : BinaryReader
    {
        public BigEndianBinaryReader(Stream input) : base(input)
        {
        }

        public override short ReadInt16()
        {
            return BitConverter.ToInt16(GetArray(typeof(Int16)), 0);
        }

        public override int ReadInt32()
        {
            return BitConverter.ToInt32(GetArray(typeof(Int32)), 0);
        }

        public override long ReadInt64()
        {
            return BitConverter.ToInt64(GetArray(typeof(Int64)), 0);
        }

        public override ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(GetArray(typeof(UInt16)), 0);
        }

        public override uint ReadUInt32()
        {
            return BitConverter.ToUInt32(GetArray(typeof(UInt32)), 0);
        }

        public override ulong ReadUInt64()
        {
            return BitConverter.ToUInt64(GetArray(typeof(UInt64)), 0);
        }

        private byte[] GetArray(Type type)
        {
            var array = ReadBytes(Marshal.SizeOf(type));

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }

            return array;
        }
    }
}
