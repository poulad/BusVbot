using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests.Abstractions;

// ReSharper disable once CheckNamespace
namespace Telegram.Bot
{
    /// <summary>
    /// Contains extension methods for the <see cref="ITelegramBotClient"/> instances
    /// </summary>
    public static class BotClientExtensions
    {
        /// <summary>Send a request to the Bot API with retry policy in case if the request being timed out</summary>
        /// <typeparam name="TResponse">Type of expected result in the response object</typeparam>
        /// <param name="client">Bot client instance</param>
        /// <param name="request">API request object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result of the API request</returns>
        public static Task<TResponse> MakeRequestWithRetryAsync<TResponse>(
            this ITelegramBotClient client,
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default
        ) =>
            Policy
                .Handle<ApiRequestException>(exception => exception.Message == "Request timed out")
                .WaitAndRetryAsync(
                    new[]
                    {
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(25),
                        TimeSpan.FromSeconds(30),
                    }
                )
                .ExecuteAsync(_ => client.MakeRequestAsync(request, cancellationToken), cancellationToken);
    }
}
