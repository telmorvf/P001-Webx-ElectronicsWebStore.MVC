using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using System.Threading.Tasks;
using Webx.Web.Extensions;
using Webx.Web.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace Webx.Web.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public InvoiceController(
            IOrderRepository orderRepository
            )
        {
            _orderRepository = orderRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Printpdf(int OrderId)
        {
            var order = await _orderRepository.GetCompleteOrderByIdAsync(OrderId);

            var model = new InvoiceViewModel
            {
                Id = order.InvoiceId,
                Order = order,
                orderDetails = await _orderRepository.GetOrderDetailsAsync(order.Id)
            };

            //var html = await _templateHelper.RenderAsync("_InvoicePDF", model);

            string conteudo = $"<b>Nome: </b><i> Telmo </i><br/><br/>" +
                $"<b>Morada: </b><i> Rua de São </i><br/><br/>" +
                $"<b>E-Mail: </b><i> telmo@yopmail </i><br/><br/>";

            // === como a string pode ter mais de 257 bits, passo a string para dentro de um Stream Reader
            StringReader sr = new StringReader(conteudo);//StringReader é um objeto para manipular mais de 256 caracteres

            //=== Quero um documento A4 com as seguintes margens de 10floates à volta ===
            Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 10f);//definir o documento

            //=== Instanciar um objeto que vai reconhecer o conteudo que é HTM ====
            //parser é um tradutor, vê se tá tudo bem feito
            HTMLWorker parser = new HTMLWorker(pdfDoc);//O que reconhece o HTML da string conteudo

            //=== Digo que quero despejar o ficheiro para o ecrã
            //PdfWriter.GetInstance(pdfDoc, Response.OutputStream);

            pdfDoc.Open();
            parser.Parse(sr);
            pdfDoc.Close();
            //Response.End();


            //return File(pdfContent, "application/pdf", $"Invoice-{model.Id}.pdf");
            return View(model); 
        }

    }
}
