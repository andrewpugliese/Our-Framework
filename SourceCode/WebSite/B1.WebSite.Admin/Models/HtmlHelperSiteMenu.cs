using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace B1.WebSite.Admin.Models
{
    public static class HtmlHelperSiteMenu
    {
        public static MvcHtmlString SiteMenuAsUnorderedList(this HtmlHelper helper, List<ISiteLink> siteLinks)
        {
            if (siteLinks == null || siteLinks.Count == 0)
                return MvcHtmlString.Empty;
            var topLevelParentId = SiteLinkListHelper.GetTopLevelParentId(siteLinks);
            return MvcHtmlString.Create(buildMenuItems(siteLinks, topLevelParentId));
        }

        private static string buildMenuItems(List<ISiteLink> siteLinks, int parentId)
        {
            var parentTag = new TagBuilder("ul");
            var childSiteLinks = SiteLinkListHelper.GetChildSiteLinks(siteLinks, parentId);
            foreach (var siteLink in childSiteLinks)
            {
                var itemTag = new TagBuilder("li");
                var anchorTag = new TagBuilder("a");
                anchorTag.MergeAttribute("href", siteLink.Url);
                anchorTag.SetInnerText(siteLink.Text);
                if (siteLink.OpenInNewWindow)
                {
                    anchorTag.MergeAttribute("target", "_blank");
                }
                itemTag.InnerHtml = anchorTag.ToString();
                if (SiteLinkListHelper.SiteLinkHasChildren(siteLinks, siteLink.Id))
                {
                    itemTag.InnerHtml += buildMenuItems(siteLinks, siteLink.Id);
                }
                parentTag.InnerHtml += itemTag;
            }
            return parentTag.ToString();
        }
    }
}