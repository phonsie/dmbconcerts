using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Flac;
using Flac.Metadata;
using musicFunctions;
using RestSharp;

namespace getMyTracksMD5s
{
    class getMyTracksMD5s
    {
        static string strSecretSecret = functions.generateMD5hash("Is this a godd secret scret=");

        static List<FileInfo> musicFiles = new List<FileInfo>();
        static List<FileInfo> musicFilesRedo = new List<FileInfo>();

        static List<OneMD5StringOnly> lstMyExistingMD5 = new List<OneMD5StringOnly>();
        static List<OneTrackFileName> lstExistingTrackNames = new List<OneTrackFileName>();
        static List<OneMD5> lstExistingMD5s = new List<OneMD5>();

        static List<string> lstMyExistingMD5Strings = new List<string>();
        static Dictionary<string, int> dctMD5IDs = new Dictionary<string, int>();
        static Dictionary<string, int> dctFileNameIDs = new Dictionary<string, int>();
        static DataTable dtOutPut = createOutputTable();

        static int MemberID = 1;
        static string strLogText = string.Empty;
        static string strLogFile = string.Empty;
        static string dataDir = Directory.GetCurrentDirectory() + "\\Data\\";
        static string outFile = dataDir + "log." + System.DateTime.Now.ToString("yyyy-MM-dd") + ".csv";

        /* 
         * 1. Download existing md5s for member and fileNameIDs
         * 2. Get md5s of music file on drive
         * 3. Store MID, new MD5, md5 (secretSecret, userSecret, MD5), fileNameID, fileName, track duration in mm:ss
         * 4. User then uploads the file onto the site, or auto-upload somehow?
         * 
         * Upload script parse file, creating new fileNameIDs are required and adding data to MyTracks.
         */

