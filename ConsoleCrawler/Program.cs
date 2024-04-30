namespace ConsoleCrawler
{
    using BusinessObjects;

    using System.Text;

    using Utility;

    internal class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                var url = GetURL();
                if (string.IsNullOrWhiteSpace(url))
                {
                    break;
                }

                var depth = GetDepth();

                var res = await Navigator.Crawl(url, SaveURLToFile, depth);

                if (!res.Success)
                {
                    Console.WriteLine(res.Message);
                }
            }

            Console.WriteLine("Bye");
        }

        static void SaveURLToFile(URLInfo urlInfo)
        {
            var fileName = $"{urlInfo.Depth}-{Guid.NewGuid()}.txt";
            var path = Path.Combine(Util.CurrentDirectory, "Out", fileName);

            Console.WriteLine($"Saving {urlInfo.Depth}: {urlInfo.URL}");
            Console.WriteLine($"File name: {path}");

            var info = new FileInfo(path);
            if (!Directory.Exists(info.DirectoryName))
            {
                Directory.CreateDirectory(info.DirectoryName);
            }

            var text = string.Join(Environment.NewLine,
                $"<!-- {urlInfo.URL} -->",
                $"<!-- Downloaded at {DateTime.Now} -->",
                urlInfo.HTML);

            File.WriteAllText(path, text, Encoding.UTF8);
        }

        static string? GetURL()
        {
            Console.WriteLine(new string('*', 100));
            Console.Write("Enter URL, or hit enter to exit: ");
            return Console.ReadLine();
        }

        static int GetDepth()
        {
            Console.Write("Enter crawl depth (ignored if < 1)): ");
            return int.TryParse(Console.ReadLine(), out var depth) ? Math.Max(0, depth) : 0;
        }
    }
}