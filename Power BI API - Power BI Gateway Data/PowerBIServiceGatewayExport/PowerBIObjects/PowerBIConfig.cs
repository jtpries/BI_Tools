using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PowerBIServiceGatewayExport
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
