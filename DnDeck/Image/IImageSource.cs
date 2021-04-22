using DnDeck.Monsters;

namespace DnDeck.Image
{
    public interface IImageSource
    {
        string GetImage(Monster monster);
    }
}