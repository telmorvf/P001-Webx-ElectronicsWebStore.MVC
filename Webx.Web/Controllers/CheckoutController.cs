
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using Webx.Web.Models;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using System.Net;
using System;
using System.Web;
using AspNetCoreHero.ToastNotification.Abstractions;
using Syncfusion.EJ2.ImageEditor;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using Webx.Web.Extensions;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using HttpResponse = PayPalHttp.HttpResponse;
using Order = Webx.Web.Data.Entities.Order;

namespace Webx.Web.Controllers
{
  
    public class CheckoutController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly INotyfService _toastNotification;
        private readonly IOrderRepository _orderRepository;
        private readonly IPdfHelper _pdfHelper;
        private readonly IMailHelper _mailHelper;
        private readonly ITemplateHelper _templateHelper;

        private string _paypalEnvironment = "sandbox";//live
        private string _clientId = "AQwGKp_-N9JykoPO628Q-eEhyTOiWANtO-tSKu56sAcq-gM_0gHJ6ciqY3g0e58HyMgC-f3MvdUJjuYN";
        private string _secret = "EOTNL6P23EVLwwWsQvIKgEFtNH5qk0zheY26j6hjlEVGXZAcwFZGHPyCKkdsZuZdeRsBK1dwhN07reRU";

