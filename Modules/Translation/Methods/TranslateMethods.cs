using System;
using System.Linq;
using System.Text.RegularExpressions;
using Disqord;
using Disqord.Rest;

namespace Causym.Modules.Translation
{
    /// <summary>
    /// Handles translation methods.
    /// </summary>
    public partial class TranslateService
    {
        public static string FixTranslatedString(string value)
        {
            var translationString = value;

            try
            {
                // Used to fix links that are created using the []() firmat.
                translationString = translationString.Replace("] (", "](");

                // Fixed user mentions
                var matchUser = Regex.Matches(translationString, @"(<@!?) (\d+)>");
                if (matchUser.Any())
                {
                    foreach (Match match in matchUser)
                    {
                        translationString = translationString.Replace(match.Value, $"{match.Groups[1].Value}{match.Groups[2].Value}>");
                    }
                }

                // Fixed role mentions
                var matchRole = Regex.Matches(translationString, @"<@ & (\d+)>");
                if (matchRole.Any())
                {
                    foreach (Match match in matchRole)
                    {
                        translationString = translationString.Replace(match.Value, $"<@&{match.Groups[1].Value}>");
                    }
                }

                // Fixed channel mentions
                var matchChannel = Regex.Matches(translationString, @"<# (\d+)>");
                if (matchChannel.Any())
                {
                    foreach (Match match in matchChannel)
                    {
                        translationString = translationString.Replace(match.Value, $"<#{match.Groups[1].Value}>");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return translationString;
        }

        public static LocalEmbedBuilder GetTranslationEmbed(TranslateResponse result, IUserMessage message)
        {
            if (result.TranslateResult == null)
            {
                return null;
            }

            var embed = new LocalEmbedBuilder().WithAuthor(message.Author);
            embed.AddField($"Original Message [{result.TranslateResult.SourceLanguage}]", result.TranslateResult.SourceText.FixLength());
            embed.AddField($"Translated Message [{result.TranslateResult.DestinationLanguage}]", result.TranslateResult.TranslatedText.FixLength());

            if (message is RestUserMessage restMsg)
            {
                embed.AddField("Yandex", $"[Powered by Yandex](http://translate.yandex.com/) [Original](https://discordapp.com/channels/{restMsg.GuildId?.RawValue.ToString() ?? "@me"}/{restMsg.ChannelId.RawValue}/{message.Id.RawValue}/)");
            }
            else if (message is CachedUserMessage cMsg)
            {
                var gid = cMsg.Guild == null ? "@me" : cMsg.Guild.Id.RawValue.ToString();
                embed.AddField("Yandex", $"[Powered by Yandex](http://translate.yandex.com/) [Original](https://discordapp.com/channels/{gid}/{cMsg.Channel.Id.RawValue}/{message.Id.RawValue}/)");
            }
            else
            {
                embed.AddField("Yandex", $"[Powered by Yandex](http://translate.yandex.com/)");
            }

            embed.Color = Color.Green;
            return embed;
        }

        public LocalEmbedBuilder TranslateEmbed(Embed embed, string code, IUserMessage message)
        {
            if (!embed.IsRich)
            {
                return null;
            }

            var builder = new LocalEmbedBuilder()
            {
                Timestamp = embed.Timestamp,
                Color = embed.Color
            };

            if (!string.IsNullOrWhiteSpace(embed.Title))
            {
                var titleResult = Translate(embed.Title, code);
                if (titleResult.ResponseResult == TranslateResponse.Result.Success)
                {
                    builder.Title = titleResult.TranslateResult.TranslatedText.FixLength(100);
                }
            }

            if (embed.Author != null)
            {
                // There should be no need to translate the author field.
                builder.Author = new Disqord.LocalEmbedAuthorBuilder()
                {
                    IconUrl = embed.Author.IconUrl,
                    Name = embed.Author.Name,
                    Url = embed.Author.Url
                };
            }

            if (embed.Footer != null)
            {
                if (!string.IsNullOrWhiteSpace(embed.Footer.Text))
                {
                    var footerTextResult = Translate(embed.Footer.Text, code);
                    if (footerTextResult.ResponseResult == TranslateResponse.Result.Success)
                    {
                        builder.Footer = new Disqord.LocalEmbedFooterBuilder()
                        {
                            IconUrl = embed.Footer.IconUrl,
                            Text = footerTextResult.TranslateResult.TranslatedText.FixLength(250)
                        };
                    }
                }
                else
                {
                    builder.Footer = new Disqord.LocalEmbedFooterBuilder()
                    {
                        IconUrl = embed.Footer.IconUrl
                    };
                }
            }

            if (!string.IsNullOrWhiteSpace(embed.Description))
            {
                var description = Translate(embed.Description, code);
                if (description.ResponseResult == TranslateResponse.Result.Success)
                {
                    builder.Description = description.TranslateResult.TranslatedText.FixLength(2047);
                }
            }

            foreach (var field in embed.Fields)
            {
                if (string.IsNullOrWhiteSpace(field.Name))
                {
                    continue;
                }

                var nameResult = Translate(field.Name, code);
                if (nameResult.ResponseResult != TranslateResponse.Result.Success || string.IsNullOrWhiteSpace(field.Value))
                {
                    continue;
                }

                var contentResult = Translate(field.Value, code);
                if (contentResult.ResponseResult != TranslateResponse.Result.Success)
                {
                    continue;
                }

                var newField = new Disqord.LocalEmbedFieldBuilder()
                {
                    Name = nameResult.TranslateResult.TranslatedText,
                    Value = contentResult.TranslateResult.TranslatedText,
                    IsInline = field.IsInline
                };
                builder.Fields.Add(newField);
            }

            if (builder.Fields.Count < 25 && builder.Length < 5900)
            {
                if (message is RestUserMessage restMsg)
                {
                    builder.AddField("Yandex", $"[Powered by Yandex](http://translate.yandex.com/) [Original](https://discordapp.com/channels/{restMsg.GuildId?.RawValue.ToString() ?? "@me"}/{restMsg.ChannelId.RawValue}/{message.Id.RawValue}/)");
                }
                else if (message is CachedUserMessage cMsg)
                {
                    var gid = cMsg.Guild == null ? "@me" : cMsg.Guild.Id.RawValue.ToString();
                    builder.AddField("Yandex", $"[Powered by Yandex](http://translate.yandex.com/) [Original](https://discordapp.com/channels/{gid}/{cMsg.Channel.Id.RawValue}/{message.Id.RawValue}/)");
                }
                else
                {
                    builder.AddField("Yandex", $"[Powered by Yandex](http://translate.yandex.com/)");
                }
            }

            return builder;
        }

        public TranslateResponse Translate(string inputText, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                return new TranslateResponse(TranslateResponse.Result.InvalidInputText);
            }

            try
            {
                var response = TranslateText(inputText, languageCode);
                if (response != null)
                {
                    return new TranslateResponse(TranslateResponse.Result.Success, response);
                }

                return new TranslateResponse(TranslateResponse.Result.TranslationClientNotEnabled);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString(), "TRANSLATE", Logger.LogLevel.Error);
            }

            return new TranslateResponse(TranslateResponse.Result.TranslationError);
        }

        private TranslationResult TranslateText(string inputText, string languageCode)
        {
            try
            {
                var result = translator.TranslateText(inputText, languageCode);
                if (result != null)
                {
                    return result;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString(), "TRANSLATE", Logger.LogLevel.Error);
                return null;
            }
        }
    }
}