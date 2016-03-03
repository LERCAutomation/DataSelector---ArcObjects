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
        bool blOpenForm;
        public frmDataSelector()
        {
            InitializeComponent();
            blOpenForm = true;
            // Fill with the relevant.
            myConfig = new SelectorToolConfig(); // Should find the config file automatically.
            if (myConfig.GetFoundXML() == false)
            {
                MessageBox.Show("XML file not found; form cannot load.");
                blOpenForm = false;
            }
            else if (myConfig.GetLoadedXML() == false)
            {
                MessageBox.Show("Error loading XML File; form cannot load.");
                blOpenForm = false;
            }

            if (blOpenForm)
            {
                myArcSDEFuncs = new ESRISQLServerFunctions();
                myADOFuncs = new ADOSQLServerFunctions();
                myFileFuncs = new FileFunctions();
                // fill the list box with SQL tables
                string strSDE = myConfig.GetSDEName();
                string strIncludeWildcard = myConfig.GetIncludeWildcard();
                string strExcludeWildcard = myConfig.GetExcludeWildcard();
                string strDefaultFormat = myConfig.GetDefaultFormat();

                cmbOutFormat.Text = strDefaultFormat;

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
            else
            {
                    Load += (s, e) => Close();
                    return;
            }
            
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
            IApplication theApplication = (IApplication)ArcMap.Application;
            ArcMapFunctions myArcMapFuncs = new ArcMapFunctions(theApplication);
            
            // Run the query. Everything else is allowed to be null.
            string sDefaultSchema = myConfig.GetDatabaseSchema();
            string sTableName = lstTables.Text; 
            string sColumnNames = txtColumns.Text; 
            string sWhereClause = txtWhere.Text; 
            string sGroupClause = txtGroupBy.Text; 
            string sOrderClause = txtOrderBy.Text;
            string sOutputFormat = cmbOutFormat.Text;
            string sOutputFile;
            string sUserID = Environment.UserName;
            
            // Do some basic checks and fix as required.
            // User ID should be something at least
            if (string.IsNullOrEmpty(sUserID))
            {
                sUserID = "Temp";
            }

            if (string.IsNullOrEmpty(sColumnNames))
            {
                MessageBox.Show("Please specify which columns you wish to select");
                this.BringToFront();
                this.Cursor = Cursors.Default;
                return;
            }

            // Table name should always be selected
            if (string.IsNullOrEmpty(sTableName))
            {
                MessageBox.Show("Please select a table to query from");
                this.BringToFront();
                this.Cursor = Cursors.Default;
                return;
            }

            SqlConnection dbConn = myADOFuncs.CreateSQLConnection(myConfig.GetConnectionString());
            
            // Decide whether or not there is a geometry field in the returned data.
            // Select the stored procedure accordingly
            string[] strGeometryFields = {"SP_GEOMETRY", "Shape" }; // Expand as required.
            bool blSpatial = false;
            foreach (string strField in strGeometryFields)
            {
                if (sColumnNames.ToLower().Contains(strField.ToLower()))
                    blSpatial = true;
            }

            // If "*" is used check for the existence of a geometry field in the table.
            if (sColumnNames == "*")
            {
                string strCheckTable = myConfig.GetDatabaseSchema() + "." + sTableName;
                dbConn.Open();
                foreach (string strField in strGeometryFields)
                {
                    if (myADOFuncs.FieldExists(ref dbConn, strCheckTable, strField))
                        blSpatial = true;
                }
                dbConn.Close();
            }

            // Set the temporary table names and the stored procedure names. Adjust output formats if required.
            bool blFlatTable = !blSpatial; // to start with
            string strStoredProcedure = "AFSelectSppSubset"; // Default for all data
            string strPolyFC = sTableName + "_poly_" + sUserID; ;
            string strPointFC = sTableName + "_point_" + sUserID;
            string strTable = sTableName + "_" + sUserID;
            string strSplit = "0";

            if (blSpatial)
            {
                strSplit = "1";
                if (sOutputFormat == "Geodatabase") sOutputFormat = "Geodatabase FC";
            }
            else
            {
                if (sOutputFormat == "Geodatabase") sOutputFormat = "Geodatabase Table";
                if (sOutputFormat == "Shapefile") sOutputFormat = "dBASE file";
            }

            // Get the output file name taking account of adjusted output formats.
            sOutputFile = "None";
            bool blDone = false;
            while (!blDone)
            {
                sOutputFile = myArcMapFuncs.GetOutputFileName(sOutputFormat, myConfig.GetDefaultExtractPath());
                if (sOutputFile != "None")
                {
                    // firstly check extensions are as should be.
                    string strExtensionTest = sOutputFile.Substring(sOutputFile.Length - 4, 4).Substring(0, 1);
                    Boolean blHasExtension = false;
                    if (strExtensionTest == ".") blHasExtension = true;

                    // if there isn't, put one one.
                    if (sOutputFormat == "Text file" && !blHasExtension)
                    {
                        sOutputFile = sOutputFile + ".csv";
                    }
                    else if (sOutputFormat == "dBASE file" && !blHasExtension)
                    {
                        sOutputFile = sOutputFile + ".dbf";
                    }
                    else if (sOutputFormat == "Shapefile" && !blHasExtension)
                    {
                        sOutputFile = sOutputFile + ".shp";
                    }
                    else if ((sOutputFormat.Contains("Geodatabase")) && (blHasExtension || !sOutputFile.Contains(".gdb"))) // It is a geodatabase file and should not have an extension.
                    {
                        MessageBox.Show("Please select a file geodatabase output file");
                        this.Cursor = Cursors.Default;
                        this.BringToFront();
                        return;
                    }
                    else if ((!sOutputFormat.Contains("Geodatabase")) && (sOutputFile.Contains(".gdb"))) // Trying to store a non-geoDB in a gdb
                    {
                        MessageBox.Show("Cannot store " + sOutputFormat + " inside a geodatabase. Please choose a different output location");
                        this.Cursor = Cursors.Default;
                        this.BringToFront();
                        return;
                    }
                }
                else
                    blDone = true; // user pressed cancel.

                if (blSpatial && blDone != true)
                {
                    // Check if the outputfile_point or outputfile_poly already exists. For dBase and text output the dialog does its own check.

                    string sTest1 = "";
                    string sTest2 = "";
                    if (sOutputFormat.Contains("Geodatabase"))
                    {
                        sTest1 = sOutputFile + "_Point";
                        sTest2 = sOutputFile + "_Poly";

                    }
                    else if (sOutputFormat == "Shapefile")
                    {
                        string strExtensionTest1 = sOutputFile.Substring(sOutputFile.Length - 4, 4).Substring(0, 1);
                        if (strExtensionTest1 == ".")
                        {
                            sTest1 = sOutputFile.Substring(0, sOutputFile.Length - 4) + "_Point.shp";
                            sTest2 = sOutputFile.Substring(0, sOutputFile.Length - 4) + "_Poly.shp";
                        }
                        else
                        {
                            sTest1 = sOutputFile + "_Point.shp";
                            sTest2 = sOutputFile + "_Poly.shp";
                        }
                    }
                    if (sOutputFormat.Contains("Geodatabase") || sOutputFormat == "Shapefile")
                    {
                        if (myArcMapFuncs.FeatureclassExists(sTest1) || myArcMapFuncs.FeatureclassExists(sTest2))
                        {
                            DialogResult dlResult1 = MessageBox.Show("The output file already exists. Do you want to overwrite it?", "Data Selector", MessageBoxButtons.YesNo);
                            if (dlResult1 == System.Windows.Forms.DialogResult.Yes)
                                blDone = true;
                        }
                        else
                            blDone = true;
                    }
                    else
                    {
                        // Check for dBase and CSV
                        if (sOutputFormat == "dBASE file")
                        // Basically if the user chose a text file with an extension, the dialog will already have given her feedback and we don't need to do this again.
                        {
                            if (myFileFuncs.FileExists(sOutputFile))
                            {
                                DialogResult dlResult1 = MessageBox.Show("The output file already exists. Do you want to overwrite it?", "Data Selector", MessageBoxButtons.YesNo);
                                if (dlResult1 == System.Windows.Forms.DialogResult.Yes)
                                    blDone = true;
                            }
                            else
                                blDone = true;
                        }
                        else
                            blDone = true; // Text file; already checked by dialog.

                    }
                }
                else if (blDone != true) // non-spatial, not done yet.
                {
                    // Test for the types of flat output.
                    if (sOutputFormat.Contains("Geodatabase"))
                    {
                        if (myArcMapFuncs.TableExists(sOutputFile))
                        {
                            DialogResult dlResult1 = MessageBox.Show("The output file already exists. Do you want to overwrite it?", "Data Selector", MessageBoxButtons.YesNo);
                            if (dlResult1 == System.Windows.Forms.DialogResult.Yes)
                                blDone = true;
                        }
                        else
                            blDone = true;
                    }
                    else if (sOutputFormat == "dBASE file")
                    // Basically if the user chose a text file, the dialog will already have given her feedback and we don't need to do this again.
                    {
                        if (myFileFuncs.FileExists(sOutputFile))
                        {
                            DialogResult dlResult1 = MessageBox.Show("The output file already exists. Do you want to overwrite it?", "Data Selector", MessageBoxButtons.YesNo);
                            if (dlResult1 == System.Windows.Forms.DialogResult.Yes)
                                blDone = true;
                        }
                        else
                            blDone = true;
                    }
                    else
                        blDone = true; // Text file; already checked by dialog.
                    
                }
                    
            }
            this.BringToFront();
            
            if (sOutputFile == "None")
            {
                // User has pressed Cancel. Bring original menu to the front.
                MessageBox.Show("Please select an output file");
                this.Cursor = Cursors.Default;
                return;
            }
            this.Focus();


            string strLayerName = myFileFuncs.GetFileName(sOutputFile);
            
            if (!sOutputFormat.Contains("Geodatabase"))
            {
                strLayerName = myFileFuncs.ReturnWithoutExtension(strLayerName);
            }

            // Now we are all set to go - do the process.
            // Set up all required parameters.
            //SqlConnection dbConn = myADOFuncs.CreateSQLConnection(myConfig.GetConnectionString());
            SqlCommand myCommand = myADOFuncs.CreateSQLCommand(ref dbConn, strStoredProcedure, CommandType.StoredProcedure); // Note pass connection by ref here.
            myADOFuncs.AddSQLParameter(ref myCommand, "Schema", sDefaultSchema);
            myADOFuncs.AddSQLParameter(ref myCommand, "SpeciesTable", sTableName);
            myADOFuncs.AddSQLParameter(ref myCommand, "ColumnNames", sColumnNames);
            myADOFuncs.AddSQLParameter(ref myCommand, "WhereClause", sWhereClause);
            myADOFuncs.AddSQLParameter(ref myCommand, "GroupByClause", sGroupClause);
            myADOFuncs.AddSQLParameter(ref myCommand, "OrderByClause", sOrderClause);
            myADOFuncs.AddSQLParameter(ref myCommand, "UserID", sUserID);
            myADOFuncs.AddSQLParameter(ref myCommand, "Split", strSplit);
            
            // Open SQL connection to database and
            // Run the stored procedure.
            bool blSuccess = true;
            try
            {
                dbConn.Open();
                string strRowsAffect = myCommand.ExecuteNonQuery().ToString();
                if (blSpatial)
                {
                    blSuccess = myADOFuncs.TableHasRows(ref dbConn, strPointFC);
                    if (!blSuccess)
                        blSuccess = myADOFuncs.TableHasRows(ref dbConn, strPolyFC);
                }
                else
                    blSuccess = myADOFuncs.TableHasRows(ref dbConn, strTable);
                dbConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not execute stored procedure. System returned the following message: " +
                    ex.Message);
                this.Cursor = Cursors.Default;
                this.BringToFront();
                dbConn.Close();
                return;
            }

  
            // convert the results to the designated output file.
            string strPointOutTab = myConfig.GetSDEName() + @"\" + strPointFC;
            string strPolyOutTab = myConfig.GetSDEName() + @"\" + strPolyFC; 
            string strOutTab = myConfig.GetSDEName() + @"\" + strTable; 

            string strOutPoints = "";
            string strOutPolys = "";


            bool blResult = false;
            if (blSpatial && blSuccess) 
            {
                // export points and polygons
                // How is the data to be exported?
                if (sOutputFormat == "Geodatabase FC") 
                {
                    // Easy, export without further ado.
                    strOutPoints = sOutputFile + "_Point";
                    strOutPolys = sOutputFile + "_Poly";
                    
                    blResult = myArcMapFuncs.CopyFeatures(strPointOutTab, strOutPoints);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting point geodatabase file");
                        this.Cursor = Cursors.Default;
                        this.BringToFront();
                        return;
                    }
                    blResult = myArcMapFuncs.CopyFeatures(strPolyOutTab, strOutPolys);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting polygon geodatabase file");
                        this.Cursor = Cursors.Default;
                        this.BringToFront();
                        return;
                    }
                    
                }
                else if (sOutputFormat == "Shapefile" & blSuccess)
                {
                    // Create file names first.
                    sOutputFile = myFileFuncs.ReturnWithoutExtension(sOutputFile);
                    strOutPoints = sOutputFile + "_Point.shp";
                    strOutPolys = sOutputFile + "_Poly.shp";
                    
                    blResult = myArcMapFuncs.CopyFeatures(strPointOutTab, strOutPoints);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting point shapefile");
                        this.Cursor = Cursors.Default;
                        this.BringToFront();
                        return;
                    }
                    blResult = myArcMapFuncs.CopyFeatures(strPolyOutTab, strOutPolys);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting polygon shapefile");
                        this.Cursor = Cursors.Default;
                        this.BringToFront();
                        return;
                    }
                }
                else if (sOutputFormat == "Text file" || sOutputFormat == "dBASE file")
                {
                    // Not a spatial export, but it is a spatial layer so there are two files.
                    // Function pulls them back together again.

                    // if schema.ini file exists delete it.
                    string strIniFile = myFileFuncs.GetDirectoryName(sOutputFile) + "\\schema.ini";
                    if (myFileFuncs.FileExists(strIniFile))
                    {
                        bool blDeleted = myFileFuncs.DeleteFile(strIniFile); // Not checking for success at the moment.
                    }

                    blFlatTable = true;
                    string sFinalFile = "";
                    if (sOutputFormat == "dBASE file")
                    {
                        sFinalFile = sOutputFile;
                        sOutputFile = myFileFuncs.GetDirectoryName(sOutputFile) + "\\Temp.csv";
                    }
                    blResult = myArcMapFuncs.CopyToCSV(strPointOutTab, sOutputFile, true, false, true);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting output table to text file " + sOutputFile);
                        this.Cursor = Cursors.Default;
                        this.BringToFront();
                        return;
                    }
                    // Also export the second table - append
                    blResult = myArcMapFuncs.CopyToCSV(strPolyOutTab, sOutputFile, true, true, true);
                    if (!blResult)
                    {
                        MessageBox.Show("Error appending output table to text file " + sOutputFile);
                        this.Cursor = Cursors.Default;
                        this.BringToFront();
                    }

                    // If the end output is a dBASE file, export the resulting csv to dBASE.
                    if (sOutputFormat == "dBASE file")
                    {
                        blResult = myArcMapFuncs.CopyTable(sOutputFile, sFinalFile);
                        // Delete csv file.
                        try
                        {
                            File.Delete(sOutputFile);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error deleting temporary text file: " + ex.Message);
                            this.Cursor = Cursors.Default;
                            this.BringToFront();
                        }
                        if (!blResult)
                        {
                            MessageBox.Show("Error exporting output table to dBASE file " + sFinalFile);
                            this.Cursor = Cursors.Default;
                            this.BringToFront();
                            return;
                        }
                        sOutputFile = sFinalFile;
                    }
                    else
                    {
                        myArcMapFuncs.AddTableLayerFromString(sOutputFile, strLayerName);
                    }
                }
            }
            else if (blSuccess) // Non-spatial query, successfully run.
            {
                if (sOutputFormat == "Text file")
                {
                    // We are exporting a non-spatial output to text file.
                    blResult = myArcMapFuncs.CopyToCSV(strOutTab, sOutputFile, false, false, true);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting output table to text file " + sOutputFile);
                        this.Cursor = Cursors.Default;
                        this.BringToFront();
                        return;
                    }
                    myArcMapFuncs.AddTableLayerFromString(sOutputFile, strLayerName);
                }
                else
                {
                    // We are exporting any non-spatial output to dbf or geodatabase.
                    blResult = myArcMapFuncs.CopyTable(strOutTab, sOutputFile);
                    if (!blResult)
                    {
                        MessageBox.Show("Error exporting output table");
                        this.Cursor = Cursors.Default;
                        this.BringToFront();
                        return;
                    }
                }
            }
            else if (!blSuccess)
            {
                if (sOutputFormat == "Text file")
                {
                    if (sColumnNames == "*")
                    {
                        dbConn.Open();
                        string[] strColumnNames = myADOFuncs.GetFieldNames(ref dbConn, sTableName);
                        dbConn.Close();
                        sColumnNames = "";
                        foreach (string strField in strColumnNames)
                        {
                            sColumnNames = sColumnNames + strField + ",";
                        }
                        // Remove last comma
                        sColumnNames = sColumnNames.Substring(0, sColumnNames.Length - 1);
                        myArcMapFuncs.WriteEmptyCSV(sOutputFile, sColumnNames);
                        MessageBox.Show("There were no results for the query. An empty text file has been created");
                    }
                }
                else
                    MessageBox.Show("There were no results for this query. No output has been created");
            }
            
            // Delete the temporary tables in the SQL database
            strStoredProcedure = "AFClearSppSubset";
            SqlCommand myCommand2 = myADOFuncs.CreateSQLCommand(ref dbConn, strStoredProcedure, CommandType.StoredProcedure); // Note pass connection by ref here.
            myADOFuncs.AddSQLParameter(ref myCommand2, "Schema", sDefaultSchema);
            myADOFuncs.AddSQLParameter(ref myCommand2, "SpeciesTable", sTableName);
            myADOFuncs.AddSQLParameter(ref myCommand2, "UserId", sUserID);
            try
            {
                dbConn.Open();
                string strRowsAffect = myCommand2.ExecuteNonQuery().ToString();
                dbConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not execute stored procedure. System returned the following message: " +
                    ex.Message);
                this.Cursor = Cursors.Default;
                this.BringToFront();
                return;
            }
            // Add the results to the screen.
            
            if (!blFlatTable && blSuccess) // Only truly spatial output has two files.
            {
                ILayer lyrPolys = myArcMapFuncs.GetLayer(strLayerName + "_Poly");
                ILayer lyrPoints = myArcMapFuncs.GetLayer(strLayerName + "_Point");
                myArcMapFuncs.MoveToGroupLayer(strLayerName, lyrPolys);
                myArcMapFuncs.MoveToGroupLayer(strLayerName, lyrPoints);
            }
            else if (blSuccess)
            {
                myArcMapFuncs.ShowTable(strLayerName);
            }
            
            this.Cursor = Cursors.Default;
            DialogResult dlResult = MessageBox.Show("Process complete. Do you wish to close the form?", "Data Selector", MessageBoxButtons.YesNo);
            if (dlResult == System.Windows.Forms.DialogResult.Yes)
                this.Close();
            else this.BringToFront();

            // Tidy up
            myCommand.Dispose();
            myCommand2.Dispose();
            dbConn.Dispose();

    
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IApplication theApplication = (IApplication)ArcMap.Application;
            //bool aTest = false;
            ArcMapFunctions myFuncs = new ArcMapFunctions(theApplication);
            //aTest = myFuncs.GroupLayerExists("myTest");
            //MessageBox.Show(aTest.ToString());
            //ILayer myLayer = myFuncs.GetLayer("HesTest");
            //myFuncs.MoveToGroupLayer("myTest", myLayer);
            //myFuncs.ShowTable("HesTab", true);
            //myFuncs.ShowTable("HesCSV", true);
            //SqlConnection dbConn = myADOFuncs.CreateSQLConnection(myConfig.GetConnectionString());
            //string strTable = myConfig.GetDatabaseSchema() + "." + "TVERC_Spp_Full";
            //dbConn.Open();
            //bool blTest = myADOFuncs.FieldExists(ref dbConn, strTable, "SP_GEOMETRY");
            //dbConn.Close();
            //MessageBox.Show(blTest.ToString());
            IObjectClass myObject = (IObjectClass)myFuncs.GetFeatureClass(@"H:\Dev\LERCAutomation\DataSelector---ArcObjects\Extracts\Testing.gdb",
                "Whitethroat_Poly");
            IClassSchemaEdit myEdit = (IClassSchemaEdit)myObject;
            myEdit.AlterFieldAliasName("SP_GEOMETRY", "Shape");
            myEdit.AlterFieldAliasName("SP_GEOMETRY_Area", "Shape_Area");
            myEdit.AlterFieldAliasName("SP_GEOMETRY_Length", "Shape_Length");
            MessageBox.Show("Field names changed");
               
        }
    }
}
