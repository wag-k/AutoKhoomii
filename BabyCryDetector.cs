using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Statistics;
using NAudio.Wave;
using NAudio.Codecs;
using NAudio.CoreAudioApi;

using ArrayOperation;

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
        public List<double[]> CryVolumeFrequencies{get;set;}
        public AutoResetEvent AutoResetEvent{get;set;}
        public BabyCryDetector(){
            this.WindowSize = 4096*4;
            this.CryFrequencies = new List<Complex[]>();
            this.CryVolumeFrequencies = new List<double[]>();
            this.RecordCryWaveIn = this.CreateWaveInEvent();
            this.RecordWaveIn = this.CreateWaveInEvent();
            this.AutoResetEvent = new AutoResetEvent(false);
        }

        ~BabyCryDetector(){
            this.RecordCryWaveIn?.Dispose();
            this.RecordCryWaveIn = null;

            this.RecordedCryWave?.Close();
            this.RecordedCryWave = null;

            this.RecordWaveIn?.Dispose();
            this.RecordWaveIn = null;

            this.RecordedWave?.Close();
            this.RecordedWave = null;
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
            EventHandler<WaveInEventArgs> writeWW = (_, ee) =>
            {
                WaveFileWriter waveWriter = new WaveFileWriter(this.RecordedWave, this.RecordWaveIn.WaveFormat);
                waveWriter.Write(ee.Buffer, 0, ee.BytesRecorded);
                waveWriter.Flush();
            };
            this.RecordWaveIn.DataAvailable += writeWW;
            this.RecordWaveIn.RecordingStopped += (_, __) =>
            {
                WaveFileWriter waveWriter = new WaveFileWriter(this.RecordedWave, this.RecordWaveIn.WaveFormat);
                waveWriter.Flush();
                //this.AutoResetEvent.Set();
            };

            this.RecordWaveIn?.StartRecording();
        }

        /// <summary>
        /// 自動検出用の録音を終了します
        /// </summary>
        /// <returns></returns>
        public void StopDetectingCry(){
            this.RecordWaveIn?.StopRecording();
            this.RecordedWave.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// 取得した音声とあらかじめ取得した泣き声データを比較して泣き声判定します。
        /// </summary>
        /// <returns>trueなら泣き声、falseは違う</returns>
        public bool DetectCry(){
            this.RecordWaveIn?.StopRecording();
            // 非同期の処理がなんかうまくいかない。
            /*
            if(!this.AutoResetEvent.WaitOne(5000)){
                this.RecordedWave.Dispose(); // 放っておくとどんどんメモリを食うのでクリア
                this.RecordedWave = new MemoryStream();
                return false; // Flushに時間かかりすぎたらfalseで返す
            }
            */

            Byte[] sound = this.RecordedWave.GetBuffer();
            this.AutoResetEvent.Reset(); // フラグ戻しておく。OOらしくないので、気に入らない。
            // Windowサイズより小さかったら実行しません
            if (sound.Length < this.WindowSize){
                try{
                    this.RecordWaveIn?.StartRecording(); // 再開
                } catch(InvalidOperationException e){
                    Console.WriteLine(e.Message); // なぜかStopしていないことがある。
                }
                return false;
            }
            this.FftData = this.FFT(sound); // 直近の音を使う
            double[] volumes = new double[this.WindowSize];
            for (int n_volume=0; n_volume < volumes.Length; ++n_volume){
                volumes[n_volume] = this.FftData[n_volume].Magnitude;
            }
            double[] cors = new double[this.CryVolumeFrequencies.Count];
            for(int n = 0; n < this.CryVolumeFrequencies.Count; ++n){
                double[] cryVolumeFrequency = this.CryVolumeFrequencies[n];
                cors[n] = Correlation.Pearson(volumes, cryVolumeFrequency);
                //Console.WriteLine("corr: "+cor);
            }
            double corMax = cors.Max();
            Console.WriteLine("Max Corr: "+corMax);
            this.RecordedWave.Dispose(); // 放っておくとどんどんメモリを食うのでクリア
            this.RecordedWave = new MemoryStream();
            this.RecordWaveIn?.StartRecording(); // 再開
            if(0.85< corMax){
                return true;
            } else{
                return false;
            }
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
            EventHandler<WaveInEventArgs> writeWW = (_, ee) =>
            {
                WaveFileWriter waveWriter = new WaveFileWriter(this.RecordedCryWave, this.RecordCryWaveIn.WaveFormat);
                waveWriter.Write(ee.Buffer, 0, ee.BytesRecorded);
                waveWriter.Flush();
            };
            this.RecordCryWaveIn.DataAvailable += writeWW;
            this.RecordCryWaveIn.RecordingStopped += (_, __) =>
            {
                WaveFileWriter waveWriter = new WaveFileWriter(this.RecordedCryWave, this.RecordCryWaveIn.WaveFormat);
                waveWriter.Flush();
            };

            this.RecordCryWaveIn?.StartRecording();
        }


        public void StopSamplingCry()
        {
            this.RecordCryWaveIn?.StopRecording();
            this.RecordedCryWave.Seek(0, SeekOrigin.Begin);
            Byte[] waveByte = this.RecordedCryWave.GetBuffer();
            int numFFTSample = waveByte.Length / this.WindowSize;
            for (int n=0; n < numFFTSample; ++n){
                byte[] recorded = new byte[this.WindowSize];
                Array.Copy(RecordedCryWave.GetBuffer(), n*this.WindowSize, recorded, 0, recorded.Length);
                if(recorded.Max() == 0){
                    Console.WriteLine("sample_n:"+ n);
                    return; // 全部０だったらそれ以上やらない。
                }
                this.CryFrequencies.Add(this.FFT(this.RecordedCryWave.GetBuffer(), n*this.WindowSize));
                double[] volumes = new double[this.WindowSize];
                for (int n_volume=0; n_volume < volumes.Length; ++n_volume){
                    volumes[n_volume] = this.CryFrequencies[this.CryFrequencies.Count-1][n_volume].Magnitude;
                }
                this.CryVolumeFrequencies.Add(volumes);
            }
        }
        public Complex[] FFT(Byte[] soundByte){
            return (this.FFT(soundByte, 0));
        }
        public Complex[] FFT(Byte[] soundByte, int idx_start){
            Complex[] fft = new Complex[this.WindowSize];
            byte byte_min = MinMax.Min<byte>(soundByte);
            byte byte_max = MinMax.Max<byte>(soundByte);
            double byte_amp = (double)(byte_max-byte_min);
            for (int n = 0; n < this.WindowSize; ++n){
                int idx = n+idx_start;
                double volume = (double)(soundByte[idx]-byte_min);
                fft[n] = new Complex(volume/byte_amp/Math.Sqrt(this.WindowSize), 0);
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
