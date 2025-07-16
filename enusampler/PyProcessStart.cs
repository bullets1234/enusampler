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
            Task.Run(() => ProcessInitialize());
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void ProcessInitialize()
        {
            var srcPath = @_enunupath;
            Directory.SetCurrentDirectory(Directory.GetParent(@srcPath).ToString());
            p = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    FileName = @_pythonpath,
                    UseShellExecute = false,
                    //WindowStyle = ProcessWindowStyle.Minimized,
                }

            };
        }

        public async Task<bool> EnunuStart(string ustpath, string tempWavPath,bool islegacy) {

            var srcPath = @_enunupath;
            

            // (function)def main(
            //    path_plugin: str,
            //    path_wav: str | None = None,
            //    play_wav: bool = True,
            //    lf0: Any | None = None
            //)->str
            if (islegacy)
            {
                p.StartInfo.Arguments = $@"{srcPath} {ustpath} {tempWavPath}";
            }
            else
            {
                //p.StartInfo.Arguments = $@"{srcPath} {ustpath} {tempWavPath}";
                p.StartInfo.Arguments = $@"{srcPath} --wav {tempWavPath} {ustpath} ";
            }

            p.Start();
            Console.WriteLine($"Enunu Start: {p.StartInfo.Arguments}");
            p.WaitForExit();


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
