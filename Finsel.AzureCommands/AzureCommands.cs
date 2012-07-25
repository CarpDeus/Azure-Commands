using System;
using System.Collections.Generic;
using System.Text;

using System.Globalization;
using Amundsen.Utilities; // http://code.google.com/u/mca.amundsen/updates
using System.Net;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
namespace Finsel.AzureCommands
{

 /// <summary>
 /// The types of command: Delete, Get, Post, Put
 /// </summary>
 public enum cmdType
 {
  /// <summary>
  /// Delete 
  /// </summary>
  delete,
  /// <summary>
  /// Get
  /// </summary>
  get,
  /// <summary>
  /// Post used for create
  /// </summary>
  post,
  /// <summary>
  /// Put used for update
  /// </summary>
  put,
  /// <summary>
  /// Merge 
  /// </summary>
  merge,
  /// <summary>
  /// Head calls return just header information from a GET
  /// </summary>
  head
 }

 /// <summary>
 /// Authentication object containing everything necessary
 /// to authenticate against the service
 /// </summary>
 public struct Authentication
 {
  /// <summary>
  /// The account is generally the first part of the Endpoint.
  /// </summary>
  public string Account;
  /// <summary>
  /// The Endpoint for Azure Table Storage as defined for your account
  /// (This will be in the format http://{0}.{1}.core.windows.net/
  /// where 0 is your Account and 1 is the service your using
  /// </summary>
  public string EndPoint;
  /// <summary>
  /// The SharedKey for access
  /// </summary>
  public string SharedKey;
  /// <summary>
  /// Shared
  /// </summary>
  public string KeyType;

  /// <summary>
  /// The information used for authentication
  /// </summary>
  /// <param name="pAccount">The account name is the first subdomain in your endpoint list.
  /// http://myaccount.blob.core.windows.net/ would use myaccount as the account name.</param>
  /// <param name="pEndpoint">Endpoint. This is actually calculated currently but could change in future releases.</param>
  /// <param name="pSharedKey">The shared key, either primary or secondary.</param>
  public Authentication(string pAccount, string pEndpoint, string pSharedKey)
  {
   Account = pAccount;
   EndPoint = pEndpoint;
   SharedKey = pSharedKey;
   KeyType = "SharedKey";
  }
 }

 /// <summary>
 /// Contains results about requests
 /// </summary>
 public struct azureResults
 {
  /// <summary>
  /// The httpResponse body
  /// </summary>
  public string Body;
  /// <summary>
  /// The CanonicalResource as required for authentication
  /// </summary>
  public string CanonicalResource;
  /// <summary>
  /// The actual Url that was called. 
  /// </summary>
  public string Url;
  /// <summary>
  /// The HttpStatusCode returned
  /// </summary>
  public System.Net.HttpStatusCode StatusCode;
  /// <summary>
  /// Whether or not the request was successful. This is set based on
  /// the value that the API says should be returned in StatusCode
  /// </summary>
  public bool Succeeded;
  /// <summary>
  /// The httpResponse headers collection
  /// </summary>
  public System.Collections.Hashtable Headers;
 }

 class azureCommon
 {
  /// <summary>
  /// ParsedURL represents the domain/folder/page structure used for lookups.
  /// Regex help from http://www.cambiaresearch.com/cambia3/snippets/csharp/regex/uri_regex.aspx#parsing
  /// </summary>
  struct ParsedURI
  {
   public string domainName,
       folderName,
       pageName;

