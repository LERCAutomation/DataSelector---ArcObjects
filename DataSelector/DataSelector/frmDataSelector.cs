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
using HLArcMapModule;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

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
            // Run the query. Everything else is allowed to be null.
            string sDefaultSchema = myConfig.GetDatabaseSchema();  // "dbo";
            string sTableName = lstTables.Text;  //"TVERC_Spp_Full";
            string sColumnNames = txtColumns.Text; // "*";
            string sWhereClause = txtWhere.Text; // "TaxonGroup = 'Birds'";
            string sGroupClause = txtGroupBy.Text; // "";
            string sOrderClause = txtOrderBy.Text; // "";
            string sUserID = Environment.UserName;

            // Do some basic checks and fix as required.
            // User ID should be something at least
            if (string.IsNullOrEmpty(sUserID))
            {
                sUserID = "Temp";
            }

            // Table name should always be selected
            if (string.IsNullOrEmpty(sTableName))
            {
                MessageBox.Show("Please select a table to query from");
                return;
            }

            // Decide whether or not there is a geometry field in the returned data.
            // Select the stored procedure accordingly
            string strCheck = "sp_geometry";
            bool blSpatial = sColumnNames.ToLower().Contains(strCheck);
            string strStoredProcedure = "AFHLSelectSppSubset"; // Default for non-spatial data.
            string strPolyFC = "";
            string strPointFC = "";
            string strTable = sTableName + "_" + sUserID;

            if (blSpatial)
            {
                strStoredProcedure = "AFHLSelectSppSubsetSplit";
                strPolyFC = sTableName + "_Poly_" + sUserID;
                strPointFC = sTableName + "_Point_" + sUserID;
                // If it is spatial data we are selected, make sure that a Group By clause is not used.
                // Hopefully this can be resolved at a later date.
                if (!string.IsNullOrEmpty(sGroupClause))
                {
                    MessageBox.Show("Spatial data cannot currently be selected using a Group By clause");
                    return;
                }
            }

            SqlConnection dbConn = myADOFuncs.CreateSQLConnection(myConfig.GetConnectionString());
            SqlCommand myCommand = myADOFuncs.CreateSQLCommand(ref dbConn, strStoredProcedure, CommandType.StoredProcedure); // Note pass connection by ref here.
            myADOFuncs.AddSQLParameter(ref myCommand, "Schema", sDefaultSchema);
            myADOFuncs.AddSQLParameter(ref myCommand, "SpeciesTable", sTableName);
            myADOFuncs.AddSQLParameter(ref myCommand, "ColumnNames", sColumnNames);
            myADOFuncs.AddSQLParameter(ref myCommand, "WhereClause", sWhereClause);
            myADOFuncs.AddSQLParameter(ref myCommand, "GroupByClause", sGroupClause);
            myADOFuncs.AddSQLParameter(ref myCommand, "OrderByClause", sOrderClause);
            myADOFuncs.AddSQLParameter(ref myCommand, "UserID", sUserID);

            dbConn.Open();

            // Run the stored procedure.
            string strRowsAffect = myCommand.ExecuteNonQuery().ToString();

            // convert the results to the designated output file.



            // Add the results to the screen.
            IFeatureWorkspace theFWS = (IFeatureWorkspace)myArcSDEFuncs.OpenArcSDEConnection(myConfig.GetSDEName());
            IApplication theApplication = (IApplication)ArcMap.Application;
            ArcMapFunctions myArcMapFuncs = new ArcMapFunctions(theApplication);
            if (blSpatial)
            {
                IFeatureClass thePolyFC = theFWS.OpenFeatureClass(strPolyFC);
                IFeatureClass thePointFC = theFWS.OpenFeatureClass(strPointFC);
                myArcMapFuncs.AddLayerFromFClass(thePolyFC);
                myArcMapFuncs.AddLayerFromFClass(thePointFC);
            }
            else
            {
                ITable theTable = theFWS.OpenTable(strTable);
                myArcMapFuncs.AddLayerFromTable(theTable, "Test");
            }
            

 
            
        }
    }
}
