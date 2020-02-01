using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PowerBIServiceActivityLogExport
{
    [DataContract]
    class PowerBIActivityLogSubscribeeInformation
    {
        [DataMember]
        public string RecipientEmail { get; set; }
        public string RecipientName { get; set; }
        public string ObjectId { get; set; }
    }
}
