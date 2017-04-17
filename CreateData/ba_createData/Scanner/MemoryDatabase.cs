using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ba_createData.Collection;

namespace ba_createData.Scanner
{

    public static class MemoryDatabase
    {

        /// <summary>
        /// Get of set the dictionary of strings
        /// </summary>
        public static Dictionary<char, string> TextFile { get; set; }

        /// <summary>
        /// Get or set the dictionary of suffix arrays
        /// </summary>
        public static Dictionary<char, int[]> SuffixArray { get; set; }

        /// <summary>
        /// Get or set the dictionary of lowest common prefix arrays
        /// </summary>
        public static Dictionary<char, int[]> LcpArray { get; set; }



        /// <summary>
        /// Load all values stored in the database to memory
        /// </summary>
        /// <returns></returns>
        public static bool LoadDataIntoMemory()
        {
            try
            {

                TextFile = new Dictionary<char, string>();
                SuffixArray = new Dictionary<char, int[]>();
                LcpArray = new Dictionary<char, int[]>();

                // Load from files
                var directory = Thread.GetDomain().BaseDirectory + "SuffixArrays\\";
                string[] fileEntries = Directory.GetFiles(directory);
                foreach (var file in fileEntries)
                {
                    var fileName = Path.GetFileName(file);
                    if (fileName != null)
                    {
                        var prefix = fileName.Split('_');
                        if (prefix[0].Equals("sa"))
                        {
                            var suffixArray = FileUtil.DeserializeArray(file);
                            SuffixArray.Add(Convert.ToChar(prefix[1]), suffixArray);
                        }
                        else if (prefix[0].Equals("string"))
                        {
                            var completeString = FileUtil.DeserializeString(file);
                            TextFile.Add(Convert.ToChar(prefix[1]), completeString);
                        }
                    }
                }


                //var malwareTableNames = Database.GetMalwareTableNames();
                //malwareTableNames.Sort();
                //var suffixArrayTableNames = Database.GetSuffixTableNames();
                //suffixArrayTableNames.Sort();
                //var lcpTableNames = Database.GetLcpTableNames();
                //lcpTableNames.Sort();

                //TextFile = new Dictionary<char, string>();
                //foreach (var name in malwareTableNames)
                //{
                //    var prefix = name.Split('_');
                //    var text = Database.GetMd5MalwareFromDatabaseAsCompleteString(name, true);
                //    TextFile.Add(Convert.ToChar(prefix[1]), text);
                //}

                //SuffixArray = new Dictionary<char, int[]>();
                //foreach (var name in suffixArrayTableNames)
                //{
                //    var prefix = name.Split('_');
                //    var array = Database.GetSuffixArrayTableByTableName(name);
                //    SuffixArray.Add(Convert.ToChar(prefix[1]), array);
                //}

                //LcpArray = new Dictionary<char, int[]>();
                //foreach (var name in lcpTableNames)
                //{
                //    var prefix = name.Split('_');
                //    var lcparray = Database.GetLcpTableByTableName(name);
                //    LcpArray.Add(Convert.ToChar(prefix[1]), lcparray);
                //}
            }
            catch (Exception ex)
            {
                //
            }

            return true;
        }
    }
}
