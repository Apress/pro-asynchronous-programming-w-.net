using System;

namespace LazyCreation
{

    public delegate bool TryParse<T>(string toParse, out T val);

    public class WeatherEntry
    {
        public WeatherEntry(string[] csvRow)
        {
            if (csvRow.Length != 7) return;

            When = new DateTime(int.Parse(csvRow[0]), int.Parse(csvRow[1]), 1);
            MaxTemperature = SafeParse<decimal>(csvRow[2], decimal.TryParse);
            MinTemperature = SafeParse<decimal>(csvRow[3], decimal.TryParse);
            FrostDays = SafeParse<int>(csvRow[4], int.TryParse);
            RainFall = SafeParse<decimal>(csvRow[5], decimal.TryParse);
            HoursOfSunshine = SafeParse<decimal>(csvRow[6], decimal.TryParse);
        }

        public DateTime When { get; set; }
        public decimal? MaxTemperature { get; set; }
        public decimal? MinTemperature { get; set; }
        public int? FrostDays { get; set; }
        public decimal? RainFall { get; set; }
        public decimal? HoursOfSunshine { get; set; }

        public static T? SafeParse<T>(string toParse, TryParse<T> tryParse) where T : struct
        {
            T? val = null;
            T parsedValue;

            if (tryParse(toParse, out parsedValue))
            {
                val = parsedValue;
            }

            return val;
        }
        public override string ToString()
        {
            return string.Format("When: {0}, MaxTemperature: {1}, MinTemperature: {2}, FrostDays: {3}, RainFall: {4}, HoursOfSunshine: {5}", When, MaxTemperature, MinTemperature, FrostDays, RainFall, HoursOfSunshine);
        }
    }
}