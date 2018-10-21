using System;
using System.Collections.Generic;
using System.Linq;

namespace BusV.Telegram.Services.Agency
{
    public class AgencyServiceAccessor : IAgencyServiceAccessor
    {
        public IDefaultAgencyDataParser DefaultDataParser { get; }

        public IEnumerable<IAgencyDataParser> DataParsers { get; }

        public IDefaultAgencyMessageFormatter DefaultMessageFormatter { get; }

        public IEnumerable<IAgencyMessageFormatter> MessageFormatters { get; }

        public AgencyServiceAccessor(
            IDefaultAgencyDataParser defaultParser,
            IDefaultAgencyMessageFormatter defaultFormatter,
            IEnumerable<IAgencyDataParser> dataParsers,
            IEnumerable<IAgencyMessageFormatter> messageFormatters)
        {
            DefaultDataParser = defaultParser;
            DataParsers = dataParsers;

            DefaultMessageFormatter = defaultFormatter;
            MessageFormatters = messageFormatters;
        }

        public IAgencyDataParser GetAgencyOrDefaultDataParser(string agencyTag)
        {
            IAgencyDataParser dataParser = DataParsers
                .SingleOrDefault(p => p.AgencyTag.Equals(agencyTag, StringComparison.OrdinalIgnoreCase));

            if (dataParser == null)
            {
                DefaultDataParser.AgencyTag = agencyTag;
                dataParser = DefaultDataParser;
            }

            return dataParser;
        }

        public IAgencyMessageFormatter GetAgencyOrDefaultMessageFormatter(string agencyTag)
        {
            IAgencyMessageFormatter msgFormatter = MessageFormatters
                .SingleOrDefault(p => p.AgencyTag.Equals(agencyTag, StringComparison.OrdinalIgnoreCase));

            if (msgFormatter == null)
            {
                DefaultMessageFormatter.AgencyTag = agencyTag;
                msgFormatter = DefaultMessageFormatter;
            }

            return msgFormatter;
        }
    }
}
