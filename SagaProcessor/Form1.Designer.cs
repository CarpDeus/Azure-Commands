namespace SagaProcessor
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
   this.groupBox1 = new System.Windows.Forms.GroupBox();
   this.txtNotification = new System.Windows.Forms.TextBox();
   this.btnProcess = new System.Windows.Forms.Button();
   this.button1 = new System.Windows.Forms.Button();
   this.groupBox1.SuspendLayout();
   this.SuspendLayout();
   // 
   // groupBox1
   // 
   this.groupBox1.Controls.Add(this.txtNotification);
   this.groupBox1.Location = new System.Drawing.Point(12, 49);
   this.groupBox1.Name = "groupBox1";
   this.groupBox1.Size = new System.Drawing.Size(596, 242);
   this.groupBox1.TabIndex = 0;
   this.groupBox1.TabStop = false;
   this.groupBox1.Text = "Notifications";
   // 
   // txtNotification
   // 
   this.txtNotification.Location = new System.Drawing.Point(16, 12);
   this.txtNotification.Multiline = true;
   this.txtNotification.Name = "txtNotification";
   this.txtNotification.ScrollBars = System.Windows.Forms.ScrollBars.Both;
   this.txtNotification.Size = new System.Drawing.Size(574, 224);
   this.txtNotification.TabIndex = 0;
   // 
   // btnProcess
   // 
   this.btnProcess.Location = new System.Drawing.Point(12, 12);
   this.btnProcess.Name = "btnProcess";
   this.btnProcess.Size = new System.Drawing.Size(110, 23);
   this.btnProcess.TabIndex = 1;
   this.btnProcess.Text = "Process Saga";
   this.btnProcess.UseVisualStyleBackColor = true;
   this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
   // 
   // button1
   // 
   this.button1.Location = new System.Drawing.Point(147, 12);
   this.button1.Name = "button1";
   this.button1.Size = new System.Drawing.Size(110, 23);
   this.button1.TabIndex = 2;
   this.button1.Text = "Copy Data";
   this.button1.UseVisualStyleBackColor = true;
   this.button1.Click += new System.EventHandler(this.button1_Click);
   // 
   // Form1
   // 
   this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
   this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
   this.ClientSize = new System.Drawing.Size(620, 303);
   this.Controls.Add(this.button1);
   this.Controls.Add(this.btnProcess);
   this.Controls.Add(this.groupBox1);
   this.Name = "Form1";
   this.Text = "Form1";
   this.Load += new System.EventHandler(this.Form1_Load);
   this.groupBox1.ResumeLayout(false);
   this.groupBox1.PerformLayout();
   this.ResumeLayout(false);

  }

  #endregion

  private System.Windows.Forms.GroupBox groupBox1;
  private System.Windows.Forms.TextBox txtNotification;
  private System.Windows.Forms.Button btnProcess;
  private System.Windows.Forms.Button button1;
 }
}

