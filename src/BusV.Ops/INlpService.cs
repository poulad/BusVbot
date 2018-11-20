using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wit.Ai.Client.Types;

namespace BusV.Ops
{
    public interface INlpService
    {
        Task<Meaning> ProcessTextAsync(
            string text,
            CancellationToken cancellationToken
        );

        Task<Meaning> ProcessVoiceAsync(
            Stream audioStream,
            string mimeType,
            CancellationToken cancellationToken
        );
    }
}
