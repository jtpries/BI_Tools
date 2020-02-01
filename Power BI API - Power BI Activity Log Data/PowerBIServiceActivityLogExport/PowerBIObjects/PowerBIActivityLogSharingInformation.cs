using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceActivityLogExport
{
    [DataContract]
    class PowerBIActivityLogSharingInformation
    {
        [DataMember]
        public string RecipientEmail { get; set; }
        public string ResharePermission { get; set; }
    }
}
