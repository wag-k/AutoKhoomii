using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Statistics;
using NAudio.Wave;
using NAudio.Codecs;
using NAudio.CoreAudioApi;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

using Matching;
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
        /// <summary>
        /// 時間ごとのスペクトル分布
        /// </summary>
        /// <value></value>
        double[][] TimeFFTDatas{get;set;}
        public int WindowSize{get;set;}
        public int SamplingRate{get;set;}

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
        public List<Mat> CryImages;
        public double[][] CryVolumeFrequencies{get;set;}
        public AutoResetEvent AutoResetEvent{get;set;}
        public BabyCryDetector(){
            this.WindowSize = 4096*2; // FFTするので必ず２の累乗にしてください。
            this.SamplingRate = 44100;
            this.CryFrequencies = new List<Complex[]>();
            this.CryImages = new List<Mat>();
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
            waveInEvent.WaveFormat = new WaveFormat(this.SamplingRate, WaveInEvent.GetCapabilities(deviceNumber).Channels);
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
            this.RecordWaveIn.StopRecording();
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
                    //Console.WriteLine(e.Message); // なぜかStopしていないことがある。
                }
                return false;
            }
            //bool isDetected = DetectCryBySimpleXCorr(ref sound);
            bool isDetected = DetectCryByLineZNCC(ref sound, this.WindowSize);
            
            this.RecordedWave.Dispose(); // 放っておくとどんどんメモリを食うのでクリア
            this.RecordedWave = new MemoryStream();
            this.RecordWaveIn.StartRecording(); // 再開

            return isDetected;
        }


        private bool DetectCryBySimpleXCorr(ref Byte[] sound){
            this.FftData = this.FFT(sound); // 直近の音を使う
            double[] volumes = new double[this.WindowSize];
            for (int n_volume=0; n_volume < volumes.Length; ++n_volume){
                volumes[n_volume] = this.FftData[n_volume].Magnitude;
            }
            double[] cors = new double[this.CryVolumeFrequencies.Length];
            for(int n = 0; n < this.CryVolumeFrequencies.Length; ++n){
                double[] cryVolumeFrequency = this.CryVolumeFrequencies[n];
                cors[n] = Correlation.Pearson(volumes, cryVolumeFrequency);
                //Console.WriteLine("corr: "+cor);
            }
            double corMax = cors.Max();
            Console.WriteLine("Max Corr: "+corMax);
            if(0.85< corMax){
                return true;
            } else{
                return false;
            }
        }

        private bool DetectCryByLineZNCC(ref Byte[] sound, int windowSize){
            int numSample = sound.Length / windowSize;
            List<double> trimmedSound = new List<double>();
            for (int n=0; n < numSample; ++n){
                double[] volumes = new double[windowSize];
                for (int n_volume=0; n_volume < windowSize; ++n_volume){
                    volumes[n_volume] = (double)sound[n*windowSize+n_volume];
                }
                if (volumes.Max() == 0){
                    Console.WriteLine("sample_n:"+ n);
                    break; // 全部０だったらそれ以上やらない。
                }
                trimmedSound.AddRange(volumes);
            }

            Mat soundImage = new Mat(sound.Length, 1, MatType.CV_8UC1, sound);
            double maxXCorr = 0;
            foreach(Mat cryImage in this.CryImages){
                Mat res = new Mat();
                Cv2.MatchTemplate(soundImage, cryImage, res, TemplateMatchModes.CCoeffNormed);

                for (int nrow = 0; nrow < res.Rows; ++nrow){
                    maxXCorr = maxXCorr < res.At<float>(nrow,1) ? res.At<float>(nrow, 1) : maxXCorr; 
                }
                //比較データ(配列)のうち、しきい値0.85以下を排除(0)にする
                OpenCvSharp.Cv2.Threshold(res, res, 0.65, 1.0, ThresholdTypes.Tozero);
                for(int nx = 0; nx < res.Rows; ++nx){
                    for(int ny = 0; ny < res.Cols; ++ny){
                        if(0 < res.At<float>(nx, ny)){
                            Console.WriteLine("Pattern Detected!");
                            return true;
                        }
                    }

                } 
            }
            
            Console.WriteLine("Pattern Not Included. Max XCorr: "+maxXCorr);
            return false;
        }
        private bool DetectCryByFastZNCC(ref Byte[] sound){
            List<double[]> timeFFTDatasList = WaveToFFTProfile(ref sound);
            this.TimeFFTDatas = timeFFTDatasList.ToArray();
            /*
            if(0.85< corMax){
                return true;
            } else{
                return false;
            }
            */
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
            // List<double[]> cryVolumeFrequenciesList = WaveToFFTProfile(ref waveByte);
            this.CryImages = WaveToSoundChunc(ref waveByte, this.WindowSize);
            //this.CryVolumeFrequencies = cryVolumeFrequenciesList.ToArray();
        }

        /// <summary>
        /// WaveSoundをWindowSize毎の塊で分けてdouble[]のListにします。
        /// </summary>
        /// <param name="waveByte"></param>
        /// <returns></returns>
        private List<Mat> WaveToSoundChunc(ref byte[] waveByte, int windowSize){
            int numFFTSample = waveByte.Length / windowSize;
            List<Mat> cryImages = new List<Mat>();
            for (int n=0; n < numFFTSample; ++n){
                byte[] volumes = new byte[windowSize];
                for (int n_volume=0; n_volume < windowSize; ++n_volume){
                    volumes[n_volume] = waveByte[n*windowSize+n_volume];
                }
                if (volumes.Max() == 0){
                    Console.WriteLine("sample_n:"+ n);
                    break; // 全部０だったらそれ以上やらない。
                }
                Mat cryImage = new Mat(volumes.Length, 1, MatType.CV_8UC1, volumes);
                cryImages.Add(cryImage);
                //Console.WriteLine(cryImage.Rows);
            }
            return cryImages;
        }

        private List<double[]> WaveToFFTProfile(ref byte[] waveByte){
            int numFFTSample = waveByte.Length / this.WindowSize;
            List<Complex[]> cryFrequencies = new List<Complex[]>();
            List<double[]> cryVolumeFrequenciesList = new List<double[]>();
            for (int n=0; n < numFFTSample; ++n){
                byte[] recorded = new byte[this.WindowSize];
                Array.Copy(RecordedCryWave.GetBuffer(), n*this.WindowSize, recorded, 0, recorded.Length);
                if(recorded.Max() == 0){
                    Console.WriteLine("sample_n:"+ n);
                    break; // 全部０だったらそれ以上やらない。
                }
                cryFrequencies.Add(this.FFT(this.RecordedCryWave.GetBuffer(), n*this.WindowSize));
                double[] volumes = new double[this.WindowSize];
                for (int n_volume=0; n_volume < volumes.Length; ++n_volume){
                    volumes[n_volume] = cryFrequencies[cryFrequencies.Count-1][n_volume].Magnitude;
                }
                cryVolumeFrequenciesList.Add(volumes);
            }
            return cryVolumeFrequenciesList;
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
