using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Xml;
using System.Collections;

using Finsel.AzureCommands;

namespace AzureCommandsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string azSharedKey = string.Empty;
        private string azEndPoint = string.Empty;
        private string azAccount = string.Empty;

        struct blobData
        {
           public string Url;
           public string LastModified;
           public string Etag;
           public Int64 ContentLength;
           public string ContentType;
           public string ContentEncoding;
           public string ContentLanguage;
           public string ContentMD5;
           public string CacheControl;
           public string BlobType;
           public string LeaseStatus;
           public string LeaseState;
           public string ContainerName;
           public string BlobName;
        }

        struct blobPrefix
        {
            public string ContainerName;
            public string fullPath;
        }

        struct blobContainer {
            public string ContainerName;
            public string Url;
            public string LastModified;
            public string Etag;
            public string LeaseStatus;
            public string LeaseState;
            
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        protected string FormatXml(string xmlString)
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
            //txtMetaData.Text = "";
            txtQMetaData.Text = "";
            txtResults.Text = FormatXml(ar.Body);
            lblCalledURL.Content = ar.Url;
            //txtURI.Text = ar.Url;
            lblCanonicalUrl.Content = ar.CanonicalResource.Replace("\n", "\\n");
            if (Convert.ToInt16(ar.StatusCode) == 0)
            {
                lblError.Content = "";
                lblStatus.Content = "";
            }
            else
            {
                lblError.Content = ar.StatusCode.ToString();
                lblStatus.Content = (ar.Succeeded ? "Success" : "Error");
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
                            //if (ar.Url.Contains(".blob.")) // we have blob metadata
                            //   txtMetaData.Text += string.Format("{0}: {1}\r\n", item.Key.ToString(), item.Value.ToString());
                            if (ar.Url.Contains(".queue.")) // we have queue metadata
                                txtQMetaData.Text += string.Format("{0}: {1}\r\n", item.Key.ToString(), item.Value.ToString());
                        }
                    }
                }
            }
            txtHeaders.Text = msg.ToString();
            //if (!ar.Succeeded && Convert.ToInt16(ar.StatusCode) != 0)
            //   MessageBox.Show(ar.StatusCode.ToString(), "Error with request", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
   
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Account = azAccount;
            Properties.Settings.Default.Endpoint = azEndPoint;
            Properties.Settings.Default.SharedKey = azSharedKey;
            Properties.Settings.Default.Save();

        }

        private void GetAuthDetails()
        {
            Authentication wa = new Authentication();
            wa.txtSharedKey.Text = azSharedKey;
            wa.txtAccount.Text = azAccount;
            wa.txtEndPoint.Text = azEndPoint;

            wa.ShowDialog();
            azSharedKey = wa.txtSharedKey.Text;
            azAccount = wa.txtAccount.Text;
            azEndPoint = wa.txtEndPoint.Text;
            if (azAccount.Trim() == string.Empty || azEndPoint.Trim() == string.Empty || azSharedKey.Trim() == string.Empty)
            {
                MessageBoxResult mbr = MessageBox.Show("You must have all of your authentication details filled out to use this app", "Authentication Details Required", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                mainTab.IsEnabled = false;
            }
            else mainTab.IsEnabled = true;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            azAccount = Properties.Settings.Default.Account;
            azEndPoint = Properties.Settings.Default.Endpoint;
            azSharedKey = Properties.Settings.Default.SharedKey;
            if (azAccount.Trim() == string.Empty || azEndPoint.Trim() == string.Empty || azSharedKey.Trim() == string.Empty)
            {
                GetAuthDetails();
            }
            else mainTab.IsEnabled = true;
        }

        private void click_Queues(object sender, RoutedEventArgs e)
        {
            
            if (e.Source.ToString().StartsWith("System.Windows.Controls.Button"))
            {
                this.Cursor = Cursors.Wait;
                AzureQueueStorage aqs = new AzureQueueStorage(azAccount, string.Format("http://{0}.queue.core.windows.net", azAccount), azSharedKey, "SharedKey");
                azureResults ar = new azureResults();
                Hashtable ht = new Hashtable();
                if (e.Source == btnAddQueueMetaData)
                {
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
                    ar = aqs.MetaData(cmdType.put, cbQueues.Text, ht);
                }
                else if (e.Source == btnClearQueue)
                {
                    ar = aqs.Messages(cmdType.delete, cbQueues.Text, "", "", "");
                }
                else if (e.Source == btnCreateMessage)
                {
                    ar = aqs.Messages(cmdType.post, cbQueues.Text, txtMessage.Text, "", "");
                }
                else if (e.Source == btnCreateQueue)
                {
                    ht.Add("createdBy", "Finsel.AzureCommands");
                    ar = aqs.Queues(cmdType.put, cbQueues.Text, "", ht);
                }
                else if (e.Source == btnDeleteMessage)
                {
                    string parameters = string.Empty;
                    if (txtPopReceipt.Text != string.Empty)
                        parameters = string.Format("popreceipt={0}", txtPopReceipt.Text);
                    ar = aqs.Messages(cmdType.delete, cbQueues.Text, "", parameters, txtMessageID.Text);
                }
                else if (e.Source == btnDeleteQueue)
                {
                    ar = aqs.Queues(cmdType.delete, cbQueues.Text, "", new Hashtable());
                }
                else if (e.Source == btnDeleteQueueMetaData)
                {
                    ar = aqs.MetaData(cmdType.put, cbQueues.Text, new Hashtable());
                }
                else if (e.Source == btnDisplayQueueMetadata)
                {
                    ar = aqs.MetaData(cmdType.get, cbQueues.Text, new Hashtable());
                }
                else if (e.Source == btnGetMessage)
                {
                    ar = aqs.Messages(cmdType.get, cbQueues.Text, "", txtQParameters.Text, "");
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
                }
                else if (e.Source == btnGetQueues)
                {
                    cbQueues.Items.Clear();
                    cbQueues.Text = string.Empty;
                    ar = aqs.GetQueueList("");
                    if (ar.Succeeded)
                    {
                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(ar.Body);
                        XmlNodeList nodes = xdoc.SelectNodes("//Queue");
                        foreach (XmlNode node in nodes)
                        {
                            cbQueues.Items.Add(node.SelectSingleNode("Name").InnerText);
                        }
                        if (cbQueues.Items.Count > 0)
                            cbQueues.SelectedIndex = 0;
                    }
                }
                ProcessResults(ar);
                this.Cursor = Cursors.Arrow;
            }
        }

        private void click_Tables(object sender, RoutedEventArgs e)
        {
            AzureTableStorage ats = new AzureTableStorage(azAccount, azEndPoint, azSharedKey, "SharedKey");
            azureResults ar = new azureResults();
            if (e.Source.ToString().StartsWith("System.Windows.Controls.Button"))
            {
                this.Cursor = Cursors.Wait;
                if (e.Source == btnGet)
                    Process(cmdType.get);
                else if (e.Source == btnPut)
                    Process(cmdType.put);
                else if (e.Source == btnPost)
                    Process(cmdType.post);
                else if (e.Source == btnDelete)
                    Process(cmdType.delete);
                else if (e.Source == btnMerge)
                    Process(cmdType.merge);
                else if (e.Source == btnQuery)
                {
                    string tableName = cbTables.Text;
                    string filter = txtDocumentData.Text;
                    string parameters = txtTParameters.Text;
                    if (filter.StartsWith("?"))
                        filter = filter.Substring(1);
                    if (!filter.StartsWith("$filter="))
                        filter = string.Format("$filter={0}", filter);
                    if (parameters != string.Empty)
                    {
                        if (parameters.StartsWith("?"))
                            parameters = parameters.Substring(1);
                        if (!parameters.StartsWith("&"))
                            parameters = string.Format("&{0}", parameters);
                        filter = string.Format("{0}{1}", filter, parameters);
                    }

                     ar = ats.Entities(cmdType.get, tableName, "", "", "", filter);
                    ProcessResults(ar);
                }
                else if (e.Source == btnGetTables)
                {
                    cbTables.Items.Clear();
                    cbTables.Text = "";
                    ar = ats.GetTableList();
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
                }
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Process(cmdType cmd)
        {
            azureResults ar = new azureResults();
            try
            {
                AzureTableStorage ats = new AzureTableStorage(azAccount, azEndPoint, azSharedKey, "SharedKey");
                string tableName = cbTables.Text;
                if (!(bool)chkBulk.IsChecked)
                {
                    if (txtRowKey.Text == string.Empty && txtPartitionKey.Text == string.Empty && txtDocumentData.Text == string.Empty)
                        ar = ats.Tables(cmd, tableName);
                    else
                        ar = ats.Entities(cmd, tableName, txtPartitionKey.Text, txtRowKey.Text, txtDocumentData.Text, txtTParameters.Text, txtIfMatch.Text);
                }
                else
                {
                    azureHelper ah = new azureHelper(azAccount, azEndPoint, azSharedKey, "SharedKey");
                    string results = ah.entityGroupTransaction(cmd, cbTables.Text, txtDocumentData.Text);
                    ar.Body = results;
                    ar.Succeeded = true;
                }
                ProcessResults(ar);
            }
            catch (Exception ex)
            {
                //Literal1.Text = string.Format("<textarea id=\"txtResponse\" name=\"S1\">{0}</textarea>", ex.ToString()); //.Replace("<", "&lt").Replace(">", "&gt;");
                lblError.Content = ex.ToString();
                lblStatus.Content = "Error:";
                lblCalledURL.Content = "";
            }
           
        }

        private void mnuLoadBlobTree_Click(object sender, RoutedEventArgs e)
        {
            LoadContainers();
        }

        private void LoadContainers()
        {
            this.Cursor = Cursors.Wait;
            tvBlobs.Items.Clear();

            AzureBlobStorage abs = new AzureBlobStorage(azAccount, string.Format("http://{0}.blob.core.windows.net", azAccount), azSharedKey, "SharedKey");
            azureResults ar = abs.GetContainerList("");
            if (ar.Succeeded)
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(ar.Body);
                XmlNodeList nodes = xdoc.SelectNodes("//Container");

                foreach (XmlNode node in nodes)
                {
                    TreeViewItem tvI = new TreeViewItem();
                    blobContainer bc = new blobContainer();
                    try{bc.ContainerName = node.SelectSingleNode("Name").InnerText;}
                    catch { }
                    try {bc.Etag = node.SelectSingleNode("Properties/Etag").InnerText;}
                    catch { }
                    try {bc.LeaseState = node.SelectSingleNode("Properties/LeaseState").InnerText;}
                    catch { }
                    try {bc.LeaseStatus = node.SelectSingleNode("Properties/LeaseStatus").InnerText;}
                    catch { }
                    try { bc.LastModified = node.SelectSingleNode("Properties/LastModified").InnerText; }
                    catch { }
                    tvI.Header = bc.ContainerName;
                    tvI.Tag = bc;
                    tvI.Items.Add("*");
                    tvBlobs.Items.Add(tvI);
                }
                mnuNewBlobContainer.IsEnabled = true;
                mnuDeleteContainer.IsEnabled = true;
            }
            ProcessResults(ar);
            this.Cursor = Cursors.Arrow;
        }


        private void tvBlobs_Expanded(object sender, RoutedEventArgs e)
        {
            ExpandBlobItem((TreeViewItem)e.OriginalSource); 
        }

        private void ExpandBlobItem(TreeViewItem item)
        {
            this.Cursor = Cursors.Wait;
           
            item.Items.Clear();
            AzureBlobStorage abs = new AzureBlobStorage(azAccount, string.Format("http://{0}.blob.core.windows.net", azAccount), azSharedKey, "SharedKey");
            string parameterList = string.Empty;

            parameterList = "delimiter=/";
            /* if (chkIncludeSnapshots.Checked)
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
             */
            string path = string.Empty;
            if (item.Tag is blobPrefix)
            {
                blobPrefix bp = (blobPrefix)item.Tag;
                path = bp.ContainerName;
                parameterList = string.Format("delimiter=/&prefix={0}", bp.fullPath);
            }
            if (item.Tag is blobContainer)
            {
                path = ((blobContainer)item.Tag).ContainerName;
            }
            azureResults ar = abs.GetBlobList(path, parameterList);
            if (ar.Succeeded)
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(ar.Body);
                XmlNodeList nodes = xdoc.SelectNodes("//BlobPrefix");
                foreach (XmlNode node in nodes)
                {
                    TreeViewItem tvIAdd = new TreeViewItem();
                    string headerInfo = node.SelectSingleNode("Name").InnerText;
                    if (item.Tag is blobPrefix)
                        headerInfo = headerInfo.Replace(((blobPrefix)item.Tag).fullPath, "");
                    tvIAdd.Header = headerInfo.Replace("/", "");
                    blobPrefix bp = new blobPrefix();
                    if (item.Tag is blobContainer)
                    {
                        bp.ContainerName = ((blobContainer)item.Tag).ContainerName;
                        bp.fullPath = node.SelectSingleNode("Name").InnerText;
                    }
                    if (item.Tag is blobPrefix)
                    {
                        bp.ContainerName = ((blobPrefix)item.Tag).ContainerName;
                        bp.fullPath = node.SelectSingleNode("Name").InnerText;
                    }
                    tvIAdd.Tag = bp;
                    tvIAdd.Items.Add("*");
                    item.Items.Add(tvIAdd);
                }

                nodes = xdoc.SelectNodes("//Blob");

                foreach (XmlNode node in nodes)
                {
                    TreeViewItem tvIAdd = new TreeViewItem();
                    blobData bd = new blobData();
                    bd.Url = node.SelectSingleNode("Url").InnerText;
                    bd.BlobName = bd.Url.Substring(bd.Url.IndexOf(".net") + 5);
                    bd.ContainerName = bd.BlobName.Substring(0, bd.BlobName.IndexOf("/"));
                    bd.BlobName = bd.BlobName.Substring(bd.BlobName.IndexOf("/")+1);
                    bd.LastModified = node.SelectSingleNode(@"Properties/Last-Modified").InnerText;
                    bd.Etag = node.SelectSingleNode(@"Properties/Etag").InnerText;
                    bd.ContentLength = Convert.ToInt64(node.SelectSingleNode(@"Properties/Content-Length").InnerText);
                    bd.ContentType = node.SelectSingleNode(@"Properties/Content-Type").InnerText;
                    bd.BlobType = node.SelectSingleNode(@"Properties/BlobType").InnerText;
                    bd.LeaseStatus = node.SelectSingleNode(@"Properties/LeaseStatus").InnerText;
                    tvIAdd.Tag = bd;
                    string headerInfo = node.SelectSingleNode("Name").InnerText;
                    if (item.Tag is blobPrefix)
                        headerInfo = headerInfo.Replace(((blobPrefix)item.Tag).fullPath, "");
                    if (node.SelectSingleNode("Snapshot") == null)
                        tvIAdd.Header = headerInfo;
                    else
                        tvIAdd.Header = string.Format("{0}?snapshot={1}", headerInfo, node.SelectSingleNode("Snapshot").InnerText);
                    //if (bd.ContentLength == 0) tvIAdd.Items.Add("*");

                    item.Items.Add(tvIAdd);
                }
            }
            ProcessResults(ar);
            this.Cursor = Cursors.Arrow;
        }

        private void tvBlobs_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.Source;
            if (item.Tag is blobData)
            {
                foreach (ItemsControl i in treeviewContextMenu.Items)
                {
                    if (i.Tag.ToString() == "Container")
                        i.Visibility = System.Windows.Visibility.Collapsed;
                    if (i.Tag.ToString() == "Blob")
                        i.Visibility = System.Windows.Visibility.Visible;
                }
                blobData bd =  (blobData)item.Tag;
                txtBlobSize.Text = bd.ContentLength.ToString();
                GetBlobData(((blobData)item.Tag).ContainerName, ((blobData)item.Tag).BlobName);
            }
            if (item.Tag is blobContainer)
            {
                foreach (ItemsControl i in treeviewContextMenu.Items)
                {
                    if (i.Tag.ToString() == "Container")
                        i.Visibility = System.Windows.Visibility.Visible;
                    if (i.Tag.ToString() == "Blob")
                        i.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

        }

        private void mnuNewBlobContainer_Click(object sender, RoutedEventArgs e)
        {
            newContainer nc = new newContainer();
            nc.ShowDialog();
            if (!nc.cancelled)
            {
                this.Cursor = Cursors.Wait;
                AzureBlobStorage abs = new AzureBlobStorage(azAccount, string.Format("http://{0}.blob.core.windows.net", azAccount), azSharedKey, "SharedKey");
                Hashtable htMetaData = new Hashtable();
                htMetaData.Add("x-ms-meta-createdBy", "Finsel.AzureCommands");
                azureResults ar = abs.Containers(cmdType.put, nc.newContainerName, htMetaData);
                ProcessResults(ar);
                if (ar.Succeeded)
                {
                    TreeViewItem tvI = new TreeViewItem();
                    blobContainer bc = new blobContainer();
                    bc.ContainerName = nc.newContainerName; 
                    tvI.Header = bc.ContainerName;
                    tvI.Tag = bc;
                    tvI.Items.Add("*");
                    tvBlobs.Items.Add(tvI);
                }
                this.Cursor = Cursors.Arrow;
            }
        }

        private void resetBlobContainerText()
        {
            foreach (UIElement element in blobContainerDetails.Children)
            {
                if (element is TextBox)
                    ((TextBox)element).Text = "";
            }
        }

        private void GetBlobData(string containerName, string blobName)
        {
            this.Cursor = Cursors.Wait;
            resetBlobContainerText();
            containerDataPanel.Visibility = System.Windows.Visibility.Collapsed;
            blobDataPanel.Visibility = System.Windows.Visibility.Visible;
            AzureBlobStorage abs = new AzureBlobStorage(azAccount, string.Format("http://{0}.blob.core.windows.net", azAccount), azSharedKey, "SharedKey");
            azureResults ar = abs.Blobs(cmdType.head, containerName, blobName, new Hashtable(), "");
            ProcessResults(ar);
            // load detail pane
            StringBuilder sbMeta = new StringBuilder();
            txtBlobContainerUrl.Text = ar.Url;
            foreach (DictionaryEntry item in ar.Headers)
            {
                if (item.Key.ToString().StartsWith("x-ms-meta")) // need to populate metadata
                {
                    sbMeta.AppendLine(string.Format("{0}:{1}", item.Key.ToString(), item.Value.ToString()));
                }
                switch (item.Key.ToString())
                {
                    case "Content-Encoding": txtContentEncoding.Text = item.Value.ToString(); break;
                    case "Content-Length": txtBlobSize.Text = item.Value.ToString(); break;
                    case "Content-MD5": txtBlobMD5.Text = item.Value.ToString(); break;
                    case "Content-Type": txtContentType.Text = item.Value.ToString(); break;
                    case "ETag": txtETag.Text = item.Value.ToString(); break;
                    case "Last-Modified": txtLastModified.Text = item.Value.ToString(); break;
                    case "x-ms-blob-type": txtBlobType.Text = item.Value.ToString(); break;
                    case "x-ms-lease-duration": txtLeaseDuration.Text = item.Value.ToString(); break;
                    case "x-ms-lease-state": txtLeaseState.Text = item.Value.ToString(); break;
                    case "x-ms-lease-status": txtLeaseStatus.Text = item.Value.ToString(); break;
                    case "x-ms-version": txtblobContainerVersion.Text = item.Value.ToString(); break;
                }
            }
            txtBlobContainerHeaders.Text = sbMeta.ToString();

        }

        private void mnuLeaseBlob_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            TreeViewItem tvI = (TreeViewItem)tvBlobs.SelectedItem;
            string containerName = tvI.Parent.ToString();
            string blobName = tvI.ToString();
            AzureBlobStorage abs = new AzureBlobStorage(azAccount, string.Format("http://{0}.blob.core.windows.net", azAccount), azSharedKey, "SharedKey");
            AzureBlobStorage.BlobLease bl = AzureBlobStorage.BlobLease.acquireLease;
            bl = AzureBlobStorage.BlobLease.acquireLease;
            if (e.Source == mnuAcquireLease)
                bl = AzureBlobStorage.BlobLease.acquireLease;
            if(e.Source ==mnuBreakLease)
                bl = AzureBlobStorage.BlobLease.breakLease;
            if(e.Source==mnuReleaseLease)
                bl = AzureBlobStorage.BlobLease.releaseLease;
            if(e.Source==mnuRenewLease)
                bl = AzureBlobStorage.BlobLease.renewLease;
            azureResults ar = abs.LeaseBlob(containerName, blobName , bl, txtLeaseID.Text);
            ProcessResults(ar);
            //x-ms-lease-id: 0488ee2d-7268-40fb-adc9-400549f1d86a
            if (ar.Headers != null)
            {
                if (ar.Headers.ContainsKey("x-ms-lease-id"))
                    txtLeaseID.Text = ar.Headers["x-ms-lease-id"].ToString();
                else txtLeaseID.Text = string.Empty;
            }
            this.Cursor = Cursors.Arrow;
        }

        private void mnuDeleteContainer_Click(object sender, RoutedEventArgs e)
        {
            if (tvBlobs.SelectedItem != null)
            {
                TreeViewItem item = (TreeViewItem)tvBlobs.SelectedItem;
                if (item.Tag is blobContainer)
                {
                    DeleteContainer dc = new DeleteContainer();
                    dc.txtNewContainerName.Text = ((blobContainer)item.Tag).ContainerName;
                    dc.ShowDialog();
                    if (!dc.cancelled)
                    {
                        this.Cursor = Cursors.Wait;
                        AzureBlobStorage abs = new AzureBlobStorage(azAccount, string.Format("http://{0}.blob.core.windows.net", 
                            azAccount), azSharedKey, "SharedKey");
                        azureResults ar = abs.Containers(cmdType.delete, dc.txtNewContainerName.Text, new Hashtable());
                        ProcessResults(ar);
                        if (ar.Succeeded)
                        {
                            tvBlobs.Items.Remove(item);
                        }
                        this.Cursor = Cursors.Arrow;
                    }
                }
                else if (item.Tag is blobPrefix)
                { }
                else if (item.Tag is blobData)
                { }
            }
         }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tvBlobs.Items.Count==1)
                LoadContainers();
        }

        private void mnuSetContainerRights_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuDeleteBlob_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)tvBlobs.SelectedItem;
            if (item.Tag is blobData)
            {

                if (MessageBox.Show(string.Format("Delete {0}/{1}?",((blobData)item.Tag).ContainerName, ((blobData)item.Tag).BlobName), "Confirmation", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                {
                    this.Cursor = Cursors.Wait;
                    string ContainerName = ((blobData)item.Tag).ContainerName;
                    string BlobName = ((blobData)item.Tag).BlobName;
                    AzureBlobStorage abs = new AzureBlobStorage(azAccount, string.Format("http://{0}.blob.core.windows.net", azAccount), azSharedKey, "SharedKey");
                    azureResults ar = abs.DeleteBlob(ContainerName, BlobName);
                    ProcessResults(ar);
                    if (ar.Succeeded)
                    {
                        tvBlobs.Items.Remove(item);
                    }
                    this.Cursor = Cursors.Arrow;
                }
            }
        }

        private void mnuRenameBlob_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuSetBlobRights_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuAcquireContainerLease_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuRenewContainerLease_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuBreakContainerLease_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuReleaseContainerLease_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnAuth_Click(object sender, RoutedEventArgs e)
        {
            GetAuthDetails();
        }
    }
}
