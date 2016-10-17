using System;
using System.Collections.Generic;
using System.IO;
namespace DataMatching
{
    public class StreamProcessor
    {
        private readonly Stream processingStream;

        public StreamProcessor(Stream processingStream)
        {
            this.processingStream = processingStream;
        }

        public IEnumerable<TradeDay> StartProcessing()
        {
            using (var reader = new StreamReader(processingStream))
            {
                string row = reader.ReadLine();
                while ((row = reader.ReadLine()) != null)
                {
                    var day = new TradeDay(row.Split(','));
                    yield return day;
                }
            }
        }

    }
}