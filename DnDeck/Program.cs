using System;
using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Extensions.Logging;
using ILogger = NLog.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DnDeck
{
    class Program
    {
        private static readonly Dictionary<string, string> Sources = new Dictionary<string, string>()
        {
            {"MM", "Monsters Manual + DMG"},
            {"VGtM", "Volo's Guide to Monsters"},
            {"XGTE", "Xanathar's Guide to Everything"},
            {"MToF", "Mordenkainen's Tome of Foes"},
        };

        private static readonly Dictionary<string, string> Exp = new Dictionary<string, string>()
        {
            {"0", "0 - 10"},
            {"1/8", "25"},
            {"1/4", "50"},
            {"1/2", "100"},
            {"1", "200"},
            {"2", "450"},
            {"3", "700"},
            {"4", "1100"},
            {"5", "1800"},
            {"6", "2300"},
            {"7", "2900"},
            {"8", "3900"},
            {"9", "5000"},
            {"10", "5900"},
            {"11", "7200"},
            {"12", "8400"},
            {"13", "10000"},
            {"14", "11500"},
            {"15", "13000"},
            {"16", "15000"},
            {"17", "18000"},
            {"18", "20000"},
            {"19", "22000"},
            {"20", "25000"},
            {"21", "33000"},
            {"22", "41000"},
            {"23", "50000"},
            {"24", "62000"},
            {"25", "75000"},
            {"26", "90000"},
            {"27", "105000"},
            {"28", "120000"},
            {"29", "135000"},
            {"30", "155000"},
        };

        private static readonly Dictionary<string, string> Size = new Dictionary<string, string>()
        {
            {"T", "Крошечный"},
            {"S", "Маленький"},
            {"M", "Средний"},
            {"L", "Большой"},
            {"H", "Огромный"},
            {"G", "Колоссальный"},
        };

        private static readonly Dictionary<string, string> Biomes = new Dictionary<string, string>()
        {
            {"ARCTIC", "Заполярье"},
            {"COASTAL", "Побережье"},
            {"DESERT", "Пустыня"},
            {"FOREST", "Лес"},
            {"GRASSLAND", "Равнина"},
            {"HILL", "Холмы"},
            {"MOUNTAIN", "Горы"},
            {"SWAMP", "Болота"},
            {"URBAN", "Город"},
            {"ASTRAL", "Астрал"},
            {"DUNDERDARK", "Подземье"},
            {"UNDERDARK", "Подземье"},
        };

        private static readonly Dictionary<string, string> Images = new Dictionary<string, string>();
        private static readonly List<string> MissingImages = new List<string>();
        private static List<Monster> Monsters = new List<Monster>();
        private static readonly List<CardData> Cards = new List<CardData>();

        private static readonly List<CardData> CardsW1 = new List<CardData>();
        private static readonly List<CardData> CardsW2 = new List<CardData>();

        static ILogger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory
                    .GetCurrentDirectory()) //From NuGet Package Microsoft.Extensions.Configuration.Json
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var servicesProvider = BuildDi(config);
            using (servicesProvider as IDisposable)
            {
                var runner = servicesProvider.GetRequiredService<Runner>();
                runner.DoWork();
            }

            LogManager.Shutdown();
            return;

            ParseImages();

            ParseJson();
            ProcessJson();

            PrepareCards();
            SplitCards();
            SaveCards();

            Console.WriteLine("\nDone!");
            Console.ReadKey();
        }

        static IServiceProvider BuildDi(IConfiguration config)
        {
            return new ServiceCollection()
                .AddTransient<Runner>()
                .AddLogging(loggingBuilder =>
                {
                    // configure Logging with NLog
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                    loggingBuilder.AddNLog(config);
                })
                .BuildServiceProvider();
        }

        private static void SplitCards()
        {
            CardsW1.Clear();
            CardsW2.Clear();

            foreach (var card in Cards)
            {
                if (!card.title.StartsWith("Аллип"))
                {
                    continue;
                }
                var h = card.GetHeight();
                if (h.w2 <= 41)
                {
                    CardsW2.Add(card);
                }
                else
                {
                    if (h.w1 > 41)
                    {
                        //Console.WriteLine($"{h.w1} -> {card.title}");
                    }
                    CardsW1.Add(card);
                }
            }

            Console.WriteLine($"Full: {CardsW1.Count}, half: {CardsW2.Count}");
        }

        private static void SaveCards()
        {
            File.WriteAllText("output.json", JsonConvert.SerializeObject(Cards));
            File.WriteAllText("output_2x1.json", JsonConvert.SerializeObject(CardsW1));
            File.WriteAllText("output_2x2.json", JsonConvert.SerializeObject(CardsW2));
        }

        private static void PrepareCards()
        {
            Cards.Clear();

            foreach (var monster in Monsters)
            {
                if (monster.Source == "Monsters Manual + DMG" &&
                    (monster.CR == "0" || monster.CR == "1/8" || monster.CR == "1/4" || monster.CR == "1/2" ||
                     monster.CR == "1" || monster.CR == "2" || monster.CR == "3" || monster.CR == "4" ||
                     monster.CR == "5"))
                {
                    Cards.Add(new CardData(monster));
                }
            }

            Console.WriteLine($"Total cards: {Cards.Count}");
        }

        static void ParseImages()
        {
            const string baseImageUrl = "https://tentaculus.ru/monsters";
            const string filePath = "Монстры.html";

            Images.Clear();
            MissingImages.Clear();

            var doc = new HtmlDocument();
            doc.Load(filePath);

            var monsters = doc.DocumentNode.SelectNodes("//div[@class='monsterContainer row']/div[@class='monster']");
            foreach (var monster in monsters)
            {
                var nameNode = monster.SelectSingleNode(".//div[@class='name']");
                string monsterName = nameNode.InnerText;

                var imageNode = monster.SelectSingleNode(".//img");
                string imageLocalPath = imageNode.GetDataAttribute("src").Value;
                if (string.IsNullOrWhiteSpace(imageLocalPath) || !imageLocalPath.EndsWith(".jpg"))
                {
                    MissingImages.Add(monsterName);
                    continue;
                }

                string imageUrl = $"{baseImageUrl}/{imageLocalPath}";
                Images.Add(FixName(monsterName), imageUrl);

            }

            Console.WriteLine($"Found images: {Images.Count}, missing images: {MissingImages.Count},");
        }

        static void ParseJson()
        {
            const string filePath = "monsters.json";
            string data = File.ReadAllText(filePath);

            Monsters = JsonConvert.DeserializeObject<List<Monster>>(data);
            Console.WriteLine($"Total monsters: {Monsters.Count}");
        }

        static void ProcessJson()
        {
            foreach (var monster in Monsters)
            {
                ProcessMonster(monster);
            }
        }

        private static void ProcessMonster(Monster monster)
        {
            monster.Name = FixName(monster.Name);

            monster.Size = Size[monster.Size];
            monster.Source = Sources[monster.Source];

            if (monster.Biom != null)
            {
                var biomesList = monster.Biom.Split(",",
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var biome in biomesList)
                {
                    monster.Biomes.Add(Biomes[biome]);
                }
            }

            monster.StrBonus = GetBonus(monster.Str);
            monster.DexBonus = GetBonus(monster.Dex);
            monster.ConBonus = GetBonus(monster.Con);
            monster.IntBonus = GetBonus(monster.Int);
            monster.WisBonus = GetBonus(monster.Wis);
            monster.ChaBonus = GetBonus(monster.Cha);

            monster.Exp = Exp[monster.CR];

            monster.Image = FindImage(monster.Name);

            monster.Traits = ReadTraits(monster.Trait);
            monster.Actions = ReadTraits(monster.Action);
        }

        private static string FixName(string name)
        {
            return name.Replace("[", "(").Replace("]", ")");
        }

        private static List<Trait> ReadTraits(JToken token)
        {
            List<Trait> traits = new List<Trait>();
            if (token == null)
            {
                return traits;
            }

            if (token is JArray)
            {
                traits = token.ToObject<List<Trait>>();
            }
            else if (token is JObject)
            {
                traits = new List<Trait> {token.ToObject<Trait>()};
            }
            else
            {
                throw new Exception();
            }

            foreach (var trait in traits)
            {
                trait.ParseText();
            }

            return traits;
        }

        private static string FindImage(string monsterName)
        {
            if (Images.TryGetValue(monsterName, out var url))
            {
                return url;
            }

            Console.WriteLine($"Error finding image url: {monsterName}");
            return "";
        }

        static int GetBonus(int stat)
        {
            return (int)Math.Floor((stat - 10) / 2f);
        }
    }
}
