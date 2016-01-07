using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;


namespace HLSQLServerFunctions
{
    public class SQLServerFunctions
    {
        #region OpenSQLServerConnection
        // For example, connectionFile = @"C:\myData\Connection to Kona.sde".
        public IWorkspace OpenSQLServerConnection(String connectionFile)
        {
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            return workspaceFactory.OpenFromFile(connectionFile, 0);
        }
        #endregion

        

        
    }
}
