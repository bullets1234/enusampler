using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENUNU_Engine
{
    public class ResamplerParams(string _params, string _flag, string _env, string _stp, string _vel, string _temp, string _helper):IResamplerParams
    {
        //@set params=100 0 !160 /g/q/0//AJAUAeAnAwA3A9BCBFBGBGBEBAA7A1AtAkAbAQAG/7/x/n/d/V/O/I/D/B+//A/C/G/L/S/a/j/t/3ABAKARAXAcAeAgAgAfAcAZAVARANAIAFACAA#6#
        //@set env = 77.4 5 35 100 100 100 0 77.44174
        //@set stp = 0
        //@set vel = 110
        //@set temp = "%cachedir%\296_u+RD4_E4_mclT1O.wav"
        //@echo ########################################(224/226)
        //@call %helper% "%oto%\N_D4\_おうんあんんう.wav" E4 120@160+233.258 233.2583 3831.0 400 700.0 454.0 296
        public string Params { get; set; } = _params;
        public string Flag { get; set; } = _flag;
        public string Env { get; set; } = _env;
        public string Stp { get; set; } = _stp;
        public string Vel { get; set; } = _vel;
        public string Temp { get; set; } = _temp;
        public string Helper { get; set; } = _helper;

        //%1 %temp% %2 %vel% %flag% %5 %6 %7 %8 %params%
        //helper第一引数　temp cachedir helper第2引数 vel flag helper第5引数 helper第6引数 helper第7引数 params
        public async Task CallHifisampler(string contentString)
        {
            HttpClient client = new HttpClient();
            var content = new StringContent(contentString, Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                await client.PostAsync("http://127.0.0.1:8572", content);

            }
            catch (Exception ex)
            {

            }
        }

    }

    public interface IResamplerParams
    {
        string Params { get; set; }
        string Flag { get; set; }
        string Env { get; set; }
        string Stp { get; set; }
        string Vel { get; set; }
        string Temp { get; set; }
        string Helper { get; set; }
    }
}
