using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DnDeck.Monsters;
using Newtonsoft.Json;
using NLog;

namespace DnDeck.Cards
{
    public class CardsManager
    {
        const int SmallCardHeight = 43;
        const int LargeCardHeight = 42;

        List<CardData> Cards = new();
        List<CardData> SmallCards = new();
        List<CardData> MediumCards = new();
        List<CardData> LargeCards = new();

        readonly MonstersManager Monsters;
        static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        const string StandardDeckListPath = "Data/filter_list.txt";


        public CardsManager(MonstersManager monsters)
        {
            Monsters = monsters;
        }

        public void LoadCards(double minCr = 0, double maxCr = 30, string source = null, bool filterByList = false)
        {
            List<string> filterList = null;
            if (filterByList)
            {
                filterList = new List<string>(File.ReadAllLines(StandardDeckListPath));
                var missing = filterList
                    .Where(name => !Monsters.Monsters.Exists(m => m.EnName.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();
                if (missing.Count > 0)
                {
                    Logger.Error($"Standard monster list has {missing.Count} missing entries: {string.Join(", ", missing)}");
                }
            }

            var ogre = Monsters.Monsters.First(m => m.Name.StartsWith("Полуогр (Огриллон)")).EnName;

            Logger.Info($"Loading cards in CR range [{minCr}, {maxCr}], limited to list: {filterByList}, source: '{source}'...");
            Cards = Monsters.Monsters
                .Where(m => minCr <= m.CrValue && m.CrValue <= maxCr &&
                            (string.IsNullOrWhiteSpace(source) || m.Source == source) &&
                            (filterList == null || filterList.Contains(m.EnName, StringComparer.InvariantCultureIgnoreCase)))
                .Select(m => new CardData(m))
                .ToList();

            SplitCards();

            Logger.Info($"Loaded {Cards.Count} cards");
        }

        public void SaveCards()
        {
            Logger.Info($"Saving cards to json files...");

            string baseFolder = "Output";
            Directory.CreateDirectory(baseFolder);

            File.WriteAllText(Path.Combine(baseFolder, "output_full.json"), JsonConvert.SerializeObject(Cards));
            File.WriteAllText(Path.Combine(baseFolder, "output_2x2.json"), JsonConvert.SerializeObject(SmallCards));
            File.WriteAllText(Path.Combine(baseFolder, "output_2x1.json"), JsonConvert.SerializeObject(MediumCards));
            File.WriteAllText(Path.Combine(baseFolder, "output_1x1.json"), JsonConvert.SerializeObject(LargeCards));

            string pdfGeneratorUrl = @"https://tkfu.github.io/rpg-cards/generator/generate.html";
            Logger.Info($"Cards saved to '{Path.GetFullPath(baseFolder)}, now load those to '{pdfGeneratorUrl}' to create printable PDF versions");
        }

        void SplitCards()
        {
            Logger.Info($"Splitting {Cards.Count} by size");
            SmallCards = new List<CardData>();
            MediumCards = new List<CardData>();
            LargeCards = new List<CardData>();

            foreach (var card in Cards)
            {
                var h = card.GetHeight();
                if (h.smallHeight <= SmallCardHeight)
                {
                    card.AdjustHeaderSize(CardData.CardSize.Small);
                    SmallCards.Add(card);
                }
                else if (h.largeHeight <= LargeCardHeight)
                {
                    card.AdjustHeaderSize(CardData.CardSize.Medium);
                    MediumCards.Add(card);
                }
                else
                {
                    card.AdjustHeaderSize(CardData.CardSize.Large);
                    LargeCards.Add(card);
                }
            }

            Logger.Info($"Cards split, {CardsDistributionLog}");
            FillCardPages(SmallCards, 4);
            FillCardPages(MediumCards, 2);
            FillCardPages(LargeCards, 1); // lol
            Logger.Info($"Filled with empty cards, {CardsDistributionLog}");
            Logger.Info($"Total pages: {TotalPages}");
        }

        void FillCardPages(List<CardData> cards, int modulo)
        {
            int toAdd = (modulo - cards.Count % modulo) % modulo;
            for (int i = 0; i < toAdd; i++)
            {
                cards.Add(new CardData(null));
            }
        }

        string CardsDistributionLog => $"small: {SmallCards.Count}, large: {MediumCards.Count}, huge: {LargeCards.Count}";
        int TotalPages => 2 * (SmallCards.Count / 4 + MediumCards.Count / 2 + LargeCards.Count);
    }
}