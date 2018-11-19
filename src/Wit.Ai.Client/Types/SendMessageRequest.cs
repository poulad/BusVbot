using System.IO;
using System.Net.Http;

namespace Wit.Ai.Client.Types
{
    public class GetMessageRequest : IWitRequest<Meaning>
    {
        public string ContentType { get; }

        public Stream AudioStream { get; set; }

        public string MessageId { get; }

        public string ThreadId { get; }

        public int N { get; set; }

        public string Version { get; }

        public string AccessToken { get; }

        public Context Context { get; }

        public HttpRequestMessage GetHttpRequestMessage()
        {
            throw new System.NotImplementedException();
        }
    }
}
