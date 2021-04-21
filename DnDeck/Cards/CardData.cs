using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DnDeck.Monsters;

namespace DnDeck.Cards
{
    public class CardData
    {
        public string title;
        public string icon;
        public string color = "FireBrick";
        public List<string> contents = new List<string>();
        public string background_image;

        readonly Monster Monster;

        public CardData(Monster monster)
        {
            if (monster == null)
            {
                return;
            }

            Monster = monster;

            title = Monster.Name;
            icon = "imp-laugh";
            background_image = Monster.Image;

            GenerateContents(Monster);
        }

        public (int largeHeight, int smallHeight) GetHeight()
        {
            double largeHeight = 5, smallHeight = 5;

            foreach (var content in contents)
            {
                double factor = content.Contains("<small>") ? 0.85 : 1;

                const string regex = "</{0,1}[^>]*>";
                string c = Regex.Replace(content, regex, "");

                if (c.StartsWith("subtitle"))
                {
                    if (c.Length > 49)
                        smallHeight += 1000;

                    largeHeight += 1.5;
                    smallHeight += 1.5;
                }
                else if (c.StartsWith("rule"))
                {
                    largeHeight += 1;
                    smallHeight += 1;
                }
                else if (c.StartsWith("dndstats"))
                {
                    largeHeight += 2.5;
                    smallHeight += 2.5;
                }
                else if (c.StartsWith("property"))
                {
                    largeHeight += factor * Math.Ceiling(c.Length / 132f);
                    smallHeight += factor * Math.Ceiling(c.Length / 60f);
                }
                else if (c.StartsWith("description"))
                {
                    largeHeight += factor * Math.Ceiling(c.Length / 136f);
                    smallHeight += factor * Math.Ceiling(c.Length / 63f);
                }
                else if (c.StartsWith("section"))
                {
                    largeHeight += 1.5;
                    smallHeight += 1.5;
                }
                else if (c.StartsWith("text"))
                {
                    largeHeight += factor * Math.Ceiling(c.Length / 132f);
                    smallHeight += factor * Math.Ceiling(c.Length / 60f);
                }
                else
                {
                    throw new Exception();
                }
            }

            return ((int) largeHeight, (int) smallHeight);
        }

        void GenerateContents(Monster m)
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

        void AddPropertyIfExists(string name, string description)
        {
            if (description != null)
            {
                AddProperty(name, description);
            }
        }

        void AddProperty(string name, string description)
        {
            string cleanDesc = CleanUrl(description);
            contents.Add($"property | {name} | {description}");
        }

        void AddDescription(string name, string description)
        {
            string cleanDesc = CleanUrl(description);
            contents.Add($"description | {name} | {cleanDesc}");
        }

        void AddSubtitle(string text)
        {
            contents.Add($"subtitle | {text}");
        }

        void AddSection(string text)
        {
            contents.Add($"section | {text}");
        }

        void AddText(string text)
        {
            contents.Add($"text | {text}");
        }

        void AddLine()
        {
            contents.Add("rule");
        }

        static string CleanUrl(string text)
        {
            const string regex = "</{0,1}a[^>]*>";
            // const string smartRegex = @"<a\b[^>]+>([^<]*(?:(?!</a)<[^<]*)*)</a>";
            return Regex.Replace(text, regex, "");
        }
    }
}