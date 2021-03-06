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
        private WaveFileWriter WaveWriter{get;set;}
        public BabyCryDetector(){
            this.WindowSize = 2048;
            this.FftData = new Complex[this.WindowSize];
        }

        public void DetectCry(MemoryStream stream){
            byte[] sound = stream.GetBuffer();
            for (int n = 0; n < this.WindowSize; ++n){
                FftData[n] = new Complex((double)sound[n], 0);
            }
            Fourier.Forward(this.FftData, FourierOptions.Matlab);
            for (int n = 0; n < WindowSize; ++n){
                Console.WriteLine("n: "+n+", freq: "+(float)n/((float)this.WindowSize/44100)+", Vol: "+FftData[n].Magnitude);
            }
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            // 録音デバイスを選びたい場合は、WaveInEvent.DeviceCount、WaveInEvent.GetCapabilities を使って探してください。
            var deviceNumber = 0;

            // 録画処理を開始
            // WaveIn だと、「System.InvalidOperationException: 'Use WaveInEvent to record on a background thread'」のエラーが発生する
            // WaveIn = new WaveIn();
            this.WaveInEvent = new WaveInEvent();
            this.WaveInEvent.DeviceNumber = deviceNumber;
            this.WaveInEvent.WaveFormat = new WaveFormat(44100, WaveInEvent.GetCapabilities(deviceNumber).Channels);

            WaveWriter = new WaveFileWriter("C:\\temp\\test1.wav", this.WaveInEvent.WaveFormat);

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

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.WaveInEvent?.StopRecording();
            this.WaveInEvent?.Dispose();
            this.WaveInEvent = null;

            WaveWriter?.Close();
            WaveWriter = null;
        }

    }
}
