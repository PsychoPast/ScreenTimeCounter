using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using static ScreenTimeCounter.Win32;

namespace ScreenTimeCounter
{
    internal class IdleHandler
    {
        private double idleTime = 8000;

        private readonly ScreenTimeCounter _screenTimeCounter;

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
            _screenTimeCounter.Timer.Elapsed += CheckIdle;
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
                    Console.WriteLine(true);
                    _screenTimeCounter.Timer.Elapsed -= _screenTimeCounter.Capture;
                    _screenTimeCounter.IsCapturing = false;
                }
                else
                {
                    if (!_screenTimeCounter.IsCapturing)
                    {
                        _screenTimeCounter.Timer.Elapsed += _screenTimeCounter.Capture;
                    }
                }
            }
        }
    }
}