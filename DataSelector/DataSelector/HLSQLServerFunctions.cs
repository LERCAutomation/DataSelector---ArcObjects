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
using System.Windows.Forms;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using HLStringFunctions;

using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;


namespace HLESRISQLServerFunctions
{
    public class ArcSDEFunctions
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
            // Define the wildcards as case insensitive
            Wildcard theInclude = new Wildcard(IncludeWildcard, RegexOptions.IgnoreCase);
            Wildcard theExclude = new Wildcard(ExcludeWildcard, RegexOptions.IgnoreCase);

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
            // Tidy up.
            enumDatasetName = null;
            datasetName = null;
            return theStringList;
        }
    
    }

    public class SQLServerFunctions
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
            myCmd.CommandTimeout = 4000; // TimeOut is 4000 seconds.
            return myCmd;
        }

        # region AddSQLParameter
        public bool AddSQLParameter(ref SqlCommand aCommand, string aName, string aValue)
        {
            // Note we are passing the value as a string as this will eventually become an overloaded method which will accept
            // different types of data. For the moment we only need string.
            SqlParameter myParameter = aCommand.Parameters.Add(aName, System.Data.SqlDbType.VarChar);
            myParameter.Value = aValue;
            return true;
        }

        public bool AddSQLParameter(ref SqlCommand aCommand, string aName, int aValue)
        {
            SqlParameter myParameter = aCommand.Parameters.Add(aName, System.Data.SqlDbType.BigInt);
            myParameter.Value = aValue;
            return true;
        }

        public bool AddSQLParameter(ref SqlCommand aCommand, string aName, bool aValue)
        {
            SqlParameter myParameter = aCommand.Parameters.Add(aName, System.Data.SqlDbType.Bit);
            myParameter.Value = 0;
            if (aValue) myParameter.Value = 1;
            return true;
        }
        #endregion

        public int CountRows(ref SqlConnection aConnection, string aTableName)
        {
            string strQuery = "SELECT COUNT(*) FROM " + aTableName;
            SqlCommand objCommand = new SqlCommand(strQuery, aConnection);
            int aCount = (int)objCommand.ExecuteScalar();

            return aCount;
        }

        public bool FieldExists(ref SqlConnection aConnection, string aTableName, string aColumnName)
        {
            string strQuery = "SELECT TOP 1 * FROM " + aTableName;
            SqlCommand objCommand = new SqlCommand(strQuery, aConnection);
            bool blColExists = false;
            SqlDataReader objReader = objCommand.ExecuteReader();
            for (int col = 0; col < objReader.FieldCount; col++)
            {
                if (objReader.GetName(col).ToString() == aColumnName) blColExists = true;
            }
            objCommand.Dispose();
            objReader.Dispose();
            return blColExists;
        }

        public bool TableExists(ref SqlConnection aConnection, string aTableName)
        {
            bool blTableExists = false;
            List<string> tables = new List<string>();
            DataTable dt = aConnection.GetSchema("Tables");
            foreach (DataRow row in dt.Rows)
            {
                string tablename = (string)row[2];
                if (tablename == aTableName)
                    blTableExists = true;
            }
            dt.Dispose();
            return blTableExists;
        }

        public bool TableHasRows(ref SqlConnection aConnection, string aTableName)
        {
            bool blHasRows = false;
            System.Data.SqlClient.SqlCommand CheckNone = new System.Data.SqlClient.SqlCommand(
                "IF EXISTS(SELECT * from " + aTableName + ") SELECT 1 ELSE SELECT 0", aConnection);
            int result = (int)CheckNone.ExecuteScalar();
            if (result == 1)
                blHasRows = true;

            CheckNone.Dispose();
            return blHasRows;
        }

        public string[] GetFieldNames(ref SqlConnection aConnection, string aTableName)
        {
            List<string> strFieldNames = new List<string>();

            string strQuery = "SELECT TOP 1 * FROM " + aTableName;
            SqlCommand objCommand = new SqlCommand(strQuery, aConnection);

            SqlDataReader objReader = objCommand.ExecuteReader();
            for (int col = 0; col < objReader.FieldCount; col++)
            {
                strFieldNames.Add(objReader.GetName(col).ToString());
            }
            objCommand.Dispose();
            objReader.Dispose();
            return strFieldNames.ToArray();
        }
        

    }
}
