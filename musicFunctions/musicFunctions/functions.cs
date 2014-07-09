using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;


namespace musicFunctions
{
    public class functions
    {
        private static List<OneTrackName> lstTrackName = new List<OneTrackName>();

        private static string APIBaseURL = ConfigurationManager.AppSettings["APIBaseUrl"];
        private static string username = string.Empty;
        private static string password = string.Empty;
        private static string customHeader = string.Empty;

        public static string strDelimiter = ";";

        public functions(string strBaseURL)
        {
            APIBaseURL = strBaseURL;
            customHeader = "customHeader";
        }

        public functions(string strBaseURL, string strUsername, string strPassword, string strCustomHeader)
        {
            APIBaseURL = strBaseURL;
            username = strUsername;
            password = strPassword;
            customHeader = strCustomHeader;
        }

        public static HtmlDocument getHTMLDoc(string URL)
        {
            HtmlDocument doc = getHTMLDoc(URL, null);
            return (doc);
        }

        public static HtmlDocument getHTMLDoc(string URL, CookieContainer cookies)
        {
            string strHTML = getWebPage(URL, cookies);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(strHTML);
            return (doc);
        }

        public static string generateMD5hash(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }

        public static string getMD5FromString(string strInput)
        {
            string strMD5 = string.Empty;
            string md5KeyRegEx;
            Regex objRegex;
            Match objMatch;

            md5KeyRegEx = "([0-9A-Fa-f]{32})";
            objRegex = new Regex(md5KeyRegEx);

            objMatch = objRegex.Match(strInput);
            if (objMatch.Success)
            {
                strMD5 = objMatch.Value;
            }
            return strMD5;
        }

        public static string getMD5OfFile(FileInfo thisFileInfo)
        {
            StringBuilder sb = new StringBuilder();
            FileStream fs = new FileStream(thisFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);

            byte[] hashvalueb = new MD5CryptoServiceProvider().ComputeHash(fs);
            fs.Close();

            foreach (byte hex in hashvalueb)
                sb.Append(hex.ToString("x2"));
            string md5sum = sb.ToString();
            return md5sum;
        }

        public static string getWebPage(string URL)
        {
            return (getWebPage(URL, null, null));
        }

        public static string getWebPage(string URL, CookieContainer cookies)
        {
            return (getWebPage(URL, cookies, null));
        }

        public static string getWebPage(string URL, WebHeaderCollection headers)
        {
            return (getWebPage(URL, null, headers));
        }

        public static string getWebPage(string URL, CookieContainer cookies, WebHeaderCollection headers)
        {
            string strWebContent = string.Empty;
            try
            {
                Uri uri = new Uri(URL);
                HttpWebRequest req = WebRequest.Create(uri) as HttpWebRequest;
                if (cookies != null)
                {
                    req.CookieContainer = cookies;
                }
                if (headers != null)
                {
                    req.Headers = headers;
                }
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                strWebContent = sr.ReadToEnd();
            }
            catch (Exception ex)
            {

            }
            return (strWebContent);
        }

        public static string md5OrFFP(string fileContents, string fileOrFolder)
        {
            string md5KeyRegEx;
            Regex objRegex;
            Match objMatch;
            int positionFound;

            //Console.WriteLine(APIBaseURL);

            md5KeyRegEx = "([0-9A-Fa-f]{32})";
            objRegex = new Regex(md5KeyRegEx);
            objMatch = objRegex.Match(fileContents);
            positionFound = objMatch.Index;

            if (objMatch.Success)
            {
                if (fileOrFolder.Equals("File"))
                {
                    if (positionFound > 1)
                    {
                        return ".ffp";
                    }
                    else
                    {
                        return ".md5";
                    }
                }
                if (fileOrFolder.Equals("Folder"))
                {
                    if (positionFound > 1)
                    {
                        return ".flac";
                    }
                    else
                    {
                        return ".shnf";
                    }
                }
            }
            else
            {
                return "not_listed";
            }
            return "not_listed";
        }

        public static int md5SortColumnNumber(string fileContents, string fileOrFolder)
        {
            string md5KeyRegEx;
            Regex objRegex;
            Match objMatch;
            int positionFound;

            md5KeyRegEx = "([0-9A-Fa-f]{32})";
            objRegex = new Regex(md5KeyRegEx);
            objMatch = objRegex.Match(fileContents);
            positionFound = objMatch.Index;

            if (objMatch.Success)
            {
                if (positionFound > 1)
                {
                    return 0;
                }
                else
                {
                    return 34;
                }
            }
            else
            {
                return -1;
            }
        }

        public static List<string> returnFileContents(string file_path)
        {
            StreamReader sr = new StreamReader(file_path, Encoding.GetEncoding("iso-8859-15"));
            List<string> arl = new List<string>();
            string line = null;
            while ((line = sr.ReadLine()) != null)
            {
                arl.Add(line);
            }
            sr.Close();
            return arl;
        }

