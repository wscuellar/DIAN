using OpenHtmlToPdf;

namespace Gosocket.Dian.Functions.Utils
{
    public class PdfCreator
    {
        protected static PdfCreator instance = null;
        protected static readonly object padlock = new object();
        //private DinkToPdf.Contracts.IConverter _converter;

        public PdfCreator()
        {
            //_converter = new SynchronizedConverter(new PdfTools());
        }


        public static PdfCreator Instance
        {
            get
            {
                // implementacion de singleton thread-safe usando double-check locking 
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new PdfCreator();
                        }
                    }
                }
                return instance;
            }
        }

        public byte[] PdfRender(string Html_Content, string trackId, PaperSize paperSize)
        {
            if (paperSize is null)
            {
                paperSize = PaperSize.A4;
            }

            byte[] pdf = null;
            lock (instance)
            {
                // Convert
                pdf = OpenHtmlToPdf.Pdf
                        .From(Html_Content)
                        .WithGlobalSetting("orientation", "Portrait")
                        .WithObjectSetting("web.defaultEncoding", "utf-8")
                        //.WithTitle($"{trackId}.pdf")
                        .OfSize(paperSize)
                        .Content();
            }
            return pdf;
        }
    }
}
