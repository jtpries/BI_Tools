using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceGatewayAgentExportSampleMSAL
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
