// https://bitbucket.org/snippets/wiip/nnGX d. 13-12-16

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace ba_createData.Collection
{
    public static class FileUtil
    {
        #region Types

        private enum RmAppType
        {
            RmUnknownApp = 0,

            RmMainWindow = 1,

            RmOtherWindow = 2,

            RmService = 3,

            RmExplorer = 4,

            RmConsole = 5,

            RmCritical = 1000
        }

        #endregion

        // ReSharper restore UnusedMember.Local

        // ReSharper disable MemberCanBePrivate.Local

        #region Types

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct RmProcessInfo
        {
            #region Champs

            public readonly uint AppStatus;

            public readonly RmAppType ApplicationType;

            public RM_UNIQUE_PROCESS Process;

            public readonly uint TSSessionId;

            [MarshalAs(UnmanagedType.Bool)]
            public readonly bool bRestartable;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
            public readonly string strAppName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
            public readonly string
                strServiceShortName;

            #endregion
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RM_UNIQUE_PROCESS
        {
            // ReSharper disable once MemberCanBePrivate.Local

            #region Champs

            public readonly FILETIME ProcessStartTime;

            public readonly int dwProcessId;

            #endregion
        }

        #endregion

        #region Constantes

        const int CCH_RM_MAX_APP_NAME = 255;

        const int CCH_RM_MAX_SVC_NAME = 63;

        const int RmRebootReasonNone = 0;

        #endregion

        // ReSharper disable UnusedMember.Local

        #region Méthodes

        [DllImport("rstrtmgr.dll")]
        static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll")]
        static extern int RmGetList(uint dwSessionHandle,
            out uint pnProcInfoNeeded,
            ref uint pnProcInfo,
            [In, Out] RmProcessInfo[] rgAffectedApps,
            ref uint lpdwRebootReasons);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        static extern int RmRegisterResources(uint pSessionHandle,
            UInt32 nFiles,
            string[] rgsFilenames,
            UInt32 nApplications,
            [In] RM_UNIQUE_PROCESS[] rgApplications,
            UInt32 nServices,
            string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
        static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        /// <summary>
        /// Find out what process(es) have a lock on the specified file.
        /// </summary>
        /// <param name="path">Path of the file.</param>
        /// <returns>Processes locking the file</returns>
        /// <remarks>See also:
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa373661(v=vs.85).aspx
        /// http://wyupdate.googlecode.com/svn-history/r401/trunk/frmFilesInUse.cs (no copyright in code at time of viewing)
        /// </remarks>
        public static IEnumerable<Process> WhoIsLocking(string path)
        {
            uint handle;
            string key = Guid.NewGuid().ToString();
            var processes = new List<Process>();

            int res = RmStartSession(out handle, 0, key);
            if (res != 0) throw new Exception("Could not begin restart session.  Unable to determine file locker.");

            try
            {
                const int ERROR_MORE_DATA = 234;
                uint pnProcInfoNeeded = 0,
                    pnProcInfo = 0,
                    lpdwRebootReasons = RmRebootReasonNone;

                var resources = new string[] { path }; // Just checking on one resource.

                res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);

                if (res != 0) throw new Exception("Could not register resource.");

                //Note: there's a race condition here -- the first call to RmGetList() returns
                //      the total number of process. However, when we call RmGetList() again to get
                //      the actual processes this number may have increased.
                res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

                if (res == ERROR_MORE_DATA)
                {
                    // Create an array to store the process results
                    var processInfo = new RmProcessInfo[pnProcInfoNeeded];
                    pnProcInfo = pnProcInfoNeeded;

                    // Get the list
                    res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
                    if (res == 0)
                    {
                        processes = new List<Process>((int)pnProcInfo);

                        // Enumerate all of the results and add them to the 
                        // list to be returned
                        for (int i = 0; i < pnProcInfo; i++)
                        {
                            try
                            {
                                processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
                            }
                            // catch the error -- in case the process is no longer running
                            catch (ArgumentException)
                            {
                            }
                        }
                    }
                    else throw new Exception("Could not list processes locking resource.");
                }
                else if (res != 0)
                    throw new Exception("Could not list processes locking resource. Failed to get size of result.");
            }
            finally
            {
                RmEndSession(handle);
            }

            return processes;
        }

        /// <summary>
        /// Export the suffix array to file for later process
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="pattern"></param>
        public static void ExportSuffixarrayAndText(string pattern, string directory, string _mStr, int[] suffixArray)
        {
            // Save the string
            if (File.Exists(@directory + "string_" + pattern))
            {
                File.Delete(@directory + "string_" + pattern);
            }
            if (!File.Exists(@directory + "string_" + pattern))
            {
                var stringText = File.CreateText(@directory + "string_" + pattern);
                stringText.Close();
            }
            using (var file = new StreamWriter(@directory + "string_" + pattern, true))
            {
                file.WriteLine(_mStr);
            }

            // Save the suffix array
            var result = string.Join(";", suffixArray);
            if (File.Exists(@directory + "sa_" + pattern))
            {
                File.Delete(@directory + "sa_" + pattern);
            }
            File.WriteAllText(directory + "sa_" + pattern, result);
        }


        /// <summary>
        /// Export lcp data
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="pattern"></param>
        /// <param name="lcpArray"></param>
        public static void ExportLcpData(string pattern, string directory, int[] lcpArray)
        {
            if (!File.Exists(@directory + "lcp_" + pattern))
            {
                var stringText = File.CreateText(@directory + "lcp_" + pattern);
                stringText.Close();
            }
            var result = string.Join(";", lcpArray);
            if (File.Exists(@directory + "lcp_" + pattern))
            {
                File.Delete(@directory + "lcp_" + pattern);
            }
            File.WriteAllText(directory + "lcp_" + pattern, result);
        }

        /// <summary>
        /// The deserialize array.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>int[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static int[] DeserializeArray(string filePath)
        {
            if (File.Exists(filePath))
            {
                var text = File.ReadAllText(filePath);
                string[] words = text.Split(';');
                var done = words.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                return Array.ConvertAll(done, int.Parse);
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// The deserialize string.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string DeserializeString(string filePath)
        {
            if (File.Exists(filePath))
            {
                var bytes = File.ReadAllBytes(filePath);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion
    }
}