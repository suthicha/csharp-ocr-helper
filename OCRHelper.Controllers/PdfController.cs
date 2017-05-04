using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;

namespace OCRHelper.Controllers
{
    public class PdfController
    {
        public byte[] ExtractPageToMemory(string sourceFileName)
        {
            byte[] buffer = null;
            PdfReader reader = null;
            Document document = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage = null;

            try
            {
                FileInfo fi = new FileInfo(sourceFileName);
                reader = new PdfReader(sourceFileName);

                document = new Document(reader.GetPageSizeWithRotation(1));

                MemoryStream ms = new MemoryStream();
                pdfCopyProvider = new PdfCopy(document, ms);

                document.Open();
                importedPage = pdfCopyProvider.GetImportedPage(reader, 1);
                pdfCopyProvider.AddPage(importedPage);

                document.Close();
                reader.Close();
                buffer = ms.ToArray();
                ms.Flush();
                ms.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return buffer;
        }

        public string WritePdf(string destinationFileName, byte[] buffer)
        {
            try
            {
                if (File.Exists(destinationFileName))
                    File.Delete(destinationFileName);

                File.WriteAllBytes(destinationFileName, buffer);

                return destinationFileName;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}