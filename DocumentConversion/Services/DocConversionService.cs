using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace DocumentConversion.Services
{
    public static class DocConversionService
    {

        //method to convert a doc file to a docx file
        public static void ConvertDocToDocx(string oldPath, string newPath)
        {
            //create a word application object
            Microsoft.Office.Interop.Word.Application word = new Word.Application();
            //create a missing variable for missing value
            object miss = System.Reflection.Missing.Value;
            //create a variable to hold the format of the document
            object format = Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatXMLDocument;
            //open the document that we want to convert
            Microsoft.Office.Interop.Word.Document doc = word.Documents.Open(oldPath);
            //convert the document to docx
            doc.SaveAs2(newPath, format);
            //close the document
            doc.Close();
            //quit the application
            word.Quit();
        }


        //method to convert a xls stream to a xlsx stream
        public static void ConvertXlsToXlsx(string oldPath, string newPath)
        {
            //create an excel application object
            Excel.Application excel = new Excel.Application();
            //create a missing variable for missing value
            object miss = System.Reflection.Missing.Value;
            //open the document that we want to convert
            Microsoft.Office.Interop.Excel.Workbook wb = excel.Workbooks.Open(oldPath);
            //convert the document to xlsx
            wb.SaveAs(newPath, Microsoft.Office.Interop.Excel.XlFileFormat.xlOpenXMLWorkbook);
            //close the document
            wb.Close();
            //quit the application
            excel.Quit();
        }
    }
}
