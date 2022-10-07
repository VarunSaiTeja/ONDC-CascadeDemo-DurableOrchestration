using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CascadeDemo
{
    public class OnSearchRespone
    {
        public string Provider { get; set; }
        public string Price { get; set; }
        public string TransactionId { get; set; }
        public string MessageId { get; set; }
    }
}