        public CheckoutController(IUserHelper userHelper,
            IProductRepository productRepository,
            IStockRepository stockRepository,
            IStoreRepository storeRepository,
            INotyfService toastNotification,
            IOrderRepository orderRepository,
            IPdfHelper pdfHelper,
            IMailHelper mailHelper,
            ITemplateHelper templateHelper 
            )
        {
            _userHelper = userHelper;
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _storeRepository = storeRepository;
            _toastNotification = toastNotification;
            _orderRepository = orderRepository;
            _pdfHelper = pdfHelper;
            _mailHelper = mailHelper;
            _templateHelper = templateHelper;
        }

        
        public async Task<IActionResult> Index(List<string> results = null)
        {
  
           
            var model = await _productRepository.GetInitialShopViewModelAsync();

            foreach(var item in model.Cart)
            {
                //verifica se tem stock de todos os produtos que cliente deseja adquirir nas lojas selecionadas por cliente.
                if (!item.Product.IsService)
                {
                    var stock = await _stockRepository.GetProductStockInStoreAsync(item.Product.Id, item.StoreId);
                    if (stock.Quantity < item.Quantity)
                    {
                        _toastNotification.Warning("There are products with inssuficient stock in the selected store. Please try to either order from another store or come back later to check stock out.");
                        return RedirectToAction("Index", "Cart");
                    }
                }                
            }

            if (this.User.Identity.IsAuthenticated)
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                ViewBag.UserFullName = user.FullName;
                ViewBag.IsActive = user.Active;

                var checkoutModel = new CheckoutViewModel();
                checkoutModel.User = user;
                checkoutModel.ShippingAddress = user.Address;

                model.CheckoutViewModel = checkoutModel;

                if(results != null && results.Count > 0)
                {
                    foreach(string item in results)
                    {
                        _toastNotification.Warning(item);
                    }
                }

                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Account", new {returnUrl = "/Checkout/Index/" } );
            }
        }

        public async Task<IActionResult> SaveFormData(ShopViewModel model)
        {
            var checkoutTemps = new CheckoutTempData
            {
                Address = model.CheckoutViewModel.ShippingAddress,
                Email = model.CheckoutViewModel.User.Email,
                FirstName = model.CheckoutViewModel.User.FirstName,
                LastName = model.CheckoutViewModel.User.LastName,
                NIF = model.CheckoutViewModel.User.NIF,
                PhoneNumber = model.CheckoutViewModel.User.PhoneNumber
            };

            try
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                user.CheckoutTempData = checkoutTemps;
                await _userHelper.UpdateUserAsync(user);
                return RedirectToAction("Paypalvtwo");
            }
            catch (Exception ex)
            {
                _toastNotification.Error("There was a problem saving the shipping data. Please try again."+ex.Message);
                return RedirectToAction("Index");
            }            
        }


        public async Task<ActionResult> Paypalvtwo(string Cancel = null)
        {
         
            var model = await _productRepository.GetInitialShopViewModelAsync();

            //var url = Url.Action("Paypalvtwo", "Checkout", new { shopViewModel = shopViewModel});

            //setup paypal environment to save some essential varaibles
            MyPaypalSetup payPalSetup = new MyPaypalSetup { Environment = _paypalEnvironment, ClientId = _clientId, Secret = _secret };

            //a list if string to collect messages to be displayed to the payer
            List<string> paymentResultList = new List<string>();          

         

            //check if payer has cancelled the transaction, if yes, do nothing. Let the payer know about his actions
            if (!string.IsNullOrEmpty(Cancel) && Cancel.Trim().ToLower() == "true")
            {
                _toastNotification.Warning("Transaction has been canceled.");
                return RedirectToAction("Index");
            }

            
            
            var queryString = HttpContext.Request.QueryString;
            string token = HttpUtility.ParseQueryString(queryString.ToString()).Get("token");
            string payerId = HttpUtility.ParseQueryString(queryString.ToString()).Get("PayerID");

            payPalSetup.PayerApprovedOrderId = token;
            string PayerID = payerId;

            //when payerID is null it means order is not approved by the payer.            
            if (string.IsNullOrEmpty(PayerID))
            {
              
                //Create order and display it to the payer to approve. 
                //This is the first PayPal screen where payer signin using his PayPal credentials
                try
                {
                    //redirect URL. when approved or cancelled on PayPal, PayPal uses this URL to redirect to your app/website.
                    payPalSetup.RedirectUrl = HttpContext.Request.Scheme +"://"+ HttpContext.Request.Headers["Host"] + "/" + Request.RouteValues["Controller"] + "/Paypalvtwo?";
                    HttpResponse response = await MyPaypalPaymentHelper.CreateOrder(payPalSetup,model);

                    var statusCode = response.StatusCode;
                    PayPalCheckoutSdk.Orders.Order result = response.Result<PayPalCheckoutSdk.Orders.Order>();
                    //Console.WriteLine("Status: {0}", result.Status);
                    //Console.WriteLine("Order Id: {0}", result.Id);
                    //Console.WriteLine("Intent: {0}", result.CheckoutPaymentIntent);
                    //Console.WriteLine("Links:");
                    foreach (PayPalCheckoutSdk.Orders.LinkDescription link in result.Links)
                    {
                        //Console.WriteLine("\t{0}: {1}\tCall Type: {2}", link.Rel, link.Href, link.Method);
                        if (link.Rel.Trim().ToLower() == "approve")
                        {
                            payPalSetup.ApproveUrl = link.Href;
                        }
                    }

                    if (!string.IsNullOrEmpty(payPalSetup.ApproveUrl))
                        return Redirect(payPalSetup.ApproveUrl);
                }
                catch (Exception ex)
                {
                    _toastNotification.Error("There was an error in processing your payment. Details:" + ex.Message);                   
                }

            }
            else
            {
              
                //this is where actual transaction is carried out
                HttpResponse response = await MyPaypalPaymentHelper.captureOrder(payPalSetup);
                try
                {
                    var statusCode = response.StatusCode;
                    PayPalCheckoutSdk.Orders.Order result = response.Result<PayPalCheckoutSdk.Orders.Order>();
                    //Console.WriteLine("Status: {0}", result.Status);
                    //Console.WriteLine("Capture Id: {0}", result.Id);

                    //update view bag so user/payer gets to know the status
                    if (result.Status.Trim().ToUpper() == "COMPLETED")
                    {
                        string paymentID = result.Id.ToString();                        

                        return RedirectToAction("Success",new {paymentId = paymentID});
                    }
                    

                  
                    //if (result.PurchaseUnits != null && result.PurchaseUnits.Count > 0 &&
                    //    result.PurchaseUnits[0].Payments != null && result.PurchaseUnits[0].Payments.Captures != null &&
                    //    result.PurchaseUnits[0].Payments.Captures.Count > 0)
                    //    #endregion
                    //    paymentResultList.Add("Transaction ID: " + result.PurchaseUnits[0].Payments.Captures[0].Id);
                }
                catch (Exception ex)
                {                                        
                    paymentResultList.Add("There was an error in processing your payment");
                    paymentResultList.Add("Details: " + ex.Message);
                    return RedirectToAction("Index", new { results = paymentResultList });
                }
            }

            return RedirectToAction("Index", new { paymentResultList });
        }

       

        public async Task<IActionResult> Success(string paymentId)
        {

            var model = await _productRepository.GetInitialShopViewModelAsync();
            var user = await _userHelper.GetUserByEmailWithCheckoutTempsAsync(User.Identity.Name);
            ViewBag.UserFullName = user.FullName;
            ViewBag.IsActive = user.Active;

            if (!string.IsNullOrEmpty(paymentId) && model.Cart.Count > 0)
            {
                ViewBag.PaymentId = paymentId;
                var checkoutModel = new CheckoutViewModel();
                checkoutModel.User = user;
                checkoutModel.ShippingAddress = user.Address;
                model.CheckoutViewModel = checkoutModel;
                model.Invoices = new List<InvoiceViewModel>();

                //Verificar se encomendas são procedidas a lojas distintas
                List<int> storesIdsInOrder = new List<int>();

                foreach (var item in model.Cart)
                {
                    bool storeAlreadyInList = false;

                    foreach (int id in storesIdsInOrder)
                    {
                        if (id == item.StoreId)
                        {
                            storeAlreadyInList = true;
                            break;
                        }
                    }

                    if (!storeAlreadyInList)
                    {
                        storesIdsInOrder.Add(item.StoreId);
                    }
                }

                //Cria encomendas a lojas distintas (caso existam) e determina o estado da encomenda, caso tenha um serviço atribuido, como pending appointment, caso não o tenha
                //como order created(predefinição)
                //List<Data.Entities.Order> Orders = new List<Data.Entities.Order>();
                var date = DateTime.UtcNow;

                foreach (int storeId in storesIdsInOrder)
                {
                    decimal orderTotal = 0;
                    int orderTotalQuantity = 0;
                    OrderStatus status = await _orderRepository.GetOrderStatusByNameAsync("Order Created");

                    foreach (var item in model.Cart)
                    {
                        if (item.StoreId == storeId)
                        {
                            if (item.Product.IsService)
                            {
                                status = await _orderRepository.GetOrderStatusByNameAsync("Pending Appointment");
                            }
                            else
                            {
                                var stock = await _stockRepository.GetProductStockInStoreAsync(item.Product.Id,item.StoreId);
                                stock.Quantity -= item.Quantity;
                                await _stockRepository.UpdateAsync(stock);
                            }

                            orderTotal += (item.Product.Price * item.Quantity);
                            orderTotalQuantity += item.Quantity;
                        }
                    }

                    OrderViewModel orderVM = new OrderViewModel
                    {
                        Customer = user,
                        OrderDate = date,
                        DeliveryDate = date.AddDays(3),
                        Appointment = null,
                        TotalPrice = orderTotal,
                        TotalQuantity = orderTotalQuantity,
                        Status = status,
                    };              

                    //Grava encomenda na B.D
                    try
                    {
                        await _orderRepository.AddOrderAsync(orderVM, storeId);
                    }
                    catch (Exception ex)
                    {
                        _toastNotification.Error("There was a problem creating the order, please contact WebX with your payment details at hand. Sorry for the incovenience." + ex.Message, 20);
                        return RedirectToAction("Index");
                    }



                }

                // Associar detalhes de encomenda a cada uma das encomendas criadas

                var orders = await _orderRepository.GetCustomerRecentOrdersAsync(user, date);

                if (orders == null)
                {
                    _toastNotification.Error("There was a problem creating the order, please contact WebX with your payment details at hand. Sorry for the incovenience.", 20);
                    return RedirectToAction("Index");
                }

                foreach (var order in orders)
                {
                    List<OrderDetail> orderDetails = new List<OrderDetail>();
                    var orderId = order.Id;
                    order.InvoiceId = orderId * 1000;


                    foreach (var item in model.Cart)
                    {
                        if (item.StoreId == order.Store.Id)
                        {
                            orderDetails.Add(new OrderDetail
                            {
                                Order = order,
                                Price = (item.Product.Price * item.Quantity),
                                Product = item.Product,
                                Quantity = item.Quantity
                            });
                        }
                    }

                    try
                    {
                        await _orderRepository.CreateOrderDetailsAsync(orderDetails);
                        await _orderRepository.UpdateAsync(order);
                    }
                    catch (Exception ex)
                    {
                        _toastNotification.Error("There was a problem creating the order, please contact WebX with your payment details at hand. Sorry for the incovenience." + ex.Message, 20);
                        return RedirectToAction("Index");
                    }
                }


                //apagar cookie do carrinho

                var response = _productRepository.ClearCart();

                if (!response.IsSuccess)
                {
                    _toastNotification.Warning($"Your Order was created with success but there was a problem clearing your cart! {response.Message}", 10);
                }

                //Cria pdf's de cada uma das orders lançadas e retorna a string com o caminho e nome do ficheiro criado para conseguir após envio por email, eliminar ficheiro.

                List<string> filepaths = new List<string>();

                foreach (var order in orders)
                {
                    var invoiceModel = new InvoiceViewModel
                    {
                        Id = order.InvoiceId,
                        Order = order,
                        orderDetails = await _orderRepository.GetOrderDetailsAsync(order.Id)
                    };

                    model.Invoices.Add(invoiceModel);

                    var path = await _pdfHelper.PrintPDFAsync(invoiceModel);

                    if (!string.IsNullOrEmpty(path))
                    {
                        filepaths.Add(path);
                    }
                    else
                    {
                        _toastNotification.Warning("There was a problem creating the invoice file. Please contact Webx for more information.", 10);
                    }
                }

                //Enviar email com cada um dos pdf's criados para o cliente

                var result = await _mailHelper.SendEmailWithInvoicesAsync(user.CheckoutTempData.Email, filepaths, user, paymentId);

                if (result.IsSuccess)
                {
                    _toastNotification.Success($"An email with the invoice/s was sent to {user.CheckoutTempData.Email}", 10);
                }
                else
                {
                    _toastNotification.Warning($"there was a problem sending the invoice/s to {user.CheckoutTempData.Email}", 10);
                }

                foreach (var file in filepaths)
                {
                    System.IO.File.Delete(file);
                }

                return View(model);
            }

            return RedirectToAction("Index", "Products");
            
        }



        public async Task<IActionResult>Printpdf(int OrderId)
        {
            
            var order = await _orderRepository.GetCompleteOrderByIdAsync(OrderId);

            
            var model = new InvoiceViewModel
            {
                Id = order.InvoiceId,
                Order = order,
                orderDetails = await _orderRepository.GetOrderDetailsAsync(order.Id)
            };


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

            return File(pdfContent, "application/pdf", $"Invoice-{model.Id}.pdf");         
            
        }


    }
}
