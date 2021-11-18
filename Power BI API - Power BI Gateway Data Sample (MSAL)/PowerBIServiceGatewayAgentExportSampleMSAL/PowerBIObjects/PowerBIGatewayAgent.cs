using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceGatewayAgentExportSampleMSAL
{
    class PowerBIGatewayAgent
    {
        public string gatewayId { get; set; }
        public string gatewayObjectId { get; set; }
        public string gatewayName { get; set; }
        public string gatewayAnnotation { get; set; }
        public string gatewayStatus { get; set; }
        public bool isAnchorGateway { get; set; }
        public string gatewayClusterStatus { get; set; }
        public string gatewayLoadBalancingSettings { get; set; }
        public string gatewayStaticCapabilities { get; set; }
        public string gatewayPublicKey { get; set; }
        public string gatewayVersion { get; set; }
        public string gatewayVersionStatus { get; set; }
        public string expiryDate { get; set; }
    }
}
