using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

using ESRI.ArcGIS.ArcMapUI;

namespace DataSelector
{
    public class LaunchDataSelector : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public LaunchDataSelector()
        {
        }

        protected override void OnClick()
        {
            //
            //  Click simply launches the Data Selector Tool
            //
            frmDataSelector frmMyForm;
            frmMyForm = new frmDataSelector();
            frmMyForm.Show();

            ArcMap.Application.CurrentTool = null;
        }
        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
