﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceGatewayAgentExportSampleMSAL
{
    class PowerBIConfigService
    {
        public string name { get; set; }
        public string endpoint { get; set; }
        public string resourceId { get; set; }
        public List<object> allowedDomains { get; set; }
    }
}
