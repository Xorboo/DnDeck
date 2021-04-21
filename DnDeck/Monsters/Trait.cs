using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DnDeck.Monsters
{
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
                throw new InvalidCastException();
            }
        }
    }
}