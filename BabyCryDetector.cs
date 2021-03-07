using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using NAudio.Wave;
using NAudio.Codecs;
using NAudio.CoreAudioApi;

namespace AutoKhoomii
{
    public class BabyCryDetector
    {
        Complex[] fftData;
        public Complex[] FftData{
            get{return this.fftData;}
            set{this.fftData = value;}
        }
        public int WindowSize{get;set;}

        public WaveInEvent WaveInEvent{get;set;}
        public MemoryStream RecordedWave{get; private set;}
        public List<Complex[]> CryFrequencies{get;set;}
        public BabyCryDetector(){
            this.WindowSize = 2048;
            this.CryFrequencies = new List<Complex[]>();
        }

        ~BabyCryDetector(){
            this.WaveInEvent?.Dispose();
            this.WaveInEvent = null;

            this.RecordedWave?.Close();
            this.RecordedWave = null;
        }
        public void DetectCry(MemoryStream stream){
            byte[] sound = stream.GetBuffer();
            this.FftData = this.FFT(sound);
        }

        /// <summary>
        /// 泣き声のサンプリングを行います。
        /// </summary>
        public void StartSamplingCry()
        {
            // 録音デバイスを選びたい場合は、WaveInEvent.DeviceCount、WaveInEvent.GetCapabilities を使って探してください。
            var deviceNumber = 0;

            // 録画処理を開始
            // WaveIn だと、「System.InvalidOperationException: 'Use WaveInEvent to record on a background thread'」のエラーが発生する
            // WaveIn = new WaveIn();
            this.WaveInEvent = new WaveInEvent();
            this.WaveInEvent.DeviceNumber = deviceNumber;
            this.WaveInEvent.WaveFormat = new WaveFormat(44100, WaveInEvent.GetCapabilities(deviceNumber).Channels);

            this.RecordedWave = new MemoryStream();
            WaveFileWriter WaveWriter = new WaveFileWriter(this.RecordedWave, this.WaveInEvent.WaveFormat);

            this.WaveInEvent.DataAvailable += (_, ee) =>
            {
                WaveWriter.Write(ee.Buffer, 0, ee.BytesRecorded);
                WaveWriter.Flush();
            };
            this.WaveInEvent.RecordingStopped += (_, __) =>
            {
                WaveWriter.Flush();
            };

            this.WaveInEvent.StartRecording();
        }


        public void StopSamplingCry()
        {
            this.WaveInEvent?.StopRecording();
            this.RecordedWave.Seek(0, SeekOrigin.Begin);
            Byte[] waveByte = this.RecordedWave.GetBuffer();
            int numFFTSample = waveByte.Length / this.WindowSize;
            for (int n=0; n < numFFTSample; ++n){
                this.CryFrequencies.Add(this.FFT(this.RecordedWave.GetBuffer(), n*this.WindowSize));
            }
        }
        public Complex[] FFT(Byte[] soundByte){
            return (this.FFT(soundByte, 0));
        }
        public Complex[] FFT(Byte[] soundByte, int idx_start){
            Complex[] fft = new Complex[this.WindowSize];

            for (int n = 0; n < this.WindowSize; ++n){
                int idx = n+idx_start;
                fft[n] = new Complex((double)soundByte[idx], 0);
            }
            Fourier.Forward(fft, FourierOptions.Matlab);
            /*
            for (int n = 0; n < WindowSize; ++n){
                Console.WriteLine("n: "+n+", freq: "+(float)n/((float)this.WindowSize/44100)+", Vol: "+fft[n].Magnitude);
            }
            */
            return fft;
        }

    }
}
