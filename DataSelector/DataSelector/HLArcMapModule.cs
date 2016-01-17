using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;

using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;

using HLFileFunctions;

namespace HLArcMapModule
{
    class ArcMapFunctions
    {
        #region Constructor
        private IApplication thisApplication;
        private FileFunctions myFileFuncs;
        // Class constructor.
        public ArcMapFunctions(IApplication theApplication)
        {
            // Set the application for the class to work with.
            // Note the application can be got at from a command / tool by using
            // IApplication pApp = ArcMap.Application - then pass pApp as an argument.
            this.thisApplication = theApplication;
            myFileFuncs = new FileFunctions();
        }
        #endregion

        public IMxDocument GetIMXDocument()
        {
            ESRI.ArcGIS.ArcMapUI.IMxDocument mxDocument = ((ESRI.ArcGIS.ArcMapUI.IMxDocument)(thisApplication.Document));
            return mxDocument;
        }

        public ESRI.ArcGIS.Carto.IMap GetMap()
        {
            if (thisApplication == null)
            {
                return null;
            }
            ESRI.ArcGIS.ArcMapUI.IMxDocument mxDocument = ((ESRI.ArcGIS.ArcMapUI.IMxDocument)(thisApplication.Document)); // Explicit Cast
            ESRI.ArcGIS.Carto.IActiveView activeView = mxDocument.ActiveView;
            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;

            return map;
        }

        public IWorkspaceFactory GetWorkspaceFactory(string aFilePath, bool Messages = false)
        {
            // This function decides what type of feature workspace factory would be best for this file.
            // it is up to the user to decide whether the file path and file names exist (or should exist).

            IWorkspaceFactory pWSF;
            // What type of output file it it? This defines what kind of workspace factory.
            if (aFilePath.Substring(aFilePath.Length - 4, 4) == ".gdb")
            {
                // It is a file geodatabase file.
                pWSF = new FileGDBWorkspaceFactory();
            }
            else if (aFilePath.Substring(aFilePath.Length - 4, 4) == ".mdb")
            {
                // Personal geodatabase.
                pWSF = new AccessWorkspaceFactory();
            }
            else
            {
                pWSF = new ShapefileWorkspaceFactory();
            }
            return pWSF;
        }

        public IFeatureClass GetFeatureClass(string aFilePath, string aDatasetName, bool Messages = false)
        // This is incredibly quick.
        {
            // Check input first.
            if (myFileFuncs.DirExists(aFilePath) == false || aDatasetName == null)
            {
                if (Messages) MessageBox.Show("Please provide valid input", "Open Feature Class from Disk");
                return null;
            }

            IWorkspaceFactory pWSF = GetWorkspaceFactory(aFilePath);
            IFeatureWorkspace pWS = (IFeatureWorkspace)pWSF.OpenFromFile(aFilePath, 0);
            IFeatureClass pFC = pWS.OpenFeatureClass(aDatasetName);
            if (pFC == null)
            {
                if (Messages) MessageBox.Show("The file " + aDatasetName + " doesn't exist in this location", "Open Feature Class from Disk");
                return null;
            }
            return pFC;
        }

        public bool AddLayerFromFClass(IFeatureClass theFeatureClass, bool Messages = false)
        {
            // Check we have input
            if (theFeatureClass == null)
            {
                if (Messages)
                {
                    MessageBox.Show("Please pass a feature class", "Add Layer From Feature Class");
                }
                return false;
            }
            IMap pMap = GetMap();
            if (pMap == null)
            {
                if (Messages)
                {
                    MessageBox.Show("No map found", "Add Layer From Feature Class");
                }
                return false;
            }
            FeatureLayer pFL = new FeatureLayer();
            pFL.FeatureClass = theFeatureClass;
            pFL.Name = theFeatureClass.AliasName;
            pMap.AddLayer(pFL);

            return true;
        }

        public bool AddFeatureLayerFromString(string aFeatureClassName, bool Messages = false)
        {
            // firstly get the Feature Class
            // Does it exist?
            if (!myFileFuncs.FileExists(aFeatureClassName))
            {
                if (Messages)
                {
                    MessageBox.Show("The featureclass " + aFeatureClassName + " does not exist");
                }
                return false;
            }
            string aFilePath = myFileFuncs.GetDirectoryName(aFeatureClassName);
            string aFCName = myFileFuncs.GetFileName(aFeatureClassName);

            IFeatureClass myFC = GetFeatureClass(aFilePath, aFCName);
            if (myFC == null)
            {
                if (Messages)
                {
                    MessageBox.Show("Cannot open featureclass " + aFeatureClassName);
                }
                return false;
            }

            // Now add it to the view.
            bool blResult = AddLayerFromFClass(myFC);
            if (blResult)
            {
                return true;
            }
            else
            {
                if (Messages)
                {
                    MessageBox.Show("Cannot add featureclass " + aFeatureClassName);
                }
                return false;
            }

        }

        public ITable GetTable(string aFilePath, string aDatasetName, bool Messages = false)
        {
            // Check input first.
            if (myFileFuncs.DirExists(aFilePath) == false || aDatasetName == null)
            {
                if (Messages) MessageBox.Show("Please provide valid input", "Open Feature Class from Disk");
                return null;
            }

            IWorkspaceFactory pWSF = GetWorkspaceFactory(aFilePath);
            IFeatureWorkspace pWS = (IFeatureWorkspace)pWSF.OpenFromFile(aFilePath, 0);
            ITable pTable = pWS.OpenTable(aDatasetName);
            if (pTable == null)
            {
                if (Messages) MessageBox.Show("The file " + aDatasetName + " doesn't exist in this location", "Open Table from Disk");
                return null;
            }
            return pTable;
        }

