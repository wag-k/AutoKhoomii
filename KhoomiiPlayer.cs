using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace AutoKhoomii
{
    class KhoomiiPlayer
    {
        public void Run(){
            MemoryStream waveStream = CreateWave();
            PlayKhoomii(ref waveStream);
            waveStream.Close();
        }
        
        public MemoryStream CreateWave(){
            const uint sampleRate = 44100;  // サンプリング周波数
            // 波形データの生成
            uint wavelen = (uint)(sampleRate * 1.0);
            byte[] wave = CreateKhoomiiSound(wavelen, sampleRate);

            //using (FileStream st = new FileStream(@"c:\tmp\hoge.wav", FileMode.Create))
            MemoryStream st = new MemoryStream();
        
            // WAVEファイルヘッダ
            WriteStr(st, "RIFF");
            WriteVal(st, 4, wavelen + 36);
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
            WriteVal(st, 4, wavelen);

            st.Write(wave, 0, wave.Length);

            st.Seek(0, SeekOrigin.Begin); // Seekを原点にしておく。

            return st;
        }

        public byte[] CreateKhoomiiSound(uint wavelen, uint sampleRate){
            byte[] wave = new byte[wavelen];

            double t = 0;
            for (uint i = 0; i < wavelen; i++)
            {
                t = (t + 880.0 / sampleRate) % 1;
                wave[i] = (byte)(128 + Math.Sin(2 * Math.PI * t) * (1 - i / (double)wavelen) * 10);
            }
            return wave;
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

        public void PlayKhoomii(ref MemoryStream wave){
            wave.Seek(0, SeekOrigin.Begin);
            var player = new SoundPlayer(wave);
            player.PlaySync();
        }
    }
}
