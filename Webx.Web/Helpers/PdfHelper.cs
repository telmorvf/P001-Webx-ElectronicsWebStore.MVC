using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using System.IO;
using System.Threading.Tasks;
using Webx.Web.Extensions;
using Webx.Web.Models;
using Microsoft.AspNetCore.Hosting;

namespace Webx.Web.Helpers
{
    public class PdfHelper : IPdfHelper
    {

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ITemplateHelper _templateHelper;

        public PdfHelper(IWebHostEnvironment hostingEnvironment, ITemplateHelper templateHelper)
        {
            _hostingEnvironment = hostingEnvironment;
            _templateHelper = templateHelper;
        }

        public async Task<string> PrintPDFAsync(InvoiceViewModel model)
        {
            try
            {
                var html = await _templateHelper.RenderAsync("_InvoicePDF", model);
                var Renderer = new IronPdf.HtmlToPdf();
                Renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
                Renderer.RenderingOptions.MarginBottom = 1;
                Renderer.RenderingOptions.MarginTop = 1;
                Renderer.RenderingOptions.MarginLeft = 1;
                Renderer.RenderingOptions.MarginRight = 1;
                var PDF = Renderer.RenderHtmlAsPdf(html);

                //await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                //{
                //    Headless = true,
                //    ExecutablePath = PuppeteerExtensions.ExecutablePath
                //});
                //await using var page = await browser.NewPageAsync();
                //await page.EmulateMediaTypeAsync(MediaType.Screen);
                //await page.SetContentAsync(html);
                //var pdfContent = await page.PdfStreamAsync(new PdfOptions
                //{
                //    Format = PaperFormat.A4,
                //    PrintBackground = true
                //});                     
            

                string invoices = Path.Combine(_hostingEnvironment.WebRootPath, "Invoices");
                string filePath = Path.Combine(invoices, $"Invoice-{model.Id}.pdf");
                var OutputPath = filePath;
                PDF.SaveAs(OutputPath);
                //byte[] pdfbytes = System.IO.File.ReadAllBytes(OutputPath);


                //using (Stream filestream = new FileStream(filePath, FileMode.Create))
                //{
                //    await pdfbytes.CopyToAsync(filestream);
                //}

                return filePath;
            }
            catch (System.Exception ex)
            {
                return string.Empty;
            }

        }



    }
}
