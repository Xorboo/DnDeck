using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace DnDeck
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
        public string Exp { get; set; }
    }

    public class Trait
    {
        public string Name { get; set; }
        public string ParsedText { get; private set; }
        public JToken Text { get; set; }

        public void ParseText()
        {
            if (Text is JArray textArray)
            {
                var texts = textArray.ToObject<List<string>>();
                ParsedText = string.Join(" ", texts);
            }
            else if (Text is JValue textValue)
            {
                ParsedText = textValue.ToString();
            }
            else
            {
                throw new Exception();
            }
        }
    }

    public class CardData
    {
        public string title;
        public string icon;
        public List<string> contents = new List<string>();
        public string background_image;

        public CardData(Monster monster)
        {
            title = monster.Name;
            icon = "imp-laugh";
            background_image = monster.Image;

            GenerateContents(monster);
        }

        public (int w1, int w2) GetHeight()
        {
            double w1 = 5, w2 = 5;

            foreach (var content in contents)
            {
                double factor = content.Contains("<small>") ? 0.85 : 1;

                const string regex = "</{0,1}[^>]*>";
                string c = Regex.Replace(content, regex, "");

                if (c.StartsWith("subtitle"))
                {
                    if (c.Length > 49)
                        w2 += 1000;

                    w1 += 1.5;
                    w2 += 1.5;
                }
                else if (c.StartsWith("rule"))
                {
                    w1 += 1;
                    w2 += 1;
                }
                else if (c.StartsWith("dndstats"))
                {
                    w1 += 2.5;
                    w2 += 2.5;
                }
                else if (c.StartsWith("property"))
                {
                    w1 += factor * Math.Ceiling(c.Length / 132f);
                    w2 += factor * Math.Ceiling(c.Length / 60f);
                }
                else if (c.StartsWith("description"))
                {
                    w1 += factor * Math.Ceiling(c.Length / 136f);
                    w2 += factor * Math.Ceiling(c.Length / 63f);
                }
                else if (c.StartsWith("section"))
                {
                    w1 += 1.5;
                    w2 += 1.5;
                }
                else if (c.StartsWith("text"))
                {
                    w1 += factor * Math.Ceiling(c.Length / 132f);
                    w2 += factor * Math.Ceiling(c.Length / 60f);
                }
                else
                {
                    throw new Exception();
                }
            }

            return ((int)w1, (int)w2);
        }

        private void GenerateContents(Monster m)
        {
            AddText($"<small>{m.Size}, {m.Type}, {m.Alignment}</small>");
            AddLine();

            AddText($"<b>AC:</b> {m.AC}, <b>HP:</b> {m.HP}, <b>Скорость:</b> {m.Speed}");
            AddLine();

            contents.Add($"dndstats | {m.Str} | {m.Dex} | {m.Con} | {m.Int} | {m.Wis} | {m.Cha}");
            AddLine();

            AddPropertyIfExists("Спасбросок", m.Save);
            AddPropertyIfExists("Способности", m.Skill);
            AddPropertyIfExists("Чувства", m.Senses);
            AddPropertyIfExists("Языки", m.Languages);
            string challenge = @$"{m.CR} <em><span style=""color: gray"">({m.Exp} XP)</span></em>";
            AddProperty("Сложность", challenge);

            if (m.Traits.Count > 0)
            {
                AddSection("Особенности");
                foreach (var trait in m.Traits)
                {
                    AddDescription($"<em>{trait.Name}</em>", trait.ParsedText);
                }
            }

            if (m.Actions.Count > 0)
            {
                AddSection("Действия");
                foreach (var action in m.Actions)
                {
                    AddDescription($"<em>{action.Name}</em>", action.ParsedText);
                }
            }
        }

        private void AddPropertyIfExists(string name, string description)
        {
            if (description != null)
            {
                AddProperty(name, description);
            }
        }

        private void AddProperty(string name, string description)
        {
            string cleanDesc = CleanUrl(description);
            contents.Add($"property | {name} | {description}");
        }

        private void AddDescription(string name, string description)
        {
            string cleanDesc = CleanUrl(description);
            contents.Add($"description | {name} | {cleanDesc}");
        }

        private void AddSubtitle(string text)
        {
            contents.Add($"subtitle | {text}");
        }

        private void AddSection(string text)
        {
            contents.Add($"section | {text}");
        }

        private void AddText(string text)
        {
            contents.Add($"text | {text}");
        }

        private void AddLine()
        {
            contents.Add("rule");
        }

        static string CleanUrl(string text)
        {
            const string regex = "</{0,1}a[^>]*>";
            const string smartRegex = @"<a\b[^>]+>([^<]*(?:(?!</a)<[^<]*)*)</a>";
            return Regex.Replace(text, regex, "");
        }
    }
}
