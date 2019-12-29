using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBIServiceGatewayExport
{
    class PowerBIConfigService
    {
        public string name { get; set; }
        public string endpoint { get; set; }
        public string resourceId { get; set; }
        public List<object> allowedDomains { get; set; }
    }
}
