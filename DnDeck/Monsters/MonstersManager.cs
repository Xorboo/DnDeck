using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DnDeck.Image;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace DnDeck.Monsters
{
    public class MonstersManager
    {
        const string JsonPath = "Data/monsters.json";

        readonly IImageSource Images;

        public List<Monster> Monsters { get; private set; } = new();

        static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public MonstersManager(IImageSource images)
        {
            Images = images;
        }

        public void LoadJson()
        {
            ReadJson();
            ProcessJson();
        }

        void ReadJson()
        {
            Logger.Info($"Loading monsters from '{JsonPath}'...");
            string data = File.ReadAllText(JsonPath);

            Monsters = JsonConvert.DeserializeObject<List<Monster>>(data);
            Logger.Info($"Total monsters loaded: {Monsters.Count}");
        }

        void ProcessJson()
        {
            Logger.Info($"Processing {Monsters.Count} monsters...");
            foreach (var monster in Monsters)
            {
                ProcessMonster(monster);
            }

            Monsters = Monsters.OrderBy(m => m.CrValue).ToList();

            Logger.Info("Monsters processed");
        }

        void ProcessMonster(Monster monster)
        {
            monster.Name = FixName(monster.Name);

            monster.Size = Parameters.Size[monster.Size];
            monster.Source = Parameters.Sources[monster.Source];

            if (monster.Biom != null)
            {
                var biomesList = monster.Biom.Split(",",
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var biome in biomesList)
                {
                    monster.Biomes.Add(Parameters.Biomes[biome]);
                }
            }

            monster.StrBonus = GetBonus(monster.Str);
            monster.DexBonus = GetBonus(monster.Dex);
            monster.ConBonus = GetBonus(monster.Con);
            monster.IntBonus = GetBonus(monster.Int);
            monster.WisBonus = GetBonus(monster.Wis);
            monster.ChaBonus = GetBonus(monster.Cha);

            monster.Exp = Parameters.Exp[monster.CR];
            monster.CrValue = Parameters.CrValues[monster.CR];

            monster.Image = Images.GetImage(monster);

            monster.Traits = ReadTraits(monster.Trait);
            monster.Actions = ReadTraits(monster.Action);
            monster.Reactions = ReadTraits(monster.Reaction);
        }

        static string FixName(string name)
        {
            return name.Replace("[", "(").Replace("]", ")");
        }

        List<Trait> ReadTraits(JToken token)
        {
            List<Trait> traits = new List<Trait>();
            if (token == null)
            {
                return traits;
            }

            if (token is JArray)
            {
                traits = token.ToObject<List<Trait>>();
            }
            else if (token is JObject)
            {
                traits = new List<Trait> {token.ToObject<Trait>()};
            }
            else
            {
                throw new Exception();
            }

            foreach (var trait in traits)
            {
                trait.ParseText();
            }

            return traits;
        }

        static int GetBonus(int stat)
        {
            return (int) Math.Floor((stat - 10) / 2f);
        }
    }
}