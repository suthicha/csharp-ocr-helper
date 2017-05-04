using NSOCR_NameSpace;
using NSOCRLib;
using OCRHelper.Controllers;
using OCRHelper.WinApp.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OCRHelper.WinApp
{
    public partial class Form1 : Form
    {
        private BindingList<Logs> items;
        private BindingSource bs;
        private readonly string EXP_PATH;
        private readonly string EXP_INDEX_PATH;
        private readonly string EXP_ERR_PATH;
        private readonly string IMP_PATH;
        private readonly string IMP_INDEX_PATH;
        private readonly string IMP_ERR_PATH;
        private readonly string APP_TEMP_PATH;
        private int CfgObj, OcrObj, ImgObj;
        private NSOCRClass NsOCR = new NSOCRClass();
        private PdfController _pdfController;
        private IOController _ioController;
        private PdfiumViewerHelper _pdfViewerHelper;

        public Form1()
        {
            InitializeComponent();
            items = new BindingList<Logs>();
            bs = new BindingSource();
            bs.DataSource = items;
            dgv.DataSource = bs;

            EXP_PATH = ConfigurationManager.AppSettings["EXP_PATH"];
            EXP_INDEX_PATH = ConfigurationManager.AppSettings["EXP_INDEX_PATH"];
            EXP_ERR_PATH = ConfigurationManager.AppSettings["EXP_ERR_PATH"];
            IMP_PATH = ConfigurationManager.AppSettings["IMP_PATH"];
            IMP_INDEX_PATH = ConfigurationManager.AppSettings["IMP_INDEX_PATH"];
            IMP_ERR_PATH = ConfigurationManager.AppSettings["IMP_ERR_PATH"];
            APP_TEMP_PATH = Path.Combine(Application.StartupPath, "temp");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            NsOCR.Engine_InitializeAdvanced(out CfgObj, out OcrObj, out ImgObj);

            if (!Directory.Exists(APP_TEMP_PATH))
                Directory.CreateDirectory(APP_TEMP_PATH);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            items.Clear();
            ExecuteOCR(EXP_PATH, EXP_INDEX_PATH);

            MessageBox.Show("OCR Export Completed");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            items.Clear();
            ExecuteOCR(IMP_PATH, IMP_INDEX_PATH);

            MessageBox.Show("OCR Import Completed");
        }

        private void ExecuteOCR(string sourcePath, string indexPath)
        {
            _ioController = new IOController();
            _pdfController = new PdfController();
            _pdfViewerHelper = new PdfiumViewerHelper();

            var exportPdfFiles = _ioController.getPdfs(sourcePath);
            _ioController.Clear(APP_TEMP_PATH);

            for (int i = 0; i < exportPdfFiles.Count; i++)
            {
                var info = exportPdfFiles[i];

                items.Add(new Logs { LogText = "Read ==> " + info.FullName });

                var pdfBuffer = _pdfController.ExtractPageToMemory(info.FullName);

                if (pdfBuffer == null || pdfBuffer.Length == 0)
                    continue;

                var pdfTempName = Path.Combine(APP_TEMP_PATH, Guid.NewGuid().ToString() + ".pdf");
                var pdfFullName = _pdfController.WritePdf(pdfTempName, pdfBuffer);

                items.Add(new Logs { LogText = "Create PDF ==> " + pdfFullName });

                var pngBuffer = _pdfViewerHelper.ConvertToPng(pdfFullName);
                if (pngBuffer == null || pngBuffer.Length == 0)
                    continue;

                var pngTempName = Path.Combine(APP_TEMP_PATH, Guid.NewGuid().ToString() + ".png");
                var pngFullName = _pdfViewerHelper.WritePngFile(pngTempName, pngBuffer);

                items.Add(new Logs { LogText = "Create PNG ==> " + pngFullName });

                var ocrText = OCRImageToText(pngFullName);
                items.Add(new Logs { LogText = "Create OCR ==> " + ocrText });

                var reverseTextToObj = _pdfViewerHelper.ReverseTextToObj(ocrText);

                if (reverseTextToObj.Count > 0)
                {
                    var sortDocumentType = reverseTextToObj.OrderBy(d => d.ReferType).ToList();
                    var elementObj = sortDocumentType.ElementAt(0);
                    var pdfOCRName = _ioController.RemoveSpecialCharacters(elementObj.ReferNo);
                    var destinationFileName = Path.Combine(indexPath, pdfOCRName + ".pdf");

                    // CopyToIndexPath(pdfQueueInfo.FullName, indexPath, elementObj.ReferNo);
                    items.Add(new Logs { LogText = "Read NAME ==> " + pdfOCRName });

                    var copyStatus = _ioController.Copy(info.FullName, destinationFileName);

                    items.Add(new Logs { LogText = "Copy To ==> " + destinationFileName });

                    if (copyStatus)
                    {
                        info.Delete();
                    }
                }

                dgv.Refresh();
                dgv.FirstDisplayedScrollingRowIndex = dgv.RowCount - 1;
                dgv.CurrentCell = dgv[0, dgv.RowCount - 1];
            }
        }

        private string OCRImageToText(string sourceFileName)
        {
            string readText = string.Empty;

            try
            {
                NsOCR.Img_LoadFile(ImgObj, sourceFileName);

                SendKeys.Send("{ESC}");

                NsOCR.Img_OCR(ImgObj, TNSOCR.OCRSTEP_FIRST, TNSOCR.OCRSTEP_LAST, 0);
                NsOCR.Img_GetImgText(ImgObj, out readText, TNSOCR.FMT_EXACTCOPY);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OCRImageToText : {0}", ex.Message);
            }
            return readText;
        }
    }

    public class Logs
    {
        public string LogText { get; set; }
    }
}