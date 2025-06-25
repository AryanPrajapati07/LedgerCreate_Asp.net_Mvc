
namespace LedgerCreate.Controllers
{
    internal class SynchronizedConverter
    {
        private PdfTools pdfTools;

        public SynchronizedConverter(PdfTools pdfTools)
        {
            this.pdfTools = pdfTools;
        }

        internal byte[] Convert(DinkToPdf.HtmlToPdfDocument doc)
        {
            throw new NotImplementedException();
        }
    }
}