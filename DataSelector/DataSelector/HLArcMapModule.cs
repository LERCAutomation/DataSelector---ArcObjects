// DataSelector is an ArcGIS add-in used to extract biodiversity
// information from SQL Server based on any selection criteria.
//
// Copyright © 2016 Sussex Biodiversity Record Centre
//
// This file is part of DataSelector.
//
// DataSelector is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// DataSelector is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with DataSelector.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;

using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseUI;

using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesOleDB;

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

        public void RefreshTOC()
        {
            IMxDocument theDoc = GetIMXDocument();
            theDoc.CurrentContentsView.Refresh(null);
        }

        public IWorkspaceFactory GetWorkspaceFactory(string aFilePath, bool aTextFile = false, bool Messages = false)
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
            else if (aFilePath.Substring(aFilePath.Length - 4, 4) == ".sde")
            {
                // ArcSDE connection
                pWSF = new SdeWorkspaceFactory();
            }
            else if (aTextFile == true)
            {
                // Text file
                pWSF = new TextFileWorkspaceFactory();
            }
            else
            {
                pWSF = new ShapefileWorkspaceFactory();
            }
            return pWSF;
        }


        #region FeatureclassExists
        public bool FeatureclassExists(string aFilePath, string aDatasetName)
        {
            
            if (aDatasetName.Substring(aDatasetName.Length - 4, 1) == ".")
            {
                // it's a file.
                if (myFileFuncs.FileExists(aFilePath + @"\" + aDatasetName))
                    return true;
                else
                    return false;
            }
            else if (aFilePath.Substring(aFilePath.Length - 3, 3) == "sde")
            {
                // It's an SDE class
                // Not handled. We know the table exists.
                return true;
            }
            else // it is a geodatabase class.
            {
                IWorkspaceFactory pWSF = GetWorkspaceFactory(aFilePath);
                IWorkspace2 pWS = (IWorkspace2)pWSF.OpenFromFile(aFilePath, 0);
                if (pWS.get_NameExists(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTFeatureClass, aDatasetName))
                    return true;
                else
                    return false;
            }
        }

        public bool FeatureclassExists(string aFullPath)
        {
            return FeatureclassExists(myFileFuncs.GetDirectoryName(aFullPath), myFileFuncs.GetFileName(aFullPath));
        }
        #endregion

        #region GetFeatureClass
        public IFeatureClass GetFeatureClass(string aFilePath, string aDatasetName, bool Messages = false)
        // This is incredibly quick.
        {
            // Check input first.
            string aTestPath = aFilePath;
            if (aFilePath.Contains(".sde"))
            {
                aTestPath = myFileFuncs.GetDirectoryName(aFilePath);
            }
            if (myFileFuncs.DirExists(aTestPath) == false || aDatasetName == null)
            {
                if (Messages) MessageBox.Show("Please provide valid input", "Get Featureclass");
                return null;
            }
            

            IWorkspaceFactory pWSF = GetWorkspaceFactory(aFilePath);
            IFeatureWorkspace pWS = (IFeatureWorkspace)pWSF.OpenFromFile(aFilePath, 0);
            if (FeatureclassExists(aFilePath, aDatasetName))
            {
                IFeatureClass pFC = pWS.OpenFeatureClass(aDatasetName);
                return pFC;
            }
            else
            {
                if (Messages) MessageBox.Show("The file " + aDatasetName + " doesn't exist in this location", "Open Feature Class from Disk");
                return null;
            }
            
        }


        public IFeatureClass GetFeatureClass(string aFullPath, bool Messages = false)
        {
            string aFilePath = myFileFuncs.GetDirectoryName(aFullPath);
            string aDatasetName = myFileFuncs.GetFileName(aFullPath);
            IFeatureClass pFC = GetFeatureClass(aFilePath, aDatasetName, Messages);
            return pFC;
        }

        #endregion

        public IFeatureLayer GetFeatureLayerFromString(string aFeatureClassName, bool Messages = false)
        {
            // as far as I can see this does not work for geodatabase files.
            // firstly get the Feature Class
            // Does it exist?
            if (!myFileFuncs.FileExists(aFeatureClassName))
            {
                if (Messages)
                {
                    MessageBox.Show("The featureclass " + aFeatureClassName + " does not exist");
                }
                return null;
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
                return null;
            }

            // Now get the Feature Layer from this.
            FeatureLayer pFL = new FeatureLayer();
            pFL.FeatureClass = myFC;
            pFL.Name = myFC.AliasName;
            return pFL;
        }

        public ILayer GetLayer(string aName, bool Messages = false)
        {
            // Gets existing layer in map.
            // Check there is input.
           if (aName == null)
           {
               if (Messages)
               {
                   MessageBox.Show("Please pass a valid layer name", "Find Layer By Name");
               }
               return null;
            }
        
            // Get map, and layer names.
            IMap pMap = GetMap();
            if (pMap == null)
            {
                if (Messages)
                {
                    MessageBox.Show("No map found", "Find Layer By Name");
                }
                return null;
            }
            IEnumLayer pLayers = pMap.Layers;
            Boolean blFoundit = false;
            ILayer pTargetLayer = null;

            ILayer pLayer = pLayers.Next();

            // Look through the layers and carry on until found,
            // or we have reached the end of the list.
            while ((pLayer != null) && !blFoundit)
            {
                if (!(pLayer is ICompositeLayer))
                {
                    if (pLayer.Name == aName)
                    {
                        pTargetLayer = pLayer;
                        blFoundit = true;
                    }
                }
                pLayer = pLayers.Next();
            }

            if (pTargetLayer == null)
            {
                if (Messages) MessageBox.Show("The layer " + aName + " doesn't exist", "Find Layer");
                return null;
            }
            return pTargetLayer;
        }

        public bool FieldExists(string aFilePath, string aDatasetName, string aFieldName, bool Messages = false)
        {
            // This function returns true if a field (or a field alias) exists, false if it doesn (or the dataset doesn't)
            IFeatureClass myFC = GetFeatureClass(aFilePath, aDatasetName);
            ITable myTab;
            if (myFC == null)
            {
                myTab = GetTable(aFilePath, aDatasetName);
                if (myTab == null) return false; // Dataset doesn't exist.
            }
            else
            {
                myTab = (ITable)myFC;
            }

            int aTest;
            IFields theFields = myTab.Fields;
            aTest = theFields.FindField(aFieldName);
            if (aTest == -1)
            {
                aTest = theFields.FindFieldByAliasName(aFieldName);
            }

            if (aTest == -1) return false;
            return true;
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

        #region TableExists
        public bool TableExists(string aFilePath, string aDatasetName)
        {

            if (aDatasetName.Substring(aDatasetName.Length - 4, 1) == ".")
            {
                // it's a file.
                if (myFileFuncs.FileExists(aFilePath + @"\" + aDatasetName))
                    return true;
                else
                    return false;
            }
            else if (aFilePath.Substring(aFilePath.Length - 3, 3) == "sde")
            {
                // It's an SDE class
                // Not handled. We know the table exists.
                return true;
            }
            else // it is a geodatabase class.
            {
                IWorkspaceFactory pWSF = GetWorkspaceFactory(aFilePath);
                IWorkspace2 pWS = (IWorkspace2)pWSF.OpenFromFile(aFilePath, 0);
                if (pWS.get_NameExists(ESRI.ArcGIS.Geodatabase.esriDatasetType .esriDTTable, aDatasetName))
                    return true;
                else
                    return false;
            }
        }

        public bool TableExists(string aFullPath)
        {
            return TableExists(myFileFuncs.GetDirectoryName(aFullPath), myFileFuncs.GetFileName(aFullPath));
        }
        #endregion

        #region GetTable
        public ITable GetTable(string aFilePath, string aDatasetName, bool Messages = false)
        {
            // Check input first.
            string aTestPath = aFilePath;
            if (aFilePath.Contains(".sde"))
            {
                aTestPath = myFileFuncs.GetDirectoryName(aFilePath);
            }
            if (myFileFuncs.DirExists(aTestPath) == false || aDatasetName == null)
            {
                if (Messages) MessageBox.Show("Please provide valid input", "Get Table");
                return null;
            }
            bool blText = false;
            string strExt = aDatasetName.Substring(aDatasetName.Length - 4, 4);
            if (strExt == ".txt" || strExt == ".csv" || strExt == ".tab")
            {
                blText = true;
            }

            IWorkspaceFactory pWSF = GetWorkspaceFactory(aFilePath, blText);
            IFeatureWorkspace pWS = (IFeatureWorkspace)pWSF.OpenFromFile(aFilePath, 0);
            ITable pTable = pWS.OpenTable(aDatasetName);
            if (pTable == null)
            {
                if (Messages) MessageBox.Show("The file " + aDatasetName + " doesn't exist in this location", "Open Table from Disk");
                return null;
            }
            return pTable;
        }

        public ITable GetTable(string aTableLayer, bool Messages = false)
        {
            IMap pMap = GetMap();
            IStandaloneTableCollection pColl = (IStandaloneTableCollection)pMap;
            IStandaloneTable pThisTable = null;

            for (int I = 0; I < pColl.StandaloneTableCount; I++)
            {
                pThisTable = pColl.StandaloneTable[I];
                if (pThisTable.Name == aTableLayer)
                {
                    ITable myTable = pThisTable.Table;
                    return myTable;
                }
            }
            if (Messages)
            {
                MessageBox.Show("The table layer " + aTableLayer + " could not be found in this map");
            }
            return null;
        }
        #endregion

        public bool AddTableLayerFromString(string aTableName, string aLayerName, bool Messages = false)
        {
            // firstly get the Table
            // Does it exist? // Does not work for GeoDB tables!!
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
            bool blResult = AddLayerFromTable(myTable, aLayerName);
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
            IMxDocument mxDoc = GetIMXDocument();

            pTable.Table = theTable;
            pTable.Name = aName;

            // Remove if already exists
            if (TableLayerExists(aName))
                RemoveStandaloneTable(aName);

            mxDoc.UpdateContents();
            
            pStandaloneTableCollection.AddStandaloneTable(pTable);
            mxDoc.UpdateContents();
            return true;
        }

        public bool TableLayerExists(string aLayerName, bool Messages = false)
        {
            // Check there is input.
            if (aLayerName == null)
            {
                if (Messages) MessageBox.Show("Please pass a valid layer name", "Find Layer By Name");
                return false;
            }

            // Get map, and layer names.
            IMxDocument mxDoc = GetIMXDocument();
            IMap pMap = GetMap();
            if (pMap == null)
            {
                if (Messages) MessageBox.Show("No map found", "Find Layer By Name");
                return false;
            }

            IStandaloneTableCollection pColl = (IStandaloneTableCollection)pMap;
            IStandaloneTable pThisTable = null;
            for (int I = 0; I < pColl.StandaloneTableCount; I++)
            {
                pThisTable = pColl.StandaloneTable[I];
                if (pThisTable.Name == aLayerName)
                {
                    return true;
                    //pColl.RemoveStandaloneTable(pThisTable);
                   // mxDoc.UpdateContents();
                    //break; // important: get out now, the index is no longer valid
                }
            }
            return false;
        }

        public bool RemoveStandaloneTable(string aTableName, bool Messages = false)
        {
            // Check there is input.
            if (aTableName == null)
            {
                if (Messages) MessageBox.Show("Please pass a valid layer name", "Find Layer By Name");
                return false;
            }

            // Get map, and layer names.
            IMxDocument mxDoc = GetIMXDocument();
            IMap pMap = GetMap();
            if (pMap == null)
            {
                if (Messages) MessageBox.Show("No map found", "Find Layer By Name");
                return false;
            }

            IStandaloneTableCollection pColl = (IStandaloneTableCollection)pMap;
            IStandaloneTable pThisTable = null;
            for (int I = 0; I < pColl.StandaloneTableCount; I++)
            {
                pThisTable = pColl.StandaloneTable[I];
                if (pThisTable.Name == aTableName)
                {
                    try
                    {
                        pColl.RemoveStandaloneTable(pThisTable);
                        mxDoc.UpdateContents();
                        return true; // important: get out now, the index is no longer valid
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            return false;
        }


        public bool LayerExists(string aLayerName, bool Messages = false)
        {
            // Check there is input.
            if (aLayerName == null)
            {
                if (Messages) MessageBox.Show("Please pass a valid layer name", "Find Layer By Name");
                return false;
            }

            // Get map, and layer names.
            IMap pMap = GetMap();
            if (pMap == null)
            {
                if (Messages) MessageBox.Show("No map found", "Find Layer By Name");
                return false;
            }
            IEnumLayer pLayers = pMap.Layers;

            ILayer pLayer = pLayers.Next();

            // Look through the layers and carry on until found,
            // or we have reached the end of the list.
            while (pLayer != null)
            {
                if (!(pLayer is IGroupLayer))
                {
                    if (pLayer.Name == aLayerName)
                    {
                        return true;
                    }

                }
                pLayer = pLayers.Next();
            }
            return false;
        }

        public bool GroupLayerExists(string aGroupLayerName, bool Messages = false)
        {
            // Check there is input.
            if (aGroupLayerName == null)
            {
                if (Messages) MessageBox.Show("Please pass a valid layer name", "Find Layer By Name");
                return false;
            }

            // Get map, and layer names.
            IMap pMap = GetMap();
            if (pMap == null)
            {
                if (Messages) MessageBox.Show("No map found", "Find Layer By Name");
                return false;
            }
            IEnumLayer pLayers = pMap.Layers;

            ILayer pLayer = pLayers.Next();

            // Look through the layers and carry on until found,
            // or we have reached the end of the list.
            while (pLayer != null)
            {
                if (pLayer is IGroupLayer)
                {
                    if (pLayer.Name == aGroupLayerName)
                    {
                        return true;
                    }

                }
                pLayer = pLayers.Next();
            }
            return false;
        }

        public ILayer GetGroupLayer(string aGroupLayerName, bool Messages = false)
        {
            // Check there is input.
            if (aGroupLayerName == null)
            {
                if (Messages) MessageBox.Show("Please pass a valid layer name", "Find Layer By Name");
                return null;
            }

            // Get map, and layer names.
            IMap pMap = GetMap();
            if (pMap == null)
            {
                if (Messages) MessageBox.Show("No map found", "Find Layer By Name");
                return null;
            }
            IEnumLayer pLayers = pMap.Layers;

            ILayer pLayer = pLayers.Next();

            // Look through the layers and carry on until found,
            // or we have reached the end of the list.
            while (pLayer != null)
            {
                if (pLayer is IGroupLayer)
                {
                    if (pLayer.Name == aGroupLayerName)
                    {
                        return pLayer;
                    }

                }
                pLayer = pLayers.Next();
            }
            return null;
        }      
        
        public bool MoveToGroupLayer(string theGroupLayerName, ILayer aLayer,  bool Messages = false)
        {
            bool blExists = false;
            IGroupLayer myGroupLayer = new GroupLayer(); 
            // Does the group layer exist?
            if (GroupLayerExists(theGroupLayerName))
            {
                myGroupLayer = (IGroupLayer)GetGroupLayer(theGroupLayerName);
                blExists = true;
            }
            else
            {
                myGroupLayer.Name = theGroupLayerName;
            }
            string theOldName = aLayer.Name;

            // Remove the original instance, then add it to the group.
            RemoveLayer(aLayer);
            myGroupLayer.Add(aLayer);
            
            if (!blExists)
            {
                // Add the layer to the map.
                IMap pMap = GetMap();
                pMap.AddLayer(myGroupLayer);
            }
            RefreshTOC();
            return true;
        }

        #region RemoveLayer
        public bool RemoveLayer(string aLayerName, bool Messages = false)
        {
            // Check there is input.
            if (aLayerName == null)
            {
                MessageBox.Show("Please pass a valid layer name", "Find Layer By Name");
                return false;
            }

            // Get map, and layer names.
            IMap pMap = GetMap();
            if (pMap == null)
            {
                if (Messages) MessageBox.Show("No map found", "Find Layer By Name");
                return false;
            }
            IEnumLayer pLayers = pMap.Layers;

            ILayer pLayer = pLayers.Next();

            // Look through the layers and carry on until found,
            // or we have reached the end of the list.
            while (pLayer != null)
            {
                if (!(pLayer is IGroupLayer))
                {
                    if (pLayer.Name == aLayerName)
                    {
                        pMap.DeleteLayer(pLayer);
                        return true;
                    }

                }
                pLayer = pLayers.Next();
            }
            return false;
        }

        public bool RemoveLayer(ILayer aLayer, bool Messages = false)
        {
            // Check there is input.
            if (aLayer == null)
            {
                MessageBox.Show("Please pass a valid layer ", "Remove Layer");
                return false;
            }

            // Get map, and layer names.
            IMap pMap = GetMap();
            if (pMap == null)
            {
                if (Messages) MessageBox.Show("No map found", "Remove Layer");
                return false;
            }
            pMap.DeleteLayer(aLayer);
            return true;
        }
        #endregion


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
                case "Text file":
                    myFilter = new GxFilterTextFiles();
                    break;
                default:
                    myFilter = new GxFilterDatasets();
                    break;
            }

            myDialog.ObjectFilter = myFilter;
            myDialog.Title = "Save Output As...";
            myDialog.ButtonCaption = "OK";

            string strOutFile = "None";
            if (myDialog.DoModalSave(thisApplication.hWnd))
            {
                strOutFile = myDialog.FinalLocation.FullName + @"\" + myDialog.Name;
                
            }
            myDialog = null;
            return strOutFile; // "None" if user pressed exit
            
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
                gp = null;
                return true;
            }
            catch (Exception ex)
            {
                if (Messages)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show(gp.GetMessages(ref sev));
                }
                gp = null;
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
            // This works absolutely fine for dbf and geodatabase but does not export to CSV.

            // Note the csv export already removes ghe geometry field; in this case it is not necessary to check again.

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
                gp = null;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                gp = null;
                return false;
            }
        }

        public bool AlterFieldAliasName(string aDatasetName, string aFieldName, string theAliasName, bool Messages = false)
        {
            // This script changes the field alias of a the named field in the layer.
            IObjectClass myObject = (IObjectClass)GetFeatureClass(aDatasetName);
            IClassSchemaEdit myEdit = (IClassSchemaEdit)myObject;
            try
            {
                myEdit.AlterFieldAliasName(aFieldName, theAliasName);
                myObject = null;
                myEdit = null;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                myObject = null;
                myEdit = null;
                return false;
            }
        }

        public IField getFCField(string InputDirectory, string FeatureclassName, string FieldName, bool Messages = false)
        {
            IFeatureClass featureClass = GetFeatureClass(InputDirectory, FeatureclassName);
            // Find the index of the requested field.
            int fieldIndex = featureClass.FindField(FieldName);

            // Get the field from the feature class's fields collection.
            if (fieldIndex > -1)
            {
                IFields fields = featureClass.Fields;
                IField field = fields.get_Field(fieldIndex);
                return field;
            }
            else
            {
                if (Messages)
                {
                    MessageBox.Show("The field " + FieldName + " was not found in the featureclass " + FeatureclassName);
                }
                return null;
            }
        }

        public IField getTableField(string TableName, string FieldName, bool Messages = false)
        {
            ITable theTable = GetTable(myFileFuncs.GetDirectoryName(TableName), myFileFuncs.GetFileName(TableName), Messages);
            int fieldIndex = theTable.FindField(FieldName);

            // Get the field from the feature class's fields collection.
            if (fieldIndex > -1)
            {
                IFields fields = theTable.Fields;
                IField field = fields.get_Field(fieldIndex);
                return field;
            }
            else
            {
                if (Messages)
                {
                    MessageBox.Show("The field " + FieldName + " was not found in the table " + myFileFuncs.GetFileName(TableName));
                }
                return null;
            }
        }

        public bool AppendTable(string InTable, string TargetTable, bool Messages = false)
        {
            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
            gp.OverwriteOutput = true;

            IGeoProcessorResult myresult = new GeoProcessorResultClass();

            // Create a variant array to hold the parameter values.
            IVariantArray parameters = new VarArrayClass();


            // Populate the variant array with parameter values.
            parameters.Add(InTable);
            parameters.Add(TargetTable);

            // Execute the tool. Note this only works with geodatabase tables.
            try
            {
                myresult = (IGeoProcessorResult)gp.Execute("Append_management", parameters, null);

                // Wait until the execution completes.
                while (myresult.Status == esriJobStatus.esriJobExecuting)
                    Thread.Sleep(1000);
                // Wait for 1 second.
                if (Messages)
                {
                    MessageBox.Show("Process complete");
                }
                gp = null;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                gp = null;
                return false;
            }
        }

        public bool CopyToCSV(string InTable, string OutTable, bool Spatial, bool Append, bool Messages = false)
        {
            // This sub copies the input table to CSV.
            string aFilePath = myFileFuncs.GetDirectoryName(InTable);
            string aTabName = myFileFuncs.GetFileName(InTable);
            
            ICursor myCurs = null;
            IFields fldsFields = null;
            if (Spatial)
            {
                
                IFeatureClass myFC = GetFeatureClass(aFilePath, aTabName, true); 
                myCurs = (ICursor)myFC.Search(null, false);
                fldsFields = myFC.Fields;
            }
            else
            {
                ITable myTable = GetTable(aFilePath, aTabName, true);
                myCurs = myTable.Search(null, false);
                fldsFields = myTable.Fields;
            }

            if (myCurs == null)
            {
                if (Messages)
                {
                    MessageBox.Show("Cannot open table " + InTable);
                }
                return false;
            }

            // Open output file.
            StreamWriter theOutput = new StreamWriter(OutTable, Append);

            string strField = null;
            string strHeader = "";
            int intFieldCount = fldsFields.FieldCount;
            int intIgnore = -1;
            
            // iterate through the fields in the collection to create header.
            for (int i = 0; i < intFieldCount; i++)
            {
                // Get the field at the given index.
                strField = fldsFields.get_Field(i).Name;
                if (strField == "SP_GEOMETRY" || strField == "Shape")
                    intIgnore = i;
                else
                    strHeader = strHeader + strField + ",";
            }
            if (!Append)
            {
                // Write the header.
                strHeader = strHeader.Substring(0, strHeader.Length - 1);
                theOutput.WriteLine(strHeader);
            }
            // Now write the file.
            IRow aRow = myCurs.NextRow();
            //MessageBox.Show("Writing ...");
            while (aRow != null)
            {
                string strRow = "";
                for (int i = 0; i < intFieldCount; i++)
                {
                    if (i != intIgnore)
                    {
                        var theValue = aRow.get_Value(i);
                        // Wrap value if quotes if it is a string that contains a comma
                        if ((theValue is string) &&
                           (theValue.ToString().Contains(","))) theValue = "\"" + theValue.ToString() + "\"";
                        strRow = strRow + theValue.ToString();
                        if (i < intFieldCount - 1) strRow = strRow + ",";
                    }
                }
                theOutput.WriteLine(strRow);
                aRow = myCurs.NextRow();
            }

            theOutput.Close();
            theOutput.Dispose();
            myCurs = null;
            aRow = null;
            return true;
        }

        public bool WriteEmptyCSV(string OutTable, string theHeader)
        {
            // Open output file.
            StreamWriter theOutput = new StreamWriter(OutTable, false);
            theOutput.Write(theHeader);
            theOutput.Close();
            theOutput.Dispose();
            return true;
        }

        public void ShowTable(string aTableName, bool Messages = false)
        {
            if (aTableName == null)
            {
                if (Messages) MessageBox.Show("Please pass a table name", "Show Table");
                return;
            }

            ITable myTable = GetTable(aTableName);
            if (myTable == null)
            {
                if (Messages)
                {
                    MessageBox.Show("Table " + aTableName + " not found in map");
                    return;
                }
            }

            ITableWindow myWin = new TableWindow();
            myWin.Table = myTable;
            myWin.Application = thisApplication;
            myWin.Show(true);
        }
    }
}
