using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Data.SqlClient;
using System.IO;

namespace CreateMOOPDataXSD
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      txtAzureAccount.Text = Properties.Settings.Default.AzureAccount;
      txtAzureSharedKey.Text = Properties.Settings.Default.AzureSharedKey;
      txtSQLConnection.Text = Properties.Settings.Default.SQLConnect;
      txtConnectioName.Text = Properties.Settings.Default.ConnectionName;
      chkPost.Checked = Properties.Settings.Default.UsePost;
      cmbCacheability.SelectedItem = Properties.Settings.Default.cacheablilityType;
      if (txtSQLConnection.Text == string.Empty)
        tabControl1.SelectTab("tabPage2");
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
      Properties.Settings.Default.AzureAccount = txtAzureAccount.Text;
      Properties.Settings.Default.AzureSharedKey = txtAzureSharedKey.Text;
      Properties.Settings.Default.SQLConnect = txtSQLConnection.Text;
      Properties.Settings.Default.ConnectionName = txtConnectioName.Text;
      Properties.Settings.Default.UsePost = chkPost.Checked;
      Properties.Settings.Default.cacheablilityType = cmbCacheability.SelectedItem.ToString();
      Properties.Settings.Default.Save();
    }


    private bool validateSQLConnection(string sqlConnectionString)
    {
      bool retVal = false;
      SqlConnection cn = new SqlConnection(sqlConnectionString);
      try
      {
        cn.Open();
        cn.Close();
        retVal = true;

      }
      catch
      {
        retVal = false;
      }
      return retVal;
    }

    private void UpdateStatus(string message)
    {
      toolStripStatusLabel1.Text = message;
      Application.DoEvents();
    }

    private void btnGetSprocs_Click(object sender, EventArgs e)
    {
      if (txtSQLConnection.Text == string.Empty)
      {
        MessageBox.Show("Must have a SQL Connection String Defined", "No SQL Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else
      {
        Cursor.Current = Cursors.WaitCursor;
        UpdateStatus("Validating connection");
        if (validateSQLConnection(txtSQLConnection.Text))
        {
          try
          {
            UpdateStatus("Getting procedures");
            SqlCommand cmd = new SqlCommand("SELECT schema_name(schema_id) + '.' + name SprocName, " +
              "Object_ID FROM sys.procedures WHERE TYPE = 'P' ORDER BY 1", new SqlConnection(txtSQLConnection.Text));
            cmd.Connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            cmbStoredProcedures.Items.Clear();
            while (dr.Read())
            {
              cmbStoredProcedures.Items.Add(new cbItem(dr[0].ToString(), dr[1].ToString()));
            }
            cmd.Connection.Close();
          }
          catch
          {
          }
        }
        else
        {
          MessageBox.Show("Error opening SQL Connection", "Invalid SQL Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      UpdateStatus("Ready");
      Cursor.Current = Cursors.Default ;
    }

    
    private string resolveObjectID(string objectName)
    {
      return string.Empty;
    }

    private string parseSprocNameForFileName(string sprocName)
    {
      string retVal = string.Empty;
      if (sprocName.StartsWith("dbo."))
        sprocName = sprocName.Substring(4);
      foreach (char c in sprocName.ToCharArray())
      {
        if (c >= 'A' && c<='Z')
        {
          if (!retVal.EndsWith(".") && retVal != string.Empty )
            retVal += '-';
          retVal +=  c.ToString().ToLower();
        }
        else retVal += c;
      }
      retVal = retVal.Replace(".","-") + ".xml";
      return retVal;
    }

    private string generateXML(string StoredProcedureName, string StoredProcedureID, string xmlConnectionName , string sqlConnString, string cacheabiltyType)
    {
      if (StoredProcedureName == StoredProcedureID || StoredProcedureID == string.Empty ) // need to get an id!
      {
      }
      UpdateStatus("Getting Stored Procedure Information");
      StringBuilder sb = new StringBuilder("SELECT param.Name, usrt.name AS [DataType], ");
      sb.Append("CAST(CASE WHEN baset.name IN (N'nchar', N'nvarchar') AND param.max_length <> -1 THEN param.max_length/2 ELSE param.max_length END AS int) AS [Length], ");
      sb.Append("CAST(param.precision AS int) AS [NumericPrecision], ");
      sb.Append("CAST(param.scale AS int) AS [NumericScale], ");
      sb.Append("param.is_output AS [IsOutputParameter], ");
      sb.Append("param.is_cursor_ref AS [IsCursorParameter] ");
      sb.Append("FROM sys.all_parameters AS param   ");
      sb.Append("LEFT OUTER JOIN sys.types AS usrt ON usrt.user_type_id = param.user_type_id ");
      sb.Append("LEFT OUTER JOIN sys.types AS baset ON (baset.user_type_id = param.system_type_id and baset.user_type_id = baset.system_type_id)  ");
      sb.AppendFormat("WHERE param.object_id={0} ORDER BY param.parameter_id ASC", StoredProcedureID );
      SqlCommand cmd = new SqlCommand(sb.ToString(), new SqlConnection(sqlConnString));
      cmd.Connection.Open();
      StringBuilder xsd = new StringBuilder("<?xml version=\"1.0\"?>\r\n<MOOPData>\r\n");
      xsd.AppendFormat("<storedProcedure procedureName=\"{0}\" connectionName=\"{1}\" requirePost=\"{2}\" >\r\n", StoredProcedureName, xmlConnectionName, 
        (chkPost.Checked ? "true" : "false"));
      SqlDataReader dr = cmd.ExecuteReader();
      UpdateStatus("Creating XSD");
      while (dr.Read())
      {
        xsd.AppendFormat("<parameter parameterName=\"{0}\" urlParameterName=\"{1}\" dataType=\"{2}\" dataLength=\"{3}\" defaultValue=\"DBNull\" isOutput=\"{4}\" />\r\n",
          dr[0].ToString(), dr[0].ToString().Replace("@", ""), dr[1].ToString(), dr[2].ToString(), (dr["IsOutputParameter"].ToString() == "1" ? "true" : "false"));
      }
      cmd.Connection.Close();
      xsd.AppendFormat("<cacheInformation expireSeconds=\"0\" cacheability=\"{0}\" />\r\n</storedProcedure>\r\n</MOOPData>", cacheabiltyType);
      return  xsd.ToString();
    }


    private void btnGenerateXML_Click(object sender, EventArgs e)
    {
      string objID = string.Empty;
      cbItem cb = (cbItem)cmbStoredProcedures.SelectedItem;
      if (cb.Name == cb.Value) // need to get an id!
      {
      }
      else
      {
        objID = cb.Value;
      }
      Cursor.Current = Cursors.WaitCursor;
      UpdateStatus("Getting Stored Procedure Information");

      //StringBuilder sb = new StringBuilder("SELECT param.Name, usrt.name AS [DataType], ");
      //sb.Append("CAST(CASE WHEN baset.name IN (N'nchar', N'nvarchar') AND param.max_length <> -1 THEN param.max_length/2 ELSE param.max_length END AS int) AS [Length], ");
      //sb.Append("CAST(param.precision AS int) AS [NumericPrecision], ");
      //sb.Append("CAST(param.scale AS int) AS [NumericScale], ");
      //sb.Append("param.is_output AS [IsOutputParameter], ");
      //sb.Append("param.is_cursor_ref AS [IsCursorParameter] ");
      //sb.Append("FROM sys.all_parameters AS param   ");
      //sb.Append("LEFT OUTER JOIN sys.types AS usrt ON usrt.user_type_id = param.user_type_id ");
      //sb.Append("LEFT OUTER JOIN sys.types AS baset ON (baset.user_type_id = param.system_type_id and baset.user_type_id = baset.system_type_id)  ");
      //sb.AppendFormat("WHERE param.object_id={0} ORDER BY param.parameter_id ASC", objID);
      //SqlCommand cmd = new SqlCommand(sb.ToString(), new SqlConnection(txtSQLConnection.Text));
      //cmd.Connection.Open();
      //StringBuilder xsd = new StringBuilder("<?xml version=\"1.0\"?>\r\n");
      //xsd.AppendFormat("<storedProcedure name=\"{0}\" connectionName=\"{1}\" requirePost=\"{2}\" >\r\n", cb.Name, txtConnectioName.Text, "true");
      //SqlDataReader dr = cmd.ExecuteReader();
      //UpdateStatus("Creating XSD");
      //while (dr.Read())
      //{
      //  xsd.AppendFormat("<parameter parameterName=\"{0}\" urlParameterName=\"{1}\" dataType=\"{2}\" dataLength=\"{3}\" defaultValue=\"DBNull\" isOutput=\"{4}\" />\r\n",
      //    dr[0].ToString(), dr[0].ToString().Replace("@", ""), dr[1].ToString(), dr[2].ToString(), (dr["IsOutputParameter"].ToString() == "1" ? "True" : "False"));
      //}
      //cmd.Connection.Close();
      //xsd.AppendFormat("<cacheInformation expireSeconds=\"0\" cacheability=\"NoCache\" />\r\n</storedProcedure>");
      //txtXSD.Text = xsd.ToString();
      txtXSD.Text = generateXML(cb.Name, cb.Value, txtConnectioName.Text, txtSQLConnection.Text, cmbCacheability.Text);
      txtXMLFileName.Text = parseSprocNameForFileName(cb.Name);
      UpdateStatus("Ready");
      Cursor.Current = Cursors.Default;

    }

    private void btnGenerateAll_Click(object sender, EventArgs e)
    {
      Cursor.Current = Cursors.WaitCursor;
      if (!Directory.Exists("outputFiles"))
        Directory.CreateDirectory("outputFiles");
      foreach (cbItem cb in cmbStoredProcedures.Items)
      {
        File.WriteAllText(string.Format(@"outputFiles\{0}", parseSprocNameForFileName(cb.Name)), generateXML(cb.Name, cb.Value, txtConnectioName.Text, txtSQLConnection.Text, cmbCacheability.Text ));
      }
      UpdateStatus("Ready");
      Cursor.Current = Cursors.Default;
    }

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
    public string Name { get; set; }

    /// <summary>
    /// The value of the object
    /// </summary>
    public string Value { get; set; }

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
}
