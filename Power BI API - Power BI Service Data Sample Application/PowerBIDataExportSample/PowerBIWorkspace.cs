using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PowerBIDataExportSample
{
    [DataContract]
    class PowerBIWorkspace
    {
        [DataMember]
        public List<PowerBIWorkspaceValue> value { get; set; }
    }
}
