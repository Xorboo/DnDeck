using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DnDeck.Monsters
{
    public class Monster
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Fiction { get; set; }
        public string Size { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Alignment { get; set; }
        public string AC { get; set; }
        public string HP { get; set; }
        public string Speed { get; set; }
        public string Resist { get; set; }
        public string Immune { get; set; }
        public string ConditionImmune { get; set; }
        public string Senses { get; set; }
        public string Save { get; set; }
        public string Skill { get; set; }
        public string Passive { get; set; }
        public string Languages { get; set; }
        public string Spells { get; set; }

        public JToken Trait { get; set; }
        public List<Trait> Traits { get; set; } = new List<Trait>();
        public JToken Action { get; set; }
        public List<Trait> Actions { get; set; } = new List<Trait>();

        // Legendary, Lair, Local

        // Biomes
        public string Biom { get; set; }
        public List<string> Biomes { get; } = new List<string>();

        // Stats
        public int Str { get; set; }
        public int StrBonus { get; set; }
        public int Dex { get; set; }
        public int DexBonus { get; set; }
        public int Con { get; set; }
        public int ConBonus { get; set; }
        public int Int { get; set; }
        public int IntBonus { get; set; }
        public int Wis { get; set; }
        public int WisBonus { get; set; }
        public int Cha { get; set; }
        public int ChaBonus { get; set; }

        // Combat raiting
        public string CR { get; set; }
        public double CrValue { get; set; }
        public string Exp { get; set; }

        public override string ToString()
        {
            return $"[{CrValue}] {Name}";
        }
    }
}
