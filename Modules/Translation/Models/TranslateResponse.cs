namespace Causym.Modules.Translation
{
    public class TranslateResponse
    {
        public TranslateResponse(Result responseResult, TranslationResult translateResult = null)
        {
            ResponseResult = responseResult;
            TranslateResult = translateResult;
        }

        public enum Result
        {
            Success,
            InvalidInputText,
            TranslationClientNotEnabled,
            TranslationError
        }

        public Result ResponseResult { get; set; }

        public TranslationResult TranslateResult { get; set; }

        public class TranslationResult
        {
            public string SourceLanguage { get; set; }

            public string DestinationLanguage { get; set; }

            public string SourceText { get; set; }

            public string TranslatedText { get; set; }
        }
    }
}