using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace GitBisect
{
    internal class Program
    {
        static Process cmd = new Process();
        static void Main(string[] args)
        {
            string path = "C:\\Users\\volkov\\source\\repos\\BinarySearch";
            string start = "0e670e12dc53f4bfae7f0de097d28ef06721e4b8";
            string end= "83a0c969fec0d618fb09e96c86476ac83c39b4e1";
            string testFile = "BinarySearch\\Source.c";
            string result = Solution(path, start, end, testFile);
            Console.WriteLine(result);
            Console.ReadLine();
        }
        static string Solution(string path, string start, string end, string testFile)
        {
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            cmd.StandardInput.WriteLine($"cd {path}");
            cmd.StandardInput.Flush();
            string fileName = "commits.txt";
            cmd.StandardInput.WriteLine($"git log --format=\"%H\" > {fileName}");
            cmd.StandardInput.Flush();
            Thread.Sleep(100);
            StreamReader sr = new StreamReader($"{path}\\{fileName}", Encoding.Default);
            List<string> commits = new List<string>(sr.ReadToEnd().Split('\n').Reverse());
            sr.Close();
            commits.RemoveAll(x => x == "");
            cmd.StandardInput.WriteLine($"del {fileName}");
            cmd.StandardInput.Flush();
            int startIndex = commits.IndexOf(start);
            int endIndex = commits.IndexOf(end);
            string result = BinarySearch(commits, startIndex, endIndex, path, testFile);

            cmd.StandardInput.WriteLine("git checkout origin");
            cmd.StandardInput.Flush();

            cmd.StandardInput.Close();
            cmd.WaitForExit();
#if DEBUG
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
#endif
            return result;
        }
        static string BinarySearch(List<string> commits, int start, int end, string path, string testFile)
        {
            int c = (start + end) / 2;
            if (CheckCommit(path, testFile, commits[c]))
                if (CheckCommit(path, testFile, commits[c + 1]))
                    return BinarySearch(commits, c + 1, end, path, testFile);
                else
                    return commits[c];
            else
                return BinarySearch(commits, start, c - 1, path, testFile);
        }
        static bool CheckCommit(string path, string testFile, string commit)
        {
            try
            {
                cmd.StandardInput.WriteLine($"git checkout {commit}");
                cmd.StandardInput.Flush();
                string fileResult = "result.txt";
                cmd.StandardInput.WriteLine($"gcc {testFile}");
                cmd.StandardInput.Flush();
                cmd.StandardInput.WriteLine($"a.exe > {fileResult}");
                cmd.StandardInput.Flush();
                Thread.Sleep(100);
                StreamReader sr = new StreamReader($"{path}\\{fileResult}", Encoding.Default);
                bool result = sr.ReadLine() == "Tests passed";
                cmd.StandardInput.WriteLine($"del {fileResult}");
                cmd.StandardInput.Flush();
                sr.Close();
                return result;
            }
            catch { return false; }
        }
    }
}
