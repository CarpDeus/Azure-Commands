using System;
using System.Collections.Generic;
using System.Text;

using Finsel.AzureCommands;
using System.Collections;
using System.IO;
using Microsoft.Win32;
using System.Xml;

namespace SagaProcessor
{
 class ATSSagaProcessor
 {
  private string sagaPoint = "registrationsaga";

   private Authentication pAuth = new Authentication();
  /// <summary>
  /// Object that stores information needed to access an instance of 
  /// Azure Table Storage
  /// </summary>
   public Authentication auth { get { return pAuth; } set { pAuth = value; ad.auth = pAuth; } }

  AzureQueueStorage aqs = new AzureQueueStorage();
  AzureBlobStorage abs = new AzureBlobStorage();
  AzureTableStorage ats = new AzureTableStorage();
  azureDirect ad = new azureDirect();
  azureResults ar = new azureResults();

  static int acceptOrDecline = 0;
  public ATSSagaProcessor(string account, string endPoint, string sharedKey, string keyType)
  {
   pAuth = new Authentication(account, "", sharedKey);
   ad.auth = pAuth;
   ats.auth = pAuth;
   aqs.auth = pAuth;
   abs.auth = pAuth;
  }

  #region Notification
  /// <summary>
  /// Get messages from the Class as it processes
  /// </summary>
  /// <param name="message">Message data</param>
  public delegate void LogHandler(string message);
  
  private LogHandler mvLogHandler;
  
  public void OnLogHandler(LogHandler msgString)
  {
   if (msgString != null)
   {
    mvLogHandler = msgString;
    Console.WriteLine(msgString);
   }
  }

  /// <summary>
  /// Notify the calling object of an event
  /// </summary>
  /// <param name="msg">Message of what has happened</param>
  void NotifyCaller(string msg)
  {
   if (mvLogHandler != null)
    mvLogHandler(msg);
   else
    Console.WriteLine(msg);
  }

  #endregion

  public void GetRequests()
  {
   // Anything in the Queue we need to get
   azureResults ar = aqs.Messages(cmdType.get, sagaPoint, "", "", "");
   string messageID = string.Empty;
   string popReceipt = string.Empty;
   string messageBody = string.Empty;
   if (ar.Body != null) // We have a message
   {
    System.Xml.XmlDocument xdoc = new System.Xml.XmlDocument();
    xdoc.LoadXml(ar.Body);
    System.Xml.XmlNodeList nodes = xdoc.SelectNodes("//QueueMessage");
    if (nodes.Count == 0)
     NotifyCaller( "No message to process");
    else
     foreach (System.Xml.XmlNode node in nodes)
     {
      messageID = node.SelectSingleNode("MessageId").InnerText;
      if (node.SelectNodes("//PopReceipt").Count > 0)
       popReceipt = node.SelectSingleNode("PopReceipt").InnerText;
      if (node.SelectNodes("//MessageText").Count > 0)
       messageBody = node.SelectSingleNode("MessageText").InnerText;
      NotifyCaller(string.Format("Processing Message ID:{0}",messageID));
      ProcessMessage(messageID, popReceipt, messageBody);
     }
   }
   else NotifyCaller("No message to process");
  }

  private void UpdateSagaDetails(string transactionID, int stepNumber, string results, string nextStep)
  {
   string sagaUpdateBody = string.Format(sagaUpdateTemplate, transactionID, "Status", stepNumber + 1, nextStep,
    string.Format("Step{0}", stepNumber.ToString("D15")), results);
   azureResults ar = ats.Entities(cmdType.merge, sagaPoint, transactionID, "Status", sagaUpdateBody, "", "*");
   NotifyCaller(string.Format("TransactionID: {0} Results:{1}",transactionID, results));
  }

  private void UpdateSagaError(string transactionID,  string results, string nextStep)
  {
   string sagaUpdateBody = string.Format(sagaErrorTemplate , transactionID, "Status", -99, nextStep,
     "", results);
   azureResults ar = ats.Entities(cmdType.merge, sagaPoint, transactionID, "Status", sagaUpdateBody, "", "*");
   NotifyCaller(string.Format("TransactionID: {0} Results:{1}", transactionID, results));
  }

