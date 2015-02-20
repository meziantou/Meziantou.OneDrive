using System.Globalization;

namespace Meziantou.OneDrive
{
    public class GetOptions
    {
        public int? Offset { get; set; }
        public int? Limit { get; set; }
        public ItemTypeFilter Filters { get; set; }
        internal bool? Pretty { get; set; }
        public CultureInfo Locale { get; set; }
        public bool? ReturnSslResources { get; set; }
        internal bool? SuppressRedirect { get; set; }
        public SortProperty SortOrder { get; set; }
        public SortDirection SortDirection { get; set; }
    }
}
