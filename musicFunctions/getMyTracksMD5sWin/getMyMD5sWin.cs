using getMyTracksMD5sWin.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Flac;
using Flac.Metadata;
using musicFunctions;
using HtmlAgilityPack;

namespace getMyTracksMD5sWin
{
    public partial class getMyMD5sWin : Form
    {
        private MD5Functions objMD5Functions = new MD5Functions();
        private BackgroundWorker bgwGetMD5s = new BackgroundWorker();
        private BackgroundWorker bgwFindDuplicates = new BackgroundWorker();
        private BackgroundWorker bgwGetSeedsNeededOnDT = new BackgroundWorker();
        private FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        private DataTable dtSeedsNeeded = MD5Functions.createDT2EtreeIDs();

        public getMyMD5sWin()
        {
            FormClosing += new FormClosingEventHandler(getMyMD5sWin_FormClosing);
            //dgvSeedsNeededOnDT.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(dgvSeedsNeededOnDT_ColumnHeaderMouseClick);
            InitializeComponent();
            initBGWGetMD5s();
            initBGWReportDuplicates();
            initBGWGetSeedsNeededOnDT();
        }

        void dgvSeedsNeededOnDT_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            throw new NotImplementedException();
        }

        private void getMyMD5sWin_Load(object sender, EventArgs e)
        {
            txtMySecret.Text = Settings.Default.mySecret;
            lblGetMD5sFolderPath.Text = Settings.Default.startupFolder;
        }

