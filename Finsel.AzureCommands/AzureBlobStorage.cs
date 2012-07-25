using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.IO;

using Amundsen.Utilities; // http://code.google.com/u/mca.amundsen/updates
using System.Xml;
using System.Collections;


namespace Finsel.AzureCommands
{

 /// <summary>
 /// Interface to Microsoft's Azure Blob Storage
 /// </summary>
  public class AzureBlobStorage
  {
    /// <summary>
    /// The type of blob, whether for Streamming or being page writable
    /// </summary>
    public enum BlobType
    {
      /// <summary>
      /// BlockBlob are optimized for streaming
      /// </summary>
      BlockBlob,
      /// <summary>
      /// PageBlob are optimzed for read/write operations
      /// </summary>
      PageBlob
    }


    private Authentication pAuth = new Authentication();
    /// <summary>
    /// Contains the authentication information
    /// </summary>
    public Authentication auth { get { return pAuth; } set { pAuth = value; pAuth.EndPoint = string.Format(EndpointFormat, pAuth.Account); ad.auth = pAuth; ; } }
    azureCommon ac = new azureCommon();
    azureDirect ad = new azureDirect();

    private string EndpointFormat = "http://{0}.blob.core.windows.net/";

    private int maxNonBlockBytes = 1073741824;

    HttpClient client = new HttpClient();
    Hashing h = new Hashing();

    /// <summary>
    /// Defines the UserAgent string passed in when making HTTP requests
    /// </summary>
    string UserAgent = "amundsen-finsel/1.0";

    /// <summary>
    /// Create a new AzureCommands object
    /// </summary>
    public AzureBlobStorage()
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
    public AzureBlobStorage(string account, string endPoint, string sharedKey, string keyType)
    {
      client.UserAgent = UserAgent;
      pAuth = new Authentication(account, string.Format(EndpointFormat, account), sharedKey);
      ad.auth = pAuth;
      client.UserAgent = UserAgent;
    }

    /// <summary>
    ///  Create a new AzureCommands object with an auth object
    /// </summary>
    /// <param name="auth">Azure Authetication Object</param>
    public AzureBlobStorage(Authentication auth)
    {
      pAuth = auth;
      ad.auth = pAuth;
    }

    /// <summary>
    /// Results of request
    /// </summary>
    public azureResults RequestResults = new azureResults();


    /// <summary>
    /// ETag for caching
    /// </summary>
    public string ETag = string.Empty;


    /// <summary>
    /// Data used in POST to ATS
    /// </summary>
    public string PostData = string.Empty;
    private string contentType = "";
    private DateTime requestDate = DateTime.UtcNow;
    private string contentMD5 = string.Empty;
    private string authHeader = string.Empty;
    private string method = string.Empty;



    /// <summary>
    /// Get a list of tables defined for an ATS instance
    /// </summary>
    /// <returns>string array of table names</returns>
    public azureResults GetContainerList(string parameters)
    {
      string listParameters = "comp=list";
      if (parameters != string.Empty && parameters != null)
      {
        if (parameters.StartsWith("?"))
          parameters = parameters.Substring(1);
        if (!parameters.StartsWith("&"))
          parameters = string.Format(CultureInfo.CurrentCulture, "&{0}", parameters);
        listParameters = string.Format(CultureInfo.CurrentCulture, "{0}{1}", listParameters, parameters);
      }
      return Containers(cmdType.get, "", new Hashtable(), listParameters);
    }

    /// <summary>
    /// Containers handles container level operations against Azure Blob Storage
    /// </summary>
    /// <param name="cmd">The type of operation you want to commit: Delete, Post, Put, Get </param>
    /// <param name="containerName">Name of the container to perform the command agains</param>
    /// <param name="headers">A hashtable of Name-Value pairs containing header information</param>
    /// <returns></returns>
    public azureResults Containers(cmdType cmd, string containerName, Hashtable headers)
    {
      return Containers(cmd, containerName, headers, "");
    }




