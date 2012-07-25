using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.IO;


using System.Xml;
using System.Collections;




namespace Finsel.AzureCommands
{
 /// <summary>
 /// Used for interfacing with the Microsoft's Azure Table Storage
 /// </summary>
 public class AzureTableStorage
 {

  azureCommon ac = new azureCommon();
  azureDirect ad = new azureDirect();

   private Authentication pAuth = new Authentication();
  /// <summary>
  /// Object that stores information needed to access an instance of 
  /// Azure Table Storage
  /// </summary>
  public Authentication auth { get { return pAuth; } set { pAuth = value; pAuth.EndPoint = string.Format(EndpointFormat, pAuth.Account); ad.auth = pAuth; } }


  /// <summary>
  /// Create a new AzureCommands object
  /// This requires filling in the .auth object
  /// </summary>
  public AzureTableStorage()
  {
  }

  /// <summary>
  /// Create a new AzureTableStorage object and populate the Authentication information
  /// </summary>
  /// <param name="account">The account is generally the first part of the Endpoint.</param>
  /// <param name="endPoint">The Endpoint for Azure Table Storage as defined for your account</param>
  /// <param name="sharedKey">The SharedKey for access</param>
  /// <param name="keyType">Shared</param>
  public AzureTableStorage(string account, string endPoint, string sharedKey, string keyType)
  {
   pAuth  = new Authentication(account, string.Format(EndpointFormat, account), sharedKey);
   ad.auth = pAuth;
  }

  /// <summary>
  /// Create a new AzureTableStorage object and populate the Authentication information
  /// </summary>
  /// <param name="pAuth">takes an Azure Authentication object</param>
  public AzureTableStorage(Authentication pAuth)
  {
   auth = pAuth;
  }

  // In the current release, this is the same for all clients, differing only in the 
  // opening subdomain
  private string EndpointFormat = "http://{0}.table.core.windows.net/";

  /// <summary>
  /// Results of request
  /// </summary>
  public string ETag = string.Empty;



  private string contentType = "application/atom+xml";
  private DateTime requestDate = DateTime.UtcNow;
  private string contentMD5 = string.Empty;
  private string authHeader = string.Empty;
  private string method = string.Empty;


 


  /// <summary>
  /// Get a list of tables defined for an ATS instance
  /// </summary>
  /// <returns>An azureResults showing the results of the request.</returns>
  public azureResults GetTableList()
  {
   return Tables(cmdType.get, "");

  }

  /// <summary>
  /// Tables handles table level operations against Azure Table Storage
  /// </summary>
  /// <param name="cmd">The type of operation you want to commit: Delete, Post, Put, Get </param>
  /// <param name="tableName">Name of the table to perform the command agains</param>
  /// <returns>An azureResults showing the results of the request.</returns>
  public azureResults Tables(cmdType cmd, string tableName)
  {
   return Tables(cmd, tableName, "");
  }

  /// <summary>
  /// Tables handles table level operations against Azure Table Storage
  /// </summary>
  /// <param name="cmd">The type of operation you want to commit: Delete, Post, Put, Get </param>
  /// <param name="tableName">Name of the table to perform the command agains</param>
  /// <param name="parameters">Uri Parameters to include</param>
  /// <returns>An azureResults showing the results of the request.</returns>
  public azureResults Tables(cmdType cmd, string tableName, string parameters)
  {
   azureResults retVal = new azureResults();
   HttpStatusCode success = HttpStatusCode.NotImplemented;
   Hashtable headers = new Hashtable();
   try
   {
    StringBuilder sb = new StringBuilder();
    string sendBody = string.Empty;
    string rtnBody = string.Empty;
    string requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}", auth.EndPoint, "Tables");
    requestDate = DateTime.UtcNow;
    switch (cmd)
    {
     case cmdType.get:
      method = "GET";
      string allData = tableName;
      if (tableName != string.Empty)
      {
       if (parameters == string.Empty)
       {
        requestUrl = requestUrl.Replace("Tables", string.Format(CultureInfo.CurrentCulture, "{0}()", allData));
       }
       else
       {  
        requestUrl += string.Format(CultureInfo.CurrentCulture, "('{0}')", allData);
        if (!parameters.StartsWith("?"))
         parameters = "?" + parameters;
        requestUrl += string.Format(CultureInfo.CurrentCulture, "{0}", parameters);
       }
      }
      success = HttpStatusCode.OK;
      break;
     case cmdType.post:
      method = "POST";
      // build valid Atom document
      sendBody = string.Format(createTableXml, requestDate, tableName);
      success=  HttpStatusCode.Created;
      break;
     case cmdType.delete:
      method = "DELETE";
      requestUrl += string.Format(CultureInfo.CurrentCulture, "('{0}')", tableName);
      success = HttpStatusCode.NoContent;
      break;
     default:
      break;
    }

    headers.Add("Content-Type", contentType);
    retVal = ad.ProcessRequest(cmd, requestUrl, sendBody, headers);
    if (success == HttpStatusCode.NotImplemented)
    {
      retVal.Succeeded = false;
      retVal.StatusCode = success;
    }
    else
      retVal.Succeeded = (retVal.StatusCode == success);

   }
   catch (HttpException hex)
   {
    retVal.StatusCode = (HttpStatusCode)hex.GetHttpCode();
    retVal.Succeeded = false;
    retVal.Body = hex.GetHtmlErrorMessage();
   }
   catch (Exception ex)
   {
    retVal.StatusCode = HttpStatusCode.SeeOther;
    retVal.Body = ex.ToString();
    retVal.Succeeded = false;
   }

