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

        public WaveInEvent RecordCryWaveIn{get;set;}
        public WaveInEvent RecordWaveIn{get;set;}
        /// <summary>
        /// 泣き声のサンプルデータ
        /// </summary>
        /// <value></value>
        public MemoryStream RecordedCryWave{get; private set;}
        /// <summary>
        /// 自動検出で用いるストリーミングデータ
        /// </summary>
        /// <value></value>
        public MemoryStream RecordedWave{get; private set;}
        public List<Complex[]> CryFrequencies{get;set;}
        public BabyCryDetector(){
            this.WindowSize = 2048;
            this.CryFrequencies = new List<Complex[]>();
            this.RecordCryWaveIn = this.CreateWaveInEvent();
            this.RecordWaveIn = this.CreateWaveInEvent();
        }

        ~BabyCryDetector(){
            this.RecordCryWaveIn?.Dispose();
            this.RecordCryWaveIn = null;

            this.RecordedCryWave?.Close();
            this.RecordedCryWave = null;
        }

        private WaveInEvent CreateWaveInEvent(){
            // 録音デバイスを選びたい場合は、WaveInEvent.DeviceCount、WaveInEvent.GetCapabilities を使って探してください。
            var deviceNumber = 0;

            WaveInEvent waveInEvent = new WaveInEvent();
            waveInEvent.DeviceNumber = deviceNumber;
            waveInEvent.WaveFormat = new WaveFormat(44100, WaveInEvent.GetCapabilities(deviceNumber).Channels);
            return waveInEvent;
        }

        /// <summary>
        /// 自動検出用の録音を開始します
        /// </summary>
        public void StartDetectingCry(){
            this.RecordedWave = new MemoryStream();
            WaveFileWriter waveWriter = new WaveFileWriter(this.RecordedWave, this.RecordWaveIn.WaveFormat);
            EventHandler<WaveInEventArgs> writeWW = (_, ee) =>
            {
                waveWriter.Write(ee.Buffer, 0, ee.BytesRecorded);
                waveWriter.Flush();
            };
            this.RecordWaveIn.DataAvailable += writeWW;
            this.RecordWaveIn.RecordingStopped += (_, __) =>
            {
                waveWriter.Flush();
            };

            this.RecordWaveIn.StartRecording();
        }

        /// <summary>
        /// 自動検出用の録音を終了します
        /// </summary>
        /// <returns></returns>
        public void StopDetectingCry(){
            this.RecordCryWaveIn?.StopRecording();
            this.RecordedCryWave.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// 取得した音声とあらかじめ取得した泣き声データを比較して泣き声判定します。
        /// </summary>
        /// <returns>trueなら泣き声、falseは違う</returns>
        public bool DetectCry(){
            this.RecordWaveIn?.StopRecording(); // いったん止めてFlushする
            Byte[] sound = this.RecordedWave.GetBuffer();
            // Windowサイズより小さかったら実行しません
            if (sound.Length < this.WindowSize){
                return false;
            }
            this.FftData = this.FFT(sound, sound.Length - 1 - this.WindowSize); // 直近の音を使う
            this.RecordCryWaveIn?.StartRecording(); // 再開
            return true;
        }

        /// <summary>
        /// 泣き声のサンプリングを行います。
        /// </summary>
        public void StartSamplingCry()
        {
            // 録画処理を開始
            // WaveIn だと、「System.InvalidOperationException: 'Use WaveInEvent to record on a background thread'」のエラーが発生する
            // WaveIn = new WaveIn();

            this.RecordedCryWave = new MemoryStream();
            WaveFileWriter waveWriter = new WaveFileWriter(this.RecordedCryWave, this.RecordCryWaveIn.WaveFormat);
            EventHandler<WaveInEventArgs> writeWW = (_, ee) =>
            {
                waveWriter.Write(ee.Buffer, 0, ee.BytesRecorded);
                waveWriter.Flush();
            };
            this.RecordCryWaveIn.DataAvailable += writeWW;
            this.RecordCryWaveIn.RecordingStopped += (_, __) =>
            {
                waveWriter.Flush();
            };

            this.RecordCryWaveIn.StartRecording();
        }


        public void StopSamplingCry()
        {
            this.RecordCryWaveIn?.StopRecording();
            this.RecordedCryWave.Seek(0, SeekOrigin.Begin);
            Byte[] waveByte = this.RecordedCryWave.GetBuffer();
            int numFFTSample = waveByte.Length / this.WindowSize;
            for (int n=0; n < numFFTSample; ++n){
                this.CryFrequencies.Add(this.FFT(this.RecordedCryWave.GetBuffer(), n*this.WindowSize));
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