    /// <summary>
    /// Containers handles container level operations against Azure Blob Storage
    /// </summary>
    /// <param name="cmd">The type of operation you want to commit: Delete, Post, Put, Get </param>
    /// <param name="containerName">Name of the container to perform the command agains</param>
    /// <param name="headers">A hashtable of Name-Value pairs containing header information</param>
    /// <param name="parameters">Parameters to add to the URI</param>
    /// <returns></returns>
    public azureResults Containers(cmdType cmd, string containerName, Hashtable headers, string parameters)
    {
      azureResults retVal = new azureResults();
      HttpStatusCode success = HttpStatusCode.NotImplemented;
      try
      {
        StringBuilder sb = new StringBuilder();
        string sendBody = string.Empty;
        string rtnBody = string.Empty;
        string requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}", auth.EndPoint, containerName);
        requestDate = DateTime.UtcNow;

        // handle version 2009-0-17
        if (parameters == string.Empty)
        {
          parameters = "?restype=container";
        }
        else
        {
          if (!parameters.ToLower().Contains("comp=list"))
          {
            parameters = string.Format("{0}&{1}", parameters, "restype=container");
          }
        }

        switch (cmd)
        {
          case cmdType.get:
          case cmdType.head:
            method = cmd.ToString().ToUpper();

            // do GET
            success = HttpStatusCode.OK;
            break;
          case cmdType.delete:
            method = "DELETE";
            // do DELETE
            success = HttpStatusCode.Accepted;
            break;
          case cmdType.put:
            method = "PUT";
            success = HttpStatusCode.Created;
            break;
          default:
            //retVal.StatusCode = HttpStatusCode.NotImplemented;
            break;
        }
        if (parameters != string.Empty && parameters != null)
        {
          if (!parameters.StartsWith("?"))
            parameters = string.Format("?{0}", parameters);
          requestUrl += string.Format(CultureInfo.CurrentCulture, "{0}", parameters);
          retVal.Url = requestUrl;
        }
        retVal = ad.ProcessRequest(cmd, requestUrl, "", headers);
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
    /// Blobs handles blob level operations against Azure Blob Storage
    /// </summary>
    /// <param name="cmd">The type of operation you want to commit: Delete, Post, Put, Get </param>
    /// <param name="containerName">Name of the container where blob exists</param>
    /// <param name="blobName">Name of blob to perform command against</param>
    /// <param name="headers">A hashtable of Name-Value pairs containing header information</param>
    /// <param name="parameters">Parameters to add to the URI</param>
    /// <returns></returns>
    public azureResults Blobs(cmdType cmd, string containerName, string blobName, Hashtable headers, string parameters)
    {
      azureResults retVal = new azureResults();
      HttpStatusCode success = HttpStatusCode.NotImplemented;
      try
      {
        StringBuilder sb = new StringBuilder();
        string sendBody = string.Empty;
        string rtnBody = string.Empty;
        string requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}", auth.EndPoint, containerName, blobName);
        requestDate = DateTime.UtcNow;
        switch (cmd)
        {
          case cmdType.get:
          case cmdType.head:
            method = cmd.ToString().ToUpper();

            // do GET
            success = HttpStatusCode.OK;
            break;
          case cmdType.delete:
            method = "DELETE";
            // do DELETE
            success = HttpStatusCode.Accepted;
            break;
          case cmdType.put:
            method = "PUT";
            success = HttpStatusCode.OK;
            break;
          default:
            //retVal.StatusCode = HttpStatusCode.NotImplemented;
            break;
        }
        if (parameters != string.Empty && parameters != null)
        {
          if (!parameters.StartsWith("?"))
            parameters = string.Format("?{0}", parameters);
          requestUrl += string.Format(CultureInfo.CurrentCulture, "{0}", parameters);
        }
        retVal = ad.ProcessRequest(cmd, requestUrl, "", headers);
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
    /// Defines whether a container is accessible without authentication
    /// </summary>
    /// <param name="containerName">Name of the container</param>
    /// <param name="MakePublic">If true, all blobs in the container will be available through standard http without authentication</param>
    /// <returns>An azureResults showing the results of the request.</returns>
    public azureResults SetContainerAccess(string containerName, bool MakePublic)
    {
      Hashtable ht = new Hashtable();
//      ht.Add("x-ms-prop-publicaccess", (MakePublic ? "True" : "False"));
      if (MakePublic)
        ht.Add("x-ms-blob-public-access", "blob");
      azureResults ar = Containers(cmdType.put, containerName, ht, "?comp=acl");
      ar.Succeeded = (ar.StatusCode == HttpStatusCode.OK);
      return ar;
    }

    /// <summary>
    /// Delete a blob stored in a container.
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name, including paths</param>
    /// <returns>An azureResults showing the results of the request.</returns>
    public azureResults DeleteBlob(string containerName, string blobName)
    {
      azureResults retVal = new azureResults();
      Hashtable ht = new Hashtable();
      ht.Add("If-Match", "*");
      retVal = Blobs(cmdType.delete, containerName, blobName, ht, "");
      retVal.Succeeded = (retVal.StatusCode == HttpStatusCode.OK || retVal.StatusCode == HttpStatusCode.Accepted);
      return retVal;
    }

    /// <summary>
    /// GetBlobList returns a list of the blobs in a container.
    /// </summary>
    /// <param name="containerName">Name of the container</param>
    /// <param name="parameters">Parameters include: prefix, marker and maxresults.</param>
    /// <returns>azureResults object with results of the request.</returns>
    public azureResults GetBlobList(string containerName, string parameters)
    {
      azureResults retVal = new azureResults();
      string bParameters = string.Empty;
      if (!parameters.EndsWith("&") && parameters != string.Empty)
        parameters = parameters + "&";
      parameters = string.Format("{0}restype=container&comp=list", parameters );
      retVal = Blobs(cmdType.get, containerName, "", new Hashtable(), parameters );
      retVal.Succeeded = (retVal.StatusCode == HttpStatusCode.OK);
      return retVal;
    }

    /// <summary>
    /// GetBlob gets a blob from Microsoft's Azure Blob Storage
    /// </summary>
    /// <param name="containerName">Name of the container holding the blob.</param>
    /// <param name="blobName">Name of the blob, can include paths</param>
    /// <param name="Range">Used to get a range of the blob rather than the whole thing.</param>
    /// <returns>byte array containing the blob.</returns>
    public byte[] GetBlob(string containerName, string blobName, string Range)
    {
      azureResults ar = new azureResults();
      return GetBlob(containerName, blobName, Range, ref ar,"");
    }

    /// <summary>
    /// GetBlob gets a blob from Microsoft's Azure Blob Storage
    /// </summary>
    /// <param name="containerName">Name of the container holding the blob.</param>
    /// <param name="blobName">Name of the blob, can include paths</param>
    /// <param name="Range">Used to get a range of the blob rather than the whole thing.</param>
    /// <param name="ar">Passed in by ref, will contain the results of the request.</param>
    /// <returns>byte array containing the blob.</returns>
    public byte[] GetBlob(string containerName, string blobName, string Range, ref azureResults ar)
    {
      return GetBlob(containerName, blobName, Range, ref ar, "");
    }

    /// <summary>
    /// GetBlob gets a blob from Microsoft's Azure Blob Storage
    /// </summary>
    /// <param name="containerName">Name of the container holding the blob.</param>
    /// <param name="blobName">Name of the blob, can include paths</param>
    /// <param name="Range">Used to get a range of the blob rather than the whole thing.</param>
    /// <param name="ar">Passed in by ref, will contain the results of the request.</param>
    /// <param name="ETag">ETag to compare with existing.</param>
    /// <returns>byte array containing the blob.</returns>
    public byte[] GetBlob(string containerName, string blobName, string Range, ref azureResults ar, string ETag)
    {
      MemoryStream retVal = null;
      try
      {
        StringBuilder sb = new StringBuilder();
        string sendBody = string.Empty;
        string rtnBody = string.Empty;
        string requestUrl = string.Empty;
        if (blobName == string.Empty)
          requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}/", auth.EndPoint, containerName);
        else
        {
          if (blobName.StartsWith("/"))
            requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", auth.EndPoint, containerName, blobName);
          else
            requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}", auth.EndPoint, containerName, blobName);
        }
        requestDate = DateTime.UtcNow;

        method = "GET";

        if (Range != string.Empty)
          client.RequestHeaders["range"] = Range;

        // do GET
        if (ETag != string.Empty)
          client.RequestHeaders["If-None-Match"] = ETag;
        client.RequestHeaders["x-ms-date"] = string.Format(CultureInfo.CurrentCulture, "{0:R}", requestDate);
        client.RequestHeaders["x-ms-version"] = "2009-09-19"; // added to support $root calls.
        ar.CanonicalResource = ac.CanonicalizeUrl2(requestUrl);
        authHeader = ac.CreateSharedKeyAuth(method, ar.CanonicalResource, contentMD5, requestDate, client, auth);
        client.RequestHeaders["authorization"] = authHeader;
        client.UseBinaryStream = true;
        client.Execute(requestUrl, method, contentType, "", ref retVal);
        this.ETag = client.ResponseHeaders["etag"];
        ar.Url = requestUrl;
        ar.Body = "";
        ar.StatusCode = client.ResponseStatusCode;
        ar.Headers = ac.Headers2Hash(client.ResponseHeaders);
        if (ETag != string.Empty && ar.StatusCode == HttpStatusCode.NotModified)
          ar.Succeeded = true;
        else
          ar.Succeeded = (ar.StatusCode == HttpStatusCode.OK);
      }
      catch (HttpException hex)
      {
        ar.StatusCode = (HttpStatusCode)hex.GetHttpCode();
        ar.Succeeded = false;
        ar.Body = hex.GetHtmlErrorMessage();
      }
      catch (Exception ex)
      {
        ar.StatusCode = HttpStatusCode.SeeOther;
        ar.Body = ex.ToString();
        ar.Succeeded = false;
      }
      if (retVal != null)
        return retVal.ToArray();// retVal.ToArray();
      else return null;
    }

