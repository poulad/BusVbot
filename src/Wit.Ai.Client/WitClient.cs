using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Wit.Ai.Client
{
    public class WitClient : IWitClient
    {
        private readonly string _token;

        private readonly HttpClient _httpClient;

        public WitClient(
            string token,
            HttpClient httpClient = default
        )
        {
            _token = token ?? throw new ArgumentNullException(nameof(token));
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<string> SendAudioAsync(
            Stream audioStream,
            string contentType,
            CancellationToken cancellationToken = default
        )
        {
            string url = "https://api.wit.ai/speech?v=20170307";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StreamContent(audioStream) { Headers = { { "Content-Type", contentType } } },
            };
            httpRequest.Headers.TransferEncodingChunked = true;
            httpRequest.Headers.Add("Authorization", $"Bearer {_token}");

            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
//                if (cancellationToken.IsCancellationRequested)
                throw;
            }

            string responseJson;
            using (httpResponse)
            {
                responseJson = await httpResponse.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);
            }

            return responseJson;
        }
    }
}
