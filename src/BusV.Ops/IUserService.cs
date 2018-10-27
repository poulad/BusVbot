using System.Threading;
using System.Threading.Tasks;

namespace BusV.Ops
{
    public interface IUserService
    {
        Task<Error> SetDefaultAgencyAsync(
            string user,
            string chat,
            string agencyTag,
            CancellationToken cancellationToken = default
        );
    }
}
