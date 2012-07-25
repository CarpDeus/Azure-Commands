using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.Specialized;
using System.Text.RegularExpressions;

using System.IO;
using Finsel.AzureCommands;
using System.Xml;

using Microsoft.Win32;

namespace AzureBlobLoader
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
   string StartingLocation = (CommandLine["from"] == null ? CommandLine["f"] : CommandLine["from"]);
   if (StartingLocation == null)
    StartingLocation = System.Environment.CurrentDirectory;
   bool directoryStructure = (CommandLine["directory"] != null);
   bool archiveOnly = (CommandLine["archive"] != null);
   string FileFilter = CommandLine["filter"];
   string FilterOut = CommandLine["filterout"];
   bool verboseOutput = (CommandLine["verbose"] != null);
   if (!DisplayHelp && Container == null)
   {
    ErrorMessage = ErrorMessage + "Container is REQUIRED!\r\n";
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
    BlobLoader bl = new BlobLoader();
    Console.WriteLine(bl.BuildInfo());
    Console.Write("\r\n\r\n{0}", System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name);
    
    Console.WriteLine(" -? -s[haredkey]:AzureSharedKey -e[ndpoint]:AzureEndPoint");
    Console.WriteLine("\t\t    -a[ccount]:AzureAccount -c[ontainer]:BlobContainer ");
    Console.WriteLine("\t\t    -f[rom]:StartingDirectory -filter:FileFilter -archive");
    Console.WriteLine("\t\t    -verbose");
    Console.WriteLine("\t-? shows this message");
    Console.WriteLine("\t-s[haredkey] your Azure Shared Key");
    Console.WriteLine("\t-e[ndpoint]  your Azure Endpoint");
    Console.WriteLine("\t-a[ccount]   your Azure Account");
    Console.WriteLine("\t-c[ontainer] the Container where the blobs are to be stored");
    Console.WriteLine("\t\t The container will be created if it doesn't exist.");
    
    Console.WriteLine("\t-f[rom] the starting directory location. If not specified, the ");
    Console.WriteLine("\t\tdirectory the program is run from will be used");
    Console.WriteLine("\t-filter a file filter to be applied. * matches all, ? matches one letter");

    Console.WriteLine("\t\t if not included, will be where the application was run from");
    Console.WriteLine("\t-directory Flag to store blobs with directory structure");
    Console.WriteLine("\t-archive Flag to store blobs with archive attribute set.\r\n\t\tResets ArhiveAttribute");
    Console.WriteLine("\t-verbose Flag to determine whether to show archived files.\r\n\t\tWhen included, shows archived files as they are processed.");

     }
   else
   {
    BlobLoader bl1 = new BlobLoader();
    bl1.Account = Account;
    bl1.Container = Container;
    bl1.Directories = directoryStructure;
    bl1.StartingDirectory = StartingLocation ;
    bl1.EndPoint = EndPoint;
    bl1.SharedKey = SharedKey;
    bl1.FileFilter = FileFilter;
    bl1.FilterOut = FilterOut;
    bl1.ArchiveAttributes = archiveOnly;
    bl1.verbose = verboseOutput;
    bl1.LoadBlobs();
      }


  }
 }


 class BlobLoader
 {
  public string Account = string.Empty;
  public string SharedKey = string.Empty;
  public string Container = string.Empty;
  public string EndPoint = string.Empty;
  public string StartingDirectory = string.Empty;
  public bool Directories = false;
  public string FileFilter = string.Empty;
  public bool ArchiveAttributes = false;
  public string FilterOut = string.Empty;
  public bool verbose = false;
  public  BlobLoader()
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


  public void LoadBlobs()
  {
    if (FileFilter == string.Empty || FileFilter == null)
      FileFilter = "*";
   System.Collections.Hashtable metadata = new System.Collections.Hashtable();
   metadata.Add("loadingSource", "finsel.AzureBlobLoader");
   metadata.Add("dateLoaded",DateTime.Now.ToShortDateString());
   azureResults ar = new azureResults();
   AzureBlobStorage abs = new AzureBlobStorage(Account, EndPoint, SharedKey, "SharedKey");
   if (Container == "*")
   {
    foreach (string newContainer in Directory.GetDirectories(StartingDirectory))
    {
     Container = newContainer.Replace(StartingDirectory,"");
     if (Container.StartsWith(@"\")) Container = Container.Substring(1);
     ar = abs.Containers(cmdType.get, Container, new System.Collections.Hashtable());
     if (!ar.Succeeded)
      ar = abs.Containers(cmdType.put, Container, metadata);
     if(ar.Succeeded)
      ProcessDirectory(abs, metadata, newContainer, newContainer );
    }
    // Process $root
    ar = abs.Containers(cmdType.get, "$root%", new System.Collections.Hashtable());
    if (ar.Succeeded)
        ProcessDirectory(abs, metadata, StartingDirectory, "$root");
   }
   else
   {
    ar = abs.Containers(cmdType.get, Container, new System.Collections.Hashtable());
    if (!ar.Succeeded)
     ar = abs.Containers(cmdType.put, Container, metadata);
    ProcessDirectory(abs, metadata, StartingDirectory, StartingDirectory);
   }
  }

  string GetMimeType(string ext)
  {
   string mime = "application/octetstream";

   Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
   if (ext == ".js")
    mime = "text/javascript";
   else
    if (rk != null && rk.GetValue("Content Type") != null)
     mime = rk.GetValue("Content Type").ToString();

   return mime;
  }


  void ProcessDirectory(AzureBlobStorage abs, System.Collections.Hashtable metadata, string DirectoryPath, string startDirectory)
  {

   foreach(string fileName in Directory.GetFiles(DirectoryPath, FileFilter))
   {
    FileInfo fi = new FileInfo(fileName);
    // check whether a file has archive attribute
    bool isArchive = ((File.GetAttributes(fileName) & FileAttributes.Archive) == FileAttributes.Archive);
    if ((ArchiveAttributes && isArchive) || !ArchiveAttributes) // || (FilterOut != string.Empty && file))
    {
      string blobname = fi.Name;
      if (Directories)
        blobname = fileName.Replace(startDirectory, "").Replace(@"\", "/");
      if (blobname.StartsWith("/"))
        blobname = blobname.Substring(1);
      azureResults ar = abs.PutBlob(fi.Length, GetMimeType(fi.Extension),
       File.ReadAllBytes(fileName), Container, blobname, metadata);
      if (ar.Succeeded)
      {
        Console.WriteLine("Loaded: {0}", ar.Url);
        if (ArchiveAttributes)
          File.SetAttributes(fileName, File.GetAttributes(fileName) & ~(FileAttributes.Archive));
      }
      else
        Console.WriteLine("Error loading {0}\r\n\tStatus:{1}\r\n\r\n", blobname, ar.StatusCode);
    }
    if (ArchiveAttributes && !isArchive && verbose)
      Console.WriteLine("ArchiveAttribute Not Set on {0}", fileName);
   }
   foreach(string subdirectory in Directory.GetDirectories(DirectoryPath))
   {
    ProcessDirectory(abs,metadata, subdirectory, startDirectory );
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
