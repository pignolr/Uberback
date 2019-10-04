using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uberback.API
{
    interface IImageAnalyser
    {
        Task<Dictionary<string, string>> AnalyseImageUrlAsync(string url);
    }
}
