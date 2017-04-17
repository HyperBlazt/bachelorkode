using System;

namespace ba_createData.Scanner
{
    public static class BinarySearchUsingLcp
    {
        /// <summary>
        /// 
        /// </summary>
        public static int[] BuildLcpArray(int[] mSa, string mStr)
        {
            var mLcp = new int[mSa.Length + 1];
            mLcp[0] = mLcp[mSa.Length] = 0;

            for (int i = 1; i < mSa.Length; i++)
            {
                mLcp[i] = CalcLcp(mSa[i - 1], mSa[i], mStr);
            }
            return mLcp;
        }

        private static int CalcLcp(int i, int j, string mStr)
        {
            int lcp;
            var maxIndex = mStr.Length - Math.Max(i, j); // Out of bounds prevention
            for (lcp = 0; (lcp < maxIndex) && (mStr[i + lcp] == mStr[j + lcp]); lcp++)
            {
            }
            return lcp;
        }

        // It works assuming you have builded the concatenated string and
        // computed the suffix and the lcp arrays
        // text.length() ---> tlen
        // pattern.length() ---> plen
        // concatenated string: str

        public static bool BinarySearchLcp(string mStr, int[] sa, int[] lcp, string pattern)
        {
            var plen = pattern.Length;
            var tlen = mStr.Length;
            var total = tlen + plen;
            var pos = -1;
            for (var i = 0; i < total; ++i)
                if (total - sa[i] == plen)
                { pos = i; break; }
            if (pos == -1) return false;
            int hi;
            var lo = hi = pos;
            while (lo - 1 >= 0 && lcp[lo - 1] >= plen) lo--;
            while (hi + 1 < tlen && lcp[hi] >= plen) hi++;
            for (var i = lo; i <= hi; ++i)
                if (total - sa[i] >= 2 * plen)
                    return true;
            return false;
        }

        public static bool BinarySearchWLcp(string mStr, int[] mSa, string substr)
        {

            var bigL = 0;
            var bigR = mSa.Length - 1;
            var bigM = (bigL + bigR) / 2;
            var concat = mStr + substr;

            if ((substr == null) || (substr.Length == 0))
            {
                return false;
            }
            // longest prefix that match P left
            // ReSharper disable once StringCompareToIsCultureSpecific
            var l = CalcLcp(mSa[bigL], mStr.Length, concat);

            // longest prefix that match P right
            // ReSharper disable once StringCompareToIsCultureSpecific
            var r = CalcLcp(mSa[bigR], mStr.Length, concat);
            var mrl = Math.Min(l, r);

            // Binary search for substring
            while (true)
            {
                bigM = (bigL + bigR) / 2;

                //Case one
                var lcp = CalcLcp(mSa[bigL], mSa[bigM], mStr);
                if (r == l)
                {
                    // ReSharper disable once StringCompareToIsCultureSpecific
                    var compare = mStr.Substring(mSa[bigM]).CompareTo(substr);
                    if (compare == 1)
                    {
                        //right we go
                        bigL = bigM;
                    }
                    else if (compare == -1)
                    {
                        bigR = bigM;
                        r = lcp;
                    }
                }
                else if (lcp > l)
                {
                    if (mStr.Substring(mSa[bigM]).StartsWith(substr))
                    {
                        return true;
                    }
                    if (bigL == bigM || bigM == bigR)
                    {
                        return false;
                    }
                    bigL = bigM;
                }
                else if (lcp < l)
                {
                    r = lcp;
                    bigR = bigM;
                }
                else if (lcp == l)
                {
                    for (int i = bigM; i < bigR; i++)
                    {
                        if (mStr.Substring(mSa[bigM]).StartsWith(substr))
                        {
                            return true;
                        }
                    }
                    // Its not in the list
                    return false;
                }
            }
        }
    }
}
