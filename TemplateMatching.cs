using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Statistics;

namespace Matching
{
    class TemplateMatching
    {
        
        /// <summary>
        /// imgにpatが含まれているか調査します。
        /// </summary>
        /// <param name="img"></param>
        /// <param name="pat"></param>
        /// <returns></returns>
        public double FastZNCC(ref Complex[][] img, ref Complex[][] pat){            
            if (img.Length < pat.Length || img[0].Length < pat[0].Length){
                throw new ArgumentException("pat must be smaller than img.");
            }

            /// <summary>
            ///  未着手
            /// </summary>
            Complex[][] imgFFT = FFT2D(ref img);
            Complex[][] patFFT = FFT2D(ref pat);

            // Combolution
            for(int nx = 0; nx < imgFFT.Length; ++nx){
                for(int ny = 0; ny < imgFFT[0].Length; ++ny){
                    imgFFT[nx][ny] = imgFFT[nx][ny]*Complex.Conjugate(patFFT[nx][ny]);
                }
            }
            return 1;
        }

        /// <summary>
        /// Row-Column法で２次元FFTを行う
        /// </summary>
        /// <param name="origin">FFTしたい配列。DeepCopyするので、配列は壊さない</param>
        /// <returns>FFT結果</returns>
        public Complex[][] FFT2D(ref Complex[][] origin){
            Complex[][] ffts = (Complex[][])origin.Clone();
            foreach(Complex[] fft in ffts){
                Fourier.Forward(fft, FourierOptions.Matlab);
            }
            /// <summary>
            ///  列のFFＴは未着手
            /// </summary>
            return ffts;
        }

        /// <summary>
        /// 転置する。
        /// @kskhsn_kskhsn15さんのソースをそのまま拝借(https://qiita.com/kskhsn_kskhsn15/items/0c42f560fe20259ff83f)
        /// </summary>
        /// <param name="values"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<IEnumerable<T>> Tranpose<T>(IEnumerable<IEnumerable<T>> values){
            return Enumerable.Range(0, values.Max(c => c.Count())).Select(i => values.Select(c => i < c.Count() ? c.ElementAt(i) : default(T)));
        }

        public static int CalcNearestPower2(int num){
            int res = 1;
            while(res < num){
                res*=2;
            }
            return res;
        }
    }
}
