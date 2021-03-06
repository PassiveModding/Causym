using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Causym.Modules.Translation
{
    public class YandexTranslator
    {
        private SpecificCulture[] availableLanguages;

        public YandexTranslator(string apiKey)
        {
            Client = new HttpClient();
            ApiKey = apiKey;

            // Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            PopulateLanguages();
        }

        public HttpClient Client { get; }

        public string ApiKey { get; }

        public bool IsValidLanguageCode(string code)
        {
            if (availableLanguages.All(x => !x.BaseCulture.Name.Equals(code, StringComparison.InvariantCultureIgnoreCase) && !x.SpecificName.Equals(code, StringComparison.InvariantCultureIgnoreCase)))
            {
                return false;
            }

            return true;
        }

        public SpecificCulture[] GetAvailableLanguages()
        {
            return availableLanguages;
        }

        public TranslationResult TranslateText(string source, string targetLanguage)
        {
            if (!IsValidLanguageCode(targetLanguage))
            {
                throw new Exception("Invalid Target Language Code.");
            }

            // TODO: fix source text for uri encoding.
            var response = Client.GetAsync($"https://translate.yandex.net/api/v1.5/tr.json/translate?key={ApiKey}&text={Uri.EscapeDataString(source)}&lang={targetLanguage}").Result;
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseJson = response.Content.ReadAsStringAsync().Result;
            var token = JToken.Parse(responseJson);

            var result = new TranslationResult();

            var lang = token.Value<JToken>("lang").ToString();
            var splitChar = lang.IndexOf("-");

            // TODO: Default if split char is not found.
            var sourceLang = lang.Substring(0, splitChar);
            var destLang = lang.Substring(splitChar + 1);
            var text = token.Value<JArray>("text").FirstOrDefault().ToString();
            result.DestinationLanguage = destLang;
            result.SourceLanguage = sourceLang;
            result.SourceText = source;
            result.TranslatedText = TranslateService.FixTranslatedString(text);

            return result;
        }

        public TranslationResult TranslateText(string source, string sourceLanguage, string targetLanguage)
        {
            if (!IsValidLanguageCode(targetLanguage))
            {
                throw new Exception("Invalid Target Language Code.");
            }
            else if (!IsValidLanguageCode(sourceLanguage))
            {
                throw new Exception("Invalid Source Language Code.");
            }

            source = Uri.EscapeDataString(source);
            var response = Client.GetAsync($"https://translate.yandex.net/api/v1.5/tr.json/translate?key={ApiKey}&text={source}&lang={sourceLanguage}-{targetLanguage}").Result;
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseJson = response.Content.ReadAsStringAsync().Result;
            var token = JToken.Parse(responseJson);

            var result = new TranslationResult();

            var text = token.Value<JArray>("text").FirstOrDefault().ToString();
            result.DestinationLanguage = targetLanguage;
            result.SourceLanguage = sourceLanguage;
            result.SourceText = source;
            result.TranslatedText = TranslateService.FixTranslatedString(text);

            return result;
        }

        private void PopulateLanguages()
        {
            var response = Client.GetAsync($"https://translate.yandex.net/api/v1.5/tr.json/getLangs?key={ApiKey}&ui=en").GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                availableLanguages = Array.Empty<SpecificCulture>();
            }

            var jResponse = JToken.Parse(response.Content.ReadAsStringAsync().Result);

            // var directions = jResponse.Value<JArray>("dirs");
            var langs = jResponse.Value<JObject>("langs");
            var cultureInfos = new List<SpecificCulture>();
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(x => new SpecificCulture(x));
            foreach (var lang in langs)
            {
                var cultureMatch = cultures.FirstOrDefault(x => x.BaseCulture.Name.Equals(lang.Key, StringComparison.InvariantCultureIgnoreCase) || x.SpecificName.Equals(lang.Key, StringComparison.InvariantCultureIgnoreCase));
                if (cultureMatch != null)
                {
                    cultureInfos.Add(cultureMatch);
                }
            }

            availableLanguages = cultureInfos.ToArray();
        }
    }
}