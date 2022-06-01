using OpenHtmlToPdf;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
                var pdfGenerator = OpenHtmlToPdf.Pdf
                        .From(Html_Content)
                        .WithGlobalSetting("orientation", "Portrait")
                        .WithObjectSetting("web.defaultEncoding", "utf-8")
                        //.WithTitle($"{trackId}.pdf")
                        .OfSize(paperSize);


                if (
                    paperSize.Width == "5.5in" && 
                    paperSize.Height == "8.5in")
                {
                    pdfGenerator = pdfGenerator.Landscape();
                }

                pdf = pdfGenerator.Content();


            }
            return pdf;
        }
        public async Task<byte[]> PdfRenderAsync(string Html_Content, string trackId, PaperSize paperSize)
        {
            if (paperSize is null)
            {
                paperSize = PaperSize.A4;
            }

            byte[] pdf = null;

            var poolItem = await CustomPool.Instance.GetAsync();

            try
            {
                var pdfGenerator = OpenHtmlToPdf.Pdf
                         .From(Html_Content)
                         .WithGlobalSetting("orientation", "Portrait")
                         .WithObjectSetting("web.defaultEncoding", "utf-8")
                         .OfSize(paperSize);

                if (paperSize.Width == "5.5in" && paperSize.Height == "8.5in")
                {
                    pdfGenerator = pdfGenerator.Landscape();
                }

                pdf = pdfGenerator.Content();

            }
            finally
            {
                CustomPool.Instance.Return(poolItem);
            }

            return pdf;
        }

    }
    public class CustomPool
    {
        private readonly ConcurrentBag<object> _objects;
        private static readonly object objLock = new object();
        private static CustomPool _instance;

        public static CustomPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (objLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CustomPool();
                        }
                    }
                }
                return _instance;
            }
        }

        public CustomPool()
        {
            _objects = new ConcurrentBag<object>();

            int numInstances;
            if(!int.TryParse(Environment.GetEnvironmentVariable("concurrenciapdf"),out numInstances))
            {
                numInstances = 3;
            }

            for (var x = 0; x < numInstances; x++)
            {
                _objects.Add(new object());
            }
        }

        public async Task<object> GetAsync()
        {
            while (true)
            {
                if (_objects.TryTake(out object item))
                {
                    return item;
                }

                await Task.Delay(200);
            }
        }

        public void Return(object item) => _objects.Add(item);
    }
}
