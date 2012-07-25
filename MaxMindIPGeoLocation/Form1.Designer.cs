namespace MaxMindIPGeoLocation
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
   this.txtBlocksFile = new System.Windows.Forms.TextBox();
   this.btnFindBlocks = new System.Windows.Forms.Button();
   this.btnFindCity = new System.Windows.Forms.Button();
   this.txtCityFile = new System.Windows.Forms.TextBox();
   this.label2 = new System.Windows.Forms.Label();
   this.btnLoad = new System.Windows.Forms.Button();
   this.statusStrip1 = new System.Windows.Forms.StatusStrip();
   this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
   this.txtSharedKey = new System.Windows.Forms.TextBox();
   this.label3 = new System.Windows.Forms.Label();
   this.txtEndpoint = new System.Windows.Forms.TextBox();
   this.label4 = new System.Windows.Forms.Label();
   this.txtAccount = new System.Windows.Forms.TextBox();
   this.label5 = new System.Windows.Forms.Label();
   this.statusStrip1.SuspendLayout();
   this.SuspendLayout();
   // 
   // label1
   // 
   this.label1.AutoSize = true;
   this.label1.Location = new System.Drawing.Point(20, 93);
   this.label1.Name = "label1";
   this.label1.Size = new System.Drawing.Size(61, 13);
   this.label1.TabIndex = 0;
   this.label1.Text = "Blocks File:";
   // 
   // txtBlocksFile
   // 
   this.txtBlocksFile.Location = new System.Drawing.Point(87, 90);
   this.txtBlocksFile.Name = "txtBlocksFile";
   this.txtBlocksFile.Size = new System.Drawing.Size(586, 20);
   this.txtBlocksFile.TabIndex = 1;
   // 
   // btnFindBlocks
   // 
   this.btnFindBlocks.Location = new System.Drawing.Point(695, 90);
   this.btnFindBlocks.Name = "btnFindBlocks";
   this.btnFindBlocks.Size = new System.Drawing.Size(75, 23);
   this.btnFindBlocks.TabIndex = 2;
   this.btnFindBlocks.Text = "Find &Blocks";
   this.btnFindBlocks.UseVisualStyleBackColor = true;
   this.btnFindBlocks.Click += new System.EventHandler(this.btnFindBlocks_Click);
   // 
   // btnFindCity
   // 
   this.btnFindCity.Location = new System.Drawing.Point(695, 119);
   this.btnFindCity.Name = "btnFindCity";
   this.btnFindCity.Size = new System.Drawing.Size(75, 23);
   this.btnFindCity.TabIndex = 5;
   this.btnFindCity.Text = "Find &City";
   this.btnFindCity.UseVisualStyleBackColor = true;
   this.btnFindCity.Click += new System.EventHandler(this.btnFindCity_Click);
   // 
   // txtCityFile
   // 
   this.txtCityFile.Location = new System.Drawing.Point(87, 119);
   this.txtCityFile.Name = "txtCityFile";
   this.txtCityFile.Size = new System.Drawing.Size(586, 20);
   this.txtCityFile.TabIndex = 4;
   // 
   // label2
   // 
   this.label2.AutoSize = true;
   this.label2.Location = new System.Drawing.Point(35, 122);
   this.label2.Name = "label2";
   this.label2.Size = new System.Drawing.Size(46, 13);
   this.label2.TabIndex = 3;
   this.label2.Text = "City File:";
   // 
   // btnLoad
   // 
   this.btnLoad.Location = new System.Drawing.Point(695, 148);
   this.btnLoad.Name = "btnLoad";
   this.btnLoad.Size = new System.Drawing.Size(75, 23);
   this.btnLoad.TabIndex = 6;
   this.btnLoad.Text = "&Load Data";
   this.btnLoad.UseVisualStyleBackColor = true;
   this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
   // 
   // statusStrip1
   // 
   this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
   this.statusStrip1.Location = new System.Drawing.Point(0, 178);
   this.statusStrip1.Name = "statusStrip1";
   this.statusStrip1.Size = new System.Drawing.Size(782, 22);
   this.statusStrip1.TabIndex = 7;
   this.statusStrip1.Text = "statusStrip1";
   // 
   // toolStripStatusLabel1
   // 
   this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
   this.toolStripStatusLabel1.Size = new System.Drawing.Size(38, 17);
   this.toolStripStatusLabel1.Text = "Ready";
   // 
   // txtSharedKey
   // 
   this.txtSharedKey.Location = new System.Drawing.Point(87, 64);
   this.txtSharedKey.Name = "txtSharedKey";
   this.txtSharedKey.Size = new System.Drawing.Size(683, 20);
   this.txtSharedKey.TabIndex = 13;
   // 
   // label3
   // 
   this.label3.AutoSize = true;
   this.label3.Location = new System.Drawing.Point(19, 64);
   this.label3.Name = "label3";
   this.label3.Size = new System.Drawing.Size(65, 13);
   this.label3.TabIndex = 12;
   this.label3.Text = "Shared Key:";
   // 
   // txtEndpoint
   // 
   this.txtEndpoint.Location = new System.Drawing.Point(87, 38);
   this.txtEndpoint.Name = "txtEndpoint";
   this.txtEndpoint.Size = new System.Drawing.Size(683, 20);
   this.txtEndpoint.TabIndex = 11;
   // 
   // label4
   // 
   this.label4.AutoSize = true;
   this.label4.Location = new System.Drawing.Point(31, 15);
   this.label4.Name = "label4";
   this.label4.Size = new System.Drawing.Size(50, 13);
   this.label4.TabIndex = 10;
   this.label4.Text = "Account:";
   // 
   // txtAccount
   // 
   this.txtAccount.Location = new System.Drawing.Point(87, 12);
   this.txtAccount.Name = "txtAccount";
   this.txtAccount.Size = new System.Drawing.Size(683, 20);
   this.txtAccount.TabIndex = 9;
   // 
   // label5
   // 
   this.label5.AutoSize = true;
   this.label5.Location = new System.Drawing.Point(29, 41);
   this.label5.Name = "label5";
   this.label5.Size = new System.Drawing.Size(52, 13);
   this.label5.TabIndex = 8;
   this.label5.Text = "Endpoint:";
   // 
   // Form1
   // 
   this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
   this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
   this.ClientSize = new System.Drawing.Size(782, 200);
   this.Controls.Add(this.txtSharedKey);
   this.Controls.Add(this.label3);
   this.Controls.Add(this.txtEndpoint);
   this.Controls.Add(this.label4);
   this.Controls.Add(this.txtAccount);
   this.Controls.Add(this.label5);
   this.Controls.Add(this.statusStrip1);
   this.Controls.Add(this.btnLoad);
   this.Controls.Add(this.btnFindCity);
   this.Controls.Add(this.txtCityFile);
   this.Controls.Add(this.label2);
   this.Controls.Add(this.btnFindBlocks);
   this.Controls.Add(this.txtBlocksFile);
   this.Controls.Add(this.label1);
   this.Name = "Form1";
   this.Text = "Form1";
   this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
   this.statusStrip1.ResumeLayout(false);
   this.statusStrip1.PerformLayout();
   this.ResumeLayout(false);
   this.PerformLayout();

  }

  #endregion

  private System.Windows.Forms.Label label1;
  private System.Windows.Forms.TextBox txtBlocksFile;
  private System.Windows.Forms.Button btnFindBlocks;
  private System.Windows.Forms.Button btnFindCity;
  private System.Windows.Forms.TextBox txtCityFile;
  private System.Windows.Forms.Label label2;
  private System.Windows.Forms.Button btnLoad;
  private System.Windows.Forms.StatusStrip statusStrip1;
  private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
  private System.Windows.Forms.TextBox txtSharedKey;
  private System.Windows.Forms.Label label3;
  private System.Windows.Forms.TextBox txtEndpoint;
  private System.Windows.Forms.Label label4;
  private System.Windows.Forms.TextBox txtAccount;
  private System.Windows.Forms.Label label5;
 }
}