   public ParsedURI(string URL)
   {
    string regexPattern = @"^(?<s1>(?<s0>[^:/\?#]+):)?(?<a1>"
          + @"//(?<a0>[^/\?#]*))?(?<p0>[^\?#]*)"
          + @"(?<q1>\?(?<q0>[^#]*))?"
          + @"(?<f1>#(?<f0>.*))?";

    Regex re = new Regex(regexPattern, RegexOptions.ExplicitCapture);
    Match m = re.Match(URL);
    domainName = m.Groups["a0"].Value;// +"  (Authority without //)<br>";
    folderName = m.Groups["p0"].Value.Substring(0, m.Groups["p0"].Value.LastIndexOf("/")); // +"  (Path)<br>";
    pageName = m.Groups["p0"].Value.Substring(m.Groups["p0"].Value.LastIndexOf("/") + 1);

    // Note: The passed in URL should never have arguments built in but, if it does, this strips
    //       them out.
    //if (pageName.IndexOf(",") != -1)
    // pageName = pageName.Substring(0, pageName.IndexOf(",")) + pageName.Substring(pageName.LastIndexOf("."));
   }
  }

  Hashing h = new Hashing();

  public System.Collections.Hashtable Headers2Hash(WebHeaderCollection coll)
  {
   System.Collections.Hashtable retVal = new System.Collections.Hashtable();
   // We need to get headers
   foreach (string header in coll)
   {
    retVal.Add(header, coll[header]);

   }
   return retVal;
  }

 

  public string CreateSharedKeyAuth(string method, string resource, string contentMD5, DateTime requestDate, HttpWebRequest client, Authentication auth)
  {
   Hashtable ht = new Hashtable();
   StringBuilder sbHdrs = new StringBuilder();
   StringBuilder sbHeader = new StringBuilder();
   string rtn = string.Empty;
   // support for 2009-09-09 version
   string contentEncoding = string.Empty;
   string contentLanguage = string.Empty;
   string dateForSigning = string.Empty;
   string ifModifiedSince = string.Empty;// client.IfModifiedSince.ToString();
   string ifMatch = string.Empty;
   string ifNoneMatch = string.Empty;
   string ifUnmodifiedSince = string.Empty;
   string rangeForSigning = string.Empty;
   string clientContentLength = string.Empty;
   string clientContentType = string.Empty;
   if (client.Headers["if-match"] != null)// && client.Headers["if-match"] != "*")
     ifMatch = client.Headers["if-match"].ToString();
   if (client.Headers["if-none-match"] != null)
     ifNoneMatch  = client.Headers["if-none-match"].ToString();
   if (client.Headers["if-unmodified-since"] != null)
     ifUnmodifiedSince = client.Headers["if-unmodified-since"].ToString();
   if (client.ContentLength > 0 || ( method != "GET" && method != "HEAD"))
     clientContentLength = client.ContentLength.ToString();

   if (client.ContentType != null)
     clientContentType = client.ContentType.ToString();

   



   //string fmtStringToSign = "{0}\n{1}\n{2}\n{3:R}\n{4}{5}";
   string fmtStringToSign = "{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n{8}\n{9}\n{10}\n{11}\n{12}{13}";
   string hdrs = CanonicalizeHeaders(client.Headers);
   //string authValue = string.Format(fmtStringToSign, method, contentMD5, client.ContentType, "", hdrs, resource);
   string authValue = string.Format(fmtStringToSign, method, contentEncoding,contentLanguage,clientContentLength, contentMD5, clientContentType,
     dateForSigning, ifModifiedSince, ifMatch,ifNoneMatch,ifUnmodifiedSince,rangeForSigning, hdrs, resource);


   byte[] signatureByteForm = System.Text.Encoding.UTF8.GetBytes(authValue);

   System.Security.Cryptography.HMACSHA256 hasher = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(auth.SharedKey));


   // Now build the Authorization header
   String authHeader = String.Format(CultureInfo.InvariantCulture,
                              "{0} {1}:{2}",
                              "SharedKey",
                              auth.Account,
                              System.Convert.ToBase64String(hasher.ComputeHash(signatureByteForm)
                              ));


   rtn = authHeader;


   return rtn;
  }

  public string CreateSharedKeyAuth(string method, string resource, string contentMD5, DateTime requestDate, HttpClient client, Authentication auth)
  {
   return CreateSharedKeyAuth(method, resource, contentMD5, requestDate, client, "", auth);
  }

 

