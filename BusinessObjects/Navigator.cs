namespace BusinessObjects
{
    using HtmlAgilityPack;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Utility;

    public static class Navigator
    {
        private static readonly HashSet<string> History = new(StringComparer.InvariantCultureIgnoreCase);

        public static async Task<Result<URLInfo>> Crawl(string url, Action<URLInfo> requestedAction, int maxDepth = 0)
        {
            History.Clear();

            try
            {
                var urlInfo = new URLInfo(url, 0);
                if (!urlInfo.IsValid)
                {
                    return new Result<URLInfo>(false, "Invalid URL");
                }

                var res = await Navigate(urlInfo, requestedAction, maxDepth);

                if (!res.Success)
                {
                    return new Result<URLInfo>(false, res.Message);
                }

                return new Result<URLInfo>(urlInfo);
            }

            catch (Exception ex)
            {
                return new Result<URLInfo>(ex);
            }
        }

        // Recursive internal navigation method
        private static async Task<Result> Navigate(URLInfo urlInfo, Action<URLInfo> requestedAction, int maxDepth = 0)
        {
            try
            {
                string message;

                if (!string.IsNullOrEmpty(urlInfo.URL) && History.Contains(urlInfo.URL))
                {
                    message = $"\tURL already visited: {urlInfo.URL}";
                    Console.WriteLine(message);
                    return Result.Fail(message);
                }

                if (maxDepth > 0 && urlInfo.Depth >= maxDepth)
                {
                    message = $"\tURL too deep: {urlInfo.Depth}, {urlInfo.URL}";
                    Console.WriteLine(message);
                    return Result.Fail(message);
                }

                History.Add(urlInfo.URL);
                var res = await FetchURLAsync(urlInfo.URL);

                if (!res.Success)
                {
                    Console.WriteLine(res.Message);
                    return Result.Fail(res.Message);
                }

                var document = res.Data;
                var HTML = document.DocumentNode.OuterHtml;

                var innerLinks = ParseLinks(urlInfo, document.DocumentNode.SelectNodes("//a[@href]"));

                urlInfo.HTML = HTML;
                urlInfo.InnerLinks = innerLinks;
                urlInfo.Document = document;

                requestedAction(urlInfo);

                // Go recursive
                foreach (var link in innerLinks)
                {
                    await Navigate(link, requestedAction, maxDepth);
                }

                return Result.OK();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex);
            }
        }

        public static async Task<Result<HtmlDocument>> FetchURLAsync(string url)
        {
            try
            {
                var res = await DownloadURLAsync(url);
                if (!res.Success)
                {
                    return new Result<HtmlDocument>(false, res.Message);
                }

                // Fix some of the invalid HTML structures.
                var document = new HtmlDocument
                {
                    OptionOutputOriginalCase = false,
                    OptionOutputOptimizeAttributeValues = true,
                    OptionCheckSyntax = true,
                    OptionFixNestedTags = true,
                    OptionWriteEmptyNodes = false,
                    OptionAddDebuggingAttributes = false,
                    OptionAutoCloseOnEnd = true
                };

                document.LoadHtml(res.Data);
                return new Result<HtmlDocument>(document);
            }
            catch (Exception ex)
            {
                return new Result<HtmlDocument>(ex);
            }
        }

        private static List<URLInfo> ParseLinks(URLInfo urlInfo, HtmlNodeCollection allLinks)
        {
            var innerLinks = new List<URLInfo>();

            if (allLinks?.Count == 0 || string.IsNullOrWhiteSpace(urlInfo.URL))
            {
                return innerLinks;
            }

            var mainUri = new Uri(urlInfo.URL);

            foreach (HtmlNode link in allLinks)
            {
                HtmlAttribute attribute = link.Attributes["href"];
                string linkHref = attribute == null ? link.InnerText : attribute.Value; ;

                if (linkHref.NullOrEmpty() || !linkHref.IsValidLink())
                {
                    continue;
                }

                // Relative link to absolute link: "/contact", "/help"
                if (!linkHref.StartsWith("http"))
                {
                    linkHref = new Uri(mainUri, linkHref).ToString();
                }

                // "abc#anchor" => "abc"
                linkHref = linkHref.Split('#')[0];

                // Encode
                linkHref = linkHref.Replace("&amp;", "&");

                var childUri = new URLInfo(linkHref, urlInfo.Depth + 1);
                if (!childUri.IsValid ||
                    innerLinks.Contains(childUri) ||
                    urlInfo.DomainName != childUri.DomainName ||
                    string.Equals(childUri.URL, urlInfo.URL, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                innerLinks.Add(childUri);
            }

            return innerLinks.DistinctBy(l => l.URL).ToList();
        }

        public static async Task<Result<string>> DownloadURLAsync(string url,
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36")
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls13;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                var html = await client.GetStringAsync(url);

                return new Result<string>(html);
            }
            catch (Exception ex)
            {
                return new Result<string>(ex);
            }
        }
    }
}
