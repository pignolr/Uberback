using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uberback.API
{
    interface ITranslator
    {
        Task<string> DetectLanguageAsync(string str);
        Task<string> TranslateTextAsync(string str, string language);
    }
}
