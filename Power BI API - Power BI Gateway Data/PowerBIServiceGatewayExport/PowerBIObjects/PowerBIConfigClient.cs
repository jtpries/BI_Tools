using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBIServiceGatewayExport
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
