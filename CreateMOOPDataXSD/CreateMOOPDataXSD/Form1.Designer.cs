namespace CreateMOOPDataXSD
{
  partial class Form1
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
      this.label1 = new System.Windows.Forms.Label();
      this.txtSQLConnection = new System.Windows.Forms.TextBox();
      this.txtAzureAccount = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.txtAzureSharedKey = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.cmbStoredProcedures = new System.Windows.Forms.ComboBox();
      this.btnGetSprocs = new System.Windows.Forms.Button();
      this.label4 = new System.Windows.Forms.Label();
      this.txtXSD = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.txtXMLFileName = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.btnGenerateXML = new System.Windows.Forms.Button();
      this.btnSaveXML = new System.Windows.Forms.Button();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.txtConnectioName = new System.Windows.Forms.TextBox();
      this.label7 = new System.Windows.Forms.Label();
      this.chkPost = new System.Windows.Forms.CheckBox();
      this.btnGenerateAll = new System.Windows.Forms.Button();
      this.label8 = new System.Windows.Forms.Label();
      this.cmbCacheability = new System.Windows.Forms.ComboBox();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.statusStrip1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(18, 29);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(88, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "SQL Connection:";
      // 
      // txtSQLConnection
      // 
      this.txtSQLConnection.Location = new System.Drawing.Point(112, 26);
      this.txtSQLConnection.Name = "txtSQLConnection";
      this.txtSQLConnection.Size = new System.Drawing.Size(661, 20);
      this.txtSQLConnection.TabIndex = 1;
      // 
      // txtAzureAccount
      // 
      this.txtAzureAccount.Location = new System.Drawing.Point(112, 52);
      this.txtAzureAccount.Name = "txtAzureAccount";
      this.txtAzureAccount.Size = new System.Drawing.Size(661, 20);
      this.txtAzureAccount.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(29, 55);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(77, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Azure Account";
      // 
      // txtAzureSharedKey
      // 
      this.txtAzureSharedKey.Location = new System.Drawing.Point(112, 78);
      this.txtAzureSharedKey.Name = "txtAzureSharedKey";
      this.txtAzureSharedKey.Size = new System.Drawing.Size(661, 20);
      this.txtAzureSharedKey.TabIndex = 5;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(18, 81);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(92, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Azure Shared Key";
      // 
      // cmbStoredProcedures
      // 
      this.cmbStoredProcedures.FormattingEnabled = true;
      this.cmbStoredProcedures.Location = new System.Drawing.Point(107, 34);
      this.cmbStoredProcedures.Name = "cmbStoredProcedures";
      this.cmbStoredProcedures.Size = new System.Drawing.Size(568, 21);
      this.cmbStoredProcedures.TabIndex = 6;
      // 
      // btnGetSprocs
      // 
      this.btnGetSprocs.Location = new System.Drawing.Point(693, 32);
      this.btnGetSprocs.Name = "btnGetSprocs";
      this.btnGetSprocs.Size = new System.Drawing.Size(75, 23);
      this.btnGetSprocs.TabIndex = 7;
      this.btnGetSprocs.Text = "Get Sprocs";
      this.btnGetSprocs.UseVisualStyleBackColor = true;
      this.btnGetSprocs.Click += new System.EventHandler(this.btnGetSprocs_Click);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(3, 37);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(98, 13);
      this.label4.TabIndex = 8;
      this.label4.Text = "Stored Procedures:";
      // 
      // txtXSD
      // 
      this.txtXSD.Location = new System.Drawing.Point(107, 87);
      this.txtXSD.Multiline = true;
      this.txtXSD.Name = "txtXSD";
      this.txtXSD.Size = new System.Drawing.Size(837, 451);
      this.txtXSD.TabIndex = 9;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(7, 87);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(98, 13);
      this.label5.TabIndex = 10;
      this.label5.Text = "Stored Procedures:";
      // 
      // txtXMLFileName
      // 
      this.txtXMLFileName.Location = new System.Drawing.Point(107, 61);
      this.txtXMLFileName.Name = "txtXMLFileName";
      this.txtXMLFileName.ReadOnly = true;
      this.txtXMLFileName.Size = new System.Drawing.Size(568, 20);
      this.txtXMLFileName.TabIndex = 12;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(13, 64);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(82, 13);
      this.label6.TabIndex = 11;
      this.label6.Text = "XML File Name:";
      // 
      // btnGenerateXML
      // 
      this.btnGenerateXML.Location = new System.Drawing.Point(405, 3);
      this.btnGenerateXML.Name = "btnGenerateXML";
      this.btnGenerateXML.Size = new System.Drawing.Size(75, 23);
      this.btnGenerateXML.TabIndex = 14;
      this.btnGenerateXML.Text = "Generate";
      this.btnGenerateXML.UseVisualStyleBackColor = true;
      this.btnGenerateXML.Click += new System.EventHandler(this.btnGenerateXML_Click);
      // 
      // btnSaveXML
      // 
      this.btnSaveXML.Location = new System.Drawing.Point(693, 58);
      this.btnSaveXML.Name = "btnSaveXML";
      this.btnSaveXML.Size = new System.Drawing.Size(75, 23);
      this.btnSaveXML.TabIndex = 15;
      this.btnSaveXML.Text = "Save";
      this.btnSaveXML.UseVisualStyleBackColor = true;
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
      this.statusStrip1.Location = new System.Drawing.Point(0, 585);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(988, 22);
      this.statusStrip1.TabIndex = 16;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(38, 17);
      this.toolStripStatusLabel1.Text = "Ready";
      // 
      // txtConnectioName
      // 
      this.txtConnectioName.Location = new System.Drawing.Point(112, 104);
      this.txtConnectioName.Name = "txtConnectioName";
      this.txtConnectioName.Size = new System.Drawing.Size(661, 20);
      this.txtConnectioName.TabIndex = 18;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(18, 107);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(95, 13);
      this.label7.TabIndex = 17;
      this.label7.Text = "Connection Name:";
      // 
      // chkPost
      // 
      this.chkPost.AutoSize = true;
      this.chkPost.Location = new System.Drawing.Point(330, 8);
      this.chkPost.Name = "chkPost";
      this.chkPost.Size = new System.Drawing.Size(69, 17);
      this.chkPost.TabIndex = 19;
      this.chkPost.Text = "Use Post";
      this.chkPost.UseVisualStyleBackColor = true;
      // 
      // btnGenerateAll
      // 
      this.btnGenerateAll.Location = new System.Drawing.Point(497, 2);
      this.btnGenerateAll.Name = "btnGenerateAll";
      this.btnGenerateAll.Size = new System.Drawing.Size(75, 23);
      this.btnGenerateAll.TabIndex = 20;
      this.btnGenerateAll.Text = "Generate All";
      this.btnGenerateAll.UseVisualStyleBackColor = true;
      this.btnGenerateAll.Click += new System.EventHandler(this.btnGenerateAll_Click);
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(3, 9);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(64, 13);
      this.label8.TabIndex = 22;
      this.label8.Text = "Cacheability";
      // 
      // cmbCacheability
      // 
      this.cmbCacheability.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
      this.cmbCacheability.FormattingEnabled = true;
      this.cmbCacheability.Items.AddRange(new object[] {
            "nocache",
            "private",
            "public",
            "server",
            "serverandnocache",
            "serverandprivate"});
      this.cmbCacheability.Location = new System.Drawing.Point(107, 6);
      this.cmbCacheability.Name = "cmbCacheability";
      this.cmbCacheability.Size = new System.Drawing.Size(200, 21);
      this.cmbCacheability.TabIndex = 21;
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Location = new System.Drawing.Point(0, 1);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(988, 581);
      this.tabControl1.TabIndex = 23;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.cmbCacheability);
      this.tabPage1.Controls.Add(this.label8);
      this.tabPage1.Controls.Add(this.cmbStoredProcedures);
      this.tabPage1.Controls.Add(this.btnGetSprocs);
      this.tabPage1.Controls.Add(this.btnGenerateAll);
      this.tabPage1.Controls.Add(this.label4);
      this.tabPage1.Controls.Add(this.chkPost);
      this.tabPage1.Controls.Add(this.txtXSD);
      this.tabPage1.Controls.Add(this.label5);
      this.tabPage1.Controls.Add(this.btnSaveXML);
      this.tabPage1.Controls.Add(this.label6);
      this.tabPage1.Controls.Add(this.btnGenerateXML);
      this.tabPage1.Controls.Add(this.txtXMLFileName);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(980, 555);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Main";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.label1);
      this.tabPage2.Controls.Add(this.txtSQLConnection);
      this.tabPage2.Controls.Add(this.label2);
      this.tabPage2.Controls.Add(this.txtAzureAccount);
      this.tabPage2.Controls.Add(this.label3);
      this.tabPage2.Controls.Add(this.txtConnectioName);
      this.tabPage2.Controls.Add(this.txtAzureSharedKey);
      this.tabPage2.Controls.Add(this.label7);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(968, 544);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Config";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(988, 607);
      this.Controls.Add(this.tabControl1);
      this.Controls.Add(this.statusStrip1);
      this.Name = "Form1";
      this.Text = "Create MOOP Data XSD";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
      this.Load += new System.EventHandler(this.Form1_Load);
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.tabPage2.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txtSQLConnection;
    private System.Windows.Forms.TextBox txtAzureAccount;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox txtAzureSharedKey;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox cmbStoredProcedures;
    private System.Windows.Forms.Button btnGetSprocs;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox txtXSD;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox txtXMLFileName;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Button btnGenerateXML;
    private System.Windows.Forms.Button btnSaveXML;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.TextBox txtConnectioName;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.CheckBox chkPost;
    private System.Windows.Forms.Button btnGenerateAll;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.ComboBox cmbCacheability;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
  }
}

