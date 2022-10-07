using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CascadeDemo
{
    public static class RetailSelect
    {
        [FunctionName("SelectOrchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var logger = context.CreateReplaySafeLogger(log);
            var outputs = new List<string>();

            var logisticRequest = await context.CallActivityAsync<LogisticSearchRequest>("BuildLogisticSearchRequest", context.InstanceId);

            await context.CallActivityAsync("CallLogisticSearchRequest", logisticRequest);
            OnSearchRespone bestOnSearchResponse = default;

            var timerCancellationToken = await context.CallActivityAsync<CancellationTokenSource>("GetLogisticSearchTimerCancellationToken", null);
            var timer = context.CreateTimer(context.CurrentUtcDateTime.AddSeconds(60), timerCancellationToken.Token);

            var quoteEvent = context.WaitForExternalEvent<OnSearchRespone>("bestOnSearch");
            await Task.WhenAny(timer, quoteEvent);

            if (quoteEvent.IsCompletedSuccessfully)
            {
                bestOnSearchResponse = quoteEvent.Result;
                timerCancellationToken.Cancel();
            }

            if (bestOnSearchResponse == null)
            {
                logger.LogWarning("Timer executed, looking for existing onSearch responses");
                bestOnSearchResponse ??= Ram.OnSearchResponses[context.InstanceId].OrderBy(x => x.Price).FirstOrDefault();
            }
            else
            {
                logger.LogWarning("Got best provider out of 3");
            }

            if (bestOnSearchResponse == null)
            {
                logger.LogWarning("No logistic on_search, No retail on_select");
            }
            else
            {
                await context.CallActivityAsync("DoRetailOnSelect", bestOnSearchResponse);
            }
            return outputs;
        }

        [FunctionName("DoRetailOnSelect")]
        public static void DoRetailOnSelect([ActivityTrigger] OnSearchRespone bestOnSearchResponse, ILogger log)
        {
            log.LogWarning($"Best quote is from {bestOnSearchResponse.Provider} for {bestOnSearchResponse.Price}");
            log.LogWarning($"Preparing retail on_select response");
        }

        [FunctionName("GetLogisticSearchTimerCancellationToken")]
        public static CancellationTokenSource GetLogisticSearchTimerCancellationToken([ActivityTrigger] string myData)
        {
            return new CancellationTokenSource();
        }

        [FunctionName("CallLogisticSearchRequest")]
        public static void CallLogisticSearchRequest([ActivityTrigger] LogisticSearchRequest requestData, ILogger log)
        {
            log.LogWarning($"Calling search request with \nMsgId:{requestData.MessageId}\nTxId:{requestData.TransactionId}");
        }

        [FunctionName("BuildLogisticSearchRequest")]
        public static LogisticSearchRequest BuildLogisticSearchRequest([ActivityTrigger] string instanceId, ILogger log)
        {
            var logSearchReq = new LogisticSearchRequest
            {
                MessageId = Guid.NewGuid().ToString(),
                TransactionId = Guid.NewGuid().ToString(),
                Data = instanceId
            };
            var entityId = $"{logSearchReq.TransactionId}:{logSearchReq.MessageId}";
            Ram.OnSelectOrch.Add(entityId, instanceId);
            Ram.OnSearchResponses[instanceId] = new List<OnSearchRespone>();
            return logSearchReq;
        }

        [FunctionName("Select")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("SelectOrchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            return starter.CreateCheckStatusResponse(req, instanceId);
            //return new HttpResponseMessage { Content = new StringContent("{\"message\": {\"ack\": {\"status\": \"ACK\"}}") };
        }
    }
}