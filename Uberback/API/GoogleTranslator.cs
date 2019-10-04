using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Cloud.Translation.V2;
using System.Threading.Tasks;

namespace Uberback.API
{
    class GoogleTranslator : ITranslator
    {
        private readonly TranslationClient TranslationClient;

        public GoogleTranslator()
        {
            TranslationClient = TranslationClient.Create();
        }

        public async Task<string> DetectLanguageAsync(string str)
        {
            var language = await TranslationClient.DetectLanguageAsync(str);
            return language.Language;
        }

        public async Task<string> TranslateTextAsync(string str, string language)
        {
            var translatedText = await TranslationClient.TranslateTextAsync(str, language);
            return translatedText.TranslatedText;
        }
    }
}
