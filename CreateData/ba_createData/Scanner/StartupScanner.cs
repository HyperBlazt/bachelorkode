using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ba_createData.Collection;

namespace ba_createData.Scanner
{
    public static class StartupScanner
    {

        /// <summary>
        /// The object used for cancelling tasks.
        /// </summary>
        private static CancellationTokenSource _mCancelTokenSource;

        public static void BuildStartup()
        {

            var _workingDirectory = Thread.GetDomain().BaseDirectory;
            _mCancelTokenSource = new CancellationTokenSource();
            var startUpFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            File.AppendAllText(_workingDirectory + "ServiceTest.txt", "--- STARTUP WORKING DIR: " + startUpFolderPath + Environment.NewLine);

            _mCancelTokenSource = new CancellationTokenSource();
            var startupDict = new Dictionary<string, string>();

            // Get a reference to the cancellation token.
            CancellationToken readFileCancelToken = _mCancelTokenSource.Token;
            Task.Run(() =>
            {
                string filepath = "";
                // If cancel has been chosen, throw an exception now before doing anything.
                readFileCancelToken.ThrowIfCancellationRequested();
                try
                {
                    String path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    path = Path.GetDirectoryName(path);
                    if (path != null) Directory.SetCurrentDirectory(path);
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        RedirectStandardOutput = true,
                        Arguments = string.Format("/c " + _workingDirectory + "autorunsc.exe " + "-a * -h * -nobanner -accepteula")
                    };
                    try
                    {
                        // Start the process with the info we specified.
                        // Call WaitForExit and then the using statement will close.
                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            if (exeProcess != null)
                            {
                                exeProcess.Start();
                                string standardOutput;
                                File.AppendAllText(_workingDirectory + "ServiceTest.txt", DateTime.Now + " CMD started: " + path + Environment.NewLine);
                                while ((standardOutput = exeProcess.StandardOutput.ReadLine()) != null)
                                {
                                    var environmentVariable = Environment.GetEnvironmentVariable("SystemRoot");
                                    if (environmentVariable != null)
                                    {
                                        var windownInstallRoot = environmentVariable.Replace("WINDOWS", string.Empty).ToLower();
                                        if (standardOutput.Contains(windownInstallRoot))
                                        {
                                            filepath = standardOutput;

                                        }
                                    }
                                    if (standardOutput.Contains("MD5"))
                                    {
                                        if (standardOutput.Split(':').Length >= 2)
                                        {
                                            var puremd5 = standardOutput.Split(':')[1].Trim().ToLower();
                                            if (!puremd5.Equals(string.Empty) && !startupDict.Keys.Contains(puremd5 + "$") && puremd5.Length.Equals(32))
                                            {
                                                startupDict.Add(puremd5 + "$", filepath);
                                                var result = Scanner.ScanFile(puremd5 + "$");
                                                if (result)
                                                {

                                                    File.AppendAllText(_workingDirectory + "ServiceTest.txt",
                                                        DateTime.Now + @"  -- Buildstartup --  File: " + filepath +
                                                        @"   @@@ is infected - moved to quarantine - with: " + puremd5 +
                                                        Environment.NewLine);
                                                    var fileNameWithOutExtension =
                                                        Path.GetFileNameWithoutExtension(filepath);
                                                    var pathToFile = filepath.Replace(Path.GetFileName(filepath),
                                                        string.Empty);
                                                    var currentProcess =
                                                        Process.GetProcessesByName(fileNameWithOutExtension)
                                                            .FirstOrDefault(
                                                                p => p.MainModule.FileName.StartsWith(pathToFile));
                                                    var lockedProcesses = FileUtil.WhoIsLocking(filepath).ToList();
                                                    foreach (var lockedProcess in lockedProcesses)
                                                    {
                                                        lockedProcess.Kill();
                                                    }
                                                    currentProcess?.Kill();
                                                    File.SetAttributes(filepath, FileAttributes.Normal);
                                                    var databaseDirectory = Thread.GetDomain().BaseDirectory +
                                                                            @"Quarantine\";
                                                    //File.Copy(filepath, databaseDirectory + fileNameWithOutExtension.ToLowerInvariant() + ".qua", true);
                                                    //File.Delete(filepath);
                                                }
                                                else
                                                {
                                                    File.AppendAllText(_workingDirectory + "ServiceTest.txt",
                                                        DateTime.Now + "  -- Buildstartup --  File: " + filepath + " Scan reported no virus :)" + Environment.NewLine);
                                                }
                                            }
                                        }
                                    }
                                }
                                exeProcess.WaitForExit();
                            }
                            GC.Collect();
                        }

                    }
                    catch (NullReferenceException ex)
                    {
                        File.AppendAllText(_workingDirectory + "ServiceTest.txt", "Error: " + ex.StackTrace + Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(_workingDirectory + "ServiceTest.txt", "Error: " + ex.StackTrace + Environment.NewLine);
                }
                finally
                {
                    GC.Collect();
                }
            }, readFileCancelToken);
        }
    }
}
