using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DnDeck.Monsters;
using NLog;

namespace DnDeck.Image
{
    public class ImagesManager: IImageSource
    {
        const string ImagesFolder = "Data/Images";
        const string RemoteRoot = "https://raw.githubusercontent.com/Xorboo/DnDeck/master/DnDeck/Data/Images";

        readonly ImageLoader ImageLoader = new ImageLoader();
        readonly Dictionary<string, string> Images = new(StringComparer.InvariantCultureIgnoreCase);

        static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public ImagesManager()
        {
            ImageLoader.ParseLocalImages();
        }

        public string GetImage(Monster monster)
        {
            string defaultName = ImageLoader.GetImage(monster);
            if (!string.IsNullOrWhiteSpace(defaultName))
            {
                string path = $"{RemoteRoot}/{Path.GetFileName(defaultName)}";
                if (Images.ContainsValue(path))
                {
                    return path;
                }
            }

            string enName = monster.EnName.Replace(' ', '_').Replace('/', '_');
            if (!Images.TryGetValue(enName, out var url))
            {
                Logger.Error($"Couldn't find image for '{monster.Name}' (converted name: '{enName}')");
            }

            return url;
        }

        public void LoadImages()
        {
            Logger.Info("Loading local images...");
            Images.Clear();

            var images = Directory.GetFiles(ImagesFolder);
            foreach (var image in images)
            {
                Images.Add(Path.GetFileNameWithoutExtension(image), $"{RemoteRoot}/{Path.GetFileName(image)}");
            }

            Logger.Info($"Loaded {Images.Count} images");
        }
    }
}