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

            dynamic name = null;
            if (args != null && args["name"] != null)
            {
                name = args["name"];
            }

            log.LogDebug($"HelloWorld: {new { input = name } }");

            return new {
                callerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
                inputData = name
            };
        }
    }
}