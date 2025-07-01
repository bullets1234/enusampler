using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using Microsoft.Extensions.Configuration;



namespace ENUNU_Engine;
class Program
{
    // 処理中タスクのキュー
    private static ConcurrentQueue<string[]> taskQueue = new ConcurrentQueue<string[]>();
    private static bool isProcessing = false;
    private static readonly string batName = "temp.bat";
    private static readonly string helperName = "temp_helper.bat";
    private static readonly string ustName = "temp$$$.ust";
    private static readonly string wavName = "temp.wav";
    private static bool TunedWavOut = false;
    private static string pythonEnvPath = string.Empty;
    private static bool ignoreEndPhoneme = true;
    private static readonly string[] ignoreEndPhonemeList = { "a R", "i R", "u R", "e R", "o R","n R"};
    private static readonly IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json")).Build();

    static async Task Main(string[] args)
    {
        //var pythonpath = configuration["Environment:PythonPath"];
        //バッチファイルをロードし、RenderConfigを作成する
#if DEBUG
        //デバッグ用
        var tempdatPath = Path.Combine(Directory.GetCurrentDirectory(), "temp.wav.dat");
        var tempwhdPath = Path.Combine(Directory.GetCurrentDirectory(), "temp.wav.whd");

        var batchFilePath = Path.Combine(Directory.GetCurrentDirectory(), batName);
        var ustFilePath = Path.Combine(Directory.GetCurrentDirectory(), ustName);
        var tempWavPath = Path.Combine(Directory.GetCurrentDirectory(), wavName);
        //バッチファイルが存在しているか確認
        if (!File.Exists(batchFilePath))
        {
            Console.WriteLine("Batch file not found.");
            Environment.Exit(1);
        }

        //バッチファイルが存在しているか確認
        if (!File.Exists(ustFilePath))
        {
            Console.WriteLine("Batch file not found.");
            Environment.Exit(1);
        }
        var file = LoadBatFile(batchFilePath);
        var ust = SeparateUst(ustFilePath);
#else

        var tempdatPath = Path.Combine(Directory.GetCurrentDirectory(), "temp.wav.dat");
        var tempwhdPath = Path.Combine(Directory.GetCurrentDirectory(), "temp.wav.whd");

        var batchFilePath = Path.Combine(Directory.GetCurrentDirectory(), batName);
        var ustFilePath = Path.Combine(Directory.GetCurrentDirectory(), ustName);
        var tempWavPath = Path.Combine(Directory.GetCurrentDirectory(), wavName);
        //バッチファイルが存在しているか確認
        if (!File.Exists(batchFilePath))
        {
            Console.WriteLine("Batch file not found.");
            Environment.Exit(1);
        }
        var file = LoadBatFile(batchFilePath);

        //バッチファイルが存在しているか確認
        if (!File.Exists(ustFilePath))
        {
            Console.WriteLine("temp ust file not found.");
            ustFilePath = file.Cachedir.Replace(".cache",".ust");
            if (!File.Exists(ustFilePath))
            {
                Console.WriteLine("ust file not found.");
                Environment.Exit(1);
            }
            //Environment.Exit(1);
        }
        var ust = SeparateUst(ustFilePath);
#endif
        //%1 %temp% %2 %vel% %flag% %5 %6 %7 %8 %params%
        //helper第一引数　temp cachedir helper第2引数 vel flag helper第5引数 helper第6引数 helper第7引数 helper第8引数 params

        //hifisampler
        //for (int i = 0; i < file.ResamplerParamList.Count; i++)
        //{

        //    ResamplerParams call = file.ResamplerParamList[i];
        //    var helperArray = call.Helper.Split(" ");
        //    var contentString = $"{helperArray[0].Replace("\"", "")},{call.Temp.Replace("\"", "")},{helperArray[1]},{call.Vel},{call.Flag},{helperArray[4]},{helperArray[5]},{helperArray[6]},{helperArray[7]},{call.Params}";
        //    contentString = contentString.Replace(",", " ");
        //    Console.WriteLine(contentString);
        //    await call.CallHifisampler(contentString);
        //}


        //simpleenunu
        var isStart = false;
        var StartNoteNum = 0;
        var noteNumOnUst = 0;
        for (int i = 0; i < file.ResamplerParamList.Count; i++)
        {

            ResamplerParams call = file.ResamplerParamList[i];
            var helperArray = call.Helper.Split(" ");
            if (helperArray.Length < 8)
            {
                continue;
            }

            if (helperArray.Length == 9)
            {
                isStart = true;
                StartNoteNum = i;
                noteNumOnUst = Convert.ToInt32(helperArray[8]);
                break;
            }
        }

        //初期ノート番号の計算の計算
        if (StartNoteNum != 0)
        {
            noteNumOnUst -= StartNoteNum;
        }

        //取り出しは2ずれる 0->version 1->setting
        //var ustForRender = ust[noteNumOnUst..(file.ResamplerParamList.Count - 1)];

        //for (int i = noteNumOnUst; i<file.ResamplerParamList.Count-1;i++)
        //{

        //}
        var ustForRender = new List<KeyValuePair<string, Dictionary<string, string>>>();
        foreach (var note in ust.Select((item, index) => new { item, index }))
        {
            if (note.index == 0 || note.index == 1)
            {
                //setting version
                ustForRender.Add(note.item);
                continue;
            }

            if (note.index < (noteNumOnUst + 2))
            {
                continue;
            }

            //Console.WriteLine(note.item.Key);
            ustForRender.Add(note.item);

            if (note.index >= (file.ResamplerParamList.Count + noteNumOnUst + 1))
            {
                break;
            }

        }
        Console.WriteLine($"選択ノート数:{ustForRender.Count}");

        //Console.WriteLine(testlist.Count);
        var lastTempFile = string.Empty;
        var isfirst = true;
        for (int i = 0; i < file.ResamplerParamList.Count; i++)
        {
            var path = file.ResamplerParamList[i].Temp.Replace("\"", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (isfirst)
                {
                    lastTempFile = path;
                    isfirst = false;
                    continue;
                }
                if (lastTempFile != string.Empty)
                {
                    //CreateFakeWav(path);
                    continue;
                }
            }
        }

        Console.WriteLine($"選択シンガー:{file.Oto}");
        var pyfilePath = configuration["Environment:ENUNU:Path"];

        if (string.IsNullOrEmpty(configuration["Environment:Python"]) || configuration["Environment:Python"] =="")
        {
            var pythonDir = Directory.GetDirectories(configuration["Environment:ENUNU:Path"], "*embed-amd64", SearchOption.TopDirectoryOnly);
            if (pythonDir.Length == 0)
            {
                Console.WriteLine("Error: python env dir not found");
                //Console.WriteLine("TunedWavOutはture,falseで設定してください。");
                Environment.Exit(1);
            }

            pythonEnvPath = Path.Combine(pythonDir[0], @"python.exe");
            if (!File.Exists(pythonEnvPath))
            {
                Console.WriteLine("Error: python.exe not found");
                //Console.WriteLine("TunedWavOutはture,falseで設定してください。");
                Environment.Exit(1);
            }
        }
        else
        {
            pythonEnvPath = configuration["Environment:Python"];
            Console.WriteLine(pythonEnvPath);
        }

        if (!Boolean.TryParse(configuration["Environment:ENUNU:TunedWavOut"],out TunedWavOut))
        {
            Console.WriteLine("Error:appsettings.json");
            Console.WriteLine("TunedWavOutはture,falseで設定してください。");
            Environment.Exit(1);
        }

        if(!TunedWavOut)
        {
            //TODO:pyファイルはリストを用意してその中に一致するものがあれば選択するようにする
            pyfilePath = Path.Combine(pyfilePath,"simple_enunu.py");
        }
        else
        {

            var tunedwavout = Directory.GetFiles(pyfilePath, "*TunedWavOut.py", SearchOption.TopDirectoryOnly);
            if(tunedwavout.Length == 0)
            {
                Console.WriteLine("Error: tuned wav out file not found");
                //Console.WriteLine("TunedWavOutはture,falseで設定してください。");
                Environment.Exit(1);
            }
            Console.WriteLine(tunedwavout[0]);
            pyfilePath = Path.Combine(pyfilePath, tunedwavout[0]);

        }


        if (!File.Exists(pyfilePath)){
            Console.WriteLine("Error: python file not found");
            //Console.WriteLine("TunedWavOutはture,falseで設定してください。");
            Environment.Exit(1);
        }


        var pyprocess = new PyProcessStart(pythonEnvPath, pyfilePath);
        var ustpath = CreateUst(file.Cachedir, ustForRender, file.Oto);
        var isBoolean = Boolean.TryParse(configuration["Environment:ENUNU:Legacy"], out bool legacy);
        if(!isBoolean)
        {
            Console.WriteLine("Error:appsettings.json");
            Console.WriteLine("Legacyはture,falseで設定してください。");
            Environment.Exit(1);
        }
        Console.WriteLine("EnunuStart");
        await pyprocess.EnunuStart(ustpath, tempWavPath, legacy);
        ModBatchFile(batchFilePath,file);

        pyprocess.EnunuClose();
        Environment.Exit(0);
    }

