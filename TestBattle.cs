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

        private static float ELO_K = 10f;

        [FunctionName("TestBattle")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            dynamic args = context.FunctionArgument;

            string playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

            string opponentId = null;
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

            BattleData player = await FetchBattleData(serverApi, playerId);
            BattleData opponent = await FetchBattleData(serverApi, opponentId);

            double chanceOfPlayerWin = ExpectationOfPlayerWin(player.power, opponent.power);
            Random random = new Random();
            bool playerWin = random.NextDouble() <= chanceOfPlayerWin;

            int playerRatingDelta = PlayerRatingDelta(player.rating, opponent.rating, playerWin);
            
            int newPlayerRating = player.rating + playerRatingDelta;
            int newOpponentRating = opponent.rating - playerRatingDelta;

            UpdateRating(serverApi, playerId, newPlayerRating);
            UpdateRating(serverApi, opponentId, newOpponentRating);
            
            return new {
                win = playerWin,
                winChance = chanceOfPlayerWin, // currently only using for display
                rating = newPlayerRating
            };
        }

        private static int PlayerRatingDelta(int playerRating, int opponentRating, bool playerWin) {
            double expectationOfPlayerWin = ExpectationOfPlayerWin(playerRating, opponentRating);
            
            int outcome = playerWin ? 1 : 0;
            return (int)(ELO_K * ((float)outcome - expectationOfPlayerWin));
        }

        private static double ExpectationOfPlayerWin(int playerRating, int opponentRating) {
            return 1f / (1f + Math.Pow(10f, (opponentRating - playerRating) / 400f));
        }

        private static void UpdateRating(PlayFabServerInstanceAPI serverApi, string playfabId, int newRating) {
            UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest() {
                PlayFabId = playfabId,
                Statistics = new List<StatisticUpdate>() {
                    new StatisticUpdate() {
                        StatisticName = RATING_KEY,
                        Value = newRating
                    }
                }
            };
            serverApi.UpdatePlayerStatisticsAsync(request);
        }

        private static async Task<BattleData> FetchBattleData(PlayFabServerInstanceAPI serverApi, string playfabId) {
            GetPlayerStatisticsRequest request = new GetPlayerStatisticsRequest() {
                PlayFabId = playfabId,
                StatisticNames = DATA_KEYS
            };
            PlayFabResult<GetPlayerStatisticsResult> playfabResult = await serverApi.GetPlayerStatisticsAsync(request);
            GetPlayerStatisticsResult result = playfabResult.Result;

            int rating = 1000;
            int power = 0;
            foreach(StatisticValue statistic in result.Statistics)
            {
                if (statistic.StatisticName.Equals(RATING_KEY))
                {
                    rating = statistic.Value;
                }
                if (statistic.StatisticName.Equals(POWER_KEY))
                {
                    power = statistic.Value;
                }
            }

            return new BattleData(power, rating);
        }
    }

    internal class BattleData {
        public readonly int power;
        public readonly int rating;

        public BattleData(int power, int rating) {
            this.power = power;
            this.rating = rating;
        }
    }
}