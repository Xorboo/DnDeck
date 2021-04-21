using System.Collections.Generic;

namespace DnDeck.Monsters
{
    public static class Parameters
    {
        public static readonly Dictionary<string, string> Sources = new()
        {
            {"MM", "Monsters Manual + DMG"},
            {"VGtM", "Volo's Guide to Monsters"},
            {"XGTE", "Xanathar's Guide to Everything"},
            {"MToF", "Mordenkainen's Tome of Foes"},
        };

        public static readonly Dictionary<string, string> Exp = new()
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

        public static readonly Dictionary<string, double> CrValues = new()
        {
            {"0", 0},
            {"1/8", 0.125},
            {"1/4", 0.25},
            {"1/2", 0.5},
            {"1", 1},
            {"2", 2},
            {"3", 3},
            {"4", 4},
            {"5", 5},
            {"6", 6},
            {"7", 7},
            {"8", 8},
            {"9", 9},
            {"10", 10},
            {"11", 11},
            {"12", 12},
            {"13", 13},
            {"14", 14},
            {"15", 15},
            {"16", 16},
            {"17", 17},
            {"18", 18},
            {"19", 19},
            {"20", 20},
            {"21", 21},
            {"22", 22},
            {"23", 23},
            {"24", 24},
            {"25", 25},
            {"26", 26},
            {"27", 27},
            {"28", 28},
            {"29", 29},
            {"30", 30},
        };

        public static readonly Dictionary<string, string> Size = new()
        {
            {"T", "Крошечный"},
            {"S", "Маленький"},
            {"M", "Средний"},
            {"L", "Большой"},
            {"H", "Огромный"},
            {"G", "Колоссальный"},
        };

        public static readonly Dictionary<string, string> Biomes = new()
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

    }
}