    static string CreateUst(string cacheDir, List<KeyValuePair<string, Dictionary<string, string>>> list,string Oto)
    {
        FileStream f = File.Create(Path.Combine(cacheDir, $"enu_temp.ust"));
        //tmpfilePath.Add(Path.Combine(cacheDir, $"enu_temp.ust"));
        f.Close();
        using FileStream fs = new(Path.Combine(cacheDir, $"enu_temp.ust"), FileMode.Open, FileAccess.ReadWrite);
        fs.SetLength(0);//中身を削除
        fs.Close();
        StringBuilder sb = new();
        foreach (var note in list)
        {
            sb.AppendLine(note.Key);
            foreach (var param in note.Value)
            {
                if (param.Key.StartsWith("version"))
                {
                    sb.AppendLine(param.Value);
                    continue;
                }

                if (param.Key.StartsWith("VoiceDir"))
                {
                    sb.AppendLine(param.Key + "=" + Oto);
                    continue;
                }

                //if (param.Key.StartsWith("VoiceDir") && param.Value.Split("\\").Last() != Oto.Split("\\").Last())
                //{
                //    sb.AppendLine(param.Key + "=" + Oto);
                //    continue;
                //}

                //if (param.Key.StartsWith("VoiceDir") && param.Value.Contains("%DATA%"))
                //{
                //    var utauDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UTAU");
                //    var voiceDir = param.Value.Replace("%DATA%", utauDir);
                //    sb.AppendLine(param.Key + "=" + voiceDir);
                //    continue;
                //}

                if (param.Key.StartsWith("Lyric") && ignoreEndPhoneme)
                {
                    if (ignoreEndPhonemeList.Any(pattern => param.Value.StartsWith(pattern)))
                    {
                        sb.AppendLine(param.Key + "=" + "R");
                        continue;
                    }
                }

                sb.AppendLine(param.Key + "=" + param.Value);
            }



        }

        EncodingProvider ep = CodePagesEncodingProvider.Instance;
        StreamWriter sw = new(Path.Combine(cacheDir, $"enu_temp.ust"), false, encoding: Encoding.GetEncoding("shift-jis"));
        sw.Write(sb.ToString());
        sw.Close();

        return Path.Combine(cacheDir, $"enu_temp.ust");
    }

