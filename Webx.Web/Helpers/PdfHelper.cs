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
                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = PuppeteerExtensions.ExecutablePath
                });
                await using var page = await browser.NewPageAsync();
                await page.EmulateMediaTypeAsync(MediaType.Screen);
                await page.SetContentAsync(html);
                var pdfContent = await page.PdfStreamAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true
                });

                //return File(pdfContent, "application/pdf", $"Invoice-{model.Id}.pdf");
                //var file = File.Create(pdfContent, "application/pdf", $"Invoice-{model.Id}.pdf");

                string invoices = Path.Combine(_hostingEnvironment.WebRootPath, "Invoices");
                string filePath = Path.Combine(invoices, $"Invoice-{model.Id}.pdf");
                using (Stream filestream = new FileStream(filePath, FileMode.Create))
                {
                    await pdfContent.CopyToAsync(filestream);
                }

                return filePath;
            }
            catch (System.Exception ex)
            {
                return string.Empty;
            }

        }



    }
}
