using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Collections;

namespace SagaProcessor
{
 public partial class Form1 : Form
 {
  public Form1()
  {
   InitializeComponent();
  }

  ATSSagaProcessor asp = new ATSSagaProcessor("finseldemos", "", "EszaEnqThvqlMyBkct9ELzGSfm30r6l6q890XoxsN+gezNVFZNGhmlaXGmdrhFlV2870N0yMoFOgi8MbQGEEmQ==", "SharedKey");
  public void siNotification(string Notification)
  {
   if (txtNotification.Text.Length > 4096)
    txtNotification.Text = "";
   txtNotification.Text = Notification + " " + DateTime.Now.ToLongTimeString() + "\r\n" + txtNotification.Text;
   Application.DoEvents();
  }

  private void btnProcess_Click(object sender, EventArgs e)
  {
   asp.GetRequests();
  }

  private void Form1_Load(object sender, EventArgs e)
  {
   asp.OnLogHandler(siNotification);
  }

  private void button1_Click(object sender, EventArgs e)
  {
   string queueInitialCreationEntity = "<m:properties>\n\t<d:PartitionKey>{0}</d:PartitionKey>\n\t<d:RowKey>_Summary</d:RowKey><d:Status>Pending</d:Status>\n\t</m:properties>";
   string newID = Guid.NewGuid().ToString();
   System.Text.StringBuilder sb = new StringBuilder();
   Random r = new Random();
   string tmpTable =string.Format("newtable{0:d5}",r.Next());
   sb.AppendFormat("<message><updateID>{0}</updateID><toAccount>{1}</toAccount><toKey>{2}</toKey><toTable>{3}</toTable>",
   newID, "finseldemos", "EszaEnqThvqlMyBkct9ELzGSfm30r6l6q890XoxsN+gezNVFZNGhmlaXGmdrhFlV2870N0yMoFOgi8MbQGEEmQ==", tmpTable);
   sb.AppendFormat("<monitorAccount>{0}</monitorAccount><monitorKey>{1}</monitorKey><monitorTable>{2}</monitorTable>",
   "finseldemos", "EszaEnqThvqlMyBkct9ELzGSfm30r6l6q890XoxsN+gezNVFZNGhmlaXGmdrhFlV2870N0yMoFOgi8MbQGEEmQ==", "codestockservices");
   sb.AppendFormat("<sourceAccount>{0}</sourceAccount><sourceKey>{1}</sourceKey><sourceContainer>{2}</sourceContainer><sourceBlob>{3}</sourceBlob>",
      "finseldemos", "EszaEnqThvqlMyBkct9ELzGSfm30r6l6q890XoxsN+gezNVFZNGhmlaXGmdrhFlV2870N0yMoFOgi8MbQGEEmQ==", "public", "CalendarDemo.xml");

   sb.AppendFormat("<source>http://finseldemos.blob.core.windows.net/public/CalendarDemo.xml</source></message>");
   string queueMessage = sb.ToString();
   Finsel.AzureCommands.AzureTableStorage ats = new Finsel.AzureCommands.AzureTableStorage("finseldemos", "", "EszaEnqThvqlMyBkct9ELzGSfm30r6l6q890XoxsN+gezNVFZNGhmlaXGmdrhFlV2870N0yMoFOgi8MbQGEEmQ==", "SharedKey");
   Finsel.AzureCommands.azureResults ar = ats.Tables(Finsel.AzureCommands.cmdType.post, "codestockservices");
   ar = ats.Entities(Finsel.AzureCommands.cmdType.post, "codestockservices", newID, "_Summary", string.Format(queueInitialCreationEntity, newID), "", "");
   Finsel.AzureCommands.AzureQueueStorage aqs = new Finsel.AzureCommands.AzureQueueStorage("finseldemos", "", "EszaEnqThvqlMyBkct9ELzGSfm30r6l6q890XoxsN+gezNVFZNGhmlaXGmdrhFlV2870N0yMoFOgi8MbQGEEmQ==", "SharedKey");
   ar = aqs.Queues(Finsel.AzureCommands.cmdType.put, "codestockservices", null, new Hashtable());
   ar = aqs.Messages(Finsel.AzureCommands.cmdType.post, "codestockservices", queueMessage, null, null);

  }

 }
}