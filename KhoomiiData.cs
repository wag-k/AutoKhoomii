using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace AutoKhoomii
{
    /// <summary>
    /// 周波数の情報を扱う。周波数とボリュームを保持する。
    /// ボリュームの単位はdB
    /// </summary>
    [DataContract]
    public class FrequencyInfo{
        [DataMember(Name="Frequency")]
        public float Frequency{get;set;}
        [DataMember(Name="Volume")]
        public float Volume{get;set;}
    }

    [DataContract]
    public class KhoomiiData
    {
        List<FrequencyInfo> frequencyInfos;

        [DataMember(Name="Name")]
        public String Name{ get; set;}
        [DataMember(Name="FrequencyInfos")]
        public List<FrequencyInfo> FrequencyInfos{
            get{return this.frequencyInfos;}
            set{this.frequencyInfos = value;}
        }

        public static float dBToAmplitude(float db, float standard){
            return (float)Math.Pow(10, db/20.0) * standard;
        }
    }

}
