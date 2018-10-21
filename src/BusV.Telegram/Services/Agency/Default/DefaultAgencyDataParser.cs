using BusV.Telegram.Models;

// ReSharper disable once CheckNamespace
namespace BusV.Telegram.Services.Agency
{
    public class DefaultAgencyDataParser : AgencyDataParserBase, IDefaultAgencyDataParser
    {
        public new string AgencyTag
        {
            get { return base.AgencyTag; }
            set { base.AgencyTag = value; }
        }

        public DefaultAgencyDataParser(BusVbotDbContext dbContext)
            : base(dbContext)
        {

        }
    }
}
