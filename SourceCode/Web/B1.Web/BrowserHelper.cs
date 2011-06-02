using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace B1.Web
{
    public class BrowserHelper
    {
        public bool Is_iOS_client(HttpRequest request)
        {
            string userAgent = request.Params["HTTP_USER_AGENT"];
            return userAgent.Contains("iPhone") || userAgent.Contains("iPod") || userAgent.Contains("iPad");
        }

        // request.Browser.IsMobileDevice --- NOT all mobile device supports HLS

    }
}