  /*
   Constructing the CanonicalizedHeaders Element

   To construct the CanonicalizedHeaders portion of the string required for the signature, follow these steps:

   1. Retrieve all headers for the resource that begin with x-ms-, including the x-ms-date header.
   2. Convert each HTTP header name to lowercase.
   3. Sort the container of headers lexicographically by header name, in ascending order.
   4. Combine headers with the same name into one header. The resulting header should be a name-value pair of the format "header-name:comma-separated-value-list", without any white space between values. Important   The comma-separated list of headers is not ordered by the header values but by the order in which the headers appear in the request. The list of headers must be in the correct order to properly authenticate the request.
   5. Replace any breaking white space with a single space.
   6. Trim any white space around the colon in the header.
   7. Finally, append a new line character to each canonicalized header in the resulting list. Construct the CanonicalizedHeaders element by concatenating all headers in this list into a single string. 

*/
  public string CanonicalizeHeaders(WebHeaderCollection hdrCollection)
  {
   StringBuilder retVal = new StringBuilder();
   // Look for header names that start with "x-ms-"
   // Then sort them in case-insensitive manner.
   ArrayList httpStorageHeaderNameArray = new ArrayList();
   Hashtable ht = new Hashtable();
   foreach (string key in hdrCollection.AllKeys)
   {
    if (key.ToLowerInvariant().StartsWith("x-ms-", StringComparison.Ordinal))
    {
     if (ht.Contains(key.ToLowerInvariant()))
     {
      ht[key.ToLowerInvariant()] = string.Format("{0},{1}", ht[key.ToLowerInvariant()], hdrCollection[key].ToString().Replace("\n", string.Empty).Replace("\r", string.Empty).Trim());
     }
     else
     {
      httpStorageHeaderNameArray.Add(key.ToLowerInvariant());
      ht.Add(key.ToLowerInvariant(), hdrCollection[key].ToString().Replace("\n", string.Empty).Replace("\r", string.Empty).Trim());
     }
    }
   }
   httpStorageHeaderNameArray.Sort();
   // Now go through each header's values in the sorted order and append them to the canonicalized string.
   foreach (string key in httpStorageHeaderNameArray)
   {
    retVal.AppendFormat("{0}:{1}\n", key.Trim(), ht[key].ToString());
   }
   return retVal.ToString();
  }

