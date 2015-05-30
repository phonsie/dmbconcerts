using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using PH_Common_4._0;
using musicFunctions;
using RestSharp;
using WDS.Bencode;

namespace dreamingtree
{
    class dt
    {
        #region Credentials
        private static string strUID = ConfigurationManager.AppSettings["UID"];
        private static string strPass = ConfigurationManager.AppSettings["Passkey"];

        #endregion
        private static string outFile = Directory.GetCurrentDirectory() + "\\Data\\output." + System.DateTime.Now.ToString("yyyy-MM-dd___HH-mm-ss") + ".txt";
        private static string outDirectory = Directory.GetCurrentDirectory() + "\\Data\\";
        private static string strFreeDownload = "freedownload";
        private static string strOutput = string.Empty;
        private static string strBaseURL = "http://www.dreamingtree.org/";

        private static string BaseUrl = ConfigurationManager.AppSettings["APIBaseUrl"];
        private static string username = ConfigurationManager.AppSettings["username"];
        private static string password = ConfigurationManager.AppSettings["password"];
        private static string customHeader = ConfigurationManager.AppSettings["customHeader"];

        static void Main(string[] args)
        {
            var functions = new functions(BaseUrl, username, password, customHeader);

            WebClient webClient = new WebClient();

            CookieContainer cookies = new CookieContainer();
            Cookie uidcookie = new Cookie("uid", strUID, "/", "www.dreamingtree.org");
            Cookie passcookie = new Cookie("pass", strPass, "/", "www.dreamingtree.org");
            cookies.Add(uidcookie);
            cookies.Add(passcookie);

            bool bCheckExistingPagesForNewSHNIDs = true;
            #region Check Existing Pages For New SHNIDs
            if (bCheckExistingPagesForNewSHNIDs)
            {
                List<OneDT> lstDTNotOnEtree = functions.getDTsNotOnEtree();
                foreach (OneDT thisOneDT in lstDTNotOnEtree)
                {
                    if (thisOneDT.DTID > 9906)
                    {
                        try
                        {
                            PH_CF.pause(750);
                            string strDetails = musicFunctions.functions.getWebPage(strBaseURL + "details.php?id=" + thisOneDT.DTID, cookies);
                            string strSourceID = "999999999";

                            //<a href="redir.php?url=http://db.etree.org/shn/126813" target="_blank">http://db.etree.org/shn/126813</a>
                            //string strEtreeURL = ">http://db.etree.org/shninfo_detail.php?shnid=";

                            string strEtreeURL = ">http://db.etree.org/shn/";
                            int iEtreePos = strDetails.IndexOf(strEtreeURL);
                            if (iEtreePos > -1)
                            {
                                int iStart = iEtreePos + strEtreeURL.Length;
                                int iFinish = strDetails.IndexOf("</a>", iStart);
                                strSourceID = strDetails.Substring(iStart, iFinish - iStart);
                            }
                            else
                            {
                                strEtreeURL = "http://db.etree.org/shninfo_detail.php?shnid=";
                                iEtreePos = strDetails.IndexOf(strEtreeURL);
                                if (iEtreePos > -1)
                                {
                                    int iStart = iEtreePos + strEtreeURL.Length;
                                    int iFinish = strDetails.IndexOf(">", iStart);
                                    strSourceID = strDetails.Substring(iStart, iFinish - iStart);
                                }
                            }

                            if (!strSourceID.Equals("999999999"))
                            {
                                Console.WriteLine(strSourceID);
                                thisOneDT.ShowSourceID = functions.getShowSource(Int32.Parse(strSourceID)).SSID;
                                IRestResponse irrDT = functions.updateDT(thisOneDT);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            #endregion

            bool bUpdateTorrentNames = false;
            #region Get New Torrents
            if (bUpdateTorrentNames)
            {
                List<OneDT> lstDTs = functions.getDTs();

                // http://www.dreamingtree.org/browse.php?incldead=1&sort=4&type=desc&page=0
                int i = 0;
                //for (int i = 1; i < 29; i++)
                {
                    string strHTML = musicFunctions.functions.getWebPage(strBaseURL + "browse.php?incldead=1&sort=4&type=desc&page=" + i, cookies);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(strHTML);

                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        if (link.OuterHtml.Contains("download.php"))
                        {
                            HtmlNode parent = link.ParentNode;
                            if (link.HasAttributes)
                            {
                                try
                                {

                                    string dllink = link.Attributes["href"].Value;
                                    //dllink = "download.php/8511/daveandfriends2004-06-11.tlm170.flac16%2015-52-39-289.torrent";
                                    string dtID = dllink.Substring(13, dllink.IndexOf("/", 14) - 13);
                                    int iThisDTID = Int32.Parse(dtID);

                                    //Console.WriteLine(dtID + " " + i);
                                    PH_CF.pause(500);

                                    // need this to ignore non-DMB torrents
                                    var thisOne = from a in lstDTs
                                                  where a.DTID.Equals(iThisDTID)
                                                  select a;

                                    OneDT dtExists = thisOne.FirstOrDefault();
                                    if (dtExists != null)
                                    {
                                        string strTempFileName = @"C:\temp\temp.torrent";
                                        string strDLURL = strBaseURL + HttpUtility.UrlDecode(dllink);
                                        webClient.DownloadFile(strDLURL, strTempFileName);

                                        FileStream fileT = File.OpenRead(strTempFileName);
                                        var thisTorrent = WDS.Bencode.Extensions.ToBencodeDictionaryElement(fileT).Elements;

                                        string strTorrentName = string.Empty;

                                        foreach (var item in thisTorrent)
                                        {
                                            switch (item.Key.Value)
                                            {
                                                case "info":
                                                    //Console.WriteLine("***");

                                                    foreach (var item2 in (BencodeDictionary)item.Value)
                                                    {
                                                        switch (item2.Key.Value)
                                                        {
                                                            case "name":
                                                                strTorrentName = ((BencodeString)item2.Value).Value;
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        fileT.Close();

                                        OneDT thisDT = functions.getDT(iThisDTID);
                                        if (!thisDT.Name.Equals(strTorrentName))
                                        {
                                            thisDT.Name = strTorrentName;
                                            IRestResponse irr = functions.updateDT(thisDT);
                                            Console.WriteLine(thisDT.DTID + " updated" + i);
                                        }
                                        else
                                        {
                                            Console.WriteLine(thisDT.DTID + " matched " + i);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("non-DMB" + iThisDTID);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("non-DMB" + ex.Message);                                 
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            // This one works well!  As of June 2014
            bool bGetNewTorrents = true;
            #region Get New Torrents
            if (bGetNewTorrents)
            {
                DataTable dt = createDT2EtreeIDs();
                List<OneDT> lstDTs = functions.getDTs();
                LastDTID lastDTID = functions.getLastDT();

                // http://www.dreamingtree.org/browse.php?incldead=1&sort=4&type=desc&page=0
                int i = 0;
                //for (int i = 1; i < 29; i++)
                {
                    string strHTML = musicFunctions.functions.getWebPage(strBaseURL + "browse.php?incldead=1&sort=4&type=desc&page=" + i, cookies);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(strHTML);

                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        if (link.OuterHtml.Contains("download.php"))
                        {
                            HtmlNode parent = link.ParentNode;
                            if (link.HasAttributes)
                            {
                                string dllink = link.Attributes["href"].Value;
                                string dtID = dllink.Substring(13, dllink.IndexOf("/", 14) - 13);
                                int iThisDTID = Int32.Parse(dtID);

                                //if (iThisDTID > lastDTID.LastDT)
                                //if (iThisDTID.Equals(10155))
                                {
                                    {
                                        var oneDT = from r in lstDTs
                                                    where r.DTID.Equals(iThisDTID)
                                                    select r;

                                        OneDT oneDTFound = oneDT.FirstOrDefault();
                                        if (oneDTFound != null)
                                        {
                                            Console.WriteLine(iThisDTID + " exists");
                                        }
                                        else
                                        {
                                            Console.WriteLine(dtID + " " + i);
                                            PH_CF.pause(500);
                                            string strDetails = musicFunctions.functions.getWebPage(strBaseURL + "details.php?id=" + dtID, cookies);

                                            string strSourceID = "999999999";
                                            //<a href="redir.php?url=http://db.etree.org/shn/126813" target="_blank">http://db.etree.org/shn/126813</a>
                                            string strEtreeURL = ">http://db.etree.org/shn/";
                                            int iEtreePos = strDetails.IndexOf(strEtreeURL);
                                            if (iEtreePos > -1)
                                            {
                                                int iStart = iEtreePos + strEtreeURL.Length;
                                                int iFinish = strDetails.IndexOf("</a>", iStart);
                                                strSourceID = strDetails.Substring(iStart, iFinish - iStart);
                                            }

                                            OneShowSource oss = functions.getShowSource(Int32.Parse(strSourceID));

                                            OneDT thisDT = new OneDT();
                                            thisDT.DTID = iThisDTID;
                                            thisDT.Free = parent.InnerHtml.Contains(strFreeDownload) ? 1 : 0;
                                            thisDT.Name = Path.GetFileName(dllink);
                                            thisDT.ShowSourceID = oss.SSID;
                                            IRestResponse irr = functions.addDT(thisDT);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            bool bCheckAllDTIDsAreRecorded = false;
            #region  Check All DTIDs Are Recorded
            if (bCheckAllDTIDsAreRecorded)
            {
                List<OneDT> lstCurrentDTs = functions.getDTs();

                for (int i = 0; i < 28; i++)
                {
                    string strHTML = musicFunctions.functions.getWebPage(strBaseURL + "browse.php?incldead=1&sort=4&type=desc&page=" + i, cookies);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(strHTML);

                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        if (link.OuterHtml.Contains("download.php"))
                        {
                            HtmlNode parent = link.ParentNode;
                            if (link.HasAttributes)
                            {
                                string dllink = link.Attributes["href"].Value;
                                string dtID = dllink.Substring(13, dllink.IndexOf("/", 14) - 13);
                                int iThisDTID = Int32.Parse(dtID);

                                // need this to ignore non-DMB torrents
                                var thisOne = from a in lstCurrentDTs
                                              where a.DTID.Equals(iThisDTID)
                                              select a;

                                OneDT dtExists = thisOne.FirstOrDefault();
                                if (dtExists != null)
                                {
                                    //Console.WriteLine(iThisDTID + " exists");
                                }
                                else
                                {
                                    strOutput += strBaseURL + "details.php?id=" + iThisDTID + ";" + link.ParentNode.InnerText.Trim() + Environment.NewLine;
                                    Console.WriteLine(iThisDTID + " is missing " + link.ParentNode.InnerText.Trim());
                                }
                            }
                        }
                    }
                    PH_CF.pause(1000);
                }
                PH_CF.WriteToFile(outFile, strOutput, false);
            }
            #endregion

            bool bParseLogAndDeleteDeadDTLinks = true;
            #region Parse Log And Delete Dead DT Links
            if (bParseLogAndDeleteDeadDTLinks)
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                string format = "yyyy-MM-dd-HH:mm:ss";
                string strEvent = string.Empty;

                DataTable dt = createDTParseLog();

                for (int i = 0; i < 20; i++)
                {
                    HtmlDocument doc = musicFunctions.functions.getHTMLDoc(strBaseURL + "log.php?page=" + i, cookies);
                    foreach (HtmlNode hnLogEntry in doc.DocumentNode.SelectNodes("/html[1]/body[1]/table[3]/tr[1]/td[1]/tr[1]/td[1]/table[1]/tr"))
                    {
                        if (!hnLogEntry.ChildNodes[0].InnerText.Equals("Date"))
                        {
                            strEvent = hnLogEntry.ChildNodes[2].InnerText.Trim();
                            if (strEvent.StartsWith("Torrent"))
                            {
                                strEvent = strEvent.Replace("Torrent ", "");
                                DataRow dr = dt.NewRow();
                                dr["dtID"] = Int32.Parse(strEvent.Substring(0, strEvent.Trim().IndexOf(' ')).Trim());
                                dr["DateAndTime"] = DateTime.ParseExact(hnLogEntry.ChildNodes[0].InnerText + "-" + hnLogEntry.ChildNodes[1].InnerText, format, provider);
                                dr["EventText"] = strEvent;
                                if (strEvent.Contains("was deleted by"))
                                {
                                    dr["Event"] = "Deleted";
                                }
                                else if (strEvent.Contains("was uploaded by"))
                                {
                                    dr["Event"] = "Uploaded";
                                }
                                else if (strEvent.Contains("was edited by"))
                                {
                                    dr["Event"] = "Edited";
                                }
                                else if (strEvent.Contains("No Seeders For 30 days!"))
                                {
                                    dr["Event"] = "Deleted";
                                }
                                else
                                {
                                    dr["Event"] = "Unknown";
                                }
                                dt.Rows.Add(dr);
                            }
                        }
                    }
                }

                List<OneDT> lstDTs = functions.getDTs();

                DataRow[] drArrDeleted = dt.Select("Event='Deleted'");
                foreach (DataRow thisDR in drArrDeleted)
                {
                    var GetDeletedDT = from dts in lstDTs
                                       where dts.DTID.Equals(thisDR["dtID"])
                                       select dts;

                    OneDT thisDeletedDT = GetDeletedDT.FirstOrDefault();
                    if (thisDeletedDT != null)
                    {
                        Console.WriteLine(thisDR["EventText"]);
                        IRestResponse irr = functions.deleteDT(thisDeletedDT.DTID);
                        Console.WriteLine(thisDeletedDT.DTID + " has been deleted!");
                    }
                    else
                    {
                        Console.WriteLine(thisDR["dtID"] + " was not there!");
                    }
                }
                PH_CF.writeDataToFile(outFile, dt, "DateAndTime");
            }
            #endregion

            bool bCheckShowsNeedSeeding = false;
            #region  Check Shows Need Seeding
            if (bCheckShowsNeedSeeding)
            {
                List<OneMyDT> lstMyDTSources = functions.getMyDTSources(1);

                string strHTML = musicFunctions.functions.getWebPage(strBaseURL + "index.php", cookies);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(strHTML);

                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//b/a[@href]"))
                {
                    string strLink = "\"details.php?id=";
                    if (link.OuterHtml.Contains(strLink))
                    {
                        if (link.HasAttributes)
                        {
                            string dllink = link.Attributes["href"].Value;
                            int dtID = Int32.Parse(dllink.Substring(strLink.Length - 1, dllink.IndexOf("&", strLink.Length) - strLink.Length + 1));

                            var oneIHave = from row in lstMyDTSources
                                           where row.DTID.Equals(dtID)
                                           select row;

                            foreach (var item in oneIHave)
                            {
                                Console.WriteLine(link.OuterHtml);
                                strOutput += strBaseURL + strLink.Replace("\"", "") + item.DTID + Environment.NewLine;
                            }
                        }
                    }
                }
                if (strOutput.Length > 0)
                {
                    PH_CF.WriteToFile(outFile, strOutput, false);
                }
            }
            #endregion

            bool bGetOfficialTracks = false;
            #region Get Official Tracks
            if (bGetOfficialTracks)
            {
                HtmlDocument doc = functions.getHTMLDoc("http://davematthewsband.shop.musictoday.com/Product.aspx?cp=1_5575_8858&pc=DMDD114");

                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    if (link.OuterHtml.Contains("Product.aspx"))
                    {
                        PH_CF.pause(1000);
                        Console.WriteLine(link.InnerHtml);
                    }
                }
            }
            #endregion
        }

        private static DataTable createDTParseLog()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("dtID", typeof(int));
            dt.Columns.Add("DateAndTime", typeof(DateTime));
            dt.Columns.Add("EventText", typeof(string));
            dt.Columns.Add("Event", typeof(string));
            return (dt);
        }

        private static DataTable createDT2EtreeIDsSimple()
        {
            // sourceID, dtID, name, freeDownload
            DataTable dt = new DataTable();
            dt.Columns.Add("dtID", typeof(string));
            dt.Columns.Add("sourceID", typeof(string));
            dt.Columns.Add("name", typeof(string));
            return (dt);
        }

        private static DataTable createDT2EtreeIDs()
        {
            // sourceID, dtID, name, freeDownload
            DataTable dt = new DataTable();
            dt.Columns.Add("dtID", typeof(string));
            dt.Columns.Add("sourceID", typeof(string));
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("freeDownload", typeof(bool));
            return (dt);
        }
    }
}