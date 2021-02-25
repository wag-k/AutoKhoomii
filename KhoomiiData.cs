using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace AutoKhoomii
{
    [DataContract]
    public class KhoomiiData
    {
        List<float> frequencies;

        [DataMember(Name="Name")]
        public String Name{ get; set;}
        [DataMember(Name="Frequencies")]
        public List<float> Frequencies{
            get{return this.frequencies;}
            set{this.frequencies = value;}
        }
    }
}
