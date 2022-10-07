using System.Collections.Generic;

namespace CascadeDemo
{
    public static class Ram
    {
        /// <summary>
        /// Key => TxId:MsgId
        /// Value => onSelectOrchestrator instanceId
        /// </summary>
        public static Dictionary<string, string> OnSelectOrch = new Dictionary<string, string>();

        /// <summary>
        /// Key => onSelectOrchestrator instanceId
        /// </summary>
        public static Dictionary<string, List<OnSearchRespone>> OnSearchResponses = new Dictionary<string, List<OnSearchRespone>>();
    }
}
