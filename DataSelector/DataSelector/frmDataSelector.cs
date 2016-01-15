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
using HLFileFunctions;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseUI;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;

using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;

// Unfortunately we also need ADO.Net in order to run the stored procedures with parameters...
using System.Data.SqlClient;


namespace DataSelector
{
    public partial class frmDataSelector : Form
    {
        SelectorToolConfig myConfig;
        ESRISQLServerFunctions myArcSDEFuncs;
        ADOSQLServerFunctions myADOFuncs;
        FileFunctions myFileFuncs;
        public frmDataSelector()
        {
            InitializeComponent();
            // Fill with the relevant.
            myConfig = new SelectorToolConfig(); // Should find the config file automatically.
            myArcSDEFuncs = new ESRISQLServerFunctions();
            myADOFuncs = new ADOSQLServerFunctions();
            myFileFuncs = new FileFunctions();
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

            this.Cursor = Cursors.WaitCursor;
            // Run the query. Everything else is allowed to be null.
            string sDefaultSchema = myConfig.GetDatabaseSchema();
            string sTableName = lstTables.Text; 
            string sColumnNames = txtColumns.Text; 
            string sWhereClause = txtWhere.Text; 
            string sGroupClause = txtGroupBy.Text; 
            string sOrderClause = txtOrderBy.Text;
            string sOutputFormat = cmbOutFormat.Text;
            string sOutputFile = txtOutputFile.Text;
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

            if (string.IsNullOrEmpty(sOutputFile))
            {
                MessageBox.Show("Please specify an output file");
                return;
            }
               

            // Decide whether or not there is a geometry field in the returned data.
            // Select the stored procedure accordingly
            string strCheck = "sp_geometry";
            bool blSpatial = sColumnNames.ToLower().Contains(strCheck);
            // TO DO: IF "*" IS USED CHECK FOR THE EXISTENCE OF THE SPATIAL FIELD IN THE TABLE.
            // IF IT EXISTS SET blSpatial TO TRUE.
            bool blFlatTable = blSpatial; // to start with
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
            IApplication theApplication = (IApplication)ArcMap.Application;
            ArcMapFunctions myArcMapFuncs = new ArcMapFunctions(theApplication);

            //string strtargetWorkspaceName =  myFileFuncs.GetDirectoryName(sOutputFile);
            string strPointOutTab = myConfig.GetSDEName() + @"\" + sTableName + "_Point_" + sUserID;
            string strPolyOutTab = myConfig.GetSDEName() + @"\" + sTableName + "_Poly_" + sUserID;
            string strOutTab = myConfig.GetSDEName() + @"\" + sTableName + "_" + sUserID;

            string strOutPoints = "";
            string strOutPolys = "";

            bool blResult = false;
            if (blSpatial) 
            {
                // export points and polygons
                // How is the data to be exported?
                if (sOutputFormat == "Geodatabase") 
                {
                    // Easy, export without further ado.
                    strOutPoints = sOutputFile + "_Point";
                    strOutPolys = sOutputFile + "_Polys";
                    MessageBox.Show("About to export points to " + strOutPoints);
                    blResult = myArcMapFuncs.CopyFeatures(strPointOutTab, strOutPoints);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting point geodatabase file");
                        return;
                    }
                    blResult = myArcMapFuncs.CopyFeatures(strPolyOutTab, strOutPolys);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting polygon geodatabase file");
                        return;
                    }
                }
                else if (sOutputFormat == "Shapefile")
                {
                    // Create file names first.
                    sOutputFile = myFileFuncs.ReturnWithoutExtension(sOutputFile);
                    strOutPoints = sOutputFile + "_Point.shp";
                    strOutPolys = sOutputFile + "_Poly.shp";
                    blResult = myArcMapFuncs.CopyFeatures(strPointOutTab, strOutPoints);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting point shapefile");
                        return;
                    }
                    blResult = myArcMapFuncs.CopyFeatures(strPolyOutTab, strOutPolys);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting polygon shapefile");
                        return;
                    }
                }
                else
                {
                    // Not a spatial export, but it is a spatial layer so there are two files.
                    blFlatTable = true;
                    sOutputFile = myFileFuncs.ReturnWithoutExtension(sOutputFile);
                    string strExtension = sOutputFile.Substring(sOutputFile.Length - 4, 4);
                    strOutPoints = sOutputFile + "_Point" + strExtension;
                    strOutPolys = sOutputFile + "_Poly" + strExtension;

                    blResult = myArcMapFuncs.CopyTable(strPointOutTab, strOutPoints);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting output table");
                        return;
                    }
                    blResult = myArcMapFuncs.CopyTable(strPolyOutTab, strOutPolys);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting output table");
                        return;
                    }
                }
            }
            else
            {
                // We are exporting a non-spatial output.
                // TO BE DONE CHANGE .SHP TO .DBF
                blResult = myArcMapFuncs.CopyTable(strOutTab, sOutputFile);
                if (!blResult)
                {
                    MessageBox.Show("Error exporting output table");
                    return;
                }
            }
           

            // Add the results to the screen.
            
            if (blSpatial && !blFlatTable)
            {
                myArcMapFuncs.AddFeatureLayerFromString(strOutPoints);
                myArcMapFuncs.AddFeatureLayerFromString(strOutPolys);
            }
            else
            {
                //ITable theTable = theFWS.OpenTable(strTable);
                //myArcMapFuncs.AddLayerFromTable(theTable, "Test");
                myArcMapFuncs.AddTableLayerFromString(strOutTab);
            }

            this.Cursor = Cursors.Default;
            MessageBox.Show("Process complete");
 
            
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            // Show SaveAs dialog.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            if (cmbOutFormat.Text == "Geodatabase") saveFileDialog1.Filter = "Geodatabase files|*.*";
            if (cmbOutFormat.Text == "Shapefile") saveFileDialog1.Filter = "Shapefiles (*.shp)|*.shp";
            if (cmbOutFormat.Text == "Text file") saveFileDialog1.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";

            saveFileDialog1.InitialDirectory = myConfig.GetDefaultExtractPath();
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Put the Save As result into the text box.
                txtOutputFile.Text = saveFileDialog1.FileName;
            }
        }

        

      

    }
}
