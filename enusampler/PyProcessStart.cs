using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace ENUNU_Engine
{
    internal class PyProcessStart:IDisposable
    {
        private Process p;
        private string _pythonpath;
        private string _enunupath;

        public PyProcessStart(string pythonpath , string enunupath)
        {
            _pythonpath = pythonpath;
            _enunupath = enunupath;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> EnunuStart(string ustpath, string tempWavPath) {
            await Task.Run(() => {
                //var current = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();

                //var srcPath = @"G:\SimpleEnunu-0.4.0_PitchTune\simple_enunu040.py";
                var srcPath = @_enunupath;
                Directory.SetCurrentDirectory(Directory.GetParent(@srcPath).ToString());
                //var srcPath = System.IO.Path.Combine(Environment.CurrentDirectory,@"\SimpleEnunu-0.3.1\emb-python\src\simple_enunu.py");
                p = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        FileName = @_pythonpath,
                        UseShellExecute = false,
                        //WindowStyle = ProcessWindowStyle.Minimized,
            }

                };

                // (function)def main(
                //    path_plugin: str,
                //    path_wav: str | None = None,
                //    play_wav: bool = True,
                //    lf0: Any | None = None
                //)->str
                p.StartInfo.Arguments = $@"{srcPath} {ustpath} {tempWavPath}";// {_path} {_path.Replace(".ust", ".wav")}
                p.Start();
                p.WaitForExit();
            });

            return true;
        }

        public void EnunuClose()
        {
            try
            {
                p.Kill();

            }
            catch(Exception e)
            {

            }
        }
    }
}
