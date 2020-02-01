using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceActivityLogExport
{
    [DataContract]
    class PowerBIActivityLogDataset
    {
        [DataMember]
        public string DatasetId { get; set; }
        public string DatasetName { get; set; }
    }
}
