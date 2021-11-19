using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PowerBIDataExportSampleSPN
{
    [DataContract]
    class PowerBIWorkspace
    {
        [DataMember]
        public List<PowerBIWorkspaceValue> value { get; set; }
    }
}
