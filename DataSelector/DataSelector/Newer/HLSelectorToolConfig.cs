using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using System.Windows.Forms;
using HLFileFunctions;
using DataSelector.Properties;


// This module reads the config XML file and stores the results

namespace HLSelectorToolConfig
{
    class SelectorToolConfig
    {
        // Declare all the variables
        string FileDSN;
        string ConnectionString;
        string LogFilePath;
        string DefaultExtractPath;
        string DefaultQueryPath;
        string DefaultFormat;
        string DatabaseSchema;
        string IncludeWildcard;
        string ExcludeWildcard;
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
            // Get the XML file path
            //string strXMLFile = strFolder + @"\AODataSelector.xml";
            string strXMLFile = Settings.Default.XMLFile;

            // If the XML file path is blank or doesn't exist
            if (String.IsNullOrEmpty(strXMLFile) || (!myFileFuncs.FileExists(strXMLFile)))
                {
                // Prompt the user for the correct file path
                string strFolder2 = GetConfigFilePath();
                if (!String.IsNullOrEmpty(strFolder2))
                    strXMLFile = strFolder2 + @"\DataSelector.xml";

                // Check the new file path exists
                if (myFileFuncs.FileExists(strXMLFile))
                {
                    Settings.Default.XMLFile = strXMLFile;
                    Settings.Default.Save();
                }
                else
                {
                    MessageBox.Show("XML File not found.");
                }
            }

            // Read the file.
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(strXMLFile);

            XmlNode currNode = xmlConfig.DocumentElement.FirstChild; // This gets us the DataSelector.
            xmlDataSelector = (XmlElement)currNode;

            // Get all of the detail into the object
            FileDSN = xmlDataSelector["FileDSN"].InnerText;
            ConnectionString = xmlDataSelector["ConnectionString"].InnerText;
            LogFilePath = xmlDataSelector["LogFilePath"].InnerText;
            DefaultExtractPath = xmlDataSelector["DefaultExtractPath"].InnerText;
            DefaultQueryPath = xmlDataSelector["DefaultQueryPath"].InnerText;
            DefaultFormat = xmlDataSelector["DefaultFormat"].InnerText;
            DatabaseSchema = xmlDataSelector["DatabaseSchema"].InnerText;
            IncludeWildcard = xmlDataSelector["IncludeWildcard"].InnerText;
            ExcludeWildcard = xmlDataSelector["ExcludeWildcard"].InnerText;
            RecMax = xmlDataSelector["RecMax"].InnerText;
            DefaultSetSymbology = xmlDataSelector["DefaultSetSymbology"].InnerText;
            LayerLocation = xmlDataSelector["LayerLocation"].InnerText;
            EnableSpatialPlotting = xmlDataSelector["EnableSpatialPlotting"].InnerText;
        }

        // Function to prompt the user for the XML file path.
        private string GetConfigFilePath()
        {
            // Create folder dialog.
            FolderBrowserDialog xmlFolder = new FolderBrowserDialog();

            // Show folder dialog.
            if (xmlFolder.ShowDialog() == DialogResult.OK)
            {
                // Return the selected path.
                return xmlFolder.SelectedPath;
            }
            else
                return null;
        }


        // Functions to return each element under here.
        public string GetSDEName()
        {
            return FileDSN;
        }

        public string GetConnectionString()
        {
            return ConnectionString;
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

        public string GetIncludeWildcard()
        {
            return IncludeWildcard;
        }

        public string GetExcludeWildcard()
        {
            return ExcludeWildcard;
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
