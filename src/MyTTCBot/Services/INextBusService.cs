using System.Collections.Generic;
using System.Threading.Tasks;
using MyTTCBot.Models.NextBus;

namespace MyTTCBot.Services
{
    public interface INextBusService
    {
        Task<PredictionsResponse> GetPredictions();
    }
}
