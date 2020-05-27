using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using File = System.IO.File;

namespace ScreenTimeCounter
{
    internal class ScreenTimeCounter
    {
        public Timer Timer { get; private set; }

        private static readonly string filePathFolder = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\ScreenTimeCounter";

        private string today = DateTime.Today.ToString("MM-dd-yyyy");

        private string fileName;

        private long rawTime;

        private ScreenTimeCounter() => fileName = $@"{filePathFolder}\{today}.txt";

        private static void Main(string[] args)
        {
            bool noArgs = args.Length == 0;
            if (Process.GetProcessesByName("ScreenTimeCounter").Length != 1 && noArgs)
            {
                Environment.Exit(-1);
            }
            if (noArgs)
            {
                new ScreenTimeCounter().StartCounter();
            }
            else
            {
                switch (args[0])
                {
                    case "-total":
                        ShowScreenTime(Directory.GetFiles(filePathFolder).ToList());
                        break;
                    case "-day":
                        ShowScreenTime(GetFilesWithFilter(TimeFilter.Day));
                        break;
                    case "-week":
                        ShowScreenTime(GetFilesWithFilter(TimeFilter.Week));
                        break;
                    case "-month":
                        ShowScreenTime(GetFilesWithFilter(TimeFilter.Month));
                        break;
                    case "-year":
                        ShowScreenTime(GetFilesWithFilter(TimeFilter.Year));
                        break;
                    case "-count":
                        ShowScreenTime(GetFilesWithFilter(-(int.Parse(args[1]) - 1), true));
                        break;
                    case "-range":
                        ShowScreenTime(GetFilesWithFilter(args[1].Split("~")));
                        break;
                    case "-date":
                        ShowScreenTime(GetFilesWithFilter(args[1]));
                        break;
                    case "-removeCorrupted":
                        Console.WriteLine($"{RemoveCorruptedFiles()} corrupted files were found and cleaned!");
                        break;
                    default:
                        throw new ArgumentException("This is not a valid argument.");
                }
            }
        }

        private static bool MatchesFilter(string file, int timeFilter) => DateTime.Parse(new FileInfo(file).CreationTime.Date.ToShortDateString()) < DateTime.Parse(DateTime.Now.Date.AddDays(timeFilter).ToShortDateString());

        private static List<string> GetFilesWithFilter(TimeFilter timeFilter) => GetFilesWithFilter((int)timeFilter);

        private static List<string> GetFilesWithFilter(int timeFilter, bool checkForInput = false)
        {
            List<string> fileList = Directory.GetFiles(filePathFolder).ToList();
            if (checkForInput)
            {
                if ((timeFilter * -1) > fileList.Count - 1)
                {
                    throw new IndexOutOfRangeException($"Maximum count is \"{fileList.Count}\".");
                }
            }
            fileList.RemoveAll(x => MatchesFilter(x, timeFilter));
            return fileList;
        }

        private static string GetFilesWithFilter(string date)
        {
            List<string> fileList = Directory.GetFiles(filePathFolder).ToList();
            string file = fileList.FirstOrDefault(x => x.Split(@"\")[6].Replace(".txt", string.Empty) == date);
            return file;
        }

        private static List<string> GetFilesWithFilter(string[] date)
        {
            List<string> fileList = Directory.GetFiles(filePathFolder).ToList();
            string big;
            string small;
            int comparaison = date[0].CompareTo(date[1]);
            if (comparaison > 0)
            {
                big = date[0];
                small = date[1];
            }
            else
            {
                small = date[0];
                big = date[1];
            }
            int index = fileList.IndexOf(fileList.FirstOrDefault(x => x.Contains(small)));
            fileList.RemoveAll(x => fileList.IndexOf(x) < index);
            fileList.RemoveAll(x => (x.Split(@"\")[6].Replace(".txt", string.Empty)).CompareTo(big) > 0);
            return fileList;
        }

        private static void ShowScreenTime(List<string> files)
        {
            int count = files.Count;
            long rawTime = 0;
            foreach (string file in files)
            {
                string[] readInfos = File.ReadAllLines(file);
                if (readInfos.Length != 2)
                {
                    count -= 1;
                    continue;
                }
                rawTime += long.Parse(readInfos[0].Split(":")[1]);
            }
            string time = $"Screen Time for the last {count} days: {new TimeSpanExtension().FromSeconds(rawTime)}";
            Console.WriteLine(time);
            Console.ReadLine();
        }

        private static void ShowScreenTime(string file)
        {
            if (file is null)
            {
                Console.WriteLine("No file with such a date.");
                Console.ReadLine();
                return;
            }
            long rawTime = 0;
            string[] readInfos = File.ReadAllLines(file);
            if (readInfos.Length != 2)
            {
                return;
            }
            rawTime += long.Parse(readInfos[0].Split(":")[1]);
            string time = $"Screen Time: {new TimeSpanExtension().FromSeconds(rawTime)}";
            Console.WriteLine(time);
            Console.ReadLine();
        }

        private static int RemoveCorruptedFiles()
        {
            int count = 0;
            string[] files = Directory.GetFiles(filePathFolder);
            for (int i = 0; i < files.Length; i++)
            {
                if (new FileInfo(files[i]).Length == 0)
                {
                    File.Delete(files[i]);
                    count++;
                }
            }
            return count;
        }

        private void StartCounter()
        {
            Console.WriteLine("Running . . .");
            Timer = new Timer()
            {
                Interval = 1000,
                AutoReset = true
            };
            Timer.Elapsed += Capture;
            Timer.Start();
            new IdleHandler(this);
            Console.ReadLine();
        }

        public void Capture(object sender, ElapsedEventArgs e)
        {
            string privateToday = DateTime.Today.ToString("MM-dd-yyyy");
            if (today != privateToday)
            {
                today = privateToday;
                string newFileName = $@"{filePathFolder}\{today}.txt";
                fileName = newFileName;
                rawTime = 0;
            }
            if (File.Exists(fileName))
            {
                string[] readInfos = File.ReadAllLines(fileName);
                rawTime = readInfos.Length == 2 ? long.Parse(readInfos[0].Split(":")[1]) : 0;
            }
            string timeRaw = $"Raw Time (seconds): {rawTime += 1}";
            TimeSpanExtension convertedTime = new TimeSpanExtension().FromSeconds(rawTime);
            string time = $"Screen Time: {convertedTime.Hours} hours {convertedTime.Minutes} minutes {convertedTime.Seconds} seconds";
            string[] infos = { timeRaw, time };
            File.WriteAllLines(fileName, infos);
        }
    }


    internal enum TimeFilter : int
    {
        Day = 0,
        Week = -6,
        Month = -29,
        Year = -364
    }
}


internal class TimeSpanExtension
{
    public long Days { get; private set; }
    public long Hours { get; private set; }
    public long Minutes { get; private set; }
    public long Seconds { get; private set; }
    public TimeSpanExtension FromSeconds(long timeInSeconds)
    {
        Days = timeInSeconds / 86400;
        Hours = timeInSeconds % 86400 / 3600;
        Minutes = timeInSeconds % 3600 / 60;
        Seconds = timeInSeconds % 60;
        return this;
    }
    public override string ToString() => $"{Days} days {Hours} hours {Minutes} minutes {Seconds} seconds";
}