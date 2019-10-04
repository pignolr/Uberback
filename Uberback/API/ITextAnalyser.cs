using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uberback.API
{
    interface ITextAnalyser
    {
        Task<Dictionary<string, string>> AnalyseTextAsync(string text);
    }
}
