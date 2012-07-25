using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;
using System.Configuration;

using System.IO;

namespace Windows.Azure.Storage.Blobs
{
 public enum BlobConnectionModes
 {
  Local,
  Cloud
 }

 public class ConnectionDetails
 {
  public string Account = string.Empty;
  public string Key = string.Empty;
  public string Endpoint = string.Empty;
  public BlobConnectionModes ConnectionMode ;

  public ConnectionDetails(BlobConnectionModes connectionMode)
  {
   ConnectionMode = connectionMode;
  }

  public string GetConnectionURI()
  {
   return String.Format(Endpoint, Account);
  }
 }

 /// <summary>
 /// Blob Request is based on the HttpWebRequest. It provides extra capabilities to handle the REST API for Blob Storage
 /// </summary>
 public class BlobRequest
 {
  private ConnectionDetails connectionDetails;

  public BlobRequest(ConnectionDetails connectionDetails)
  {
   this.connectionDetails = connectionDetails;
  }

  public BlobResponse SetContainerPermissions(string containerName, bool makePublic)
  {
   HttpWebRequest request;

   request = (HttpWebRequest)WebRequest.Create(GetUri(containerName, new string[] { "acl" }));

   request.Method = "PUT";
   request.ContentLength = 0;

   SetDateHeader(request);

   SetContainerACL(request, makePublic);

   SetAuthorizationHeader(request);

   return DispatchRequest(request);
  }

  public BlobResponse CreateContainer(string containerName)
  {
   HttpWebRequest request;

   request = (HttpWebRequest)WebRequest.Create(GetUri(containerName, null));

   request.Method = "PUT";
   request.ContentLength = 0;

   SetDateHeader(request);

   SetAuthorizationHeader(request);

   return DispatchRequest(request);

  }

  public BlobResponse RemoveContainer(string containerName)
  {
   HttpWebRequest request;

   request = (HttpWebRequest)WebRequest.Create(GetUri(containerName, null));

   request.Method = "DELETE";
   request.ContentLength = 0;

   SetDateHeader(request);

   SetAuthorizationHeader(request);

   return DispatchRequest(request);

  }

  public BlobResponse GetContainers()
  {
   HttpWebRequest request;

   request = (HttpWebRequest)WebRequest.Create(GetUri(String.Empty, new string[] { "list" }));

   request.Method = "GET";
   request.ContentLength = 0;

   SetDateHeader(request);

   SetAuthorizationHeader(request);

   return DispatchRequest(request);

  }

  public BlobResponse UploadBlob(string containerName, string blobName, byte[] blobContents)
  {
   HttpWebRequest request;

   request = (HttpWebRequest)WebRequest.Create(GetUri(String.Format("{0}/{1}", containerName, blobName), null));

   request.Method = "PUT";
   request.ContentLength = blobContents.Length;

   SetDateHeader(request);

   SetAuthorizationHeader(request);

   SetBodyContent(request, blobContents);

   return DispatchRequest(request);

  }

  public BlobResponse SetBlobMetadata(string containerName, string blobName, Dictionary<String, String> metadata)
  {
   HttpWebRequest request;

   request = (HttpWebRequest)WebRequest.Create(GetUri(String.Format("{0}/{1}", containerName, blobName), new string[] { "metadata" }));

   request.Method = "PUT";
   request.ContentLength = 0;

   SetDateHeader(request);

   SetMetadataHeaders(request, metadata);

   SetAuthorizationHeader(request);

   return DispatchRequest(request);

  }

  public BlobResponse GetBlobMetadata(string containerName, string blobName)
  {
   HttpWebRequest request;

   request = (HttpWebRequest)WebRequest.Create(GetUri(String.Format("{0}/{1}", containerName, blobName), new string[] { "metadata" }));

   request.Method = "HEAD";
   request.ContentLength = 0;

   SetDateHeader(request);

   SetAuthorizationHeader(request);

   return DispatchRequest(request);

  }

  private void SetMetadataHeaders(HttpWebRequest request, Dictionary<string, string> metadata)
  {
   foreach (KeyValuePair<string, string> metadataItem in metadata)
   {
    request.Headers.Add(String.Format("x-ms-meta-{0}:{1}", metadataItem.Key, metadataItem.Value));
   }
  }

