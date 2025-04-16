using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizFlow.Core.Internal
{
    class TriggerContext
    {
        public string OperationName { get; set; }
        public bool RunUnblockedOnlyOps { get; set; }
        public string TriggerName { get; set; }
        public string CronExpression { get; set; }
        public string LaunchId { get; set; }
    }
}
