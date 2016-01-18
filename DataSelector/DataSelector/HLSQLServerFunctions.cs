using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using HLStringFunctions;

using System.Data;
using System.Data.SqlClient;


namespace HLESRISQLServerFunctions
{
    public class ESRISQLServerFunctions
    {
        // For example, connectionFile = @"C:\myData\Connection to Kona.sde".
        public IWorkspace OpenArcSDEConnection(String connectionFile)
        {
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            return workspaceFactory.OpenFromFile(connectionFile, 0);
        }

        public List<string> GetTableNames(IWorkspace aWorkspace, string IncludeWildcard, string ExcludeWildcard, bool IncludeFullName = false)
        {
            // Define the wildcards 
            Wildcard theInclude = new Wildcard(IncludeWildcard);
            Wildcard theExclude = new Wildcard(ExcludeWildcard);

            List<string> theStringList = new List<string>();
            IEnumDatasetName enumDatasetName = aWorkspace.get_DatasetNames(esriDatasetType.esriDTAny);
            IDatasetName datasetName = enumDatasetName.Next();
            while (datasetName != null) 
            {
                string strName = datasetName.Name;
                // Does the name conform to the IncludeWildcard?
                if (theInclude.IsMatch(strName))
                {
                    if (!theExclude.IsMatch(strName))
                    {
                        if (IncludeFullName)
                        {
                            theStringList.Add(strName);
                        }
                        else
                        {
                            strName = strName.Split('.')[2];
                            theStringList.Add(strName);
                        }
                    }
                }
                datasetName = enumDatasetName.Next();
            }
            return theStringList;
        }
    
    }

    public class ADOSQLServerFunctions
    {
        public SqlConnection CreateSQLConnection(string connectionString)
        {
            SqlConnection con = new SqlConnection(connectionString);
            return con;
        }

        public SqlCommand CreateSQLCommand(ref SqlConnection aConnection, string aName, CommandType aCommandType)
        {
            SqlCommand myCmd = new SqlCommand(aName, aConnection);
            myCmd.CommandType = aCommandType;
            myCmd.CommandTimeout = 1000; // TimeOut is 1000 seconds.
            return myCmd;
        }

        public bool AddSQLParameter(ref SqlCommand aCommand, string aName, string aValue)
        {
            // Note we are passing the value as a string as this will eventually become an overloaded method which will accept
            // different types of data. For the moment we only need string.
            SqlParameter myParameter = aCommand.Parameters.Add(aName, System.Data.SqlDbType.VarChar);
            myParameter.Value = aValue;
            return true;
        }

    }
}
