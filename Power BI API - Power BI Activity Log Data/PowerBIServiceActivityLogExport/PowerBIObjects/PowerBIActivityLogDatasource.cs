using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceActivityLogExport
{
    [DataContract]
    class PowerBIActivityLogDatasource
    {
        [DataMember]
        public string DatasourceType { get; set; }
        public string ConnectionDetails { get; set; }
    }
}
