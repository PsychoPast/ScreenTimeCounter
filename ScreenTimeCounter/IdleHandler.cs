using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using static ScreenTimeCounter.Win32;

namespace ScreenTimeCounter
{
    internal class IdleHandler
    {
        private double idleTime = 300000;

        private readonly ScreenTimeCounter _screenTimeCounter;

        private DateTime idleTimeStamp;
        public IdleHandler(ScreenTimeCounter screenTimeCounter)
        {
            _screenTimeCounter = screenTimeCounter;
            Initialize();
        }

        private void Initialize()
        {
            if (File.Exists("config.txt") && double.TryParse(File.ReadAllText("config.txt").Split("=")[1], out double interval))
            {
                idleTime = interval;
            }
            Timer idleTimer = new Timer()
            {
                Interval = 1000,
                AutoReset = true
            };
            idleTimer.Elapsed += CheckIdle;
            idleTimer.Start();
        }

        private void CheckIdle(object sender, ElapsedEventArgs e)
        {
            LASTINPUTINFO lASTINPUTINFO = new LASTINPUTINFO
            {
                cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>()
            };
            if (GetLastUserInput(ref lASTINPUTINFO))
            {
                long lastInputTime = Environment.TickCount64 - lASTINPUTINFO.dwTime;
                if (lastInputTime >= idleTime)
                {
                    if (_screenTimeCounter.Timer.Enabled)
                    {
                        idleTimeStamp = DateTime.Now;
                        Console.WriteLine($"[{idleTimeStamp:MM/dd/yyyy HH:mm:ss}] User has entered idle mode.");
                    }
                    _screenTimeCounter.Timer.Stop();
                }
                else
                {
                    if (!_screenTimeCounter.Timer.Enabled)
                    {
                        DateTime idleStop = DateTime.Now;
                        Console.WriteLine($"[{idleStop:MM/dd/yyyy HH:mm:ss}] User's not idle anymore (was idle for {new TimeSpanExtension().FromSeconds(Convert.ToInt32((idleStop - idleTimeStamp).TotalSeconds))})");
                        _screenTimeCounter.Timer.Start();
                    }
                }
            }
        }
    }
}