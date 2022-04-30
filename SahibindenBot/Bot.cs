using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack; //Required library for Web Scraping functions.

namespace SahibindenBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var htmlAdvertisements = await GetHtml("https://www.sahibinden.com");
            var data = ParseHtmlUsingHtmlAgilityPack(htmlAdvertisements);
            foreach (var item in data)
            {
                var priceData = await GetHtml("https://www.sahibinden.com" + item.Url);
                item.Price = GetPriceForAdvertisement(priceData);
            }
            var totalaveragePrice = GetAverageValue(data);
            Console.WriteLine("Total Average Price : " + totalaveragePrice);
            CreatePriceListFile(data,totalaveragePrice);
        }
        static async Task<string> GetHtml(string uri) // Getting Html Data for scraping by using Uri.
        {
            var client = new HttpClient();
            return await client.GetStringAsync(uri);
        }

        static List<Post> ParseHtmlUsingHtmlAgilityPack(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var repositories =
                htmlDoc
                    .DocumentNode
                    .SelectNodes("//div[@id='container']/div[3]/div/div[3]/div[3]/ul/li");
                    // Xpath for the main website

            List<Post> postList = new();

            foreach (var repo in repositories)
            {
                var name = repo.SelectSingleNode("a/span").InnerText;
                var url = repo.SelectSingleNode("a").Attributes["href"].Value.ToString();

                postList.Add(new Post()
                {
                    Title = name,
                    Url = url
                });
            }
            return postList;
        }
        static string GetPriceForAdvertisement(string html)  // Parsing data from advertisement main page for filling price
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            return htmlDoc
                    .DocumentNode
                    .SelectSingleNode("//div[@id='favoriteClassifiedPrice']").Attributes["value"].Value.ToString();
        }

        static int GetAverageValue(List<Post> posts) // get average value from post List
        {
            int total = 0;
            int count = 0;
            foreach (var item in posts)
            {
                total += Convert.ToInt32(item.Price);
                count++;
            }
            return total / count;
        }

        static void CreatePriceListFile(List<Post> posts, int totalPrice) // Creating a file to Desktop with Name, Price and Total Price
        {
            
            var fileName = @"C:\Users\PC\Desktop\Sahibinden.txt";
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                using (StreamWriter sw = File.CreateText(fileName))
                {
                    foreach (var item in posts)
                    {
                        sw.WriteLine("Title: {0}, Price: {1}", item.Title, item.Price);
                    }
                    sw.WriteLine("Total Price: " + totalPrice);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }
        }
    }
}
