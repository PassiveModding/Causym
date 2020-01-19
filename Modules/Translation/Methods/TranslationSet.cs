using System.Collections.Generic;

namespace Causym.Translation.TranslationService
{
    public class TranslationSet
    {
        public List<string> EmoteMatches { get; set; } = new List<string>();

        public string LanguageString { get; set; }
    }
}