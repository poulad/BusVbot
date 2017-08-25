using System.Collections.Generic;

namespace BusVbot.Services.Agency
{
    public interface IAgencyServiceAccessor
    {
        IDefaultAgencyDataParser DefaultDataParser { get; }

        IEnumerable<IAgencyDataParser> DataParsers { get; }

        IDefaultAgencyMessageFormatter DefaultMessageFormatter { get; }

        IEnumerable<IAgencyMessageFormatter> MessageFormatters { get; }

        IAgencyDataParser GetAgencyOrDefaultDataParser(string agencyTag);

        IAgencyMessageFormatter GetAgencyOrDefaultMessageFormatter(string agencyTag);
    }
}
