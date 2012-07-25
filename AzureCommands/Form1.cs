using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Finsel.AzureCommands;
using System.Collections;
using System.IO;
using Microsoft.Win32;
using System.Xml;
using Microsoft.WindowsAzure.StorageClient;

namespace AzureCommands
{


 public partial class Form1 : Form
 {

  private string bulkTag = "bulkprocessing";

  public Form1()
  {
   InitializeComponent();
  }

  private void Form1_Load(object sender, EventArgs e)
  {
   txtAccount.Text = AzureCommands.Properties.Settings.Default.Account;
   txtEndpoint.Text = AzureCommands.Properties.Settings.Default.Endpoint;
   txtSharedKey.Text = AzureCommands.Properties.Settings.Default.SharedKey;

   ProcessResults(new azureResults());

  }

  private void Form1_FormClosing(object sender, FormClosingEventArgs e)
  {
   AzureCommands.Properties.Settings.Default.Account = txtAccount.Text;
   AzureCommands.Properties.Settings.Default.Endpoint = txtEndpoint.Text;
   AzureCommands.Properties.Settings.Default.SharedKey = txtSharedKey.Text;
   AzureCommands.Properties.Settings.Default.Save();

  }

  private void btnGetTables_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   cbTables.Items.Clear();
   cbTables.Text = "";
   AzureTableStorage ats = new AzureTableStorage(txtAccount.Text, txtEndpoint.Text, txtSharedKey.Text, "SharedKey");
   azureResults ar = ats.GetTableList();
   if (ar.Succeeded)
   {
    XmlDocument xdoc = new XmlDocument();
    xdoc.LoadXml(ar.Body);
    //Instantiate an XmlNamespaceManager object. 
    System.Xml.XmlNamespaceManager xmlnsManager = new System.Xml.XmlNamespaceManager(xdoc.NameTable);

    //Add the namespaces used in books.xml to the XmlNamespaceManager.
    xmlnsManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
    xmlnsManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
    XmlNodeList nodes = xdoc.SelectNodes("//d:TableName", xmlnsManager);

    foreach (XmlNode node in nodes)
    {
     cbTables.Items.Add(node.InnerText);
    }
   }
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void btnGet_Click(object sender, EventArgs e)
  {
   Process(cmdType.get);
  }

  private void btnPost_Click(object sender, EventArgs e)
  {
   Process(cmdType.post);
  }

  private void btnPut_Click(object sender, EventArgs e)
  {
   Process(cmdType.put);
  }

  private void btnDelete_Click(object sender, EventArgs e)
  {
   Process(cmdType.delete);
  }

  private void btnMerge_Click(object sender, EventArgs e)
  {
   Process(cmdType.merge);
  }

  private void Process(cmdType cmd)
  {
   this.Cursor = Cursors.WaitCursor;
   azureResults ar = new azureResults();
   try
   {
    AzureTableStorage ats = new AzureTableStorage(txtAccount.Text, txtEndpoint.Text, txtSharedKey.Text, "SharedKey");
    string tableName = cbTables.Text;
    if (!cbBulkProcess.Checked)
    {
     if (txtRowKey.Text == string.Empty && txtPartitionKey.Text == string.Empty && txtDocumentData.Text == string.Empty )
      ar = ats.Tables(cmd, tableName);
     else
      ar = ats.Entities(cmd, tableName, txtPartitionKey.Text, txtRowKey.Text, txtDocumentData.Text, txtTParameters.Text ,txtIfMatch.Text );
    }
    else
    {
     azureHelper ah = new azureHelper(txtAccount.Text, txtEndpoint.Text, txtSharedKey.Text, "SharedKey");
     string results = ah.entityGroupTransaction(cmd, cbTables.Text, txtDocumentData.Text);
     ar.Body = results;
     ar.Succeeded = true;
    }
    ProcessResults(ar);
   }
   catch (Exception ex)
   {
    //Literal1.Text = string.Format("<textarea id=\"txtResponse\" name=\"S1\">{0}</textarea>", ex.ToString()); //.Replace("<", "&lt").Replace(">", "&gt;");
    lblError.Text = ex.ToString();
    lblStatus.Text = "Error:";
    lblCalledURL.Text = "";
   }
   this.Cursor = Cursors.Default;
  }

  private void btnQuery_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureTableStorage ats = new AzureTableStorage(txtAccount.Text, txtEndpoint.Text, txtSharedKey.Text, "SharedKey");
   string tableName = cbTables.Text;
   string filter = txtDocumentData.Text;
   string parameters = txtTParameters.Text;
   if (filter.StartsWith("?"))
    filter = filter.Substring(1);
   if (!filter.StartsWith("$filter=")) 
    filter = string.Format("$filter={0}",filter);
   if(parameters != string.Empty){
   if(parameters.StartsWith("?"))
    parameters = parameters.Substring(1);
   if(!parameters.StartsWith("&"))
    parameters = string.Format("&{0}",parameters);
   filter = string.Format("{0}{1}",filter, parameters);
   }

   azureResults ar = ats.Entities(cmdType.get, tableName,"","","",filter  );
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void btnGetContainers_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   cbBlobContainers.Items.Clear();
   cbBlobContainers.Text = string.Empty;

   AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = abs.GetContainerList("");
   if (ar.Succeeded)
   {
    XmlDocument xdoc = new XmlDocument();
    xdoc.LoadXml(ar.Body);
    XmlNodeList nodes = xdoc.SelectNodes("//Container");

    foreach (XmlNode node in nodes)
    {
     cbBlobContainers.Items.Add(node.SelectSingleNode("Name").InnerText);
    }
   }
   ProcessResults(ar);
   this.Cursor = Cursors.Default;

  }

  private void btnBulkFiles_Click(object sender, EventArgs e)
  {
   //this.Cursor = Cursors.WaitCursor;
   //AzureTableStorage ats = new AzureTableStorage(txtAccount.Text, txtEndpoint.Text, txtSharedKey.Text, "SharedKey");
   // azureResults ar= ats.ListBulkRequestFiles();
   // txtResults.Text = ar.Body;
   //this.Cursor = Cursors.Default;
  }

  private void btnDeleteContainer_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = abs.Containers(cmdType.delete, cbBlobContainers.Text, new Hashtable());
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void btnCreateContainer_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   Hashtable htMetaData = new Hashtable();
   htMetaData.Add("x-ms-meta-createdBy", "Finsel.AzureCommands");
   azureResults ar = abs.Containers(cmdType.put, cbBlobContainers.Text, htMetaData);
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void btnDispCMetadata_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = abs.MetaData(cmdType.get, cbBlobContainers.Text, cbBlobs.Text, null);
   StringBuilder msg = new StringBuilder();
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void button1_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = abs.MetaData(cmdType.put , cbBlobContainers.Text,cbBlobs.Text , null);
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void btnAddMetaData_Click(object sender, EventArgs e)
  {
   Hashtable ht = new Hashtable();
   string[] AllData = txtMetaData.Text.Split("\n".ToCharArray());
   foreach (string metadataDetail in AllData)
   {
    //string[] detail = metadataDetail.Split(":".ToCharArray());
    string metadataName = string.Empty;
    string metadataValue = string.Empty;
    if (metadataDetail.Contains(":"))
    {
     metadataName = metadataDetail.Substring(0, metadataDetail.IndexOf(":"));
     if (!metadataName.StartsWith("x-ms-meta-"))
       metadataName = "x-ms-meta-" + metadataName;
     if (metadataDetail.Length > metadataDetail.IndexOf(":"))
      metadataValue = metadataDetail.Substring(metadataDetail.IndexOf(":") + 1);
    }
    else
     metadataName = metadataDetail;

    if (metadataName != string.Empty)
    {

     metadataValue = metadataValue.Replace("\r", "");
     if (ht.ContainsKey(metadataName))
      ht[metadataName] = string.Format("{0},{1}", ht[metadataName].ToString(), metadataValue);
     else
      ht.Add(metadataName, metadataValue);
    }
   }
   AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = abs.MetaData(cmdType.put, cbBlobContainers.Text, cbBlobs.Text, ht);
   ProcessResults(ar);
  }

   private string getMimeType(string fileName)
   {
     return getMimeType(fileName, "binary/octet-stream");
   }

private string getMimeType(string fileName, string defaultMimeType)
{
  string mimeType = defaultMimeType ;
  string ext = System.IO.Path.GetExtension(fileName).ToLower();
  Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
  if (regKey != null && regKey.GetValue("Content Type") != null)
    mimeType = regKey.GetValue("Content Type").ToString();
  if ( ext == ".js")
      mimeType = "text/javascript";
  if (ext == ".css")
      mimeType = "text/css";

  return mimeType;
}

  static public string Beautify(string xmlData)
  {
   XmlDocument doc = new XmlDocument();
   doc.LoadXml(xmlData);
   StringBuilder sb = new StringBuilder();
   XmlWriterSettings settings = new XmlWriterSettings();
   settings.Indent = true;
   settings.IndentChars = "  ";
   settings.NewLineChars = "\r\n";
   settings.NewLineHandling = NewLineHandling.Replace;
   XmlWriter writer = XmlWriter.Create(sb, settings);
   doc.Save(writer);
   writer.Close();
   return sb.ToString();
  }

  private void btnPutBlobs_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   string filename = txtBlobLocation.Text;
   if (!File.Exists(filename))
    MessageBox.Show(string.Format("{0} is not a valid file", filename), "Error with file", MessageBoxButtons.OK, MessageBoxIcon.Error);
   else
   {
     if (cbBlobs.Text == string.Empty)
       //     MessageBox.Show("You must specify a blob name!", "Blob name missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
       cbBlobs.Text = new FileInfo(filename).Name;
     //else
     {
       byte[] blobArray = FileToByteArray(filename);

       AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
       azureResults ar = abs.PutBlob(blobArray.Length, getMimeType(filename), blobArray, cbBlobContainers.Text, cbBlobs.Text, new Hashtable(), (chkPageBlob.Checked ? AzureBlobStorage.BlobType.PageBlob : AzureBlobStorage.BlobType.BlockBlob));
       ProcessResults(ar);
     }
   }
   this.Cursor = Cursors.Default;
  }

