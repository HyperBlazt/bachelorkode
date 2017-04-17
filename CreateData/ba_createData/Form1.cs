/*
 Copyright (c) 2016 Mark Roland, University of Copenhagen, Department of Computer Science
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
*/


using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ba_createData.Collection;
using ba_createData.Scanner;
using ba_createData.Server;
using ba_createData.Suffix_Arrays;
using NUnit.Framework.Constraints;

namespace ba_createData
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    /// The form 1.
    /// </summary>
    public partial class Form1 : Form
    {

        private TcpListener _tcpListener;

        private Thread _listenThread;

        private bool ServerIsActive = false;

        private MainServerControl CurrentServer;


        /// <summary>
        /// The set text callback.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public delegate void SetTextCallback(string text);

        /// <summary>
        /// The set text.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (MainTextArea.InvokeRequired)
            {
                var d = new SetTextCallback(SetText);
                Invoke(d, text);
            }
            else
            {
                MainTextArea.Text += text;
            }
        }


        /// <summary>
        /// The set text.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public void SetScanText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (MainTextArea.InvokeRequired)
            {
                var d = new SetTextCallback(SetScanText);
                Invoke(d, text);
            }
            else
            {
                richTextBox1.Text += text;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void SetMainTime(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (MainTextArea.InvokeRequired)
            {
                SetTextCallback d = SetMainTime;
                Invoke(d, text);
            }
            else
            {
                MainTextArea.Text += text;
            }
        }

        /// <summary>
        /// The _reset event.
        /// </summary>
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);

        /// <summary>
        /// The build suffix array.
        /// </summary>
        private readonly BackgroundWorker _buildSuffixArray = new BackgroundWorker();

        /// <summary>
        /// The build suffix array.
        /// </summary>
        private readonly BackgroundWorker _buildSuffixTree = new BackgroundWorker();

        /// <summary>
        /// The object used for cancelling tasks.
        /// </summary>
        private CancellationTokenSource _mCancelTokenSource;

        private int GetSplitOption { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class. 
        /// The form 1.
        /// </summary>
        public Form1()
        {

            InitializeComponent();
            InitializeWorkerProcesses();


            // Applying default settings
            // Default options are found by right-clicking ba_createData in the Project Panel, and selecting Settings
            LoadSavedDefaultSetting();

            Credentials.Add("USER", "1234");

            var splitOption = new[] { "0", "1", "2", "3", "4", "5" };
            comboBox1.DataSource = splitOption;

            // Set split option to 1 as standard value
            comboBox1.SelectedIndex = comboBox1.FindString("1");

            SetMainTime("Welcome to Malware Detection System Database" + Environment.NewLine);
        }

        /// <summary>
        /// Default options are found by right-clicking ba_createData in the Project Panel, and selecting Settings
        /// </summary>
        private void LoadSavedDefaultSetting()
        {

            UseRAMOnly.Checked = Properties.Settings.Default.UseRAMOnly;
            UseHHDOnly.Checked = Properties.Settings.Default.UseHDDOnly;
            comboBox1.SelectedItem = Properties.Settings.Default.SplitOption;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.UseRAMOnly)
            {
                // Load files into ram
                SetText(MemoryDatabase.LoadDataIntoMemory() ? "Database loaded into memory" + Environment.NewLine : "Database load failed..." + Environment.NewLine);
            }
        }

        /// <summary>
        /// The initialize worker processes.
        /// </summary>
        private void InitializeWorkerProcesses()
        {
            this.createDataFile.WorkerReportsProgress = true;
            this.createDataFile.WorkerSupportsCancellation = true;
            this.createDataFile.DoWork += CreateDatafileDoWork;

            this._buildSuffixArray.WorkerReportsProgress = true;
            this._buildSuffixArray.WorkerSupportsCancellation = true;
            this._buildSuffixArray.DoWork += CreateSuffixArrayDoWork;

            this._buildSuffixTree.WorkerReportsProgress = true;
            this._buildSuffixTree.WorkerSupportsCancellation = true;
            this._buildSuffixTree.DoWork += CreateSuffixTreeDoWork;
        }

        /// <summary>
        /// The cancel.
        /// </summary>
        public void Cancel()
        {
            createDataFile.CancelAsync();
            _resetEvent.WaitOne();
        }

        /// <summary>
        /// The create datafile_ do work.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CreateDatafileDoWork(object sender, DoWorkEventArgs e)
        {
            SetText("Updating database...." + Environment.NewLine);
            while (!e.Cancel)
            {
                var buildingData = new UpdateData();
                buildingData.UpdateDataFile();
                Cancel();
            }

            _resetEvent.WaitOne();

        }

        /// <summary>
        /// Builds suffix arrays and append them to a database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateSuffixArrayDoWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                // Init - Load files from database?
                //var malwareTableNames = Database.GetMalwareTableNames();
                //malwareTableNames.Sort();

                _mCancelTokenSource = new CancellationTokenSource();

                // Get a reference to the cancellation token.
                var readFileCancelToken = _mCancelTokenSource.Token;

                // I am iterating through the list of table names to use one task per table to do them in parallel.
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                Task buildSuffixArrayTask = null;

                // Iterate over malware datatables
                //foreach (var table in malwareTableNames)
                //{
                //var prefix = table.Split('_');
                //var text = Database.GetMalwarValuesByTablename(table);
                buildSuffixArrayTask = Task.Factory.StartNew(() =>
                {
                    //var byteAlignedBigULongArray = new MemoryEfficientByteAlignedBigULongArray(text.Length);
                    //Suffix_Arrays.SuffixArrayInducedSorting.Sufsort(text, byteAlignedBigULongArray,
                    //    text.Length);

                    var dataDirectory = Thread.GetDomain().BaseDirectory + "Data";
                    string[] fileEntries = Directory.GetFiles(dataDirectory);
                    foreach (var file in fileEntries)
                    {
                        var fileName = Path.GetFileName(file);
                        if (fileName != null)
                        {
                            var prefix = fileName.Split('.')[0];
                            var text = DataBaseToText(file);
                            // TODO: REMOVE THIS!!!!
                            var suffixArray = new SuffixArray(text, true);
                            var suffArray = suffixArray._mSa;
                            //var lcpArray = suffixArray._mLcp;

                            var directory = Thread.GetDomain().BaseDirectory + "\\SuffixArrays\\";
                            FileUtil.ExportSuffixarrayAndText(prefix, directory, text, suffArray);
                        }
                    }
                    //FileUtil.ExportLcpData(directory, prefix[1], lcpArray);
                    //Convert long array to int array
                    //var suffixArrayuLong = byteAlignedBigULongArray.ToArray();
                    //var suffixArray = suffixArrayuLong.Select(x => int.Parse(x.ToString())).ToArray();
                    //var sufMaxValue = suffixArray.Max();
                    //SetScanText("sufMax: " + sufMaxValue + Environment.NewLine);
                    //var textLength = text.Length;
                    //SetScanText("textLength: " + textLength + Environment.NewLine);
                    //var intSuffixArray = MinimizeSuffixArray.Minimize(suffixArray, text);
                    //var minimizedMaxValue = intSuffixArray.Max();
                    //SetScanText("minSufMax: " + minimizedMaxValue + Environment.NewLine);
                    //var intLcpArray = BuildLcpArray.Build(intSuffixArray, text);
                    //Database.BuildSqlSuffixArrayDatabase(suffArray, Convert.ToChar(prefix[1]), false);
                    //Database.BuildLcpDatabase(lcpArray, Convert.ToChar(prefix[1]), false);
                    //FileUtil.ExportSuffixarrayAndText(prefix[1], Thread.GetDomain().BaseDirectory + "suffixTrees\\", text, suffArray);
                    //var databaseDirectory = Thread.GetDomain().BaseDirectory + "Data\\";
                    //FileUtil.ExportSuffixarrayAndText(prefix, databaseDirectory, text, suffArray);

                    // Should clear up RAM after build
                    GC.Collect();

                }, readFileCancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);


                Task.WaitAll(buildSuffixArrayTask);
                stopWatch.Stop();
                SetMainTime(stopWatch.Elapsed + Environment.NewLine);

            }
            catch (Exception ex)
            {

                SetMainTime("Build Failed: " + ex.Message + " ---- || ---- " + ex.Source + " ---- || ---- " + ex.StackTrace + Environment.NewLine); ;
            }
        }

        /// <summary>
        /// Export the suffix array to file for later process
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="directory"></param>
        /// <param name="suffixArray"></param>
        /// <param name="completeText"></param>
        private void ExportData(string fileName, string directory, HashSet<long> suffixArray, string completeText)
        {
            if (!File.Exists(@directory + "string_" + fileName))
            {
                var stringText = File.CreateText(@directory + "string_" + fileName);
                stringText.Close();
            }
            using (var file = new StreamWriter(@directory + "string_" + fileName, true))
            {
                file.WriteLine(completeText);
            }

            var result = string.Join(";", suffixArray);
            File.WriteAllText(directory + fileName, result);
        }

        /// <summary>
        /// The create suffix array do work.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CreateSuffixTreeDoWork(object sender, DoWorkEventArgs e)
        {

            var path = Thread.GetDomain().BaseDirectory + "Data\\";
            var files = Directory.GetFiles(path);

            _mCancelTokenSource = new CancellationTokenSource();

            // Get a reference to the cancellation token.
            var readFileCancelToken = _mCancelTokenSource.Token;

            // I am iterating through the list of files to use one task per file to do them in parallel.
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var file in files)
            {
                var buildSuffixTreeTask = Task.Factory.StartNew(() =>
                {
                    // If cancel has been chosen, throw an exception now before doing anything.
                    readFileCancelToken.ThrowIfCancellationRequested();
                    try
                    {
                        var completeStringTask = DataBaseToText(file);
                        BuildSuffixTree(completeStringTask, file);
                    }

                    catch (Exception ex)
                    {
                        //
                    }
                    finally
                    {
                        GC.Collect();
                    }

                }, readFileCancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                Task.WaitAll(buildSuffixTreeTask);
            }
            stopWatch.Stop();
            SetMainTime(stopWatch.Elapsed + Environment.NewLine);
        }


        /// <summary>
        /// Build the suffix tree
        /// </summary>
        private void BuildSuffixTree(string text, string path)
        {
            var suffixTree = new suffixTree.SuffixTree();
            suffixTree.Create(text);

            var filePacementDirectoryTask = Thread.GetDomain().BaseDirectory + "suffixTrees\\";
            var fileName = Path.GetFileName(path);
            WriteToBinaryFile(filePacementDirectoryTask + fileName, suffixTree);
        }

        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the XML file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the XML file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        /// <summary>
        /// Reads an object instance from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the XML.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the binary file.</returns>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="filePath">
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string DataBaseToText(string filePath)
        {
            var stringHolder = string.Empty;

            using (Stream f = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var offset = 0;
                var len = f.Length;
                var buffer = new byte[len];

                var readLen = 2048;


                while (offset != len)
                {
                    if (offset + readLen > len)
                    {
                        readLen = (int)len - offset;
                    }

                    offset += f.Read(buffer, offset, readLen);
                }

                var incomingOffset = 0;
                var outboundBuffer = new byte[2048];

                while (incomingOffset < buffer.Length)
                {
                    var length = Math.Min(outboundBuffer.Length, buffer.Length - incomingOffset);

                    Buffer.BlockCopy(buffer, incomingOffset, outboundBuffer, 0, readLen);

                    incomingOffset += length;
                    var currentString = Encoding.UTF8.GetString(outboundBuffer);
                    stringHolder += currentString;
                }

                f.Close();
            }
            var array = stringHolder.Split('!').ToList();

            // MAKE SURE THAT IT IS A Md5
            const string pattern = "^[0-9a-fA-F]{32}[$]$";
            var cleanArray = array.Where(text => Regex.Match(text, pattern, RegexOptions.None).Success).ToList();

            // ONLY TAKE DISTINCT STRINGS
            var distinct = cleanArray.Distinct();
            var completeString = string.Join(string.Empty, distinct);
            return completeString;
        }


        public Dictionary<string, string> Credentials = new Dictionary<string, string>();

        /// <summary>
        /// Indicate server activity
        /// </summary>
        public void Server()
        {
            textBox6.ForeColor = System.Drawing.Color.Green;
            textBox6.Text = @"On";
            CurrentServer = new MainServerControl();
        }


        /// <summary>
        /// The remove special characters.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string RemoveSpecialCharacters(string str)
        {
            var sb = new StringBuilder();
            foreach (var c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '$')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // Starts the server
            Server();
        }


        /// <summary>
        /// Close tcp connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            MainTextArea.Text = @"Closing server - tear down port 3000";
            textBox6.ForeColor = System.Drawing.Color.Red;
            textBox6.Text = @"Off";
            foreach (var client in MainServerControl.ActiveTcpClients)
            {
                client.Close();
            }
            MainServerControl._clientListener.Stop();
            ServerIsActive = false;
        }

        #region ButtonListener


        /// <summary>
        /// Simple checkbox listener for database options
        /// - Set use HHD only
        /// - If checkbox is checked or unchecked, update default settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UseHHDOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (UseHHDOnly.Checked)
            {
                UseRAMOnly.Checked = false;
                Properties.Settings.Default.UseHDDOnly = true;
            }
            else
            {
                Properties.Settings.Default.UseHDDOnly = false;
            }
            Properties.Settings.Default.Save();
        }


        /// <summary>
        /// Simple checkbox listener for database options
        /// - Set use RAM only
        /// - If checkbox is checked or unchecked, update default settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UseRAMOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (UseRAMOnly.Checked)
            {
                UseHHDOnly.Checked = false;
                Properties.Settings.Default.UseRAMOnly = true;
            }
            else
            {
                Properties.Settings.Default.UseHDDOnly = false;
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Close application event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            _tcpListener?.Stop();
            Scanner.ScannerCaching.SaveCache();
            if (Application.MessageLoop)
            {
                if (MainServerControl.ActiveTcpClients != null)
                {
                    foreach (var client in MainServerControl.ActiveTcpClients)
                    {
                        client.Close();
                    }
                    MainServerControl._clientListener.Stop();
                }
                Application.Exit();

            }
            else
            {
                // Console app
                //Environment.Exit(1);
                Application.Exit();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // On button click, create database
            SetMainTime("Starting database build..." + Environment.NewLine);
            Database.BuildSqlDatabase();
            SetMainTime("Database is build." + Environment.NewLine);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // On button click, create database
            SetMainTime("Starting test database build..." + Environment.NewLine);
            Database.BuildTestSqlDatabase();
            SetMainTime("Test Database is build." + Environment.NewLine);
        }

        /// <summary>
        /// Set the search method specified by the user.
        /// </summary>
        /// <returns></returns>
        private void SetSearchMethod()
        {
            var ischecked = checkedListBox1.CheckedIndices;
            if (ischecked[0] == 0)
            {
                // Binary search
                Properties.Settings.Default.UseBinary = true;
                Properties.Settings.Default.UseBinaryWLcp = false;
                Properties.Settings.Default.UseSQL = false;
                Properties.Settings.Default.Save();
            }

            else if (ischecked[0] == 1)
            {
                // Binary search w. LCP
                Properties.Settings.Default.UseBinary = false;
                Properties.Settings.Default.UseBinaryWLcp = true;
                Properties.Settings.Default.UseSQL = false;
                Properties.Settings.Default.Save();
            }

            else if (ischecked[0] == 2)
            {
                // Binary search w. LCP
                Properties.Settings.Default.UseBinary = false;
                Properties.Settings.Default.UseBinaryWLcp = false;
                Properties.Settings.Default.UseSQL = true;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// uncheck all checkbox beside
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (i != e.Index)
                {
                    checkedListBox1.SetItemChecked(i, false);
                }
            }
        }

        /// <summary>
        /// Single file scanner
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            // Init serach method
            SetSearchMethod();

            SetScanText("File :" + File.Exists(Thread.GetDomain().BaseDirectory + "\\" + ScannerConst.Md5Cashing));

            _mCancelTokenSource = new CancellationTokenSource();
            // Get a reference to the cancellation token.
            CancellationToken readFileCancelToken = _mCancelTokenSource.Token;
            DialogResult result = openFileDialog1.ShowDialog();
            openFileDialog1.Multiselect = false;
            var task = Task.Factory.StartNew(() =>
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                // If cancel has been chosen, throw an exception now before doing anything.
                readFileCancelToken.ThrowIfCancellationRequested();
                try
                {
                    if (result == DialogResult.OK)
                    {
                        string file = openFileDialog1.FileName;
                        try
                        {
                            using (var md5 = MD5.Create())
                            {
                                using (var stream = File.OpenRead(openFileDialog1.FileName))
                                {
                                    var md5Byte = md5.ComputeHash(stream);
                                    string md5String = BitConverter.ToString(md5Byte).Replace("-", "");
                                    stream.Close();
                                    SetScanText("Prefix: " + md5String[0] + Environment.NewLine);
                                    var scanResult = Scanner.Scanner.ScanFile(md5String.ToLower() + "$");
                                    stopWatch.Stop();
                                    if (scanResult)
                                    {
                                        SetScanText(Path.GetFileName(file) +
                                                    " ---- is a virus -- Quarantined. (ScanTime: " +
                                                    stopWatch.Elapsed + " )" + Environment.NewLine);
                                        var fileName = Path.GetFileNameWithoutExtension(file);
                                        var path = file.Replace(Path.GetFileName(file), string.Empty);
                                        var process =
                                            Process.GetProcessesByName(fileName)
                                                .FirstOrDefault(p => p.MainModule.FileName.StartsWith(path));
                                        var lockedProcesses = FileUtil.WhoIsLocking(file).ToList();
                                        foreach (var lockedProcess in lockedProcesses)
                                        {
                                            lockedProcess.Kill();
                                        }
                                        process?.Kill();
                                        File.SetAttributes(file, FileAttributes.Normal);
                                        var databaseDirectory = Thread.GetDomain().BaseDirectory + "Quarantine\\";
                                        if (File.Exists(databaseDirectory + fileName + "." + md5String))
                                            File.Delete(databaseDirectory + fileName + "." + md5String);
                                        File.Move(file, databaseDirectory + fileName + "." + md5String);
                                    }
                                    else
                                    {
                                        SetScanText(Path.GetFileName(file) + " |---| is clear. (ScanTime: " +
                                                    stopWatch.Elapsed + " )" + Environment.NewLine);

                                    }
                                }
                            }
                        }
                        catch (IOException ex)
                        {
                            SetScanText(ex.Message + Environment.NewLine);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    //
                }
                catch (Exception ex)
                {
                    SetScanText(ex.Message + Environment.NewLine);
                }
                finally
                {
                    GC.Collect();
                }
            }, readFileCancelToken);

        }

        private void button10_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string folder = folderBrowserDialog1.SelectedPath;
                var result1 = Scanner.Scanner.ScanFolder(folder);
                foreach (var infectedFile in result1)
                {
                    SetScanText(infectedFile + "-- INFECTED!" + Environment.NewLine);
                }
                GC.Collect();
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            StartupScanner.BuildStartup();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _buildSuffixArray.RunWorkerAsync();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _buildSuffixTree.RunWorkerAsync();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SplitOption = int.Parse(comboBox1.Text);
            Properties.Settings.Default.Save();
            GetSplitOption = int.Parse(comboBox1.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }
        #endregion
    }
}
