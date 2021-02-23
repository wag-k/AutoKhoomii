using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AutoKhoomii
{
    /// <summary>
    /// 以下の記事そのまま
    /// [C#でWAVE再生] https://qiita.com/Stosstruppe/items/4d5e1511d082471d1cad
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    class WaveFileHeader
    {
        public uint riff_ckid = 0x46464952;    // "RIFF"
        public uint riff_cksize;
        public uint fccType = 0x45564157;      // "WAVE"
        public uint fmt_ckid = 0x20746d66;     // "fmt "
        public uint fmt_cksize = 16;
        public ushort wFormatTag = 0x0001;     // WAVE_FORMAT_PCM
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public uint data_ckid = 0x61746164;    // "data"
        public uint data_cksize;
    }
}
