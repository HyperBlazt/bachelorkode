using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ba_createData.Suffix_Arrays
{
    /// <summary>
    /// 
    /// </summary>
    public static class BuildLcpArray
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="suffixArray"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int[] Build(int[] suffixArray, string text)
        {
            var _mLcp = new int[suffixArray.Length + 1];
            _mLcp[0] = _mLcp[suffixArray.Length] = 0;

            for (int i = 0; i < suffixArray.Length -1 ; i++)
            {
                _mLcp[i] = CalcLcp(suffixArray[i], suffixArray[i+1], text);
            }
            return _mLcp;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static int CalcLcp(int i, int j, string text)
        {
            var length = Properties.Settings.Default.EncryptionLength;
            var first = text.Substring(i, length);
            var second = text.Substring(j, length);
            var lcp = 0;
            for (var k = 0; k < first.Length - 1; k++)
            {
                if (first[k].Equals(second[k]))
                {
                    lcp++;
                }
                else
                {
                    break;
                }
            }
            //int lcp;
            //int maxIndex = _mStr.Length - Math.Max(i, j); // Out of bounds prevention
            //for (lcp = 0; (lcp < maxIndex) && (_mStr[i + lcp] == _mStr[j + lcp]); lcp++)
            //{
            //}
            return lcp;
        }
    }
}
