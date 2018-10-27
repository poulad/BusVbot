using System;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BusV.Ops
{
    public class UserService : IUserService
    {
        private readonly IAgencyRepo _agencyRepo;
        private readonly IUserProfileRepo _userProfileRepo;

        public UserService(
            IAgencyRepo agencyRepo,
            IUserProfileRepo userProfileRepo
        )
        {
            _agencyRepo = agencyRepo;
            _userProfileRepo = userProfileRepo;
        }

        public async Task<Error> SetDefaultAgencyAsync(
            string user,
            string chat,
            string agencyTag,
            CancellationToken cancellationToken = default
        )
        {
            Error error;

            var profile = await _userProfileRepo.GetByUserchatAsync(user, chat, cancellationToken)
                .ConfigureAwait(false);

            var agency = await _agencyRepo.GetByTagAsync(agencyTag, cancellationToken)
                .ConfigureAwait(false);

            if (profile is null)
            {
                profile = new UserProfile
                {
                    UserId = user,
                    ChatId = chat,
                    AgencyDbRef = new MongoDBRef(
                        Data.Constants.Collections.Agencies.Name,
                        ObjectId.Parse(agency.Id)
                    )
                };
                var profileError = await _userProfileRepo.AddAsync(profile, cancellationToken)
                    .ConfigureAwait(false);

                if (profileError is null)
                    error = null;
                else
                {
                    // ToDo log
                    // ToDo create a new Error w another code
                    error = profileError;
                }
            }
            else
            {
                // ToDo update the value
                error = new Error(new NotImplementedException().Message);
            }

            return error;
        }
    }
}