        private void getMyMD5sWin_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.mySecret = txtMySecret.Text;
            Settings.Default.startupFolder = lblGetMD5sFolderPath.Text;
            Settings.Default.Save();
        }

        private void resetFields()
        {
            txtGetMD5sResults.Text = string.Empty;
            txtGetMD5sErrors.Text = string.Empty;
            lblFLACCount.Text = "FLAC Count: 0";
            lblSHNCount.Text = "SHN Count: 0";
            lblWAVCount.Text = "WAV Count: 0";
        }

        private void btnGetMD5s_Click(object sender, EventArgs e)
        {
            Settings.Default.Save();
            try
            {
                OneMemberID memberIdFromSecret = new functions(ConfigurationManager.AppSettings["APIBaseUrl"]).getMemberIDFromSecret(txtMySecret.Text);
                int mid = memberIdFromSecret.MID;
                if (mid.Equals(0))
                {
                    resetFields();
                    throw new Exception("Secret is not valid!");
                }
                else if (!Directory.Exists(this.lblGetMD5sFolderPath.Text))
                {
                    resetFields();
                    throw new Exception("Directory " + this.lblGetMD5sFolderPath.Text + " not found!");
                }
                else
                {
                    Button workerButton = (Button)sender;
                    if (workerButton.Text.Equals("S&earch"))
                    {
                        //string sourceDir, string folderMask, string fileMask, string strMemberSecret, string MemberID
                        string[] args = new string[] 
                        {
                            lblGetMD5sFolderPath.Text, 
                            txtGetMD5sFolderMask.Text, 
                            txtGetMD5sFileMask.Text, 
                            txtMySecret.Text,
                            mid.ToString()
                        };
                        txtGetMD5sResults.Text = "";
                        proBarGetMD5sFilesFound.Style = ProgressBarStyle.Marquee;
                        bgwGetMD5s.RunWorkerAsync(args);
                        workerButton.Text = "&Cancel";
                        btnGetMD5sSelectFolder.Enabled = false;
                    }
                    else
                    {
                        txtGetMD5sResults.AppendText("Cancelling in progress. Please wait ..." + Environment.NewLine);
                        if (bgwGetMD5s.IsBusy)
                        {
                            bgwGetMD5s.CancelAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TextBox textBox = this.txtGetMD5sErrors;
                string str = textBox.Text + ex.Message;
                textBox.Text = str;
            }
        }

        private void initBGWGetMD5s()
        {
            bgwGetMD5s.WorkerReportsProgress = true;
            bgwGetMD5s.WorkerSupportsCancellation = true;
            bgwGetMD5s.DoWork += new DoWorkEventHandler(bgwGetMD5s_DoWork);
            bgwGetMD5s.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwGetMD5s_RunWorkerCompleted);
            bgwGetMD5s.ProgressChanged += new ProgressChangedEventHandler(bgwGetMD5s_ProgressChanged);
        }

        private void bgwGetMD5s_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            string[] strArray = (string[])e.Argument;
            objMD5Functions.getMD5s(worker, e, strArray[0], strArray[1], strArray[2], strArray[3], strArray[4]);
        }

        private void bgwGetMD5s_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 0:
                    this.txtGetMD5sResults.AppendText((string)e.UserState);
                    break;
                case 1:
                    this.txtGetMD5sErrors.AppendText((string)e.UserState);
                    break;
                case 2:
                    this.txtGetMD5sResults.Text = (string)e.UserState;
                    break;
                case 3:
                    this.lblFLACCount.Text = (string)e.UserState;
                    break;
                case 4:
                    this.lblSHNCount.Text = (string)e.UserState;
                    break;
                case 5:
                    this.lblWAVCount.Text = (string)e.UserState;
                    break;
            }
        }

        private void bgwGetMD5s_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            proBarGetMD5sFilesFound.Style = ProgressBarStyle.Continuous;
            btnGetMD5s.Text = "S&earch";
            btnGetMD5sSelectFolder.Enabled = true;
            if (e.Cancelled)
            {
                txtGetMD5sResults.AppendText("Cancelled!" + Environment.NewLine);
            }
            else
            {
                txtGetMD5sResults.AppendText("Finished" + Environment.NewLine);
                if (e.Result != null)
                {
                    if (e.Result.Equals("Nothing new"))
                    {
                        txtGetMD5sResults.AppendText("No new tracks found!");
                    }
                }
                else
                {
                    txtGetMD5sResults.AppendText("Please upload the results file saved at " + this.lblGetMD5sFolderPath.Text + MD5Functions.outFile);
                }
            }
        }

        private void btnReportDuplicates_Click2(object sender, EventArgs e)
        {
            if (!Directory.Exists(this.lblGetMD5sFolderPath.Text))
                throw new Exception("Directory " + this.lblGetMD5sFolderPath.Text + " not found!");
            Button button = (Button)sender;
            if (button.Text.Equals("&Find Duplicates"))
            {
                string[] strArray = new string[5]
        {
          lblGetMD5sFolderPath.Text,
          txtGetMD5sFolderMask.Text,
          txtGetMD5sFileMask.Text,
          txtMySecret.Text,
          "1"
        };
                this.txtGetMD5sResults.Text = string.Empty;
                this.txtGetMD5sErrors.Text = string.Empty;
                this.lblFLACCount.Text = "FLAC Count: 0";
                this.lblSHNCount.Text = "SHN Count: 0";
                this.lblWAVCount.Text = "WAV Count: 0";
                this.proBarGetMD5sFilesFound.Style = ProgressBarStyle.Marquee;
                this.bgwFindDuplicates.RunWorkerAsync(strArray);
                button.Text = "&Cancel";
                this.btnGetMD5sSelectFolder.Enabled = false;
            }
            else
            {
                this.txtGetMD5sResults.AppendText("Cancelling in progress. Please wait ..." + Environment.NewLine);
                if (this.bgwFindDuplicates.IsBusy)
                    this.bgwFindDuplicates.CancelAsync();
            }
        }

        private void initBGWReportDuplicates()
        {
            this.bgwFindDuplicates.WorkerReportsProgress = true;
            this.bgwFindDuplicates.WorkerSupportsCancellation = true;
            this.bgwFindDuplicates.DoWork += new DoWorkEventHandler(this.bgwFindDuplicates_DoWork);
            this.bgwFindDuplicates.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bgwFindDuplicates_RunWorkerCompleted);
            this.bgwFindDuplicates.ProgressChanged += new ProgressChangedEventHandler(this.bgwFindDuplicates_ProgressChanged);
        }

        private void bgwFindDuplicates_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            string[] strArray = (string[])e.Argument;
            this.objMD5Functions.FindDuplicates(worker, e, strArray[0], strArray[1], strArray[2], strArray[3], strArray[4]);
        }

        private void bgwFindDuplicates_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 0:
                    this.txtGetMD5sResults.AppendText((string)e.UserState);
                    break;
                case 1:
                    this.txtGetMD5sErrors.AppendText((string)e.UserState);
                    break;
                case 2:
                    this.txtGetMD5sResults.Text = (string)e.UserState;
                    break;
                case 3:
                    this.lblFLACCount.Text = (string)e.UserState;
                    break;
                case 4:
                    this.lblSHNCount.Text = (string)e.UserState;
                    break;
                case 5:
                    this.lblWAVCount.Text = (string)e.UserState;
                    break;
            }
        }

        private void bgwFindDuplicates_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.proBarGetMD5sFilesFound.Style = ProgressBarStyle.Continuous;
            this.btnReportDuplicates.Text = "&Find Duplicates";
            this.btnGetMD5sSelectFolder.Enabled = true;
            if (e.Cancelled)
            {
                this.txtGetMD5sResults.AppendText("Cancelled!" + Environment.NewLine);
            }
            else
            {
                this.txtGetMD5sResults.AppendText("Finished, check the results below for duplicates." + Environment.NewLine);
            }
        }

        private void lblSave_Click(object sender, EventArgs e)
        {
            string path = this.lblGetMD5sFolderPath.Text + "\\md5sErrorLog.csv";
            try
            {
                File.WriteAllText(path, this.txtGetMD5sErrors.Text);
                int num = (int)MessageBox.Show("Error log saved to " + path, "Error log", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Failed to save log file to " + path, "Error log", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void btnGetMD5sSelectFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.ShowNewFolderButton = false;
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                lblGetMD5sFolderPath.Text = this.folderBrowserDialog.SelectedPath;
            }
            txtGetMD5sResults.Text = "";
        }

        private void initBGWGetSeedsNeededOnDT()
        {
            bgwGetSeedsNeededOnDT.WorkerReportsProgress = true;
            bgwGetSeedsNeededOnDT.WorkerSupportsCancellation = true;
            bgwGetSeedsNeededOnDT.DoWork += new DoWorkEventHandler(bgwGetSeedsNeededOnDT_DoWork);
            bgwGetSeedsNeededOnDT.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwGetSeedsNeededOnDT_RunWorkerCompleted);
            bgwGetSeedsNeededOnDT.ProgressChanged += new ProgressChangedEventHandler(bgwGetSeedsNeededOnDT_ProgressChanged);
        }

        private void bgwGetSeedsNeededOnDT_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            string[] strArray = (string[])e.Argument;
            objMD5Functions.getSeedsNeededOnDT(worker, e, strArray[0]);
        }

        private void bgwGetSeedsNeededOnDT_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 0:
                    dtSeedsNeeded.ImportRow((DataRow)e.UserState);
                    break;
                case 1:
                    lblCurrentOne.Text = "Searching ... :" + (string)e.UserState;
                    break;
                case 2:
                    this.txtGetMD5sResults.Text = (string)e.UserState;
                    break;
                case 3:
                    this.lblFLACCount.Text = (string)e.UserState;
                    break;
                case 4:
                    this.lblSHNCount.Text = (string)e.UserState;
                    break;
                case 5:
                    this.lblWAVCount.Text = (string)e.UserState;
                    break;
            }
        }

        private void bgwGetSeedsNeededOnDT_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            proBarGetMD5sFilesFound.Style = ProgressBarStyle.Continuous;
            btnGetSeedsNeededOnDT.Text = "&Search";


            if (e.Cancelled)
            {

            }
            else
            {
                if (e.Result != null)
                {
                    if (e.Result.Equals("Nothing new"))
                    {
                        txtGetMD5sResults.AppendText("No new tracks found!");
                    }
                }
                else
                {
                    dgvSeedsNeededOnDT.DataSource = dtSeedsNeeded;
                    lblCurrentOne.Text = "Done!";
                    //    dtSeedsNeeded 
                }
            }
        }

        private void btnGetSeedsNeededOnDT_Click(object sender, EventArgs e)
        {
            Settings.Default.Save();
            try
            {
                OneMemberID memberIdFromSecret = new functions(ConfigurationManager.AppSettings["APIBaseUrl"]).getMemberIDFromSecret(txtMySecret.Text);
                int mid = memberIdFromSecret.MID;
                if (mid.Equals(0))
                {
                    resetFields();
                    throw new Exception("Secret is not valid!");
                }
                else
                {
                    Button workerButton = (Button)sender;
                    if (workerButton.Text.Equals("&Search"))
                    {
                        string[] args = new string[] { mid.ToString() };
                        proBarGetMD5sFilesFound.Style = ProgressBarStyle.Marquee;
                        bgwGetSeedsNeededOnDT.RunWorkerAsync(args);
                        workerButton.Text = "&Cancel";
                    }
                    else
                    {
                        lblCurrentOne.Text = "Cancelling in progress. Please wait ...";
                        if (bgwGetSeedsNeededOnDT.IsBusy)
                        {
                            bgwGetSeedsNeededOnDT.CancelAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TextBox textBox = txtGetMD5sErrors;
                string str = textBox.Text + ex.Message;
                textBox.Text = str;
            }
        }

        private void chkOnlyShowMine_CheckedChanged(object sender, EventArgs e)
        {

        }
    }

    public class MD5Functions
    {

        private static string DTBaseURL = "http://www.dreamingtree.org/";
        public static string outFile = string.Empty;
        private static string strSecretSecret = functions.generateMD5hash(ConfigurationManager.AppSettings["SecretSecret"]);
        private static List<FileInfo> musicFiles = new List<FileInfo>();
        private static List<FileInfo> musicFilesRedo = new List<FileInfo>();
        private static List<OneMD5StringOnly> lstMyExistingMD5 = new List<OneMD5StringOnly>();
        private static List<OneMD5> lstExistingMD5s = new List<OneMD5>();
        private static List<string> lstMyExistingMD5Strings = new List<string>();
        private static List<OneMD5FileDetails> lstPossibleDuplicatesMD5s = new List<OneMD5FileDetails>();
        private static Dictionary<string, int> dctMD5IDs = new Dictionary<string, int>();
        private static DataTable dtOutPut = createOutputTable();
        private static int iFLACCount = 0;
        private static int iSHNCount = 0;
        private static int iWAVCount = 0;

        static MD5Functions()
        {
        }

        public void getSeedsNeededOnDT(BackgroundWorker worker, DoWorkEventArgs e, string MemberID)
        {
            functions functions = new musicFunctions.functions(ConfigurationManager.AppSettings["APIBaseUrl"]);

            List<OneMyDT> lstMyDTSources = functions.getMyDTSources(Int32.Parse(MemberID));

            WebClient webClient = new WebClient();

            CookieContainer cookies = new CookieContainer();
            Cookie uidcookie = new Cookie("uid", ConfigurationManager.AppSettings["UID"], "/", "www.dreamingtree.org");
            Cookie passcookie = new Cookie("pass", ConfigurationManager.AppSettings["Passkey"], "/", "www.dreamingtree.org");
            cookies.Add(uidcookie);
            cookies.Add(passcookie);

            DataTable dt = createDT2EtreeIDs();

            int j = 0;
            int iLeechers = 1;
            while (iLeechers > 0)
            {
                string strHTML = musicFunctions.functions.getWebPage(DTBaseURL + "browse.php?incldead=1&sort=8&type=desc&page=" + j, cookies);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(strHTML);

                foreach (HtmlNode hnLogEntry in doc.DocumentNode.SelectNodes("/html[1]/body[1]/table[3]/tr[1]/td[1]/tr[1]/td[1]/table[2]/tr"))
                {
                    if (!hnLogEntry.ChildNodes[1].InnerText.Equals("Category"))
                    {
                        DataRow dr = dt.NewRow();
                        string strName = hnLogEntry.ChildNodes[3].InnerText.Trim();
                        string dllink = hnLogEntry.ChildNodes[3].ChildNodes[2].Attributes["href"].Value;
                        string dtID = dllink.Substring(13, dllink.IndexOf("/", 14) - 13);
                        int iThisDTID = Int32.Parse(dtID);
                        int iSeeders = Int32.Parse(hnLogEntry.ChildNodes[13].InnerText.Trim());
                        iLeechers = Int32.Parse(hnLogEntry.ChildNodes[15].InnerText.Trim());

                        if (iLeechers.Equals(0))
                        {
                            break;
                        }

                        if (iSeeders < 5)
                        {
                            var oneIHave = from row in lstMyDTSources
                                           where row.DTID.Equals(iThisDTID)
                                           select row;


                            dr["DTID"] = iThisDTID;
                            dr["Seeders"] = iSeeders;
                            dr["Leeches"] = iLeechers;
                            dr["TorrentName"] = strName;
                            dr["Download"] = dllink;
                            dr["Have"] = oneIHave.FirstOrDefault<OneMyDT>() != null ? "Yes" : "No";
                            dt.Rows.Add(dr);
                            worker.ReportProgress(0, dr);
                            worker.ReportProgress(1, strName);
                        }
                        iLeechers = 0;
                    }
                }
                j++;
            }
        }

        public void getMD5s(BackgroundWorker worker, DoWorkEventArgs e, string sourceDir, string folderMask, string fileMask, string strMemberSecret, string MemberID)
        {
            dtOutPut.Clear();
            outFile = "\\dmbMD5s." + MemberID + "." + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".csv";
            try
            {
                int num = int.Parse(MemberID);
                iFLACCount = 0;
                iSHNCount = 0;
                iWAVCount = 0;
                this.getCurrentData(worker, strMemberSecret, sourceDir, num);
                DirectoryInfo dir = new DirectoryInfo(sourceDir);
                musicFiles.Clear();
                bool cancel = e.Cancel;
                if (cancel.Equals(false))
                {
                    worker.ReportProgress(0, "Finding .wav files" + Environment.NewLine);
                    FullDirList(worker, e, dir, musicFiles, "*" + fileMask + "*.wav");
                }
                cancel = e.Cancel;
                if (cancel.Equals(false))
                {
                    worker.ReportProgress(0, "Finding .flac files" + Environment.NewLine);
                    FullDirList(worker, e, dir, musicFiles, "*" + fileMask + "*.flac");
                }
                cancel = e.Cancel;
                if (cancel.Equals(false))
                {
                    worker.ReportProgress(0, "Finding .shn files" + Environment.NewLine);
                    FullDirList(worker, e, dir, musicFiles, "*" + fileMask + "*.shn");
                }
                cancel = e.Cancel;
                if (cancel.Equals(false))
                {
                    worker.ReportProgress(0, musicFiles.Count + " files found" + Environment.NewLine);
                }

                if (!worker.CancellationPending)
                {
                    processFiles(worker, e, strMemberSecret, num);
                }

                musicFiles.Clear();

                if (!musicFilesRedo.Count.Equals(0))
                {
                    musicFiles = Enumerable.ToList<FileInfo>((IEnumerable<FileInfo>)musicFilesRedo);
                    worker.ReportProgress(0, musicFiles.Count + " failed first time around. Retrying ... " + Environment.NewLine);
                    processFiles(worker, e, strMemberSecret, num);
                }
                cancel = e.Cancel;

                if (!cancel.Equals(false))
                {
                    return;
                }
                if (dtOutPut.Rows.Count > 0)
                {
                    functions.writeDataToFile(sourceDir + outFile, dtOutPut, "MD5ID", false);
                }
                else
                {
                    e.Result = "Nothing new";
                }
            }
            catch (FileLoadException ex)
            {
                worker.ReportProgress(1, "An error occurred: " + ex.Message);
            }
        }

        public void FindDuplicates(BackgroundWorker worker, DoWorkEventArgs e, string sourceDir, string folderMask, string fileMask, string strMemberSecret, string MemberID)
        {
            dtOutPut.Clear();
            outFile = "\\duplicates." + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".csv";
            try
            {
                int.Parse(MemberID);
                iFLACCount = 0;
                iSHNCount = 0;
                iWAVCount = 0;
                DirectoryInfo dir = new DirectoryInfo(sourceDir);
                musicFiles.Clear();
                if (e.Cancel.Equals(false))
                {
                    worker.ReportProgress(0, "Finding .wav files" + Environment.NewLine);
                    FullDirList(worker, e, dir, musicFiles, "*" + fileMask + "*.wav");
                }
                bool cancel = e.Cancel;
                if (cancel.Equals(false))
                {
                    worker.ReportProgress(0, "Finding .flac files" + Environment.NewLine);
                    FullDirList(worker, e, dir, musicFiles, "*" + fileMask + "*.flac");
                }
                cancel = e.Cancel;
                if (cancel.Equals(false))
                {
                    worker.ReportProgress(0, "Finding .shn files" + Environment.NewLine);
                    FullDirList(worker, e, dir, musicFiles, "*" + fileMask + "*.shn");
                }
                cancel = e.Cancel;
                if (cancel.Equals(false))
                    worker.ReportProgress(0, musicFiles.Count + " files found" + Environment.NewLine);
                if (!worker.CancellationPending)
                    findDuplicateFiles(worker, e);
                musicFiles.Clear();
                if (!musicFilesRedo.Count.Equals(0))
                {
                    musicFiles = Enumerable.ToList<FileInfo>((IEnumerable<FileInfo>)musicFilesRedo);
                    worker.ReportProgress(0, musicFiles.Count + " failed first time around. Retrying ... " + Environment.NewLine);
                    findDuplicateFiles(worker, e);
                }

                if (!e.Cancel.Equals(false))
                {
                    return;
                }
                if (dtOutPut.Rows.Count > 0)
                {
                    functions.writeDataToFile(sourceDir + outFile, dtOutPut, "MD5ID", false);
                }
                else
                {
                    e.Result = "Nothing new";
                }
            }
            catch (Exception ex)
            {
                worker.ReportProgress(1, "An error occurred: " + ex.Message);
            }
        }

        public void getCurrentData(BackgroundWorker worker, string strMySecret, string strDirectory, int iMemberID)
        {
            functions functions = new functions(ConfigurationManager.AppSettings["APIBaseUrl"]);
            int iLimit = 5000;
            worker.ReportProgress(0, "Getting your existing tracks ... " + Environment.NewLine);
            worker.ReportProgress(0, "Getting batch ");
            int iOffset1 = 0;
            for (lstMyExistingMD5 = functions.getMyTracksByMemberID(iMemberID, iLimit, iOffset1); lstMyExistingMD5.Count > 1; lstMyExistingMD5 = functions.getMyTracksByMemberID(iMemberID, iLimit, iOffset1))
            {
                if (!worker.CancellationPending)
                {
                    worker.ReportProgress(0, " - " + iOffset1);
                    foreach (OneMD5StringOnly oneMd5StringOnly in lstMyExistingMD5)
                    {
                        if (!lstMyExistingMD5Strings.Contains(oneMd5StringOnly.MD5))
                        {
                            lstMyExistingMD5Strings.Add(oneMd5StringOnly.MD5);
                        }
                    }
                    iOffset1 += iLimit;
                }
                else
                {
                    worker.ReportProgress(0, Environment.NewLine);
                    break;
                }
            }
            if (worker.CancellationPending)
            {
                return;
            }
            worker.ReportProgress(0, Environment.NewLine);
            worker.ReportProgress(0, lstMyExistingMD5Strings.Count + " tracks found." + Environment.NewLine);
            worker.ReportProgress(0, "Getting existing MD5 strings from the system ... " + Environment.NewLine);
            worker.ReportProgress(0, "Getting batch ");

            int iOffset2 = 0;
            for (lstExistingMD5s = functions.getMD5s(iLimit, iOffset2); lstExistingMD5s != null; lstExistingMD5s = functions.getMD5s(iLimit, iOffset2))
            {
                if (!worker.CancellationPending)
                {
                    worker.ReportProgress(0, " - " + iOffset2);
                    foreach (OneMD5 oneMd5 in lstExistingMD5s)
                    {
                        if (!dctMD5IDs.ContainsKey(oneMd5.MD5))
                        {
                            dctMD5IDs.Add(oneMd5.MD5, oneMd5.MD5ID);
                        }
                    }
                    iOffset2 += iLimit;
                }
                else
                {
                    worker.ReportProgress(0, Environment.NewLine);
                    break;
                }
            }
            worker.ReportProgress(0, Environment.NewLine);
            worker.ReportProgress(0, dctMD5IDs.Count + " MD5 strings found in the system." + Environment.NewLine);
        }

        private static void findDuplicateFiles(BackgroundWorker worker, DoWorkEventArgs e)
        {
            int num = 1;
            foreach (FileInfo thisFileInfo in musicFiles)
            {
                if (!worker.CancellationPending)
                {
                    string strFileName = Path.GetFileName(thisFileInfo.FullName).ToLower();
                    try
                    {
                        string strMD5String;
                        if (thisFileInfo.FullName.EndsWith(".flac"))
                        {
                            FlacFile flacFile = new FlacFile(thisFileInfo.FullName);
                            Metadata metadata = flacFile.Metadata;
                            strMD5String = flacFile.StreamInfo.MD5String.ToLower();
                        }
                        else
                        {
                            strMD5String = functions.getMD5OfFile(thisFileInfo).ToLower();
                        }
                        lstPossibleDuplicatesMD5s.Add(new OneMD5FileDetails() { FileName = strFileName, FolderName = thisFileInfo.DirectoryName, MD5 = strMD5String });
                        worker.ReportProgress(0, num++ + "/" + musicFiles.Count + ";" + strFileName + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        worker.ReportProgress(0, num++ + "/" + musicFiles.Count + ";" + strFileName + Environment.NewLine);
                        Console.WriteLine(ex.Message);
                        musicFilesRedo.Add(thisFileInfo);
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private static void processFiles(BackgroundWorker worker, DoWorkEventArgs e, string strMemberSecret, int MemberID)
        {
            int num = 1;
            bool bSHNToolExists = File.Exists(Directory.GetCurrentDirectory() + "\\shntool.exe");
            foreach (FileInfo thisFileInfo in musicFiles)
            {
                if (!worker.CancellationPending)
                {
                    string strFileName = Path.GetFileName(thisFileInfo.FullName).ToLower();
                    try
                    {
                        string strMD5String;
                        string strDuration;
                        if (thisFileInfo.FullName.EndsWith(".flac"))
                        {
                            FlacFile flacFile = new FlacFile(thisFileInfo.FullName);
                            Metadata metadata = flacFile.Metadata;
                            strMD5String = flacFile.StreamInfo.MD5String.ToLower();
                            strDuration = getFFPDuration(flacFile.Duration);
                        }
                        else
                        {
                            strMD5String = functions.getMD5OfFile(thisFileInfo).ToLower();
                            strDuration = bSHNToolExists ? getSHNDuration(thisFileInfo.FullName) : "";
                        }
                        if (lstMyExistingMD5Strings.Contains(strMD5String))
                        {
                            worker.ReportProgress(1, num++ + "/" + musicFiles.Count + ";Already added;" + strMD5String + ";" + thisFileInfo.FullName + Environment.NewLine);
                        }
                        else if (dctMD5IDs.ContainsKey(strMD5String))
                        {
                            DataRow row = dtOutPut.NewRow();
                            row["MemberID"] = MemberID;
                            row["MD5ID"] = dctMD5IDs[strMD5String];
                            row["Length"] = strDuration;
                            row["Checksum"] = functions.generateMD5hash(strSecretSecret + strMemberSecret + dctMD5IDs[strMD5String]);
                            dtOutPut.Rows.Add(row);
                            lstMyExistingMD5Strings.Add(strMD5String);
                            worker.ReportProgress(0, num++ + "/" + musicFiles.Count + ";" + strFileName + Environment.NewLine);
                        }
                        else
                        {
                            worker.ReportProgress(1, num++ + "/" + musicFiles.Count + ";Not on Etree;" + strMD5String + ";" + thisFileInfo.FullName + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        worker.ReportProgress(0, num++ + "/" + musicFiles.Count + ";" + strFileName + Environment.NewLine);
                        Console.WriteLine(ex.Message);
                        musicFilesRedo.Add(thisFileInfo);
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private static void FullDirList(BackgroundWorker worker, DoWorkEventArgs e, DirectoryInfo dir, List<FileInfo> thisList, string searchPattern)
        {
            try
            {
                foreach (FileInfo fileInfo in dir.GetFiles(searchPattern))
                {
                    thisList.Add(fileInfo);
                    if (searchPattern.EndsWith("*.flac"))
                    {
                        worker.ReportProgress(3, "FLAC Count: " + (++iFLACCount).ToString());
                    }
                    else if (searchPattern.EndsWith("*.shn"))
                    {
                        worker.ReportProgress(4, "SHN Count: " + (++iSHNCount).ToString());
                    }
                    else if (searchPattern.EndsWith("*.wav"))
                    {
                        worker.ReportProgress(5, "WAV Count: " + (++iWAVCount).ToString());
                    }
                }
            }
            catch
            {
                return;
            }
            foreach (DirectoryInfo dir1 in dir.GetDirectories())
            {
                if (!worker.CancellationPending)
                {
                    FullDirList(worker, e, dir1, thisList, searchPattern);
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        public class OneMD5FileDetails
        {
            public string MD5 { get; set; }
            public string FileName { get; set; }
            public string FolderName { get; set; }
            public int Free { get; set; }
        }

        public static DataTable createDT2EtreeIDs()
        {
            return new DataTable()
            {
                Columns = { 
                                {"DTID", typeof (int) },
                                {"Seeders",typeof (int)},
                                {"Leeches",typeof (int)},
                                {"TorrentName",typeof (string)},
                                {"Download",typeof (string)},
                                {"Have",typeof (string)}
                          }
            };
        }

        private static DataTable createOutputTable()
        {
            return new DataTable()
            {
                Columns = { 
                                { "MemberID", typeof (int) },
                                {"MD5ID",typeof (int)},
                                {"Length",typeof (string)},
                                {"Checksum",typeof (string)}
                          }
            };
        }

        private static string getFFPDuration(Time duration)
        {
            int num = (int)Math.Pow(10.0, (double)(7 - 0));
            TimeSpan result = new TimeSpan();
            TimeSpan timeSpan = new TimeSpan();
            string str = string.Empty;
            try
            {
                int startIndex = duration.ToString().LastIndexOf(':');
                TimeSpan.TryParseExact(duration.ToString().Remove(startIndex, 1).Insert(startIndex, "."), "mm\\:ss\\.ff", (IFormatProvider)CultureInfo.InvariantCulture, out result);
                timeSpan = new TimeSpan((long)Math.Round(1.0 * (double)result.Ticks / (double)num) * (long)num);
            }
            catch (Exception ex)
            {
            }
            return timeSpan.ToString("mm\\:ss");
        }

        private static string getSHNDuration(string FileName)
        {
            int num = (int)Math.Pow(10.0, (double)(7 - 0));
            TimeSpan result = new TimeSpan();
            TimeSpan timeSpan = new TimeSpan();
            string str = string.Empty;
            try
            {
                using (Process process = Process.Start(new ProcessStartInfo()
                {
                    FileName = Directory.GetCurrentDirectory() + "\\shntool.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "len \"" + FileName + "\""
                }))
                {
                    using (StreamReader standardOutput = process.StandardOutput)
                    {
                        List<string> list = Enumerable.ToList<string>((IEnumerable<string>)standardOutput.ReadToEnd().Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        for (int index = 1; index < list.Count - 1; ++index)
                        {
                            string[] separator = new string[1] { " " };
                            TimeSpan.TryParseExact(Enumerable.ToList<string>((IEnumerable<string>)list[index].Split(separator, StringSplitOptions.RemoveEmptyEntries))[0].PadLeft(8, '0'), "mm\\:ss\\.ff", (IFormatProvider)CultureInfo.InvariantCulture, out result);
                            timeSpan = new TimeSpan((long)Math.Round(1.0 * (double)result.Ticks / (double)num) * (long)num);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return timeSpan.ToString("mm\\:ss");
        }
    }
}