        public bool AddTableLayerFromString(string aTableName, bool Messages = false)
        {
            // firstly get the Feature Class
            // Does it exist?
            if (!myFileFuncs.FileExists(aTableName))
            {
                if (Messages)
                {
                    MessageBox.Show("The table " + aTableName + " does not exist");
                }
                return false;
            }
            string aFilePath = myFileFuncs.GetDirectoryName(aTableName);
            string aTabName = myFileFuncs.GetFileName(aTableName);

            ITable myTable = GetTable(aFilePath, aTabName);
            if (myTable == null)
            {
                if (Messages)
                {
                    MessageBox.Show("Cannot open table " + aTableName);
                }
                return false;
            }

            // Now add it to the view.
            bool blResult = AddLayerFromTable(myTable, aTabName);
            if (blResult)
            {
                return true;
            }
            else
            {
                if (Messages)
                {
                    MessageBox.Show("Cannot add table " + aTabName);
                }
                return false;
            }
        }

        public bool AddLayerFromTable(ITable theTable, string aName, bool Messages = false)
        {
            // check we have nput
            if (theTable == null)
            {
                if (Messages)
                {
                    MessageBox.Show("Please pass a table", "Add Layer From Table");
                }
                return false;
            }
            IMap pMap = GetMap();
            if (pMap == null)
            {
                if (Messages)
                {
                    MessageBox.Show("No map found", "Add Layer From Table");
                }
                return false;
            }
            IStandaloneTableCollection pStandaloneTableCollection = (IStandaloneTableCollection)pMap;
            IStandaloneTable pTable = new StandaloneTable();
            pTable.Table = theTable;
            pTable.Name = aName;

            IMxDocument mxDoc = GetIMXDocument();
            pStandaloneTableCollection.AddStandaloneTable(pTable);
            mxDoc.UpdateContents();
            return true;
        }

        public string GetOutputFileName(string aFileType, string anInitialDirectory = @"C:\")
        {
            // This would be done better with a custom type but this will do for the momment.
            IGxDialog myDialog = new GxDialogClass();
            myDialog.set_StartingLocation(anInitialDirectory);
            IGxObjectFilter myFilter;


            switch (aFileType)
            {
                case "Geodatabase FC":
                    myFilter = new GxFilterFGDBFeatureClasses();
                    break;
                case "Geodatabase Table":
                    myFilter = new GxFilterFGDBTables();
                    break;
                case "Shapefile":
                    myFilter = new GxFilterShapefiles();
                    break;
                case "DBASE file":
                    myFilter = new GxFilterdBASEFiles();
                    break;
                case "Text File":
                    myFilter = new GxFilterTextFiles();
                    break;
                default:
                    myFilter = new GxFilterDatasets();
                    break;
            }

            myDialog.ObjectFilter = myFilter;
            myDialog.Title = "Save Output As...";
            myDialog.ButtonCaption = "OK";

            if (myDialog.DoModalSave(thisApplication.hWnd))
            {
                return myDialog.Name;
            }
            else return "None"; // user pressed exit
            
        }


        #region CopyFeatures
        public bool CopyFeatures(string InFeatureClass, string OutFeatureClass, bool Messages = false)
        {
            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
            gp.OverwriteOutput = true;
            IGeoProcessorResult myresult = new GeoProcessorResultClass();
            object sev = null;

            // Create a variant array to hold the parameter values.
            IVariantArray parameters = new VarArrayClass();

            // Populate the variant array with parameter values.
            parameters.Add(InFeatureClass);
            parameters.Add(OutFeatureClass);

            // Execute the tool.
            try
            {
                myresult = (IGeoProcessorResult)gp.Execute("CopyFeatures_management", parameters, null);
                // Wait until the execution completes.
                while (myresult.Status == esriJobStatus.esriJobExecuting)
                    Thread.Sleep(1000);
                    // Wait for 1 second.
                if (Messages)
                {
                    MessageBox.Show("Process complete");
                }
                return true;
            }
            catch (Exception ex)
            {
                if (Messages)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show(gp.GetMessages(ref sev));
                }
                return false;
            }
        }

        public bool CopyFeatures(string InWorkspace, string InDatasetName, string OutFeatureClass, bool Messages = false)
        {
            string inFeatureClass = InWorkspace + @"\" + InDatasetName;
            return CopyFeatures(inFeatureClass, OutFeatureClass, Messages);
        }

        public bool CopyFeatures(string InWorkspace, string InDatasetName, string OutWorkspace, string OutDatasetName, bool Messages = false)
        {
            string inFeatureClass = InWorkspace + @"\" + InDatasetName;
            string outFeatureClass = OutWorkspace + @"\" + OutDatasetName;
            return CopyFeatures(inFeatureClass, outFeatureClass, Messages);
        }
        #endregion

        public bool CopyTable(string InTable, string OutTable, bool Messages = false)
        {
            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
            gp.OverwriteOutput = true;

            IGeoProcessorResult myresult = new GeoProcessorResultClass();

            // Create a variant array to hold the parameter values.
            IVariantArray parameters = new VarArrayClass();


            // Populate the variant array with parameter values.
            parameters.Add(InTable);
            parameters.Add(OutTable);

            // Execute the tool.
            try
            {
                myresult = (IGeoProcessorResult)gp.Execute("CopyRows_management", parameters, null);

                // Wait until the execution completes.
                while (myresult.Status == esriJobStatus.esriJobExecuting)
                    Thread.Sleep(1000);
                    // Wait for 1 second.
                if (Messages)
                {
                    MessageBox.Show("Process complete");
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
