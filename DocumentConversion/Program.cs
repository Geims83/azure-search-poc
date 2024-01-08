using DocumentConversion.Services;

namespace DocumentConversion
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                System.Console.WriteLine("Usage: DocumentConversion <input file>");
                return;
            }

            string inputFile = args[0];
            string outputFile = inputFile + "x";

            FileInfo fi = new FileInfo(inputFile);
            if (!fi.Exists)
            {
                System.Console.WriteLine("File not found: " + inputFile);
                return;
            }

            string extension = fi.Extension.ToLower();
            if (extension == ".doc")
            {
                DocConversionService.ConvertDocToDocx(inputFile, outputFile);
            }
            else if (extension == ".xls")
            {
                DocConversionService.ConvertXlsToXlsx(inputFile, outputFile);
            }
            else
            {
                System.Console.WriteLine("Unsupported file type: " + extension);
                return;
            }

        }
    }
}