    /// <summary>
    /// Make a head call to determine if the ETag matches
    /// </summary>
    /// <param name="containerName">Name of the container holding the blob</param>
    /// <param name="blobName">Name of the blob, can include paths</param>
    /// <param name="ETag">ETag to check</param>
    /// <returns></returns>
    public azureResults  CheckBlobCache(string containerName, string blobName, string ETag)
    {
      azureResults ar = new azureResults();
      try
      {
        StringBuilder sb = new StringBuilder();
        string sendBody = string.Empty;
        string rtnBody = string.Empty;
        string requestUrl = string.Empty;
        if (blobName == string.Empty)
          requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}/", auth.EndPoint, containerName);
        else
        {
          if (blobName.StartsWith("/"))
            requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", auth.EndPoint, containerName, blobName);
          else
            requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}", auth.EndPoint, containerName, blobName);
        }
        requestDate = DateTime.UtcNow;
        method = "HEAD";

        // do GET
        client.RequestHeaders["If-Modified"] = ETag;
        client.RequestHeaders["x-ms-date"] = string.Format(CultureInfo.CurrentCulture, "{0:R}", requestDate);
        ar.CanonicalResource = ac.CanonicalizeUrl2(requestUrl);
        authHeader = ac.CreateSharedKeyAuth(method, ar.CanonicalResource, contentMD5, requestDate, client, auth);
        client.RequestHeaders["authorization"] = authHeader;
        client.UseBinaryStream = true;
        client.Execute(requestUrl, method, contentType, "");
       // this.ETag = client.ResponseHeaders["etag"];
        ar.Url = requestUrl;
        ar.Body = "";
        if (client.ResponseHeaders["etag"] == ETag)
          ar.StatusCode = HttpStatusCode.NotModified;
        else ar.StatusCode = HttpStatusCode.PreconditionFailed;
        ar.Headers = ac.Headers2Hash(client.ResponseHeaders);
        ar.Succeeded = (ar.StatusCode == HttpStatusCode.NotModified);
      }
      catch (HttpException hex)
      {
        ar.StatusCode = (HttpStatusCode)hex.GetHttpCode();
        ar.Succeeded = false;
        ar.Body = hex.GetHtmlErrorMessage();
      }
      catch (Exception ex)
      {
        ar.StatusCode = HttpStatusCode.SeeOther;
        ar.Body = ex.ToString();
        ar.Succeeded = false;
      }
      return ar;
    }

    /// <summary>
    /// PutBlob creates/updates a blob within Microsoft's Azure Blob Storage
    /// </summary>
    /// <param name="ContentLength">How many bytes are in the Content</param>
    /// <param name="ContentType">Content type of the blob. application/bin is used by default if nothing else is passed in</param>
    /// <param name="Content">A byte array representing the blob being stored.</param>
    /// <param name="containerName">The name of the container to store the blob in.</param>
    /// <param name="blobName">The name of the blob. Can inlcude paths (/path/blob.txt, for instance)</param>
    /// <param name="htMetaData">A hashtable containing the Name-Value pairs of MetaData.</param>
    /// <returns></returns>
    public azureResults PutBlob(Int64 ContentLength, string ContentType, byte[] Content, string containerName, string blobName, Hashtable htMetaData)
    {
      return PutBlob(ContentLength, ContentType,  Content, containerName, blobName, htMetaData, BlobType.BlockBlob );
    }



    /// <summary>
    /// PutBlob creates/updates a blob within Microsoft's Azure Blob Storage
    /// </summary>
    /// <param name="ContentLength">How many bytes are in the Content</param>
    /// <param name="ContentType">Content type of the blob. application/bin is used by default if nothing else is passed in</param>
    /// <param name="Content">A byte array representing the blob being stored.</param>
    /// <param name="containerName">The name of the container to store the blob in.</param>
    /// <param name="blobName">The name of the blob. Can inlcude paths (/path/blob.txt, for instance)</param>
    /// <param name="htMetaData">A hashtable containing the Name-Value pairs of MetaData.</param>
    /// <param name="typeOfBlob">Whether this is a Block or Page type of blob</param>
    /// <returns></returns>
    public azureResults PutBlob(Int64 ContentLength, string ContentType, byte[] Content, string containerName, string blobName, Hashtable htMetaData, BlobType typeOfBlob)
    {
      azureResults retVal = new azureResults();
      if (ContentLength < maxNonBlockBytes && typeOfBlob == BlobType.BlockBlob)
      {
          retVal = putBlockBlob(ContentLength, ContentType, Content, containerName, blobName, htMetaData);
      }
      else
      {
          retVal = putPageBlob(ContentLength, ContentType, Content, containerName, blobName, htMetaData);
      }

      return retVal;
    }

    private azureResults putPageBlob(Int64 ContentLength, string ContentType, byte[] Content, string containerName, string blobName, Hashtable htMetaData)
    {
        azureResults retVal = new azureResults();
        try
        {
            // first we call put blob to initialize the blob
            Int64 boundaryContentLength = 0; 
            StringBuilder sb = new StringBuilder();
            string sendBody = string.Empty;
            string rtnBody = string.Empty;
            string requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}", auth.EndPoint, containerName, blobName);
            requestDate = DateTime.UtcNow;
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
                req.Method = "PUT";
                req.ContentType = ContentType;
                req.ContentLength = 0;
                if (htMetaData.Count > 0)
                    foreach (DictionaryEntry item in htMetaData)
                    {
                        string metaDataName = item.Key.ToString().ToLower().Replace(" ", "").Replace("\r", "");
                        if (!metaDataName.StartsWith("x-ms-meta-"))
                            metaDataName = "x-ms-meta-" + metaDataName;
                        try
                        {
                            if (item.Value.ToString().Trim() != string.Empty)
                                req.Headers[metaDataName] = item.Value.ToString();
                        }
                        catch (Exception ex) { }
                    }
                req.Headers["x-ms-blob-type"] = "PageBlob";
                req.Headers["x-ms-blob-sequence-number"] = "0";
                
                if (ContentLength % 512 == 0)
                    boundaryContentLength = ContentLength;
                else
                    boundaryContentLength = ((ContentLength / 512) * 512) + 512;
                req.Headers["x-ms-blob-content-length"] = boundaryContentLength.ToString();
                req.Headers["x-ms-date"] = string.Format(CultureInfo.CurrentCulture, "{0:R}", requestDate);
                req.Headers["x-ms-version"] = "2012-02-12";
                retVal.CanonicalResource = ac.CanonicalizeUrl2(requestUrl);
                authHeader = ac.CreateSharedKeyAuth(req.Method, retVal.CanonicalResource, contentMD5, requestDate, req, auth);
                req.Headers["authorization"] = authHeader;

                Stream requestStream = null;// req.GetRequestStream(); 
                
                //requestStream.Write(new byte[0], 0, 0);
                //requestStream.Flush();
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                response.Close();

                retVal.Url = requestUrl;
                retVal.Body = "";
                retVal.StatusCode = response.StatusCode;
                retVal.Headers = ac.Headers2Hash(response.Headers);
                retVal.Succeeded = (retVal.StatusCode == HttpStatusCode.Created);
                if (retVal.Succeeded) // we have an initialized blob, now we need to post it
                {
                    Int64 blockSize = 65536;
                    Int64 currentBlock = 0;
                    // now we need to load the blob 

                    while ((currentBlock * blockSize) < boundaryContentLength)
                    {

                        sendBody = string.Empty;
                        rtnBody = string.Empty;
                        requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}?comp=page", auth.EndPoint, containerName, blobName);
                        req = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
                        req.Method = "PUT";
                        req.ContentType = ContentType;
                        Int64 currentBlockSize = (boundaryContentLength - (currentBlock * blockSize) > blockSize ? blockSize : boundaryContentLength - (currentBlock * blockSize));
                        Int64 lowByte = (currentBlock) * blockSize;
                        Int64 highByte = (currentBlock) * blockSize + currentBlockSize - 1;
                        req.Headers["x-ms-range"] = string.Format("bytes={0}-{1}", lowByte, highByte);
                        req.ContentLength = currentBlockSize;
                        req.Headers["x-ms-date"] = string.Format(CultureInfo.CurrentCulture, "{0:R}", requestDate);
                        req.Headers["x-ms-version"] = "2012-02-12";
                        req.Headers["x-ms-blob-type"] = "PageBlob";
                        req.Headers["x-ms-page-write"] = "update";
                        retVal.CanonicalResource = ac.CanonicalizeUrl2(requestUrl);
                        authHeader = ac.CreateSharedKeyAuth(req.Method, retVal.CanonicalResource, contentMD5, requestDate, req, auth);
                        req.Headers["authorization"] = authHeader;

                        requestStream = req.GetRequestStream();
                        byte[] contentBlock = new byte[blockSize];
                        if(currentBlockSize < blockSize)
                            Buffer.BlockCopy(Content, (int)lowByte, contentBlock, 0, (int)(ContentLength - (currentBlock * blockSize)) );
                        else
                            Buffer.BlockCopy(Content, (int)lowByte , contentBlock, 0,(int)currentBlockSize);

                        requestStream.Write(contentBlock, 0, (int)currentBlockSize);
                        requestStream.Flush();
                        response = (HttpWebResponse)req.GetResponse();
                        response.Close();

                        retVal.Url = requestUrl;
                        retVal.Body = "";
                        retVal.StatusCode = response.StatusCode;
                        retVal.Headers = ac.Headers2Hash(response.Headers);
                        retVal.Succeeded = (retVal.StatusCode == HttpStatusCode.Created);
                        currentBlock++;
                    }
                }
            }
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
        finally
        {

        }
        return retVal;
    }
    private azureResults putBlockBlob(Int64 ContentLength, string ContentType, byte[] Content, string containerName, string blobName, Hashtable htMetaData)
    {
        azureResults retVal = new azureResults();
        try
        {
            StringBuilder sb = new StringBuilder();
            string sendBody = string.Empty;
            string rtnBody = string.Empty;
            string requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}", auth.EndPoint, containerName, blobName);
            requestDate = DateTime.UtcNow;
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
                req.Method = "PUT";
                req.ContentType = ContentType;
                req.ContentLength = ContentLength;
                if (htMetaData.Count > 0)
                    foreach (DictionaryEntry item in htMetaData)
                    {
                        string metaDataName = item.Key.ToString().ToLower().Replace(" ", "").Replace("\r", "");
                        if (!metaDataName.StartsWith("x-ms-meta-"))
                            metaDataName = "x-ms-meta-" + metaDataName;
                        try
                        {
                            if (item.Value.ToString().Trim() != string.Empty)
                                req.Headers[metaDataName] = item.Value.ToString();
                        }
                        catch (Exception ex) { }
                    }
                req.Headers["x-ms-date"] = string.Format(CultureInfo.CurrentCulture, "{0:R}", requestDate);
                req.Headers["x-ms-version"] = "2012-02-12";
                req.Headers["x-ms-blob-type"] = "BlockBlob";
                retVal.CanonicalResource = ac.CanonicalizeUrl2(requestUrl);
                authHeader = ac.CreateSharedKeyAuth(req.Method, retVal.CanonicalResource, contentMD5, requestDate, req, auth);
                req.Headers["authorization"] = authHeader;

                Stream requestStream = req.GetRequestStream();
                requestStream.Write(Content, 0, (int)ContentLength);
                requestStream.Flush();
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                response.Close();

                retVal.Url = requestUrl;
                retVal.Body = "";
                retVal.StatusCode = response.StatusCode;
                retVal.Headers = ac.Headers2Hash(response.Headers);
                retVal.Succeeded = (retVal.StatusCode == HttpStatusCode.Created);
            }
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
        finally
        {

        }
        return retVal;
    }

    /// <summary>
    /// PutBlob creates/updates a blob within Microsoft's Azure Blob Storage
    /// </summary>
    /// <param name="ContentLength">How many bytes are in the Content</param>
    /// <param name="ContentType">Content type of the blob. application/bin is used by default if nothing else is passed in</param>
    /// <param name="Content">A byte array representing the blob being stored.</param>
    /// <param name="containerName">The name of the container to store the blob in.</param>
    /// <param name="blobName">The name of the blob. Can inlcude paths (/path/blob.txt, for instance)</param>
    /// <param name="htMetaData">A hashtable containing the Name-Value pairs of MetaData.</param>
    /// <returns></returns>
    public azureResults PutBlobWithBlocks(string ContentType, byte[] Content, string containerName, string blobName, Hashtable htMetaData, int BlockLength, int StartBlock)
    {
      azureResults retVal = new azureResults();
      try
      {
        StringBuilder sb = new StringBuilder();
        string sendBody = string.Empty;
        string rtnBody = string.Empty;

        string requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}", auth.EndPoint, containerName, blobName);
        requestDate = DateTime.UtcNow;

        string blockURI = string.Empty;
        Hashtable htHeaders = htMetaData;
        htHeaders.Add("Content-Type", ContentType);
        azureDirect ad = new azureDirect(auth.Account, auth.EndPoint, auth.SharedKey, auth.KeyType);
        int blocksCount = (int)Math.Ceiling((double)Content.Length / BlockLength);
        string[] blockIds = new string[blocksCount];
        int startPosition = StartBlock * BlockLength;
        int blockIdPosition = 0;
        for (int i = StartBlock; i < blocksCount; i++)
        {
          blockIds[blockIdPosition] = Convert.ToBase64String(BitConverter.GetBytes(i));
          blockURI = string.Format("{0}?comp=block&blockid={1}", requestUrl, blockIds[blockIdPosition]);
          blockIdPosition++;
          byte[] blockContent = new byte[BlockLength];
          Array.Copy(Content, startPosition, blockContent, 0, (startPosition + BlockLength > Content.Length ? Content.Length - startPosition : BlockLength));
          retVal = ad.ProcessRequest(cmdType.put, blockURI, blockContent, htHeaders);
          while (retVal.StatusCode != HttpStatusCode.Created)
            retVal = ad.ProcessRequest(cmdType.put, blockURI, blockContent, htHeaders);
          startPosition += BlockLength;
          Console.WriteLine(i);
        }
        blockURI = string.Format("{0}?comp=blocklist", requestUrl);
        StringBuilder sbBlockList = new StringBuilder();
        sbBlockList.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<BlockList>");
        foreach (string id in blockIds)
        {
          sbBlockList.AppendFormat("<Block>{0}</Block>\n", id);
        }
        sbBlockList.Append("</BlockList>");
        retVal = ad.ProcessRequest(cmdType.put, blockURI, new System.Text.ASCIIEncoding().GetBytes(sbBlockList.ToString()), htHeaders);

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

    public azureResults CopyBlob(string OriginalBlobUrl, string NewBlobUrl, Hashtable htHeaders)
    {
      if (htHeaders.ContainsKey("x-ms-copy-source"))
        htHeaders["x-ms-copy-source"] = new azureCommon().CanonicalizeUrl(OriginalBlobUrl);
      else
        htHeaders.Add("x-ms-copy-source", new azureCommon().CanonicalizeUrl(OriginalBlobUrl));
      azureResults retVal = new azureResults();
      azureDirect ad = new azureDirect(auth);
      retVal = ad.ProcessRequest(cmdType.put, NewBlobUrl, "", htHeaders);
      return retVal;
    }

    /// <summary>
    /// Metadata is a quick way to get/set the metadata about a container or a blob
    /// </summary>
    /// <param name="cmd">The type of command you want to execute. GET, PUT and DELETE are supported</param>
    /// <param name="containerName">Name of the container. If no blobName is passed in, will apply Metadata
    /// command to the Container.</param>
    /// <param name="blobName">The name of the blob in the container, if you want to apply the Metadata command to a blob.</param>
    /// <param name="htMetaData">A hashtable containing the Name-Value pairs of MetaData.</param>
    /// <returns>An azureResults showing the results of the request.</returns>
    public azureResults MetaData(cmdType cmd, string containerName, string blobName, Hashtable htMetaData)
    {
      azureResults retVal = new azureResults();
      Hashtable headers = new Hashtable();
      if (htMetaData != null)
        if (htMetaData.Count > 0)
          foreach (DictionaryEntry item in htMetaData)
          {
            string metaDataName = item.Key.ToString().Replace(" ", "-").Replace("\r", "");
            if (!metaDataName.StartsWith("x-ms-meta-"))
              metaDataName = "x-ms-meta-" + metaDataName;
            headers.Add(ac.CleanMetaDataNames(metaDataName), item.Value);
          }
      if (blobName == string.Empty)
      {
        retVal = Containers(cmd, containerName, headers, "?comp=metadata");
        retVal.Succeeded = (retVal.StatusCode == HttpStatusCode.OK);
      }
      else
      {
        retVal = Blobs(cmd, containerName, blobName, headers, "?comp=metadata");
      }
      return retVal;
    }


    /// <summary>
    /// The types of actions that can be taken when leasing a blob 
    /// </summary>
    public enum BlobLease
    {
      /// <summary>
      /// Acquire a new lease
      /// </summary>
      acquireLease,
      /// <summary>
      /// Release an existing lease
      /// </summary>
      releaseLease,
      /// <summary>
      /// renew an existing lease
      /// </summary>
      renewLease,
      /// <summary>
      /// break an existing lease
      /// </summary>
      breakLease
    }


    /// <summary>
    /// Lease a blob stored in a container.
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="blobName">Blob name, including paths</param>
    /// <returns>An azureResults showing the results of the request.</returns>
    public azureResults LeaseBlob(string containerName, string blobName, BlobLease leaseType, string leaseGuid)
    {
      //if (leaseGuid.Trim() == string.Empty)
      //  leaseGuid = Guid.NewGuid().ToString();
      azureResults retVal = new azureResults();
      Hashtable ht = new Hashtable();
      ht.Add("If-Match", "*");
      string leaseTypeString = string.Empty;
      switch (leaseType)
      {
        case BlobLease.acquireLease: leaseTypeString = "acquire"; break;
        case BlobLease.breakLease: leaseTypeString = "break"; break;
        case BlobLease.releaseLease: leaseTypeString = "release"; break;
        case BlobLease.renewLease: leaseTypeString = "renew"; break;
      }
      ht.Add("x-ms-lease-action", leaseTypeString );
      if (leaseGuid.Trim() != string.Empty)

        ht.Add("x-ms-lease-id", leaseGuid);
      retVal = Blobs(cmdType.put , containerName, blobName, ht, "comp=lease");
      retVal.Succeeded = (retVal.StatusCode == HttpStatusCode.OK || retVal.StatusCode == HttpStatusCode.Accepted || retVal.StatusCode == HttpStatusCode.Created );
      return retVal;
    }

    public azureResults SnapshotBlob(string containerName, string blobName)
    {
      azureResults retVal = new azureResults();
      Hashtable headers = new Hashtable();
      retVal = Blobs(cmdType.put, containerName, blobName, headers, "?comp=snapshot");
      if (retVal.StatusCode == HttpStatusCode.Created)
        retVal.Succeeded = true;
      return retVal;
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
  }
}