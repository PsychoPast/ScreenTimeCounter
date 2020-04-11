using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using static ScreenTimeCounter.Win32;

namespace ScreenTimeCounter
{
    internal class IdleHandler
    {
        private long _lastInputTime;
        private readonly ScreenTimeCounter _screenTimeCounter;
        public IdleHandler(ScreenTimeCounter screenTimeCounter)
        {
            _screenTimeCounter = screenTimeCounter;
            Initialize();
        }
        private void Initialize()
        {
            double interval = 300000;
            if (File.Exists("config.txt"))
            {
                if (double.TryParse(File.ReadAllText("config.txt").Split("=")[1], out double interv))
                {
                    interval = interv;
                }
            }
            Timer timer = new Timer()
            {
                Interval = interval,
                AutoReset = true
            };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            LASTINPUTINFO lASTINPUTINFO = new LASTINPUTINFO
            {
                cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>()
            };
            if (GetLastUserInput(ref lASTINPUTINFO))
            {
                long lastInputTime = lASTINPUTINFO.dwTime;
                if (_lastInputTime == lastInputTime)
                {
                    _screenTimeCounter.Timer.Stop();
                }
                else
                {
                    if (!_screenTimeCounter.Timer.Enabled)
                    {
                        _screenTimeCounter.Timer.Start();
                    }
                }
                _lastInputTime = lastInputTime;
            }
        }
    }
}