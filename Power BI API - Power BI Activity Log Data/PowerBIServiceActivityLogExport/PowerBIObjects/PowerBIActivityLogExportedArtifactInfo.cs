using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceActivityLogExport
{
    [DataContract]
    class PowerBIActivityLogExportedArtifactInfo
    {
        [DataMember]
        public string ExportType { get; set; }
        public string ArtifactType { get; set; }
        public int ArtifactId { get; set; }
    }
}
