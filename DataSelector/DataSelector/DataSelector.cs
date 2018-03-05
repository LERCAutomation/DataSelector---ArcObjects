using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using ESRI.ArcGIS.ArcMapUI;

namespace DataSelector
{
    public class DataSelector : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public DataSelector()
        {
        }

        protected override void OnClick()
        {
            //
            //  Click simply launches the Data Selector Tool
            //
            frmDataSelector frmMyForm;
            frmMyForm = new frmDataSelector();
            //frmMyForm.Show();
            frmMyForm.ShowDialog();

            ArcMap.Application.CurrentTool = null;
        }
        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
