using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;

namespace MangaKeeper
{
    class Program
    {
        public static string TName = "";
        public static string Url = "";
        public static string TPath = "";
        public static string MServer = "";
        public static int StrCount = 0;
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Title = "MangaSammler 2wei";
            Console.WriteLine("MangaSammler by Rimfaxe");
            Console.WriteLine("v0.2 (25 Jul 2015)");
            Console.WriteLine("");//http://readmanga.me/
            Url = File.ReadAllText(Environment.CurrentDirectory + @"\input.txt");
            TName = Url.Replace("://readmanga.me/", "").Replace("://adultmanga.ru/", "").Replace("https", "").Replace("http", "");
            Console.WriteLine("Ввести краткое название (используется при наименовании папок с главами)");
            Console.Write(">: ");
            TName = Console.ReadLine().Replace(" ","_");
            string dirname = TName;
            if (dirname.Length < 1) dirname = "namenlos";
            TPath = Environment.CurrentDirectory + @"\" + dirname;
            try
            {
                Directory.Delete(TPath, true);
            }
            catch { }
            Directory.CreateDirectory(TPath);
            Regex regex = new Regex("<option value=.+<\\/option\\>");
            string inhtml = "";
            //WebRequest req = WebRequest.Create(url + "/vol1/1");
            //WebResponse resp = req.GetResponse();
            if (Url.Contains("readmanga")) MServer = "http://readmanga.me";
            if (Url.Contains("adultmanga")) MServer = "http://adultmanga.ru";
            WebClient Client = new WebClient();
            Stream stream = Client.OpenRead(Url + "/vol1/1");
            StreamReader sr = new StreamReader(stream);
            inhtml = sr.ReadToEnd();
            sr.Close();
            Match match = regex.Match(inhtml);
            while (match.Success)
            {
                
                        Download(match.Value);
                        
                match = match.NextMatch();
            }


            Console.ReadLine();
        }
        public static string Volume(string input)
        {
            Regex regex = new Regex(@"/vol\d+");
            Match match = regex.Match(input);
            string vol = "";
            while (match.Success)
            {
                vol = match.Value.Replace("/vol", "");
                break;
            }
            if (Convert.ToInt32(vol) > 0 && Convert.ToInt32(vol) < 10) vol = "0" + vol;
            Directory.CreateDirectory(TPath+@"\vol "+vol);
            return vol;
        }
        public static string Chapter(string input)
        {
            Regex regex = new Regex(@"/vol\d+");
            Regex regex2 = new Regex(@"/vol\d+/\d+");
            Match match = regex.Match(input);
            Match match2 = regex2.Match(input);
            string vol = "";
            string cha = "";
            while (match.Success)
            {
                vol = match.Value;
                break;
            }
            while (match2.Success)
            {
                cha = match2.Value.Replace(vol, "").Replace("/","");
                break;
            }
            if (Convert.ToInt32(cha) > 0 && Convert.ToInt32(cha) < 10) cha = "00" + cha;
            if (Convert.ToInt32(cha) > 9 && Convert.ToInt32(cha) < 100) cha = "0" + cha;
            return cha;
        }
        public static void Download(string option)
        {
            Regex regex = new Regex("\".+?\"");
            Regex regex2 = new Regex(">.+?<");
            Match match = regex.Match(option);
            Match match2 = regex2.Match(option);
            string opt = "";
            while (match.Success)
            {
                opt = match.Value.Replace("\"", "");
                break;
            }
            string vol = Volume(opt);
            string cha = Chapter(opt);
            string numb = "";
            while (match2.Success)
            {
                numb = match2.Value.Replace(">", "").Replace("<", "");
                break;
            }
            string extra = "";
            if (numb.Contains("Экстра")) extra = "(extra)_";
            WebClient Client = new WebClient();
            Stream stream = Client.OpenRead(MServer + opt);
            StreamReader sr = new StreamReader(stream);
            string inhtml = sr.ReadToEnd();
            sr.Close();
            Regex regex3 = new Regex(@"\{url\:.+?\}");
            Regex regex4 = new Regex("http.+?\"");
            Match match3 = regex3.Match(inhtml);
            int i = 0;
            //class="person-link">
            Regex regex6 = new Regex("class=\"person-link\">.+?<");
            Match match6 = regex6.Match(inhtml);
            string transl = " ";
            while (match6.Success)
            {
                transl = match6.Value.Replace("class=\"person-link\">", "").Replace("<", "");
                break;
            }
            string chaname = TName+" v"+vol+" ch"+cha+" "+extra+"["+transl+"]";
            Directory.CreateDirectory(TPath + @"\" + (chaname).Replace(" ", "_"));
            while (match3.Success)
            {
                i++;
                Match match4 = regex4.Match(match3.Value);
                while (match4.Success)
                {
                    using (WebClient client = new WebClient())
                    {
                        string ex = "";
                        string qwe = match4.Value.Substring(match4.Value.Length - 5);
                        if (qwe.Contains(".jpg")) ex = "jpg";
                        if (qwe.Contains(".JPG")) ex = "jpg";
                        if (qwe.Contains(".jpeg")) ex = "jpeg";
                        if (qwe.Contains(".png")) ex = "png";
                        if (!qwe.Contains(".jpg") && !qwe.Contains(".png") && !qwe.Contains(".JPG") && !qwe.Contains(".jpeg")) Console.WriteLine("Ошибка в изображении " + i.ToString() + " в главе " + numb);
                        client.DownloadFile(match4.Value.Replace("\"", ""), TPath + @"\" + (chaname).Replace(" ", "_") + @"\"+"v" + vol + "c" + cha+"_" + Nint(i) + "." + ex);
                    }
                    break;
                }
                match3 = match3.NextMatch();
            }
            Console.WriteLine("Скачано " + numb + " [" + transl + "]");
            FileInfo someTextFileInfo = new FileInfo(Environment.CurrentDirectory + @"\" + TName + ".txt");
            string someTextFileContentStr = "";
            try
            {
                StreamReader someTextFileReader = new StreamReader(someTextFileInfo.OpenRead(), Encoding.GetEncoding("windows-1251"));
              
                while (someTextFileReader.Peek() != -1)
                {
                    someTextFileContentStr = someTextFileReader.ReadToEnd();
                }
                someTextFileReader.Close();
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Environment.CurrentDirectory + @"\" + TName + ".txt", true);
                writer.WriteLine(" ");
                writer.Close();
            }
            

            StreamWriter someTextFileStreamWriter = new StreamWriter(Environment.CurrentDirectory + @"\" + TName + ".txt", false, Encoding.GetEncoding("windows-1251"));
            someTextFileStreamWriter.WriteLine(numb);
            someTextFileStreamWriter.WriteLine(someTextFileContentStr);

            //someTextFileStreamWriter.Flush();
            someTextFileStreamWriter.Close();
           // StrCount--;


    }
        public static string Nint(int num)
        {
            string nint = "";
            if (num > 0 && num < 10) nint = "00" + num.ToString();
            if (num > 9 && num < 100) nint = "0" + num.ToString();
            if (num > 99) nint = num.ToString();
            return nint;
        }
    }
}
