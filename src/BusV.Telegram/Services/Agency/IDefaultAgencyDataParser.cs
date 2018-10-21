namespace BusVbot.Services.Agency
{
    public interface IDefaultAgencyDataParser : IAgencyDataParser
    {
        new string AgencyTag { get; set; }
    }
}
