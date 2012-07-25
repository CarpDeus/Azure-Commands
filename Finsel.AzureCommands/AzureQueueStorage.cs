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
  /// For interfacing with Microsoft's Azure Queue Storage
  /// </summary>
  public class AzureQueueStorage
  {
    azureDirect ad = new azureDirect();
    azureCommon ac = new azureCommon();
    /// <summary>
    /// Create a new AzureCommands object
    /// </summary>
    public AzureQueueStorage()
    {
    }

    /// <summary>
    /// Create a new AzureCommands object with default settings
    /// </summary>
    /// <param name="account">The account is generally the first part of the Endpoint.</param>
    /// <param name="endPoint">The Endpoint for Azure Table Storage as defined for your account</param>
    /// <param name="sharedKey">The SharedKey for access</param>
    /// <param name="keyType">Shared</param>
    public AzureQueueStorage(string account, string endPoint, string sharedKey, string keyType)
    {

      pAuth = new Authentication(account, string.Format(EndpointFormat, account), sharedKey);

      ad = new azureDirect(pAuth);
    }

    private Authentication pAuth = new Authentication();

    /// <summary>
    /// The authentication object for the Azure Table Storage
    /// </summary>
   public Authentication auth { get { return pAuth; } set { pAuth = value; pAuth.EndPoint = string.Format(EndpointFormat, pAuth.Account); ad.auth = pAuth; } }

    private string EndpointFormat = "http://{0}.queue.core.windows.net/";


    /// <summary>
    /// Results of request
    /// </summary>
    public azureResults RequestResults = new azureResults();


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
    /// Get a list of queues defined for an ATS instance
    /// </summary>
    /// <returns>string array of table names</returns>
    public azureResults GetQueueList(string parameters)
    {
      return Queues(cmdType.get, "", parameters, new Hashtable());
    }

    /// <summary>
    /// Queues handles queue level operations against Azure Queue Storage
    /// </summary>
    /// <param name="cmd">The type of operation you want to commit: Delete, Post, Put, Get </param>
    /// <param name="queueName">Name of the table to perform the command agains</param>
    /// <param name="parameters">Uri Parameters to be included</param>
    /// <param name="htMetaData">Hashtable of Name-Value pairs of metadata for the queue</param>
    /// <returns>An azureResults showing the results of the request.</returns>
    public azureResults Queues(cmdType cmd, string queueName, string parameters, Hashtable htMetaData)
    {
      azureResults retVal = new azureResults();
      Hashtable headers = new Hashtable();

      try
      {
        if (parameters == string.Empty && !queueName.EndsWith("/") && queueName != string.Empty)
          queueName = queueName + "/";
        StringBuilder sb = new StringBuilder();
        string sendBody = string.Empty;
        string rtnBody = string.Empty;
        string requestUrl = string.Format(CultureInfo.CurrentCulture, auth.EndPoint);
        requestDate = DateTime.UtcNow;
        HttpStatusCode success = HttpStatusCode.NotImplemented;
        switch (cmd)
        {
          case cmdType.get:
            method = "GET";
            if (queueName == string.Empty)
            {
              requestUrl += string.Format(CultureInfo.CurrentCulture, "?comp=list");
            }
            if (parameters != string.Empty)
            {
              if (!parameters.StartsWith("?"))
                parameters = "?" + parameters;
              requestUrl += string.Format(CultureInfo.CurrentCulture, "{0}", parameters);
            }
            success = HttpStatusCode.OK;

            break;
          case cmdType.delete:
            method = "DELETE";
            requestUrl += string.Format(CultureInfo.CurrentCulture, string.Format("{0}", queueName));
            success = HttpStatusCode.NoContent;

            break;
          case cmdType.put:
            method = "PUT";
            requestUrl += string.Format(CultureInfo.CurrentCulture, string.Format("{0}", queueName));
            // do PUT
            if (htMetaData.Count > 0)
              foreach (DictionaryEntry item in htMetaData)
              {
                string metaDataName = item.Key.ToString().ToLower().Replace(" ", "-").Replace("\r", "");

                if (!metaDataName.StartsWith("x-ms-meta-"))
                  metaDataName = "x-ms-meta-" + metaDataName;

                try
                {
                  if (item.Value.ToString().Trim() != string.Empty)
                    headers.Add(ac.CleanMetaDataNames(metaDataName), item.Value.ToString());
                }
                catch
                {

                }
              }
            success = HttpStatusCode.Created;
            break;
          default:
            break;
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
    /// Execute requests on Azure Queue Messages
    /// </summary>
    /// <param name="cmd">Type of command: Get, Delete, Post are supported</param>
    /// <param name="queueName">Name of the queue</param>
    /// <param name="messageBody">Body of the message. Max is 8KB</param>
    /// <param name="parameters">Uri Parameters to be included</param>
    /// <param name="messageID">MessageID</param>
    /// <returns>An azureResults object with the results of the request.</returns>
    public azureResults Messages(cmdType cmd, string queueName, string messageBody, string parameters, string messageID)
    {
      azureResults retVal = new azureResults();
      HttpStatusCode success = HttpStatusCode.NotImplemented;
      Hashtable headers = new Hashtable();
      try
      {
        string requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}/messages", auth.EndPoint, queueName);
        if (messageID != string.Empty)
        {
          requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}/{1}", requestUrl, messageID);
        }
        if (parameters != string.Empty && parameters != null)
        {
          if (!parameters.StartsWith("?"))
            parameters = string.Format("?{0}", parameters);
          requestUrl += string.Format(CultureInfo.CurrentCulture, "{0}", parameters);
        }
        requestDate = DateTime.UtcNow;
        switch (cmd)
        {
          case cmdType.get:
            method = "GET";
            // do GET
            success = HttpStatusCode.OK;
            break;
          case cmdType.delete:
            method = "DELETE";
            // do DELETE
            success = HttpStatusCode.NoContent;

            break;
          case cmdType.post:
            method = "POST";
            messageBody = string.Format("<QueueMessage><MessageText><![CDATA[{0}]]></MessageText></QueueMessage>", messageBody);
            success = HttpStatusCode.Created;
            break;
          default:
            retVal.StatusCode = HttpStatusCode.NotImplemented;
            break;
        }
        headers.Add("Content-Type", contentType);
        retVal = ad.ProcessRequest(cmd, requestUrl, messageBody, headers);
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
    /// Metadata is a quick way to get/set the metadata about a queue
    /// </summary>
    /// <param name="cmd">The type of command you want to execute. GET, PUT and DELETE are supported</param>
    /// <param name="queueName">Name of the queue</param>
    /// <param name="htMetaData">A hashtable containing the Name-Value pairs of MetaData.</param>
    /// <returns>An azureResults showing the results of the request.</returns>
    public azureResults MetaData(cmdType cmd, string queueName, Hashtable htMetaData)
    {
      azureResults retVal = new azureResults();
      Hashtable headers = new Hashtable();
      HttpStatusCode success = HttpStatusCode.NotImplemented;
      try
      {
        StringBuilder sb = new StringBuilder();
        string requestUrl = string.Format(CultureInfo.CurrentCulture, "{0}{1}", auth.EndPoint, queueName);
        requestUrl += string.Format(CultureInfo.CurrentCulture, "?comp=metadata");
        requestDate = DateTime.UtcNow;

        switch (cmd)
        {
          case cmdType.get:
          case cmdType.head:
            method = cmd.ToString().ToUpper();
            StringBuilder metaDataInformation = new StringBuilder();
            success = HttpStatusCode.OK;
            break;
          case cmdType.put:
            method = "PUT";
            foreach (DictionaryEntry item in htMetaData)
            {
              string metaDataName = item.Key.ToString().ToLower().Replace(" ", "-").Replace("\r", "");
              if (!metaDataName.StartsWith("x-ms-meta-"))
                metaDataName = "x-ms-meta-" + metaDataName;
              try
              {
                if (item.Value.ToString().Trim() != string.Empty)
                  //client.RequestHeaders[metaDataName] = item.Value.ToString();
                  headers.Add(ac.CleanMetaDataNames(metaDataName), item.Value.ToString());
              }
              catch
              {
              }
            }
            success = HttpStatusCode.NoContent;
            break;
          default:
            retVal.StatusCode = HttpStatusCode.NotImplemented;
            break;
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