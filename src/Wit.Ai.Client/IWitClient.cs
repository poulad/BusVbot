using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Wit.Ai.Client
{
    public interface IWitClient
    {
        Task<string> SendAudioAsync(
            Stream audioStream,
            string contentType,
            CancellationToken cancellationToken = default
        );
    }
}
