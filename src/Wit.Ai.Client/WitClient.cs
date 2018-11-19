using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Wit.Ai.Client.Types;

namespace Wit.Ai.Client
{
    public class WitClient : IWitClient
    {
        public const string ApiVersion = "20170307";

        private const string BaseUrl = "https://api.wit.ai";

        private readonly string _token;

        private readonly HttpClient _httpClient;

        public WitClient(
            string token,
            HttpClient httpClient = default
        )
        {
            _token = token ?? throw new ArgumentNullException(nameof(token));
            _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(
            IWitRequest<TResponse> request,
            CancellationToken cancellationToken = default
        )
        {
            var httpRequest = request.GetHttpRequestMessage();

            bool hasQueryString = httpRequest.RequestUri.OriginalString.Contains("?");
            if (hasQueryString)
            {
                bool hasVersionParam = httpRequest.RequestUri.OriginalString.Contains("v=");
                if (!hasVersionParam)
                {
                    var absoluteUrl = new Uri($"{BaseUrl}{httpRequest.RequestUri}", UriKind.Absolute);
                    var query = HttpUtility.ParseQueryString(absoluteUrl.Query);
                    query["v"] = ApiVersion;

                    string relativeUrl = httpRequest.RequestUri.OriginalString.Substring(
                        0, httpRequest.RequestUri.OriginalString.IndexOf("?", StringComparison.Ordinal)
                    );
                    httpRequest.RequestUri = new Uri($"{relativeUrl}?{query}", UriKind.Relative);
                }
            }
            else
            {
                httpRequest.RequestUri = new Uri($"{httpRequest.RequestUri.OriginalString}?v={ApiVersion}");
            }

            httpRequest.Headers.Add("Accept", "application/json");
            if (request.AccessToken == null) httpRequest.Headers.Add("Authorization", $"Bearer {_token}");

            var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken)
                .ConfigureAwait(false);

            string responseText;
            using (httpResponse)
            {
                responseText = await httpResponse.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);
            }

            TResponse response;
            if (httpResponse.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<TResponse>(responseText);
            }
            else if (responseText.Trim().StartsWith("{"))
            {
                var error = JsonConvert.DeserializeObject<WitError>(responseText);
                throw new Exception(error.ToString());
            }
            else
            {
                throw new Exception(responseText);
            }

            return response;
        }

        public Task<Meaning> SendAudioAsync(
            Stream audioStream,
            string contentType,
            Context context = default,
            string messageId = default,
            string threadId = default,
            int n = default,
            CancellationToken cancellationToken = default
        ) =>
            SendRequestAsync(new SendSpeechRequest
            {
                AudioStream = audioStream,
                ContentType = contentType,
                Context = context,
                MessageId = messageId,
                ThreadId = threadId,
                N = n,
            }, cancellationToken);
    }
}
