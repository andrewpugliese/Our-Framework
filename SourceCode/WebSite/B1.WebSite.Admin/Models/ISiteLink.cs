using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.WebSite.Admin.Models
{
    public interface ISiteLink
    {
        int Id { get; }
        int ParentId { get; }
        string Text { get; }
        string Url { get; }
        bool OpenInNewWindow { get; }
        int SortOrder { get; }
    }
}
