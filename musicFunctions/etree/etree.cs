using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using musicFunctions;
using MySql.Data.MySqlClient;
using PH_Common_4._0;
using HtmlAgilityPack;
using RestSharp;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace etree
{
    class etree
    {
        #region Constants
        private static int VENUE_DATE = 1;
        private static int VENUE_NAME = 3;
        private static int VENUE_CITY = 5;
        private static int VENUE_STATE = 7;
        private static int SHOW_ID = 11;
        private static int SOURCE_IDS = 13;
        private static int COLUMN_COUNT = 15;
        #endregion

        #region URLs
        private static string etreeBaseURL = "http://db.etree.org/";
        private static string etreeShowsLink = "db/shows/browse/artist_key/";
        private static string etreeSourceLink = "shn/";
        #endregion

        #region Lists
        private static List<string> lstVenueMD5s = new List<string>();
        private static List<int> lstShowIDs = new List<int>();
        private static List<string> lstSourceIDs = new List<string>();
        private static List<string> lstNonStates = new List<string>();
        private static List<int> lstNoMD5sOnEtree = new List<int>();
        private static List<OneTrackName> lstOneTracksNames = new List<OneTrackName>();
        private static List<string> lstTracksNames = new List<string>();
        #endregion

        static string dataDir = Directory.GetCurrentDirectory() + "\\Data\\";
        static string inFile = dataDir + "dtLinks8.csv";
        static string tapersFile = dataDir + "showTapers2.csv";
        static string outFile = dataDir + "log." + System.DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
        static string outFileToImport = dataDir + @"sources\ToImport\log." + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".csv";
        static string trackNamesFile = Directory.GetCurrentDirectory() + "\\Data\\trackNames." + System.DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
        static string strOutput = string.Empty;

        private static string BaseUrl = ConfigurationManager.AppSettings["APIBaseURL"];
        private static string username = ConfigurationManager.AppSettings["username"];
        private static string password = ConfigurationManager.AppSettings["password"];
        private static string customHeader = ConfigurationManager.AppSettings["customHeader"];

        static void Main(string[] args)
        {
            var functions = new functions(BaseUrl, username, password, customHeader);

            // Delete a link to DT!! Just use the DT ID
            // IRestResponse irr1212 = functions.deleteDT(6717);            

            // http://dmb.phevey.com/md5Missing.php?ffp
            // Use this to update FFPs from Official Releases also.
            if (false)
            #region Update MD5ID e.g to fix all FFPs missing issues & Use this to update FFPs from Official Releases also.
            {

                // Get from website as below
                // http://dmb.phevey.com/sourceDetails.php?sourceID=94223&showID
                // Just make sure they're in sequence!
                int iTrackID = 92840;
                int iSourceID = 362057016;
                string strFFPpath = @"C:\Users\ph\Music\Dave Matthews Band\The Bayou\dmb1992-12-21.ffp";

                // This adds the new FFPs and then updates the tracks with newly created MD5ID
                List<string> lstTrackDetails = functions.returnFileContents(strFFPpath);

                OneShowSource oss = functions.getShowSource(iSourceID);
                if (checkIfMD5sAlreadyExist(oss, strFFPpath).Equals(false))
                {
                    int intColumnNumber = musicFunctions.functions.md5SortColumnNumber(lstTrackDetails[0], "File");
                    lstTrackDetails = lstTrackDetails.OrderBy(line => line.Substring(intColumnNumber)).ToList<string>();
                    lstTrackDetails = lstTrackDetails.Distinct<string>().ToList<string>();
                    for (int i = 0; i < lstTrackDetails.Count; i++)
                    {
                        string md5String = musicFunctions.functions.getMD5FromString(lstTrackDetails[i]).ToLower();
                        OneMD5 thisMD5 = new OneMD5();
                        thisMD5.MD5 = md5String;
                        int iMD5ID = functions.getIDFromRestResponse(functions.addMD5(thisMD5));
                        Console.WriteLine(iMD5ID);
                        OneTrack thisTrack = functions.getTrack(iTrackID++);
                        thisTrack.MD5ID = iMD5ID;
                        IRestResponse irrUT = functions.updateTrack(thisTrack);
                    }
                }
                else
                {
                    Console.WriteLine("An MD5 already exists!");
                }
            }
            #endregion

            if (false)
            #region BannedSources
            {
                List<OneBannedSource> lstBannedSources = new List<OneBannedSource>();
                OneBannedSource obs = new OneBannedSource();

                obs.SourceID = 2953;
                obs.Reason = "MD5s on Etree are from 5396";
                lstBannedSources.Add(obs);
                #region Done
                /*
                obs.SourceID = 3533;
                obs.Reason = "Post 1995 DSBD";
                lstBannedSources.Add(obs);
                obs.SourceID = 022260;
                obs.Reason = "25287 is supposedly better";
                lstBannedSources.Add(obs);
                obs.SourceID = 098325;
                obs.Reason = "This recording is missing Rhyme & Reason";
                lstBannedSources.Add(obs);
                /*
                obs.SourceID = 017998;
                obs.Reason = "95300 is more complete";
                lstBannedSources.Add(obs);
                obs.SourceID = 0;
                obs.Reason = "";
                lstBannedSources.Add(obs);
                */
                #endregion

                foreach (var item in lstBannedSources)
                {
                    IRestResponse irr = functions.addBannedSource(item);
                }
            }
            /*
            OneBannedSource obs2 = new OneBannedSource();
            obs2.SourceID = 124712;
            obs2.Reason = "Official Release";
            functions.addBannedSource(obs2);
            */
            #endregion

            if (false)
            #region Set All Official to Banned
            {
                List<OneShowSource> lstSources = functions.getShowSources();
                foreach (OneShowSource thisSource in lstSources)
                {
                    if (thisSource.SourceID > 362000000 && thisSource.SourceID < 372000000)
                    {
                        Console.WriteLine(thisSource.SourceID);
                        OneBannedSource obs = new OneBannedSource();
                        obs.SourceID = thisSource.SourceID;
                        obs.Reason = "Official Release";
                        functions.addBannedSource(obs);
                    }
                }
                //obs.Reason = "Post 1995 DSBD";
            }
            //functions.addBannedSource(obs);
            #endregion

            if (false)
            #region Banned Sources unit testing
            {
                // Remember to set the AUTO_INCREMENT on the primary key!
                List<OneBannedSource> lstBannedSources = functions.getBannedSources();

                OneBannedSource oneBannedSource = new OneBannedSource();
                oneBannedSource.Reason = "Test";
                oneBannedSource.SourceID = 12345;

                IRestResponse irr = functions.addBannedSource(oneBannedSource);
                int ID = functions.getIDFromRestResponse(irr);

                OneBannedSource oneBannedSource2 = functions.getBannedSource(ID);
                oneBannedSource2.Reason = "Update test";

                irr = functions.updateBannedSource(oneBannedSource2);

                lstBannedSources = functions.getBannedSources();

                irr = functions.deleteBannedSource(ID);

                lstBannedSources = functions.getBannedSources();

            }
            #endregion

            if (false)
            #region Change the source type from 24 to 16
            {
                OneShowSource oneShowSource = functions.getShowSource(131185);
                OneSourceInfo oneSourceInfo = functions.getSourceInfoFromShowSourceID(oneShowSource.SSID);
                oneSourceInfo.Type = 1;
                functions.updateSourceInfo(oneSourceInfo);
            }
            #endregion

            if (false)
            #region Add One Track Name
            {
                OneTrackName otn = new OneTrackName();
                otn.TrackName = "Be Yourself";
                functions.addTrackName(otn);
                otn.TrackName = "You And Me > Grey Street";
                //functions.addTrackName(otn);
            }
            #endregion

            #region IMPORT NEW SOURCES
            // 1. Get any new sources from all the DMB related pages
            if (false)
            #region GetNewShowsAndSources
            {
                lstNonStates.Add("Unknown");
                lstNonStates.Add("Mislabeled Show");

                List<OneVenue> lstVenues = functions.getVenues();
                foreach (var item in lstVenues)
                {
                    lstVenueMD5s.Add(item.VenueMD5);
                }

                List<OneShow> lstShows = functions.getShows();
                foreach (var item in lstShows)
                {
                    lstShowIDs.Add(item.EtreeID);
                }

                List<OneShowSource> lstShowSources = functions.getShowSources();
                foreach (var item in lstShowSources)
                {
                    lstSourceIDs.Add(item.SourceID.ToString());
                }

                bool bAll = true;
                if (bAll)
                {
                    List<OneArtist> lstArtists = functions.getArtists();
                    foreach (var item in lstArtists)
                    {
                        for (int i = 1989; i < DateTime.Now.Year + 1; i++)
                        {
                            getShowsAndSourceIDs(i, item.AID);
                            Console.WriteLine(i + ":" + item.Artist);
                        }
                    }
                }
                else
                {
                    getShowsAndSourceIDs(1995, 4329);
                }
            }
            #endregion

            // 2. Save the new ones to files, don't import them directly because of track names issues.
            if (false)
            #region Get Missing Source Details
            {
                List<OneFailedToParseSourceID> lstParseError = functions.getShowSourcesWhichFailedToParse();
                foreach (var item in lstParseError)
                {
                    if (item.SourceID < 362000000)
                    {
                        Console.WriteLine(item.SourceID);
                        OneShowSource oneShowSource = functions.getShowSource(item.SourceID);
                        saveSourceDetailsToFile(oneShowSource);
                    }
                }
            }
            #endregion

            // 2b. Save the new Official ones to files, don't import them directly because of track names issues.
            if (false)
            #region Get Missing Source Details
            {
                List<OneFailedToParseSourceID> lstParseError = functions.getShowSourcesWhichFailedToParse();
                foreach (var item in lstParseError)
                {
                    Console.WriteLine(item.SourceID);
                    saveSourceDetailsToFile(item.SourceID);
                }
            }
            #endregion

            // 3. Check the files for errors manually and fix them, after that import them
            if (true)
            #region Import New Sources From Files
            {
                List<OneFailedToParseSourceID> lstParseError = functions.getShowSourcesWhichFailedToParse();
                foreach (var item in lstParseError)
                {
                    //if (item.SourceID < 362000000)
                    {
                        OneShowSource oneShowSource = functions.getShowSource(item.SourceID);
                        getSourceDetailsFromFiles(oneShowSource, true);
                    }
                }
                if (!strOutput.Equals(string.Empty))
                {
                    PH_CF.WriteToFile(outFileToImport, strOutput, true);
                }
            }
            #endregion

            // 4. Link them to DT sources if the exist
            // Set bool bGetNewTorrents = true; in dt.cs

            // 5. Add the taper; *** done all ready at step 3! ***
            if (false)
            #region Add tapers
            {
                OneTaper ota = new OneTaper();
                ota.Name = "Official Live Trax";
                //IRestResponse irr3 = functions.addTaper(ota);
                List<int> list = new List<int>();
                list.Add(362587018);
                list.Add(362587020);
                list.Add(362587022);
                list.Add(362587023);
                list.Add(362587024);
                list.Add(362587025);
                list.Add(362587026);
                list.Add(362587027);
                list.Add(362587028);
                list.Add(362587030);
                list.Add(362587032);

                foreach (var item in list)
                {
                    OneShowSource oneShowSource = functions.getShowSource(item);
                    int iTaperID = functions.getTaperID(ota.Name);
                    int iType = getSourceType(oneShowSource);

                    int iSSID = oneShowSource.SSID;

                    if (iSSID > 0 && iTaperID > 0)
                    {
                        OneSourceInfo oneSourceInfo = new OneSourceInfo();
                        oneSourceInfo.TaperID = iTaperID;
                        oneSourceInfo.Type = iType;
                        oneSourceInfo.ShowSourceID = iSSID;

                        IRestResponse irr = functions.addSourceInfo(oneSourceInfo);
                        Console.WriteLine(functions.getLineForLog(irr));
                    }
                }
                PH_CF.WriteToFile(outFile, strOutput, false);
            }
            #endregion
            #endregion

            if (false)
            #region Correct wrong DT Link
            {
                OneShowSource oss = functions.getShowSource(8449);
                OneDT thisDT5 = functions.getDT(6243);
                thisDT5.ShowSourceID = oss.SSID;
                functions.updateDT(thisDT5);
            }
            #endregion

            if (false)
            #region Official Releases
            {
                //"TPID":"559","Name":"Official DMBLive"}
                //"TPID":"560","Name":"Official Live Recordings"}

                List<OneVenue> lstVenues = functions.getVenues();
                foreach (var item in lstVenues)
                {
                    lstVenueMD5s.Add(item.VenueMD5);
                }

                // 362057XXX For Live Series
                Dictionary<int, int> dctOfficial = new Dictionary<int, int>();
                #region Processed

                //dctOfficial.Add(2939, 362587012);
                /*
                dctOfficial.Add(2001, 362587018);
                dctOfficial.Add(6728, 362587019);
                dctOfficial.Add(116038, 362587020);
                dctOfficial.Add(2753, 362587022);
                dctOfficial.Add(109960, 362587023);
                dctOfficial.Add(109047, 362587024);
                dctOfficial.Add(2688, 362587025);
                dctOfficial.Add(20423, 362587026);
                dctOfficial.Add(34856, 362587027);
                dctOfficial.Add(24698, 362587028);
                dctOfficial.Add(110809, 362587030);
                dctOfficial.Add(124783, 362587031);
                dctOfficial.Add(81687, 362587032);
                */
                //dctOfficial.Add(81687, 362057001);
                /*
                //addSourceWhereNoneExists(1994, 3, 2, 6, 362057001, 362057001, 559, "Masquerade Nightclub", "Tampa", "FL",lstVenueMD5s);
                dctOfficial.Add(17533, 362057002);
                //addSourceWhereNoneExists(1995, 3, 25, 6, 362057003, 362057003, 559, "The Revolver Club", "Madrid", "Spain", lstVenueMD5s);
                //addSourceWhereNoneExists(1995, 2, 8, 6, 362057004, 362057004, 559, "", "", "", lstVenueMD5s);
                dctOfficial.Add(13832, 362057005);
                //addSourceWhereNoneExists(2004, 1, 9, 10, 362057006, 362057006, 559, "", "", "", lstVenueMD5s);
                dctOfficial.Add(82220, 362057007);
                dctOfficial.Add(29941, 362057008);
                dctOfficial.Add(14202, 362057009);
                dctOfficial.Add(3495, 362057010);
                dctOfficial.Add(17540, 362057011);
                dctOfficial.Add(124712, 362057012);
                dctOfficial.Add(17253, 362057013);
                dctOfficial.Add(2734, 362057014);
                dctOfficial.Add(18505, 362057015);
                //addSourceWhereNoneExists(1992, 12, 21, 6, 362057016, 362057016, 559, "", "", "", lstVenueMD5s);
                dctOfficial.Add(2820, 362057017);
                dctOfficial.Add(84319, 362057018);
                //addSourceWhereNoneExists(1996, 9, 9, 6, 362057019, 362057019, 559, "", "", "", lstVenueMD5s);
                //addSourceWhereNoneExists(1995, 7, 20, 6, 362057020, 362057020, 559, "", "", "", lstVenueMD5s);
                //addSourceWhereNoneExists(1993, 6, 13, 17, 362057021, 362057021, 559, "", "", "", lstVenueMD5s);
                dctOfficial.Add(96904, 362057022);
                dctOfficial.Add(91553, 362057023);
                dctOfficial.Add(31400, 362657000);
                dctOfficial.Add(31413, 362657001);
                dctOfficial.Add(31414, 362657002);
                dctOfficial.Add(31420, 362657003);                
                dctOfficial.Add(12716, 362657004);
                dctOfficial.Add(12717, 362657005);
                dctOfficial.Add(12718, 362657006);
                dctOfficial.Add(114340, 362657007);
                dctOfficial.Add(109868, 362657008);
                dctOfficial.Add(110302, 362657009);
                dctOfficial.Add(109035, 362657010);
                dctOfficial.Add(100146, 362657011);
                dctOfficial.Add(103620, 362657012);
                dctOfficial.Add(19846, 362657013);
                dctOfficial.Add(84613, 362657014);
                dctOfficial.Add(6614, 362657015);
                dctOfficial.Add(90358, 362657016);
                dctOfficial.Add(2978, 362657017);
                dctOfficial.Add(93492, 362657018);
                dctOfficial.Add(5140, 362657019);               
                dctOfficial.Add(2777, 362657020);
                */
                #endregion

                foreach (var item in dctOfficial)
                {
                    OneShowSource oss = functions.getShowSource(item.Key);

                    OneShowSource newOne = new OneShowSource();
                    newOne.ShowID = oss.ShowID;
                    newOne.SourceID = item.Value;
                    functions.addShowSource(newOne);

                    oss = functions.getShowSource(item.Value);

                    OneSourceInfo oneSourceInfo = new OneSourceInfo();
                    oneSourceInfo.TaperID = 560;
                    oneSourceInfo.Type = 3;
                    oneSourceInfo.ShowSourceID = oss.SSID;
                    IRestResponse irr = functions.addSourceInfo(oneSourceInfo);
                }
            }
            #endregion

            if (false)
            #region Rename One Track
            {
                OneTrackName thisTN = functions.getTrackName(2474);
                thisTN.TrackName = "Tim Solo";
                IRestResponse irr = functions.updateTrackName(thisTN);
            }
            #endregion

            if (false)
            #region Remove Source
            {
                int iSourceID = 2953;
                OneShowSource thisShowSource = functions.getShowSource(iSourceID);
                List<OneTrack> lstTracks = functions.getTrackIDsFromShowSourceID(thisShowSource.SSID);
                foreach (var item in lstTracks)
                {
                    IRestResponse irr = functions.deleteTrack(item.TID);
                    //irr = functions.deleteMD5(item.MD5ID);
                    //irr = functions.deleteTrackName(item.NameID);
                    //irr = functions.deleteTrackFileName(item.FileNameID);
                }
            }
            #endregion

            if (false)
            #region Set Track Length
            {
                List<OneTrack> lstTracks = functions.getTracks();

                ArrayList arlData = new ArrayList();
                arlData = functions.returnFileContents(@"C:\Program Files (x86)\EasyPHP-12.1\www\dmbonlinedb\Uploads\log.2014-02-27.csv", 7);
                foreach (string[] thisData in arlData)
                {
                    int iMD5ID = Int32.Parse(thisData[1]);
                    string strMD5 = thisData[2];
                    string strLength = thisData[5];

                    IEnumerable<OneTrack> tracksWithThisMD5ID = from t in lstTracks
                                                                where t.MD5ID.Equals(iMD5ID)
                                                                select t;
                    foreach (var item in tracksWithThisMD5ID)
                    {
                        if (item.Length.Equals("00:00:00"))
                        {
                            Console.WriteLine(item.TID + " " + strMD5);
                            OneTrack thisTrack = functions.getTrack(item.TID);
                            thisTrack.Length = "00:" + strLength;
                            functions.updateTrack(thisTrack);
                        }
                    }
                }
            }
            #endregion

            if (false)
            #region Export My Track IDs
            {
                List<OneMyTrack> lstMyTracks = functions.getMyTracks();
                foreach (var item in lstMyTracks)
                {
                    strOutput += item.MD5ID + ";" + PH_CF.strEOL;
                }
                PH_CF.WriteToFile(outFile, strOutput, false);
            }
            #endregion

            if (false)
            #region Delete some tracks from a show
            {
                OneShowSource oss21 = functions.getShowSource(16288);
                List<OneTrack> lstTraacks = functions.getTrackIDsFromShowSourceID(oss21.SSID);
                for (int i = 9; i < 16; i++)
                {
                    Console.WriteLine(lstTraacks[i].TID);
                    functions.deleteTrack(lstTraacks[i].TID);
                }
            }
            #endregion

            #region Misc
            /*
            for (int i = 84612; i < 84634; i++)
            {
                OneTrack ot = functions.getTrack(i);
                ot.Number = i - 84611;
                IRestResponse irrUpdateTrack = functions.updateTrack(ot);
            }
            */

            /*
            OneShow thisShow = new OneShow();
            thisShow.ArtistID = 6;
            thisShow.Day = 0;
            thisShow.EtreeID = 999999999;
            thisShow.Month = 0;
            thisShow.Year = 1939;   
            thisShow.VenueMD5 = "e81a2d35093fdd65a495df75813aec70";
            IRestResponse irrShow = functions.addShow(thisShow);                

            OneShowSource thisShowSource2 = new OneShowSource();
            thisShowSource2.ShowID = functions.getIDFromRestResponse(irrShow);
            thisShowSource2.SourceID = 999999999;
            IRestResponse irrShowSource = functions.addShowSource(thisShowSource2);
            */

            //IRestResponse irrDTN3 = functions.deleteTrackName(2350);
            #endregion

            if (false)
            #region One off Taper Entry
            {
                int iSourceID = 128797;
                string strType = "Flac16";
                string strTaper = "Matt Hucul";

                if (strTaper.Trim().Equals(""))
                {
                    strTaper = "Missing";
                }
                int iTaperID = functions.getTaperID(strTaper);

                int iSSID = functions.getShowSource(iSourceID).SSID;

                if (iSSID > 0 && iTaperID > 0)
                {
                    OneSourceInfo oneSourceInfo = new OneSourceInfo();
                    oneSourceInfo.TaperID = iTaperID;
                    oneSourceInfo.Type = strType.Equals("Flac24") ? 2 : 1;
                    oneSourceInfo.ShowSourceID = iSSID;

                    IRestResponse irr = functions.addSourceInfo(oneSourceInfo);
                    Console.WriteLine(functions.getLineForLog(irr));
                }
            }
            #endregion

            // http://dmb.phevey.com/shows.php?TNID=9582
            if (false)
            #region Fix Unclear from source text issue
            {
                int iSourceID = 94890;
                OneShowSource oss = functions.getShowSource(iSourceID);
                List<OneTrack> lstTracks = functions.getTrackIDsFromShowSourceID(oss.SSID);
                List<string> lstTrackNames = functions.returnFileContents(dataDir + @"sources\FixedArchive\Issues\TrackCountNotMatchingMD5s\" + oss.SourceID + ".tracks.txt");
                Dictionary<int, int> dctTrackNameIDs = new Dictionary<int, int>();
                // Work Around to fix issue with 3028
                //lstTracks.Add(lstTracks[0]);
                //lstTracks.RemoveAt(0);

                if (lstTracks.Count.Equals(lstTrackNames.Count))
                {
                    bool bAllTrackFileNamesExist = true;
                    for (int i = 0; i < lstTrackNames.Count; i++)
                    {
                        int iTFNID = functions.getTrackNameID(lstTrackNames[i].Trim(), false);
                        if (iTFNID.Equals(0))
                        {
                            bAllTrackFileNamesExist = false;
                            Console.WriteLine(iTFNID + " ; " + lstTrackNames[i]);
                        }
                        else
                        {
                            dctTrackNameIDs.Add(i, iTFNID);
                        }
                    }

                    if (bAllTrackFileNamesExist)
                    {
                        Console.WriteLine("All track names exist");
                        for (int i = 0; i < lstTrackNames.Count; i++)
                        {
                            OneTrack thisOneTrack = lstTracks[i];
                            thisOneTrack.NameID = dctTrackNameIDs[i];
                            IRestResponse irr = functions.updateTrack(thisOneTrack);
                            Console.WriteLine(irr.Content);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Fix those!");
                    }
                }
            }
            #endregion

            if (false)
            #region Fix Track Names
            {
                int iCorrectTNID = 3175












;
                /*
                OneTrackName thisTN = functions.getTrackName(iCorrectTNID);
                thisTN.TrackName = "#41 > Everyday";
                functions.updateTrackName(thisTN);
                */

                List<int> lstIncorrectTNID = new List<int>();

































                OneTrackName thisCorrectTrackName = functions.getTrackName(iCorrectTNID);

                foreach (var incorrectTNID in lstIncorrectTNID)
                {
                    List<OneTrackID> lstTrackIDs = functions.getTrackIDsFromNameID(incorrectTNID);
                    if (!iCorrectTNID.Equals(incorrectTNID))
                    {
                        foreach (var item in lstTrackIDs)
                        {
                            OneTrack thisTrack = functions.getTrack(item.TID);
                            OneTrackName thisInCorrectTrackName = functions.getTrackName(thisTrack.NameID);
                            Console.WriteLine(thisCorrectTrackName.TrackName + " <-- " + thisInCorrectTrackName.TrackName);
                            thisTrack.NameID = iCorrectTNID;
                            IRestResponse irrUT = functions.updateTrack(thisTrack);
                            //break;
                        }
                        IRestResponse irrDTN = functions.deleteTrackName(incorrectTNID);
                    }
                }
            }
            #endregion

            if (false)
            #region Get All Track Names
            {
                List<OneTrackName> lstTrackNames = functions.getTrackNames();
                foreach (var item in lstTrackNames)
                {
                    strOutput += item.TNID + PH_CF.strDelimiter;
                    strOutput += item.TrackName + PH_CF.strDelimiter;
                    strOutput += PH_CF.strEOL;
                }
                PH_CF.WriteToFile(trackNamesFile, strOutput, true);
            }
            #endregion

            if (false)
            #region Add DT links from file
            {
                ArrayList arlData = new ArrayList();
                arlData = functions.returnFileContents(inFile, 4, ";", true);

                foreach (string[] thisData in arlData)
                {
                    OneDT thisDT = new OneDT();
                    thisDT.DTID = Int32.Parse(thisData[0]);
                    thisDT.Free = Int32.Parse(thisData[3]);
                    thisDT.Name = thisData[2];
                    thisDT.ShowSourceID = 4460;

                    IRestResponse irr = functions.addDT(thisDT);
                    Console.WriteLine(irr.Content);
                }
            }
            #endregion

            if (false)
            #region Add DT Links
            {
                List<OneShowSource> lstShowSource;
                Dictionary<int, int> dctDTLinks = new Dictionary<int, int>();
                // Need to add the source for these as above ??
                // Something weird with there two ?? 
                // The problem was the name contained a ' 
                //dctDTLinks.Add(7954, 108987);
                //dctDTLinks.Add(5602, 2677);
                //dctDTLinks.Add(9924, 130412);
                //dctDTLinks.Add(9923, 130413);
                //dctDTLinks.Add(9999, 130726);
                //dctDTLinks.Add(10002, 130743);
                //dctDTLinks.Add(10072, 131493);
                //dctDTLinks.Add(7362, 2826);
                //130768
                


                foreach (var item in dctDTLinks)
                {
                    OneDT thisDT2 = functions.getDT(item.Key);
                    // Get this from DT or from the excel spreadsheet
                    OneShowSource oss = functions.getShowSource(item.Value);
                    thisDT2.ShowSourceID = oss.SSID;
                    thisDT2.Name = thisDT2.Name.Replace("'", "");
                    // Get this from the SSID at http://127.0.0.1:8080/Arrest/showsources/ 
                    // {"SSID":"4444","ShowID":"2035","SourceID":"111120399"} Based on the SourceID
                    IRestResponse irrDT = functions.updateDT(thisDT2);
                }
            }
            #endregion

            if (false)
            #region Add Conversion
            {
                //IRestResponse irrDeleteShowSource = functions.deleteShowSource(111002097);
                //IRestResponse irrDeleteSourceInfo = functions.deleteSourceInfo(4703);
                //for (int i = 108513; i < 108516; i++)
                {
                    int iSourceID = 12729;
                    //List<OneShowSource> lstShowSource2;
                    //lstShowSource2 = functions.getShowSources(iSourceID); getSourceDetailsFromFiles(lstShowSource2[0]);

                    OneShowSource thisShowSource = functions.getShowSource(iSourceID);
                    thisShowSource.SourceID = Int32.Parse("111" + iSourceID.ToString().PadLeft(6, '0'));
                    IRestResponse irr2 = functions.addShowSource(thisShowSource);

                    //thisShowSource.SourceID = Int32.Parse("121" + iSourceID.ToString().PadLeft(6, '0'));
                    //IRestResponse irr3 = functions.addShowSource(thisShowSource);

                    OneSourceInfo thisSourceInfo = functions.getSourceInfoFromShowSourceID(thisShowSource.SSID);
                    OneSourceInfo newSourceInfo = new OneSourceInfo();
                    newSourceInfo.ShowSourceID = functions.getIDFromRestResponse(irr2);
                    newSourceInfo.TaperID = thisSourceInfo.TaperID;
                    newSourceInfo.Type = thisSourceInfo.Type;
                    IRestResponse irr4 = functions.addSourceInfo(newSourceInfo);

                    /*
                    OneSourceInfo thisSourceInfo = functions.getSourceInfoFromShowSourceID(thisShowSource.SSID);                
                    OneSourceInfo newSourceInfo2 = new OneSourceInfo();
                    newSourceInfo2.ShowSourceID = functions.getIDFromRestResponse(irr3);
                    newSourceInfo2.TaperID = thisSourceInfo.TaperID;
                    newSourceInfo2.Type = thisSourceInfo.Type;
                    IRestResponse irr5 = functions.addSourceInfo(newSourceInfo);
                    */

                    List<OneShowSource> lstShowSource;
                    lstShowSource = functions.getShowSources(iSourceID); getSourceDetailsFromFiles(lstShowSource[0], true);
                    lstShowSource = functions.getShowSources(Int32.Parse("111" + iSourceID.ToString().PadLeft(6, '0'))); getSourceDetailsFromFiles(lstShowSource[0], true);
                    //lstShowSource = functions.getShowSources(Int32.Parse("121" + iSourceID.ToString().PadLeft(6, '0'))); getSourceDetailsFromFiles(lstShowSource[0]);                        
                }
            }
            #endregion

            if (false)
            #region Remove Duplicate Source Infos
            {
                Dictionary<int, OneSourceInfo> dupes = new Dictionary<int, OneSourceInfo>();
                List<OneSourceInfo> lstSourceInfos = functions.getSourceInfos();
                int i = 1;
                foreach (var item in lstSourceInfos)
                {
                    if (dupes.ContainsKey(item.ShowSourceID))
                    {
                        //Console.WriteLine(i++ + ";" + item.ShowSourceID + " is duplicated " + item.TaperID + " with " + dupes[item.ShowSourceID]);
                        OneSourceInfo osiToBeDeleted = dupes[item.ShowSourceID];
                        IRestResponse irr = functions.deleteSourceInfo(osiToBeDeleted.SIID);
                        Console.WriteLine(irr.Content);
                    }
                    else
                    {
                        dupes.Add(item.ShowSourceID, item);
                    }
                }
            }
            #endregion

            bool BulkRenameFiles = false;
            if (false)
            #region Bulk Rename Files
            {
                string[] strArrFiles = Directory.GetFiles(@"Z:\2003\AOK\d+t2003-03-19.akg483.shnf");
                foreach (var item in strArrFiles)
                {
                    if (item.EndsWith(".shn"))
                    {
                        Console.WriteLine(item);
                        try
                        {
                            File.Move(item, item.Replace("d&t", "d+t"));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            #endregion

            if (false)
            #region Rename tracks
            {
                /*
                OneTrackName otn = new OneTrackName();
                otn.TrackName = "Minarets > I Dreamed I Killed God";
                IRestResponse irrTN = functions.addTrackName(otn);
                */

                int iFirstTrackID = 21128;

                OneTrack ot = functions.getTrack(iFirstTrackID);
                ot.NameID = 141;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 1);
                ot.NameID = 627;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 2);
                ot.NameID = 142;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 3);
                ot.NameID = 1520;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 4);
                ot.NameID = 135;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 5);
                ot.NameID = 247;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 6);
                ot.NameID = 813;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 7);
                ot.NameID = 249;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 8);
                ot.NameID = 152;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 9);
                ot.NameID = 282;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 10);
                ot.NameID = 1544;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 11);
                ot.NameID = 1077;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 12);
                ot.NameID = 5;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 13);
                ot.NameID = 136;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 14);
                ot.NameID = 134;
                functions.updateTrack(ot);
                ot = functions.getTrack(iFirstTrackID + 15);
                ot.NameID = 1517;
                functions.updateTrack(ot);

                for (int i = 4322; i < 4337; i++)
                {
                    functions.deleteTrackName(i);
                }

                //functions.deleteTrack(84067);

                /*
                for (int i = 84646; i < 84657; i++)
                {
                    OneTrack ot = functions.getTrack(i);
                    ot.Number = i - 84634;
                    IRestResponse irrUpdateTrack = functions.updateTrack(ot);
                }
                */
            }
            #endregion

            if (false)
            #region Update Track
            {
                functions.deleteTrack(84611);
                //OneTrackFileName otfn = new OneTrackFileName();
                //otfn.FileName = "d&t1997-02-17.d1.t12.flac";
                //IRestResponse irr = functions.addTrackFileName(otfn);
                OneTrack thisTrack = functions.getTrack(84612);
                // Get correct MD5ID from e.g. http://127.0.0.1:8080/Arrest/md5exists/27840ae482f69eac797cb78b98b92a68
                thisTrack.MD5ID = 83974;
                IRestResponse irr2 = functions.updateTrack(thisTrack);
            }
            #endregion

            if (false)
            #region SortOutTrackFileNames
            {
                List<OneTrackFileName> lstOneTFN = new List<OneTrackFileName>();
                List<OneTrack> lstOneTrack = new List<OneTrack>();
                List<OneMyTrack> lstOneMyTrack = new List<OneMyTrack>();

                Dictionary<int, string> dctInUse = new Dictionary<int, string>();
                Dictionary<int, string> dctNotInUse = new Dictionary<int, string>();

                lstOneTrack = functions.getTracks();
                foreach (var item in lstOneTrack)
                {
                    if (!dctInUse.ContainsKey(item.FileNameID))
                    {
                        dctInUse.Add(item.FileNameID, "OT");
                    }
                    else
                    {
                        Console.WriteLine("Dupe");
                    }
                }

                // SELECT FileNameID FROM `tracks` 
                // UNION 
                // SELECT FileNameID FROM `mytracks`
                // 103,445 same as dctInUse.Count

                lstOneTFN = functions.getTrackFileNames();
                foreach (var item in lstOneTFN)
                {
                    if (!dctInUse.ContainsKey(item.FNID))
                    {
                        dctNotInUse.Add(item.FNID, item.FileName);
                    }
                }

                // SELECT FNID FROM `trackfilenames` WHERE FNID NOT IN (
                //      SELECT FileNameID FROM `tracks`
                //      UNION
                //      SELECT FileNameID FROM `mytracks` )
                // Returns 648 equal to dctNotInUse.Count

                foreach (var item in dctNotInUse)
                {
                    Console.WriteLine(item.Value);
                    functions.deleteTrackFileName(item.Key);
                }

            }
            #endregion

            if (false)
            #region Remove BOM symbol
            {
                List<int> lstInt = new List<int>();
                foreach (var item in lstInt)
                {
                    OneTrackFileName OneTF = functions.getTrackFileName(item);
                    OneTF.FileName = OneTF.FileName.Replace("\ufeff", "");
                    functions.updateTrackFileName(OneTF);
                }
            }
            #endregion

            //functions.deleteTrackFileName(110171);
            if (false)
            #region Sort Out Duplicates Track File Names
            {
                int i = 0;
                Dictionary<string, int> dctMD5sLower = new Dictionary<string, int>();
                List<OneTrackFileName> lst = functions.getTrackFileNames();

                foreach (var item in lst)
                {
                    if (!hasUpperCase(item.FileName))
                    {
                        dctMD5sLower.Add(item.FileName, item.FNID);
                    }
                }

                foreach (var item in lst)
                {
                    if (!dctMD5sLower.ContainsKey(item.FileName))
                    {
                        // Then it's an upper case one
                        Console.WriteLine(item.FNID);

                        List<OneTrackID> lstTrackIDs = functions.getTrackIDsFromFNID(item.FNID);
                        foreach (var trackItem in lstTrackIDs)
                        {
                            if (lstTrackIDs.Count > 1)
                            {
                                Console.WriteLine("Check this");
                            }
                            if (!trackItem.TID.Equals(0))
                            {
                                OneTrack thisTrack = functions.getTrack(trackItem.TID);
                                int iFirstFileNameID = dctMD5sLower[item.FileName];
                                //thisTrack.FileNameID = iFirstFileNameID;
                                Console.Write(thisTrack.FileNameID + " -> ");
                                Console.Write(thisTrack.FileNameID);
                                Console.WriteLine(" " + item.FileName + " " + functions.getTrackFileName(iFirstFileNameID).FileName);

                                functions.deleteTrackFileName(item.FNID);

                                /*
                                //PH_CF.pause(50);
                                functions.updateTrack(thisTrack);
                                bDeleteTrackFileName = true;    
                                */
                            }
                            else
                            {
                                functions.deleteTrackFileName(item.FNID);
                                //Console.WriteLine("Deleting " + item.FileName);
                            }
                        }
                    }
                }
                PH_CF.WriteToFile(outFile, strOutput, false);
            }
            #endregion
        }

        private static void addSourceWhereNoneExists(int Year, int Month, int Day, int ArtistID, int ShowID, int SourceID, int TaperID, string VenueName, string VenueCity, string VenueState, List<string> lstVenueMD5s)
        {
            var functions = new functions(BaseUrl, username, password, customHeader);

            List<OneShow> lstShows = functions.getShowsForDateAndArtist(Year, Month, Day, ArtistID);

            if (lstShows.Count.Equals(0))
            {
                string strVenueMD5 = PH_CF.getMD5hash(VenueName + VenueCity + VenueState);

                #region Venue Entry
                if (!lstVenueMD5s.Contains(strVenueMD5))
                {
                    #region state/country
                    string strState = string.Empty;
                    string strCountry = string.Empty;
                    if (VenueState.Length.Equals(2) || lstNonStates.Contains(VenueState))
                    {
                        strState = VenueState;
                        strCountry = "US";
                    }
                    else
                    {
                        strState = "";
                        strCountry = VenueState;
                    }
                    #endregion

                    OneVenue thisVenue = new OneVenue();
                    thisVenue.CityID = functions.getCityID(VenueCity);
                    thisVenue.VenueMD5 = strVenueMD5;
                    thisVenue.VenueName = VenueName;
                    thisVenue.State = strState;
                    thisVenue.CountryID = functions.getCountryID(strCountry);
                    lstVenueMD5s.Add(strVenueMD5);
                    IRestResponse irrVenue = functions.addVenue(thisVenue);
                    Console.WriteLine("New venue found:" + thisVenue.VenueName);
                    strOutput += functions.getLineForLog(irrVenue) + PH_CF.strEOL;
                }
                #endregion

                OneShow thisShow = new OneShow();
                thisShow.ArtistID = ArtistID;
                thisShow.Day = Day;
                thisShow.EtreeID = ShowID;
                thisShow.Month = Month;
                thisShow.Year = Year;
                thisShow.VenueMD5 = strVenueMD5;
                Console.WriteLine("New show added:" + thisShow.EtreeID);

                OneShowSource thisShowSource = new OneShowSource();
                thisShowSource.ShowID = functions.getIDFromRestResponse(functions.addShow(thisShow));
                thisShowSource.SourceID = SourceID;
                IRestResponse irrShowSource = functions.addShowSource(thisShowSource);

                int iSSID = functions.getIDFromRestResponse(irrShowSource);

                OneSourceInfo oneSourceInfo = new OneSourceInfo();
                oneSourceInfo.TaperID = TaperID;
                oneSourceInfo.Type = 3;
                oneSourceInfo.ShowSourceID = iSSID;

                IRestResponse irr = functions.addSourceInfo(oneSourceInfo);
                Console.WriteLine(functions.getLineForLog(irr));
            }
            else if (lstShows.Count.Equals(1))
            {
                OneShowSource thisShowSource = new OneShowSource();
                thisShowSource.ShowID = lstShows[0].SID;
                thisShowSource.SourceID = SourceID;
                IRestResponse irrShowSource = functions.addShowSource(thisShowSource);

                int iSSID = functions.getIDFromRestResponse(irrShowSource);

                OneSourceInfo oneSourceInfo = new OneSourceInfo();
                oneSourceInfo.TaperID = TaperID;
                oneSourceInfo.Type = 3;
                oneSourceInfo.ShowSourceID = iSSID;

                IRestResponse irr = functions.addSourceInfo(oneSourceInfo);
                Console.WriteLine(functions.getLineForLog(irr));
            }
            else if (lstShows.Count.Equals(3))
            {
                OneShowSource thisShowSource = new OneShowSource();
                thisShowSource.ShowID = lstShows[2].SID;
                thisShowSource.SourceID = SourceID;
                IRestResponse irrShowSource = functions.addShowSource(thisShowSource);

                int iSSID = functions.getIDFromRestResponse(irrShowSource);

                OneSourceInfo oneSourceInfo = new OneSourceInfo();
                oneSourceInfo.TaperID = TaperID;
                oneSourceInfo.Type = 3;
                oneSourceInfo.ShowSourceID = iSSID;

                IRestResponse irr = functions.addSourceInfo(oneSourceInfo);
                Console.WriteLine(functions.getLineForLog(irr));
            }
            else
            {
                Console.WriteLine("More than one show exists");
            }
        }

        private static bool hasUpperCase(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]))
                    return true;
            }
            return false;
        }

        private static void setArtists()
        {
            var functions = new functions(BaseUrl, username, password, customHeader);

            List<string> lstArtistShortNames = new List<string>();
            lstArtistShortNames.Add("dmb");
            lstArtistShortNames.Add("dm+f");
            lstArtistShortNames.Add("d+t");
            lstArtistShortNames.Add("dm");

            List<string> lstArtistNames = new List<string>();
            lstArtistNames.Add("Dave Matthews Band");
            lstArtistNames.Add("Dave Matthews & Friends");
            lstArtistNames.Add("Dave Matthews & Tim Reynolds");
            lstArtistNames.Add("Dave Matthews (solo)");

            List<int> lstArtistIDs = new List<int>();
            lstArtistIDs.Add(6);
            lstArtistIDs.Add(16458);
            lstArtistIDs.Add(17);
            lstArtistIDs.Add(10);

            for (int i = 0; i < 4; i++)
            {
                OneArtist oneArtist = new OneArtist();
                oneArtist.AID = lstArtistIDs[i];
                oneArtist.Artist = lstArtistNames[i];
                oneArtist.ShortName = lstArtistShortNames[i];
                functions.updateArtist(oneArtist);
            }
        }

        private static bool checkIfMD5sAlreadyExist(OneShowSource oneShowSource, string filepath)
        {
            Boolean bMD5xists = false;
            try
            {
                var functions = new functions(BaseUrl, username, password, customHeader);
                string strDirectory = string.Empty;
                List<string> lstTrackDetails = functions.returnFileContents(filepath);

                if (lstTrackDetails.Count > 0)
                {
                    int intColumnNumber = musicFunctions.functions.md5SortColumnNumber(lstTrackDetails[0], "File");

                    lstTrackDetails = lstTrackDetails.OrderBy(line => line.Substring(intColumnNumber)).ToList<string>();
                    lstTrackDetails = lstTrackDetails.Distinct<string>().ToList<string>();

                    for (int i = 0; i < lstTrackDetails.Count; i++)
                    {
                        string md5String = musicFunctions.functions.getMD5FromString(lstTrackDetails[i]).ToLower();
                        if (!functions.getMD5(md5String).MD5ID.Equals(0))
                        {
                            bMD5xists = true;
                            string strMD5 = md5String;
                            Console.WriteLine(functions.getMD5(md5String).MD5ID + ";" + strMD5);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return (bMD5xists);
        }

        private static void getSourceDetailsFromFiles(OneShowSource oneShowSource, bool addTaper)
        {
            try
            {
                var functions = new functions(BaseUrl, username, password, customHeader);

                //TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                string strDirectory = string.Empty;
                strDirectory = "ToImport\\";
                List<string> lstTrackDetails = functions.returnFileContents(dataDir + "sources\\" + strDirectory + oneShowSource.SourceID + ".md5s.txt");

                if (lstTrackDetails.Count > 0)
                {
                    int intColumnNumber = musicFunctions.functions.md5SortColumnNumber(lstTrackDetails[0], "File");

                    lstTrackDetails = lstTrackDetails.OrderBy(line => line.Substring(intColumnNumber)).ToList<string>();
                    lstTrackDetails = lstTrackDetails.Distinct<string>().ToList<string>();

                    List<string> lstTrackNames = functions.returnFileContents(dataDir + "sources\\" + strDirectory + oneShowSource.SourceID + ".tracks.txt");
                    /*
                    List<string> lstTrackNames = new List<string>();
                    lstTrackNames.Add("Unclear from source text");
                    for (int i = 1; i < lstTrackDetails.Count; i++)
                    {
                        lstTrackNames.Add("Unclear from source text dupe");
                    }
                    */

                    lstTrackNames.RemoveAll(TrackNamesPredicate);

                    if (lstTrackNames.Count.Equals(lstTrackDetails.Count))
                    {
                        List<OneTrackName> lstAllTrackNames = functions.getTrackNames();
                        bool bAllTrackNamesExist = true;
                        for (int i = 0; i < lstTrackNames.Count; i++)
                        {
                            IEnumerable<OneTrackName> existingTrackName = from t in lstAllTrackNames
                                                                          where t.TrackName.Equals(lstTrackNames[i].Trim())
                                                                          select t;

                            try
                            {
                                if (existingTrackName.FirstOrDefault<OneTrackName>().Equals(null))
                                {
                                    Console.WriteLine("Doesn't match!");
                                }

                            }
                            catch (NullReferenceException nre)
                            {
                                bAllTrackNamesExist = false;
                                strOutput += lstTrackNames[i] + " ;doesn't exist! for;" + oneShowSource.SourceID + PH_CF.strEOL;
                                Console.WriteLine(lstTrackNames[i] + " doesn't exist!");
                            }
                        }

                        if (bAllTrackNamesExist)
                        {
                            for (int i = 0; i < lstTrackDetails.Count; i++)
                            {
                                OneTrack thisTrack = new OneTrack();
                                thisTrack.ShowSourceID = oneShowSource.SSID;
                                thisTrack.NameID = functions.getTrackNameID(lstTrackNames[i].Trim(), false);
                                thisTrack.Number = i + 1;
                                string md5String = musicFunctions.functions.getMD5FromString(lstTrackDetails[i]).ToLower();
                                thisTrack.MD5ID = functions.getMD5ID(md5String);
                                //if (i.Equals(0))
                                {
                                    //thisTrack.FileNameID = 111186;                                    
                                }
                                //else
                                {
                                    thisTrack.FileNameID = functions.getTrackFileNameID(lstTrackDetails[i].ToLower().Replace(md5String, "").Trim().Replace("*", "").Replace(":", ""));
                                }
                                thisTrack.Length = "";
                                thisTrack.AddedTime = functions.ConvertToUnixTimestamp(DateTime.Now);
                                IRestResponse irrAddTrack = functions.addTrack(thisTrack);
                                Console.WriteLine(functions.getLineForLog(irrAddTrack));
                            }

                            if (addTaper)
                            {
                                List<string> lstTaper = functions.returnFileContents(dataDir + "sources\\" + strDirectory + oneShowSource.SourceID + ".taper.txt");
                                try
                                {
                                    if (lstTaper.Count.Equals(2))
                                    {
                                        int iTaperID = functions.getTaperID(lstTaper[0]);
                                        int iSSID = oneShowSource.SSID;

                                        if (iSSID > 0 && iTaperID > 0)
                                        {
                                            OneSourceInfo oneSourceInfo = new OneSourceInfo();
                                            oneSourceInfo.TaperID = iTaperID;
                                            oneSourceInfo.Type = Int32.Parse(lstTaper[1]);
                                            oneSourceInfo.ShowSourceID = iSSID;

                                            IRestResponse irr = functions.addSourceInfo(oneSourceInfo);
                                            Console.WriteLine(functions.getLineForLog(irr));
                                        }
                                    }
                                    else
                                    {
                                        strOutput += "Failed to get taper for;" + oneShowSource.SourceID + PH_CF.strEOL;
                                        Console.WriteLine("Failed to get taper for " + oneShowSource.SourceID);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    strOutput += "Failed to get taper for;" + oneShowSource.SourceID + PH_CF.strEOL;
                                    Console.WriteLine("Failed to get taper for " + oneShowSource.SourceID);
                                }
                            }
                        }
                    }
                    else
                    {
                        functions.WriteToFile(dataDir + "sources\\" + oneShowSource.SourceID + ".md5s.txt", string.Join(Environment.NewLine, lstTrackDetails), false);
                        functions.WriteToFile(dataDir + "sources\\" + oneShowSource.SourceID + ".tracks.txt", string.Join(Environment.NewLine, lstTrackNames), false);
                        strOutput += "Counts don't match for;" + oneShowSource.SourceID + PH_CF.strEOL;
                        Console.WriteLine("Counts don't match for " + oneShowSource.SourceID);
                    }


                }
                else
                {
                    strOutput += "No checksums for;" + oneShowSource.SourceID + PH_CF.strEOL;
                    Console.WriteLine("No checksums for " + oneShowSource.SourceID);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void getSourceDetailsWhichHaveNoMD5s3(OneShowSource oneShowSource)
        {
            /*
            try
            {
                var functions = new functions(BaseUrl, username, password, customHeader);

                HtmlDocument doc = musicFunctions.functions.getHTMLDoc(etreeBaseURL + etreeSourceLink + oneShowSource.SourceID.ToString());
                var details = doc.DocumentNode
                                 .Descendants("div")
                                 .Where(div => div.GetAttributeValue("class", "").Contains("md5detail") && div.GetAttributeValue("class", "").Contains("b1"))
                                 .ToArray();

                var details2 = doc.DocumentNode
                                  .Descendants("div")
                                  .Where(div => div.GetAttributeValue("class", "").Contains("md5detail") && div.GetAttributeValue("class", "").Contains("b2"))
                                  .ToArray();

                var sourceTextNode = doc.DocumentNode
                                  .Descendants("div")
                                  .Where(div => div.GetAttributeValue("class", "").Contains("b1 t"))
                                  .ToArray();

                List<OneArtist> lstArtists = functions.getArtists();

                HtmlNodeCollection hncArtistID = doc.DocumentNode.SelectNodes("/html[1]/body[1]/center[1]/div[3]/div[1]/a[1]");
                string strArtistID = hncArtistID[0].OuterHtml.Substring(29, hncArtistID[0].OuterHtml.IndexOf("&") - 29);
                int iArtistID = 0;
                HtmlNodeCollection hncDate = doc.DocumentNode.SelectNodes("/html[1]/body[1]/center[1]/div[3]/div[1]/a[2]");

                IEnumerable<OneArtist> thisArtists = from c in lstArtists
                                                     where c.AID.Equals(Int32.Parse(strArtistID))
                                                     select c;

                OneArtist thisArtist = thisArtists.First<OneArtist>();


                string strDate = hncDate[0].InnerHtml;
                string strYear = strDate.Substring(6, 2);
                string strMonth = strDate.Substring(0, 2);
                string strDay = strDate.Substring(3, 2);
                string strTrackName = thisArtist.ShortName + strYear + "-" + strMonth + "-" + strDay + "t";

                string strSourceText = sourceTextNode[0].InnerText;
                strSourceText = strSourceText.Replace("&nbsp;", " ");
                string strSourceDetails = string.Empty;
                foreach (HtmlNode sourceDetails in details)
                {
                    strSourceDetails += sourceDetails.InnerText.Trim();
                    strSourceDetails += "\r\n";
                }
                foreach (HtmlNode sourceDetails in details2)
                {
                    strSourceDetails += sourceDetails.InnerText.Trim();
                    strSourceDetails += "\r\n";
                }

                string[] stringSeparators = new string[] { "\r\n", "\r", "\n" };

                string strDirectory = @"Issues\";
                strDirectory = string.Empty;

                List<string> lstTrackNames = functions.returnFileContents(dataDir + "sources\\" + strDirectory + oneShowSource.SourceID + ".tracks.txt"); ;
                lstTrackNames.RemoveAll(TrackNamesPredicate);

                //List<string> lstTrackNames = getTrackNames(strSourceText, 0);
                lstTrackNames.RemoveAll(TrackNamesPredicate);

                for (int i = 0; i < lstTrackNames.Count; i++)
                {
                    OneTrack thisTrack = new OneTrack();
                    thisTrack.ShowSourceID = oneShowSource.SSID;
                    thisTrack.NameID = functions.getTrackNameID(lstTrackNames[i]);
                    thisTrack.Number = i + 1;
                    string md5String = i > 0 ? "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" + i.ToString().PadLeft(2, '0') : "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                    thisTrack.MD5ID = functions.getMD5ID(md5String);
                    thisTrack.FileNameID = functions.getTrackFileNameID(strTrackName + i.ToString().PadLeft(2, '0'));
                    thisTrack.Length = "";
                    IRestResponse irrAddTrack = functions.addTrack(thisTrack);
                    Console.WriteLine(functions.getLineForLog(irrAddTrack));
                }
                functions.WriteToFile(dataDir + "sources\\" + oneShowSource.SourceID + ".tracks.txt", string.Join(Environment.NewLine, lstTrackNames), false);
                functions.WriteToFile(dataDir + "sources\\" + oneShowSource.SourceID + ".source.txt", string.Join(Environment.NewLine, strSourceText), false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            */
        }

        private static void getSourceDetails3(OneShowSource oneShowSource)
        {
            /*
            try
            {
                var functions = new functions(BaseUrl, username, password, customHeader);

                //if (!lstNoMD5sOnEtree.Contains(oneShowSource.SourceID))
                {
                    HtmlDocument doc = musicFunctions.functions.getHTMLDoc(etreeBaseURL + etreeSourceLink + oneShowSource.SourceID.ToString());
                    var details = doc.DocumentNode
                                     .Descendants("div")
                                     .Where(div => div.GetAttributeValue("class", "").Contains("md5detail") && div.GetAttributeValue("class", "").Contains("b1"))
                                     .ToArray();

                    var details2 = doc.DocumentNode
                                      .Descendants("div")
                                      .Where(div => div.GetAttributeValue("class", "").Contains("md5detail") && div.GetAttributeValue("class", "").Contains("b2"))
                                      .ToArray();

                    var sourceTextNode = doc.DocumentNode
                                      .Descendants("div")
                                      .Where(div => div.GetAttributeValue("class", "").Contains("b1 t"))
                                      .ToArray();

                    string strSourceText = sourceTextNode[0].InnerText;
                    strSourceText = strSourceText.Replace("&nbsp;", " ");
                    string strSourceDetails = string.Empty;
                    foreach (HtmlNode sourceDetails in details)
                    {
                        strSourceDetails += sourceDetails.InnerText.Trim();
                        strSourceDetails += "\r\n";
                    }
                    foreach (HtmlNode sourceDetails in details2)
                    {
                        strSourceDetails += sourceDetails.InnerText.Trim();
                        strSourceDetails += "\r\n";
                    }

                    string[] stringSeparators = new string[] { "\r\n", "\r", "\n" };
                    List<string> lstTrackDetails = strSourceDetails.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                    lstTrackDetails.Remove("Disc One:");

                    if (lstTrackDetails.Count > 0)
                    {
                        int intColumnNumber = musicFunctions.functions.md5SortColumnNumber(lstTrackDetails[0], "File");

                        if (lstTrackDetails[0].Contains(".flac"))
                        {
                            List<string> lstTemp = new List<string>();
                            foreach (var item in lstTrackDetails)
                            {
                                if (musicFunctions.functions.md5SortColumnNumber(item, "File") != 0)
                                {
                                    lstTemp.Add(item);
                                }
                            }
                            foreach (var item in lstTemp)
                            {
                                lstTrackDetails.Remove(item);
                            }
                        }

                        lstTrackDetails = lstTrackDetails.OrderBy(line => line.Substring(intColumnNumber)).ToList<string>();
                        lstTrackDetails = lstTrackDetails.Distinct<string>().ToList<string>();

                        List<string> lstTrackNames = getTrackNames(strSourceText, lstTrackDetails.Count);
                        lstTrackNames.RemoveAll(TrackNamesPredicate);

                        if (lstTrackNames.Count.Equals(lstTrackDetails.Count))
                        {
                            for (int i = 0; i < lstTrackDetails.Count; i++)
                            {
                                OneTrack thisTrack = new OneTrack();
                                thisTrack.ShowSourceID = oneShowSource.SSID;
                                thisTrack.NameID = functions.getTrackNameID(lstTrackNames[i]);
                                thisTrack.Number = i + 1;
                                string md5String = musicFunctions.functions.getMD5FromString(lstTrackDetails[i]).ToLower();
                                thisTrack.MD5ID = functions.getMD5ID(md5String);
                                thisTrack.FileNameID = functions.getTrackFileNameID(lstTrackDetails[i].ToLower().Replace(md5String, "").Trim().Replace("*", "").Replace(":", ""));
                                thisTrack.Length = "";
                                IRestResponse irrAddTrack = functions.addTrack(thisTrack);
                                Console.WriteLine(functions.getLineForLog(irrAddTrack));
                            }
                        }
                        else
                        {
                            functions.WriteToFile(dataDir + "sources\\" + oneShowSource.SourceID + ".md5s.txt", string.Join(Environment.NewLine, lstTrackDetails), false);
                            functions.WriteToFile(dataDir + "sources\\" + oneShowSource.SourceID + ".tracks.txt", string.Join(Environment.NewLine, lstTrackNames), false);
                            functions.WriteToFile(dataDir + "sources\\" + oneShowSource.SourceID + ".source.txt", string.Join(Environment.NewLine, strSourceText), false);
                            strOutput += "Counts don't match for;" + oneShowSource.SourceID + PH_CF.strEOL;
                            Console.WriteLine("Counts don't match for " + oneShowSource.SourceID);
                        }
                    }
                    else
                    {
                        strOutput += "No checksums for;" + oneShowSource.SourceID + PH_CF.strEOL;
                        Console.WriteLine("No checksums for " + oneShowSource.SourceID);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            */
        }

        private static int getSourceType(OneShowSource oneShowSource)
        {
            int iSourceType = 1;
            HtmlDocument doc = musicFunctions.functions.getHTMLDoc(etreeBaseURL + etreeSourceLink + oneShowSource.SourceID.ToString());
            var sourceType = doc.DocumentNode
                              .Descendants("td")
                              .Where(div => div.GetAttributeValue("colspan", "").Contains("3"))
                              .ToArray();
            try
            {
                if (sourceType[0].InnerText.StartsWith("FLAC24"))
                {
                    iSourceType = 2;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to check FLAC24 type");
            }
            return iSourceType;
        }

        private static void saveSourceDetailsToFile(OneShowSource oneShowSource)
        {
            try
            {
                HtmlDocument doc = musicFunctions.functions.getHTMLDoc(etreeBaseURL + etreeSourceLink + oneShowSource.SourceID.ToString());
                var details = doc.DocumentNode
                                 .Descendants("div")
                                 .Where(div => div.GetAttributeValue("class", "").Contains("md5detail") && div.GetAttributeValue("class", "").Contains("b1"))
                                 .ToArray();

                var details2 = doc.DocumentNode
                                  .Descendants("div")
                                  .Where(div => div.GetAttributeValue("class", "").Contains("md5detail") && div.GetAttributeValue("class", "").Contains("b2"))
                                  .ToArray();

                var sourceTextNode = doc.DocumentNode
                                  .Descendants("div")
                                  .Where(div => div.GetAttributeValue("class", "").Contains("b1 t"))
                                  .ToArray();

                var sourceType = doc.DocumentNode
                                  .Descendants("td")
                                  .Where(div => div.GetAttributeValue("colspan", "").Contains("3"))
                                  .ToArray();

                string strSourceText = sourceTextNode[0].InnerText;
                strSourceText = strSourceText.Replace("&nbsp;", " ");

                string strTaper = getSourceTaper(strSourceText);
                string strType = "1";
                try
                {
                    if (sourceType[0].InnerText.StartsWith("FLAC24"))
                    {
                        strType = "2";
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to check FLAC24 type");
                }

                string strSourceDetails = string.Empty;
                foreach (HtmlNode sourceDetails in details)
                {
                    strSourceDetails += sourceDetails.InnerText.Trim();
                    strSourceDetails += "\r\n";
                }
                foreach (HtmlNode sourceDetails in details2)
                {
                    strSourceDetails += sourceDetails.InnerText.Trim();
                    strSourceDetails += "\r\n";
                }

                string[] stringSeparators = new string[] { "\r\n", "\r", "\n" };
                List<string> lstTrackDetails = strSourceDetails.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                lstTrackDetails.Remove("Disc One:");

                if (lstTrackDetails.Count > 0)
                {
                    int intColumnNumber = musicFunctions.functions.md5SortColumnNumber(lstTrackDetails[0], "File");

                    if (lstTrackDetails[0].Contains(".flac"))
                    {
                        List<string> lstTemp = new List<string>();
                        foreach (var item in lstTrackDetails)
                        {
                            if (musicFunctions.functions.md5SortColumnNumber(item, "File") != 0)
                            {
                                lstTemp.Add(item);
                            }
                        }
                        foreach (var item in lstTemp)
                        {
                            lstTrackDetails.Remove(item);
                        }
                    }

                    lstTrackDetails = lstTrackDetails.OrderBy(line => line.Substring(intColumnNumber)).ToList<string>();
                    lstTrackDetails = lstTrackDetails.Distinct<string>().ToList<string>();

                    List<string> lstTrackNames = getTrackNames(strSourceText, lstTrackDetails.Count);
                    lstTrackNames.RemoveAll(TrackNamesPredicate);

                    functions.WriteToFile(dataDir + "sources\\toImport\\" + oneShowSource.SourceID + ".taper.txt", string.Join(Environment.NewLine, strTaper + Environment.NewLine + strType), false);
                    functions.WriteToFile(dataDir + "sources\\toImport\\" + oneShowSource.SourceID + ".md5s.txt", string.Join(Environment.NewLine, lstTrackDetails), false);
                    functions.WriteToFile(dataDir + "sources\\toImport\\" + oneShowSource.SourceID + ".tracks.txt", string.Join(Environment.NewLine, lstTrackNames), false);
                    functions.WriteToFile(dataDir + "sources\\toImport\\" + oneShowSource.SourceID + ".source.txt", string.Join(Environment.NewLine, strSourceText), false);
                    //strOutput += "Counts don't match for;" + oneShowSource.SourceID + PH_CF.strEOL;
                    //Console.WriteLine("Counts don't match for " + oneShowSource.SourceID);
                }
                else
                {
                    strOutput += "No checksums for;" + oneShowSource.SourceID + PH_CF.strEOL;
                    Console.WriteLine("No checksums for " + oneShowSource.SourceID);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void saveSourceDetailsToFile(int iSourceID)
        {
            try
            {
                var functions = new functions(BaseUrl, username, password, customHeader);

                //string strType = "3";
                //string strTaper = "Official DMBLive";

                List<OneSourceID> lstRelatedSources = functions.getRelatedSources(iSourceID);
                var sampleSource = (from row in lstRelatedSources.AsEnumerable<OneSourceID>()
                                    where row.SourceID < 362000000
                                    select row).FirstOrDefault<OneSourceID>();

                if (sampleSource != null)
                {
                    Console.WriteLine(sampleSource.SourceID);
                    List<OneSourceDetails> lstSourceDetails = functions.getSourceDetails(sampleSource.SourceID);
                    List<string> lstTrackNames = new List<string>();
                    List<string> lstTrackDetails = new List<string>();

                    lstTrackNames.Add(lstSourceDetails[0].TrackName);
                    lstTrackDetails.Add(lstSourceDetails[0].FileName.Replace(".shn", ".flac") + ":" + "dddddddddddddddddddddddddddddddd");

                    for (int i = 1; i < lstSourceDetails.Count; i++)
                    {
                        lstTrackNames.Add(lstSourceDetails[i].TrackName);
                        lstTrackDetails.Add(lstSourceDetails[i].FileName.Replace(".shn", ".flac") + ":" + "dddddddddddddddddddddddddddddd" + i.ToString().PadLeft(2, '0'));
                    }

                    //functions.WriteToFile(dataDir + "sources\\toImport\\" + iSourceID + ".taper.txt", string.Join(Environment.NewLine, strTaper + Environment.NewLine + strType), false);
                    functions.WriteToFile(dataDir + "sources\\toImport\\" + iSourceID + ".md5s.txt", string.Join(Environment.NewLine, lstTrackDetails), false);
                    functions.WriteToFile(dataDir + "sources\\toImport\\" + iSourceID + ".tracks.txt", string.Join(Environment.NewLine, lstTrackNames), false);
                    //strOutput += "Counts don't match for;" + oneShowSource.SourceID + PH_CF.strEOL;
                    //Console.WriteLine("Counts don't match for " + oneShowSource.SourceID);                
                }
                else
                {
                    List<string> lstTrackNames = new List<string>();
                    List<string> lstTrackDetails = new List<string>();

                    OneShowSource oss = functions.getShowSource(iSourceID);
                    OneShow thisShow = functions.getShow(oss.ShowID);

                    List<OneArtist> lstArtists = functions.getArtists();
                    var thisArtistNameEnum = from a in lstArtists
                                             where a.AID.Equals(thisShow.ArtistID)
                                             select a.ShortName;
                    string strArtistShortName = thisArtistNameEnum.FirstOrDefault<string>();
                    string strFileNamePrefix = strArtistShortName + thisShow.Year + "-" + thisShow.Month.ToString().PadLeft(2, '0') + "-" + thisShow.Day.ToString().PadLeft(2, '0') + "t";

                    lstTrackNames.Add("Unknown");
                    lstTrackDetails.Add(strFileNamePrefix + "01.flac" + ":" + "dddddddddddddddddddddddddddddddd");

                    for (int i = 1; i < 30; i++)
                    {
                        lstTrackNames.Add("Unknown");
                        lstTrackDetails.Add(strFileNamePrefix + (i + 1).ToString().PadLeft(2, '0') + ".flac" + ":" + "dddddddddddddddddddddddddddddd" + i.ToString().PadLeft(2, '0'));
                    }

                    //functions.WriteToFile(dataDir + "sources\\toImport\\" + iSourceID + ".taper.txt", string.Join(Environment.NewLine, strTaper + Environment.NewLine + strType), false);
                    functions.WriteToFile(dataDir + "sources\\toImport\\" + iSourceID + ".md5s.txt", string.Join(Environment.NewLine, lstTrackDetails), false);
                    functions.WriteToFile(dataDir + "sources\\toImport\\" + iSourceID + ".tracks.txt", string.Join(Environment.NewLine, lstTrackNames), false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void getSourceTaper(string Year, int ArtistID)
        {
            HtmlDocument doc = musicFunctions.functions.getHTMLDoc(etreeBaseURL + etreeShowsLink + ArtistID + "/year/" + Year);
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a"))
            {
                if (link.GetAttributeValue("href", "").Contains("shn/"))
                {
                    string strSourceID = link.GetAttributeValue("href", "").Replace("/shn/", "");
                    string strTitle = link.GetAttributeValue("title", "");
                    string strTaper = string.Empty;

                    strSourceID += (strTitle.StartsWith(" FLAC24")) ? ";Flac24" : ";Flac16";
                    try
                    {
                        strTitle = strTitle.Replace("\t", "").Replace("\r", "").Replace("\n", "");
                        if (strTitle.StartsWith(" Matrix"))
                        {
                            strTaper = "Matrix";
                            int iSource1 = strTitle.IndexOf("Source 1: ") + 10;
                            int iSource1Source = strTitle.IndexOf("Source:", iSource1);
                            strTaper = strTitle.Substring(iSource1, iSource1Source - iSource1).Trim();

                            int iSource2 = strTitle.IndexOf("Source 2: ") + 10;
                            strTaper += "/" + strTitle.Substring(iSource2).Trim();

                        }
                        else
                        {
                            List<string> lstTapers = getTaperDescriptionText();

                            List<int> lstTaperPositions = new List<int>();
                            foreach (var item in lstTapers)
                            {
                                lstTaperPositions.Add(strTitle.IndexOf(item) > -1 ? strTitle.IndexOf(item) + item.Length : 88888);
                            }
                            lstTaperPositions.Sort();
                            int iTaperStart = lstTaperPositions[0];


                            List<string> lstAnnotation = new List<string>();
                            lstAnnotation.Add("Source");
                            lstAnnotation.Add("Equipment");
                            lstAnnotation.Add("Rig Info");
                            lstAnnotation.Add("Location");

                            List<int> lstPositions = new List<int>();
                            foreach (var item in lstAnnotation)
                            {
                                lstPositions.Add(strTitle.IndexOf(item) > 0 ? strTitle.IndexOf(item) : 999999);
                            }
                            lstPositions.Sort();
                            int iTaperEnd = lstPositions[0];

                            if (iTaperStart < 88888)
                            {
                                strTaper = strTitle.Substring(iTaperStart, iTaperEnd - iTaperStart).Replace("&amp;", "+");
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        strOutput += strSourceID + ";Error" + Environment.NewLine;
                        Console.WriteLine("Error");
                    }
                    strOutput += strSourceID + ";" + strTaper.Trim() + Environment.NewLine;
                    Console.WriteLine(strSourceID + ";" + strTaper.Trim());
                }
            }
        }

        private static List<string> getTaperDescriptionText()
        {
            List<string> lstTapers = new List<string>();
            lstTapers.Add("TAPER:");
            lstTapers.Add("Taper:");
            lstTapers.Add("TAPERS:");
            lstTapers.Add("Tapers:");
            lstTapers.Add("Recorded by:");
            lstTapers.Add("Taped and Transfered by");
            lstTapers.Add("Taped and Transferred by");
            return (lstTapers);

        }

        private static string getSourceTaper(string SourceFile)
        {
            string strTaper = "Error";
            bool bTaperFound = false;
            try
            {
                List<string> lstTapers = getTaperDescriptionText();

                string[] strSeparators = new string[] { "\r\n", "\n", "\r" };
                string[] strarrSourceText = SourceFile.Split(strSeparators, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in strarrSourceText)
                {
                    foreach (var item in lstTapers)
                    {
                        if (s.StartsWith(item))
                        {
                            strTaper = s.Replace(item, "").Trim();
                            bTaperFound = true;
                            break;
                        }
                    }
                    if (bTaperFound)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
            }
            return (strTaper);
        }

        private static string getSourceTaper(OneShowSource oneShowSource)
        {
            string strTaper = "Error";
            bool bTaperFound = false;
            try
            {
                HtmlDocument doc = musicFunctions.functions.getHTMLDoc(etreeBaseURL + etreeSourceLink + oneShowSource.SourceID.ToString());
                var sourceTextNode = doc.DocumentNode
                                  .Descendants("div")
                                  .Where(div => div.GetAttributeValue("class", "").Contains("b1 t"))
                                  .ToArray();

                string strSourceText = sourceTextNode[0].InnerText;
                strSourceText = strSourceText.Replace("&nbsp;", " ");

                List<string> lstTapers = getTaperDescriptionText();

                string[] strSeparators = new string[] { "\r\n", "\n", "\r" };
                string[] strarrSourceText = strSourceText.Split(strSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in strarrSourceText)
                {
                    foreach (var item in lstTapers)
                    {
                        if (s.StartsWith(item))
                        {
                            strTaper = s.Replace(item, "").Trim();
                            bTaperFound = true;
                            break;
                        }
                    }
                    if (bTaperFound)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
            }
            return (strTaper);
        }

        private static bool TrackNamesPredicate(string str)
        {
            List<string> lstNonSongs = new List<string>();
            lstNonSongs.Add("totals");

            if (lstNonSongs.Contains(str))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void getShowsAndSourceIDs(int Year, int ArtistID)
        {
            var functions = new functions(BaseUrl, username, password, customHeader);

            PH_CF.pause(300);
            HtmlDocument doc = musicFunctions.functions.getHTMLDoc(etreeBaseURL + etreeShowsLink + ArtistID + "/year/" + Year);
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//tr"))
            {
                HtmlNodeCollection columns = link.ChildNodes;
                if (columns.Count.Equals(COLUMN_COUNT) && columns[VENUE_DATE].InnerHtml.Trim() != "Date")
                {
                    string strVenueMD5 = PH_CF.getMD5hash(columns[VENUE_NAME].InnerHtml.Trim() + columns[VENUE_CITY].InnerHtml.Trim() + columns[VENUE_STATE].InnerHtml.Trim());

                    #region Venue Entry
                    if (!lstVenueMD5s.Contains(strVenueMD5))
                    {
                        string strCity = columns[VENUE_CITY].InnerHtml.Trim();
                        string strVenueState = columns[VENUE_STATE].InnerHtml.Trim();

                        #region state/country
                        string strState = string.Empty;
                        string strCountry = string.Empty;
                        if (strVenueState.Length.Equals(2) || lstNonStates.Contains(strVenueState))
                        {
                            strState = strVenueState;
                            strCountry = "US";
                        }
                        else
                        {
                            strState = "";
                            strCountry = strVenueState;
                        }
                        #endregion

                        OneVenue thisVenue = new OneVenue();
                        thisVenue.CityID = functions.getCityID(strCity);
                        thisVenue.VenueMD5 = strVenueMD5;
                        thisVenue.VenueName = columns[VENUE_NAME].InnerHtml.Trim();
                        thisVenue.State = strState;
                        thisVenue.CountryID = functions.getCountryID(strCountry);
                        lstVenueMD5s.Add(strVenueMD5);
                        IRestResponse irrVenue = functions.addVenue(thisVenue);
                        Console.WriteLine("New venue found:" + thisVenue.VenueName);
                        strOutput += functions.getLineForLog(irrVenue) + PH_CF.strEOL;
                    }
                    #endregion

                    #region Show Entry
                    int iEtreeIDStart = columns[SHOW_ID].InnerHtml.ToString().IndexOf("addShow(") + 8;
                    int iEtreeIDEnd = columns[SHOW_ID].InnerHtml.ToString().IndexOf(")", iEtreeIDStart);
                    int iEtreeID = Int32.Parse(columns[SHOW_ID].InnerHtml.Substring(iEtreeIDStart, iEtreeIDEnd - iEtreeIDStart));

                    string strDate = columns[VENUE_DATE].InnerText.Trim().Replace("??", "00");

                    int iShowID = 0;
                    if (!lstShowIDs.Contains(iEtreeID))
                    {
                        OneShow thisShow = new OneShow();
                        if (ArtistID.Equals(4329))
                        {
                            thisShow.ArtistID = 6;
                        }
                        else
                        {
                            thisShow.ArtistID = ArtistID;
                        }
                        thisShow.Day = Int32.Parse(strDate.Substring(3, 2));
                        thisShow.EtreeID = iEtreeID;
                        thisShow.Month = Int32.Parse(strDate.Substring(0, 2));
                        thisShow.Year = Year;
                        thisShow.VenueMD5 = strVenueMD5;
                        iShowID = functions.getIDFromRestResponse(functions.addShow(thisShow));
                        Console.WriteLine("New show found:" + thisShow.EtreeID);
                        strOutput += iShowID + PH_CF.strEOL;
                    }
                    #endregion

                    #region Source Entry
                    if (!columns[SOURCE_IDS].InnerHtml.Trim().Equals(String.Empty))
                    {
                        string strSourceID = columns[SOURCE_IDS].InnerText.Trim();
                        string[] strarrSourceIDs = strSourceID.Split(',');
                        foreach (string sourceID in strarrSourceIDs)
                        {
                            if (!lstSourceIDs.Contains(sourceID.Trim()))
                            {
                                if (iShowID.Equals(0))
                                {
                                    iShowID = functions.getShowByEtreeID(iEtreeID).SID;
                                }

                                OneShowSource thisShowSource = new OneShowSource();
                                thisShowSource.ShowID = iShowID;
                                thisShowSource.SourceID = Int32.Parse(sourceID.Trim());
                                lstSourceIDs.Add(sourceID);
                                IRestResponse irrShowSource = functions.addShowSource(thisShowSource);
                                Console.WriteLine("New show source found:" + thisShowSource.SourceID);
                                strOutput += functions.getLineForLog(irrShowSource) + PH_CF.strEOL;

                                // Used for Sound Check Sources
                                /* 
                                OneShow thisShow = new OneShow();
                                thisShow.ArtistID = 6; //ArtistID;
                                thisShow.Day = Int32.Parse(strDate.Substring(3, 2));
                                thisShow.EtreeID = iEtreeID;
                                thisShow.Month = Int32.Parse(strDate.Substring(0, 2));
                                thisShow.Year = Int32.Parse(Year);
                                thisShow.VenueMD5 = strVenueMD5;
                                IRestResponse irrShow = functions.addShow(thisShow);
                                //strOutput += functions.getLineForLog(irrShow) + PH_CF.strEOL;                    
                                */
                            }
                        }
                    }
                    #endregion
                }
            }
            //PH_CF.WriteToFile(outFile, strOutput, true);
        }

        private static List<string> getTrackNames(string strSourceText, int ilstTrackDetailsCount)
        {

            string[] strSeparators = new string[] { "\r\n", "\n", "\r" };
            string[] strarrSourceText = strSourceText.Split(strSeparators, StringSplitOptions.RemoveEmptyEntries);

            List<string> lstTrackNames = new List<string>();
            List<string> lstDateFormats = new List<string>();
            lstDateFormats.Add("MM/dd/yyyy");
            lstDateFormats.Add("MM/dd/yy");
            lstDateFormats.Add("dd/MM/yy");
            lstDateFormats.Add("d/M/yy");
            lstDateFormats.Add("d/MM/yy");
            lstDateFormats.Add("M/d/yy");
            lstDateFormats.Add("M/dd/yy");
            lstDateFormats.Add("M/dd/yyyy");
            lstDateFormats.Add("MM/dd/yy");
            lstDateFormats.Add("dd/MM/yyyy");
            lstDateFormats.Add("d-M-yyyy");
            lstDateFormats.Add("M-dd-yy");
            lstDateFormats.Add("MM-dd-yy");
            lstDateFormats.Add("dd MMMM yyyy");
            lstDateFormats.Add("yyyy-MM-dd");

            string sPattern = string.Empty;
            sPattern += @"^(d)?(t)?(T)?(E:)?\d"; // Line starts with a d or a t or a E: or just a digit and then a digit, this will also return dates.                              
            // d is for disc, t for track, E: for Encore track
            foreach (string s in strarrSourceText)
            {
                if (s.StartsWith("744") || s.StartsWith("9FT"))
                {

                }
                else
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(s, sPattern))
                    {
                        bool isDate = false;
                        foreach (string format in lstDateFormats)
                        {
                            DateTime dtResult;
                            if (DateTime.TryParseExact(s, format, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dtResult))
                            {
                                isDate = true;
                            }
                        }
                        if (!isDate)
                        {
                            string strNameAndLength = s.Replace("&nbsp;", " ").Trim();
                            strNameAndLength = strNameAndLength.Replace("E:", "");                                  // Remove E: which signifies Encore
                            strNameAndLength = strNameAndLength.Replace("[", "");                                   // Remove [
                            strNameAndLength = strNameAndLength.Replace("]", "");                                   // Remove ]
                            strNameAndLength = Regex.Replace(strNameAndLength, @"\d\d:\d\d:\d\d$", "").Trim();      // Remove Length in format mm:ss:xx at the end of a line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"\d:\d\d:\d\d$", "").Trim();        // Remove Length in format m:ss:xx at the end of a line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"\d\d:\d\d.\d\d$", "").Trim();      // Remove Length in format mm:ss.xx at the end of a line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"\d:\d\d.\d\d$", "").Trim();        // Remove Length in format m:ss.xx at the end of a line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"\d\d:\d\d.\d\d\d$", "").Trim();    // Remove Length in format mm:ss.xxx at the end of a line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"\d:\d\d.\d\d\d$", "").Trim();      // Remove Length in format m:ss.xxx at the end of a line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"\d\d:\d\d$", "").Trim();           // Remove Length in format mm:ss at the end of a line                                
                            strNameAndLength = Regex.Replace(strNameAndLength, @"\d:\d\d$", "").Trim();             // Remove Length in format m:ss at the end of a line                                
                            strNameAndLength = Regex.Replace(strNameAndLength, @"\d\d.\d\d$", "").Trim();           // Remove Length in format mm.ss at the end of a line                                
                            strNameAndLength = Regex.Replace(strNameAndLength, @"\d.\d\d$", "").Trim();             // Remove Length in format m.ss at the end of a line                                
                            strNameAndLength = Regex.Replace(strNameAndLength, @"^([d])([\d]*)", "").Trim();        // Remove dXX at the start of the line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"^([t])([\d]*)", "").Trim();        // Remove tXX at the start of the line
                            //strNameAndLength = Regex.Replace(strNameAndLength, @"^([T])([\d]*)", "").Trim();        // Remove TXX at the start of the line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"^([\d]*)\s", "").Trim();           // Remove XX and a space at the start of the line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"([\d]*)[(\.)(\:)]", "").Trim();    // Remove XX and a period or colon at the start of the line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"-$", "").Trim();                   // Remove a - at the end of the line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"^-", "").Trim();                   // Remove a - at the start of the line
                            strNameAndLength = Regex.Replace(strNameAndLength, @"[^\w\'\.\ \#\?\->]", "").Trim();   // Remove everything except the quoted characters


                            //strNameAndLength = strNameAndLength.Replace("", "");

                            strNameAndLength = strNameAndLength.Replace("Big-Eyed Fish", "Big Eyed Fish");
                            strNameAndLength = strNameAndLength.Replace("Big Eyed Fish >", "Big Eyed Fish");
                            strNameAndLength = strNameAndLength.Replace("Burning Down the House", "Burning Down The House");
                            strNameAndLength = strNameAndLength.Replace("Cornbread", "Corn Bread");
                            strNameAndLength = strNameAndLength.Replace("Crash into Me", "Crash Into Me");
                            strNameAndLength = strNameAndLength.Replace("Crush Stefan bass intro", "Crush");
                            strNameAndLength = strNameAndLength.Replace("Digging a Ditch", "Digging A Ditch");
                            strNameAndLength = strNameAndLength.Replace("Don't Drink the Water", "Don't Drink The Water");
                            strNameAndLength = strNameAndLength.Replace("encore", "Encore Break");
                            strNameAndLength = strNameAndLength.Replace("Funny the Way It Is", "Funny The Way It Is");
                            strNameAndLength = strNameAndLength.Replace("Grave Digger", "Gravedigger");
                            strNameAndLength = strNameAndLength.Replace("Introduction", "Intro");
                            strNameAndLength = strNameAndLength.Replace("JTR", "John The Revelator");
                            strNameAndLength = strNameAndLength.Replace("Lie in Our Graves", "Lie In Our Graves");
                            strNameAndLength = strNameAndLength.Replace("Needle And The Damage Done", "The Needle And The Damage Done");
                            strNameAndLength = strNameAndLength.Replace("Pay for What You Get", "Pay For What You Get");
                            strNameAndLength = strNameAndLength.Replace("Pantala Naga Pampa >", "Pantala Naga Pampa");
                            strNameAndLength = strNameAndLength.Replace("Pantala Naga Pampa->", "Pantala Naga Pampa");
                            strNameAndLength = strNameAndLength.Replace("PNP", "Pantala Naga Pampa");
                            strNameAndLength = strNameAndLength.Replace("Rhyme  Reason", "Rhyme And Reason");
                            strNameAndLength = strNameAndLength.Replace("Set 1 Intro", "Intro");
                            strNameAndLength = strNameAndLength.Replace("Set 2 Intro", "Intro");
                            strNameAndLength = strNameAndLength.Replace("Shake Me Like a Monkey", "Shake Me Like A Monkey");
                            strNameAndLength = strNameAndLength.Replace("Slip Slidin' Away", "Slip Sliding Away");
                            strNameAndLength = strNameAndLength.Replace("Stay Wasting Time", "Stay (Wasting Time)");
                            strNameAndLength = strNameAndLength.Replace("Stay or Leave", "Stay Or Leave");
                            strNameAndLength = strNameAndLength.Replace("Stolen Away on 55th  3rd", "Stolen Away On 55th And 3rd");
                            strNameAndLength = strNameAndLength.Replace("Take Me to Tomorrow", "Take Me To Tomorrow");
                            strNameAndLength = strNameAndLength.Replace("The Idea of You", "The Idea Of You");
                            strNameAndLength = strNameAndLength.Replace("The Needle and the Damage Done", "The Needle And The Damage Done");
                            strNameAndLength = strNameAndLength.Replace("You and Me", "You And Me");
                            strNameAndLength = strNameAndLength.Replace("You  Me", "You And Me");
                            strNameAndLength = strNameAndLength.Replace("Water into Wine", "Water Into Wine");
                            strNameAndLength = strNameAndLength.Replace("What Would You Say", "What Would You Say?");
                            strNameAndLength = strNameAndLength.Replace("When the World Ends", "When The World Ends");
                            strNameAndLength = strNameAndLength.Replace("Where Are You Going", "Where Are You Going?");
                            strNameAndLength = strNameAndLength.Replace("Write a Song", "Write A Song");



                            string sSeparator = string.Empty;
                            if (s.Contains("\t"))
                            {
                                sSeparator += @"\t"; // A tab character is found
                            }
                            else
                            {
                                sSeparator += @"\s[\-:/]\s"; // A - : or / is found surrounded by a space    
                            }

                            string[] strParts = System.Text.RegularExpressions.Regex.Split(strNameAndLength, sSeparator);
                            if (!strParts[0].Trim().Equals(""))
                            {
                                if (!strParts[0].Contains("kHz"))
                                {
                                    lstTrackNames.Add(strParts[0].Trim());
                                }
                            }
                        }
                    }
                    else
                    {
                        //System.Console.WriteLine(" - invalid");
                    }
                }
            }

            // Track names aren't numbered numerically try something else
            if (!lstTrackNames.Count.Equals(ilstTrackDetailsCount))
            {
                Console.WriteLine("Error!");
                //Console.Read();
            }

            List<string> lstToRemove = new List<string>();
            lstToRemove.Add("otals");
            lstToRemove.Add("January");
            lstToRemove.Add("February");
            lstToRemove.Add("March");
            lstToRemove.Add("April");
            lstToRemove.Add("May");
            lstToRemove.Add("June");
            lstToRemove.Add("July");
            lstToRemove.Add("August");
            lstToRemove.Add("September");
            lstToRemove.Add("October");
            lstToRemove.Add("November");
            lstToRemove.Add("December");
            lstToRemove.Add("2448 version exists");
            lstToRemove.Add("10000 Lakes Festival");
            foreach (var item in lstToRemove)
            {
                while (lstTrackNames.Contains(item))
                {
                    lstTrackNames.Remove(item);
                }
            }
            return lstTrackNames;
        }

        private static void createMySqlTables(bool bDropExisting)
        {
            string strConnect = Properties.Settings.Default.musicConnectionString;
            using (MySqlConnection connection = new MySqlConnection(strConnect))
            {
                connection.Open();

                #region drop existing tables
                List<string> lstTables = new List<string>();
                lstTables.Add("mytracks");
                lstTables.Add("tracks");
                lstTables.Add("tracknames");
                lstTables.Add("trackfilenames");
                lstTables.Add("md5s");
                lstTables.Add("showsources");
                lstTables.Add("shows");
                lstTables.Add("venues");
                lstTables.Add("cities");
                lstTables.Add("countries");

                //lstTables.Add("artists");
                //lstTables.Add("members");                

                if (bDropExisting)
                {
                    foreach (var item in lstTables)
                    {
                        using (MySqlCommand cmd = new MySqlCommand("DROP TABLE IF EXISTS `" + item + "`", connection))
                        {
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                        }
                    }
                }
                #endregion

                #region artists table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `artists` (" +
                            "`AID` SMALLINT," +
                            "`Artist` VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "`ShortName` VARCHAR(5) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "PRIMARY KEY(AID));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region cities table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `cities` (" +
                            "`CIID` SMALLINT AUTO_INCREMENT," +
                            "`City` VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "PRIMARY KEY(CIID));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region countries table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `countries` (" +
                            "`COID` SMALLINT AUTO_INCREMENT," +
                            "`Country` VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "PRIMARY KEY(COID));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region members table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `members` (" +
                                "`MID` int(11) NOT NULL auto_increment, " +
                                "`usr` varchar(32) CHARACTER SET utf8 collate utf8_unicode_ci NOT NULL default ''," +
                                "`pass` varchar(32) CHARACTER SET utf8 collate utf8_unicode_ci NOT NULL default ''," +
                                "`email` varchar(255) CHARACTER SET utf8 collate utf8_unicode_ci NOT NULL default ''," +
                                "`regIP` varchar(15) CHARACTER SET utf8 collate utf8_unicode_ci NOT NULL default ''," +
                                "`secret` varchar(32) CHARACTER SET utf8 collate utf8_unicode_ci NOT NULL default ''," +
                                "`dt` datetime NOT NULL default '0000-00-00 00:00:00'," +
                                "UNIQUE KEY (usr), " +
                                "PRIMARY KEY(MID));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region md5s table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `md5s` (" +
                            "`MD5ID` INT AUTO_INCREMENT," +
                            "`MD5` VARCHAR(32) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "PRIMARY KEY(MD5ID));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region myTracks file names table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `trackfilenames` (" +
                            "`FNID` INT AUTO_INCREMENT," +
                            "`FileName` VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "PRIMARY KEY(FNID));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region myTracks table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `mytracks` (" +
                            "`MID` INT NOT NULL," +
                            "`MD5ID` INT NOT NULL," +
                            "`AddedTime` INT NOT NULL," +
                            "PRIMARY KEY(`MID`, `MD5ID`)," +
                            "FOREIGN KEY(`MD5ID`) REFERENCES md5s(`MD5ID`)," +
                            "FOREIGN KEY(`MID`) REFERENCES members(`MID`));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region venues table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `venues` (" +
                            "`VenueMD5` VARCHAR(32) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "`VenueName` VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "`CityID` SMALLINT," +
                            "`State` VARCHAR(2) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "`CountryID` SMALLINT," +
                            "PRIMARY KEY(VenueMD5)," +
                            "FOREIGN KEY(`CityID`) REFERENCES cities(`CIID`)," +
                            "FOREIGN KEY(`CountryID`) REFERENCES countries(`COID`));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region shows table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `shows` (" +
                            "`SID` INT AUTO_INCREMENT," +
                            "`EtreeID` INT," +
                            "`Day` SMALLINT," +
                            "`Month` SMALLINT," +
                            "`Year` SMALLINT," +
                            "`ArtistID` SMALLINT," +
                            "`VenueMD5` VARCHAR(32) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "PRIMARY KEY(SID) , UNIQUE (EtreeID)," +
                            "FOREIGN KEY(`ArtistID`) REFERENCES artists(`AID`)," +
                            "FOREIGN KEY(`VenueMD5`) REFERENCES venues(`VenueMD5`));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                /*
                CREATE TABLE `bannedSources` 
`BSID` INT AUTO_INCREMENT,
`SourceID` INT,
`Reason` VARCHAR(1024) CHARACTER SET utf8 COLLATE utf8_bin
PRIMARY KEY(BSID)
    */
                #region showSources table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `showsources` (" +
                            "`SSID` INT AUTO_INCREMENT," +
                            "`ShowID` INT," +
                            "`SourceID` INT," +
                            "FOREIGN KEY(`ShowID`) REFERENCES shows(`SID`)," +
                            "PRIMARY KEY(SSID));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region track names table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `tracknames` (" +
                            "`TNID` INT AUTO_INCREMENT," +
                            "`TrackName` VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "PRIMARY KEY(TNID));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region tracks table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `tracks` (" +
                            "`TID` INT AUTO_INCREMENT," +
                            "`ShowSourceID` INT," +
                            "`Number` SMALLINT," +
                            "`FileNameID` INT," +
                            "`NameID` INT," +
                            "`MD5ID` INT NOT NULL," +
                            "`Length` VARCHAR(8) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "PRIMARY KEY(TID)," +
                            "UNIQUE KEY `showSourceIDAndMD5` (`ShowSourceID`,`MD5ID`)," +
                            "FOREIGN KEY(`MD5ID`) REFERENCES md5s(`MD5ID`)," +
                            "FOREIGN KEY(`ShowSourceID`) REFERENCES showsources(`SSID`)," +
                            "FOREIGN KEY(`FileNameID`) REFERENCES trackfilenames(`FNID`)," +
                            "FOREIGN KEY(`NameID`) REFERENCES trackNames(`TNID`));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region dt table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `dt` (" +
                            "`DTID` INT," +
                            "`ShowSourceID` INT," +
                            "`Name` VARCHAR(256) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "`Free` TINYINT," +
                            "PRIMARY KEY(DTID)," +
                            "FOREIGN KEY(`ShowSourceID`) REFERENCES showsources(`SSID`));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region taper table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `taper` (" +
                            "`TPID` INT AUTO_INCREMENT," +
                            "`Name` VARCHAR(256) CHARACTER SET utf8 COLLATE utf8_bin," +
                            "PRIMARY KEY(TPID));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion

                #region sourceInfo table
                using (MySqlCommand cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS `sourceinfo` (" +
                            "`SIID` INT AUTO_INCREMENT," +
                            "`ShowSourceID` INT," +
                            "`TaperID` INT," +
                            "`Type` TINYINT," +
                            "PRIMARY KEY(SIID)," +
                            "FOREIGN KEY(`TaperID`) REFERENCES taper(`TPID`)," +
                            "FOREIGN KEY(`ShowSourceID`) REFERENCES showsources(`SSID`));", connection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                #endregion



                connection.Close();
            }
        }
    }
}
