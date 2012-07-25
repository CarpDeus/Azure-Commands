using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.Specialized;
using System.Text.RegularExpressions;

using System.IO;
using Finsel.AzureCommands;
using System.Xml;

using Microsoft.Win32;

namespace AzureBlobRetriever
{
 class Program
 {
  static void Main(string[] args)
  {
   Arguments CommandLine = new Arguments(args);
   bool DisplayHelp = false;
   if (CommandLine["?"] != null)
    DisplayHelp = true;
   string ErrorMessage = "";

   string EndPoint = (CommandLine["endpoint"] == null ? CommandLine["e"] : CommandLine["endpoint"]);
   if (EndPoint == null)
   {
    EndPoint = Properties.Settings.Default.Endpoint;
    if (EndPoint == string.Empty) EndPoint = null;
   }
   string SharedKey = (CommandLine["sharedkey"] == null ? CommandLine["s"] : CommandLine["sharedkey"]);
   if (SharedKey == null)
   {
    SharedKey = Properties.Settings.Default.SharedKey;
    if (SharedKey == string.Empty) SharedKey = null;
   }
   string Account = (CommandLine["account"] == null ? CommandLine["a"] : CommandLine["account"]);
   if (Account == null)
   {
    Account = Properties.Settings.Default.Account;
    if (Account == string.Empty) Account = null;
   }

   
   string Container = (CommandLine["container"] == null ? CommandLine["c"] : CommandLine["container"]);
   string StartingLocation = (CommandLine["to"] == null ? CommandLine["t"] : CommandLine["to"]);
   if (StartingLocation == null)
    StartingLocation = System.Environment.CurrentDirectory;
   bool directoryStructure = (CommandLine["directory"] != null);
   bool deleteAfterRetrieval = (CommandLine["delete"] != null);
   if (!DisplayHelp && Container == null)
   {
    ErrorMessage = ErrorMessage + "Container is REQUIRED! Use * to get all containers.\r\n";
   }

   if (!DisplayHelp && EndPoint == null)
   {
    ErrorMessage = ErrorMessage + "Endpoint is REQUIRED!\r\n";
   }
   if (!DisplayHelp && Account == null)
   {
    ErrorMessage = ErrorMessage + "Account is REQUIRED!\r\n";
   }
   if (!DisplayHelp && SharedKey == null)
   {
    ErrorMessage = ErrorMessage + "SharedKey is REQUIRED!\r\n";
   }

   if (DisplayHelp || ErrorMessage != string.Empty)
   {

    /// <summary>
    System.Version AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

    Console.Clear();
    if (ErrorMessage != string.Empty)
     Console.WriteLine(ErrorMessage);
    Console.WriteLine("{4}\tVersion: {0}.{1}.{2}.{3}", AppVersion.Major.ToString(), AppVersion.Minor.ToString(),
     AppVersion.Build.ToString(), AppVersion.Revision.ToString(), System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name);
    BlobRetriever bl = new BlobRetriever();
    Console.WriteLine(bl.BuildInfo());
    Console.Write("\r\n\r\n{0}", System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name);

    Console.Write(" -? -s[haredkey]:AzureSharedKey -e[ndpoint]:AzureEndPoint -a[ccount]:AzureAccount ");
    Console.WriteLine("\t[-c[ontainer]]:BlobContainer -t[o]:SaveDirectory -directory -delete");
    Console.WriteLine("\t-? shows this message");
    Console.WriteLine("\t-s[haredkey] your Azure Shared Key");
    Console.WriteLine("\t-e[ndpoint]  your Azure Endpoint");
    Console.WriteLine("\t-a[ccount]   your Azure Account");
    Console.WriteLine("\t-c[ontainer] the Container where the blobs are stored.");
    Console.WriteLine("\t\t Use * to download from all containers.");
    Console.WriteLine("\t-directory Use blob names as directories");
    Console.WriteLine("\t-t[o] the directory to store the blobs");
    Console.WriteLine("\t\t if not included, will be where the application was run from");
    Console.WriteLine("\t-delete Delete the blob after retrieval");
     
    
   }
   else
   {
    BlobRetriever br1 = new BlobRetriever();
    br1.Account = Account;
    br1.Container = Container;
    br1.Directories = directoryStructure;
    br1.StartingDirectory = StartingLocation;
    br1.EndPoint = EndPoint;
    br1.SharedKey = SharedKey;
    br1.deleteAfterRetrieval = deleteAfterRetrieval;
    br1.RetrieveBlobs();
   }

  }
 }

