namespace getMyTracksMD5sWin
{
    partial class getMyMD5sWin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(getMyMD5sWin));
            this.tctlGetMyMD5s = new System.Windows.Forms.TabControl();
            this.tabPageGetMyMD5s = new System.Windows.Forms.TabPage();
            this.btnReportDuplicates = new System.Windows.Forms.Button();
            this.lblWAVCount = new System.Windows.Forms.Label();
            this.lblSHNCount = new System.Windows.Forms.Label();
            this.lblFLACCount = new System.Windows.Forms.Label();
            this.btnGetMD5s = new System.Windows.Forms.Button();
            this.lblGetMD5sFolderPath = new System.Windows.Forms.Label();
            this.btnGetMD5sSelectFolder = new System.Windows.Forms.Button();
            this.lblGetMD5sResults = new System.Windows.Forms.Label();
            this.txtGetMD5sErrors = new System.Windows.Forms.TextBox();
            this.lblGetMD5sErrors = new System.Windows.Forms.Label();
            this.txtGetMD5sResults = new System.Windows.Forms.TextBox();
            this.tabPgSeedsNeededOnDt = new System.Windows.Forms.TabPage();
            this.lblCurrentOne = new System.Windows.Forms.Label();
            this.chkOnlyShowMine = new System.Windows.Forms.CheckBox();
            this.dgvSeedsNeededOnDT = new System.Windows.Forms.DataGridView();
            this.btnGetSeedsNeededOnDT = new System.Windows.Forms.Button();
            this.proBarGetMD5sFilesFound = new System.Windows.Forms.ProgressBar();
            this.txtMySecret = new System.Windows.Forms.TextBox();
            this.lblMySecret = new System.Windows.Forms.Label();
            this.txtGetMD5sFolderMask = new System.Windows.Forms.TextBox();
            this.txtGetMD5sFileMask = new System.Windows.Forms.TextBox();
            this.tctlGetMyMD5s.SuspendLayout();
            this.tabPageGetMyMD5s.SuspendLayout();
            this.tabPgSeedsNeededOnDt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSeedsNeededOnDT)).BeginInit();
            this.SuspendLayout();
            // 
            // tctlGetMyMD5s
            // 
            this.tctlGetMyMD5s.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tctlGetMyMD5s.Controls.Add(this.tabPageGetMyMD5s);
            this.tctlGetMyMD5s.Controls.Add(this.tabPgSeedsNeededOnDt);
            this.tctlGetMyMD5s.Location = new System.Drawing.Point(-3, 61);
            this.tctlGetMyMD5s.Name = "tctlGetMyMD5s";
            this.tctlGetMyMD5s.SelectedIndex = 0;
            this.tctlGetMyMD5s.Size = new System.Drawing.Size(986, 500);
            this.tctlGetMyMD5s.TabIndex = 0;
            // 
            // tabPageGetMyMD5s
            // 
            this.tabPageGetMyMD5s.Controls.Add(this.btnReportDuplicates);
            this.tabPageGetMyMD5s.Controls.Add(this.lblWAVCount);
            this.tabPageGetMyMD5s.Controls.Add(this.lblSHNCount);
            this.tabPageGetMyMD5s.Controls.Add(this.lblFLACCount);
            this.tabPageGetMyMD5s.Controls.Add(this.btnGetMD5s);
            this.tabPageGetMyMD5s.Controls.Add(this.lblGetMD5sFolderPath);
            this.tabPageGetMyMD5s.Controls.Add(this.btnGetMD5sSelectFolder);
            this.tabPageGetMyMD5s.Controls.Add(this.lblGetMD5sResults);
            this.tabPageGetMyMD5s.Controls.Add(this.txtGetMD5sErrors);
            this.tabPageGetMyMD5s.Controls.Add(this.lblGetMD5sErrors);
            this.tabPageGetMyMD5s.Controls.Add(this.txtGetMD5sResults);
            this.tabPageGetMyMD5s.Location = new System.Drawing.Point(4, 22);
            this.tabPageGetMyMD5s.Name = "tabPageGetMyMD5s";
            this.tabPageGetMyMD5s.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGetMyMD5s.Size = new System.Drawing.Size(978, 474);
            this.tabPageGetMyMD5s.TabIndex = 0;
            this.tabPageGetMyMD5s.Text = "Get My MD5s";
            this.tabPageGetMyMD5s.UseVisualStyleBackColor = true;
            // 
            // btnReportDuplicates
            // 
            this.btnReportDuplicates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReportDuplicates.Location = new System.Drawing.Point(765, 445);
            this.btnReportDuplicates.Name = "btnReportDuplicates";
            this.btnReportDuplicates.Size = new System.Drawing.Size(100, 23);
            this.btnReportDuplicates.TabIndex = 18;
            this.btnReportDuplicates.Text = "&Get Duplicates";
            this.btnReportDuplicates.UseVisualStyleBackColor = true;
            this.btnReportDuplicates.Visible = false;
            // 
            // lblWAVCount
            // 
            this.lblWAVCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWAVCount.AutoSize = true;
            this.lblWAVCount.Location = new System.Drawing.Point(867, 3);
            this.lblWAVCount.Name = "lblWAVCount";
            this.lblWAVCount.Size = new System.Drawing.Size(75, 13);
            this.lblWAVCount.TabIndex = 17;
            this.lblWAVCount.Text = "WAV Count: 0";
            // 
            // lblSHNCount
            // 
            this.lblSHNCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSHNCount.AutoSize = true;
            this.lblSHNCount.Location = new System.Drawing.Point(766, 3);
            this.lblSHNCount.Name = "lblSHNCount";
            this.lblSHNCount.Size = new System.Drawing.Size(73, 13);
            this.lblSHNCount.TabIndex = 16;
            this.lblSHNCount.Text = "SHN Count: 0";
            // 
            // lblFLACCount
            // 
            this.lblFLACCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFLACCount.AutoSize = true;
            this.lblFLACCount.Location = new System.Drawing.Point(659, 3);
            this.lblFLACCount.Name = "lblFLACCount";
            this.lblFLACCount.Size = new System.Drawing.Size(76, 13);
            this.lblFLACCount.TabIndex = 15;
            this.lblFLACCount.Text = "FLAC Count: 0";
            // 
            // btnGetMD5s
            // 
            this.btnGetMD5s.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetMD5s.Location = new System.Drawing.Point(871, 445);
            this.btnGetMD5s.Name = "btnGetMD5s";
            this.btnGetMD5s.Size = new System.Drawing.Size(100, 23);
            this.btnGetMD5s.TabIndex = 14;
            this.btnGetMD5s.Text = "S&earch";
            this.btnGetMD5s.UseVisualStyleBackColor = true;
            this.btnGetMD5s.Click += new System.EventHandler(this.btnGetMD5s_Click);
            // 
            // lblGetMD5sFolderPath
            // 
            this.lblGetMD5sFolderPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblGetMD5sFolderPath.AutoSize = true;
            this.lblGetMD5sFolderPath.Location = new System.Drawing.Point(109, 450);
            this.lblGetMD5sFolderPath.Name = "lblGetMD5sFolderPath";
            this.lblGetMD5sFolderPath.Size = new System.Drawing.Size(22, 13);
            this.lblGetMD5sFolderPath.TabIndex = 12;
            this.lblGetMD5sFolderPath.Text = "C:\\";
            // 
            // btnGetMD5sSelectFolder
            // 
            this.btnGetMD5sSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGetMD5sSelectFolder.Location = new System.Drawing.Point(3, 445);
            this.btnGetMD5sSelectFolder.Name = "btnGetMD5sSelectFolder";
            this.btnGetMD5sSelectFolder.Size = new System.Drawing.Size(100, 23);
            this.btnGetMD5sSelectFolder.TabIndex = 13;
            this.btnGetMD5sSelectFolder.Text = "&Select Folder";
            this.btnGetMD5sSelectFolder.UseVisualStyleBackColor = true;
            this.btnGetMD5sSelectFolder.Click += new System.EventHandler(this.btnGetMD5sSelectFolder_Click);
            // 
            // lblGetMD5sResults
            // 
            this.lblGetMD5sResults.AutoSize = true;
            this.lblGetMD5sResults.Location = new System.Drawing.Point(3, 3);
            this.lblGetMD5sResults.Name = "lblGetMD5sResults";
            this.lblGetMD5sResults.Size = new System.Drawing.Size(42, 13);
            this.lblGetMD5sResults.TabIndex = 9;
            this.lblGetMD5sResults.Text = "Results";
            // 
            // txtGetMD5sErrors
            // 
            this.txtGetMD5sErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGetMD5sErrors.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGetMD5sErrors.Location = new System.Drawing.Point(6, 248);
            this.txtGetMD5sErrors.Multiline = true;
            this.txtGetMD5sErrors.Name = "txtGetMD5sErrors";
            this.txtGetMD5sErrors.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtGetMD5sErrors.Size = new System.Drawing.Size(972, 176);
            this.txtGetMD5sErrors.TabIndex = 11;
            // 
            // lblGetMD5sErrors
            // 
            this.lblGetMD5sErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblGetMD5sErrors.AutoSize = true;
            this.lblGetMD5sErrors.Location = new System.Drawing.Point(11, 232);
            this.lblGetMD5sErrors.Name = "lblGetMD5sErrors";
            this.lblGetMD5sErrors.Size = new System.Drawing.Size(34, 13);
            this.lblGetMD5sErrors.TabIndex = 8;
            this.lblGetMD5sErrors.Text = "Errors";
            // 
            // txtGetMD5sResults
            // 
            this.txtGetMD5sResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGetMD5sResults.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGetMD5sResults.Location = new System.Drawing.Point(3, 16);
            this.txtGetMD5sResults.Margin = new System.Windows.Forms.Padding(0);
            this.txtGetMD5sResults.Multiline = true;
            this.txtGetMD5sResults.Name = "txtGetMD5sResults";
            this.txtGetMD5sResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtGetMD5sResults.Size = new System.Drawing.Size(975, 206);
            this.txtGetMD5sResults.TabIndex = 10;
            this.txtGetMD5sResults.WordWrap = false;
            // 
            // tabPgSeedsNeededOnDt
            // 
            this.tabPgSeedsNeededOnDt.Controls.Add(this.lblCurrentOne);
            this.tabPgSeedsNeededOnDt.Controls.Add(this.chkOnlyShowMine);
            this.tabPgSeedsNeededOnDt.Controls.Add(this.dgvSeedsNeededOnDT);
            this.tabPgSeedsNeededOnDt.Controls.Add(this.btnGetSeedsNeededOnDT);
            this.tabPgSeedsNeededOnDt.Location = new System.Drawing.Point(4, 22);
            this.tabPgSeedsNeededOnDt.Name = "tabPgSeedsNeededOnDt";
            this.tabPgSeedsNeededOnDt.Padding = new System.Windows.Forms.Padding(3);
            this.tabPgSeedsNeededOnDt.Size = new System.Drawing.Size(978, 474);
            this.tabPgSeedsNeededOnDt.TabIndex = 1;
            this.tabPgSeedsNeededOnDt.Text = "Seeds Needed On DT";
            this.tabPgSeedsNeededOnDt.UseVisualStyleBackColor = true;
            // 
            // lblCurrentOne
            // 
            this.lblCurrentOne.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCurrentOne.AutoSize = true;
            this.lblCurrentOne.Location = new System.Drawing.Point(7, 448);
            this.lblCurrentOne.Name = "lblCurrentOne";
            this.lblCurrentOne.Size = new System.Drawing.Size(0, 13);
            this.lblCurrentOne.TabIndex = 3;
            // 
            // chkOnlyShowMine
            // 
            this.chkOnlyShowMine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkOnlyShowMine.AutoSize = true;
            this.chkOnlyShowMine.Location = new System.Drawing.Point(754, 448);
            this.chkOnlyShowMine.Name = "chkOnlyShowMine";
            this.chkOnlyShowMine.Size = new System.Drawing.Size(136, 17);
            this.chkOnlyShowMine.TabIndex = 2;
            this.chkOnlyShowMine.Text = "Only Show My Sources";
            this.chkOnlyShowMine.UseVisualStyleBackColor = true;
            this.chkOnlyShowMine.CheckedChanged += new System.EventHandler(this.chkOnlyShowMine_CheckedChanged);
            // 
            // dgvSeedsNeededOnDT
            // 
            this.dgvSeedsNeededOnDT.AllowUserToAddRows = false;
            this.dgvSeedsNeededOnDT.AllowUserToDeleteRows = false;
            this.dgvSeedsNeededOnDT.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSeedsNeededOnDT.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvSeedsNeededOnDT.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSeedsNeededOnDT.Location = new System.Drawing.Point(3, 3);
            this.dgvSeedsNeededOnDT.Name = "dgvSeedsNeededOnDT";
            this.dgvSeedsNeededOnDT.Size = new System.Drawing.Size(972, 435);
            this.dgvSeedsNeededOnDT.TabIndex = 1;
            this.dgvSeedsNeededOnDT.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSeedsNeededOnDT_CellContentClick);
            // 
            // btnGetSeedsNeededOnDT
            // 
            this.btnGetSeedsNeededOnDT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetSeedsNeededOnDT.Location = new System.Drawing.Point(896, 444);
            this.btnGetSeedsNeededOnDT.Name = "btnGetSeedsNeededOnDT";
            this.btnGetSeedsNeededOnDT.Size = new System.Drawing.Size(75, 23);
            this.btnGetSeedsNeededOnDT.TabIndex = 0;
            this.btnGetSeedsNeededOnDT.Text = "&Search";
            this.btnGetSeedsNeededOnDT.UseVisualStyleBackColor = true;
            this.btnGetSeedsNeededOnDT.Click += new System.EventHandler(this.btnGetSeedsNeededOnDT_Click);
            // 
            // proBarGetMD5sFilesFound
            // 
            this.proBarGetMD5sFilesFound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.proBarGetMD5sFilesFound.Location = new System.Drawing.Point(1, 32);
            this.proBarGetMD5sFilesFound.Name = "proBarGetMD5sFilesFound";
            this.proBarGetMD5sFilesFound.Size = new System.Drawing.Size(982, 23);
            this.proBarGetMD5sFilesFound.TabIndex = 1;
            // 
            // txtMySecret
            // 
            this.txtMySecret.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMySecret.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMySecret.Location = new System.Drawing.Point(727, 6);
            this.txtMySecret.MaxLength = 32;
            this.txtMySecret.Name = "txtMySecret";
            this.txtMySecret.Size = new System.Drawing.Size(245, 20);
            this.txtMySecret.TabIndex = 2;
            this.txtMySecret.Text = "abcdefg1234567890abcdefg1234567890";
            // 
            // lblMySecret
            // 
            this.lblMySecret.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMySecret.AutoSize = true;
            this.lblMySecret.Location = new System.Drawing.Point(660, 9);
            this.lblMySecret.Name = "lblMySecret";
            this.lblMySecret.Size = new System.Drawing.Size(61, 13);
            this.lblMySecret.TabIndex = 3;
            this.lblMySecret.Text = "My Secret :";
            // 
            // txtGetMD5sFolderMask
            // 
            this.txtGetMD5sFolderMask.Location = new System.Drawing.Point(188, 6);
            this.txtGetMD5sFolderMask.Name = "txtGetMD5sFolderMask";
            this.txtGetMD5sFolderMask.Size = new System.Drawing.Size(100, 20);
            this.txtGetMD5sFolderMask.TabIndex = 4;
            this.txtGetMD5sFolderMask.Visible = false;
            // 
            // txtGetMD5sFileMask
            // 
            this.txtGetMD5sFileMask.Location = new System.Drawing.Point(59, 6);
            this.txtGetMD5sFileMask.Name = "txtGetMD5sFileMask";
            this.txtGetMD5sFileMask.Size = new System.Drawing.Size(100, 20);
            this.txtGetMD5sFileMask.TabIndex = 5;
            this.txtGetMD5sFileMask.Visible = false;
            // 
            // getMyMD5sWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 562);
            this.Controls.Add(this.txtGetMD5sFileMask);
            this.Controls.Add(this.txtGetMD5sFolderMask);
            this.Controls.Add(this.lblMySecret);
            this.Controls.Add(this.txtMySecret);
            this.Controls.Add(this.proBarGetMD5sFilesFound);
            this.Controls.Add(this.tctlGetMyMD5s);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "getMyMD5sWin";
            this.Text = "DMB Concerts";
            this.Load += new System.EventHandler(this.getMyMD5sWin_Load);
            this.tctlGetMyMD5s.ResumeLayout(false);
            this.tabPageGetMyMD5s.ResumeLayout(false);
            this.tabPageGetMyMD5s.PerformLayout();
            this.tabPgSeedsNeededOnDt.ResumeLayout(false);
            this.tabPgSeedsNeededOnDt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSeedsNeededOnDT)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tctlGetMyMD5s;
        private System.Windows.Forms.TabPage tabPageGetMyMD5s;
        private System.Windows.Forms.Label lblGetMD5sResults;
        private System.Windows.Forms.TextBox txtGetMD5sErrors;
        private System.Windows.Forms.Label lblGetMD5sErrors;
        private System.Windows.Forms.TextBox txtGetMD5sResults;
        private System.Windows.Forms.TabPage tabPgSeedsNeededOnDt;
        private System.Windows.Forms.ProgressBar proBarGetMD5sFilesFound;
        private System.Windows.Forms.TextBox txtMySecret;
        private System.Windows.Forms.Button btnGetMD5s;
        private System.Windows.Forms.Label lblGetMD5sFolderPath;
        private System.Windows.Forms.Button btnGetMD5sSelectFolder;
        private System.Windows.Forms.Label lblMySecret;
        private System.Windows.Forms.TextBox txtGetMD5sFolderMask;
        private System.Windows.Forms.TextBox txtGetMD5sFileMask;
        private System.Windows.Forms.Label lblWAVCount;
        private System.Windows.Forms.Label lblSHNCount;
        private System.Windows.Forms.Label lblFLACCount;
        private System.Windows.Forms.Button btnReportDuplicates;
        private System.Windows.Forms.DataGridView dgvSeedsNeededOnDT;
        private System.Windows.Forms.Button btnGetSeedsNeededOnDT;
        private System.Windows.Forms.CheckBox chkOnlyShowMine;
        private System.Windows.Forms.Label lblCurrentOne;
    }
}