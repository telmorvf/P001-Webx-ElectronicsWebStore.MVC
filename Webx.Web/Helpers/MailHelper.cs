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

namespace Webx.Web.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContext;

        public MailHelper(IConfiguration configuration,IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContext)
        {
           _configuration = configuration;
           _webHostEnvironment = webHostEnvironment;
           _httpContext = httpContext;
        }


        public async Task<Response> SendConfirmationEmail(string to, string tokenLink,User customer)
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

            var pathToMainPage = _httpContext.HttpContext.Request.ToString();

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
            string messageBody = string.Format(htmlBody, pathToMainPage,customer.FullName, tokenLink,titleImage.ContentId,logoImage.ContentId,welcomeImage.ContentId,fbImage.ContentId,twitterImage.ContentId,instaImage.ContentId);

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

        public async Task<Response> SendResetPasswordEmail(string to, string link, User customer)
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

            var pathToMainPage = _httpContext.HttpContext.Request.ToString();

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
            string messageBody = string.Format(htmlBody, titleImage.ContentId, pathToMainPage, logoImage.ContentId, resetImage.ContentId,link, customer.Email, fbImage.ContentId, twitterImage.ContentId, instaImage.ContentId);

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
    }
}
