using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ba_createData;
using ba_createData.Collection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ca_createDataTest
{
    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void DatabaseInsertion()
        {
            // SET AS TEST ENVIRONMENT, THIS WILL TRIGGER TEST MODE
            // CHOOSING TEST DATABASE
            ba_createData.Properties.Settings.Default.Test = true;

            // USE RAM ONLY
            ba_createData.Properties.Settings.Default.UseRAMOnly = true;

            // USE CACHING
            ba_createData.Properties.Settings.Default.UseCashing = true;

            // DEFINE EncryptionLength AS THE LENGTH OF THE INDIVIDUAL SUBSTRINGS IN TEST
            // ONLY ACCEPT FIXED LENGTH STRINGS
            ba_createData.Properties.Settings.Default.EncryptionLength = 8;

            const string text =
                "skalskor$sibirien$gabriela$sokrates$belgiens$bicuspid$accorded$jumpsuit$sjalland$bandidos$barefoot$skotland$abrahams$afrodite$gertrude$beaching$sabrinas$gadzooks$girllike$jailbird$abutting$";

            var dictHolder = new Dictionary<char, string[]>();
            // We sort them into buckets
            var splitArray = text.Split('$');
            foreach (var stringVariable in splitArray)
            {
                if (!stringVariable.Equals(string.Empty))
                {
                    var getPrefix = Convert.ToChar(stringVariable[0]);

                    //Get dict for prefix if it exists
                    if (dictHolder.ContainsKey(getPrefix))
                    {
                        var currentTextArrayForPrefix = dictHolder[getPrefix].ToList();
                        currentTextArrayForPrefix.Add(stringVariable);
                        dictHolder[getPrefix] = currentTextArrayForPrefix.ToArray();
                    }
                    // Dict does not hold any information on the current prefix
                    else
                    {
                        var stringList = new List<string>();
                        stringList.Add(stringVariable);
                        dictHolder.Add(getPrefix, stringList.ToArray());
                    }
                }
            }

            // Clear tables
            Database.ClearTable("TEXT_a");
            Database.ClearTable("TEXT_s");
            Database.ClearTable("TEXT_g");
            Database.ClearTable("TEXT_b");
            Database.ClearTable("TEXT_j");
            Database.ClearTable("SUFFIXARRAY_s");
            Database.ClearTable("SUFFIXARRAY_a");
            Database.ClearTable("SUFFIXARRAY_g");
            Database.ClearTable("SUFFIXARRAY_b");
            Database.ClearTable("SUFFIXARRAY_j");
            Database.ClearTable("LCPARRAY_s");
            Database.ClearTable("LCPARRAY_a");
            Database.ClearTable("LCPARRAY_g");
            Database.ClearTable("LCPARRAY_b");
            Database.ClearTable("LCPARRAY_j");


            // INSERT VALUES INTO DATABSE
            foreach (KeyValuePair<char, string[]> entry in dictHolder)
            {
                var stringList = entry.Value;
                var concatText = stringList.Select(r => string.Concat(r, '$')).ToList();
                var completeText = string.Join(string.Empty, concatText);
                // do something with entry.Value or entry.Key
                Database.InsertTextIntoDatabase(completeText, entry.Key);
                // GET INFORMATION BACK FROM DATABASE
                var databaseText = Database.GetTextFromDatabase(entry.Key);
                //var databaseText = "mmiissiissiippii$";
                var byteAlignedBigULongArray = new MemoryEfficientByteAlignedBigULongArray(databaseText.Length);
                ba_createData.Suffix_Arrays.SuffixArrayInducedSorting.Sufsort(databaseText, byteAlignedBigULongArray,
                    databaseText.Length);
                var suffixArrayuLong = byteAlignedBigULongArray.ToArray();
                var suffixArray = suffixArrayuLong.Select(x => int.Parse(x.ToString())).ToArray();

                var intSuffixArray = ba_createData.Suffix_Arrays.MinimizeSuffixArray.Minimize(suffixArray, databaseText);
                var intLcpArray = ba_createData.Suffix_Arrays.BuildLcpArray.Build(intSuffixArray, databaseText);
                //var suffixArray = new SuffixArray(databaseText, true);
                //var lowestCommonPrefix = suffixArray._mLcp;
                //var array = suffixArray._mSa;

                // PLACE THEM IN DATABASE
                Database.BuildSqlSuffixArrayDatabase(intSuffixArray, entry.Key, true);
                Database.BuildLcpDatabase(intLcpArray, entry.Key, true);
            }
        }

        [TestMethod]
        public void DatabaseExtraction()
        {
            // Insert new database
            DatabaseInsertion();
            var aLcpArray = Database.GetLcpTableByPattern('a');
            CollectionAssert.AreEqual(new [] { 0, 2, 1, 1, 0 }, aLcpArray);
            var bLcpArray = Database.GetLcpTableByPattern('b');
            CollectionAssert.AreEqual(new List<int> { 0, 2, 1, 2, 1, 0 }, bLcpArray);
            var jLcpArray = Database.GetLcpTableByPattern('j');
            CollectionAssert.AreEqual(new List<int> { 0, 1, 0 }, jLcpArray);
            var gLcpArray = Database.GetLcpTableByPattern('g');
            CollectionAssert.AreEqual(new List<int> { 0, 2, 1, 1, 0 }, gLcpArray);
            var sLcpArray = Database.GetLcpTableByPattern('s');
            CollectionAssert.AreEqual(new List<int> { 0, 1, 1, 1, 2, 1, 0 }, sLcpArray);

            var aSuffixArray = Database.GetSuffixArrayTableByPattern('a');
            CollectionAssert.AreEqual(new List<int> { 9, 27, 0, 18 }, aSuffixArray['a']);
            var bSuffixArray = Database.GetSuffixArrayTableByPattern('b');
            CollectionAssert.AreEqual(new List<int> { 18, 27, 36, 0, 9}, bSuffixArray['b']);
            var jSuffixArray = Database.GetSuffixArrayTableByPattern('j');
            CollectionAssert.AreEqual(new List<int> { 9, 0 }, jSuffixArray['j']);
            var gSuffixArray = Database.GetSuffixArrayTableByPattern('g');
            CollectionAssert.AreEqual(new List<int> { 0, 18, 9, 27 }, gSuffixArray['g']);
            var sSuffixArray = Database.GetSuffixArrayTableByPattern('s');
            CollectionAssert.AreEqual(new List<int> { 45, 9, 27, 0, 36, 18 }, sSuffixArray['s']);

            var aText = Database.GetTextFromDatabase('a');
            Assert.AreEqual("accorded$abrahams$afrodite$abutting$", aText);
            var bText = Database.GetTextFromDatabase('b');
            Assert.AreEqual("belgiens$bicuspid$bandidos$barefoot$beaching$", bText);
            var jText = Database.GetTextFromDatabase('j');
            Assert.AreEqual("jumpsuit$jailbird$", jText);
            var gText = Database.GetTextFromDatabase('g');
            Assert.AreEqual("gabriela$gertrude$gadzooks$girllike$", gText);
            var sText = Database.GetTextFromDatabase('s');
            Assert.AreEqual("skalskor$sibirien$sokrates$sjalland$skotland$sabrinas$", sText);
        }
    }
}
