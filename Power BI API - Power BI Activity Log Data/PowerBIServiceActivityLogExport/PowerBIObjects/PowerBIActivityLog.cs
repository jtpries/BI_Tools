using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceActivityLogExport
{
    [DataContract]
    class PowerBIActivityLog
    {
        [DataMember]
        public List<PowerBIActivityLogEntity> activityEventEntities { get; set; }
        public string continuationUri { get; set; }
        public string continuationToken { get; set; }
    }
}
