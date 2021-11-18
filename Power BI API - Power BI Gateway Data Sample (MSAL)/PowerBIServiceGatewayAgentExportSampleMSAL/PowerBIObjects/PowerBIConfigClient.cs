using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceGatewayAgentExportSampleMSAL
{
    class PowerBIConfigClient
    {
        public string name { get; set; }
        public string appId { get; set; }
        public string redirectUri { get; set; }
        public string appInsightsId { get; set; }
        public string localyticsId { get; set; }
    }
}
