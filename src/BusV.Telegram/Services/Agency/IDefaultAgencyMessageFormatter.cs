namespace BusV.Telegram.Services.Agency
{
    public interface IDefaultAgencyMessageFormatter : IAgencyMessageFormatter
    {
        new string AgencyTag { get; set; }
    }
}