   return retVal;
  }


 


  /// <summary>
  /// Entities returns information about entities. Uses If-Match of * and forces overwrites/deletes of data.
  /// </summary>
  /// <param name="cmd">The type of command you want to execute</param>
  /// <param name="tableName">Table name</param>
  /// <param name="PartitionKey">Partition Key</param>
  /// <param name="rowKey">Row Key</param>
  /// <param name="docData">Additional Data for Updates, Adds, Merges</param>
  /// <param name="parameters">Any Uri parameters to be passed in. comp=list can be passed in to get a list.</param>
  /// <returns></returns>
  public azureResults Entities(cmdType cmd, string tableName, string PartitionKey, string rowKey, string docData, string parameters)
  {
   return Entities(cmd, tableName, PartitionKey, rowKey, docData, parameters, "*");
  }

  /// <summary>
  /// Entities returns information about entities
  /// </summary>
  /// <param name="cmd">The type of command you want to execute</param>
  /// <param name="tableName">Table name</param>
  /// <param name="PartitionKey">Partition Key</param>
  /// <param name="rowKey">Row Key</param>
  /// <param name="docData">Additional Data for Updates, Adds, Merges</param>
  /// <param name="IfMatch">The entity tag or * to execute an update/merge/delete regardless of the entity tag</param>
  /// <param name="parameters">Any Uri parameters to be passed in. comp=list can be passed in to get a list.</param>
  /// <returns></returns>
  public azureResults Entities(cmdType cmd, string tableName, string PartitionKey, string rowKey, string docData, string parameters, string IfMatch)
  {
    Hashtable headers = new Hashtable();
   azureResults retVal = new azureResults();
   HttpStatusCode success = HttpStatusCode.NotImplemented;
    try
    {
     string sendBody = string.Empty;
     string readBody = string.Empty;
     string requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}", auth.EndPoint, tableName);
     requestDate = DateTime.UtcNow;
     switch (cmd)
     {
      case cmdType.get:
       method = "GET";
       IfMatch = string.Empty;
       sendBody = string.Empty;
       if (PartitionKey != string.Empty && rowKey != string.Empty)
       {
        requestUrl += string.Format(CultureInfo.CurrentCulture, "(PartitionKey='{0}',RowKey='{1}')", PartitionKey, rowKey);
       }
       else if (PartitionKey != string.Empty)
       {
        requestUrl += string.Format(CultureInfo.CurrentCulture, "(PartitionKey='{0}')", PartitionKey);
       }
       else
       {
        requestUrl += string.Format(CultureInfo.CurrentCulture, "()");
       }
       if (parameters != string.Empty)
       {
        if (!parameters.StartsWith("?"))
         parameters = "?" + parameters;
        requestUrl += string.Format(CultureInfo.CurrentCulture, parameters);
       }
       // do GET
       success = HttpStatusCode.OK;
       break;

      case cmdType.post:
       method = "POST";

       // accept input doc and parse into valid Atom for Azure Tables
       readBody = docData;
       IfMatch = "";
       sendBody = string.Format(createEntityXml, requestDate, readBody);
       sendBody = string.Format(sendBody, PartitionKey, rowKey);
       success = HttpStatusCode.Created;
       break;

      case cmdType.put:
       method = "PUT";
       requestUrl += string.Format(CultureInfo.CurrentCulture, "(PartitionKey='{0}',RowKey='{1}')", PartitionKey, rowKey);
       // accept input doc and parse into valid Atom for Azure Tables
       sendBody = docData;
       sendBody = string.Format(updateEntityXml, tableName, PartitionKey, rowKey, requestDate, sendBody, this.ETag.Replace(@"""", "&quot;"), auth.Account);

       success = HttpStatusCode.NoContent;
       break;

      case cmdType.merge:
       method = "MERGE";
       requestUrl += string.Format(CultureInfo.CurrentCulture, "(PartitionKey='{0}',RowKey='{1}')", PartitionKey, rowKey);
       // accept input doc and parse into valid Atom for Azure Tables
       sendBody = docData;
       sendBody = string.Format(updateEntityXml, tableName, PartitionKey, rowKey, requestDate, sendBody, this.ETag.Replace(@"""", "&quot;"), auth.Account);
       success = HttpStatusCode.NoContent;
       break;

      case cmdType.delete:
       method = "DELETE";
       sendBody = string.Empty; 

       requestUrl += string.Format(CultureInfo.CurrentCulture, "(PartitionKey='{0}',RowKey='{1}')", PartitionKey, rowKey);

       success = HttpStatusCode.NoContent;
       break;

      default:
       retVal.Succeeded = false;
       retVal.StatusCode = HttpStatusCode.NotImplemented;
       break;
     }
     headers.Add("Content-Type", contentType);
     if (IfMatch != string.Empty)
       headers.Add("If-Match", IfMatch);
     retVal = ad.ProcessRequest(cmd, requestUrl, sendBody, headers);
     if (success == HttpStatusCode.NotImplemented)
     {
       retVal.Succeeded = false;
       retVal.StatusCode = success;
     }
     else
       retVal.Succeeded = (retVal.StatusCode == success);
    
    }
    catch (HttpException hex)
    {
     retVal.Succeeded = false;
     retVal.StatusCode = (HttpStatusCode)hex.GetHttpCode();
     retVal.Body = hex.GetHtmlErrorMessage();
    }
    catch (Exception ex)
    {
     retVal.StatusCode = HttpStatusCode.SeeOther;
     retVal.Body = ex.ToString();
     retVal.Succeeded = false;
    }


   // Note: Body may contain data elements with XML... will need to use
   // Uri.UnescapeDataString() to format correctly
   return retVal;
  }

  
  



  // stub create table body
  string createTableXml = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
      <entry 
        xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" 
        xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" 
        xmlns=""http://www.w3.org/2005/Atom"">
        <title />
        <updated>{0:yyyy-MM-ddTHH:mm:ss.fffffffZ}</updated>
        <author>
          <name />
        </author>
        <id />
        <content type=""application/xml"">
          <m:properties>
            <d:TableName>{1}</d:TableName>
          </m:properties>
        </content>
      </entry>";

  // stub create entity body
  string createEntityXml = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
      <entry 
        xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" 
        xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" 
        xmlns=""http://www.w3.org/2005/Atom"">
        <title />
        <updated>{0:yyyy-MM-ddTHH:mm:ss.fffffffZ}</updated>
        <author>
          <name />
        </author>
        <id />
        <content type=""application/xml"">
          {1}
        </content>
      </entry>";

  /* sample body to pass in via string or file ref
  <m:properties>
    <d:PartitionKey>{0}</d:PartitionKey>
    <d:RowKey>{1}</d:RowKey>
    <d:Address>Mountain View</d:Address>
    <d:Age m:type=""Edm.Int32"">23</d:Age>
    <d:AmountDue m:type=""Edm.Double"">200.23</d:AmountDue>
    <d:BinaryData m:type=""Edm.Binary"" m:null=""true"" />
    <d:CustomerCode m:type=""Edm.Guid"">c9da6455-213d-42c9-9a79-3e9149a57833</d:CustomerCode>
    <d:CustomerSince m:type=""Edm.DateTime"">2008-07-10T00:00:00</d:CustomerSince>
    <d:IsActive m:type=""Edm.Boolean"">true</d:IsActive>
    <d:NumOfOrders m:type=""Edm.Int64"">255</d:NumOfOrders>
    <d:Timestamp m:type=""Edm.DateTime"">0001-01-01T00:00:00</d:Timestamp>
  </m:properties>
  */

  // stub update entity body
  string updateEntityXml = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
      <entry 
        xml:base=""http://mamund.table.core.windows.net/"" 
        xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" 
        xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" 
        m:etag=""{5}"" 
        xmlns=""http://www.w3.org/2005/Atom"">
        <id>http://{6}.table.core.windows.net/{0}(PartitionKey='{1}',RowKey='{2}')</id>
        <title type=""text""></title>
        <updated>{3:yyyy-MM-ddTHH:mm:ss.fffffffZ}</updated>
        <author>
          <name />
        </author>
        <link rel=""edit""  href=""{0}(PartitionKey='{1}',RowKey='{2}')"" />
        <category term=""{6}.{0}"" scheme=""http://schemas.microsoft.com/ado/2007/08/dataservices/scheme"" />
        <content type=""application/xml"">
          {4}
        </content>
      </entry>";

  /// <summary>
  /// Get the BuildInformation for revisions
  /// </summary>
  /// <returns>String representing the build information for the builder</returns>
  public string BuildInfo()
  {
    System.Version AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
    return string.Format("Version: {0}.{1}.{2}.{3}", AppVersion.Major.ToString(), AppVersion.Minor.ToString(), AppVersion.Build.ToString(),
      AppVersion.Revision.ToString());
  }
 }

}