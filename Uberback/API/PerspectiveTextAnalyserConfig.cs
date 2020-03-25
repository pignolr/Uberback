using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Uberback.API
{
    class PerspectiveTextAnalyserConfig
    {
        public string ConfigFileName { get; }
        public string[] AllowedLanguage { get; set; }
        public Dictionary<string, List<Tuple<string, float>>> Categories { get; set; }

        private XmlDocument XmlConfigFile = new XmlDocument();

        public PerspectiveTextAnalyserConfig(string configFileName)
        {
            ConfigFileName = configFileName;
            try
            {
                XmlConfigFile.Load(ConfigFileName);
            }
            catch (Exception e) when (e is DirectoryNotFoundException || e is FileNotFoundException)
            {
                CreateDefaultConfigFile();
                return;
            }
            try
            {
                LoadConfigFromXml();
            }
            catch
            {
                LoadDefaultConfig();
            }
        }

        private void CreateDefaultConfigFile()
        {
            LoadDefaultConfig();
            CreateXmlDocumentFromConfig();

            if (File.Exists(ConfigFileName)) {
                File.Delete(ConfigFileName);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigFileName));
            XmlConfigFile.Save(ConfigFileName);
        }

        private void CreateXmlDocumentFromConfig()
        {
            XmlConfigFile = new XmlDocument();

            XmlDeclaration xmlDeclaration = XmlConfigFile.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = XmlConfigFile.DocumentElement;
            XmlConfigFile.InsertBefore(xmlDeclaration, root);

            XmlElement configNode = XmlConfigFile.CreateElement(string.Empty, "config", string.Empty);
            XmlConfigFile.AppendChild(configNode);

            XmlElement languagesNode = XmlConfigFile.CreateElement(string.Empty, "languages", string.Empty);
            configNode.AppendChild(languagesNode);

            foreach (var languageConfig in Categories)
            {
                XmlElement languageNode = XmlConfigFile.CreateElement(string.Empty, "language", string.Empty);
                languageNode.SetAttribute("name", languageConfig.Key);

                foreach (var category in languageConfig.Value)
                {
                    XmlElement categoryNode = XmlConfigFile.CreateElement(string.Empty, "category", string.Empty);
                    categoryNode.SetAttribute("name", category.Item1);
                    categoryNode.SetAttribute("value", category.Item2.ToString(CultureInfo.InvariantCulture.NumberFormat));
                    languageNode.AppendChild(categoryNode);
                }
                languagesNode.AppendChild(languageNode);
            }
        }

        private void LoadDefaultConfig()
        {
            AllowedLanguage = new string[] {
                "en",
                "fr",
                "es",
            };
            Categories = new Dictionary<string, List<Tuple<string, float>>> {
                {
                    "en",
                    new List<Tuple<string, float>> {
                        new Tuple<string, float>("TOXICITY", .80f),
                        new Tuple<string, float>("SEVERE_TOXICITY", .60f),
                        new Tuple<string, float>("IDENTITY_ATTACK", .60f),
                        new Tuple<string, float>("INSULT", .60f),
                        new Tuple<string, float>("PROFANITY", .80f),
                        new Tuple<string, float>("THREAT", .60f),
                        new Tuple<string, float>("INFLAMMATORY", .60f),
                        new Tuple<string, float>("OBSCENE", .9f)
                    }
                },
                {
                    "fr",
                    new List<Tuple<string, float>> {
                        new Tuple<string, float>("TOXICITY", .80f),
                        new Tuple<string, float>("SEVERE_TOXICITY", .60f),
                    }
                },
                {
                    "es",
                    new List<Tuple<string, float>> {
                        new Tuple<string, float>("TOXICITY", .80f),
                        new Tuple<string, float>("SEVERE_TOXICITY", .60f),
                    }
                }
            };
        }

        private void LoadConfigFromXml()
        {
            var allowedLanguage = new List<string>();
            var categories = new Dictionary<string, List<Tuple<string, float>>>();
            XmlNode ConfigLanguagesNode =
                XmlConfigFile.DocumentElement.SelectSingleNode("/config/languages");

            foreach (XmlNode languageNode in ConfigLanguagesNode.ChildNodes)
            {
                if (languageNode.Name != "language")
                    continue;
                var language = languageNode.Attributes["name"].InnerText;
                var categoriesLanguage = new List<Tuple<string, float>>();
                foreach (XmlNode categoryNode in languageNode.ChildNodes)
                {
                    if (categoryNode.Name != "category")
                        continue;
                    categoriesLanguage.Add(new Tuple<string, float>(
                        categoryNode.Attributes["name"].InnerText,
                        float.Parse(
                            categoryNode.Attributes["value"].InnerText,
                            CultureInfo.InvariantCulture.NumberFormat))
                        );
                }
                allowedLanguage.Add(language);
                categories.Add(language, categoriesLanguage);
            }
            Categories = categories;
            AllowedLanguage = allowedLanguage.ToArray();
        }
    }
}
