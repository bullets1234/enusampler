using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENUNU_Engine
{
    public class RenderConfig
    {
        public string Loadmodule { get; set; } = string.Empty;
        public string Tempo { get; set; } = string.Empty;
        public string Samples { get; set; } = string.Empty;
        public string Oto { get; set; } = string.Empty;
        public string Tool { get; set; } = string.Empty;
        public string Resamp { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;
        public string Helper { get; set; } = string.Empty;
        public string Cachedir { get; set; } = string.Empty;
        public string Flag { get; set; } = string.Empty;
        public string Env { get; set; } = string.Empty;
        public string Stp { get; set; } = string.Empty;

        public List<ResamplerParams> ResamplerParamList { get; set; } = default!;

    }
}