 class BlobRetriever
 {
  public string Account = string.Empty;
  public string SharedKey = string.Empty;
  public string Container = string.Empty;
  public string EndPoint = string.Empty;
  public string StartingDirectory = string.Empty;
  public bool Directories = false;
  public bool deleteAfterRetrieval = false;
  public BlobRetriever()
  { }

  /// <summary>
  /// Get the BuildInformation for revisions
  /// </summary>
  /// <returns></returns>
  public string BuildInfo()
  {
   System.Version AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
   return (string.Format("{4}\tVersion: {0}.{1}.{2}.{3}", AppVersion.Major.ToString(), AppVersion.Minor.ToString(),
     AppVersion.Build.ToString(), AppVersion.Revision.ToString(), System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name));
  }


  public void RetrieveBlobs()
  {
   System.Collections.Hashtable metadata = new System.Collections.Hashtable();
   AzureBlobStorage abs = new AzureBlobStorage(Account, EndPoint, SharedKey, "SharedKey");
   if (Container == "*")
   {
    azureResults ar = abs.GetContainerList("");
    XmlDocument xdoc = new XmlDocument();
    xdoc.LoadXml(ar.Body);
    XmlNodeList xnl = xdoc.SelectNodes("//Name");
    foreach (XmlNode xn in xnl)
    {
     ProcessContainer(abs, metadata, StartingDirectory,  xn.InnerText);
    }
   }
   else
   {
    azureResults ar = abs.Containers(cmdType.get, Container, new System.Collections.Hashtable());
    if (!ar.Succeeded)
     ar = abs.Containers(cmdType.post, Container, metadata);
    ProcessContainer(abs, metadata, StartingDirectory, Container);
   }
  }

 
  void ProcessContainer(AzureBlobStorage abs, System.Collections.Hashtable metadata, string DirectoryPath, string ContainerName)
  {
   string baseDirectory = DirectoryPath + (DirectoryPath.EndsWith(@"\") ? "" : @"\") + ContainerName;
   if (!Directory.Exists(baseDirectory))
    Directory.CreateDirectory(baseDirectory);
   if(! baseDirectory.EndsWith(@"\"))
    baseDirectory = baseDirectory + @"\";

   azureResults arList = abs.GetBlobList(ContainerName, "");
   XmlDocument xdoc = new XmlDocument();
   xdoc.LoadXml(arList.Body);
   XmlNode xContainerInfo = xdoc.SelectSingleNode("//EnumerationResults [1]");
   string containerStart = xContainerInfo.Attributes["ContainerName"].Value;
   XmlNodeList xnl = xdoc.SelectNodes("//Url");
   
   foreach (XmlNode blobNode in xnl)
   {
    // get the relative path of the blob
    string baseName = blobNode.InnerText.Replace(baseDirectory, "");
    // get directory
    string relativeDirectory = baseName.Replace(containerStart,"");
    if(relativeDirectory.Contains("/"))
     relativeDirectory = relativeDirectory.Replace("/", @"\");
    // Get filename
    string fileName = blobNode.InnerText.Substring(blobNode.InnerText.LastIndexOf("/") + 1);
    relativeDirectory = relativeDirectory.Replace(fileName, "");
    fileName = baseDirectory + relativeDirectory + fileName;
    if (!Directory.Exists(baseDirectory + relativeDirectory))
     Directory.CreateDirectory(baseDirectory + relativeDirectory);
    azureResults arBlob = new azureResults();
    byte[] fileBytes = abs.GetBlob(ContainerName, blobNode.InnerText.Replace(containerStart,"") , "", ref arBlob);
    if (fileBytes == null)
     Console.WriteLine("Skipped {0}: 0 bytes", fileName);
    else
     File.WriteAllBytes(fileName, fileBytes);
    Console.WriteLine("Retrieved {0}", fileName);
    if (deleteAfterRetrieval)
    {
      abs.DeleteBlob(ContainerName, blobNode.InnerText.Replace(containerStart, ""));
    }
    // save
    
    
   }
  }

 }


 /// <summary>
 /// Arguments class- Parsed Arguments where valid argurments can being with
 /// -, / or -- followed by argument name.
 /// 
 /// When next token is another argument, argument is treated as a flag.
 /// Space, = or : terminate the token of the argument name definition. Anything after that
 /// up to the next argument identifier is defined as the value of that argument.
 /// 
 /// Values beginning with " run until the next " 
 /// 
 /// Valid argument examples include:
 /// -argument1 value1 --argument2 /argument3="This example contains spaces- though it could contain dashes-" /argument4 Hello /argument5:world
 /// These break down into:
 ///   argument1 = value1
 ///   argument2 exists
 ///   argument3 = This is a test
 ///   argument4 = Hello
 ///   argument5 = world
 /// </summary>
 public class Arguments
 {

  private StringDictionary pArguments;

  // Constructor
  public Arguments(string[] Args)
  {

   pArguments = new StringDictionary();
   Regex Splitter = new Regex(@"^-{1,2}|^/|=|:",
       RegexOptions.IgnoreCase | RegexOptions.Compiled);

   Regex matchRemoval = new Regex(@"^['""]?(.*?)['""]?$",
       RegexOptions.IgnoreCase | RegexOptions.Compiled);

   string Arguments = null;
   string[] argumentPieces;

   foreach (string individualArgument in Args)
   {
    // Look for new arguments and possible values
    argumentPieces = Splitter.Split(individualArgument, 3);

    switch (argumentPieces.Length)
    {
     // Either small value or space seperator
     case 1:
      if (Arguments != null)
      {
       if (!pArguments.ContainsKey(Arguments))
       {
        argumentPieces[0] =
            matchRemoval.Replace(argumentPieces[0], "$1");

        pArguments.Add(Arguments.ToLower(), argumentPieces[0]);
       }
       Arguments = null;
      }
      // else Error: no parameter waiting for a value (skipped)
      break;

     // Only found a parameter token, no value
     case 2:
      // The last parameter is still waiting. 
      // With no value, set it to true.
      if (Arguments != null)
      {
       if (!pArguments.ContainsKey(Arguments))
        pArguments.Add(Arguments, "true");
      }
      Arguments = argumentPieces[1];
      break;

     // Parameter with enclosed value
     case 3:
      // The last parameter is still waiting. 
      // With no value, set it to true.
      if (Arguments != null)
      {
       if (!pArguments.ContainsKey(Arguments))
        pArguments.Add(Arguments, "true");
      }

      Arguments = argumentPieces[1];

      // Remove possible enclosing characters (",')
      if (!pArguments.ContainsKey(Arguments))
      {
       argumentPieces[2] = matchRemoval.Replace(argumentPieces[2], "$1");
       pArguments.Add(Arguments, argumentPieces[2]);
      }

      Arguments = null;
      break;
    }
   }
   // In case a parameter is still waiting
   if (Arguments != null)
   {
    if (!pArguments.ContainsKey(Arguments))
     pArguments.Add(Arguments, "true");
   }
  }

  // Retrieve a parameter value if it exists 
  // (overriding C# indexer property)
  public string this[string Param]
  {
   get
   {
    return (pArguments[Param]);
   }
  }

 }


}

