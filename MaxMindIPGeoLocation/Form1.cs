using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using Finsel.AzureCommands;
using System.Xml;


namespace MaxMindIPGeoLocation
{
 public partial class Form1 : Form
 {
  public Form1()
  {
   InitializeComponent();
   txtAccount.Text = MaxMindIPGeoLocation.Properties.Settings.Default.Account;
   txtEndpoint.Text = MaxMindIPGeoLocation.Properties.Settings.Default.Endpoint;
   txtSharedKey.Text = MaxMindIPGeoLocation.Properties.Settings.Default.SharedKey;
  }

  private void btnFindBlocks_Click(object sender, EventArgs e)
  {
   OpenFileDialog ofd = new OpenFileDialog();
   ofd.ShowDialog();
   txtBlocksFile.Text = ofd.FileName;
  }

  private void btnFindCity_Click(object sender, EventArgs e)
  {
   OpenFileDialog ofd = new OpenFileDialog();
   ofd.ShowDialog();
   txtCityFile.Text = ofd.FileName;
  }

  private void btnLoad_Click(object sender, EventArgs e)
  {
   Int64 iCounter = 0;

   string regexCSVSplit = string.Empty;
   regexCSVSplit = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
   regexCSVSplit = @"([^\\x22\\]*(:?:\\.[^\\x22\\]*)*)\\x22,?|([^,]+),?|,";
   System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(regexCSVSplit);
   string tableName = "MaxMindData";
   string cityPartition = "Cities";
   string blockPartition = "IPBlocks";
   this.Cursor = Cursors.WaitCursor;
   System.IO.StreamReader file =   new System.IO.StreamReader(txtCityFile.Text);
   string inputString = string.Empty;
   AzureTableStorage ats = new AzureTableStorage(txtAccount.Text, txtEndpoint.Text, txtSharedKey.Text, "SharedKey");
   StringBuilder sb = new StringBuilder();
   sb.AppendFormat(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<entry xml:base=""http://finseldemos.table.core.windows.net/"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" 
 xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns=""http://www.w3.org/2005/Atom"">");
   int entityCount = 0;
   ats.Tables(cmdType.post, tableName);
   inputString = file.ReadLine(); // read copyright
   inputString = file.ReadLine(); // read column headers
   azureHelper ah = new azureHelper(ats.auth);
   while ((inputString = file.ReadLine()) != null)
   {
    iCounter++;
    
    string[] dataSet = r.Split(inputString);
    for (int i = 0; i < dataSet.GetUpperBound(0); i++)
     dataSet[i] = dataSet[i].ToString().Replace("\"", "");
    if ((iCounter % 100) == 0)
    {
     Notify(string.Format("Processing location {0}: {1}", iCounter, DateTime.Now.ToString("O")));
    }
    //locId,country,region,city,postalCode,latitude,longitude,metroCode,areaCode
   // string ds = string.Format(locationTemplate, tableName, cityPartition, dataSet[0], inputString);
    sb.AppendFormat(locationTemplate, dataSet);
    entityCount++;
    if (entityCount >= 99)
    {
     sb.Append("</entry>");
    // Notify(string.Format("Transmitting Group of transactions, processed through {0}",iCounter));
     ah.entityGroupTransaction(cmdType.post , tableName, sb.ToString());
     sb = new StringBuilder();
     sb.AppendFormat(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<entry xml:base=""http://finseldemos.table.core.windows.net/"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" 
 xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns=""http://www.w3.org/2005/Atom"">");
     entityCount = 0;
    }
    if ((iCounter % 100)==0)
     Application.DoEvents();
   }
   if (entityCount > 0)
   {
    Notify("Transmitting Final Group of transactions");
    sb.Append("</entry>");
    ah.entityGroupTransaction(cmdType.post, tableName, sb.ToString());
    sb = new StringBuilder();
    entityCount = 0;
   }
   file.Close();


   sb = new StringBuilder();
   iCounter = 0;
   sb.AppendFormat(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<entry xml:base=""http://finseldemos.table.core.windows.net/"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" 
 xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns=""http://www.w3.org/2005/Atom"">");
   entityCount = 0;
   file = new System.IO.StreamReader(txtBlocksFile.Text);
   inputString = string.Empty;
   inputString = file.ReadLine(); // read copyright
   inputString = file.ReadLine(); // read column headers
   Int64 ipLoadRecord = 0;
   while ((inputString = file.ReadLine()) != null)
   {
    iCounter++;

    string[] dataSet = r.Split(inputString);
    for (int i = 0; i < dataSet.GetUpperBound(0); i++)
     dataSet[i] = dataSet[i].ToString().Replace("\"", "");
    if ((iCounter % 100) == 0)
    {
     Notify(string.Format("Processing location {0}: {1}", iCounter, DateTime.Now.ToString("O")));
    }
    sb.AppendFormat(ipTemplate, dataSet);
    entityCount++;
    if (entityCount >= 500)
    {
     sb.Append("</entry>");
      Notify(string.Format("Transmitting Group of transactions, processed through {0}",iCounter));
     ah.entityGroupTransaction(cmdType.post, tableName, sb.ToString());
     sb = new StringBuilder();
     sb.AppendFormat(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<entry xml:base=""http://finseldemos.table.core.windows.net/"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" 
 xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns=""http://www.w3.org/2005/Atom"">");
     entityCount = 0;
    }
    if ((iCounter % 100) == 0)
    {
     Notify(string.Format("Processed through {0}", iCounter));
     Application.DoEvents();
    }
   }
   if (entityCount > 0)
   {
    Notify("Transmitting Final Group of transactions");
    sb.Append("</entry>");
    ah.entityGroupTransaction(cmdType.post, tableName, sb.ToString());
    sb = new StringBuilder();
    entityCount = 0;
   }
   file.Close();
   this.Cursor = Cursors.Default;
  }

  private void Notify(string message)
  {
   toolStripStatusLabel1.Text = message;
   Application.DoEvents();
  }

  string ipTemplate = @"<m:properties>
        <d:PartitionKey>IPBlocks</d:PartitionKey>
        <d:RowKey>{0}</d:RowKey>
	<d:StartIPNumber m:Type=""Edm.Int64"">{0}</d:StartIPNumber>
	<d:EndIPNumber m:Type=""Edm.Int64"">{1}</d:EndIPNumber>
      </m:properties>";

  //locId,country,region,city,postalCode,latitude,longitude,metroCode,areaCode
  string locationTemplate = @"<m:properties>
        <d:PartitionKey>Cities</d:PartitionKey>
        <d:RowKey>{0}</d:RowKey>
        <d:country>{1}</d:country>
        <d:region>{2}</d:region>
        <d:city>{3}</d:city>
        <d:postalCode>{4}</d:postalCode>
	      <d:latitude>{5}</d:latitude>
	      <d:longitude>{6}</d:longitude>
	      <d:metroCode>{7}</d:metroCode>
	      <d:areaCode>{8}</d:areaCode>
      </m:properties>";

  private void Form1_FormClosing(object sender, FormClosingEventArgs e)
  {
   MaxMindIPGeoLocation.Properties.Settings.Default.Account  = txtAccount.Text;
   MaxMindIPGeoLocation.Properties.Settings.Default.Endpoint = txtEndpoint.Text;
   MaxMindIPGeoLocation.Properties.Settings.Default.SharedKey = txtSharedKey.Text;
   MaxMindIPGeoLocation.Properties.Settings.Default.Save();
  }



  
 }
}