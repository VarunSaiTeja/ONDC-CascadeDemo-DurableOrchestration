using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CascadeDemo
{
    public static class LogisticOnSearch
    {
        [FunctionName("OnSearch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, [DurableClient] IDurableOrchestrationClient durableCliebt)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var onSearchResp = JsonConvert.DeserializeObject<OnSearchRespone>(requestBody);
            var entityId = $"{onSearchResp.TransactionId}:{onSearchResp.MessageId}";
            var orchId = Ram.OnSelectOrch[entityId];
            Ram.OnSearchResponses[orchId].Add(onSearchResp);
            if (Ram.OnSearchResponses[orchId].Count > 2)
            {
                onSearchResp = Ram.OnSearchResponses[orchId].OrderBy(x => x.Price).FirstOrDefault();
                await durableCliebt.RaiseEventAsync(orchId, "bestOnSearch", onSearchResp);
            }
            return new OkObjectResult(new { message = new { ack = new { status = "ack" } } });
        }
    }
}
