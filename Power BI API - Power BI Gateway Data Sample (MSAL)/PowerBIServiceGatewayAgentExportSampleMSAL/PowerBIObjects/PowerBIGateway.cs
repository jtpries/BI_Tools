using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceGatewayAgentExportSampleMSAL
{
    [DataContract]
    class PowerBIGateway
    {
        [DataMember]
        public string gatewayId { get; set; }
        public string objectId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string publicKey { get; set; }
        public string keyword { get; set; }
        public string metadata { get; set; }
        public List<PowerBIGatewayAgent> gateways { get; set; }
        public string loadBalancingSettings { get; set; }
        public string annotation { get; set; }
        public string versionStatus { get; set; }
        public string expiryDate { get; set; }
        public string type { get; set; }
        public string options { get; set; }
    }
}
