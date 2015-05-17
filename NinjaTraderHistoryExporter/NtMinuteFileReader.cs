using System;
using System.Collections.Generic;
using System.IO;

namespace NinjaTraderHistoryExporter
{
    public class NtMinuteFileReader
    {
        private readonly uint[] _volumeMultipliers = { 100, 500, 1000 };

        public IEnumerable<MarketDataBar> Read(string symbol, Stream stream)
        {
            var reader = new BinaryReader(stream);
            var tickSize = -(decimal)reader.ReadDouble();

            // Don't know what this is yet, but seems to always be 2
            var unknown = reader.ReadInt32();
            if (unknown != 2)
            {
                Console.WriteLine("Unexpected value encountered: {0:X}. Export may fail.", unknown);
            }

            var numRecords = reader.ReadInt32();

            var bars = new List<MarketDataBar>();
            MarketDataBar lastMarketDataBar = null;

            if (numRecords > 0)
            {
                var open = reader.ReadDouble();
                var high = reader.ReadDouble();
                var low = reader.ReadDouble();
                var close = reader.ReadDouble();
                var dateTime = new DateTime(reader.ReadInt64());
                var volume = reader.ReadUInt64();

                bars.Add(lastMarketDataBar = new MarketDataBar(dateTime, (decimal)open, (decimal)high, (decimal)low, (decimal)close, (long)volume));
            }

            // Rest of file has big-endian ordering
            reader = new BigEndianBinaryReader(stream);

            for (var i = 1; i < numRecords; i++)
            {
                var format = reader.ReadUInt16();

                if ((format & 0x800c) != 0)
                {
                    Console.WriteLine("Unexpected format '{0:X}' following time '{1:yyyy-MM-dd HH:mm:ss}' on symbol {2}. It's likely all further data will be corrupt.",
                                    format, lastMarketDataBar.DateTime, symbol);
                }

                var dateTime = lastMarketDataBar.DateTime.AddMinutes(Math.Max(1, GetNumber(GetFormatBits(format, 8), false, reader)));

                var open = GetPrice(lastMarketDataBar.Open, GetFormatBits(format, 10), true, false, reader, tickSize);
                var high = GetPrice(open, GetFormatBits(format, 4), false, false, reader, tickSize);
                var low = GetPrice(open, GetFormatBits(format, 6), false, true, reader, tickSize);
                var close = GetPrice(low, GetFormatBits(format, 0), false, false, reader, tickSize);

                var volume = GetVolume(format, reader);

                bars.Add(lastMarketDataBar = new MarketDataBar(dateTime, open, high, low, close, volume));
            }

            return bars;
        }

        private decimal GetPrice(decimal referencePrice, int format, bool signed, bool invert, BinaryReader reader, decimal tickSize)
        {
            return referencePrice + (GetNumber(format, signed, reader) * tickSize) * (invert ? -1 : 1); 
        }

        private long GetNumber(int format, bool signed, BinaryReader reader)
        {
            switch (format)
            {
                case 0 :
                    return 0;
                case 1:
                    var val = reader.ReadByte();
                    return signed ? (long)(sbyte)(val ^ 0x80) : val;
                case 2:
                    return signed ? reader.ReadInt16() : (long)reader.ReadUInt16();
                case 3:
                    return signed ? reader.ReadInt32() : (long)reader.ReadUInt32();
                default:
                    throw new ArgumentException(string.Format("Number format {0:X} was not recognised", format));
            }
        }

        private long GetVolume(ushort format, BinaryReader reader)
        {
            long volume;
            var volumeFormat = (format >> 12) & 7;

            switch (volumeFormat)
            {
                case 1:
                case 6:
                case 7:
                    volume = (uint)GetNumber(GetFormatBits(format, 12), false, reader);
                    break;

                case 3:
                case 4:
                case 5:
                    volume = (uint)GetNumber(1, false, reader) * _volumeMultipliers[volumeFormat - 3];
                    break;

                case 2:
                    volume = reader.ReadInt64();
                    break;

                default:
                    throw new ArgumentException(string.Format("Unknown volume format {0:X} ", volumeFormat));
            }

            return volume;
        }

        private int GetFormatBits(int format, int bitsToShift)
        {
            return (format >> bitsToShift) & 3;
        }
    }
}