        static void Main(string[] args)
        {
            try
            {
                string strMemberSecret = args[0];
                string strValidMemberSecret = functions.getMD5FromString(strMemberSecret);
                if (strValidMemberSecret.Equals(""))
                {
                    throw new Exception("Secret is not in correct format!");
                }

                if (!Directory.Exists(args[1]))
                {
                    throw new Exception("Directory " + args[1] + " not found!");
                }

                int i = 0;
                Console.Write("Getting your existing tracks ... ");
                lstMyExistingMD5 = functions.getMyTracksByMemberID(MemberID);
                foreach (var item in lstMyExistingMD5)
                {
                    if (!lstMyExistingMD5Strings.Contains(item.MD5))
                    {
                        lstMyExistingMD5Strings.Add(item.MD5);
                    }
                    else
                    {
                        Console.WriteLine("Duplicate found " + i++);
                    }
                }
                Console.WriteLine("Done!");

                Console.Write("Getting existing MD5 strings from the system ...");
                int iLimit = 10000;
                for (int offset = 0; offset < 100001; offset = offset + iLimit)
                {
                    lstExistingMD5s = functions.getMD5s(iLimit, offset);
                    foreach (var item in lstExistingMD5s)
                    {
                        if (!dctMD5IDs.ContainsKey(item.MD5))
                        {
                            dctMD5IDs.Add(item.MD5, item.MD5ID);
                        }
                    }
                }
                Console.WriteLine("Done!");

                Console.Write("Getting existing track file names from the system ...");
                lstExistingTrackNames = functions.getTrackFileNames();
                foreach (var item in lstExistingTrackNames)
                {
                    if (!dctFileNameIDs.ContainsKey(item.FileName))
                    {
                        dctFileNameIDs.Add(item.FileName, item.FNID);
                    }
                }
                Console.WriteLine("Done!");


                if (args.Count<string>() != 2)
                {
                    Console.WriteLine(@"Please add your secret and state which folder the files are in e.g. C:\");
                    Console.WriteLine(@"Usage: getMyTracksMD5s.exe 162316667917b4be2956d0ffa5540e6d C:\");
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(args[1]);
                    strLogFile = di.FullName + @"\phMD5.log";
                    musicFiles.Clear();

                    Console.WriteLine("Finding .wav files");
                    FullDirList(di, musicFiles, "*.wav");
                    Console.WriteLine("Finding .flac files");
                    FullDirList(di, musicFiles, "*.flac");
                    Console.WriteLine("Finding .shn files");
                    FullDirList(di, musicFiles, "*.shn");
                    Console.WriteLine(musicFiles.Count + " files found");

                    processFiles(strMemberSecret);

                    musicFiles.Clear();

                    musicFiles = musicFilesRedo.ToList<FileInfo>();

                    processFiles(strMemberSecret);

                    functions.writeDataToFile(outFile, dtOutPut, "MD5ID", false);
                    functions.WriteToFile(strLogFile, strLogText, false);
                }
                Console.WriteLine("All Done! Please upload the file now as instructed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }

        private static void processFiles(string strMemberSecret)
        {
            int i = 1;

            bool bGetSHNDuration = File.Exists(Directory.GetCurrentDirectory() + @"\shntool.exe");

            foreach (var thisFile in musicFiles)
            {
                if (!thisFile.DirectoryName.Contains(@"E:\liveMusic\Other"))
                {
                    try
                    {
                        string strMD5 = string.Empty;
                        string strLength = string.Empty;
                        if (thisFile.FullName.EndsWith(".flac"))
                        {
                            FlacFile myFF = new Flac.FlacFile(thisFile.FullName);
                            Metadata myMetaData = myFF.Metadata;
                            strMD5 = myFF.StreamInfo.MD5String.ToLower();
                            strLength = getFFPDuration(myFF.Duration);
                        }
                        else
                        {
                            strMD5 = functions.getMD5OfFile(thisFile).ToLower();
                            strLength = bGetSHNDuration ? getSHNDuration(thisFile.FullName) : "";
                        }

                        string strFileName = Path.GetFileName(thisFile.FullName).ToLower();
                        if (lstMyExistingMD5Strings.Contains(strMD5))
                        {
                            strLogText += "existing" + functions.strDelimiter
                                    + strMD5 + functions.strDelimiter
                                    + thisFile.FullName
                                    + Environment.NewLine;
                        }
                        else
                        {
                            DataRow dr = dtOutPut.NewRow();
                            dr["MemberID"] = MemberID;
                            dr["MD5ID"] = dctMD5IDs.ContainsKey(strMD5) ? dctMD5IDs[strMD5] : 0;
                            dr["MD5"] = strMD5;

                            if (dctFileNameIDs.ContainsKey(strFileName))
                            {
                                dr["FNID"] = dctFileNameIDs[strFileName];
                                dr["FileName"] = "";
                            }
                            else
                            {
                                dr["FNID"] = 0;
                                dr["FileName"] = strFileName;
                            }
                            dr["Length"] = strLength;
                            dr["Checksum"] = functions.generateMD5hash(strSecretSecret + strMemberSecret + strMD5);
                            dtOutPut.Rows.Add(dr);

                            lstMyExistingMD5Strings.Add(strMD5);
                        }
                        //strLogText += i++ + ":" + musicFiles.Count + ";" + strMD5 + ";" + strFileName + Environment.NewLine;
                        strLogText += strFileName + ":" + strMD5 + Environment.NewLine;
                        Console.WriteLine(i++ + ":" + musicFiles.Count + ";" + strMD5 + ";" + strFileName);
                    }
                    catch (Exception ex)
                    {
                        // Redo files that fail, if the connection to the external disks fails some files might go "missing" temporarily
                        Console.WriteLine(ex.Message);
                        musicFilesRedo.Add(thisFile);
                    }
                }
            }
        }

        private static DataTable createOutputTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MemberID", typeof(int));
            dt.Columns.Add("MD5ID", typeof(int));
            dt.Columns.Add("MD5", typeof(string));
            dt.Columns.Add("FNID", typeof(int));
            dt.Columns.Add("FileName", typeof(string));
            dt.Columns.Add("Length", typeof(string));
            dt.Columns.Add("Checksum", typeof(string));
            return (dt);
        }

        private static string getFFPDuration(Time duration)
        {
            // Rounding with the help of:
            // http://stackoverflow.com/questions/338658/can-you-round-a-net-timespan-object
            const int TIMESPAN_SIZE = 7;
            int precision = 0;
            int factor = (int)Math.Pow(10, (TIMESPAN_SIZE - precision));

            TimeSpan interval = new TimeSpan();
            TimeSpan roundedTimeSpan = new TimeSpan();

            string strTrackLength = string.Empty;
            try
            {
                int iPosition = duration.ToString().LastIndexOf(':');
                strTrackLength = duration.ToString().Remove(iPosition, 1).Insert(iPosition, ".");
                TimeSpan.TryParseExact(strTrackLength, @"mm\:ss\.ff", CultureInfo.InvariantCulture, out interval);
                roundedTimeSpan = new TimeSpan(((long)Math.Round((1.0 * interval.Ticks / factor)) * factor));
            }
            catch (Exception)
            {
                // Just send back the empty string
            }
            return (roundedTimeSpan.ToString(@"mm\:ss"));
        }

        private static string getSHNDuration(string FileName)
        {
            // Rounding with the help of:
            // http://stackoverflow.com/questions/338658/can-you-round-a-net-timespan-object
            const int TIMESPAN_SIZE = 7;
            int precision = 0;
            int factor = (int)Math.Pow(10, (TIMESPAN_SIZE - precision));

            TimeSpan interval = new TimeSpan();
            TimeSpan roundedTimeSpan = new TimeSpan();

            string strTrackLength = string.Empty;
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = Directory.GetCurrentDirectory() + @"\shntool.exe";
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.Arguments = "len \"" + FileName + "\"";

                //
                // Start the process.
                //
                using (Process process = Process.Start(start))
                {
                    //
                    // Read in all the text from the process with the StreamReader.
                    //
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        string[] stringSeparators = new string[] { "\r\n", "\r", "\n" };
                        List<string> lstTrackDetails = result.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                        for (int i = 1; i < lstTrackDetails.Count - 1; i++)
                        {
                            string[] stringTabSeparators = new string[] { " " };
                            List<string> lstOneTrackDetails = lstTrackDetails[i].Split(stringTabSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                            strTrackLength = lstOneTrackDetails[0].PadLeft(8, '0');
                            TimeSpan.TryParseExact(strTrackLength, @"mm\:ss\.ff", CultureInfo.InvariantCulture, out interval);
                            roundedTimeSpan = new TimeSpan(((long)Math.Round((1.0 * interval.Ticks / factor)) * factor));
                        }
                        //Console.Write(result);
                    }
                }
            }
            catch (Exception)
            {
                // Just send back the empty string
            }
            return (roundedTimeSpan.ToString(@"mm\:ss"));
        }

        static void FullDirList(DirectoryInfo dir, List<FileInfo> thisList, string searchPattern)
        {
            try
            {
                foreach (FileInfo f in dir.GetFiles(searchPattern))
                {
                    thisList.Add(f);
                }
            }
            catch
            {
                return;
            }

            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                FullDirList(d, thisList, searchPattern);
            }
        }
    }
}