public byte[] FileToByteArray(string fileName)
{
 byte[] retVal = null;
 try
 {
  FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
  retVal = new BinaryReader(fs).ReadBytes((int)fs.Length);
 }
 catch (Exception ex)
 {
  Console.WriteLine(ex.Message);
 }
 return retVal;
}

  private void btnGetBlobs_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   cbBlobs.Items.Clear();
   cbBlobs.Text = string.Empty;

   AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   string parameterList = string.Empty;
   if (chkBlobDirectories.Checked)
     if (cbBlobs.Text == string.Empty)
       parameterList = "delimiter=/";
     else
       parameterList = string.Format("prefix={0}&delimiter={1}", (cbBlobs.Text == string.Empty ? "/" : cbBlobs.Text), "/");
   if (chkIncludeSnapshots.Checked)
   {
     if (parameterList != string.Empty)
       parameterList = string.Format("{0}&include=snapshots", parameterList);
     else parameterList = "include=snapshots";
   }
   if (chkUncommittedBlobs.Checked)
   {
     if (parameterList != string.Empty)
       parameterList = string.Format("{0}&include=uncommittedblobs", parameterList);
     else parameterList = "include=uncommittedblobs";
   }

   azureResults ar = abs.GetBlobList(cbBlobContainers.Text, parameterList);
   if (ar.Succeeded)
   {
    XmlDocument xdoc = new XmlDocument();
    xdoc.LoadXml(ar.Body);
    XmlNodeList nodes = xdoc.SelectNodes("//Blob");

    foreach (XmlNode node in nodes)
    {
      
      if (node.SelectSingleNode("Snapshot") == null)
        cbBlobs.Items.Add(node.SelectSingleNode("Name").InnerText);
      else
        cbBlobs.Items.Add( string.Format("{0}?snapshot={1}", node.SelectSingleNode("Name").InnerText, node.SelectSingleNode("Snapshot").InnerText));
    }
   }
   ProcessResults(ar);
   this.Cursor = Cursors.Default;

  }

  private void btnDeleteBlob_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = abs.DeleteBlob(cbBlobContainers.Text, cbBlobs.Text);
   ProcessResults(ar);

   this.Cursor = Cursors.Default;
  }

  private void btnGetBlob_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = new azureResults();
   byte[] blob = abs.GetBlob(cbBlobContainers.Text, cbBlobs.Text, "", ref ar);
   if (blob != null)
   {
    string fileName = txtBlobLocation.Text;
    if (File.Exists(fileName))
     fileName = string.Format("{0}{1}", fileName, Guid.NewGuid().ToString());
     if(fileName != string.Empty)
      File.WriteAllBytes(fileName, blob);
   }
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void btnGetQueues_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   cbQueues.Items.Clear();
   cbQueues.Text = string.Empty;

   AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");

   azureResults ar = aqs.GetQueueList("");
   if (ar.Succeeded)
   {
    XmlDocument xdoc = new XmlDocument();
    xdoc.LoadXml(ar.Body);
    XmlNodeList nodes = xdoc.SelectNodes("//Queue");

    foreach (XmlNode node in nodes)
    {
     cbQueues.Items.Add(node.SelectSingleNode("Name").InnerText);
    }
   }
   ProcessResults(ar);



   this.Cursor = Cursors.Default;

  }

  private void btnCreateQueue_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   Hashtable htMetaData = new Hashtable();
   htMetaData.Add("createdBy", "Finsel.AzureCommands");

   azureResults ar = aqs.Queues(cmdType.put, cbQueues.Text, "", htMetaData);
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void btnDeleteQueue_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = aqs.Queues(cmdType.delete, cbQueues.Text, "", new Hashtable());
   ProcessResults(ar);

   this.Cursor = Cursors.Default;
  }

  private void btnDisplayQMetaData_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = aqs.MetaData(cmdType.get, cbQueues.Text, new Hashtable());
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void btnAddQueueMetadata_Click(object sender, EventArgs e)
  {
   Hashtable ht = new Hashtable();
   string[] AllData = txtQMetaData.Text.Split("\n".ToCharArray());
   foreach (string metadataDetail in AllData)
   {
    string[] detail = metadataDetail.Split(":".ToCharArray());
    string key = "";
    string value = "";
    if (detail[0] != string.Empty)
    {
     key = detail[0];
     if (metadataDetail.Contains(":"))
      value = detail[1];
     ht.Add(key, value);
    }
   }
   AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");

   azureResults ar = aqs.MetaData(cmdType.put, cbQueues.Text, ht);
   ProcessResults(ar);
  }

  private void btnDeleteQueueMetadata_Click(object sender, EventArgs e)
  {
   AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = aqs.MetaData(cmdType.put, cbQueues.Text, new Hashtable());
   ProcessResults(ar);
  }


  private void btnCreateMessage_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = aqs.Messages(cmdType.post, cbQueues.Text, txtMessage.Text, "", "");
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void btnGetMessage_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;

   AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = aqs.Messages(cmdType.get, cbQueues.Text, "", txtQParameters.Text, "");
   txtMessageID.Text = "";
   txtPopReceipt.Text = "";
   txtMessage.Text = "";
   if (ar.Body != null)
   {
    System.Xml.XmlDocument xdoc = new System.Xml.XmlDocument();
    xdoc.LoadXml(ar.Body);
    System.Xml.XmlNodeList nodes = xdoc.SelectNodes("//QueueMessage");
    if (nodes.Count == 0)
     txtMessage.Text = "No message to process";
    else
     foreach (System.Xml.XmlNode node in nodes)
     {
      txtMessageID.Text = node.SelectSingleNode("MessageId").InnerText;
      if (node.SelectNodes("//PopReceipt").Count > 0)
       txtPopReceipt.Text = node.SelectSingleNode("PopReceipt").InnerText;
      if (node.SelectNodes("//MessageText").Count > 0)
      txtMessage.Text = node.SelectSingleNode("MessageText").InnerText;

      
     }
   }
   else txtMessage.Text = "No message to process";
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void btnDeleteMessage_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   string parameters = string.Empty;
   if (txtPopReceipt.Text != string.Empty)
    parameters = string.Format("popreceipt={0}", txtPopReceipt.Text);
   azureResults ar = aqs.Messages(cmdType.delete, cbQueues.Text, "", parameters, txtMessageID.Text);
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  //private string[] ParseTableName(string xml)
  //{

  //  XmlDocument xdoc = new XmlDocument();
  //  xdoc.LoadXml(tableXML);
  //  //Instantiate an XmlNamespaceManager object. 
  //  System.Xml.XmlNamespaceManager xmlnsManager = new System.Xml.XmlNamespaceManager(xdoc.NameTable);

  //  //Add the namespaces used in books.xml to the XmlNamespaceManager.
  //  xmlnsManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
  //  xmlnsManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
  //  XmlNodeList nodes = xdoc.SelectNodes("//d:TableName", xmlnsManager);

  //  foreach (XmlNode node in nodes)
  //  {
  //    sb.AppendFormat("{0},", node.InnerText);
  //  }
  //  return  sb.ToString().Split(",".ToCharArray());
  //}

  private string FormatXml(string xmlString)
  {

      StringBuilder sb = new StringBuilder();
      try
      {
          XmlDocument doc = new XmlDocument();
          doc.LoadXml(xmlString);

          System.IO.TextWriter tr = new System.IO.StringWriter(sb);
          XmlTextWriter wr = new XmlTextWriter(tr);
          wr.Formatting = Formatting.Indented;
          doc.Save(wr);
          wr.Close();
      }
      catch { sb.Append(xmlString); }
      return sb.ToString();
  }

  private void ProcessResults(azureResults ar)
  {
   txtMetaData.Text = "";
   txtQMetaData.Text = "";
   txtResults.Text = FormatXml(ar.Body);
   lblCalledURL.Text = ar.Url;
   txtURI.Text = ar.Url;
   lblCanonicalUrl.Text = ar.CanonicalResource;
   lblCanonicalUrl.Text = lblCanonicalUrl.Text.Replace("\n","\\n");
   if (Convert.ToInt16(ar.StatusCode) == 0)
   {
    lblError.Text = "";
    lblStatus.Text = "";
   }
   else
   {
    lblError.Text = ar.StatusCode.ToString();
    lblStatus.Text = (ar.Succeeded ? "Success" : "Error");
   }
   StringBuilder msg = new StringBuilder();
   if (ar.Headers != null)
   {
    if (ar.Headers.Count > 0)
    {
     foreach (DictionaryEntry item in ar.Headers)
     {
      msg.AppendFormat("{0}: {1}\r\n", item.Key.ToString(), item.Value.ToString());
      if (item.Key.ToString().StartsWith("x-ms-meta")) // need to populate metadata
      {
       if (ar.Url.Contains(".blob.")) // we have blob metadata
        txtMetaData.Text += string.Format("{0}: {1}\r\n", item.Key.ToString(), item.Value.ToString());
       if (ar.Url.Contains(".queue.")) // we have queue metadata
        txtQMetaData.Text += string.Format("{0}: {1}\r\n", item.Key.ToString(), item.Value.ToString());
      }
     }
    }
   }
   txtHeaders.Text = msg.ToString();
   if (!ar.Succeeded && Convert.ToInt16(ar.StatusCode) != 0)
    MessageBox.Show(ar.StatusCode.ToString(), "Error with request", MessageBoxButtons.OK, MessageBoxIcon.Error);
  }

  private void btnBlobPublic_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = abs.SetContainerAccess(cbBlobContainers.Text, true);
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }

  private void btnBlobPrivate_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = abs.SetContainerAccess(cbBlobContainers.Text, false);
   ProcessResults(ar);
   this.Cursor = Cursors.Default;
  }





  /// <summary>
  /// cbItem is a handy class for adding items to
  /// a ComboBox when you don't want to databind.
  /// </summary>
  public class cbItem
  {
   /// <summary>
   /// The name of the object
   /// </summary>
   public string Name = string.Empty;

   /// <summary>
   /// The value of the object
   /// </summary>
   public string Value = string.Empty;

   /// <summary>
   /// Create a new Combo Box object
   /// </summary>
   /// <param name="oName">Name of the object, will display in the ComboBox</param>
   /// <param name="oValue">Value of the object.</param>
   public cbItem(string oName, string oValue)
   {
    Name = oName;
    Value = oValue;
   }

   public override string ToString()
   {
    return Name;
   }
  }

  private void btnLoadDemo_Click(object sender, EventArgs e)
  {
   string newID = Guid.NewGuid().ToString();
   string queueMessage = string.Format("<message><updateID>{0}</updateID><account>{1}</account><key>{2}</key><table>{3}</table><source>http://finseldemos.blob.core.windows.net/public/CalendarDemo.xml</source></message>",
    newID,txtAccount.Text, txtSharedKey.Text, cbTables.Text );
   AzureTableStorage ats = new AzureTableStorage(txtAccount.Text, "", txtSharedKey.Text, "SharedKey");
   azureResults ar = ats.Tables(cmdType.post, bulkTag);
   ar = ats.Entities(cmdType.post, bulkTag, newID, "Summary", string.Format(queueInitialCreationEntity, newID), null);
   AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, "", txtSharedKey.Text, "SharedKey");
   ar = aqs.Queues(cmdType.put, bulkTag, null, new Hashtable());

   ar = aqs.Messages(cmdType.post, bulkTag, queueMessage, string.Empty, string.Empty);
  }

  


  string queueInitialCreationEntity = "<m:properties>\n\t<d:PartitionKey>{0}</d:PartitionKey>\n\t<d:RowKey>000000000000000</d:RowKey><d:Status>Pending</d:Status>\n\t</m:properties>";
  string queueUpdateCreationEntity = "<m:properties>\n\t<d:PartitionKey>{0}</d:PartitionKey>\n\t<d:RowKey>000000000000000</d:RowKey><d:Status>Processing</d:Status><d:Processed>{1}</d:Processed><d:Errors>{2}</d:Errors>\n\t</m:properties>";
  string queueProcessingEntity =     "<m:properties>\n\t<d:PartitionKey>{0}</d:PartitionKey><d:RowKey>{1}</d:RowKey><d:Status>{2}</d:Status><d:Information>{3}</d:Information></m:properties>";

  private void txtProcessDemo_Click(object sender, EventArgs e)
  {
   this.Cursor = Cursors.WaitCursor;
   
   StringBuilder sbResultsForStorage = new StringBuilder();
   AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
   azureResults ar = aqs.Messages(cmdType.get, bulkTag, "", "visibilitytimeout=7200", "");
   string MessageID = "";
   string PopReceipt = "";
   string Message = "";
   if (ar.Body != null)
   {
    System.Xml.XmlDocument xdoc = new System.Xml.XmlDocument();
    if (ar.Succeeded)
    {
     xdoc.LoadXml(ar.Body);
     System.Xml.XmlNodeList nodes = xdoc.SelectNodes("//QueueMessage");
     StringBuilder sbMultipart = new StringBuilder();
     if (nodes.Count == 0)
      txtMessage.Text = "No message to process";
     else
      foreach (System.Xml.XmlNode node in nodes)
      {
       MessageID = node.SelectSingleNode("MessageId").InnerText;
       PopReceipt = node.SelectSingleNode("PopReceipt").InnerText;
       Message = node.SelectSingleNode("MessageText").InnerText;

       System.Xml.XmlDocument msgDoc = new XmlDocument();
       msgDoc.LoadXml(Message);
       string newAccount = msgDoc.SelectSingleNode("//account[1]").InnerXml;
       string newKey = msgDoc.SelectSingleNode("//key[1]").InnerXml;
       string newSource = msgDoc.SelectSingleNode("//source[1]").InnerXml;
       string updateID = msgDoc.SelectSingleNode("//updateID[1]").InnerXml;
       string newTable = msgDoc.SelectSingleNode("//table[1]").InnerXml;
       AzureTableStorage ats = new AzureTableStorage(txtAccount.Text, "", txtSharedKey.Text, "SharedKey");
       AzureTableStorage ats1 = new AzureTableStorage(txtAccount.Text, "", txtSharedKey.Text, "SharedKey");

       azureHelper ah = new azureHelper(txtAccount.Text, txtEndpoint.Text, txtSharedKey.Text, "SharedKey");

       string mrgMessage = string.Format(queueUpdateCreationEntity, updateID, 0, 0);
       ats.Entities(cmdType.merge, bulkTag, updateID, "000000000000000", mrgMessage, "");
       ats.Entities(cmdType.merge, bulkTag, updateID, "000000000000000", string.Format("<m:properties>\n\t<d:PartitionKey>{0}</d:PartitionKey>\n\t<d:RowKey>000000000000000</d:RowKey><d:StartedProcessing>{1}</d:StartedProcessing></m:properties>", updateID, DateTime.UtcNow.ToLongTimeString()), "");

       AzureBlobStorage abs = new AzureBlobStorage(newAccount, string.Format("http://{0}.blob.core.windows.net", newAccount), newKey, "SharedKey");

       AzureTableStorage atsNew = new AzureTableStorage(newAccount, string.Format("http://{0}.blob.core.windows.net", newAccount), newKey, "SharedKey");
       ar = atsNew.Tables(cmdType.post, newTable);
       if (ar.Succeeded || ar.StatusCode == System.Net.HttpStatusCode.Conflict)
       {
        ar = new azureResults();
        string newContainer = newSource.Replace(string.Format("http://{0}.blob.core.windows.net/", newAccount), "");
        newContainer = newContainer.Substring(0, newContainer.IndexOf("/"));
        string newBlob = newSource.Replace(string.Format("http://{0}.blob.core.windows.net/", newAccount), "").Replace(newContainer, "");
        byte[] blob = abs.GetBlob(newContainer, newBlob, "", ref ar);
        string x = new System.Text.UTF8Encoding().GetString(blob);
        x = x.Substring(x.IndexOf("<"));
        msgDoc.LoadXml(x);

        int errorCt = 0;
        int processedCt = 0;
        //Instantiate an XmlNamespaceManager object. 
        System.Xml.XmlNamespaceManager xmlnsManager = new System.Xml.XmlNamespaceManager(xdoc.NameTable);

        //Add the namespaces used in books.xml to the XmlNamespaceManager.
        xmlnsManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
        xmlnsManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
        XmlNodeList pnodes = msgDoc.SelectNodes("//m:properties", xmlnsManager);
        int iCounter = 101;
        int iResponse = 1;
        foreach (XmlNode pnode in pnodes)
        {
         if (iCounter > 100)
         {
          if (sbMultipart.Length > 0)
          {
           sbMultipart.Append("</entry>");
           ProcessMultiPartForStatus(ats.auth, ah.entityGroupTransaction(cmdType.post, newTable, sbMultipart.ToString()), bulkTag, updateID, iResponse.ToString("D15"), ref processedCt, ref errorCt, "201 Created");
           ar = ats1.Entities(cmdType.post, bulkTag, updateID, iResponse.ToString("D15"), sbResultsForStorage.ToString(), "");
           mrgMessage = string.Format(queueUpdateCreationEntity, updateID, processedCt, errorCt);
           ats.Entities(cmdType.merge, bulkTag, updateID, "000000000000000", mrgMessage, "", "*");
           iResponse++;
          }
          sbMultipart = new StringBuilder();
          sbMultipart.AppendFormat(@"<?xml version=""1.0"" encoding=""utf-8"" ?><entry xml:base=""http://finseldemos.table.core.windows.net/"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns=""http://www.w3.org/2005/Atom"">");
          iCounter = 0;
         }
         sbMultipart.Append(pnode.OuterXml);
         iCounter++;

        }
        sbMultipart.Append("</entry>");
        ProcessMultiPartForStatus(ats.auth, ah.entityGroupTransaction(cmdType.post, newTable, sbMultipart.ToString()), bulkTag, updateID, iResponse.ToString("D15"), ref processedCt, ref errorCt, "201 Created");
        mrgMessage = string.Format(queueUpdateCreationEntity, updateID, processedCt, errorCt).Replace("Processing", "Completed");
        ats.Entities(cmdType.merge, bulkTag, updateID, "000000000000000", mrgMessage, "", "*");

       }
       else
       {
        mrgMessage = string.Format(queueUpdateCreationEntity, updateID, 0, 0).Replace("Processing", "Failed to create table!");
        ats.Entities(cmdType.merge, bulkTag, updateID, "000000000000000", mrgMessage, "");
       }
       aqs.Messages(cmdType.delete, bulkTag, "", string.Format("popreceipt={0}", PopReceipt), MessageID);
       ats.Entities(cmdType.merge, bulkTag, updateID, "000000000000000", string.Format("<m:properties>\n\t<d:PartitionKey>{0}</d:PartitionKey>\n\t<d:RowKey>000000000000000</d:RowKey><d:CompletedProcessing>{1}</d:CompletedProcessing></m:properties>", updateID, DateTime.UtcNow.ToLongTimeString()), "");
      }


    }
    else txtMessage.Text = "No message to process";
    ProcessResults(ar);
    this.Cursor = Cursors.Default;
   }
  }

  private void ProcessMultiPartForStatus(Authentication auth, string MultipartResults, string containerName, string partitionKey, string rowKey, ref int processedCt, ref int ErrorCt, string successString)
  {
   //string results = string.Empty;
   string[] resultSet = null;
   string location = string.Empty;
   string resultStatus = string.Empty;
   StringBuilder sbResultsForStorage = new StringBuilder();
   resultSet = MultipartResults.Split("\n".ToCharArray());
   
   location = string.Empty;
   resultStatus = string.Empty;
   foreach (string result in resultSet)
   {

    if (result.StartsWith("HTTP/1.1"))
    {
     resultStatus = result.Substring(8).Replace("\r","");
     if (resultStatus.ToLower().Trim() != successString.ToLower().Trim())
      ErrorCt++;
     location = string.Empty;
     processedCt++;
    }
    if (result.StartsWith("Location:"))
     sbResultsForStorage.AppendFormat("{0}: {1}\r\n", resultStatus.Replace("\r", ""), result.Substring(result.IndexOf(":") + 1).Replace("\r", ""));
   }
   AzureTableStorage ats = new AzureTableStorage(auth);

  azureResults ar= ats.Entities(cmdType.post, containerName, partitionKey, rowKey, string.Format(statusUpdateTEmplate, partitionKey, rowKey, sbResultsForStorage.ToString()), "", "");


  }
string statusUpdateTEmplate = @"
      <m:properties>
        <d:PartitionKey>{0}</d:PartitionKey>
        <d:RowKey>{1}</d:RowKey>
        <d:DetailedUpdate>{2}</d:DetailedUpdate>
      </m:properties>";

  private void textBox1_TextChanged(object sender, EventArgs e)
  {

  }

  private void btnGenericGet_Click(object sender, EventArgs e)
  {
   ProcessGeneric(cmdType.get);
  }

  private void ProcessGeneric(cmdType cmd)
  {
   this.Cursor = Cursors.WaitCursor;
   azureResults ar = new azureResults();
   try
   {
    azureDirect  ad= new azureDirect(txtAccount.Text, txtEndpoint.Text, txtSharedKey.Text, "SharedKey");
    Hashtable ht = new Hashtable();
    string[] AllData = txtGenericHeaders.Text.Split("\n".ToCharArray());
    foreach (string metadataDetail in AllData)
    {
     string[] detail = metadataDetail.Split(":".ToCharArray());
     string key = "";
     string value = "";
     if (detail[0] != string.Empty)
     {
      key = detail[0];
      if (metadataDetail.Contains(":"))
       value = detail[1];
      ht.Add(key, value.Replace("\r",""));
     }
    }
    ar= ad.ProcessRequest(cmd, txtURI.Text, new System.Text.ASCIIEncoding().GetBytes(txtGenericBody.Text), ht);
    ProcessResults(ar);
   }
   catch (Exception ex)
   {
    //Literal1.Text = string.Format("<textarea id=\"txtResponse\" name=\"S1\">{0}</textarea>", ex.ToString()); //.Replace("<", "&lt").Replace(">", "&gt;");
    lblError.Text = ex.ToString();
    lblStatus.Text = "Error:";
    lblCalledURL.Text = "";
   }
   this.Cursor = Cursors.Default;
  }

  
  private void btnGenericPost_Click(object sender, EventArgs e)
  {
   ProcessGeneric(cmdType.post);
  }

  private void btnGenericPut_Click(object sender, EventArgs e)
  {
   ProcessGeneric(cmdType.put);
  }

  private void btnGenericDelete_Click(object sender, EventArgs e)
  {
   ProcessGeneric(cmdType.delete);
  }

  private void btnGenericMerge_Click(object sender, EventArgs e)
  {
   ProcessGeneric(cmdType.merge);
  }

  private void btnBulk_Click(object sender, EventArgs e)
  {

  }

   private void btnHead_Click(object sender, EventArgs e)
   {
     ProcessGeneric(cmdType.head);
   }

  private void btnTestBlocks_Click(object sender, EventArgs e)
  {
   Hashtable ht = new Hashtable();
   string[] AllData = txtMetaData.Text.Split("\n".ToCharArray());
   foreach (string metadataDetail in AllData)
   {
    //string[] detail = metadataDetail.Split(":".ToCharArray());
    string metadataName = string.Empty;
    string metadataValue = string.Empty;
    if (metadataDetail.Contains(":"))
    {
     metadataName = metadataDetail.Substring(0, metadataDetail.IndexOf(":"));
     if (!metadataName.StartsWith("x-ms-meta-"))
      metadataName = "x-ms-meta-" + metadataName;
     if (metadataDetail.Length > metadataDetail.IndexOf(":"))
      metadataValue = metadataDetail.Substring(metadataDetail.IndexOf(":") + 1);
    }
    else
     metadataName = metadataDetail;

    if (metadataName != string.Empty)
    {

     metadataValue = metadataValue.Replace("\r", "");
     if (ht.ContainsKey(metadataName))
      ht[metadataName] = string.Format("{0},{1}", ht[metadataName].ToString(), metadataValue);
     else
      ht.Add(metadataName, metadataValue);
    }
   }

   AzureBlobStorage  abs= new AzureBlobStorage (txtAccount.Text, txtEndpoint.Text, txtSharedKey.Text, "SharedKey");
   string OriginalUrl = string.Format("http://{0}.blob.core.windows.net/{1}/{2}", txtAccount.Text, cbBlobContainers.Text, cbBlobs.Text);
   if (txtBlobLocation.Text.StartsWith("/"))
    txtBlobLocation.Text = txtBlobLocation.Text.Substring(1);
   string newURL = string.Format("http://{0}.blob.core.windows.net/{1}", txtAccount.Text,txtBlobLocation.Text );
   azureResults ar = abs.CopyBlob(OriginalUrl, newURL, ht);
   ar.Succeeded = (ar.StatusCode == System.Net.HttpStatusCode.Created);
  }

  private void btnLeaseBlob_Click(object sender, EventArgs e)
  {
    this.Cursor = Cursors.WaitCursor;
    AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
    AzureBlobStorage.BlobLease bl = AzureBlobStorage.BlobLease.acquireLease;
    if (cbLeaseType.SelectedIndex == -1)
      cbLeaseType.SelectedItem = "acquire";
    switch (cbLeaseType.SelectedItem.ToString())
    {
      case "acquire": bl = AzureBlobStorage.BlobLease.acquireLease; break;
      case "break": bl = AzureBlobStorage.BlobLease.breakLease; break;
      case "renew": bl = AzureBlobStorage.BlobLease.renewLease; break;
      case "release": bl = AzureBlobStorage.BlobLease.releaseLease; break;
    }
    
    azureResults ar = abs.LeaseBlob(cbBlobContainers.Text, cbBlobs.Text, bl, txtLeaseID.Text );
    ProcessResults(ar);
    //x-ms-lease-id: 0488ee2d-7268-40fb-adc9-400549f1d86a
    if (ar.Headers != null)
    {
      if (ar.Headers.ContainsKey("x-ms-lease-id"))
        txtLeaseID.Text = ar.Headers["x-ms-lease-id"].ToString();
      else txtLeaseID.Text = string.Empty;
    }
    this.Cursor = Cursors.Default;

  }

  private void btnClearQueue_Click(object sender, EventArgs e)
  {
    this.Cursor = Cursors.WaitCursor;
    AzureQueueStorage aqs = new AzureQueueStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
    azureResults ar = aqs.Messages(cmdType.delete, cbQueues.Text, "", "", "");
    ProcessResults(ar);

    this.Cursor = Cursors.Default;
  }

  private void btnSnapshotBlob_Click(object sender, EventArgs e)
  {
    this.Cursor = Cursors.WaitCursor;
    AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
    azureResults ar = abs.SnapshotBlob(cbBlobContainers.Text, cbBlobs.Text);
    ProcessResults(ar);
    this.Cursor = Cursors.Default;
  }

  private void btnVerifyBlob_Click(object sender, EventArgs e)
  {
    this.Cursor = Cursors.WaitCursor;
    System.Collections.Hashtable ht = new Hashtable();
    ht.Add("If-Modified", txtETag.Text);
    AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.queue.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
    //azureResults ar =abs.CheckBlobCache(cbBlobContainers.Text, cbBlobs.Text, txtETag.Text);
    azureResults ar = new azureResults();
    abs.GetBlob(cbBlobContainers.Text, cbBlobs.Text, "", ref ar, txtETag.Text);
    ProcessResults(ar);
    this.Cursor = Cursors.Default;
  }

  private void btnHeadBlob_Click(object sender, EventArgs e)
  {
      this.Cursor = Cursors.WaitCursor;
      AzureBlobStorage abs = new AzureBlobStorage(txtAccount.Text, string.Format("http://{0}.blob.core.windows.net", txtAccount.Text), txtSharedKey.Text, "SharedKey");
      azureResults ar = new azureResults();
      ar= abs.Blobs(cmdType.head,cbBlobContainers.Text, cbBlobs.Text, new Hashtable(), "");
      ProcessResults(ar);
      this.Cursor = Cursors.Default;
  }

  private void button1_Click_1(object sender, EventArgs e)
  {
      this.Cursor = Cursors.WaitCursor;
      var account = Microsoft.WindowsAzure.CloudStorageAccount.Parse(
          string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",txtAccount.Text, txtSharedKey.Text));
      var container = account
          .CreateCloudBlobClient()
          .GetContainerReference(cbBlobContainers.Text);
      container.CreateIfNotExist();
      var blob = container.GetBlobReference(cbBlobs.Text);

      // create a shared access signature (looks like a query param: ?se=...)
      var sas = blob.GetSharedAccessSignature(new SharedAccessPolicy()
      {
          Permissions = SharedAccessPermissions.Read,
          SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
      });
      Console.WriteLine("This link should work for the next five minutes:");
      Console.WriteLine(blob.Uri.AbsoluteUri + sas);
      Clipboard.SetText(blob.Uri.AbsoluteUri + sas);
      
/*      var uri = new UriBuilder();
      uri.Scheme = "https";
      uri.Host = string.Format("{0}.{1}", txtAccount.Text , "blob.core.windows.net");
      uri.Path = string.Format("/{0}/{1}", cbBlobContainers.Text, cbBlobs.Text);
      DateTime starttime = DateTime.UtcNow;
      DateTime endtime = DateTime.UtcNow + TimeSpan.FromMinutes(5);
      uri.Query = string.Format("st={0}&se={1}&sr=b&sp=r&sig={2}&sv=2012-02-12",
          Uri.EscapeDataString(starttime.ToString("yyyy-MM-ddTHH:mm:ssZ")),
          Uri.EscapeDataString(endtime.ToString("yyyy-MM-ddTHH:mm:ssZ")),
          Uri.EscapeDataString(MakeBlobReadSignature(starttime, endtime,
              txtAccount.Text , cbBlobContainers.Text, cbBlobs.Text, txtSharedKey.Text)));
      Clipboard.SetText(uri.ToString());*/
      this.Cursor = Cursors.Default;

  }
  public static string MakeBlobReadSignature(DateTime starttime, DateTime endtime,
 string account, string container, string blobname, string sharedKey)
  {
      string stringtosign =
          string.Format("r\n{0:yyyy-MM-ddThh:mm:ssZ}\n{1:yyyy-MM-ddTHH:mm:ssZ}\n/{2}/{3}/{4}\n\n2012-02-12",
          starttime, endtime, account, container, blobname);
      byte[] signatureByteForm = System.Text.Encoding.UTF8.GetBytes(sharedKey );

      using (var hmac = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(sharedKey)))
      {
          return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringtosign)));
      } 
  }

     
 }
 public class ListItem
 {
   private string id = string.Empty;
   private string name = string.Empty;
   public ListItem(string sid, string sname)
   {
     id = sid;
     name = sname;
   }
   public override string ToString()
   {
     return this.name;
   }
   public string ID
   {
     get
     {
       return this.id;
     }
     set
     {
       this.id = value;
     }
   }
   public string Name
   {
     get
     {
       return this.name;
     }
     set
     {
       this.name = value;
     }
   }
 }
}