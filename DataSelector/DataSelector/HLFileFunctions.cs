using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HLFileFunctions
{
    class FileFunctions
    {
        public bool DirExists(string aFilePath)
        {
            // Check input first.
            if (aFilePath == null) return false;
            DirectoryInfo myDir = new DirectoryInfo(aFilePath);
            if (myDir.Exists == false) return false;
            return true;
        }

        public string GetDirectoryName(string aFullPath)
        {
            // Check input.
            if (aFullPath == null) return null;

            // split at the last \
            int LastIndex = aFullPath.LastIndexOf(@"\");
            string aPath = aFullPath.Substring(0, LastIndex);
            return aPath;
        }

        public bool FileExists(string aFilePath, string aFileName)
        {
            if (DirExists(aFilePath))
            {
                string strFileName = aFilePath;
                string aTest = aFilePath.Substring(aFilePath.Length - 1, 1);
                if (aTest != @"\")
                {
                    strFileName = strFileName + @"\" + aFileName;
                }
                else
                {
                    strFileName = strFileName + aFileName;
                }

                System.IO.FileInfo myFileInfo = new FileInfo(strFileName);

                if (myFileInfo.Exists) return true;
                else return false;
            }
            return false;
        }
        public bool FileExists(string aFullPath)
        {
            System.IO.FileInfo myFileInfo = new FileInfo(aFullPath);
            if (myFileInfo.Exists) return true;
            return false;
        }

        public string GetFileName(string aFullPath)
        {
            // Check input.
            if (aFullPath == null) return null;

            // split at the last \
            int LastIndex = aFullPath.LastIndexOf(@"\");
            string aFile = aFullPath.Substring(LastIndex + 1, aFullPath.Length - (LastIndex + 1));
            return aFile;
        }

        public string ReturnWithoutExtension(string aFileName)
        {
            // check input
            if (aFileName == null) return null;
            int aLen = aFileName.Length;
            // check if it has an extension at all
            string aTest = aFileName.Substring(aLen - 4, 1);
            if (aTest != ".") return aFileName;

            return aFileName.Substring(0, aLen - 4);
        }

    }
}
