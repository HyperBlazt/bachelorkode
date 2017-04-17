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
 
 IMPORTANT NOTICE:

 ANY CODE FLAGED TO BE OWNED BY AUTHORS OR COPYRIGHT HOLDERS ARE NOT FREE OF
 CHARGE, AND SHOULD BE USED WITH ANY RESTRICTIONS ASSOCIATED THE FILES/CODE.
*/

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ba_createData.Collection;

namespace ba_createData
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// The malware database.
    /// </summary>
    [Serializable]
    public static class Database
    {
        private const string Pattern = "^[0-9a-fA-F]{32}$";

        private static List<string> SuffixDatabaseNames;

        private static List<string> LcpDatabaseNames;

        private static List<string> TextDatabaseNames;

        private static SqlConnection SetupSqlConnection()
        {
            // Base directory connection
            var databasePath = Thread.GetDomain().BaseDirectory + "\\Database.mdf";
            // D:\GitHub bachelor\BachelorNew\CreateData\ba_createData\Database.mdf
            SqlConnection databaseConnection = new SqlConnection(
                $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={databasePath};Integrated Security=True");
            //SuffixDatabaseNames = GetMalwareDataBaseNames();
            return databaseConnection;
        }


        private static SqlConnection SetupSqlSuffixArrayConnection()
        {

            // Use test database if is true
            if (Properties.Settings.Default.Test)
            {
                var databasePath = Thread.GetDomain().BaseDirectory + "\\TestDatabase.mdf";
                // Base directory connection
                // D:\GitHub bachelor\BachelorNew\CreateData\ba_createData\TestDatabase.mdf
                var databaseConnection = new SqlConnection(
                    $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={databasePath};Integrated Security=True");
                SuffixDatabaseNames = GetSuffixArrayDataBaseNames(databaseConnection);
                LcpDatabaseNames = GetLcpArrayDataBaseNames(databaseConnection);
                TextDatabaseNames = GetTextDataBaseNames(databaseConnection);

                return databaseConnection;
            }
            else
            {

                var databasePath = Thread.GetDomain().BaseDirectory + "\\SuffixArray.mdf";
                // Base directory connection
                //D:\GitHub bachelor\BachelorNew\CreateData\ba_createData\SuffixArray.mdf
                var databaseConnection = new SqlConnection(
                    $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={databasePath};Integrated Security=True");
                SuffixDatabaseNames = GetSuffixArrayDataBaseNames(databaseConnection);
                LcpDatabaseNames = GetLcpArrayDataBaseNames(databaseConnection);
                return databaseConnection;
            }
        }

        /// <summary>
        /// The create database, such that each row in the .csv files are concat with the termination character '$'
        /// Data is saved as a file/ files
        /// </summary>
        public static void CreateDatabase()
        {
            var splitOption = Properties.Settings.Default.SplitOption;
            var databaseDirectory = Thread.GetDomain().BaseDirectory;
            var path = Thread.GetDomain().BaseDirectory + "Hashes//";
            var files = Directory.GetFiles(path);
            if (files.Length == 0) return;
            foreach (var file in files)
            {
                using (var sr = new StreamReader(file))
                {
                    // currentLine will be null when the StreamReader reaches the end of file
                    string currentLine;
                    while ((currentLine = sr.ReadLine()) != null)
                    {

                        // Checking current line to insure that it is a MD5 hash, with appropriate length
                        if (Regex.Match(currentLine, Pattern, RegexOptions.None).Success)
                        {
                            var filePath = databaseDirectory + "\\Data\\" + currentLine.Substring(0, splitOption) +
                                           ".data";
                            using (
                                var fileStream = new FileStream(
                                    filePath,
                                    FileMode.Append,
                                    FileAccess.Write,
                                    FileShare.Write))
                            using (var bw = new BinaryWriter(fileStream))
                            {
                                bw.Write(currentLine + "$");
                            }
                        }
                    }

                    sr.Close();
                    sr.Dispose();
                }
            }
        }



        /// <summary>
        /// Build the SQL database with all distinct MD5 string contained in the filedatabase 
        /// </summary>
        public static void BuildSqlDatabase()
        {
            var databaseConnection = SetupSqlConnection();
            var splitOption = Properties.Settings.Default.SplitOption;
            var path = Thread.GetDomain().BaseDirectory + "Hashes//";
            var files = Directory.GetFiles(path);
            if (files.Length == 0) return;
            databaseConnection.Open();
            var filePath = Thread.GetDomain().BaseDirectory + "\\Data\\" + "databaseError.txt";
            foreach (var file in files)
            {
                using (var sr = new StreamReader(file))
                {
                    string currentLine;
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        if (!Regex.Match(currentLine, Pattern, RegexOptions.None).Success) continue;
                        try
                        {
                            Database.ClearMalwareTable("MALWARE_" + currentLine.Substring(0, splitOption));
                            // Insert into database into the propper table in dbo
                            var database = new SqlCommand
                            {
                                Connection = databaseConnection,
                                CommandText =
                                    $"INSERT INTO {"MALWARE_" + currentLine.Substring(0, splitOption)} (md5Value) VALUES (@MD5ID)"
                            };
                            database.Parameters.AddWithValue("@MD5ID", currentLine + "$");
                            database.ExecuteNonQuery();

                        }
                        catch (Exception ex)
                        {
                            using (
                                var fileStream = new FileStream(
                                    filePath,
                                    FileMode.Append,
                                    FileAccess.Write,
                                    FileShare.Write))
                            using (var bw = new BinaryWriter(fileStream))
                            {
                                bw.Write(ex.Message + Environment.NewLine);
                            }
                        }
                    }
                }
            }
            databaseConnection.Close();
        }

        /// <summary>
        /// Build the TEST SQL database with all distinct MD5 string contained in the filedatabase 
        /// </summary>
        public static void BuildTestSqlDatabase()
        {
            var databaseConnection = SetupSqlConnection();
            var splitOption = Properties.Settings.Default.SplitOption;
            var path = Thread.GetDomain().BaseDirectory + "Testdatabase//";
            var files = Directory.GetFiles(path);
            if (files.Length == 0) return;
            databaseConnection.Open();
            var filePath = Thread.GetDomain().BaseDirectory + "\\Data\\" + "databaseError.txt";
            foreach (var file in files)
            {
                using (var sr = new StreamReader(file))
                {
                    var i = 0;
                    string currentLine;
                    while ((currentLine = sr.ReadLine()) != null && i < 2000)
                    {
                        if (!Regex.Match(currentLine, Pattern, RegexOptions.None).Success) continue;
                        try
                        {
                            Database.ClearMalwareTable("MALWARE_" + currentLine.Substring(0, splitOption));
                            // Insert into database into the propper table in dbo
                            var database = new SqlCommand
                            {
                                Connection = databaseConnection,
                                CommandText =
                                    $"INSERT INTO {"MALWARE_" + currentLine.Substring(0, splitOption)} (md5Value) VALUES (@MD5ID)"
                            };
                            database.Parameters.AddWithValue("@MD5ID", currentLine + "$");
                            database.ExecuteNonQuery();
                            i++;

                        }
                        catch (Exception ex)
                        {
                            using (
                                var fileStream = new FileStream(
                                    filePath,
                                    FileMode.Append,
                                    FileAccess.Write,
                                    FileShare.Write))
                            using (var bw = new BinaryWriter(fileStream))
                            {
                                bw.Write(ex.Message + Environment.NewLine);
                            }
                        }
                    }
                }
            }
            databaseConnection.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static List<string> GetTables(SqlConnection connectionString)
        {
            connectionString.Open();
            var schema = connectionString.GetSchema("Tables");
            connectionString.Close();
            return (from DataRow row in schema.Rows select row[2].ToString()).ToList();

        }

        /// <summary>
        /// Clear table with name
        /// </summary>
        public static void ClearMalwareTable(string tablename)
        {
            if (Properties.Settings.Default.Test)
            {
                var databaseConnection = SetupSqlConnection();

                var suffixArrayCommand = new SqlCommand
                {
                    Connection = databaseConnection,
                    CommandText = $"truncate table {tablename}"
                };

                databaseConnection.Open();
                suffixArrayCommand.ExecuteReader();
            }
        }


        /// <summary>
        /// Returns the tables containing lcp values
        /// - All suffix array databases are named LCPARRAY
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLcpArrayDataBaseNames(SqlConnection connectionString)
        {
            var malwareTableNames = GetTables(connectionString).Where(x => x.Contains("LCPARRAY_"));
            return malwareTableNames.ToList();
        }


        /// <summary>
        /// Returns the tables containing lcp values
        /// - All suffix array databases are named LCPARRAY
        /// </summary>
        /// <returns></returns>
        public static List<string> GetTextDataBaseNames(SqlConnection connectionString)
        {
            var malwareTableNames = GetTables(connectionString).Where(x => x.Contains("TEXT_"));
            return malwareTableNames.ToList();
        }

        /// <summary>
        /// Search the SQL database
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool SearchByPattern(string pattern)
        {
            var prefix = pattern[0];
            var databaseConnection = SetupSqlConnection();
            databaseConnection.Open();
            var dataExists = new SqlCommand
            {
                Connection = databaseConnection,
                CommandText = $"SELECT COUNT(*) FROM MALWARE_{prefix} WHERE md5Value LIKE (@MD5ID)"
            };
            dataExists.Parameters.AddWithValue("@MD5ID", pattern);
            var count = (int)dataExists.ExecuteScalar();
            return count > 0 ;
        }


        /// <summary>
        /// Build the SQL database for LCP values
        /// </summary>
        public static void BuildLcpDatabase(int[] lcpArray, char prefix, bool test)
        {
            var databaseConnection = SetupSqlSuffixArrayConnection();
            var filePath = Thread.GetDomain().BaseDirectory + "\\Error\\" + "lcpArrayError.txt";
            var i = 0;
            foreach (var entry in lcpArray)
            {
                try
                {
                    databaseConnection.Open();
                    var database = new SqlCommand
                    {
                        Connection = databaseConnection,
                        CommandText = $"INSERT INTO {"LCPARRAY_" + prefix}(id, lcpvalue) VALUES (@id, @lcpvalue)"
                    };
                    database.Parameters.AddWithValue("@id", i);
                    database.Parameters.AddWithValue("@lcpvalue", entry);
                    database.ExecuteNonQuery();
                    i++;
                    databaseConnection.Close();
                }
                catch (Exception ex)
                {
                    databaseConnection.Close();
                    using (
                        var fileStream = new FileStream(
                            filePath,
                            FileMode.Append,
                            FileAccess.Write,
                            FileShare.Write))
                    using (var bw = new BinaryWriter(fileStream))
                    {
                        bw.Write(ex.Message + Environment.NewLine);
                    }
                }
            }
        }



        /// <summary>
        /// Returns the tables containing suffix array md5 strings
        /// - All suffix array databases are named SUFFIXARRAY
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSuffixArrayDataBaseNames(SqlConnection connection)
        {
            var malwareTableNames = GetTables(connection).Where(x => x.Contains("SUFFIXARRAY_"));
            return malwareTableNames.ToList();
        }

        /// <summary>
        /// Returns the tables containing malware md5 strings
        /// - All malware table names are named MALWARE_ plus a predefined suffix
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMalwareTableNames()
        {
            // get the database connection with malware table names
            var connection = SetupSqlConnection();
            var malwareTableNames = GetTables(connection).Where(x => x.Contains("MALWARE_"));
            return malwareTableNames.ToList();
        }

        /// <summary>
        /// Returns the tables containing suffix arrays
        /// - All suffix array table names are named SUFFIXARRAY_ plus a predefined suffix
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSuffixTableNames()
        {
            // get the database connection with malware table names
            var connection = SetupSqlSuffixArrayConnection();
            var malwareTableNames = GetTables(connection).Where(x => x.Contains("SUFFIXARRAY_"));
            return malwareTableNames.ToList();
        }

        /// <summary>
        /// Returns the tables containing lcp arrays
        /// - All suffix array table names are named LCPARRAY_ plus a predefined suffix
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLcpTableNames()
        {
            // get the database connection with malware table names
            var connection = SetupSqlSuffixArrayConnection();
            var malwareTableNames = GetTables(connection).Where(x => x.Contains("LCPARRAY_"));
            return malwareTableNames.ToList();
        }


        /// <summary>
        /// Build the SQL database for suffix values
        /// </summary>
        public static void BuildSqlSuffixArrayDatabase(int[] suffixArray, char prefix, bool test)
        {
            // Clear the table as we build a new instance
            Database.ClearTable("SUFFIXARRAY_" + prefix);

            // Build the table with suffix array values
            var databaseConnection = SetupSqlSuffixArrayConnection();
            var filePath = Thread.GetDomain().BaseDirectory + "\\Error\\" + "suffixArrayError.txt";
            var i = 0;
            foreach (var entry in suffixArray)
            {
                try
                {
                    databaseConnection.Open();

                    var database = new SqlCommand
                    {
                        Connection = databaseConnection,
                        CommandText =
                            $"INSERT INTO {"SUFFIXARRAY_" + prefix}(id, suffixvalue) VALUES (@id, @suffixvalue)"
                    };
                    database.Parameters.AddWithValue("@id", i);
                    database.Parameters.AddWithValue("@suffixvalue", entry);
                    database.ExecuteNonQuery();
                    i++;
                    databaseConnection.Close();
                }
                catch (Exception ex)
                {
                    databaseConnection.Close();
                    using (
                        var fileStream = new FileStream(
                            filePath,
                            FileMode.Append,
                            FileAccess.Write,
                            FileShare.Write))
                    using (var bw = new BinaryWriter(fileStream))
                    {
                        bw.Write(ex.Message + "  ____EXTENSION ____ " + prefix + Environment.NewLine);
                    }
                }
            }
        }


        /// <summary>
        /// Insert a single md5 into SQL Database
        /// </summary>
        /// <param name="md5">
        ///     md5 string must have a termination symbol 
        /// </param>
        /// <returns></returns>
        public static bool InsertSingleMd5(string md5)
        {
            var databaseConnection = SetupSqlConnection();
            if (!Regex.Match(md5, Pattern, RegexOptions.None).Success) return false;
            // Removes duplicates
            var dataExists = new SqlCommand
            {
                Connection = databaseConnection,
                CommandText = "SELECT COUNT(*) FROM MD5 WHERE MD5ID LIKE (@MD5ID)"
            };
            dataExists.Parameters.AddWithValue("@MD5ID", md5);
            var count = (int)dataExists.ExecuteScalar();
            if (!count.Equals(0)) return false;
            var database = new SqlCommand
            {
                Connection = databaseConnection,
                CommandText = "INSERT INTO MD5(MD5ID) VALUES (@MD5ID)"
            };
            database.Parameters.AddWithValue("@MD5ID", md5);
            databaseConnection.Open();
            database.ExecuteNonQuery();
            databaseConnection.Close();

            return true;
        }


        /// <summary>
        /// Insert a single md5 into SQL Database
        /// </summary>
        /// <param name="md5">
        ///     md5 string must have a termination symbol
        /// </param>
        /// <returns></returns>
        public static bool DeleteSingleMd5(string md5)
        {
            var databaseConnection = SetupSqlConnection();
            if (!Regex.Match(md5, Pattern, RegexOptions.None).Success) return false;
            // Removes duplicates
            var dataExists = new SqlCommand
            {
                Connection = databaseConnection,
                CommandText = "SELECT COUNT(*) FROM MD5 WHERE MD5ID LIKE (@MD5ID)"
            };
            dataExists.Parameters.AddWithValue("@MD5ID", md5);
            var count = (int)dataExists.ExecuteScalar();
            if (!count.Equals(0)) return false;
            var database = new SqlCommand
            {
                Connection = databaseConnection,
                CommandText = "DELETE FROM MD5 WHERE MD5ID = (@MD5ID)"
            };
            database.Parameters.AddWithValue("@MD5ID", md5);
            databaseConnection.Open();
            database.ExecuteNonQuery();
            databaseConnection.Close();

            return true;
        }


        /// <summary>
        /// Return true if and only if the md5 string is contained in the SQL database
        /// </summary>
        public static bool IsMd5InSqlLDatabase(string md5)
        {
            var databaseConnection = SetupSqlConnection();
            var dataExists = new SqlCommand
            {
                Connection = databaseConnection,
                CommandText = "SELECT COUNT(*) FROM MD5 WHERE MD5ID LIKE (@MD5ID)"
            };
            dataExists.Parameters.AddWithValue("@MD5ID", md5 + "$");
            databaseConnection.Open();
            var count = (int)dataExists.ExecuteScalar();
            databaseConnection.Close();
            return count.Equals(0);
        }



        /// <summary>
        /// Returns the tables containing malare md5 strings
        /// - All malware databaes are named MALWARE
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMalwareDataBaseNames()
        {
            var malwareTableNames = GetTables(SetupSqlConnection()).Where(x => x.Contains("MALWARE_"));
            return malwareTableNames.ToList();
        }

        /// <summary>
        /// Return Malware Database as int[]
        /// </summary>
        public static HashSet<string> GetMd5MalwareFromDatabase(string tableName, bool distinct)
        {
            var databaseConnection = SetupSqlConnection();
            databaseConnection.Open();
            var isDistinct = distinct ? "DISTINCT" : string.Empty;
            var databaseHolder = new HashSet<string>();
            var filePath = Thread.GetDomain().BaseDirectory + "\\Error\\" + "getDatabaseError.txt";
            try
            {
                var database = new SqlCommand
                {
                    Connection = databaseConnection,
                    CommandText = $"SELECT {isDistinct} md5Value FROM {tableName}"
                };
                var reader = database.ExecuteReader();
                while (reader.Read())
                {
                    //Every new row will create a new dictionary that holds the columns
                    databaseHolder.Add(reader["md5Value"].ToString());
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                using (
                    var fileStream = new FileStream(
                        filePath,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.Write))
                using (var bw = new BinaryWriter(fileStream))
                {
                    bw.Write(ex.Message + Environment.NewLine);
                }
            }
            databaseConnection.Close();
            return databaseHolder;
        }

        public static string GetMd5MalwareFromDatabaseAsCompleteString(string tableName, bool distinct)
        {
            var databaseConnection = SetupSqlConnection();
            databaseConnection.Open();
            var databaseHolder = new StringBuilder();
            var isDistinct = distinct ? "DISTINCT" : string.Empty;
            var filePath = Thread.GetDomain().BaseDirectory + "\\Error\\" + "getDatabaseError.txt";
            try
            {
                var database = new SqlCommand
                {
                    Connection = databaseConnection,
                    CommandText = $"SELECT {isDistinct} md5Value FROM {tableName}"
                };
                var reader = database.ExecuteReader();
                while (reader.Read())
                {
                    //Every new row will create a new dictionary that holds the columns
                    // MD5 values is appended with the sentinel
                    databaseHolder.Append(reader["md5Value"]);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                using (
                    var fileStream = new FileStream(
                        filePath,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.Write))
                using (var bw = new BinaryWriter(fileStream))
                {
                    bw.Write(ex.Message + Environment.NewLine);
                }
            }
            databaseConnection.Close();
            return databaseHolder.ToString();
        }


        /// <summary>
        /// Return the lcp values stored in the database
        /// </summary>
        public static List<int> GetLcpTableByPattern(char pattern)
        {
            try
            {

                var lcpArray = new List<int>();
                var databaseConnection = SetupSqlSuffixArrayConnection();
                databaseConnection.Open();

                var lcpArrayCommand = new SqlCommand
                {
                    Connection = databaseConnection,
                    CommandText = $"SELECT lcpvalue FROM LCPARRAY_{pattern}"
                };
                var reader = lcpArrayCommand.ExecuteReader();
                while (reader.Read())
                {
                    // Every new row will create a new dictionary that holds the columns
                    var value = int.Parse(reader["lcpvalue"].ToString());
                    lcpArray.Add(value);
                }
                reader.Close();
                databaseConnection.Close();
                return lcpArray;

            }
            catch (Exception)
            {
                // Cast error to log
                return null;
            }
        }


        /// <summary>
        /// Return the lcp values stored in the database
        /// </summary>
        public static int[] GetLcpTableByPrefix(char prefix)
        {
            try
            {

                var lcpArray = new List<int>();
                var databaseConnection = SetupSqlSuffixArrayConnection();
                databaseConnection.Open();

                var lcpArrayCommand = new SqlCommand
                {
                    Connection = databaseConnection,
                    CommandText = $"SELECT lcpvalue FROM LCPARRAY_{prefix}"
                };
                var reader = lcpArrayCommand.ExecuteReader();
                while (reader.Read())
                {
                    // Every new row will create a new dictionary that holds the columns
                    var value = int.Parse(reader["lcpvalue"].ToString());
                    lcpArray.Add(value);
                }
                reader.Close();
                databaseConnection.Close();
                return lcpArray.ToArray();

            }
            catch (Exception)
            {
                // Cast error to log
                return null;
            }
        }


        /// <summary>
        /// Return all lcp table content as a collection of int lists
        /// </summary>
        public static Dictionary<char, int[]> GetAllLcpTableContent()
        {
            try
            {

                var collection = new Dictionary<char, int[]>();

                foreach (var tablename in LcpDatabaseNames)
                {
                    var split = tablename.Split('_');
                    var prefix = Convert.ToChar(split[1]);
                    var lcpArray = new List<int>();
                    var databaseConnection = SetupSqlSuffixArrayConnection();
                    databaseConnection.Open();

                    var lcpArrayCommand = new SqlCommand
                    {
                        Connection = databaseConnection,
                        CommandText = $"SELECT lcpvalue FROM {tablename}"
                    };
                    var reader = lcpArrayCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        //Every new row will create a new dictionary that holds the columns
                        var value = int.Parse(reader["lcpvalue"].ToString());
                        lcpArray.Add(value);
                    }
                    reader.Close();
                    databaseConnection.Close();
                    collection.Add(prefix, lcpArray.ToArray());

                    return collection;

                }
                return null;

            }
            catch (Exception)
            {
                // Cast error to log
                return null;
            }
        }


        /// <summary>
        /// Return the suffix array values stored in the database
        /// </summary>
        public static Dictionary<char, int[]> GetSuffixArrayTableByPattern(char prefix)
        {
            try
            {
                var suffixArrayDictionary = new Dictionary<char, int[]>();

                var databaseConnection = SetupSqlSuffixArrayConnection();
                var suffixArray = new List<int>();
                var suffixArrayCommand = new SqlCommand
                {
                    Connection = databaseConnection,
                    CommandText = $"SELECT suffixvalue FROM SUFFIXARRAY_{prefix}"
                };
                databaseConnection.Open();
                var reader = suffixArrayCommand.ExecuteReader();
                while (reader.Read())
                {
                    //Every new row will create a new dictionary that holds the columns
                    var value = int.Parse(reader["suffixvalue"].ToString());
                    suffixArray.Add(value);
                }
                reader.Close();
                databaseConnection.Close();
                suffixArrayDictionary.Add(prefix, suffixArray.ToArray());
                return suffixArrayDictionary;

            }
            catch (Exception)
            {

                // Cast error to log
                return null;
            }
        }


        /// <summary>
        /// Return the suffix array values stored in the database
        /// </summary>
        public static int[] GetSuffixArrayTableByPrefix(char prefix)
        {
            try
            {
                var databaseConnection = SetupSqlSuffixArrayConnection();
                var suffixArray = new List<int>();
                var suffixArrayCommand = new SqlCommand
                {
                    Connection = databaseConnection,
                    CommandText = $"SELECT suffixvalue FROM SUFFIXARRAY_{prefix}"
                };
                databaseConnection.Open();
                var reader = suffixArrayCommand.ExecuteReader();
                while (reader.Read())
                {
                    //Every new row will create a new dictionary that holds the columns
                    var value = int.Parse(reader["suffixvalue"].ToString());
                    suffixArray.Add(value);
                }
                reader.Close();
                databaseConnection.Close();
                return suffixArray.ToArray();

            }
            catch (Exception)
            {

                // Cast error to log
                return null;
            }
        }

        /// <summary>
        /// Return the suffix array values stored in the database by table name
        /// </summary>
        public static int[] GetSuffixArrayTableByTableName(string tableName)
        {
            try
            {
                var databaseConnection = SetupSqlSuffixArrayConnection();
                var suffixArray = new List<int>();
                var suffixArrayCommand = new SqlCommand
                {
                    Connection = databaseConnection,
                    CommandText = $"SELECT suffixvalue FROM {tableName}"
                };
                databaseConnection.Open();
                var reader = suffixArrayCommand.ExecuteReader();
                while (reader.Read())
                {
                    //Every new row will create a new dictionary that holds the columns
                    var value = int.Parse(reader["suffixvalue"].ToString());
                    suffixArray.Add(value);
                }
                reader.Close();
                databaseConnection.Close();
                return suffixArray.ToArray();

            }
            catch (Exception)
            {

                // Cast error to log
                return null;
            }
        }


        /// <summary>
        /// Return the suffix array values stored in the database by table name
        /// </summary>
        public static int[] GetLcpTableByTableName(string tableName)
        {
            try
            {
                var databaseConnection = SetupSqlSuffixArrayConnection();
                var suffixArray = new List<int>();
                var suffixArrayCommand = new SqlCommand
                {
                    Connection = databaseConnection,
                    CommandText = $"SELECT lcpvalue FROM {tableName}"
                };
                databaseConnection.Open();
                var reader = suffixArrayCommand.ExecuteReader();
                while (reader.Read())
                {
                    //Every new row will create a new dictionary that holds the columns
                    var value = int.Parse(reader["lcpvalue"].ToString());
                    suffixArray.Add(value);
                }
                reader.Close();
                databaseConnection.Close();
                return suffixArray.ToArray();

            }
            catch (Exception)
            {
                // Cast error to log
                return null;
            }
        }

        /// <summary>
        /// Return the malware values stored in the database as
        /// Retrun a string of all malwares in the defined table
        /// </summary>
        public static string GetMalwarValuesByTablename(string tablename)
        {
            try
            {
                var text = new StringBuilder();
                var databaseConnection = SetupSqlConnection();
                var suffixArrayCommand = new SqlCommand
                {
                    Connection = databaseConnection,
                    CommandText = $"SELECT md5Value FROM {tablename}"
                };
                databaseConnection.Open();
                var reader = suffixArrayCommand.ExecuteReader();
                while (reader.Read())
                {
                    //Every new row will create a new dictionary that holds the columns
                    var value = reader["md5Value"];
                    text.Append(value);
                }
                reader.Close();
                databaseConnection.Close();
                return text.ToString();

            }
            catch (Exception ex)
            {

                // Cast error to log
                return null;
            }
        }




        /// <summary>
        /// Clear table with name
        /// </summary>
        public static void ClearTable(string tablename)
        {
            var databaseConnection = SetupSqlSuffixArrayConnection();

            var suffixArrayCommand = new SqlCommand
            {
                Connection = databaseConnection,
                CommandText = $"truncate table {tablename}"
            };

            databaseConnection.Open();
            suffixArrayCommand.ExecuteReader();

        }


        /// <summary>
        /// Clear table with name
        /// </summary>
        public static void InsertTextIntoDatabase(string text, char prefix)
        {

            try
            {
                var stringlist = text.Split('$');
                stringlist = stringlist.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                var textlist = stringlist.Select(s => string.Concat(s, '$')).ToList();

                var databaseConnection = SetupSqlSuffixArrayConnection();
                databaseConnection.Open();
                for (var index = 0; index < textlist.Count; index++)
                {
                    var suffixArrayCommand = new SqlCommand
                    {
                        Connection = databaseConnection,
                        CommandText = $"INSERT INTO {"TEXT_" + prefix}(id, substring) VALUES (@id, @substring)"

                    };
                    suffixArrayCommand.Parameters.AddWithValue("@id", index);
                    suffixArrayCommand.Parameters.AddWithValue("@substring", textlist[index]);
                    suffixArrayCommand.ExecuteNonQuery();
                }
                databaseConnection.Close();

            }
            catch (Exception)
            {

                // Log this event
            }
        }


        /// <summary>
        /// Clear table with name
        /// </summary>
        public static string GetTextFromDatabase(char prefix)
        {

            var dict = new Dictionary<int, string>();

            var text = new StringBuilder();

            var databaseConnection = SetupSqlSuffixArrayConnection();
            var database = new SqlCommand
            {
                Connection = databaseConnection,
                CommandText = $"SELECT id, substring FROM TEXT_{prefix}"
            };

            databaseConnection.Open();
            var reader = database.ExecuteReader();
            while (reader.Read())
            {
                //Every new row will create a new dictionary that holds the columns
                dict.Add(int.Parse(reader["id"].ToString()), reader["substring"].ToString());
            }
            reader.Close();
            databaseConnection.Close();

            var list = dict.Keys.ToList();
            list.Sort();
            foreach (var key in list)
            {
                text.Append(dict[key]);
            }
            return text.ToString();
        }


        /// <summary>
        /// Clear table with name
        /// </summary>
        public static Dictionary<char, string> GetAllTextFromDatabase()
        {

            var completeDict = new Dictionary<char, string>();
            foreach (var databasetext in TextDatabaseNames)
            {
                var dict = new Dictionary<int, string>();

                var text = new StringBuilder();

                var databaseConnection = SetupSqlSuffixArrayConnection();
                var database = new SqlCommand
                {
                    Connection = databaseConnection,
                    CommandText = $"SELECT id, substring FROM {databasetext}"
                };

                databaseConnection.Open();
                var reader = database.ExecuteReader();
                while (reader.Read())
                {
                    //Every new row will create a new dictionary that holds the columns
                    dict.Add(int.Parse(reader["id"].ToString()), reader["substring"].ToString());
                }
                reader.Close();
                databaseConnection.Close();


                // We need them sorted as insert into database, not lexicographical
                var list = dict.Keys.ToList();
                list.Sort();
                foreach (var key in list)
                {
                    text.Append(dict[key]);
                }

                var split = databasetext.Split('_');
                completeDict.Add(Convert.ToChar(split[1]), text.ToString());

            }
            return completeDict;
        }
    }
}