  private void ProcessMessage(string messageID, string popReceipt, string messageBody)
  {
   System.Xml.XmlDocument xdoc = new System.Xml.XmlDocument();
   xdoc.LoadXml(messageBody);
   // Get the ProcessType out of the block
   string processType = xdoc.SelectSingleNode("//ProcessType[1]").InnerText;
   int stepNumber = Convert.ToInt16(xdoc.SelectSingleNode("//StepNumber[1]").InnerText);
   string stepName = xdoc.SelectSingleNode("//StepName[1]").InnerText;
   string transactionID = xdoc.SelectSingleNode("//TransactionID[1]").InnerText;
   switch (stepName)
   {
    case "PaymentProcessing": // Make call to PaymentProcessing Object
     string paymentType = xdoc.SelectSingleNode("//PaymentType[1]").InnerText;
     switch (paymentType)
     {
      case "PO": // Get all details and email to accounting to handle
       UpdateSagaDetails(transactionID, stepNumber, "Forwarded PO to accounting for processing", "AccountingPOProcess");
       // No queue message, that will be handled when Accounting process the PO
       break;
      case "CreditCard": // Get all details and attempt to authorize
       if ((acceptOrDecline % 2) == 0) // Every other request is authorized
       {
        string registeredUser = xdoc.SelectSingleNode("//UserID[1]").InnerText;
        UpdateSagaDetails(transactionID, stepNumber, string.Format("Credit card authorization: {0}", Guid.NewGuid().ToString()), "RegistrationComplete");
        UpdateRegistrationDetails(transactionID, "Completed");
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("<Saga><TransactionID>{0}</TransactionID>", transactionID);
         sb.Append("<ProcessType>Registration</ProcessType><StepName>RegistrationComplete</StepName>");
         sb.AppendFormat("<StepNumber>{0}</StepNumber><UserID>{1}</UserID>", stepNumber + 1, registeredUser);
        sb.Append("</Saga>");
        azureResults ar = aqs.Messages(cmdType.post,sagaPoint,sb.ToString(),"","");
       }
       else
        UpdateSagaError(transactionID,  "Credit card declined", "ManualIntervention");
       acceptOrDecline++;
       break;
     }
     break;
    case "RegistrationComplete":
     string emailUser = xdoc.SelectSingleNode("//UserID[1]").InnerText;
     azureResults ar1 = ats.Entities(cmdType.get, "vslivelasvegas", "RegisteredUsers", emailUser, "", "");
     UpdateSagaDetails(transactionID, 00, string.Format("Emailed {0} with completed registration information",emailUser ), "Completed successfully");
     break;
    default:
     UpdateSagaDetails(transactionID, -99, string.Format("{0} is an unknown processType", processType), "ManualIntervention");
     break;
   }
   azureResults ar2 = aqs.Messages(cmdType.delete, sagaPoint, "", string.Format("popreceipt={0}", popReceipt), messageID); 
  }

  private void UpdateRegistrationDetails(string transactionID, string results)
  {
   string registrationDetailsTemplate = @"<m:properties>
          <d:PartitionKey>RegistrationDetails</d:PartitionKey>
          <d:RowKey>{0}</d:RowKey>
          <d:RegistrationStatus>{1}</d:RegistrationStatus>
        </m:properties>
        </content>
      </entry>";
   azureResults ar1 = ats.Entities(cmdType.merge, "vslivelasvegas", "RegistrationDetails", transactionID,
    string.Format(registrationDetailsTemplate, transactionID, results), "");
  }

  string sagaUpdateTemplate = @"<m:properties>
          <d:PartitionKey>{0}</d:PartitionKey>
          <d:RowKey>{1}</d:RowKey>
          <d:CurrentStepNumber m:type=""Edm.Int32"">{2}</d:CurrentStepNumber>
          <d:CurrentStep>{3}</d:CurrentStep>
          <d:{4}>{5}</d:{4}>
        </m:properties>";


  string sagaErrorTemplate = @"<m:properties>
          <d:PartitionKey>{0}</d:PartitionKey>
          <d:RowKey>{1}</d:RowKey>
          <d:CurrentStepNumber m:type=""Edm.Int32"">{2}</d:CurrentStepNumber>
          <d:CurrentStep>{3}</d:CurrentStep>
          <d:Error>{5}</d:Error>
        </m:properties>";
  
 }
}
