using DnDeck.Cards;
using DnDeck.Image;
using DnDeck.Monsters;
using NLog;

namespace DnDeck
{
    public class Runner
    {
        static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public void DoWork()
        {
            var images = new ImagesManager();
            images.LoadImages();

            var monsters = new MonstersManager(images);
            monsters.LoadJson();

            var cards = new CardsManager(monsters);
            cards.LoadCards(filterByList: true);
            cards.SaveCards();

            Logger.Info("Done!");
        }
    }
}