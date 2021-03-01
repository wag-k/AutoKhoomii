using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

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


    }
}
