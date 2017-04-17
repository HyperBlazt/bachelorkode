using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.PropertyGridInternal;
using ba_createData.Collection;
using ba_createData.Scanner;
using ba_createData.Suffix_Arrays;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace ca_createDataTest
{

    [TestClass]
    public class ScanningTest
    {



        /// <summary>
        /// Simple test to insure that Binary Search and Binary Search w LCp works correctly
        /// </summary>
        [TestMethod]
        public void ScanTest()
        {
            ba_createData.Properties.Settings.Default.EncryptionLength = 8;
            const string text =
               "skalskor$sibirien$gabriela$sokrates$belgiens$bicuspid$accorded$jumpsuit$sjalland$bandidos$barefoot$skotland$abrahams$afrodite$gertrude$beaching$sabrinas$gadzooks$girllike$jailbird$abutting$";
            var byteAlignedBigULongArray = new MemoryEfficientByteAlignedBigULongArray(text.Length);
            SuffixArrayInducedSorting.Sufsort(text, byteAlignedBigULongArray, text.Length);
            var suffixArrayuLong = byteAlignedBigULongArray.ToArray();
            var suffixArray = suffixArrayuLong.Select(x => int.Parse(x.ToString())).ToArray();
            var intSuffixArray = MinimizeSuffixArray.Minimize(suffixArray, text);

            //var intLcpArray = BuildLcpArray.Build(intSuffixArray, text);
            var result = !Scanner.IndexOf(intSuffixArray, text, "accorded$").Equals(-1);
            var result4 = text.Contains("accorded$");
            var result1 = BinarySearchUsingLcp.BinarySearchWLcp(text, intSuffixArray, "accorded$");
            var result2 = BinarySearchUsingLcp.BinarySearchWLcp(text, intSuffixArray, "randomss$");
            var result3 = !Scanner.IndexOf(intSuffixArray, text, "randomss$").Equals(-1);

            Assert.AreEqual(true, result);
            Assert.AreEqual(true, result1);
            Assert.AreEqual(false, result2);
            Assert.AreEqual(false, result3);
        }


        /// <summary>
        /// Normal environment
        /// </summary>
        [TestMethod]
        public void ScanUsingSql()
        {

            var saveString = new StringBuilder();
            // Without caching
            ba_createData.Properties.Settings.Default.EncryptionLength = 32;
            ba_createData.Properties.Settings.Default.Test = false;
            ba_createData.Properties.Settings.Default.UseSQL = true;
            ba_createData.Properties.Settings.Default.UseBinaryWLcp = false;

            ba_createData.Properties.Settings.Default.UseRAMOnly = true;
            ba_createData.Properties.Settings.Default.UseHDDOnly = false;
            ba_createData.Properties.Settings.Default.SplitOption = 1;
            ba_createData.Properties.Settings.Default.UseCashing = false;
            var testFolderPath = Thread.GetDomain().BaseDirectory + "\\Testresults";
            var fileLocation = testFolderPath + "\\sqlsearchtest.txt";
            saveString.Append(@"SQL search Without cashing:".ToUpper() + Environment.NewLine + Environment.NewLine);

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var result = Scanner.ScanFile("C495AAE02CE54133A0FEFF7E488C3B96$".ToLower());
            stopWatch.Stop();

            // Write tesresult to file
            saveString.Append(@"Result: " + result + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result1 = Scanner.ScanFile("54697A0CC91DCC69E688FBF1EEE55FD0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result1 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();


            stopWatch.Start();
            var result2 = Scanner.ScanFile("74DFF0B6ECCEA35521A65B9A742EB735$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result2 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result3 = Scanner.ScanFile("3C9AE0FE669D6AF30B11268118C3179A$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result3 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result4 = Scanner.ScanFile("F9FDF8364F7A01247A561C67AE1A4A5B$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result4 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result5 = Scanner.ScanFile("A4C21BE59ECEC716B2F646F8D55AC622$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result5 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result6 = Scanner.ScanFile("f7198035cce341240181d005c7b022e0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result6 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var results7 = Scanner.ScanFile("2d75cc1bf8e57872781f9cd04a529256$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + results7 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            File.WriteAllText(fileLocation, saveString.ToString());
            Assert.AreEqual(false, result1);
            Assert.AreEqual(false, result);
            Assert.AreEqual(false, result3);
            Assert.AreEqual(false, result2);
            Assert.AreEqual(false, result4);
            Assert.AreEqual(false, result5);
            Assert.AreEqual(true, results7);
            Assert.AreEqual(true, result6);
        }

        /// <summary>
        /// Normal environment
        /// </summary>
        [TestMethod]
        public void ScanUsingSqlWithCashing()
        {

            var saveString = new StringBuilder();
            // Without caching
            ba_createData.Properties.Settings.Default.EncryptionLength = 32;
            ba_createData.Properties.Settings.Default.Test = false;
            ba_createData.Properties.Settings.Default.UseSQL = true;
            ba_createData.Properties.Settings.Default.UseBinaryWLcp = false;

            ba_createData.Properties.Settings.Default.UseRAMOnly = true;
            ba_createData.Properties.Settings.Default.UseHDDOnly = false;
            ba_createData.Properties.Settings.Default.SplitOption = 1;
            ba_createData.Properties.Settings.Default.UseCashing = true;
            var testFolderPath = Thread.GetDomain().BaseDirectory + "\\Testresults";
            var fileLocation = testFolderPath + "\\sqlsearchwithcashingtest.txt";
            saveString.Append(@"SQL search with cashing:".ToUpper() + Environment.NewLine + Environment.NewLine);

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var result = Scanner.ScanFile("C495AAE02CE54133A0FEFF7E488C3B96$".ToLower());
            stopWatch.Stop();

            // Write tesresult to file
            saveString.Append(@"Result: " + result + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result1 = Scanner.ScanFile("54697A0CC91DCC69E688FBF1EEE55FD0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result1 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();


            stopWatch.Start();
            var result2 = Scanner.ScanFile("74DFF0B6ECCEA35521A65B9A742EB735$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result2 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result3 = Scanner.ScanFile("3C9AE0FE669D6AF30B11268118C3179A$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result3 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result4 = Scanner.ScanFile("F9FDF8364F7A01247A561C67AE1A4A5B$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result4 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result5 = Scanner.ScanFile("A4C21BE59ECEC716B2F646F8D55AC622$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result5 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result6 = Scanner.ScanFile("f7198035cce341240181d005c7b022e0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result6 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var results7 = Scanner.ScanFile("2d75cc1bf8e57872781f9cd04a529256$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + results7 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            File.WriteAllText(fileLocation, saveString.ToString());
            Assert.AreEqual(false, result1);
            Assert.AreEqual(false, result);
            Assert.AreEqual(false, result3);
            Assert.AreEqual(false, result2);
            Assert.AreEqual(false, result4);
            Assert.AreEqual(false, result5);
            Assert.AreEqual(true, results7);
            Assert.AreEqual(true, result6);

            ScannerCaching.DeleteHashFromCache("C495AAE02CE54133A0FEFF7E488C3B96$".ToLower());
            ScannerCaching.DeleteHashFromCache("54697A0CC91DCC69E688FBF1EEE55FD0$".ToLower());

            ScannerCaching.DeleteHashFromCache("74DFF0B6ECCEA35521A65B9A742EB735$".ToLower());
            ScannerCaching.DeleteHashFromCache("3C9AE0FE669D6AF30B11268118C3179A$".ToLower());

            ScannerCaching.DeleteHashFromCache("F9FDF8364F7A01247A561C67AE1A4A5B$".ToLower());
            ScannerCaching.DeleteHashFromCache("A4C21BE59ECEC716B2F646F8D55AC622$".ToLower());
        }


        /// <summary>
        /// Normal environment
        /// </summary>
        [TestMethod]
        public void ScanUsingBinarySearch()
        {
            ba_createData.Properties.Settings.Default.EncryptionLength = 32;
            ba_createData.Properties.Settings.Default.Test = false;
            var saveString = new StringBuilder();
            // Without caching
            if(MemoryDatabase.SuffixArray == null) MemoryDatabase.LoadDataIntoMemory();
            ba_createData.Properties.Settings.Default.EncryptionLength = 32;
            ba_createData.Properties.Settings.Default.UseSQL = false;
            ba_createData.Properties.Settings.Default.UseBinaryWLcp = false;
            ba_createData.Properties.Settings.Default.UseBinary = true;

            ba_createData.Properties.Settings.Default.UseRAMOnly = true;
            ba_createData.Properties.Settings.Default.UseHDDOnly = false;
            ba_createData.Properties.Settings.Default.SplitOption = 1;
            ba_createData.Properties.Settings.Default.UseCashing = false;
            var testFolderPath = Thread.GetDomain().BaseDirectory + "\\Testresults";
            var fileLocation = testFolderPath + "\\binarysearchtest.txt";
            saveString.Append(@"Binary Search Without cashing:".ToUpper() + Environment.NewLine + Environment.NewLine);

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var result = Scanner.ScanFile("C495AAE02CE54133A0FEFF7E488C3B96$".ToLower());
            stopWatch.Stop();

            // Write tesresult to file
            saveString.Append(@"Result: " + result + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result1 = Scanner.ScanFile("54697A0CC91DCC69E688FBF1EEE55FD0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result1 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();


            stopWatch.Start();
            var result2 = Scanner.ScanFile("74DFF0B6ECCEA35521A65B9A742EB735$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result2 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result3 = Scanner.ScanFile("3C9AE0FE669D6AF30B11268118C3179A$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result3 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result4 = Scanner.ScanFile("F9FDF8364F7A01247A561C67AE1A4A5B$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result4 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result5 = Scanner.ScanFile("A4C21BE59ECEC716B2F646F8D55AC622$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result5 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result6 = Scanner.ScanFile("f7198035cce341240181d005c7b022e0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result6 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var results7 = Scanner.ScanFile("2d75cc1bf8e57872781f9cd04a529256$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + results7 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            File.WriteAllText(fileLocation, saveString.ToString());
            Assert.AreEqual(false, result1);
            Assert.AreEqual(false, result);
            Assert.AreEqual(false, result3);
            Assert.AreEqual(false, result2);
            Assert.AreEqual(false, result4);
            Assert.AreEqual(false, result5);
            Assert.AreEqual(true, results7);
            Assert.AreEqual(true, result6);

            // Save cache
            ScannerCaching.SaveCache();
        }


        /// <summary>
        /// Normal environment
        /// </summary>
        [TestMethod]
        public void ScanUsingBinarySearchWithCashing()
        {

            var saveString = new StringBuilder();
            ba_createData.Properties.Settings.Default.EncryptionLength = 32;
            ba_createData.Properties.Settings.Default.Test = false;
            // Without caching
            if (MemoryDatabase.SuffixArray == null) MemoryDatabase.LoadDataIntoMemory();
            ba_createData.Properties.Settings.Default.EncryptionLength = 32;
            ba_createData.Properties.Settings.Default.UseSQL = false;
            ba_createData.Properties.Settings.Default.UseBinaryWLcp = false;
            ba_createData.Properties.Settings.Default.UseBinary = true;

            ba_createData.Properties.Settings.Default.UseRAMOnly = true;
            ba_createData.Properties.Settings.Default.UseHDDOnly = false;
            ba_createData.Properties.Settings.Default.SplitOption = 1;
            ba_createData.Properties.Settings.Default.UseCashing = true;
            var testFolderPath = Thread.GetDomain().BaseDirectory + "\\Testresults";
            var fileLocation = testFolderPath + "\\binarysearchwithcashingtest.txt";
            saveString.Append(@"Binary Search with cashing:".ToUpper() + Environment.NewLine + Environment.NewLine);

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var result = Scanner.ScanFile("C495AAE02CE54133A0FEFF7E488C3B96$".ToLower());
            stopWatch.Stop();

            // Write tesresult to file
            saveString.Append(@"Result: " + result + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result1 = Scanner.ScanFile("54697A0CC91DCC69E688FBF1EEE55FD0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result1 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();


            stopWatch.Start();
            var result2 = Scanner.ScanFile("74DFF0B6ECCEA35521A65B9A742EB735$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result2 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result3 = Scanner.ScanFile("3C9AE0FE669D6AF30B11268118C3179A$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result3 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result4 = Scanner.ScanFile("F9FDF8364F7A01247A561C67AE1A4A5B$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result4 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result5 = Scanner.ScanFile("A4C21BE59ECEC716B2F646F8D55AC622$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result5 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result6 = Scanner.ScanFile("f7198035cce341240181d005c7b022e0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result6 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var results7 = Scanner.ScanFile("2d75cc1bf8e57872781f9cd04a529256$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + results7 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            File.WriteAllText(fileLocation, saveString.ToString());
            Assert.AreEqual(false, result1);
            Assert.AreEqual(false, result);
            Assert.AreEqual(false, result3);
            Assert.AreEqual(false, result2);
            Assert.AreEqual(false, result4);
            Assert.AreEqual(false, result5);
            Assert.AreEqual(true, results7);
            Assert.AreEqual(true, result6);

            // Save cache
            ScannerCaching.DeleteHashFromCache("C495AAE02CE54133A0FEFF7E488C3B96$".ToLower());
            ScannerCaching.DeleteHashFromCache("54697A0CC91DCC69E688FBF1EEE55FD0$".ToLower());

            ScannerCaching.DeleteHashFromCache("74DFF0B6ECCEA35521A65B9A742EB735$".ToLower());
            ScannerCaching.DeleteHashFromCache("3C9AE0FE669D6AF30B11268118C3179A$".ToLower());

            ScannerCaching.DeleteHashFromCache("F9FDF8364F7A01247A561C67AE1A4A5B$".ToLower());
            ScannerCaching.DeleteHashFromCache("A4C21BE59ECEC716B2F646F8D55AC622$".ToLower());

        }

        /// <summary>
        /// Normal environment
        /// </summary>
        [TestMethod]
        [Ignore]
        public void ScanUsingBinarySearchLcp()
        {
            ba_createData.Properties.Settings.Default.Test = false;
            ba_createData.Properties.Settings.Default.EncryptionLength = 32;
            var saveString = new StringBuilder();
            // Without caching
            if (MemoryDatabase.SuffixArray == null) MemoryDatabase.LoadDataIntoMemory();
            ba_createData.Properties.Settings.Default.EncryptionLength = 33;
            ba_createData.Properties.Settings.Default.UseSQL = false;
            ba_createData.Properties.Settings.Default.UseBinaryWLcp = true;
            ba_createData.Properties.Settings.Default.UseBinary = false;

            ba_createData.Properties.Settings.Default.UseRAMOnly = true;
            ba_createData.Properties.Settings.Default.UseHDDOnly = false;
            ba_createData.Properties.Settings.Default.SplitOption = 1;
            ba_createData.Properties.Settings.Default.UseCashing = false;
            var testFolderPath = Thread.GetDomain().BaseDirectory + "\\Testresults";
            var fileLocation = testFolderPath + "\\binarysearchlcptest.txt";
            saveString.Append(@"Binary Search w. LCP Without cashing:".ToUpper() + Environment.NewLine + Environment.NewLine);

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var result = Scanner.ScanFile("C495AAE02CE54133A0FEFF7E488C3B96$".ToLower());
            stopWatch.Stop();

            // Write tesresult to file
            saveString.Append(@"Result: " + result + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result1 = Scanner.ScanFile("54697A0CC91DCC69E688FBF1EEE55FD0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result1 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();


            stopWatch.Start();
            var result2 = Scanner.ScanFile("74DFF0B6ECCEA35521A65B9A742EB735$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result2 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result3 = Scanner.ScanFile("3C9AE0FE669D6AF30B11268118C3179A$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result3 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result4 = Scanner.ScanFile("F9FDF8364F7A01247A561C67AE1A4A5B$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result4 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result5 = Scanner.ScanFile("A4C21BE59ECEC716B2F646F8D55AC622$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result5 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result6 = Scanner.ScanFile("f7198035cce341240181d005c7b022e0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result6 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var results7 = Scanner.ScanFile("2d75cc1bf8e57872781f9cd04a529256$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + results7 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            File.WriteAllText(fileLocation, saveString.ToString());

            Assert.AreEqual(false, result1);
            Assert.AreEqual(false, result);
            Assert.AreEqual(false, result3);
            Assert.AreEqual(false, result2);
            Assert.AreEqual(false, result4);
            Assert.AreEqual(false, result5);
            Assert.AreEqual(true, results7);
            Assert.AreEqual(true, result6);
        }


        /// <summary>
        /// Normal environment
        /// </summary>
        [TestMethod]
        [Ignore]
        public void ScanUsingBinarySearchLcpWithCashing()
        {
            ba_createData.Properties.Settings.Default.Test = false;
            ba_createData.Properties.Settings.Default.EncryptionLength = 32;
            var saveString = new StringBuilder();
            // Without caching
            if (MemoryDatabase.SuffixArray == null) MemoryDatabase.LoadDataIntoMemory();
            ba_createData.Properties.Settings.Default.EncryptionLength = 32;
            ba_createData.Properties.Settings.Default.UseSQL = false;
            ba_createData.Properties.Settings.Default.UseBinaryWLcp = true;
            ba_createData.Properties.Settings.Default.UseBinary = false;

            ba_createData.Properties.Settings.Default.UseRAMOnly = true;
            ba_createData.Properties.Settings.Default.UseHDDOnly = false;
            ba_createData.Properties.Settings.Default.SplitOption = 1;
            ba_createData.Properties.Settings.Default.UseCashing = true;
            var testFolderPath = Thread.GetDomain().BaseDirectory + "\\Testresults";
            var fileLocation = testFolderPath + "\\binarysearchlcpwithsachingtest.txt";
            saveString.Append(@"Binary Search w. LCP with cashing:".ToUpper() + Environment.NewLine + Environment.NewLine);

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var result = Scanner.ScanFile("C495AAE02CE54133A0FEFF7E488C3B96$".ToLower());
            stopWatch.Stop();

            // Write tesresult to file
            saveString.Append(@"Result: " + result + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result1 = Scanner.ScanFile("54697A0CC91DCC69E688FBF1EEE55FD0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result1 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();


            stopWatch.Start();
            var result2 = Scanner.ScanFile("74DFF0B6ECCEA35521A65B9A742EB735$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result2 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result3 = Scanner.ScanFile("3C9AE0FE669D6AF30B11268118C3179A$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result3 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result4 = Scanner.ScanFile("F9FDF8364F7A01247A561C67AE1A4A5B$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result4 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result5 = Scanner.ScanFile("A4C21BE59ECEC716B2F646F8D55AC622$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result5 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var result6 = Scanner.ScanFile("f7198035cce341240181d005c7b022e0$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + result6 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            stopWatch.Start();
            var results7 = Scanner.ScanFile("2d75cc1bf8e57872781f9cd04a529256$".ToLower());
            stopWatch.Stop();
            saveString.Append(@"Result: " + results7 + @" - found in " + stopWatch.Elapsed + Environment.NewLine);
            stopWatch.Reset();

            File.WriteAllText(fileLocation, saveString.ToString());

            Assert.AreEqual(false, result1);
            Assert.AreEqual(false, result);
            Assert.AreEqual(false, result3);
            Assert.AreEqual(false, result2);
            Assert.AreEqual(false, result4);
            Assert.AreEqual(false, result5);
            Assert.AreEqual(true, results7);
            Assert.AreEqual(true, result6);
        }
    }
}
