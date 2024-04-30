namespace BusinessObjects
{
    using HtmlAgilityPack;

    using System;
    using System.Collections.Generic;

    using Utility;

#pragma warning disable S101 // Types should be named in PascalCase
    public class URLInfo
#pragma warning restore S101 // Types should be named in PascalCase
    {
        public string? URL { get; set; }

        public string? DomainName { get; private set; }

        public bool IsValid { get; set; }

        public string? HTML { get; set; }

        public HtmlDocument? Document { get; set; }

        public List<URLInfo> InnerLinks { get; set; } = new List<URLInfo>();

        public int Depth { get; set; } = 0;

        public URLInfo(string url, int depth = 0)
        {
            Depth = depth;

            try
            {
                if (!Util.ValidateURL(url, out string newURL))
                {
                    IsValid = false;
                    return;
                }

                URL = newURL;
                DomainName = new Uri(URL).Authority.ToLower();
                IsValid = true;
            }
            catch
            {
                IsValid = false;
            }
        }

        public override string? ToString() => URL;

        public override bool Equals(object? obj)
        {
            if (obj is not URLInfo other)
            {
                return false;
            }

            return string.Equals(URL, other.URL, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => URL.ToLowerInvariant().GetHashCode();
    }
}
