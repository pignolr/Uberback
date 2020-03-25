using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Globalization;

namespace Uberback.API
{
    class PerspectiveTextAnalyser : ITextAnalyser
    {
        private static string[] AllowedLanguage;
        private static Dictionary<string, List<Tuple<string, float>>> Categories;
        private readonly string PerspectiveApiToken;
        private readonly string PerspectiveApiUrl;
        private readonly ITranslator Translator;
        private PerspectiveTextAnalyserConfig Config;

        public PerspectiveTextAnalyser(string perspectiveAPITokenFile, string configFileName, ITranslator translator)
        {
            PerspectiveApiToken = File.ReadAllText(perspectiveAPITokenFile);
            PerspectiveApiUrl = "https://commentanalyzer.googleapis.com/v1alpha1";

            Translator = translator;

            Config = new PerspectiveTextAnalyserConfig(configFileName);
            Categories = Config.Categories;
            AllowedLanguage = Config.AllowedLanguage;
        }

        public async Task<Dictionary<string, string>> AnalyseTextAsync(string text)
        {
            // item1: translatedText, item2: detectedLanguage
            var translationResult = await TranslateTextIfNecessaryAsync(text);
            var jsonResponse = await AnalyseTextWithApiAsync(translationResult.Item1, translationResult.Item2);
            return GetTrigeredFlags(jsonResponse, translationResult.Item2);
        }

        private Dictionary<string, string> GetTrigeredFlags(dynamic jsonResponse, string language)
        {
            var flags = new Dictionary<string, string>();

            foreach (var s in Categories[language])
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
                + string.Join(":{}, ", Categories[language].Select(x => x.Item1))
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
                return new Tuple<string, string>(await Translator.TranslateTextAsync(text, AllowedLanguage[0]), AllowedLanguage[0]);
            else
                return new Tuple<string, string>(text, detectedLanguage);
        }
    }
}
