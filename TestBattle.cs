using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.Samples;
using PlayFab.ServerModels;

namespace Test.Functions
{
    public static class TestBattle
    {
        private static readonly string RATING_KEY = "Rating";
        private static readonly string POWER_KEY = "Power";
        private static readonly List<string> DATA_KEYS = new List<string> { RATING_KEY, POWER_KEY };

        [FunctionName("TestBattle")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            dynamic args = context.FunctionArgument;

            string playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

            dynamic opponentId = null;
            if (args != null && args["opponent"] != null)
            {
                opponentId = args["opponent"];
            }

            var settings = new PlayFabApiSettings
            {
                TitleId = context.TitleAuthenticationContext.Id,
                DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY", EnvironmentVariableTarget.Process)
            };
            var authContext = new PlayFabAuthenticationContext
            {
                EntityToken = context.TitleAuthenticationContext.EntityToken
            };
            PlayFabServerInstanceAPI serverApi = new PlayFabServerInstanceAPI(settings, authContext);

            GetPlayerStatisticsRequest playerRequest = new GetPlayerStatisticsRequest()
            {
                PlayFabId = playerId,
                StatisticNames = DATA_KEYS
            };
            PlayFabResult<GetPlayerStatisticsResult> playerPlayfabResult = await serverApi.GetPlayerStatisticsAsync(playerRequest);
            GetPlayerStatisticsResult playerResult = playerPlayfabResult.Result;

            int playerRating = 1000;
            int playerPower = 0;
            foreach(StatisticValue statistic in playerResult.Statistics)
            {
                if (statistic.StatisticName.Equals(RATING_KEY))
                {
                    playerRating = statistic.Value;
                }
                if (statistic.StatisticName.Equals(POWER_KEY))
                {
                    playerPower = statistic.Value;
                }
            }

            GetPlayerStatisticsRequest opponentRequest = new GetPlayerStatisticsRequest() {
                PlayFabId = opponentId,
                StatisticNames = DATA_KEYS
            };
            PlayFabResult<GetPlayerStatisticsResult> opponentPlayfabResult = await serverApi.GetPlayerStatisticsAsync(opponentRequest);
            GetPlayerStatisticsResult opponentResult = opponentPlayfabResult.Result;

            int opponentRating = 1000;
            int opponentPower = 0;
            foreach(StatisticValue statistic in opponentResult.Statistics)
            {
                if (statistic.StatisticName.Equals(RATING_KEY))
                {
                    opponentRating = statistic.Value;
                }
                if (statistic.StatisticName.Equals(POWER_KEY))
                {
                    opponentPower = statistic.Value;
                }
            }

            return new {
                playerRating = playerRating,
                playerPower = playerPower,
                opponentRating = opponentRating,
                opponentPower = opponentPower
            };
        }
    }
}