using BusVbot.Configurations;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace BusVbot.Services.Agency
{
    public class DefaultAgencyMessageFormatter : AgencyMessageFormatterBase, IDefaultAgencyMessageFormatter
    {
        public new string AgencyTag
        {
            get { return base.AgencyTag; }
            set { base.AgencyTag = value; }
        }

        public DefaultAgencyMessageFormatter(IOptions<AgencyTimeZonesAccessor> timezoneOptions)
            : base(null, timezoneOptions)
        {

        }
    }
}
