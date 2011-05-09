using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace B1.Web
{
    public static class RouteHelper
    {
        public static RouteData GetRouteData(this Uri uri)
        {
            return RouteTable.Routes.GetRouteData(new CustomHttpContext(uri));
        }

        public static RouteData GetRouteData(string url)
        {
            if (url == null || string.IsNullOrWhiteSpace(url)) return null;
            else return GetAbsoluteUri(url).GetRouteData();
        }

        public static Uri GetAbsoluteUri(string url)
        {
            // Check if the URL provided is already an absolute path
            if (url.IndexOf("://") == -1)
            {
                Uri requestUri = HttpContext.Current.Request.Url;
                string newUrl = requestUri.Scheme + "://" + requestUri.Authority;
                if (requestUri.Port != 80) newUrl += ":" + requestUri.Port.ToString();
                if (!url.StartsWith("/")) newUrl += "/";
                newUrl += url;
                return new Uri(newUrl);
            }

            return new Uri(url);
        }

        private class CustomHttpContext : HttpContextBase
        {
            private readonly HttpRequestBase _request;

            public CustomHttpContext(Uri uri)
                : base()
            {
                _request = new CustomRequestContext(uri);
            }

            public override HttpRequestBase Request { get { return _request; } }
        }

        private class CustomRequestContext : HttpRequestBase
        {
            private readonly string _appRelativePath;
            private readonly string _pathInfo;

            public CustomRequestContext(Uri uri)
                : base()
            {
                _pathInfo = uri.Query;
                string appPath = HttpContext.Current.Request.ApplicationPath;
                _appRelativePath = uri.AbsolutePath.StartsWith(appPath, StringComparison.OrdinalIgnoreCase) ?
                    uri.AbsolutePath.Substring(appPath.Length) : uri.AbsolutePath;
            }

            public override string AppRelativeCurrentExecutionFilePath
            {
                get { return string.Concat("~", _appRelativePath); }
            }

            public override string PathInfo { get { return _pathInfo; } }
        }
    }
}
