using System.Net.Http;

namespace Wit.Ai.Client.Types
{
    public interface IWitRequest<TResponse>
    {
        string Version { get; }

        string AccessToken { get; }

        Context Context { get; }

        HttpRequestMessage GetHttpRequestMessage();
    }
}
