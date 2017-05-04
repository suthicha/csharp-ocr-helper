using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

namespace OCRHelper.WinApp.Controllers
{
    public class PdfiumViewerHelper
    {
        public byte[] ConvertToPng(string sourceFileName)
        {
            var ms = new MemoryStream();
            using (var document = PdfiumViewer.PdfDocument.Load(sourceFileName))
            {
                var image = document.Render(0, 300, 300, true);
                image.Save(ms, ImageFormat.Png);
            }
            return ms.ToArray();
        }

        public string WritePngFile(string destinationFile, byte[] buffer)
        {
            try
            {
                if (File.Exists(destinationFile))
                    File.Delete(destinationFile);

                File.WriteAllBytes(destinationFile, buffer);

                return destinationFile;
            }
            catch
            {
                return string.Empty;
            }
        }

        public List<DocumentType> ReverseTextToObj(string spoolText)
        {
            string reduceMultiSpace = @"[ ]{2,}";
            var docTypes = new List<DocumentType>();
            var splitSpool = spoolText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            var dt = new DataTable();
            dt.Columns.Add("ReferNo", typeof(String));
            dt.Columns.Add("ReferType", typeof(String));

            for (int i = 3; i < 15; i++)
            {
                var line = splitSpool[i];

                if (line.Length > 20)
                {
                    line = line.Replace("-", "");
                    line = Regex.Replace(line.Replace("\t", " "), reduceMultiSpace, " ");

                    var obj = line.Split(' ');

                    if (obj.Length > 1)
                    {
                        var content = obj[obj.Length - 1];
                        content = content.Replace("O", "0").Replace("Q", "0").Replace(">", "3");
                        content = content.Replace("I", "1");

                        if (content.Length >= 13)
                        {
                            dt.Rows.Add();
                            dt.Rows[dt.Rows.Count - 1][0] = content;
                            dt.Rows[dt.Rows.Count - 1][1] = content.Contains("DE") ? "REFNO" : "DECNO";
                        }
                    }
                }
            }

            foreach (DataRow dr in dt.Rows)
            {
                docTypes.Add(new DocumentType()
                {
                    ReferNo = dr[0].ToString(),
                    ReferType = dr[1].ToString()
                });
            }

            return docTypes;
        }
    }
}