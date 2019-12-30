using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBIDataExportSample
{
    class PowerBIWorkspaceValue
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string capacityId { get; set; }
        public string dataflowStorageId { get; set; }
        public bool isOnDedicatedCapacity { get; set; }
        public bool isReadOnly { get; set; }
        public bool isOrphaned { get; set; }
        public string state { get; set; }
        public string type { get; set; }
    }
}
