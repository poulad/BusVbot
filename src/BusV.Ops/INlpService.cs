using System.Threading;
using System.Threading.Tasks;
using Wit.Ai.Client.Types;

namespace BusV.Ops
{
    public interface INlpService
    {
        Task<Meaning> ProcessVoiceAsync(
            string filePath,
            string mimeType,
            CancellationToken cancellationToken
        );
    }
}
