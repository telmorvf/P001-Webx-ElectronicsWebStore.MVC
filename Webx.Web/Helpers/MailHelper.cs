using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using MimeKit;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Webx.Web.Data.Entities;
using MailKit.Net.Smtp;
using MimeKit.Utils;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using PayPalCheckoutSdk.Orders;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Webx.Web.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IUserHelper _userHelper;

        public MailHelper(IConfiguration configuration,IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContext, IUserHelper userHelper)
        {
           _configuration = configuration;
           _webHostEnvironment = webHostEnvironment;
           _httpContext = httpContext;
            _userHelper = userHelper;
        }


        public async Task<Response> SendConfirmationEmail(string to, string tokenLink,User customer, string returnLink)
        {
            var nameFrom = _configuration["OnlineStoreMail:NameFrom"];
            var from = _configuration["OnlineStoreMail:From"];
            var smtp = _configuration["OnlineStoreMail:Smtp"];
            var port = _configuration["OnlineStoreMail:Port"];
            var password = _configuration["OnlineStoreMail:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = "WebX Confirmation Email";
            
            var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() +
                "assets" + Path.DirectorySeparatorChar.ToString() +
                "templates" + Path.DirectorySeparatorChar.ToString() +
                "emailTemplate.html";            

            string htmlBody = "";

            using(StreamReader streamreader = File.OpenText(pathToTemplate))
            {
                htmlBody = streamreader.ReadToEnd();
            }
            var bodybuilder = new BodyBuilder();
            /*
               {0} - link para homepage da webX
               {1} - Customer Full Name
               {2} - Token Link*/
            //{3} - images/favicon.png           
            var titleImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}favicon.png");
            titleImage.ContentId = MimeUtils.GenerateMessageId();
            //{4} - images/logo.png
            var logoImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}logo.png");
            logoImage.ContentId = MimeUtils.GenerateMessageId();
            //{5} - images/welcome.jpg
            var welcomeImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}welcome.jpg");
            welcomeImage.ContentId = MimeUtils.GenerateMessageId();
            //{6} - images/fb.png
            var fbImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}fb.png");
            fbImage.ContentId = MimeUtils.GenerateMessageId();
            //{7} - images/twitter.png
            var twitterImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}twitter.png");
            twitterImage.ContentId = MimeUtils.GenerateMessageId();
            //{8} - images/insta.png
            var instaImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}insta.png");
            instaImage.ContentId = MimeUtils.GenerateMessageId();
            // -----------------------------------------------{0}--------------{1}-------------{2}------------{3}-------------------{4}----------------{5}-----------------{6}-------------------{7}-------------------{8}---------
            string messageBody = string.Format(htmlBody,returnLink,customer.FullName, tokenLink,titleImage.ContentId,logoImage.ContentId,welcomeImage.ContentId,fbImage.ContentId,twitterImage.ContentId,instaImage.ContentId);

            bodybuilder.HtmlBody = messageBody;            

            message.Body = bodybuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(smtp, int.Parse(port), false);
                    client.Authenticate(from, password);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
            }
            catch (System.Exception ex)
            {

                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

            return new Response
            {
                IsSuccess = true
            };

        }

        public async Task<Response> SendEmailWithInvoicesAsync(string to, List<string> attachments,User user,string paymentId)
        {
            var nameFrom = _configuration["OnlineStoreMail:NameFrom"];
            var from = _configuration["OnlineStoreMail:From"];
            var smtp = _configuration["OnlineStoreMail:Smtp"];
            var port = _configuration["OnlineStoreMail:Port"];
            var password = _configuration["OnlineStoreMail:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = "WebX Order Invoice";

            var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() +
                "assets" + Path.DirectorySeparatorChar.ToString() +
                "templates" + Path.DirectorySeparatorChar.ToString() +
                "orderTemplate.html";

            string htmlBody = "";

            using (StreamReader streamreader = File.OpenText(pathToTemplate))
            {
                htmlBody = streamreader.ReadToEnd();
            }

            var bodybuilder = new BodyBuilder();

                           
            //{0} - images/favicon.png           
            var titleImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}favicon.png");
            titleImage.ContentId = MimeUtils.GenerateMessageId();

            //{1} - images/Order.jpg
            var orderImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}order-image-2.jpg");
            orderImage.ContentId = MimeUtils.GenerateMessageId();
            // {2} - customer name 
            string userFullname = user.CheckoutTempData.FirstName + " " + user.CheckoutTempData.LastName;
            // {3} - transaction id
            // {4} - shipping address

            //{5} - images/fb.png
            var fbImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}fb.png");
            fbImage.ContentId = MimeUtils.GenerateMessageId();
            
            //{6} - images/twitter.png
            var twitterImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}twitter.png");
            twitterImage.ContentId = MimeUtils.GenerateMessageId();
           
            //{7} - images/insta.png
            var instaImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}insta.png");
            instaImage.ContentId = MimeUtils.GenerateMessageId();
            
            
            // -----------------------------------------------{0}------------------------{1}-------------{2}------------{3}-------------------{4}--------------------{5}--------------------{6}-------------------{7}-----------
            string messageBody = string.Format(htmlBody, titleImage.ContentId, orderImage.ContentId, userFullname , paymentId, user.CheckoutTempData.Address, fbImage.ContentId, twitterImage.ContentId, instaImage.ContentId);

            bodybuilder.HtmlBody = messageBody;
            
            foreach(string path in attachments)
            {
                bodybuilder.Attachments.Add(path);
            }

            message.Body = bodybuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(smtp, int.Parse(port), false);
                    client.Authenticate(from, password);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
            }
            catch (System.Exception ex)
            {

                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

            return new Response
            {
                IsSuccess = true
            };
        }

        public async Task<Response> SendEmployeeConfirmationEmail(string to, string tokenLink, User user, string returnLink)
        {
            var nameFrom = _configuration["OnlineStoreMail:NameFrom"];
            var from = _configuration["OnlineStoreMail:From"];
            var smtp = _configuration["OnlineStoreMail:Smtp"];
            var port = _configuration["OnlineStoreMail:Port"];
            var password = _configuration["OnlineStoreMail:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = "WebX Confirmation Email";

            var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() +
                "assets" + Path.DirectorySeparatorChar.ToString() +
                "templates" + Path.DirectorySeparatorChar.ToString() +
                "EmployeeConfirmationEmailTemplate.html";

            string htmlBody = "";

            using (StreamReader streamreader = File.OpenText(pathToTemplate))
            {
                htmlBody = streamreader.ReadToEnd();
            }
            var bodybuilder = new BodyBuilder();
            /*
               {0} - link para homepage da webX
               {1} - Customer Full Name
               {2} - Token Link*/
            //{3} - images/favicon.png           
            var titleImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}favicon.png");
            titleImage.ContentId = MimeUtils.GenerateMessageId();
            //{4} - images/logo.png
            var logoImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}logo.png");
            logoImage.ContentId = MimeUtils.GenerateMessageId();
            //{5} - images/welcome.jpg
            var welcomeImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}welcome2.jpg");
            welcomeImage.ContentId = MimeUtils.GenerateMessageId();
            //{6} - images/fb.png
            var fbImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}fb.png");
            fbImage.ContentId = MimeUtils.GenerateMessageId();
            //{7} - images/twitter.png
            var twitterImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}twitter.png");
            twitterImage.ContentId = MimeUtils.GenerateMessageId();
            //{8} - images/insta.png
            var instaImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}insta.png");
            instaImage.ContentId = MimeUtils.GenerateMessageId();
            // -----------------------------------------------{0}--------------{1}-------------{2}------------{3}-------------------{4}----------------{5}-----------------{6}-------------------{7}-------------------{8}---------
            string messageBody = string.Format(htmlBody, returnLink, user.FullName, tokenLink, titleImage.ContentId, logoImage.ContentId, welcomeImage.ContentId, fbImage.ContentId, twitterImage.ContentId, instaImage.ContentId);

            bodybuilder.HtmlBody = messageBody;

            message.Body = bodybuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(smtp, int.Parse(port), false);
                    client.Authenticate(from, password);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
            }
            catch (System.Exception ex)
            {

                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

            return new Response
            {
                IsSuccess = true
            };
        }

        public async Task<Response> SendResetPasswordEmail(string to, string link, User customer, string returnLink)
        {
            var nameFrom = _configuration["OnlineStoreMail:NameFrom"];
            var from = _configuration["OnlineStoreMail:From"];
            var smtp = _configuration["OnlineStoreMail:Smtp"];
            var port = _configuration["OnlineStoreMail:Port"];
            var password = _configuration["OnlineStoreMail:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = "Reset Password WebX";

            var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() +
                "assets" + Path.DirectorySeparatorChar.ToString() +
                "templates" + Path.DirectorySeparatorChar.ToString() +
                "reset-password.html";           

            string htmlBody = "";

            using (StreamReader streamreader = File.OpenText(pathToTemplate))
            {
                htmlBody = streamreader.ReadToEnd();
            }
            var bodybuilder = new BodyBuilder();


            //  {0} - <!--images/favicon.png-->
            var titleImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}favicon.png");
            titleImage.ContentId = MimeUtils.GenerateMessageId();

            //  {1} - <!--//main page link-->

            //  {2} - <!--//images/logo.png-->
            var logoImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}logo.png");
            logoImage.ContentId = MimeUtils.GenerateMessageId();

            //  {3} - <!--//images/reset.jpg-->
            var resetImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}reset.jpg");
            resetImage.ContentId = MimeUtils.GenerateMessageId();

            //  {4} - <!--//link para reset de password-->

            //  {5} - <!--//Customer Email-->

            //  {6} - <!--images/fb.png-->
            var fbImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}fb.png");
            fbImage.ContentId = MimeUtils.GenerateMessageId();

            //  {7} - <!--images/twitter.png-->
            var twitterImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}twitter.png");
            twitterImage.ContentId = MimeUtils.GenerateMessageId();

            //  {8} - <!--images/insta.png-->
            var instaImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}insta.png");
            instaImage.ContentId = MimeUtils.GenerateMessageId();

            // ---------------------------------------------------{0}----------------{1}---------------{2}------------------{3}----------{4}----------{5}------------{6}-------------------{7}-------------------{8}----------
            string messageBody = string.Format(htmlBody, titleImage.ContentId, returnLink, logoImage.ContentId, resetImage.ContentId,link, customer.Email, fbImage.ContentId, twitterImage.ContentId, instaImage.ContentId);

            bodybuilder.HtmlBody = messageBody;        

            message.Body = bodybuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(smtp, int.Parse(port), false);
                    client.Authenticate(from, password);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
            }
            catch (System.Exception ex)
            {

                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

            return new Response
            {
                IsSuccess = true
            };

        }

        public async Task<Response> SendAnnouncement(string to, string title, string body, string attachment)
        {
            var nameFrom = _configuration["OnlineStoreMail:NameFrom"];
            var from = _configuration["OnlineStoreMail:From"];
            var smtp = _configuration["OnlineStoreMail:Smtp"];
            var port = _configuration["OnlineStoreMail:Port"];
            var password = _configuration["OnlineStoreMail:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = "WebX Announcement";

            var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() +
                "assets" + Path.DirectorySeparatorChar.ToString() +
                "templates" + Path.DirectorySeparatorChar.ToString() +
                "announcementTemplate.html";

            string htmlBody = "";

            using (StreamReader streamreader = File.OpenText(pathToTemplate))
            {
                htmlBody = streamreader.ReadToEnd();
            }

            var bodybuilder = new BodyBuilder();


            //{0} - images/favicon.png           
            var titleImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}favicon.png");
            titleImage.ContentId = MimeUtils.GenerateMessageId();

            //{1} - images/announcement.png
            var announcementImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}announcement.png");
            announcementImage.ContentId = MimeUtils.GenerateMessageId();
            // {2} - body

            //{3} - images/fb.png
            var fbImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}fb.png");
            fbImage.ContentId = MimeUtils.GenerateMessageId();

            //{4} - images/twitter.png
            var twitterImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}twitter.png");
            twitterImage.ContentId = MimeUtils.GenerateMessageId();

            //{5} - images/insta.png
            var instaImage = bodybuilder.LinkedResources.Add($"{_webHostEnvironment.WebRootPath}{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}insta.png");
            instaImage.ContentId = MimeUtils.GenerateMessageId();


            // -----------------------------------------------{0}------------------------{1}----------------{2}----------------------{3}-------------------{4}---------------{5}---{6}--
            string messageBody = string.Format(htmlBody, titleImage.ContentId, announcementImage.ContentId, fbImage.ContentId, twitterImage.ContentId, instaImage.ContentId,body,title);
            
            bodybuilder.HtmlBody = messageBody;

            if (attachment != null)
            {
                bodybuilder.Attachments.Add(attachment);
            }

            message.Body = bodybuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(smtp, int.Parse(port), false);
                    client.Authenticate(from, password);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
            }
            catch (System.Exception ex)
            {

                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

            return new Response
            {
                IsSuccess = true
            };
        }

        public List<SelectListItem> Destinations()
        {
            var options = new List<SelectListItem>();
            options.Insert(0, new SelectListItem
            {
                Text = " [Insert Destination] ",
                Value = "0"
            });
            options.Insert(1, new SelectListItem
            {
                Text = "Customers",
                Value = "1"
            });
            options.Insert(2, new SelectListItem
            {
                Text = "Employees",
                Value = "2"
            });

            return options;
        }

        public async Task<Response> SendAnnouncementAsync(int to, string title, string body, string path)
        {
            if (to > 0 && to < 3)
            {

                if (to == 1)
                {
                    var customers = await _userHelper.GetAllActiveCustomersAsync();

                    if (customers != null)
                    {
                        foreach (var customer in customers)
                        {
                            var response = await SendAnnouncement(customer.Email, title, body, string.IsNullOrEmpty(path) ? null : path);

                            if (response.IsSuccess == false)
                            {
                                return new Response { IsSuccess = false };
                            }
                        }

                        return new Response { IsSuccess = true };
                    }
                    else
                    {
                        return new Response { IsSuccess = false };
                    }
                }

                if (to == 2)
                {
                    var employees = await _userHelper.GetAllActiveEmployeesAsync();

                    if (employees != null)
                    {
                        foreach (var employee in employees)
                        {
                            var response = await SendAnnouncement(employee.Email, title, body, string.IsNullOrEmpty(path) ? null : path);

                            if (response.IsSuccess == false)
                            {
                                return new Response { IsSuccess = false };
                            }
                        }

                        return new Response { IsSuccess = true };
                    }
                    else
                    {
                        return new Response { IsSuccess = false };
                    }
                }

                return new Response { IsSuccess = false };

            }
            else
            {
                return new Response { IsSuccess = false };
            }
        }
    }
}
