using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using System.Windows.Forms;
using HLFileFunctions;

// This module reads the config XML file and stores the results

namespace HLSelectorToolConfig
{
    class SelectorToolConfig
    {
        // Declare all the variables
        string FileDSN;
        string LogFilePath;
        string DefaultExtractPath;
        string DefaultQueryPath;
        string DefaultFormat;
        string DatabaseSchema;
        string TableListSQL;
        string RecMax;
        string DefaultSetSymbology;
        string LayerLocation;
        string EnableSpatialPlotting; // do not currently need this but keeping for reference.

        // Initialise component - read XML
        FileFunctions myFileFuncs;
        XmlElement xmlDataSelector;
        public SelectorToolConfig()
        {
            // Open xml
            myFileFuncs = new FileFunctions();
            string strAppDLL = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strFolder = myFileFuncs.GetDirectoryName(strAppDLL);
            // Get the XML file
            string strXMLFile = strFolder + @"\AODataSelector.xml";
            
            // Read the file.
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(strXMLFile);

            XmlNode currNode = xmlConfig.DocumentElement.FirstChild; // This gets us the DataSelector.
            xmlDataSelector = (XmlElement)currNode;

            // Get all of the detail into the object
            FileDSN = xmlDataSelector["FileDSN"].InnerText;
            LogFilePath = xmlDataSelector["LogFilePath"].InnerText;
            DefaultExtractPath = xmlDataSelector["DefaultExtractPath"].InnerText;
            DefaultQueryPath = xmlDataSelector["DefaultQueryPath"].InnerText;
            DefaultFormat = xmlDataSelector["DefaultFormat"].InnerText;
            DatabaseSchema = xmlDataSelector["DatabaseSchema"].InnerText;
            TableListSQL = xmlDataSelector["TableListSQL"].InnerText;
            RecMax = xmlDataSelector["RecMax"].InnerText;
            DefaultSetSymbology = xmlDataSelector["DefaultSetSymbology"].InnerText;
            LayerLocation = xmlDataSelector["LayerLocation"].InnerText;
            EnableSpatialPlotting = xmlDataSelector["EnableSpatialPlotting"].InnerText;


        }

        // Functions to return each element under here.
        public string GetSDEName()
        {
            return FileDSN;
        }

        public string GetLogFilePath()
        {
            return LogFilePath;
        }

        public string GetDefaultExtractPath()
        {
            return DefaultExtractPath;
        }

        public string GetDefaultQueryPath()
        {
            return DefaultQueryPath;
        }

        public string GetDefaultFormat()
        {
            return DefaultFormat;
        }

        public string GetDatabaseSchema()
        {
            return DatabaseSchema;
        }

        public string GetTableListSQL()
        {
            return TableListSQL;
        }

        public string GetRecMax()
        {
            return RecMax;
        }

        public string GetDefaultSetSymbology()
        {
            return DefaultSetSymbology;
        }

        public string GetLayerLocation()
        {
            return LayerLocation;
        }

        public string GetEnableSpatialPlotting()
        {
            return EnableSpatialPlotting;
        }


    }
}
