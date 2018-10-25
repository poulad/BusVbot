using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TelegramTests.Shared
{
    public static class Extensions
    {
        public static Task<HttpResponseMessage> PostWebhookUpdateAsync(this HttpClient client, string json) =>
            client.PostAsync(
                "/api/bots/1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy/webhook",
                new StringContent(json, Encoding.UTF8)
            );
    }
}
