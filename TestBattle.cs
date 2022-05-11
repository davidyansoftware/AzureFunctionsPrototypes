using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab.Samples;

namespace Test.Functions
{
    public static class TestBattle
    {
        [FunctionName("TestBattle")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            dynamic args = context.FunctionArgument;

            dynamic opponent = null;
            if (args != null && args["opponent"] != null)
            {
                opponent = args["opponent"];
            }

            return new {
                player = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
                opponent = opponent
            };
        }
    }
}