        public static ArrayList returnFileContents(string file_path, int numberOfFields)
        {
            return returnFileContents(file_path, numberOfFields, ";");
        }
        public static ArrayList returnFileContents(string file_path, int numberOfFields, string delimiter)
        {
            return returnFileContents(file_path, numberOfFields, delimiter, false);
        }
        public static ArrayList returnFileContents(string file_path, int numberOfFields, bool ignoreHashedLines)
        {
            return returnFileContents(file_path, numberOfFields, ";", ignoreHashedLines);
        }
        public static ArrayList returnFileContents(string file_path, int numberOfFields, string delimiter, bool ignoreHashedLines)
        {
            Regex splitRx = new Regex(@"" + delimiter + @"\s*", RegexOptions.Compiled);
            StreamReader sr = new StreamReader(file_path, Encoding.GetEncoding("iso-8859-15"));
            ArrayList arl = new ArrayList();
            string line = null;
            int ln = 0;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("#") && ignoreHashedLines)
                {

                }
                else
                {
                    string[] fields = splitRx.Split(line);
                    ln++;
                    if (fields.Length != numberOfFields)
                    {
                        string[] errors = new string[] { "Error", "Invalid Input on line:" + ln + " (" + line + ")", numberOfFields + " fields are required. " + fields.Length + " found" };
                        arl.Add(errors);
                        continue;
                    }
                    arl.Add(fields);
                }
            }
            sr.Close();
            return arl;
        }

        public static void writeDataToFile(string strFileName, DataTable dtOutput, string strOrderBy, bool bIncludeHeader)
        {
            StringBuilder sb;
            EnumerableRowCollection<DataRow> drCollection;
            IEnumerable<string> columnNames = dtOutput.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);

            sb = new StringBuilder();
            if (bIncludeHeader) { sb.AppendLine(string.Join(";", columnNames)); }
            drCollection = from myRow in dtOutput.AsEnumerable()
                           orderby myRow.Field<object>(strOrderBy)
                           select myRow;

            foreach (DataRow row in drCollection)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(";", fields));
            }
            WriteToFile(strFileName, sb.ToString(), true);
        }

        public static void WriteToFile(string filePath, string line, bool append)
        {
            try
            {
                StreamWriter SW = new StreamWriter(filePath, append, Encoding.GetEncoding("iso-8859-15"));
                SW.Write(line);
                SW.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #region API Functions
        public int getCityID(string strCity)
        {
            int iCityID = 0;
            List<OneCity> lstCities = getCities();

            if (lstCities != null)
            {
                IEnumerable<OneCity> existingCity = from c in lstCities
                                                    where c.City.Equals(strCity)
                                                    select c;

                if (existingCity.FirstOrDefault<OneCity>() != null)
                {
                    OneCity thisCity = existingCity.First<OneCity>();
                    iCityID = thisCity.CIID;
                }
                else
                {
                    OneCity oneCity = new OneCity();
                    oneCity.City = strCity;
                    IRestResponse irrAddCity = addCity(oneCity);
                    ResultMessage rm = new JavaScriptSerializer().Deserialize<ResultMessage>(irrAddCity.Content);
                    iCityID = rm.result.ID;
                }
            }
            else
            {
                OneCity oneCity = new OneCity();
                oneCity.City = strCity;
                IRestResponse irrAddCity = addCity(oneCity);
                ResultMessage rm = new JavaScriptSerializer().Deserialize<ResultMessage>(irrAddCity.Content);
                iCityID = rm.result.ID;
            }
            return iCityID;
        }

        public int getCountryID(string strCountry)
        {
            int iCountryID = 0;
            List<OneCountry> lstCountries = getCountries();
            if (lstCountries != null)
            {
                IEnumerable<OneCountry> existingCountry = from c in lstCountries
                                                          where c.Country.Equals(strCountry)
                                                          select c;

                if (existingCountry.FirstOrDefault<OneCountry>() != null)
                {
                    OneCountry thisCountry = existingCountry.First<OneCountry>();
                    iCountryID = thisCountry.COID;
                }
                else
                {
                    OneCountry oneCountry = new OneCountry();
                    oneCountry.Country = strCountry;
                    IRestResponse irrAddCountry = addCountry(oneCountry);
                    ResultMessage rm = new JavaScriptSerializer().Deserialize<ResultMessage>(irrAddCountry.Content);
                    iCountryID = rm.result.ID;
                }
            }
            else
            {
                OneCountry oneCountry = new OneCountry();
                oneCountry.Country = strCountry;
                IRestResponse irrAddCountry = addCountry(oneCountry);
                ResultMessage rm = new JavaScriptSerializer().Deserialize<ResultMessage>(irrAddCountry.Content);
                iCountryID = rm.result.ID;
            }
            return iCountryID;
        }

        public int getMD5ID(string strMD5)
        {
            int iMD5ID = 0;
            OneMD5 md5ToCheck = getMD5(strMD5);
            if (md5ToCheck.MD5ID.Equals(0))
            {
                OneMD5 oneMD5 = new OneMD5();
                oneMD5.MD5 = strMD5;
                IRestResponse irrAddMD5 = addMD5(oneMD5);
                ResultMessage rm = new JavaScriptSerializer().Deserialize<ResultMessage>(irrAddMD5.Content);
                iMD5ID = rm.result.ID;
            }
            else
            {
                iMD5ID = md5ToCheck.MD5ID;
            }
            return iMD5ID;
        }
        
        public OneMemberID getMemberIDFromSecret(string strMySecret)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "memberIDfromSecret/" + strMySecret;
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<OneMemberID>(request);
            var thisData = response.Data;
            return ((OneMemberID)thisData);
        }

        public List<OneShow> getShowsForDateAndArtist(int Year, int Month, int Day, int ArtistID)
        {
            List<OneShow> lst = new List<OneShow>();
            List<OneShow> lstShows = getShows();
            var showsOnThisDate = from row in lstShows
                                  where row.ArtistID.Equals(ArtistID) && row.Day.Equals(Day) && row.Month.Equals(Month) && row.Year.Equals(Year)
                                  select row;

            foreach (var item in showsOnThisDate)
            {
                lst.Add(item);
            }
            return lst;
        }

        public List<OneSourceDetails> getSourceDetails(int iSourceID)
        {
            List<OneSourceDetails> lst = new List<OneSourceDetails>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "getSourceDetails/" + iSourceID;
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneSourceDetails>>(request);
            lst = response.Data;
            return (lst);
        }

        public int getTaperID(string strTaperName)
        {
            int iTaperID = 0;
            OneTaper taperNameToCheck = getTaperByName(strTaperName);
            if (taperNameToCheck.TPID > 0)
            {
                iTaperID = taperNameToCheck.TPID;
            }
            else
            {
                OneTaper oneTaper = new OneTaper();
                oneTaper.Name = strTaperName;
                IRestResponse irrAddTaper = addTaper(oneTaper);
                ResultMessage rm = new JavaScriptSerializer().Deserialize<ResultMessage>(irrAddTaper.Content);
                iTaperID = rm.result.ID;
            }
            return iTaperID;
        }

        public int getTrackFileNameID(string strTrackFileName)
        {
            int iTrackFileName = 0;
            OneTrackFileName trackFileNameToCheck = getTrackFileName(strTrackFileName);
            if (trackFileNameToCheck != null && trackFileNameToCheck.FNID != 0)
            {
                iTrackFileName = trackFileNameToCheck.FNID;
            }
            else
            {
                OneTrackFileName oneTrackFileName = new OneTrackFileName();
                oneTrackFileName.FileName = strTrackFileName;
                IRestResponse irrAddTrackFileName = addTrackFileName(oneTrackFileName);
                ResultMessage rm = new JavaScriptSerializer().Deserialize<ResultMessage>(irrAddTrackFileName.Content);
                iTrackFileName = rm.result.ID;
            }
            return iTrackFileName;
        }

        public int getTrackNameID(string strTrackName, bool createNew)
        {
            int iTrackNameID = 0;
            if (lstTrackName.Count.Equals(0))
            {
                lstTrackName = getTrackNames();
            }
            if (lstTrackName != null)
            {
                IEnumerable<OneTrackName> existingTrackName = from c in lstTrackName
                                                              where c.TrackName.ToLower().Equals(strTrackName.ToLower())
                                                              select c;

                if (existingTrackName.FirstOrDefault<OneTrackName>() != null)
                {
                    OneTrackName thisTrackName = existingTrackName.First<OneTrackName>();
                    iTrackNameID = thisTrackName.TNID;
                }
                else
                {
                    if (createNew)
                    {
                        OneTrackName oneTrackName = new OneTrackName();
                        oneTrackName.TrackName = strTrackName;
                        IRestResponse irrAddTrackName = addTrackName(oneTrackName);
                        ResultMessage rm = new JavaScriptSerializer().Deserialize<ResultMessage>(irrAddTrackName.Content);
                        iTrackNameID = rm.result.ID;
                        oneTrackName.TNID = iTrackNameID;
                        lstTrackName.Add(oneTrackName);
                    }
                }
            }
            else
            {
                if (createNew)
                {
                    OneTrackName oneTrackName = new OneTrackName();
                    oneTrackName.TrackName = strTrackName;
                    IRestResponse irrAddTrackName = addTrackName(oneTrackName);
                    ResultMessage rm = new JavaScriptSerializer().Deserialize<ResultMessage>(irrAddTrackName.Content);
                    iTrackNameID = rm.result.ID;
                    oneTrackName.TNID = iTrackNameID;
                    lstTrackName.Add(oneTrackName);
                }
            }
            return iTrackNameID;
        }

        public List<OneSourceID> getSourceIDsFromMD5(string MD5)
        {
            List<OneSourceID> lst = new List<OneSourceID>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "sourceIDsFromMD5/" + MD5;
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneSourceID>>(request);
            lst = response.Data;
            return (lst);
        }

        #region artists
        public IRestResponse addArtist(OneArtist oneArtist)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "artists/";
            request.AddParameter("AID", oneArtist.AID);
            request.AddParameter("Artist", oneArtist.Artist);
            request.AddParameter("ShortName", oneArtist.ShortName);
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<OneArtist>(request);
            return (response);
        }

        public IRestResponse deleteArtist(int iArtistID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "artists/" + iArtistID.ToString();
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneArtist>(request);
            return (response);
        }

        public OneArtist getArtist(int iArtistID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "artists/" + iArtistID.ToString();
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneArtist>>(request);
            var thisData = response.Data;
            return ((OneArtist)thisData[0]);
        }

        public List<OneArtist> getArtists()
        {
            List<OneArtist> lst = new List<OneArtist>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "artists/";
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneArtist>>(request);
            lst = response.Data;
            return (lst);
        }

        public IRestResponse updateArtist(OneArtist oneArtist)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "artists/" + oneArtist.AID;
            request.AddParameter("Artist", oneArtist.Artist);
            request.AddParameter("ShortName", oneArtist.ShortName);
            request.AddHeader(customHeader, "OK");
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region cities
        public IRestResponse addCity(OneCity oneCity)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "cities/";
            request.AddParameter("City", oneCity.City);
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<OneCity>(request);
            return (response);
        }

        public IRestResponse deleteCity(int iCityID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "cities/" + iCityID.ToString();
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneCity>(request);
            return (response);
        }

        public OneCity getCity(int iCityID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "cities/" + iCityID.ToString();
            request.AddHeader(customHeader, "OK");            
            var response = client.Execute<List<OneCity>>(request);
            var thisData = response.Data;
            return ((OneCity)thisData[0]);
        }

        public List<OneCity> getCities()
        {
            List<OneCity> lst = new List<OneCity>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "cities/";
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneCity>>(request);
            lst = response.Data;
            return (lst);
        }

        public IRestResponse updateCity(OneCity oneCity)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "cities/" + oneCity.CIID;
            request.AddParameter("City", oneCity.City);
            request.AddHeader(customHeader, "OK");
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region countries
        public IRestResponse addCountry(OneCountry oneCountry)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "countries/";
            request.AddParameter("Country", oneCountry.Country);
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<OneCountry>(request);
            return (response);
        }

        public IRestResponse deleteCountry(int iCountryID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "countries/" + iCountryID.ToString();
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneCountry>(request);
            return (response);
        }

        public OneCountry getCountry(int iCountryID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "countries/" + iCountryID.ToString();
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneCountry>>(request);
            var thisData = response.Data;
            return ((OneCountry)thisData[0]);
        }

        public List<OneCountry> getCountries()
        {
            List<OneCountry> lst = new List<OneCountry>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "countries/";
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneCountry>>(request);
            lst = response.Data;
            return (lst);
        }

        public IRestResponse updateCountry(OneCountry oneCountry)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "countries/" + oneCountry.COID;
            request.AddParameter("Country", oneCountry.Country);
            request.AddHeader(customHeader, "OK");
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region DT
        public IRestResponse addDT(OneDT oneDT)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "dt/";
            request.AddParameter("DTID", oneDT.DTID);
            request.AddParameter("Free", oneDT.Free);
            request.AddParameter("Name", oneDT.Name);
            request.AddParameter("ShowSourceID", oneDT.ShowSourceID);
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<OneDT>(request);
            return (response);
        }

        public IRestResponse deleteDT(int iDTID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "dt/" + iDTID.ToString();
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneDT>(request);
            return (response);
        }

        public OneDT getDT(int iDTID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "dt/" + iDTID.ToString();
            request.AddHeader(customHeader, "OK");            
            var response = client.Execute<List<OneDT>>(request);
            var thisData = response.Data;
            return ((OneDT)thisData[0]);
        }       

        public List<OneDT> getDTs()
        {
            List<OneDT> lst = new List<OneDT>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "dt/";
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneDT>>(request);
            lst = response.Data;
            return (lst);
        }

        public List<OneDT> getDTsNotOnEtree()
        {
            List<OneDT> lst = new List<OneDT>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);
            //client.Authenticator = new SimpleAuthenticator("username", username, "password", password);

            var request = new RestRequest();
            request.Resource = "dtNotOnEtree/";
            request.AddHeader(customHeader, "OK");
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneDT>>(request);
            lst = response.Data;
            return (lst);
        }

        public List<OneMyDT> getMyDTSources(int iMemberID)
        {
            List<OneMyDT> lst = new List<OneMyDT>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "MyDTSources/" + iMemberID;
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneMyDT>>(request);
            lst = response.Data;
            return (lst);
        }

        public LastDTID getLastDT()
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "lastDT/";
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<LastDTID>>(request);
            var thisData = response.Data;
            return ((LastDTID)thisData[0]);
        }

        public IRestResponse updateDT(OneDT oneDT)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "dt/" + oneDT.DTID;
            request.AddParameter("DTID", oneDT.DTID);
            request.AddParameter("Free", oneDT.Free);
            request.AddParameter("Name", oneDT.Name);
            request.AddParameter("ShowSourceID", oneDT.ShowSourceID);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region md5s
        public IRestResponse addMD5(OneMD5 oneMD5)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "md5s/";
            request.AddParameter("MD5", oneMD5.MD5);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<OneMD5>(request);
            return (response);
        }

        public IRestResponse deleteMD5(int iMD5ID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "md5s/" + iMD5ID.ToString();
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneMD5>(request);
            return (response);
        }

        public OneMD5 getMD5(int iMD5ID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "md5s/" + iMD5ID.ToString();
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneMD5>>(request);
            var thisData = response.Data;
            return ((OneMD5)thisData[0]);
        }

        public OneMD5 getMD5(string md5)
        {
            List<OneMD5> lst = new List<OneMD5>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "md5exists/" + md5;
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneMD5>>(request);
            lst = response.Data;
            return (lst[0]);
        }

        public List<OneMD5> getMD5s(int iLimit, int iOffset)
        {
            List<OneMD5> lst = new List<OneMD5>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "md5s?limit=" + iLimit + "&offset=" + iOffset;
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneMD5>>(request);
            lst = response.Data;
            return (lst);
        }

        public List<OneMD5> getMD5sNotOnEtree()
        {
            List<OneMD5> lst = new List<OneMD5>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "extraMD5s/";
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneMD5>>(request);
            lst = response.Data;
            return (lst);
        }

        public IRestResponse updateMD5(OneMD5 oneMD5)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "md5s/" + oneMD5.MD5ID;
            request.AddParameter("MD5", oneMD5.MD5);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region myTracks
        public IRestResponse addMyTrack(OneMyTrack oneMyTrack)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "mytracks/";
            request.AddParameter("MID", oneMyTrack.MID);
            request.AddParameter("MD5ID", oneMyTrack.MD5ID);
            request.AddParameter("AddedTime", oneMyTrack.AddedTime);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<OneMyTrack>(request);
            return (response);
        }

        public IRestResponse deleteMyTrack(string MD5)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "mytracks/" + MD5;
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneMyTrack>(request);
            return (response);
        }

        public OneMyTrack getMyTrack(string MD5)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "mytracks/" + MD5;
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneMyTrack>>(request);
            var thisData = response.Data;
            return ((OneMyTrack)thisData[0]);
        }

        public List<OneMyTrack> getMyTracks()
        {
            List<OneMyTrack> lst = new List<OneMyTrack>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "mytracks/";
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneMyTrack>>(request);
            lst = response.Data;
            return (lst);
        }

        public List<OneMD5StringOnly> getMyTracksByMemberID(int MemberID, int iLimit, int iOffset)
        {
            List<OneMD5StringOnly> lst = new List<OneMD5StringOnly>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "mytracksbymemberid?limit=" + iLimit + "&offset=" + iOffset + "&mid=" + MemberID;
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneMD5StringOnly>>(request);
            lst = response.Data;
            return (lst);
        }        

        public IRestResponse updateMyTrack(OneMyTrack oneMyTrack)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "mytracks/" + oneMyTrack.MD5ID;
            request.AddParameter("MID", oneMyTrack.MID);
            request.AddParameter("MD5ID", oneMyTrack.MD5ID);
            request.AddParameter("AddedTime", oneMyTrack.AddedTime);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region TrackFileNames
        public IRestResponse addTrackFileName(OneTrackFileName oneTrackFileName)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "trackfilenames/";
            request.AddParameter("FileName", oneTrackFileName.FileName);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<OneTrackFileName>(request);
            return (response);
        }

        public IRestResponse deleteTrackFileName(int id)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "trackfilenames/" + id;
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneTrackFileName>(request);
            return (response);
        }

        public OneTrackFileName getTrackFileName(string trackFileName)
        {
            OneTrackFileName oneTrackFileName = new OneTrackFileName();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "trackfilenameexists/" + trackFileName;
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneTrackFileName>>(request);
            var thisData = response.Data;
            return ((OneTrackFileName)thisData[0]);
        }

        public OneTrackFileName getTrackFileName(int id)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "trackfilenames/" + id;
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneTrackFileName>>(request);
            var thisData = response.Data;
            return ((OneTrackFileName)thisData[0]);
        }

        public List<OneTrackFileName> getTrackFileNames()
        {
            List<OneTrackFileName> lst = new List<OneTrackFileName>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "trackfilenames/";
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneTrackFileName>>(request);
            lst = response.Data;
            return (lst);
        }

        public List<OneTrackFileName> getExtraTrackFileNames()
        {
            List<OneTrackFileName> lst = new List<OneTrackFileName>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "extraTrackFileNames/";
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneTrackFileName>>(request);
            lst = response.Data;
            return (lst);
        }


        public IRestResponse updateTrackFileName(OneTrackFileName oneTrackFileName)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "trackfilenames/" + oneTrackFileName.FNID;
            request.AddParameter("FileName", oneTrackFileName.FileName);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region shows
        public IRestResponse addShow(OneShow oneShow)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "shows/";
            request.AddParameter("ArtistID", oneShow.ArtistID);
            request.AddParameter("Day", oneShow.Day);
            request.AddParameter("EtreeID", oneShow.EtreeID);
            request.AddParameter("Month", oneShow.Month);
            request.AddParameter("Year", oneShow.Year);
            request.AddParameter("VenueMD5", oneShow.VenueMD5);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<OneShow>(request);
            return (response);
        }

        public IRestResponse deleteShow(int iShowID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "shows/" + iShowID.ToString();
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneShow>(request);
            return (response);
        }

        public OneShow getShow(int iShowID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "shows/" + iShowID.ToString();
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneShow>>(request);
            var thisData = response.Data;
            return ((OneShow)thisData[0]);
        }

        public OneShow getShowByEtreeID(int iEtreeID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "ShowIDbyEtreeID/" + iEtreeID.ToString();
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneShow>>(request);
            var thisData = response.Data;
            return ((OneShow)thisData[0]);
        }

        public List<OneShow> getShows()
        {
            List<OneShow> lst = new List<OneShow>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "shows/";
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneShow>>(request);
            lst = response.Data;
            return (lst);
        }

        public IRestResponse updateShow(OneShow oneShow)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "shows/" + oneShow.SID;
            request.AddParameter("ArtistID", oneShow.ArtistID);
            request.AddParameter("EtreeID", oneShow.EtreeID);
            request.AddParameter("Day", oneShow.Day);
            request.AddParameter("Month", oneShow.Month);
            request.AddParameter("Year", oneShow.Year);
            request.AddParameter("VenueMD5", oneShow.VenueMD5);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region showSources
        public IRestResponse addShowSource(OneShowSource oneShowSource)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "showsources/";
            request.AddParameter("ShowID", oneShowSource.ShowID);
            request.AddParameter("SourceID", oneShowSource.SourceID);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<OneShowSource>(request);
            return (response);
        }

        public IRestResponse deleteShowSource(int iShowSourceID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "showsources/" + iShowSourceID.ToString();
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneShowSource>(request);
            return (response);
        }


        public List<OneSourceID> getRelatedSources(int iSourceID)
        {
            List<OneSourceID> lst = new List<OneSourceID>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "getRelatedSources/" + iSourceID;
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneSourceID>>(request);
            lst = response.Data;
            return (lst);
        }

        public OneShowSource getShowSource(int iShowSourceID)
        {
            List<OneShowSource> lst = new List<OneShowSource>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "SSIDbySourceID/" + iShowSourceID.ToString();
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneShowSource>>(request);
            lst = response.Data;
            return (lst[0]);
        }

        public List<OneShowSource> getShowSources()
        {
            return getShowSources(-1);
        }

        public List<OneShowSource> getShowSources(int iShowSourceID)
        {
            List<OneShowSource> lst = new List<OneShowSource>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            if (iShowSourceID > 1)
            {
                request.Resource = "showsources/" + iShowSourceID.ToString();
            }
            else
            {
                request.Resource = "showsources/";
            }
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneShowSource>>(request);
            lst = response.Data;
            return (lst);
        }

        public List<OneFailedToParseSourceID> getShowSourcesWhichFailedToParse()
        {
            List<OneFailedToParseSourceID> lst = new List<OneFailedToParseSourceID>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "parseError/";
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneFailedToParseSourceID>>(request);
            lst = response.Data;
            return (lst);
        }

        public IRestResponse updateShowSource(OneShowSource oneShowSource)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "showsources/" + oneShowSource.SSID;
            request.AddParameter("ShowID", oneShowSource.ShowID);
            request.AddParameter("SourceID", oneShowSource.SourceID);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region sourceInfo
        public IRestResponse addSourceInfo(OneSourceInfo oneSourceInfo)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "sourceinfo/";
            request.AddParameter("ShowSourceID", oneSourceInfo.ShowSourceID);
            request.AddParameter("TaperID", oneSourceInfo.TaperID);
            request.AddParameter("Type", oneSourceInfo.Type);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<OneSourceInfo>(request);
            return (response);
        }

        public IRestResponse deleteSourceInfo(int iSourceInfoID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "sourceinfo/" + iSourceInfoID.ToString();
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneShow>(request);
            return (response);
        }

        public OneSourceInfo getSourceInfoFromShowSourceID(int iShowSourceID)
        {
            List<OneSourceInfo> lst = new List<OneSourceInfo>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "SourceInfoByShowSourceID/" + iShowSourceID.ToString();
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneSourceInfo>>(request);
            lst = response.Data;
            return (lst[0]);
        }

        public List<OneSourceInfo> getSourceInfos()
        {
            return getSourceInfos(-1);
        }

        public List<OneSourceInfo> getSourceInfos(int iSourceInfoID)
        {
            List<OneSourceInfo> lst = new List<OneSourceInfo>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            if (iSourceInfoID > 1)
            {
                request.Resource = "sourceinfo/" + iSourceInfoID.ToString();
            }
            else
            {
                request.Resource = "sourceinfo/";
            }
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneSourceInfo>>(request);
            lst = response.Data;
            return (lst);
        }

        public IRestResponse updateSourceInfo(OneSourceInfo oneSourceInfo)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "sourceinfo/" + oneSourceInfo.SIID;
            request.AddParameter("ShowSourceID", oneSourceInfo.ShowSourceID);
            request.AddParameter("TaperID", oneSourceInfo.TaperID);
            request.AddParameter("Type", oneSourceInfo.Type);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region taper
        public IRestResponse addTaper(OneTaper oneTaper)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "taper/";
            request.AddParameter("Name", oneTaper.Name);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<OneTaper>(request);
            return (response);
        }

        public IRestResponse deleteTaper(int TPID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "taper/" + TPID;
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneTaper>(request);
            return (response);
        }

        public OneTaper getTaper(int TPID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "taper/" + TPID;
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneTaper>>(request);
            var thisData = response.Data;
            return ((OneTaper)thisData[0]);
        }

        public OneTaper getTaperByName(string taperName)
        {
            OneTaper oneTaper = new OneTaper();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "taperByName/" + taperName;
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneTaper>>(request);
            var thisData = response.Data;
            return ((OneTaper)thisData[0]);
        }

        public List<OneTaper> getTapers()
        {
            List<OneTaper> lst = new List<OneTaper>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "taper/";
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneTaper>>(request);
            lst = response.Data;
            return (lst);
        }

        public List<OneSourceID> getSourcesMissingTapers()
        {
            List<OneSourceID> lst = new List<OneSourceID>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "sourcesMissingTapers/";
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneSourceID>>(request);
            lst = response.Data;
            return (lst);
        }
        

        public IRestResponse updateTaper(OneTaper oneTaper)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "taper/" + oneTaper.TPID;
            request.AddParameter("Name", oneTaper.Name);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region tracks
        public IRestResponse addTrack(OneTrack oneTrack)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "tracks/";
            request.AddParameter("ShowSourceID", oneTrack.ShowSourceID);
            request.AddParameter("Number", oneTrack.Number);
            request.AddParameter("FileNameID", oneTrack.FileNameID);
            request.AddParameter("NameID", oneTrack.NameID);
            request.AddParameter("MD5ID", oneTrack.MD5ID);
            request.AddParameter("Length", oneTrack.Length);
            request.AddParameter("AddedTime", oneTrack.AddedTime);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<OneTrack>(request);
            return (response);
        }

        public IRestResponse deleteTrack(int TID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "tracks/" + TID;
            request.AddHeader(customHeader, "OK");
            IRestResponse response = client.Execute<OneTrack>(request);
            return (response);
        }

        public OneTrack getTrack(int TID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "tracks/" + TID;
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneTrack>>(request);
            var thisData = response.Data;
            return ((OneTrack)thisData[0]);
        }

        public List<OneTrack> getTracks()
        {
            List<OneTrack> lst = new List<OneTrack>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "tracks/";
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneTrack>>(request);
            lst = response.Data;
            return (lst);
        }

        public List<OneTrackID> getTrackIDsFromFNID(int iFNID)
        {
            List<OneTrackID> lst = new List<OneTrackID>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "trackIDsFromFNID/" + iFNID;
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneTrackID>>(request);
            lst = response.Data;
            return (lst);
        }

        public List<OneTrackID> getTrackIDsFromNameID(int iNameID)
        {
            List<OneTrackID> lst = new List<OneTrackID>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "trackIDsFromNameID/" + iNameID;
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneTrackID>>(request);
            lst = response.Data;
            return (lst);
        }

        public List<OneTrack> getTrackIDsFromShowSourceID(int iShowSourceID)
        {
            List<OneTrack> lst = new List<OneTrack>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "trackIDsFromShowSourceID/" + iShowSourceID;
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneTrack>>(request);
            lst = response.Data;
            return (lst);
        }

        public IRestResponse updateTrack(OneTrack oneTrack)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "tracks/" + oneTrack.TID;
            request.AddParameter("ShowSourceID", oneTrack.ShowSourceID);
            request.AddParameter("Number", oneTrack.Number);
            request.AddParameter("FileNameID", oneTrack.FileNameID);
            request.AddParameter("NameID", oneTrack.NameID);
            request.AddParameter("MD5ID", oneTrack.MD5ID);
            request.AddParameter("Length", oneTrack.Length);
            request.AddParameter("AddedTime", oneTrack.AddedTime);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region tracks names
        public IRestResponse addTrackName(OneTrackName oneTrackName)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "tracknames/";
            request.AddParameter("TrackName", oneTrackName.TrackName);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<OneTrackName>(request);
            return (response);
        }

        public IRestResponse deleteTrackName(int iTNID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "tracknames/" + iTNID;
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneTrackName>(request);
            return (response);
        }

        public OneTrackName getTrackName(int iTrackNameID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "tracknames/" + iTrackNameID;
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneTrackName>>(request);
            var thisData = response.Data;
            return ((OneTrackName)thisData[0]);
        }

        public List<OneTrackName> getTrackNames()
        {
            List<OneTrackName> lst = new List<OneTrackName>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "tracknames/";
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneTrackName>>(request);
            lst = response.Data;
            return (lst);
        }

        public IRestResponse updateTrackName(OneTrackName oneTrackName)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "tracknames/" + oneTrackName.TNID;
            request.AddParameter("TrackName", oneTrackName.TrackName);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        #region venues
        public IRestResponse addVenue(OneVenue oneVenue)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.POST);
            request.Resource = "venues/";
            request.AddParameter("CityID", oneVenue.CityID);
            request.AddParameter("CountryID", oneVenue.CountryID);
            request.AddParameter("State", oneVenue.State);
            request.AddParameter("VenueMD5", oneVenue.VenueMD5);
            request.AddParameter("VenueName", oneVenue.VenueName);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<OneVenue>(request);
            return (response);
        }

        public IRestResponse deleteVenue(int iVenueID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.DELETE);
            request.Resource = "venues/" + iVenueID.ToString();
            request.AddHeader(customHeader, "OK"); 
            IRestResponse response = client.Execute<OneVenue>(request);
            return (response);
        }

        public OneVenue getVenue(int iVenueID)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "venues/" + iVenueID.ToString();
            request.AddHeader(customHeader, "OK");
            var response = client.Execute<List<OneVenue>>(request);
            var thisData = response.Data;
            return ((OneVenue)thisData[0]);
        }

        public List<OneVenue> getVenues()
        {
            List<OneVenue> lst = new List<OneVenue>();
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest();
            request.Resource = "venues/";
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute<List<OneVenue>>(request);
            lst = response.Data;
            return (lst);
        }

        public IRestResponse updateVenue(OneVenue oneVenue)
        {
            var client = new RestClient();
            client.BaseUrl = APIBaseURL;
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest(Method.PUT);
            request.Resource = "venues/" + oneVenue.VID;
            request.AddParameter("CityID", oneVenue.CityID);
            request.AddParameter("CountryID", oneVenue.CountryID);
            request.AddParameter("State", oneVenue.State);
            request.AddParameter("VenueMD5", oneVenue.VenueMD5);
            request.AddParameter("VenueName", oneVenue.VenueName);
            request.AddHeader(customHeader, "OK"); 
            var response = client.Execute(request);
            return (response);
        }
        #endregion

        public string getLineForLog(IRestResponse iRestResponse)
        {
            ResultMessage rm = new JavaScriptSerializer().Deserialize<ResultMessage>(iRestResponse.Content);
            string strResponse = rm.result.Message + strDelimiter;
            strResponse += rm.result.Code + strDelimiter;
            strResponse += rm.result.ID + strDelimiter;
            return strResponse;
        }

        public int getIDFromRestResponse(IRestResponse iRestResponse)
        {
            ResultMessage rm = new JavaScriptSerializer().Deserialize<ResultMessage>(iRestResponse.Content);
            int iID = rm.result.ID;
            return iID;
        }

        public int ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return (int)Math.Floor(diff.TotalSeconds);
        }

        #endregion
    }

    public class OneArtist
    {
        public int AID { get; set; }
        public string Artist { get; set; }
        public string ShortName { get; set; }
    }

    public class OneCity
    {
        public int CIID { get; set; }
        public string City { get; set; }
    }

    public class OneCountry
    {
        public int COID { get; set; }
        public string Country { get; set; }
    }

    public class OneDT
    {
        public int DTID { get; set; }
        public int ShowSourceID { get; set; }
        public string Name { get; set; }
        public int Free { get; set; }
    }

    public class OneMyDT
    {
        public int DTID { get; set; }
        public int SourceID { get; set; }        
    }

    public class LastDTID
    {
        public int LastDT { get; set; }
    }

    public class OneMD5StringOnly
    {
        public string MD5 { get; set; }
    }

    public class OneMD5
    {
        public int MD5ID { get; set; }
        public string MD5 { get; set; }
    }

    public class OneMemberID
    {
        public int MID { get; set; }        
    }

    public class OneMyTrack
    {
        public int MID { get; set; }
        public int MD5ID { get; set; }
        public int AddedTime { get; set; }
    }

    public class OneTrackFileName
    {
        public int FNID { get; set; }
        public string FileName { get; set; }
    }

    public class OneTrackName
    {
        public int TNID { get; set; }
        public string TrackName { get; set; }
    }

    public class OneShow
    {
        public int SID { get; set; }
        public int EtreeID { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int ArtistID { get; set; }
        public string VenueMD5 { get; set; }
    }

    public class OneShowSource
    {
        public int SSID { get; set; }
        public int ShowID { get; set; }
        public int SourceID { get; set; }
    }

    public class OneSourceID
    {
        public int SourceID { get; set; }
    }

    public class OneFailedToParseSourceID
    {
        public int SourceID { get; set; }
    }

    public class OneSourceDetails
    {
        //TID, Number, TrackName, Length, TNID, FileName, MD5, tracks.MD5ID
        public int TID { get; set; }
        public int Number { get; set; }
        public string TrackName { get; set; }
        public string Length { get; set; }
        public int TNID { get; set; }
        public string FileName { get; set; }
        public string MD5 { get; set; }
        public int MD5ID { get; set; }        
    }


    public class OneSourceInfo
    {
        public int SIID { get; set; }
        public int ShowSourceID { get; set; }
        public int TaperID { get; set; }
        public int Type { get; set; }
    }

    public class OneTaper
    {
        public int TPID { get; set; }
        public string Name { get; set; }
    }

    public class OneTrack
    {
        public int TID { get; set; }
        public int ShowSourceID { get; set; }
        public int Number { get; set; }
        public int FileNameID { get; set; }
        public int NameID { get; set; }
        public int MD5ID { get; set; }
        public string Length { get; set; }
        public int AddedTime { get; set; }
    }

    public class OneTrackID
    {
        public int TID { get; set; }
    }

    public class OneVenue
    {
        public int VID { get; set; }
        public string VenueMD5 { get; set; }
        public string VenueName { get; set; }
        public int CityID { get; set; }
        public string State { get; set; }
        public int CountryID { get; set; }
    }

    public class ResultMessage
    {
        [JsonProperty("result")]
        public ResultContent result { get; set; }
    }

    public class ResultContent
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("id")]
        public int ID { get; set; }
    }
}
