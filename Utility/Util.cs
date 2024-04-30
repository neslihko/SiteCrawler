namespace Utility
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Web;
    using System.Text.RegularExpressions;

    public static class Util
    {
        public static string CurrentDirectory => AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string[] urlQuerySplitters = new string[] { "&", "&amp;", "?" };
        private static readonly Regex regValidURL = new(@"^(http[s]*://)([a-z0-9-]+[.])*([a-z0-9-]+)+([:]\d+){0,1}([/?#].*)*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly char[] urlEnders = { '?', '#', '&' };

        public static bool ValidateURL(string url, out string newURL)
        {
            newURL = url;

            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            newURL = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(newURL)).Left(1000);
            if (newURL.StartsWith("//"))
            {
                newURL = string.Concat("http:", newURL);
            }

            if (!regValidURL.IsMatch(newURL))
            {
                return false;
            }

            newURL = newURL.Trim(urlEnders).Trim();
            int ix = newURL.IndexOf('?');

            if (ix < 0)
            {
                return true;
            }

            newURL = string.Concat(newURL.Substring(0, ix),
                    "?",
                    string.Join("&amp;",
                        newURL.Substring(ix + 1)
                        .Split(urlQuerySplitters, StringSplitOptions.RemoveEmptyEntries)
                        .Where(s => !s.NullOrEmpty())
                        .Distinct()))
                        .Trim(urlEnders).Trim();

            return true;
        }

        public static string Left(this string text, int length)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Length > length ? text.Substring(0, length) : text;
        }

        public static bool NullOrEmpty(this object str) => str == null || string.IsNullOrEmpty(str.ToString());

        public static bool IsValidLink(this string href) =>
           !string.IsNullOrEmpty(href) &&
           !href.StartsWith('#') &&
           !href.Contains('<') &&
           !href.Contains("mailto:") &&
           !href.ToLower().StartsWith("javascript:");

        public static string GetExceptionMessageRecursive(Exception ex)
        {
            string sMessage = string.Empty;

            if (ex != null)
            {
                sMessage = string.Concat(ex.Message, "<br>", ex.StackTrace, "<br>");

                if (ex.InnerException != null)
                {
                    sMessage += GetExceptionMessageRecursive(ex.InnerException);
                }
            }
            return string.IsNullOrEmpty(sMessage) ? "exception empty" : sMessage.Replace(" at ", "<br> at ");
        }
    }
}
