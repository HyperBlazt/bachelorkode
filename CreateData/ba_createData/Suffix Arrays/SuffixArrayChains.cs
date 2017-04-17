﻿/*
 Copyright (c) 2012 Eran Meir
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.


 https://github.com/eranmeir/Sufa-Suffix-Array-Csharp d. 11-11-16
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using C5;

namespace ba_createData.Suffix_Arrays
{
    [Serializable]
    public class SuffixArray
    {
        private const int Eoc = int.MaxValue;
        private const string TextFormat = "string_";
        private int[] _mSa;
        private readonly int[] _mIsa;
        private int[] _mLcp;
        private readonly HashDictionary<char, int> _mChainHeadsDict = new HashDictionary<char, int>(new CharComparer());
        private readonly List<Chain> _mChainStack = new List<Chain>();
        private readonly ArrayList<Chain> _mSubChains = new ArrayList<Chain>();
        private int _mNextRank = 1;
        private readonly string _mStr;
        public string StringRepresentation => GetString(_mSa);

        private void CleanSuffixArray()
        {
            this._mSa = _mSa.Where(index => _mStr.Length - 1 != index && index + Properties.Settings.Default.EncryptionLength <= _mStr.Length - 1 && _mStr[index + Properties.Settings.Default.EncryptionLength].Equals('$')).ToArray();
        }

        private static string GetString(IEnumerable<int> bytes)
        {
            var newString = new StringBuilder();
            foreach (var str in bytes)
            {
                newString.Append(str + "; ");
            }
            return newString.ToString();

        }

        ///// <summary>
        ///// Export the suffix array to file for later process
        ///// </summary>
        ///// <param name="directory"></param>
        //private void ExportData(string directory)
        //{

        //    var extension = directory.Split('_');
        //    var extensionName = extension[1];
        //    var filePacementDirectoryTask = Thread.GetDomain().BaseDirectory + "SuffixArrays\\";
        //    if (!File.Exists(filePacementDirectoryTask + TextFormat + extensionName))
        //    {
        //        var stringText = File.CreateText(filePacementDirectoryTask + TextFormat + extensionName);
        //        stringText.Close();
        //    }
        //    using (var file = new StreamWriter(filePacementDirectoryTask + TextFormat + extensionName, true))
        //    {
        //        file.WriteLine(_mStr);
        //    }
        //    Database.BuildSqlSuffixArrayDatabase(_mSa, directory, false);

        //    //var result = string.Join(";", this._mSa);
        //    //File.WriteAllText(directory + fileName, result);
        //}

        /// <summary>
        /// Export lcp data
        /// </summary>
        /// <param name="directory"></param>
        private void ExportLcpData(char directory)
        {

            Database.BuildLcpDatabase(_mLcp, directory, false);

            //var result = string.Join(";", this._mLcp);
            //File.WriteAllText(directory + "lcp_" + fileName, result);
        }

        /// <summary>
        /// Compress the file to save storage space
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Zip(string value)
        {
            //Transform string into byte[]  
            byte[] byteArray = new byte[value.Length];
            int indexBa = 0;
            foreach (char item in value)
            {
                byteArray[indexBa++] = (byte)item;
            }

            //Prepare for compress
            var ms = new MemoryStream();
            var sw = new GZipStream(ms, CompressionMode.Compress);

            //Compress
            sw.Write(byteArray, 0, byteArray.Length);
            sw.Close();

            //Transform byte[] zip data to string
            byteArray = ms.ToArray();
            var sB = new StringBuilder(byteArray.Length);
            foreach (var item in byteArray)
            {
                sB.Append((char)item);
            }
            ms.Close();
            sw.Dispose();
            ms.Dispose();
            return sB.ToString();
        }

        /// 
        /// <summary>
        /// Build a suffix array from string str
        /// </summary>
        /// <param name="str">A string for which to build a suffix array with LCP information</param>
        /// <param name="fileName"></param>
        /// <param name="directory"></param>
        public SuffixArray(string str, string fileName, string directory) : this(str, directory, true) { }

        /// 
        /// <summary>
        /// Build a suffix array from string str
        /// </summary>
        /// <param name="str">A string for which to build a suffix array</param>
        /// <param name="directory"></param>
        /// <param name="buildLcps">Also calculate LCP information</param>
        public SuffixArray(string str, string directory, bool buildLcps)
        {
            _mStr = str;
            _mSa = new int[_mStr.Length];
            _mIsa = new int[_mStr.Length];

            FormInitialChains();
            BuildSuffixArray();
            CleanSuffixArray();
            //ExportData(directory);
            if (buildLcps)
            {
                // Lcp values are build using the reduced suffix array
                BuildLcpArray();
                //ExportLcpData(directory);
            }
        }

        /// <summary>
        /// Binary Search
        /// </summary>
        /// <param name="substr">
        /// The substr.
        /// </param>
        /// <param name="mSa">
        /// The m sa.
        /// </param>
        /// <param name="mStr">
        /// The m str.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int IndexOf(string substr, int[] mSa, string mStr)
        {
            int l = 0;
            int r = mSa.Length;
            int m = -1;

            if ((substr == null) || (substr.Length == 0))
            {
                return -1;
            }

            // Binary search for substring
            while (r > l)
            {
                m = (l + r) / 2;
                if (m < mSa.Length && mSa[m] < mStr.Length && mStr.Substring(mSa[m]).CompareTo(substr) < 0)
                {
                    l = m + 1;
                }
                else
                {
                    r = m;
                }
            }

            if ((l == r) && (l < mStr.Length) && l < mSa.Length && mSa[l] < mStr.Length && mStr.Substring(mSa[l]).StartsWith(substr))
            {
                return mSa[l];
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Link all suffixes that have the same first character
        /// </summary>
        private void FormInitialChains()
        {
            FindInitialChains();
            SortAndPushSubchains();
        }


        /// <summary>
        /// Scan the string left to right, keeping rightmost occurences of characters as the chain heads
        /// </summary>
        private void FindInitialChains()
        {
            for (var i = 0; i < _mStr.Length; i++)
            {
                if (_mChainHeadsDict.Contains(_mStr[i]))
                {
                    _mIsa[i] = _mChainHeadsDict[_mStr[i]];
                }
                else
                {
                    _mIsa[i] = Eoc;
                }
                _mChainHeadsDict[_mStr[i]] = i;
            }

            // Prepare chains to be pushed to stack
            foreach (var headIndex in _mChainHeadsDict.Values)
            {
                var newChain = new Chain(_mStr)
                {
                    Head = headIndex,
                    Length = 1
                };
                _mSubChains.Add(newChain);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void SortAndPushSubchains()
        {
            _mSubChains.Sort();
            for (int i = _mSubChains.Count - 1; i >= 0; i--)
            {
                _mChainStack.Add(_mSubChains[i]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void BuildSuffixArray()
        {
            while (_mChainStack.Count > 0)
            {
                // Pop chain
                var chain = _mChainStack[_mChainStack.Count - 1];
                _mChainStack.RemoveAt(_mChainStack.Count - 1);

                if (_mIsa[chain.Head] == Eoc)
                {
                    // Singleton (A chain that contain only 1 suffix)
                    RankSuffix(chain.Head);
                }
                else
                {
                    //RefineChains(chain);
                    RefineChainWithInductionSorting(chain);
                }
            }
        }

        private void ExtendChain(Chain chain)
        {
            var sym = _mStr[chain.Head + chain.Length];
            if (_mChainHeadsDict.Contains(sym))
            {
                // Continuation of an existing chain, this is the leftmost
                // occurence currently known (others may come up later)
                _mIsa[_mChainHeadsDict[sym]] = chain.Head;
                _mIsa[chain.Head] = Eoc;
            }
            else
            {
                // This is the beginning of a new subchain
                _mIsa[chain.Head] = Eoc;
                var newChain = new Chain(_mStr)
                {
                    Head = chain.Head,
                    Length = chain.Length + 1
                };
                _mSubChains.Add(newChain);
            }
            // Save index in case we find a continuation of this chain
            _mChainHeadsDict[sym] = chain.Head;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="chain"></param>
        private void RefineChainWithInductionSorting(Chain chain)
        {
            var notedSuffixes = new ArrayList<SuffixRank>();
            _mChainHeadsDict.Clear();
            _mSubChains.Clear();

            while (chain.Head != Eoc)
            {
                var nextIndex = _mIsa[chain.Head];
                if (chain.Head + chain.Length > _mStr.Length - 1)
                {
                    // If this substring reaches end of string it cannot be extended.
                    // At this point it's the first in lexicographic order so it's safe
                    // to just go ahead and rank it.
                    RankSuffix(chain.Head);
                }
                else if (_mIsa[chain.Head + chain.Length] < 0)
                {
                    var sr = new SuffixRank
                    {
                        Head = chain.Head,
                        Rank = -_mIsa[chain.Head + chain.Length]
                    };
                    notedSuffixes.Add(sr);
                }
                else
                {
                    ExtendChain(chain);
                }
                chain.Head = nextIndex;
            }
            // Keep stack sorted
            SortAndPushSubchains();
            SortAndRankNotedSuffixes(notedSuffixes);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="notedSuffixes"></param>
        private void SortAndRankNotedSuffixes(C5.IList<SuffixRank> notedSuffixes)
        {
            notedSuffixes.Sort(new SuffixRankComparer());
            // Rank sorted noted suffixes 
            for (var i = 0; i < notedSuffixes.Count; ++i)
            {
                RankSuffix(notedSuffixes[i].Head);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        private void RankSuffix(int index)
        {
            // We use the ISA to hold both ranks and chain links, so we differentiate by setting
            // the sign.
            _mIsa[index] = -_mNextRank;
            _mSa[_mNextRank - 1] = index;
            _mNextRank++;
        }

        /// <summary>
        /// 
        /// </summary>
        private void BuildLcpArray()
        {
            _mLcp = new int[_mSa.Length + 1];
            _mLcp[0] = _mLcp[_mSa.Length] = 0;

            for (int i = 1; i < _mSa.Length; i++)
            {
                _mLcp[i] = CalcLcp(_mSa[i - 1], _mSa[i]);
            }
        }

        private int CalcLcp(int i, int j)
        {

            var first = _mStr.Substring(i, 32);
            var second = _mStr.Substring(j, 32);
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

    #region HelperClasses
    [Serializable]
    internal class Chain : IComparable<Chain>
    {
        public int Head;
        public int Length;
        private readonly string _mStr;

        public Chain(string str)
        {
            _mStr = str;
        }

        public int CompareTo(Chain other)
        {
            return _mStr.Substring(Head, Length).CompareTo(_mStr.Substring(other.Head, other.Length));
        }

        public override string ToString()
        {
            return _mStr.Substring(Head, Length);
        }
    }

    [Serializable]
    internal class CharComparer : System.Collections.Generic.EqualityComparer<char>
    {
        public override bool Equals(char x, char y)
        {
            return x.Equals(y);
        }

        public override int GetHashCode(char obj)
        {
            return obj.GetHashCode();
        }
    }

    [Serializable]
    internal struct SuffixRank
    {
        public int Head;
        public int Rank;
    }

    [Serializable]
    internal class SuffixRankComparer : IComparer<SuffixRank>
    {
        public bool Equals(SuffixRank x, SuffixRank y)
        {
            return x.Rank.Equals(y.Rank);
        }

        public int Compare(SuffixRank x, SuffixRank y)
        {
            return x.Rank.CompareTo(y.Rank);
        }
    }
    #endregion
}
