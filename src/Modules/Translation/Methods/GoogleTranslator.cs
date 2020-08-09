using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Causym.Modules.Translation.Methods
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using Newtonsoft.Json.Linq;

    namespace Causym.Modules.Translation
    {
        public class GoogleTranslator
        {
            private SpecificCulture[] availableLanguages;

            public GoogleTranslator()
            {
                Client = new HttpClient();

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

            private async Task<(string translatedMessage, string detectedSource)> TranslateMessageAsync(string sourcelang, string language, string message)
            {
                // https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=ru&dt=t&ie=UTF-8&oe=UTF-8&q=hi there this is a test message
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourcelang}&tl={language}&dt=t&ie=UTF-8&oe=UTF-8&q={Uri.EscapeDataString(message)}";

                try
                {
                    var content = await Client.GetStringAsync(url);
                    var jResponse = JArray.Parse(content);
                    var stringList = jResponse[0].Select(section => section[0].ToString()).ToList();
                    var translatedMessage = string.Join(string.Empty, stringList);
                    var lang = jResponse.Last()[0]?[0]?.ToString();
                    return (translatedMessage, lang);
                }
                catch (Exception e)
                {
                    return (null, null);
                }
            }

            private async Task<(string translatedMessage, string detectedSource)> TranslateMessageAsync(string language, string message)
            {
                // https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=ru&dt=t&ie=UTF-8&oe=UTF-8&q=hi there this is a test message
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={language}&dt=t&ie=UTF-8&oe=UTF-8&q={Uri.EscapeDataString(message)}";

                try
                {
                    var content = await Client.GetStringAsync(url);
                    var jResponse = JArray.Parse(content);
                    var stringList = jResponse[0].Select(section => section[0].ToString()).ToList();
                    var translatedMessage = string.Join(string.Empty, stringList);
                    var lang = jResponse.Last()[0]?[0]?.ToString();
                    return (translatedMessage, lang);
                }
                catch (Exception e)
                {
                    return (null, null);
                }
            }

            public TranslationResult TranslateText(string source, string targetLanguage)
            {
                if (!IsValidLanguageCode(targetLanguage))
                {
                    throw new Exception("Invalid Target Language Code.");
                }

                var response = TranslateMessageAsync(targetLanguage, source).Result;
                if (response.translatedMessage == null)
                {
                    return null;
                }

                var result = new TranslationResult();

                result.DestinationLanguage = targetLanguage;
                result.SourceLanguage = response.detectedSource;
                result.SourceText = source;
                result.TranslatedText = TranslateService.FixTranslatedString(response.translatedMessage);

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

                var response = TranslateMessageAsync(source, targetLanguage, source).Result;
                if (response.translatedMessage == null)
                {
                    return null;
                }

                var result = new TranslationResult();

                result.DestinationLanguage = targetLanguage;
                result.SourceLanguage = sourceLanguage;
                result.SourceText = source;
                result.TranslatedText = TranslateService.FixTranslatedString(response.translatedMessage);

                return result;
            }

            private void PopulateLanguages()
            {
                var langs = new Dictionary<string, string>
                {
                    { "auto", "Automatic"},
                    { "af", "Afrikaans"},
                    { "sq", "Albanian"},
                    { "am", "Amharic"},
                    { "ar", "Arabic"},
                    { "hy", "Armenian"},
                    { "az", "Azerbaijani"},
                    { "eu", "Basque"},
                    { "be", "Belarusian"},
                    { "bn", "Bengali"},
                    { "bs", "Bosnian"},
                    { "bg", "Bulgarian"},
                    { "ca", "Catalan"},
                    { "ceb", "Cebuano"},
                    { "ny", "Chichewa"},
                    { "zh-cn", "Chinese Simplified"},
                    { "zh-tw", "Chinese Traditional"},
                    { "co", "Corsican"},
                    { "hr", "Croatian"},
                    { "cs", "Czech"},
                    { "da", "Danish"},
                    { "nl", "Dutch"},
                    { "en", "English"},
                    { "eo", "Esperanto"},
                    { "et", "Estonian"},
                    { "tl", "Filipino"},
                    { "fi", "Finnish"},
                    { "fr", "French"},
                    { "fy", "Frisian"},
                    { "gl", "Galician"},
                    { "ka", "Georgian"},
                    { "de", "German"},
                    { "el", "Greek"},
                    { "gu", "Gujarati"},
                    { "ht", "Haitian Creole"},
                    { "ha", "Hausa"},
                    { "haw", "Hawaiian"},
                    { "iw", "Hebrew"},
                    { "hi", "Hindi"},
                    { "hmn", "Hmong"},
                    { "hu", "Hungarian"},
                    { "is", "Icelandic"},
                    { "ig", "Igbo"},
                    { "id", "Indonesian"},
                    { "ga", "Irish"},
                    { "it", "Italian"},
                    { "ja", "Japanese"},
                    { "jw", "Javanese"},
                    { "kn", "Kannada"},
                    { "kk", "Kazakh"},
                    { "km", "Khmer"},
                    { "ko", "Korean"},
                    { "ku", "Kurdish (Kurmanji)"},
                    { "ky", "Kyrgyz"},
                    { "lo", "Lao"},
                    { "la", "Latin"},
                    { "lv", "Latvian"},
                    { "lt", "Lithuanian"},
                    { "lb", "Luxembourgish"},
                    { "mk", "Macedonian"},
                    { "mg", "Malagasy"},
                    { "ms", "Malay"},
                    { "ml", "Malayalam"},
                    { "mt", "Maltese"},
                    { "mi", "Maori"},
                    { "mr", "Marathi"},
                    { "mn", "Mongolian"},
                    { "my", "Myanmar (Burmese)"},
                    { "ne", "Nepali"},
                    { "no", "Norwegian"},
                    { "ps", "Pashto"},
                    { "fa", "Persian"},
                    { "pl", "Polish"},
                    { "pt", "Portuguese"},
                    { "ma", "Punjabi"},
                    { "ro", "Romanian"},
                    { "ru", "Russian"},
                    { "sm", "Samoan"},
                    { "gd", "Scots Gaelic"},
                    { "sr", "Serbian"},
                    { "st", "Sesotho"},
                    { "sn", "Shona"},
                    { "sd", "Sindhi"},
                    { "si", "Sinhala"},
                    { "sk", "Slovak"},
                    { "sl", "Slovenian"},
                    { "so", "Somali"},
                    { "es", "Spanish"},
                    { "su", "Sundanese"},
                    { "sw", "Swahili"},
                    { "sv", "Swedish"},
                    { "tg", "Tajik"},
                    { "ta", "Tamil"},
                    { "te", "Telugu"},
                    { "th", "Thai"},
                    { "tr", "Turkish"},
                    { "uk", "Ukrainian"},
                    { "ur", "Urdu"},
                    { "uz", "Uzbek"},
                    { "vi", "Vietnamese"},
                    { "cy", "Welsh"},
                    { "xh", "Xhosa"},
                    { "yi", "Yiddish"},
                    { "yo", "Yoruba"},
                    { "zu", "Zulu" }
                };

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
}