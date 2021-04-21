using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
        List<CardData> LargeCards = new();
        List<CardData> HugeCards = new();

        readonly MonstersManager Monsters;
        static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public CardsManager(MonstersManager monsters)
        {
            Monsters = monsters;
        }

        public void LoadCards(double minCr = 0, double maxCr = 30)
        {
            Logger.Info($"Loading cards in CR range [{minCr}, {maxCr}]...");
            Cards = Monsters.Monsters
                .Where(m => minCr <= m.CrValue && m.CrValue <= maxCr)
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
            File.WriteAllText(Path.Combine(baseFolder, "output_2x1.json"), JsonConvert.SerializeObject(LargeCards));
            File.WriteAllText(Path.Combine(baseFolder, "output_1x1.json"), JsonConvert.SerializeObject(HugeCards));

            string pdfGeneratorUrl = @"https://tkfu.github.io/rpg-cards/generator/generate.html";
            Logger.Info($"Cards saved to '{Path.GetFullPath(baseFolder)}, now load those to '{pdfGeneratorUrl}' to create printable PDF versions");
        }

        void SplitCards()
        {
            Logger.Info($"Splitting {Cards.Count} by size");
            SmallCards = new List<CardData>();
            LargeCards = new List<CardData>();
            HugeCards = new List<CardData>();

            foreach (var card in Cards)
            {
                var h = card.GetHeight();
                if (h.smallHeight <= SmallCardHeight)
                {
                    SmallCards.Add(card);
                }
                else if (h.largeHeight <= LargeCardHeight)
                {
                    LargeCards.Add(card);
                }
                else
                {
                    HugeCards.Add(card);
                }
            }

            Logger.Info($"Cards split, {CardsDistributionLog}");
            FillCardPages(SmallCards, 4);
            FillCardPages(LargeCards, 2);
            FillCardPages(HugeCards, 1); // lol
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

        string CardsDistributionLog => $"small: {SmallCards.Count}, large: {LargeCards.Count}, huge: {HugeCards.Count}";
        int TotalPages => 2 * (SmallCards.Count / 4 + LargeCards.Count / 2 + HugeCards.Count);
    }
}