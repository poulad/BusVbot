using System;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;

namespace Wit.Ai.Client.Types
{
    public class GetMessageRequest : IWitRequest<Meaning>
    {
        public string Q { get; set; }

        public string MessageId { get; set; }

        public string ThreadId { get; set; }

        public int N { get; set; }

        public bool Verbose { get; set; }

        public string Version { get; set; }

        public string AccessToken { get; set; }

        public Context Context { get; set; }

        public HttpRequestMessage GetHttpRequestMessage()
        {
            string url = "/message";
            {
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["q"] = Q;
                if (Version != null) queryString["v"] = Version;
                if (Context != null) queryString["context"] = JsonConvert.SerializeObject(Context);
                if (MessageId != null) queryString["msg_id"] = MessageId;
                if (ThreadId != null) queryString["thread_id"] = ThreadId;
                if (N != 0) queryString["n"] = N.ToString();
                if (Verbose) queryString["verbose"] = true.ToString();

                url += $"?{queryString}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url, UriKind.Relative));

            if (AccessToken != null) request.Headers.Add("Authorization", $"Bearer {AccessToken}");

            return request;
        }
    }
}
