using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PowerBIServiceGatewayExport
{
    [DataContract]
    class PowerBIBackend
    {
        [DataMember]
        public string FixedClusterUri { get; set; }
        public object NewTenantId { get; set; }
        public string RuleDescription { get; set; }
        public int TTLSeconds { get; set; }
    }
}
