using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using System.Collections.Generic;
using System.Net;
using System;
using System.Threading.Tasks;
using Webx.Web.Models;

namespace Webx.Web.Helpers
{
    public static class MyPaypalPaymentHelper
    {
        public static PayPalHttpClient Client(MyPaypalSetup paypalEnvironment)
        {
            PayPalEnvironment environment = null;

            if(paypalEnvironment.Environment == "live")
            {
                environment = new LiveEnvironment(paypalEnvironment.ClientId, paypalEnvironment.Secret);
            }
            else
            {
                environment = new SandboxEnvironment(paypalEnvironment.ClientId, paypalEnvironment.Secret);
            }

            PayPalHttpClient client = new PayPalHttpClient(environment);
            return client;
        }

        public async static Task<HttpResponse> CreateOrder(MyPaypalSetup paypalSetup,ShopViewModel model)
        {
            HttpResponse response = null;

            List<Item> cartItems = new List<Item>();
            decimal total = 0;
            foreach(var item in model.Cart)
            {
                
                decimal unitPrice = Math.Round(item.Product.PriceWithDiscount,2);

                //item.Product.PriceWithDiscount.ToString("D2").Replace(",", ".")
                cartItems.Add(new Item
                {
                     Quantity = item.Quantity.ToString(),
                     Name = item.Product.Name,
                     UnitAmount = new PayPalCheckoutSdk.Orders.Money() { CurrencyCode = "EUR", Value = unitPrice.ToString().Replace(",",".")},                                    
                                          
                });

                total += (unitPrice * (decimal)item.Quantity);
            }

            //total = Math.Round(total, 2);

            try
            {
                // Construct a request object and set desired parameters
                // Here, OrdersCreateRequest() creates a POST request to /v2/checkout/orders
                #region order_creation
                var order = new OrderRequest()
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    PurchaseUnits = new List<PurchaseUnitRequest>()
                        {
                            new PurchaseUnitRequest()
                            {                                

                                Items = cartItems,

                                AmountWithBreakdown = new AmountWithBreakdown()
                                {
                                    CurrencyCode = "EUR",
                                    Value = total.ToString().Replace(",","."),

                                    AmountBreakdown = new AmountBreakdown()
                                    {                                        
                                        ItemTotal = new PayPalCheckoutSdk.Orders.Money()
                                        {
                                            CurrencyCode = "EUR",
                                            Value = total.ToString().Replace(",",".")
                                        }
                                    }
                                }



                            }
                        },
                    ApplicationContext = new ApplicationContext()
                    {
                        ReturnUrl = paypalSetup.RedirectUrl,
                        CancelUrl = paypalSetup.RedirectUrl + "&Cancel=true"
                    }
                };
                             

                #endregion

                //IMPORTANT
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                // Call API with your client and get a response for your call
                var request = new OrdersCreateRequest();
                request.Prefer("return=representation");
                request.RequestBody(order);
                PayPalHttpClient paypalHttpClient = Client(paypalSetup);
                response = await paypalHttpClient.Execute(request);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
                throw;
            }
            return response;
        }


        //### Capturing an Order
        //Before capturing an order, order should be approved by the buyer using the approve link in create order response

        public async static Task<HttpResponse> captureOrder(MyPaypalSetup paypalSetup)
        {
            // Construct a request object and set desired parameters
            // Replace ORDER-ID with the approved order id from create order
            var request = new OrdersCaptureRequest(paypalSetup.PayerApprovedOrderId);
            request.RequestBody(new OrderActionRequest());
            PayPalHttpClient paypalHttpClient = Client(paypalSetup);
            HttpResponse response = await paypalHttpClient.Execute(request);
            return response;
        }

    }
}
