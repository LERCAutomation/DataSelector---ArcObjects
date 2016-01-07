using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;


namespace HLArcMapModule
{
    class ArcMapFunctions
    {
        #region Constructor
        private IApplication thisApplication;
        // Class constructor.
        public ArcMapFunctions(IApplication theApplication)
        {
            // Set the application for the class to work with.
            // Note the application can be got at from a command / tool by using
            // IApplication pApp = ArcMap.Application - then pass pApp as an argument.
            this.thisApplication = theApplication;
        }
        #endregion

        #region AddLayerFromFClass
        public bool AddLayerFromFClass(IMap theMap, IFeatureClass theFeatureClass, bool Messages = false)
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
            IMap pMap = theMap;
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

        #endregion
    }
}
