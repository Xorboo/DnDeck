using DnDeck.Image;

namespace DnDeck
{
    public class Runner
    {
        public void DoWork()
        {
            var imageLoader = new ImageLoader();
            imageLoader.ParseLocalImages();
            imageLoader.DownloadImages();
        }
    }
}