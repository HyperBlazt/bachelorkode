using System.Linq;

namespace ba_createData.Suffix_Arrays
{
    /// <summary>
    /// Minimize the suffix, removing unwanted suffies
    /// </summary>
    public static class MinimizeSuffixArray
    {
        /// <summary>
        /// Minimize the suffix, removing unwanted suffies
        /// </summary>
        /// <param name="suffixArray"></param>
        /// <param name="text"></param>
        public static int[] Minimize(int[] suffixArray, string text)
        {
            var textLength = text.Length;
            var minimizedSuffixArray = suffixArray.Where(index => textLength - 1 != index && index + Properties.Settings.Default.EncryptionLength <= textLength - 1 && text[index + Properties.Settings.Default.EncryptionLength].Equals('$')).ToArray();
            return minimizedSuffixArray;
        }

    }
}
