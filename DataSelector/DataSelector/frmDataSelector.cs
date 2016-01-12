using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HLSelectorToolConfig;
using HLESRISQLServerFunctions;

using ESRI.ArcGIS.Geodatabase;

// Unfortunately we also need ADO.Net in order to run the stored procedures with parameters...
using System.Data.SqlClient;


namespace DataSelector
{
    public partial class frmDataSelector : Form
    {
        SelectorToolConfig myConfig;
        ESRISQLServerFunctions myArcSDEFuncs;
        ADOSQLServerFunctions myADOFuncs;
        public frmDataSelector()
        {
            InitializeComponent();
            // Fill with the relevant.
            myConfig = new SelectorToolConfig(); // Should find the config file automatically.
            myArcSDEFuncs = new ESRISQLServerFunctions();
            myADOFuncs = new ADOSQLServerFunctions();
            // fill the list box with SQL tables
            string strSDE = myConfig.GetSDEName();
            string strIncludeWildcard = myConfig.GetIncludeWildcard();
            string strExcludeWildcard = myConfig.GetExcludeWildcard();
            //MessageBox.Show(strSDE);
            IWorkspace wsSQLWorkspace = myArcSDEFuncs.OpenArcSDEConnection(strSDE);
            List<string> strTableList = myArcSDEFuncs.GetTableNames(wsSQLWorkspace, strIncludeWildcard, strExcludeWildcard);
            foreach (string strItem in strTableList)
            {
                lstTables.Items.Add(strItem);
            }
            // Close the SQL connection
            wsSQLWorkspace = null;
            // However keep the Config and SQLFuncs objects alive for use later in the form.
            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Save as dialog appears.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Query files (*.qry)|*.qry";
            //saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            string strFileName;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                strFileName = saveFileDialog1.FileName;
                // Check if file exists
                if (File.Exists(strFileName))
                {
                    File.Delete(strFileName);
                }
                StreamWriter qryFile = File.CreateText(strFileName);
                // Write query
                qryFile.WriteLine("This is a test");
                qryFile.Close();
                MessageBox.Show("Query file saved");
            }
            
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            // Open file dialog appears
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "Query files (*.qry)|*.qry";
            openFileDialog1.RestoreDirectory = true;

            string strFileName;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                strFileName = openFileDialog1.FileName;
                StreamReader qryFile = new StreamReader(strFileName);
                // read query
                string qryLine;
                string allInfo = "The file contains the following info: ";
                while ((qryLine = qryFile.ReadLine()) != null)
                {
                    allInfo = allInfo + qryLine;
                }
                txtColumns.Text = allInfo;

            }
            

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string theResult = "The form contains the following: ";
            theResult = theResult + "columns: " + txtColumns.Text + ", ";
            theResult = theResult + "where clause: " + txtWhere.Text + ", ";
            theResult = theResult + "order by: " + txtOrderBy.Text + ", ";
            theResult = theResult + "group by: " + txtGroupBy.Text + ", ";
            theResult = theResult + "table names: " + lstTables.Text;
            //MessageBox.Show(theResult);

            // Connect to SQL server
            //IWorkspace wsSQLWorkspace = myArcSDEFuncs.OpenArcSDEConnection(myConfig.GetSDEName());

            // Run the query
            string sDefaultSchema = "dbo";
            string sTableName = "TVERC_Spp_Full";
            string sColumnNames = "*";
            string sWhereClause = "TaxonGroup = 'Birds'";
            string sGroupClause = "";
            string sOrderClause = "";
            string sUserID = "Hezz";


            MessageBox.Show(myConfig.GetConnectionString());
            SqlConnection dbConn = myADOFuncs.CreateSQLConnection(myConfig.GetConnectionString());
            MessageBox.Show("Connection created");
            SqlCommand myCommand = myADOFuncs.CreateSQLCommand(ref dbConn, "AFHLSelectSppSubset", CommandType.StoredProcedure); // Note pass connection by ref here.
            MessageBox.Show("Command created");
            myADOFuncs.AddSQLParameter(ref myCommand, "Schema", sDefaultSchema);
            MessageBox.Show("First parameter added");
            myADOFuncs.AddSQLParameter(ref myCommand, "SpeciesTable", sTableName);
            myADOFuncs.AddSQLParameter(ref myCommand, "ColumnNames", sColumnNames);
            myADOFuncs.AddSQLParameter(ref myCommand, "WhereClause", sWhereClause);
            myADOFuncs.AddSQLParameter(ref myCommand, "GroupByClause", sGroupClause);
            myADOFuncs.AddSQLParameter(ref myCommand, "OrderByClause", sOrderClause);
            myADOFuncs.AddSQLParameter(ref myCommand, "UserID", sUserID);

            dbConn.Open();
            MessageBox.Show("Connection opened");
            string strRowsAffect = myCommand.ExecuteNonQuery().ToString();
            MessageBox.Show("procedure run");
        }
    }
}