  private Hashtable parseQueryString(string qstring)
  {
   //simplify our task
   qstring = qstring + "&";

   Hashtable outc = new Hashtable();

   Regex r = new Regex(@"(?<name>[^=&]+)=(?<value>[^&]+)&", RegexOptions.IgnoreCase | RegexOptions.Compiled);

   IEnumerator _enum = r.Matches(qstring).GetEnumerator();
   while (_enum.MoveNext() && _enum.Current != null)
   {
    outc.Add(((Match)_enum.Current).Result("${name}"),
            ((Match)_enum.Current).Result("${value}"));
   }

   return outc;
  }

/// <summary>
/// Used to create canonical headers for 2009-09-09
/// </summary>
/// <param name="qstring">query string</param>
/// <returns>canoncicalized hashtable of values</returns>
  private Hashtable parseQueryString2(string qstring)
  {
    //simplify our task
    qstring = qstring + "&";

    Hashtable outc = new Hashtable();
    Regex r = new Regex(@"(?<name>[^=&]+)=(?<value>[^&]+)&", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    IEnumerator _enum = r.Matches(qstring).GetEnumerator();
    while (_enum.MoveNext() && _enum.Current != null)
    {
      string key = ((Match)_enum.Current).Result("${name}");
      string value = ((Match)_enum.Current).Result("${value}");
      if (!outc.ContainsKey(key))
      {
        outc.Add(key, value);
      }
      else // need to sort the values
      {
        string[] currentValue = outc[key].ToString().Split(',');
        ArrayList alVals = new ArrayList();
        foreach (string v in currentValue)
          alVals.Add(v);
        alVals.Add(value);
        alVals.Sort();
        value = string.Empty;
        foreach (string v in alVals.ToArray())
          if (value != string.Empty)
            value = string.Format("{0},{1}", value, v);
          else
            value = v;
        outc[key] = value;
      }
    }
    return outc;
  }


   /// <summary>
   /// Used for Canonicalizing URLs for 2009-09-09
   /// </summary>
   /// <param name="Url">URL To canonicalize</param>
   /// <returns>Appropriately formatted string</returns>
  public string CanonicalizeUrl2(string Url)
  {
    //bool doubleSlashFlag = (Url.Substring(8).Contains("//"));
    string retVal = string.Empty;
    ParsedURI pURI = new ParsedURI(Url);
    string canonicalParameters = string.Empty;
    System.Collections.Hashtable NVPParameters = new System.Collections.Hashtable();
    string[] subdomainCollection = pURI.domainName.Split(".".ToCharArray());
    String querystring = null;
    string parameters = string.Empty;
    int iqs = Url.IndexOf('?');
    if (iqs >= 0) // I have parameters
    {
      querystring = (iqs < Url.Length - 1) ? Url.Substring(iqs + 1) : String.Empty;
      Hashtable qscoll = parseQueryString2(querystring);
      ArrayList qSort = new ArrayList();
      foreach (string pKey in qscoll.Keys)
      {

        if (pKey.ToLowerInvariant() == "comp")
          canonicalParameters = string.Format("{0}:{1}", "comp", qscoll["comp"].ToString());
        else
        qSort.Add(pKey);
      }
      qSort.Sort();
      
      foreach (string sKey in qSort)
      {
        if (canonicalParameters != string.Empty)
        {
          canonicalParameters = string.Format("{0}\n{1}:{2}", canonicalParameters, sKey.ToLowerInvariant(), qscoll[sKey].ToString());
        }
        else
          canonicalParameters = string.Format("{0}:{1}", sKey.ToLowerInvariant(), qscoll[sKey].ToString());
      }
    }
    
    if(pURI.folderName == string.Empty)
      retVal = string.Format("/{0}/", subdomainCollection[0]);
    else
    retVal = string.Format("/{0}{1}", subdomainCollection[0], pURI.folderName);
    if (pURI.pageName == string.Empty && pURI.folderName != string.Empty && !retVal.EndsWith("/"))
      retVal = retVal + "/";
    if (pURI.pageName != string.Empty || (pURI.pageName == string.Empty && pURI.folderName != string.Empty) )
    retVal = string.Format("{0}/{1}", retVal, pURI.pageName);
      retVal = retVal.Replace("//", "/");
    if (canonicalParameters != string.Empty)
    {
      retVal = string.Format("{0}\n{1}", retVal, canonicalParameters);
    }
    return retVal;

  }

  public string CanonicalizeUrl(string Url)
  {
    return CanonicalizeUrl2(Url);
  }

  public string CreateSharedKeyAuth(string method, string resource, string contentMD5, DateTime requestDate, HttpClient client, string contentType, Authentication auth)
  {
   Hashtable ht = new Hashtable();
   StringBuilder sbHdrs = new StringBuilder();
   StringBuilder sbHeader = new StringBuilder();
   string rtn = string.Empty;
   //string fmtStringToSign = "{0}\n{1}\n{2}\n{3:R}\n{4}{5}";

   //string hdr = CanonicalizeHeaders(client.RequestHeaders);
   //string authValue = string.Format(fmtStringToSign, method, contentMD5, contentType, "", hdr, resource);
     // support for 2009-09-09 version
   string contentEncoding = string.Empty;
   string contentLanguage = string.Empty;
   string dateForSigning = string.Empty;
   string ifModifiedSince = string.Empty;// client.IfModifiedSince.ToString();
   string ifMatch = string.Empty;
   string ifNoneMatch = string.Empty;
   string ifUnmodifiedSince = string.Empty;
   string rangeForSigning = string.Empty;
   string clientContentLength = string.Empty;
   string clientContentType = string.Empty;

   if (client.RequestHeaders["if-match"] != null)// && client.Headers["if-match"] != "*")
     ifMatch = client.RequestHeaders["if-match"].ToString();
   if (client.RequestHeaders["if-none-match"] != null)
     ifNoneMatch = client.RequestHeaders["if-none-match"].ToString();
   if (client.RequestHeaders["if-unmodified-since"] != null)
     ifUnmodifiedSince = client.RequestHeaders["if-unmodified-since"].ToString();
   //if (client.ResponseLength> 0 || method != "GET")
   //  clientContentLength = client.ResponseLength.ToString();

   //if (client.ContentType != null)
   //  clientContentType = client.ContentType.ToString();

   



   //string fmtStringToSign = "{0}\n{1}\n{2}\n{3:R}\n{4}{5}";
   string fmtStringToSign = "{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n{8}\n{9}\n{10}\n{11}\n{12}{13}";
   string hdrs = CanonicalizeHeaders(client.RequestHeaders);
   //string authValue = string.Format(fmtStringToSign, method, contentMD5, client.ContentType, "", hdrs, resource);
   string authValue = string.Format(fmtStringToSign, method, contentEncoding,contentLanguage,clientContentLength, contentMD5, clientContentType,
     dateForSigning, ifModifiedSince, ifMatch,ifNoneMatch,ifUnmodifiedSince,rangeForSigning, hdrs, resource);
   byte[] signatureByteForm = System.Text.Encoding.UTF8.GetBytes(authValue);

   System.Security.Cryptography.HMACSHA256 hasher = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(auth.SharedKey));


   // Now build the Authorization header
   String authHeader = String.Format(CultureInfo.InvariantCulture,
                              "{0} {1}:{2}",
                              "SharedKey",
                              auth.Account,
                              System.Convert.ToBase64String(hasher.ComputeHash(signatureByteForm)
                              ));


   rtn = authHeader;


   return rtn;
  }