    static void ModBatchFile(string _batFilePath,RenderConfig renderConfig)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        string[] lines = File.ReadAllLines(_batFilePath, Encoding.GetEncoding("Shift_JIS"));
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            // 空行やコメント行をスキップ


            if (i < 16)
            {
                continue;
            }

            if (line.StartsWith("copy"))
            {
                lines[i] = "copy /Y ./enu_temp.wav ./temp.wav";
                if(line.Contains("%output%"))
                {
                    lines[i] = $"copy /Y \"./temp.wav\" \"{renderConfig.Output}\"";
                }

                continue;
            }

            lines[i] = "";
        }

        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            sb.AppendLine(line);
        }
        //sb.Append(lines[0..].ToString());
        StreamWriter sw = new(_batFilePath, false, encoding: Encoding.GetEncoding("shift-jis"));
        sw.Write(sb);
        sw.Close();
    }

    static RenderConfig LoadBatFile(string _batFilePath)
    {
        // バッチファイルを読み込み
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        string[] lines = File.ReadAllLines(_batFilePath, Encoding.GetEncoding("Shift_JIS"));

        bool ismkdir = false;
        RenderConfig renderConfig = new RenderConfig();

        string param = string.Empty;
        string flag = string.Empty;
        string env = string.Empty;
        string stp = string.Empty;
        string vel = string.Empty;
        string temp = string.Empty;
        string helper = string.Empty;

        var list = new List<ResamplerParams>();


        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            // 空行やコメント行をスキップ
            if (string.IsNullOrEmpty(line) || line.StartsWith("@rem") || line.StartsWith("::") || line.StartsWith("REM") || line.StartsWith("@echo") || line.StartsWith("@del"))
                continue;

            if (line.StartsWith("@mkdir"))
            {
                ismkdir = true;
                continue;
            }

            if (ismkdir)//呼び出しパラメーター格納
            {
                if (!(line.StartsWith("@set")) && !(line.StartsWith("@call")) && !(line.StartsWith("@\"%tool%\"")))
                {
                    continue;
                }
                string[] environment = [];

                //@set params=100 0 !160 /g/q/0//AJAUAeAnAwA3A9BCBFBGBGBEBAA7A1AtAkAbAQAG/7/x/n/d/V/O/I/D/B+//A/C/G/L/S/a/j/t/3ABAKARAXAcAeAgAgAfAcAZAVARANAIAFACAA#6#
                //@set env = 77.4 5 35 100 100 100 0 77.44174
                //@set stp = 0
                //@set vel = 110
                //@set temp = "%cachedir%\296_u+RD4_E4_mclT1O.wav"
                //@echo ########################################(224/226)
                //@call %helper% "%oto%\N_D4\_おうんあんんう.wav" E4 120@160+233.258 233.2583 3831.0 400 700.0 454.0 296
                //@"%tool%" "%output%" "%oto%\R.wav" 0 16260@128 - 42.219 0 0

                if (line.StartsWith("@call"))
                {
                    environment = line.Substring(7).Split("% ", 2);
                }
                else if (line.StartsWith("@\"%tool%\""))
                {
                    environment = line.Substring(10).Split(" ", 2);
                }
                else
                {
                    environment = line.Substring(5).Split('=', 2);
                }
                if (environment.Length == 2)
                {

                    switch (environment[0])
                    {
                        case "params":
                            param = environment[1];
                            break;
                        case "flag":
                            flag = environment[1];
                            break;
                        case "env":
                            env = environment[1];
                            break;
                        case "stp":
                            stp = environment[1];
                            break;
                        case "vel":
                            vel = environment[1];
                            break;
                        case "temp":
                            temp = environment[1].Replace("%cachedir%", renderConfig.Cachedir); ;
                            break;
                        case "helper":
                            helper = environment[1].Replace("%oto%", renderConfig.Oto);
                            list.Add(new ResamplerParams(param, flag, env, stp, vel, temp, helper));
                            break;
                        default:
                            if (environment[0] == "\"%output%\"")
                            {
                                helper = environment[1];
                                list.Add(new ResamplerParams("", "", "", "", "", "", helper));
                                Debug.WriteLine("R is correct");
                            }
                            break;
                    }
                }
            }
            else
            {
                if (!line.StartsWith("@set"))
                {
                    continue;
                }
                string[] parts = line.Substring(5).Split('=', 2);
                if (parts.Length == 2)
                {
                    switch (parts[0])
                    {
                        case "loadmodule":
                            renderConfig.Loadmodule = parts[1];
                            break;
                        case "tempo":
                            renderConfig.Tempo = parts[1];
                            break;
                        case "samples":
                            renderConfig.Samples = parts[1];
                            break;
                        case "oto":
                            renderConfig.Oto = parts[1];
                            break;
                        case "tool":
                            renderConfig.Tool = parts[1];
                            break;
                        case "resamp":
                            renderConfig.Resamp = parts[1];
                            break;
                        case "output":
                            renderConfig.Output = parts[1];
                            break;
                        case "helper":
                            renderConfig.Helper = parts[1];
                            break;
                        case "cachedir":
                            renderConfig.Cachedir = parts[1];
                            break;
                        case "flag":
                            renderConfig.Flag = parts[1];
                            break;
                        case "env":
                            renderConfig.Env = parts[1];
                            break;
                        case "stp":
                            renderConfig.Stp = parts[1];
                            break;
                        default:
                            break;
                    }
                }

            }
        }
        renderConfig.ResamplerParamList = list;
        return renderConfig;
    }

    static Dictionary<string, Dictionary<string, string>> SeparateUst(string ust)
    {

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        string[] lines = File.ReadAllLines(ust, Encoding.GetEncoding("Shift_JIS"));
        var arraystr = lines;
        var split_ust = arraystr[0..(arraystr.Length - 1)];
        Dictionary<string, string> note = [];
        Dictionary<string, Dictionary<string, string>> notes = [];
        var currentNote = new List<string>();
        var currentSection = "[#VERSION]";//[#VERSION]が最初のセクション
        Debug.WriteLine(split_ust.Last());
        var hasNextSection = split_ust.Contains("[#NEXT]");
        //TODO:リファクタリング
        int counter = 0;
        foreach (var u in split_ust)
        {
            counter++;
            if (u.StartsWith("[#"))
            {
                currentNote.Add(u);
                if (currentSection != u)
                {
                    notes.Add(currentSection, note);
                }
                currentSection = u;
                note = [];
                continue;
            }
            if (!u.Contains("=") && currentSection == "[#VERSION]")
            {
                note.Add("version", u);
                continue;
            }

            if (u.Contains("="))
            {
                var keyvalue = u.Split("=");
                note.Add(keyvalue[0], keyvalue[1]);
                if (u == split_ust.Last().ToString() && currentSection == "[#NEXT]" || u == split_ust.Last().ToString() && counter >= split_ust.Length - 1)
                //if (u == split_ust.Last().ToString() && currentSection == "[#NEXT]")
                {
                    notes.Add(currentSection, note);
                }
            }

        }

        Console.WriteLine("ust読み込み完了");

        return notes;
    }

}




