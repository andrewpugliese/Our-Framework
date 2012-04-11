using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B1.WebSite.Admin.Models
{
    public static class SiteLinkListHelper
    {
        public static int GetTopLevelParentId(IEnumerable<ISiteLink> siteLinks)
        {
            return siteLinks.OrderBy(i => i.ParentId).Select(i => i.ParentId).FirstOrDefault();
        }

        public static bool SiteLinkHasChildren(IEnumerable<ISiteLink> siteLinks, int id)
        {
            return siteLinks.Any(i => i.ParentId == id);
        }

        public static IEnumerable<ISiteLink> GetChildSiteLinks(IEnumerable<ISiteLink> siteLinks,
            int parentIdForChildren)
        {
            return siteLinks.Where(i => i.ParentId == parentIdForChildren)
                .OrderBy(i => i.SortOrder).ThenBy(i => i.Text);
        }
    }

}