  public string CreateSharedKeyAuthLite(string method, string resource, string contentMD5, DateTime requestDate, string contentType, Authentication auth)
  {
   string rtn = string.Empty;
   string fmtHeader = "{0} {1}:{2}";
   string fmtStringToSign = "{0}\n{1}\n{2}\n{3:R}\n{4}";

   string authValue = string.Format(fmtStringToSign, method, contentMD5, contentType, requestDate, resource);
   string sigValue = h.MacSha(authValue, Convert.FromBase64String(auth.SharedKey));
   rtn = string.Format(fmtHeader, auth.KeyType, auth.Account, sigValue);


   return rtn;
  }

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

  public string CleanMetaDataNames(string metadataName)
  {
    string retVal = metadataName.Replace("x-ms-meta-", "");
    if (retVal.Contains("-")) // we need to process
    {
      string tmp = retVal;
      retVal = "";
      while (tmp.Contains("--"))
        tmp = tmp.Replace("--", "-");
      while (tmp.EndsWith("-"))
        tmp = tmp.Substring(0, tmp.Length - 1);
      for (int i = 0; i < tmp.Length; i++)
      {
        if (tmp.Substring(i, 1) != "-")
          retVal = retVal + tmp.Substring(i, 1);
        else
        {
          retVal = retVal + tmp.Substring(i + 1, 1).ToUpper();
          i++;
        }
      }
    }
    return "x-ms-meta-" + retVal;
  }

 }

  /// <summary>
  /// For making calls directly to the Azure Platform with headers and body supplied
  /// </summary>
 public class azureDirect
 {

  HttpClient client = new HttpClient();
  Hashing h = new Hashing();

  azureCommon ac = new azureCommon();

  /// <summary>
  /// Defines the UserAgent string passed in when making HTTP requests
  /// </summary>
  string UserAgent = "amundsen-finsel/1.0";

  /// <summary>
  /// Create a new AzureCommands object
  /// </summary>
  public azureDirect()
  {
   client.UserAgent = UserAgent;
  }

  /// <summary>
  /// Create a new AzureCommands object with default settings
  /// </summary>
  /// <param name="account">The account is generally the first part of the Endpoint.</param>
  /// <param name="endPoint">The Endpoint for Azure Table Storage as defined for your account</param>
  /// <param name="sharedKey">The SharedKey for access</param>
  /// <param name="keyType">Shared</param>
  public azureDirect(string account, string endPoint, string sharedKey, string keyType)
  {

   auth = new Authentication(account, endPoint, sharedKey);
   client.UserAgent = UserAgent;
  }

  /// <summary>
  /// Create a new AzureCommands object with default settings
  /// </summary>
  /// <param name="pAuth">An Azure Authentication Object</param>
  public azureDirect(Authentication pAuth)
  {

   auth = pAuth ;
   client.UserAgent = UserAgent;
  }

  /// <summary>
  /// The authentication object for the Azure Table Storage
  /// </summary>
  public Authentication auth = new Authentication();

   /// <summary>
   /// Send a request to Azure, adding only the authetication
   /// </summary>
   /// <param name="cmd">Method to execute</param>
   /// <param name="requestUrl">URL to execute against</param>
   /// <param name="body">Body to send</param>
   /// <param name="headers">Headers to send</param>
   /// <returns>An Azure Result object</returns>
  public azureResults ProcessRequest(cmdType cmd, string requestUrl, string body, Hashtable headers)
  {
    return ProcessRequest(cmd, requestUrl, new System.Text.ASCIIEncoding().GetBytes(body), headers);
  }

  /// <summary>
  /// Send a request to Azure, adding only the authetication
  /// </summary>
  /// <param name="cmd">Method to execute</param>
  /// <param name="requestUrl">URL to execute against</param>
  /// <param name="body">Body to send</param>
  /// <param name="headers">Headers to send</param>
  /// <returns>An Azure Result object</returns>
  public azureResults ProcessRequest(cmdType cmd, string requestUrl, byte[] body, Hashtable headers)
  {
   Hashing h = new Hashing();
   HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
   req.Method = cmd.ToString().ToUpper();
   azureResults retVal = new azureResults();
   
   try
   {
    StringBuilder sb = new StringBuilder();
    string contentMD5 = string.Empty ;

    DateTime requestDate = DateTime.UtcNow;
    retVal.CanonicalResource = ac.CanonicalizeUrl2(requestUrl);
    string contentType = string.Empty;
    if (cmd == cmdType.head)
        req.ContentLength = 0;
    else
        req.ContentLength = body.Length;
    if (headers.ContainsKey("Content-Type"))
    {
     req.ContentType = headers["Content-Type"].ToString().Replace("\r", "");
     headers.Remove("Content-Type");
    }

    foreach (DictionaryEntry de in headers)
    {
     req.Headers[de.Key.ToString()] = de.Value.ToString();
    }
    
    //req.Headers["x-ms-version"] = "2009-07-17";
    //req.Headers["x-ms-version"] = "2009-09-19";
    req.Headers["x-ms-version"] = "2012-02-12";
    req.Headers["x-ms-date"] = string.Format(CultureInfo.CurrentCulture, "{0:R}", requestDate);
    string authHeader = string.Empty;
    if (requestUrl.Contains(".table."))
    {
      // Required with the 2009-09-19 authentiation
      req.Headers["DataServiceVersion"] = "1.0;NetFx";
      req.Headers["MaxDataServiceVersion"] = "1.0;NetFx";
      authHeader = ac.CreateSharedKeyAuthLite(req.Method, retVal.CanonicalResource, contentMD5, requestDate, req.ContentType, auth);
    }
    else
      authHeader = ac.CreateSharedKeyAuth(req.Method, retVal.CanonicalResource, contentMD5, requestDate, req, auth);
    req.Headers["authorization"] = authHeader;
    retVal.Url = requestUrl;
    if (body.Length > 0 && cmd != cmdType.head)
    {
        System.IO.Stream requestStream = req.GetRequestStream();
        requestStream.Write(body, 0, body.Length);
        requestStream.Flush();
    }

    
    HttpWebResponse response = (HttpWebResponse)req.GetResponse();
   // response = (HttpWebResponse)req.GetResponse();

    System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());
    retVal.Body = sr.ReadToEnd();
    response.Close();

    
    retVal.StatusCode = response.StatusCode;
    retVal.Headers = ac.Headers2Hash(response.Headers);
  }
   catch (WebException wex)
   {
     retVal.StatusCode = ((System.Net.HttpWebResponse)(wex.Response)).StatusCode;
    //retVal.StatusCode = wex.  (HttpStatusCode)hex.GetHttpCode();
    retVal.Succeeded = false;
    retVal.Body = wex.Message;
   /* for (int i=0;i< wex.Response.Headers.Count;i++)
      retVal.Headers.Add(wex.Response.Headers.Keys[i].ToString(), wex.Response.Headers[i].ToString());
     */
   }
   catch (Exception ex)
   {
    retVal.StatusCode = HttpStatusCode.SeeOther;
    retVal.Body = ex.ToString();
    retVal.Succeeded = false;
   }

   return retVal;
  }

    

 }

  /// <summary>
  /// A helper class that handles special cases 
  /// </summary>
 public class azureHelper
 {

  HttpClient client = new HttpClient();
  Hashing h = new Hashing();

  azureCommon ac = new azureCommon();

  /// <summary>
  /// Defines the UserAgent string passed in when making HTTP requests
  /// </summary>
  string UserAgent = "amundsen-finsel/1.0";

  /// <summary>
  /// Create a new AzureCommands object
  /// </summary>
  public azureHelper()
  {
   client.UserAgent = UserAgent;
  }

  /// <summary>
  /// Create a new AzureCommands object with default settings
  /// </summary>
  /// <param name="account">The account is generally the first part of the Endpoint.</param>
  /// <param name="endPoint">The Endpoint for Azure Table Storage as defined for your account</param>
  /// <param name="sharedKey">The SharedKey for access</param>
  /// <param name="keyType">Shared</param>
  public azureHelper(string account, string endPoint, string sharedKey, string keyType)
  {

   auth = new Authentication(account, endPoint, sharedKey);
   client.UserAgent = UserAgent;
  }

  /// <summary>
  /// Create a new AzureCommands object with default settings
  /// </summary>
  /// <param name="pAuth">Azure Authetication Object</param>
  public azureHelper(Authentication  pAuth)
  {
   auth = pAuth;
   client.UserAgent = UserAgent;
  }

  /// <summary>
  /// The authentication object for the Azure Table Storage
  /// </summary>
  public Authentication auth = new Authentication();

   /// <summary>
   /// Process Entity Group Transactions against a table by taking an XML block of Entity Data and turning it into Multipart and executing it.
   /// </summary>
   /// <param name="cmd">Method to execute</param>
   /// <param name="tableName">Table to execute the data against</param>
   /// <param name="entityData">Entity Data</param>
   /// <returns>string with results of the group transaction</returns>
  public string entityGroupTransaction(cmdType cmd,string tableName, string entityData)
  {
   int entityCounter = 1;
   StringBuilder sbResults = new StringBuilder();
   StringBuilder sbMultiPart = new StringBuilder();
   string singlePart = string.Empty;
   string PartitionKey = string.Empty;
   string oldPartitionKey = string.Empty;
   string boundaryIdentifier = Guid.NewGuid().ToString();
   string batchIdentifier = Guid.NewGuid().ToString();
   string contentType = string.Format("multipart/mixed; boundary=batch_{0}", boundaryIdentifier);
   azureResults ar = new azureResults();
    DateTime requestDate = DateTime.UtcNow.AddHours(1);
   
   Hashtable headers = new Hashtable();
   headers.Add("x-ms-version", "2009-04-14");
   headers.Add("Content-Type", contentType);
   azureDirect ad = new azureDirect(auth.Account, "", auth.SharedKey, auth.KeyType);
   XmlDocument msgDoc = new XmlDocument();
   
   //Instantiate an XmlNamespaceManager object. 
   System.Xml.XmlNamespaceManager xmlnsManager = new System.Xml.XmlNamespaceManager(msgDoc.NameTable);
   //Add the namespaces used in books.xml to the XmlNamespaceManager.
   xmlnsManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
   xmlnsManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
   msgDoc.LoadXml(entityData);
   XmlNodeList pnodes = msgDoc.SelectNodes("//m:properties", xmlnsManager);
   foreach (XmlNode pnode in pnodes)
   {
    PartitionKey = pnode.SelectSingleNode("d:PartitionKey", xmlnsManager).InnerText;
    string RowKey = pnode.SelectSingleNode("d:RowKey", xmlnsManager).InnerText;
    if (PartitionKey != oldPartitionKey || entityCounter > 98)
    {
     if (sbMultiPart.Length > 0)
     {
      sbMultiPart.AppendFormat("--changeset_{0}--\n--batch_{1}--", batchIdentifier, boundaryIdentifier);
      ar = ad.ProcessRequest(cmdType.post, string.Format("http://{0}.table.core.windows.net/$batch", auth.Account), new System.Text.ASCIIEncoding().GetBytes(sbMultiPart.ToString()), headers);
      sbResults.AppendLine(ar.Body);
     }
     sbMultiPart = new StringBuilder();

     boundaryIdentifier = Guid.NewGuid().ToString();
     batchIdentifier = Guid.NewGuid().ToString();
     contentType = string.Format("multipart/mixed; boundary=batch_{0}", boundaryIdentifier);
     headers["Content-Type"] = contentType;
     sbMultiPart.AppendFormat(batchTemplate, boundaryIdentifier,batchIdentifier );
     sbMultiPart.AppendLine();
     sbMultiPart.AppendLine();
     entityCounter = 1;
     oldPartitionKey = PartitionKey;
    }
    string postUrl =string.Format("http://{0}.table.core.windows.net/{1}", auth.Account,tableName);
    if (cmd != cmdType.post)
     postUrl = string.Format("{0}(PartitionKey='{1}',RowKey='{2}')", postUrl, PartitionKey, RowKey);

    string atomData = string.Format(createEntityXML, requestDate, pnode.OuterXml);

    singlePart = string.Format(entityTemplate, batchIdentifier, cmd.ToString().ToUpper(), postUrl , entityCounter,atomData.Length, atomData );
    sbMultiPart.AppendLine(singlePart);
    entityCounter++;
   }
   sbMultiPart.AppendFormat("--changeset_{0}--\n--batch_{1}--", batchIdentifier, boundaryIdentifier);
   ar = ad.ProcessRequest(cmdType.post, string.Format("http://{0}.table.core.windows.net/$batch", auth.Account), new System.Text.ASCIIEncoding().GetBytes(sbMultiPart.ToString()), headers);
   sbResults.AppendLine(ar.Body);
   return sbResults.ToString();

  }

 


  string entityTemplate = @"--changeset_{0}
Content-Type: application/http
Content-Transfer-Encoding: binary

{1} {2} HTTP/1.1
Content-ID: {3}
Content-Type: application/atom+xml;type=entry
Content-Length: {4}

{5}";
  string createEntityXML = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
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

  string batchTemplate = @"--batch_{0}
Content-Type: multipart/mixed; boundary=changeset_{1}";

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
