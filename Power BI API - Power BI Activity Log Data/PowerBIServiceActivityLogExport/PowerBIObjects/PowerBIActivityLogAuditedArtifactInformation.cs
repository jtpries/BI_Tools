using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceActivityLogExport
{
    [DataContract]
    class PowerBIActivityLogAuditedArtifactInformation
    {
        [DataMember]
        public string Id { get; set; }
        public string Name { get; set; }
        public string ArtifactObjectId { get; set; }
        public string AnnotatedItemType { get; set; }
    }
}