  public BlobResponse GetBlobs(string containerName)
  {
   HttpWebRequest request;

   request = (HttpWebRequest)WebRequest.Create(GetUri(String.Format("{0}", containerName), new string[] { "list" }));

   request.Method = "GET";
   request.ContentLength = 0;

   SetDateHeader(request);

   SetAuthorizationHeader(request);

   return DispatchRequest(request);
  }

  private void SetContainerACL(HttpWebRequest request, bool makePublic)
  {
   request.Headers.Add("x-ms-prop-publicaccess", makePublic.ToString().ToLower());
  }

  private void SetBodyContent(HttpWebRequest request, byte[] blobContents)
  {
   request.GetRequestStream().Write(blobContents, 0, blobContents.Length);
  }


  private string GetUri(string function, string[] comps)
  {
   StringBuilder uri = new StringBuilder();

   uri.Append(this.connectionDetails.GetConnectionURI());
   uri.AppendFormat("{0}", function);

   if (comps != null && comps.Length > 0)
   {
    uri.Append("?");
    for (int i = 0; i < comps.Length; i++)
    {
     uri.AppendFormat("comp={0}", comps[i]);
     if (i < comps.Length - 1)
     {
      uri.Append("&");
     }
    }
   }

   return uri.ToString();
  }

  private BlobResponse DispatchRequest(HttpWebRequest request)
  {
   try
   {
    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
    {
     return new BlobResponse(response);
    }
   }
   catch (WebException ex)
   {
    using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response)
    {
     return new BlobResponse(errorResponse);
    }
   }
  }

  private void SetAuthorizationHeader(HttpWebRequest request)
  {
   // Now sign the request
   // For a blob, you need to use this Canonical form:
   //  VERB + "\n" +
   //  Content - MD5 + "\n" +
   //  Content - Type + "\n" +
   //  Date + "\n" +
   //  CanonicalizedHeaders +
   //  CanonicalizedResource;

   StringBuilder signature = new StringBuilder();

   // Verb
   signature.Append(String.Format("{0}{1}", request.Method, "\n"));

   // Content-MD5 Header
   signature.Append("\n");

   // Content-Type Header
   signature.Append("\n");

   // Then Date, if we have already added the x-ms-date header, leave this null
   signature.Append("\n");

   // Now for CanonicalizedHeaders
   // TODO: Replace with LINQ statement
   foreach (string header in request.Headers)
   {
    if (header.StartsWith("x-ms"))
    {
     signature.Append(String.Format("{0}:{1}\n", header, request.Headers[header]));
    }
   }

   // Now for CanonicalizedResource
   // Format is /{0}/{1} where 0 is name of the account and 1 is resources URI path
   signature.Append(String.Format("/{0}{1}", connectionDetails.Account, request.RequestUri.PathAndQuery));

   // Next, we need to encode our signature using the HMAC-SHA256 algorithm
   byte[] signatureByteForm = System.Text.Encoding.UTF8.GetBytes(signature.ToString());

   System.Security.Cryptography.HMACSHA256 hasher = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(connectionDetails.Key));

   // Now build the Authorization header
   String authHeader = String.Format(CultureInfo.InvariantCulture,
                              "{0} {1}:{2}",
                              "SharedKey",
                              connectionDetails.Account,
                              System.Convert.ToBase64String(hasher.ComputeHash(signatureByteForm)
                              ));

   // And add the Authorization header to the request
   request.Headers.Add("Authorization", authHeader);
  }

  private void SetDateHeader(HttpWebRequest request)
  {
   request.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture));
  }
 }
 public class BlobResponse
 {
  public HttpStatusCode ResponseCode ;

  public string ResponseMessage = null;

  public string ResponseBody = null;

  public WebHeaderCollection Headers = null;

  public BlobResponse(HttpWebResponse response)
  {
   this.ResponseCode = response.StatusCode;
   this.ResponseMessage = response.StatusDescription;
   this.ResponseBody = ReadStream(response.GetResponseStream());
   this.Headers = response.Headers;
  }

  private string ReadStream(Stream body)
  {
   using (System.IO.StreamReader sr = new System.IO.StreamReader(body))
   {
    return sr.ReadToEnd();
   }
  }
 }

}