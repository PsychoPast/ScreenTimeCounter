using System.Runtime.InteropServices;

namespace ScreenTimeCounter
{
    /// <summary>
    /// Contains the time of the last input.
    /// </summary>
    internal struct LASTINPUTINFO
    {
        /// <summary>
        /// The size of the structure, in bytes.
        /// </summary>
        public uint cbSize;
        /// <summary>
        /// The tick count when the last input event was received.
        /// </summary>
        public uint dwTime;
    }

    internal static class Win32
    {
        /// <summary>
        /// Retrieves the time of the last input event.
        /// </summary>
        /// <param name="plii">A pointer to a LASTINPUTINFO structure that receives the time of the last input event.</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetLastInputInfo")]
        public static extern bool GetLastUserInput(ref LASTINPUTINFO plii);
    }
}