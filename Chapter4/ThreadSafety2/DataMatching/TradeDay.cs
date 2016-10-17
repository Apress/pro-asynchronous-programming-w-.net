using System;
using System.Globalization;

namespace DataMatching
{
    public class TradeDay
    {
        public TradeDay(string[] fields)
        {
            Date = DateTime.Parse(fields[0]);
            Open = decimal.Parse(fields[1]);
            High = decimal.Parse(fields[2]);
            Low = decimal.Parse(fields[3]);
            Close = decimal.Parse(fields[4]);
            Volume = long.Parse(fields[5]);
            AdjustedClose = decimal.Parse(fields[6]);
        }
        public DateTime Date { get; private set; }
        public decimal Open { get; private set; }
        public decimal Close { get; private set; }
        public decimal High { get; private set; }
        public decimal Low { get; private set; }
        public decimal AdjustedClose { get; private set; }
        public long Volume { get; private set; }
    }
}