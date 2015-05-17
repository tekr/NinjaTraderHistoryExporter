using System;

namespace NinjaTraderHistoryExporter
{
    public class MarketDataBar
    {
        public DateTime DateTime { get; private set; }
        public decimal Open { get; private set; }
        public decimal High { get; private set; }
        public decimal Low { get; private set; }
        public decimal Close { get; private set; }
        public long Volume { get; private set; }

        public MarketDataBar(DateTime dateTime, decimal open, decimal high, decimal low, decimal close, long volume)
        {
            DateTime = dateTime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }

        public override string ToString()
        {
            return String.Format("{0:yyyy-MM-dd HH:mm} Open:{1:n2} High:{2:n2} Low:{3:n2} Close:{4:n2} Volume:{5:n0}", DateTime, Open, High, Low, Close, Volume);
        }
    }
}