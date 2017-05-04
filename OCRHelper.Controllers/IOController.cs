using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OCRHelper.Controllers
{
    public class IOController
    {
        public List<FileInfo> getPdfs(string sourcePath)
        {
            var dir = new DirectoryInfo(sourcePath);
            return dir.GetFiles("*.pdf").ToList();
        }

        public void Clear(string sourcePath)
        {
            var dir = new DirectoryInfo(sourcePath);
            var fi = dir.GetFiles("*.*");

            for (int i = 0; i < fi.Length; i++)
            {
                fi[i].Delete();
            }
        }

        public string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public bool Copy(string sourceFileName, string destinationFileName)
        {
            try
            {
                File.Copy(sourceFileName, destinationFileName, true);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteFile(string sourceFileName)
        {
            try
            {
                File.Delete(sourceFileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}