using System.Threading;
using System.Threading.Tasks;

namespace BusV.Ops
{
    /// <summary>
    /// Contains operations to update the data store using the NextBus API
    /// </summary>
    public interface IDataSeeder
    {
        Task UpdateAllAgenciesAsync(
            CancellationToken cancellationToken = default
        );

        Task UpdateRoutesForAgencyAsync(
            string agencyTag,
            CancellationToken cancellationToken = default
        );
    }
}
