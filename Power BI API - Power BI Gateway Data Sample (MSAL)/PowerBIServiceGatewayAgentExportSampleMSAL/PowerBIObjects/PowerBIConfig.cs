using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceGatewayAgentExportSampleMSAL
{
    [DataContract]
    class PowerBIConfig
    {
        [DataMember]
        public string cloudName { get; set; }
        public List<PowerBIConfigService> services { get; set; }
        public List<PowerBIConfigClient> clients { get; set; }
    }
}
