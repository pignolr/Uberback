using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using System.IO;

namespace Uberback.API
{
    class PerspectiveTextAnalyser : ITextAnalyser
    {
        private static readonly string[] AllowedLanguage = new string[] {
            "en"
            //, "fr", "es", "de"
        };
        private static readonly Tuple<string, float>[] Categories = new Tuple<string, float>[] {
            new Tuple<string, float>("TOXICITY", .80f),
            new Tuple<string, float>("SEVERE_TOXICITY", .60f),
            new Tuple<string, float>("IDENTITY_ATTACK", .60f),
            new Tuple<string, float>("INSULT", .60f),
            new Tuple<string, float>("PROFANITY", .80f),
            new Tuple<string, float>("THREAT", .60f),
            new Tuple<string, float>("INFLAMMATORY", .60f),
            new Tuple<string, float>("OBSCENE", .9f)
        };
        private readonly string PerspectiveApiToken;
        private readonly string PerspectiveApiUrl;
        private readonly ITranslator Translator;

        public PerspectiveTextAnalyser(string perspectiveAPITokenFile, ITranslator translator)
        {
            PerspectiveApiToken = File.ReadAllText(perspectiveAPITokenFile); ;
            PerspectiveApiUrl = "https://commentanalyzer.googleapis.com/v1alpha1";

            Translator = translator;
        }

        public async Task<Dictionary<string, string>> AnalyseTextAsync(string text)
        {
            // item1: translatedText, item2: detectedLanguage
            var translationResult = await TranslateTextIfNecessaryAsync(text);
            var jsonResponse = await AnalyseTextWithApiAsync(translationResult.Item1, translationResult.Item2);
            return GetTrigeredFlags(jsonResponse);
        }

        private Dictionary<string, string> GetTrigeredFlags(dynamic jsonResponse)
        {
            var flags = new Dictionary<string, string>();

            foreach (var s in Categories)
            {
                double value = jsonResponse.attributeScores[s.Item1].summaryScore.value;
                if (value >= s.Item2)
                    flags[s.Item1] = value.ToString("0.00");
            }
            if (flags.Count == 0)
                flags["SAFE"] = "1.00";
            return flags;
        }

        private async Task<dynamic> AnalyseTextWithApiAsync(string text, string language)
        {
            var httpClient = new HttpClient();
            var perspectiveAPIUrlText = PerspectiveApiUrl + "/comments:analyze?key=" + PerspectiveApiToken;

            var jsonContent = "{comment: {text: \"" + @text + "\"},"
                + "languages: [\"" + @language + "\"],"
                + "requestedAttributes: {"
                + string.Join(":{}, ", Categories.Select(x => x.Item1))
                + ":{}} }";
            var jsonRequest = JsonConvert.DeserializeObject(jsonContent).ToString();
            var request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(perspectiveAPIUrlText, request);

            dynamic jsonResponse = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            return jsonResponse;
        }

        private async Task<Tuple<string, string>> TranslateTextIfNecessaryAsync(string text)
        {
            var detectedLanguage = await Translator.DetectLanguageAsync(text);
            if (!AllowedLanguage.Contains(detectedLanguage))
                return new Tuple<string, string>(await Translator.TranslateTextAsync(text, AllowedLanguage[0]), "en");
            else
                return new Tuple<string, string>(text, detectedLanguage);
        }
    }
}
