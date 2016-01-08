using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HLFileFunctions
{
    class FileFunctions
    {
        #region GetDirectoryName
        public string GetDirectoryName(string aFullPath)
        {
            // Check input.
            if (aFullPath == null) return null;

            // split at the last \
            int LastIndex = aFullPath.LastIndexOf(@"\");
            string aPath = aFullPath.Substring(0, LastIndex);
            return aPath;
        }
        #endregion
    }
}
