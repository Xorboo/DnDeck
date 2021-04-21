using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DnDeck.Monsters;
using NLog;

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

        static Dictionary<string, string> TypeIcons = new (StringComparer.InvariantCultureIgnoreCase)
        {
            {"гуманоид", "balaclava"},
            {"аберрация", "tentacles-skull"},
            {"монстр", "imp-laugh"},
            {"дракон", "dragon-spiral"},
            {"нежить", "shambling-zombie"},
            {"элементаль", "fire-dash"},
            {"зверь", "goose"},
            {"конструкт", "vintage-robot"},
            {"исчадие", "sharped-teeth-skull"},
            {"растение", "carnivorous-plant"},
            {"слизь", "dripping-goo"},
            {"фея", "fairy"},
            {"великан", "giant"},
            {"небожитель", "atlas"},
            {"демон", "lightning-mask"},
            {"рой крошечных зверей", "fly"},

        };

        static readonly string DefaultIcon = "targeted";

        static readonly ILogger Logger = LogManager.GetCurrentClassLogger();


        public CardData(Monster monster)
        {
            if (monster == null)
            {
                return;
            }

            Monster = monster;

            title = Monster.Name;
            LoadIcon();
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

        void LoadIcon()
        {
            if (!TypeIcons.TryGetValue(Monster.SimpleType, out icon))
            {
                Logger.Warn($"Couldn't find icon for monster '{Monster.SimpleType}', using '{DefaultIcon}' instead");
                icon = DefaultIcon;
            }
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

            AddTraitsBlock(m.Traits, "Особенности");
            AddTraitsBlock(m.Actions, "Действия");
            AddTraitsBlock(m.Reactions, "Реакция");
        }

        void AddTraitsBlock(List<Trait> traits, string header)
        {
            if (traits.Count > 0)
            {
                AddSection(header);
                foreach (var action in traits)
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