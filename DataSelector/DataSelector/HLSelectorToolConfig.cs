// DataSelector is an ArcGIS add-in used to extract biodiversity
// information from SQL Server based on any selection criteria.
//
// Copyright © 2016-2017 SxBRC, 2017-2018 TVERC
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
using System.Xml;
using System.Windows.Forms;

using DataSelector.Properties;
using HLFileFunctions;
using HLStringFunctions;


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
        bool DefaultClearLogFile;
        int TimeOutSeconds;
        bool FoundXML;
        bool LoadedXML;

        // Initialise component - read XML
        FileFunctions myFileFuncs;
        StringFunctions myStringFuncs;
        XmlElement xmlDataSelector;
        public SelectorToolConfig(string anXMLProfile)
        {
            // Open xml
            myFileFuncs = new FileFunctions();
            myStringFuncs = new StringFunctions();
            string strXMLFile = anXMLProfile; // The user has specified this and we've checked it exists.
            FoundXML = true; // In this version we have already checked that it exists.
            LoadedXML = true;
            // Now get all the config variables.
            // Read the file.
            if (FoundXML)
            {

                XmlDocument xmlConfig = new XmlDocument();
                try
                {
                    xmlConfig.Load(strXMLFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in XML file; cannot load. System error message: " + ex.Message, "XML Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadedXML = false;
                    return;
                }

                string strRawText;
                XmlNode currNode = xmlConfig.DocumentElement.FirstChild; // This gets us the DataSelector.
                xmlDataSelector = (XmlElement)currNode;

                // Get all of the detail into the object
                try
                {
                    LogFilePath = xmlDataSelector["LogFilePath"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'LogFilePath' in the XML file");
                    LoadedXML = false;
                    return;
                }
                try
                {
                    FileDSN = xmlDataSelector["FileDSN"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'FileDSN' in the XML file");
                    LoadedXML = false;
                    return;
                }

                try
                {
                    ConnectionString = xmlDataSelector["ConnectionString"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'ConnectionString' in the XML file");
                    LoadedXML = false;
                    return;
                }

                try
                {
                    string strTimeout = xmlDataSelector["TimeoutSeconds"].InnerText;
                    bool blSuccess;

                    if (strTimeout != "")
                    {

                        blSuccess = int.TryParse(strTimeout, out TimeOutSeconds);
                        if (!blSuccess)
                        {
                            MessageBox.Show("The value entered for TimeoutSeconds in the XML file is not an integer number");
                            LoadedXML = false;
                        }
                        if (TimeOutSeconds < 0)
                        {
                            MessageBox.Show("The value entered for TimeoutSeconds in the XML file is negative");
                            LoadedXML = false;
                        }
                    }
                    else
                    {
                        TimeOutSeconds = 0; // None given.
                    }

                }
                catch
                {
                    TimeOutSeconds = 0; // We don't really care if it's not in because there's a default anyway.
                    return;
                }

                try
                {
                    DefaultExtractPath = xmlDataSelector["DefaultExtractPath"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'DefaultExtractPath' in the XML file");
                    LoadedXML = false;
                    return;
                }
                try
                {
                    DefaultQueryPath = xmlDataSelector["DefaultQueryPath"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'DefaultQueryPath' in the XML file");
                    LoadedXML = false;
                    return;
                }
                try
                {
                    DefaultFormat = xmlDataSelector["DefaultFormat"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'DefaultFormat' in the XML file");
                    LoadedXML = false;
                    return;
                }
                try
                {
                    DatabaseSchema = xmlDataSelector["DatabaseSchema"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'DatabaseSchema' in the XML file");
                    LoadedXML = false;
                    return;
                }
                try
                {
                    IncludeWildcard = xmlDataSelector["IncludeWildcard"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'IncludeWildcard' in the XML file");
                    LoadedXML = false;
                    return;
                }
                try
                {
                    ExcludeWildcard = xmlDataSelector["ExcludeWildcard"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'ExcludeWildcard' in the XML file");
                    LoadedXML = false;
                    return;
                }
                try
                {
                    RecMax = xmlDataSelector["RecMax"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'RecMax' in the XML file");
                    LoadedXML = false;
                    return;
                }
                try
                {
                    DefaultSetSymbology = xmlDataSelector["DefaultSetSymbology"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'DefaultSetSymbology' in the XML file");
                    LoadedXML = false;
                    return;
                }
                try
                {
                    LayerLocation = xmlDataSelector["LayerLocation"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'LayerLocation' in the XML file");
                    LoadedXML = false;
                    return;
                }
                try
                {
                    EnableSpatialPlotting = xmlDataSelector["EnableSpatialPlotting"].InnerText;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'EnableSpatialPlotting' in the XML file");
                    LoadedXML = false;
                }
                try
                {
                    DefaultClearLogFile = false;
                    strRawText = xmlDataSelector["DefaultClearLogFile"].InnerText;
                    if (strRawText.ToLower() == "yes" || strRawText.ToLower() == "y")
                        DefaultClearLogFile = true;
                }
                catch
                {
                    MessageBox.Show("Could not locate the item 'DefaultClearLogFile' in the XML file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadedXML = false;
                    return;
                }

            }
            else
            {
                FoundXML = false; // this has to be checked first; all other properties are empty.
            }

        }
        private string GetConfigFilePath()
        {
            // Create folder dialog.
            FolderBrowserDialog xmlFolder = new FolderBrowserDialog();

            // Set the folder dialog title.
            xmlFolder.Description = "Select folder containing 'DataSelector.xml' file ...";
            xmlFolder.ShowNewFolderButton = false;

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
        public bool GetFoundXML()
        {
            return FoundXML;
        }

        public bool GetLoadedXML()
        {
            return LoadedXML;
        }

        public string GetSDEName()
        {
            return FileDSN;
        }

        public string GetConnectionString()
        {
            return ConnectionString;
        }

        public int GetTimeoutSeconds()
        {
            return TimeOutSeconds;
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

        public bool GetDefaultClearLogFile()
        {
            return DefaultClearLogFile;
        }


    }
}
