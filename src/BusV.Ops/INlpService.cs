using System.Threading;
using System.Threading.Tasks;

namespace BusV.Ops
{
    public interface INlpService
    {
        Task<string> ProcessVoiceAsync(
            string filePath,
            string mimeType,
            CancellationToken cancellationToken
        );
    }
}
