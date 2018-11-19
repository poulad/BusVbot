using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;

namespace Wit.Ai.Client.Types
{
    public class SendSpeechRequest : IWitRequest<Meaning>
    {
        public string ContentType { get; set; }

        public Stream AudioStream { get; set; }

        public string MessageId { get; set; }

        public string ThreadId { get; set; }

        public int N { get; set; }

        public string Version { get; set; }

        public string AccessToken { get; set; }

        public Context Context { get; set; }

        public HttpRequestMessage GetHttpRequestMessage()
        {
            string url = "/speech";
            {
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                if (Version != null) queryString["v"] = Version;
                if (Context != null) queryString["context"] = JsonConvert.SerializeObject(Context);
                if (MessageId != null) queryString["msg_id"] = MessageId;
                if (ThreadId != null) queryString["thread_id"] = ThreadId;
                if (N != 0) queryString["n"] = N.ToString();

                if (queryString.HasKeys())
                    url += $"?{queryString}";
            }

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StreamContent(AudioStream),
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);

            if (AccessToken != null) request.Headers.Add("Authorization", $"Bearer {AccessToken}");
            request.Headers.TransferEncodingChunked = true;

            return request;
        }
    }
}
