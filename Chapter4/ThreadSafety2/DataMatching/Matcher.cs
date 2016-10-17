using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataMatching
{
    public class Matcher
    {
        private readonly string dataSource;
        private readonly Action<string, IEnumerable<TradeDay>> matchesFound;
        private readonly ManualResetEventSlim controlFileAvailable;
        private readonly CountdownEvent initializeCompleteEvent;

        public Matcher(string dataSource, Action<string, IEnumerable<TradeDay>> matchesFound, ManualResetEventSlim controlFileAvailable, CountdownEvent initializeCompleteEvent)
        {
            this.dataSource = dataSource;
            this.matchesFound = matchesFound;
            this.controlFileAvailable = controlFileAvailable;
            this.initializeCompleteEvent = initializeCompleteEvent;
        }

        public Task Process()
        {
            return Task.Run((Action)InternalProcess);
        }

        private void InternalProcess()
        {
            IEnumerable<TradeDay> days = Initialize();

            initializeCompleteEvent.Signal();

            controlFileAvailable.Wait();

            ControlParameters parameters = GetControlParameters();
            IEnumerable<TradeDay> matchingDays = null;
            if (parameters != null)
            {
                matchingDays = from d in days
                                   where d.Date >= parameters.FromDate &&
                                         d.Date <= parameters.ToDate &&
                                         d.Volume >= parameters.Volume
                                   select d;
            }
            matchesFound(dataSource, matchingDays);
        }

        private IEnumerable<TradeDay> Initialize()
        {
            List<TradeDay> days = null;
            using (FileStream file = File.OpenRead(string.Format(@"..\..\\{0}.csv", dataSource)))
            {
                var streamProcessor = new StreamProcessor(file);

                days = streamProcessor.StartProcessing().ToList();
            }
            return days;
        }

        private ControlParameters GetControlParameters()
        {
            XElement controlDataRoot = XElement.Load(@"..\..\control.xml");

            XElement controlData = controlDataRoot.Element(dataSource);

            return controlData == null
                       ? null
                       : new ControlParameters
                           {
                               FromDate = (DateTime) controlData.Element("from"),
                               ToDate = (DateTime) controlData.Element("to"),
                               Volume = (long) controlData.Element("volume"),
                           };
        }
    }

    internal class ControlParameters
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long Volume { get; set; }
    }
}