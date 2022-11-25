using System;

namespace Webx.Web.Helpers
{
    public class MyPaypalSetup
    {
        
        
            /// <summary>
            /// Provide value sandbox for testing,  provide value live for real money
            /// </summary>
            public string Environment { get; set; }
            /// <summary>
            /// Client id as provided by Paypal on dashboard. Ensure you use correct value based on your environment selection
            /// Use sandbox accounts for sandbox testing
            /// </summary>
            public string ClientId { get; set; }
            /// <summary>
            /// Secret as provided by Paypal on dashboard. Ensure you use correct value based on your environment selection
            /// Use sandbox accounts for sandbox testing
            /// </summary>
            public string Secret { get; set; }

            /// <summary>
            /// This is the URL that you will pass to paypal which paypal will use to redirect payer back to your website.
            /// So essentially it is the same controller URL that you must pass
            /// </summary>
            public string RedirectUrl { get; set; }

            /// <summary>
            /// Once order is created on Paypal, it redirects control to your app with a URL that shows order details. Your website must take the payer to this page
            /// so the payer approved the payment. Store this URL in this property
            /// </summary>
            public string ApproveUrl { get; set; }

            /// <summary>
            /// When paypal redirects control to your website it provides a Approved Order ID which we then pass it back to paypal to execute the order.
            /// Store this approved order ID in this property
            /// </summary>
            public string PayerApprovedOrderId { get; set; }
        
    }
}
