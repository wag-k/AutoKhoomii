using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace AutoKhoomii
{
    public class KhoomiiPlayer
    {
        List<KhoomiiData> khoomiiDatas;
        List<MemoryStream> playList;
        public List<KhoomiiData> KhoomiiDatas{
            get{return this.khoomiiDatas;}
            set{this.khoomiiDatas = value;}
        }
        public List<MemoryStream> PlayList{
            get{return this.playList;}
            set{this.playList = value;}
        }
        public SoundPlayer Player{get;set;}
        private MemoryStream KhoomiiMelody{get;set;}

        public KhoomiiPlayer(){
            this.KhoomiiDatas = LoadKhoomiiFrequency("./data/KhoomiiFrequency.json");
        }

        ~KhoomiiPlayer(){
            this.KhoomiiMelody.Close();
        }

        public void Run(){
            MemoryStream waveStream = CreateWave(this.KhoomiiDatas, 2);
            Play(ref waveStream);
            waveStream.Close();
        }

        public void LoadKhoomiiMelody(){
            this.KhoomiiMelody = this.CreateWave(this.KhoomiiDatas, 2);
        }
        
        public MemoryStream CreateWave(List<KhoomiiData> khoomiiDatas, float duration){
            const uint sampleRate = 44100;  // サンプリング周波数
            // 波形データの生成
            uint wavelen = (uint)(sampleRate * duration);
            byte[] khoomiiMelody = new byte[wavelen*khoomiiDatas.Count];
            for(int n = 0; n < khoomiiDatas.Count; ++n){
                byte[] wave = CreateKhoomiiSound(wavelen, sampleRate, khoomiiDatas[n]);
                wave.CopyTo(khoomiiMelody, n*wavelen);
            }
            

            //using (FileStream st = new FileStream(@"c:\tmp\hoge.wav", FileMode.Create))
            MemoryStream st = new MemoryStream();
        
            // WAVEファイルヘッダ
            WriteStr(st, "RIFF");
            WriteVal(st, 4, (uint)khoomiiMelody.Length + 36);
            WriteStr(st, "WAVE");
            WriteStr(st, "fmt ");
            WriteVal(st, 4, 16);
            WriteVal(st, 2, 0x0001);         // WAVE_FORMAT_PCM
            WriteVal(st, 2, 1);              // nChannels
            WriteVal(st, 4, sampleRate);     // nSamplesPerSec
            WriteVal(st, 4, sampleRate);     // nAvgBytesPerSec
            WriteVal(st, 2, 1);              // nBlockAlign
            WriteVal(st, 2, 8);              // wBitsPerSample
            WriteStr(st, "data");
            WriteVal(st, 4, (uint)khoomiiMelody.Length);

            st.Write(khoomiiMelody, 0, khoomiiMelody.Length);

            st.Seek(0, SeekOrigin.Begin); // Seekを原点にしておく。

            return st;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // dBToAmplitudeをinlineで呼び出したい
        public byte[] CreateKhoomiiSound(uint wavelen, uint sampleRate, KhoomiiData khoomiiData){
            byte[] wave = new byte[wavelen];
            int num_freq = khoomiiData.FrequencyInfos.Count;
            foreach (var frequencyInfo in khoomiiData.FrequencyInfos)
            {
                double t = 0;
                for (uint i = 0; i < wavelen; i++)
                {
                    t = (t + frequencyInfo.Frequency / sampleRate) % 1;
                    wave[i] += (byte)(128 + Math.Sin(2 * Math.PI * t) * (1 - i / (double)wavelen) * KhoomiiData.dBToAmplitude(frequencyInfo.Volume, (float)0.0001*2) / 5);
                }
            }
            return wave;
        }

        /// <summary>
        /// Jsonファイルからホーミー周波数を読み取ります。
        /// ホーミーの周波数は以下を参考にした。
        /// [モンゴルの歌唱法「ホーミー」の音響的特徴の解析] https://www.jstage.jst.go.jp/article/jasj/56/5/56_KJ00001457362/_pdf/-char/ja 
        /// </summary>
        /// <param name="fPath"></param>
        /// <returns></returns>
        public List<KhoomiiData> LoadKhoomiiFrequency(String fPath){
            List<KhoomiiData> khoomiiDatas;
            using(FileStream fs = new FileStream(fPath, FileMode.Open)){
                var serializer = new DataContractJsonSerializer(typeof(List<KhoomiiData>));
                khoomiiDatas = serializer.ReadObject(fs) as List<KhoomiiData>;
                return khoomiiDatas;
            }
        }

        private void WriteVal(Stream st, int len, uint value)
        {
            byte[] ba = BitConverter.GetBytes(value);
            st.Write(ba, 0, len);
        }

        private void WriteStr(Stream st, string str)
        {
            byte[] ba = Encoding.ASCII.GetBytes(str);
            st.Write(ba, 0, ba.Length);
        }

        public void Play(){
            this.KhoomiiMelody.Seek(0, SeekOrigin.Begin);
            this.Player = new SoundPlayer(this.KhoomiiMelody);
            this.Player.Play();
        }
        private void Play(ref MemoryStream wave){
            wave.Seek(0, SeekOrigin.Begin);
            this.Player = new SoundPlayer(wave);
            this.Player.Play();
        }

        public void Stop(){

        }
    }
}
