/*
HTTPClient, GoogleSearch, Bing.
Завдання 1: створити додаток для пошуку інформації у пошуковій системі та відобразити першу сторінку результатів.
Завдання 2: додати одночасний пошук у різних пошукових системах, які обирає користувач.
Завдання 3: створити додаток для пошуку зображень та відобразити картинки з першої сторінки результатів.
Завдання 4: додати одночасний пошук зображень у різних пошукових системах, які обирає користувач.
*/
using System.Net.Http;
using System.Text.RegularExpressions;

namespace SearchAggregator;
internal static class Program
{
    private static readonly HttpClient Client = new();
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
        Console.Write("Search text: "); string? query = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(query)) return;
        Console.Write("1-text, 2-text in both, 3-images, 4-images in both: "); string? choice = Console.ReadLine();
        string value = Uri.EscapeDataString(query);
        Task[] jobs = choice switch
        {
            "1" => [FindTextAsync("Google", $"https://www.google.com/search?q={value}")],
            "2" => [FindTextAsync("Google", $"https://www.google.com/search?q={value}"), FindTextAsync("Bing", $"https://www.bing.com/search?q={value}")],
            "3" => [FindImagesAsync("Google", $"https://www.google.com/search?tbm=isch&q={value}")],
            "4" => [FindImagesAsync("Google", $"https://www.google.com/search?tbm=isch&q={value}"), FindImagesAsync("Bing", $"https://www.bing.com/images/search?q={value}")],
            _ => []
        };
        await Task.WhenAll(jobs); Console.WriteLine("Done.");
    }
    private static async Task FindTextAsync(string engine, string url)
    {
        try
        {
            string html = await Client.GetStringAsync(url);
            Console.WriteLine($"\n--- {engine}: text results ---"); int index = 0;
            foreach (Match match in Regex.Matches(html, "<a[^>]+href=\"(?<url>https?[^\"]+)\"[^>]*>(?<text>.*?)</a>", RegexOptions.Singleline | RegexOptions.IgnoreCase))
            {
                string title = Regex.Replace(match.Groups["text"].Value, "<.*?>", "").Trim(); string link = System.Net.WebUtility.HtmlDecode(match.Groups["url"].Value);
                if (title.Length < 3 || link.Contains("google.") || link.Contains("bing.com")) continue;
                Console.WriteLine($"{++index}. {System.Net.WebUtility.HtmlDecode(title)}\n   {link}"); if (index == 10) break;
            }
            if (index == 0) Console.WriteLine("No parseable results; the search engine may have changed its page.");
        }
        catch (Exception error) { Console.WriteLine($"{engine}: {error.Message}"); }
    }
    private static async Task FindImagesAsync(string engine, string url)
    {
        try
        {
            string html = await Client.GetStringAsync(url);
            Console.WriteLine($"\n--- {engine}: image URLs ---"); int index = 0;
            foreach (Match match in Regex.Matches(html, "https?:[^\"'\\s<>]+?\\.(?:jpg|jpeg|png|webp)", RegexOptions.IgnoreCase))
            {
                string imageUrl = match.Value.Replace("\\u003d", "="); if (imageUrl.Contains("gstatic") || imageUrl.Contains("favicon")) continue;
                Console.WriteLine($"{++index}. {imageUrl}"); if (index == 10) break;
            }
            if (index == 0) Console.WriteLine("No parseable image links; the search engine may have changed its page.");
        }
        catch (Exception error) { Console.WriteLine($"{engine}: {error.Message}"); }
    }
}
