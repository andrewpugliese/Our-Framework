using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B1.WebSite.Admin.Models
{
    public class SiteMenuManager
    {
        public List<ISiteLink> GetSitemMenuItems()
        {
            var items = new List<ISiteLink>();
            // Top Level
            items.Add(new SiteMenuItem
            {
                Id = 1,
                ParentId = 0,
                Text = "Home",
                Url = "/",
                OpenInNewWindow = false,
                SortOrder = 0
            });
            items.Add(new SiteMenuItem
            {
                Id = 2,
                ParentId = 0,
                Text = "Services",
                Url = "/Services",
                OpenInNewWindow = false,
                SortOrder = 2
            });
            items.Add(new SiteMenuItem
            {
                Id = 3,
                ParentId = 0,
                Text = "Contact Us",
                Url = "/Contact-Us",
                OpenInNewWindow = false,
                SortOrder = 1
            });
            items.Add(new SiteMenuItem
            {
                Id = 4,
                ParentId = 0,
                Text = "Our Blog",
                Url = "http://www.iwantmymvc.com",
                OpenInNewWindow = true,
                SortOrder = 3
            });
            // Contact Us Children
            items.Add(new SiteMenuItem
            {
                Id = 5,
                ParentId = 3,
                Text = "Phone Numbers",
                Url = "/Contact-Us/Phone-Numbers",
                OpenInNewWindow = false,
                SortOrder = 0
            });
            items.Add(new SiteMenuItem
            {
                Id = 6,
                ParentId = 3,
                Text = "Map",
                Url = "/Contact-Us/Map",
                OpenInNewWindow = false,
                SortOrder = 1
            });
            // Services Children
            items.Add(new SiteMenuItem
            {
                Id = 7,
                ParentId = 2,
                Text = "Technical Writing",
                Url = "/Services/Tech-Writing",
                OpenInNewWindow = false,
                SortOrder = 0
            });
            items.Add(new SiteMenuItem
            {
                Id = 8,
                ParentId = 2,
                Text = "Consulting",
                Url = "/Services/Consulting",
                OpenInNewWindow = false,
                SortOrder = 1
            });
            items.Add(new SiteMenuItem
            {
                Id = 9,
                ParentId = 2,
                Text = "Training",
                Url = "/Services/Training",
                OpenInNewWindow = false,
                SortOrder = 2
            });
            // Services/TechnicalWriting Children
            items.Add(new SiteMenuItem
            {
                Id = 10,
                ParentId = 7,
                Text = "Blog Posting",
                Url = "/Services/Tech-Writing/Blogs",
                OpenInNewWindow = false,
                SortOrder = 0
            });
            items.Add(new SiteMenuItem
            {
                Id = 11,
                ParentId = 7,
                Text = "Books",
                Url = "/Services/Tech-Writing/Books",
                OpenInNewWindow = false,
                SortOrder = 1
            });

            return items;
        }
    }
}