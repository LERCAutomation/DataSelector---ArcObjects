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
