using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using HtmlAgilityPack;
using NLog;

namespace DnDeck.Image
{
    public class ImageLoader: IImageSource
    {
        readonly Dictionary<string, string> Images = new(StringComparer.InvariantCultureIgnoreCase);
        readonly List<string> MissingImages = new();

        static readonly Uri WebHtmlPath = new("https://tentaculus.ru/monsters/index.html");
        const string BackupHtmlPath = "Data/Monsters.html";

        static readonly string BaseImageUrl = new Uri(WebHtmlPath, ".").OriginalString;

        static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public string GetImage(string monsterName)
        {
            if (!Images.TryGetValue(monsterName, out var url))
            {
                Logger.Info($"No default image for '{monsterName}'");
            }

            return url;
        }

        public void ParseWebImages()
        {
            // Doesn't work, needs a proper js execution for it to load actual data first
            // Saving the page locally and parsing it is faster for (since its a one-time thing)
            Logger.Info($"Parsing web images from '{WebHtmlPath.OriginalString}'");
            using WebClient client = new WebClient();
            string html = client.DownloadString(WebHtmlPath);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            ParseImages(doc);
        }

        public void ParseLocalImages()
        {
            Logger.Info($"Parsing local images from '{BackupHtmlPath}'");
            var doc = new HtmlDocument();
            doc.Load(BackupHtmlPath);

            ParseImages(doc);
        }

        public void DownloadImages()
        {
            Logger.Info($"Downloading {Images.Count} images...");

            string downloadDir = "Images/Site";
            if (!Directory.Exists(downloadDir))
            {
                Logger.Info($"Creating directory '{Path.GetFullPath(downloadDir)}'");
                Directory.CreateDirectory(downloadDir);
            }

            using WebClient client = new WebClient();
            foreach (var image in Images)
            {
                string imageUrl = image.Value;
                string imageName = Path.GetFileName(imageUrl);
                string imagePath = $"{downloadDir}/{imageName}";
                if (!File.Exists(imagePath))
                {
                    Logger.Debug($"Downloading '{imageUrl}'...");
                    client.DownloadFile(imageUrl, imagePath);
                }
            }

            Logger.Info($"Images downloaded");
        }

        void ParseImages(HtmlDocument doc)
        {
            Images.Clear();
            MissingImages.Clear();

            var monsters = doc.DocumentNode.SelectNodes("//div[@class='monsterContainer row']/div[@class='monster']");
            foreach (var monster in monsters)
            {
                var nameNode = monster.SelectSingleNode(".//div[@class='name']");
                string monsterName = FixName(nameNode.InnerText);

                var imageNode = monster.SelectSingleNode(".//img");
                string imageLocalPath = imageNode.GetDataAttribute("src").Value;
                if (string.IsNullOrWhiteSpace(imageLocalPath) || !imageLocalPath.EndsWith(".jpg"))
                {
                    Logger.Info($"Missing image: {monsterName}");
                    MissingImages.Add(monsterName);
                    continue;
                }

                string imageUrl = $"{BaseImageUrl}{HttpUtility.HtmlDecode(imageLocalPath)}";

                if (Images.ContainsValue(imageUrl))
                {
                    Logger.Info(
                        $"Images list already contains url '{imageUrl}' for '{Images.FirstOrDefault(x => x.Value == imageUrl).Key}, duplicating it for '{monsterName}");
                }

                Images.Add(monsterName, imageUrl);
            }

            Logger.Info($"Done parsing images. Found {Images.Count}, missing: {MissingImages.Count}");
        }

        // Unify image names to a common format "ru_name (en_name)
        static string FixName(string name)
        {
            return name.Replace("[", "(").Replace("]", ")");
        }
    }
}