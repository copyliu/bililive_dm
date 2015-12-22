using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;

namespace Bililive_dm
{
    [StructLayout(LayoutKind.Sequential)]
    public struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        public IntPtr lpData;
    }
    public class SSTPProtocol
    {
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);
        public const int WM_COPYDATA = 0x004a;

        public static void SendSSPMsg(string msg)
        {
            var sb = new StringBuilder();
            //SEND SSTP/1.1
            //                Sender: foobar2000
            //                Option: nodescript,notranslate
            //                Script: \_q\0\s[0]\e
            //                Charset: UTF - 8

            sb.Append("SEND SSTP/1.1\r\n");
            sb.Append("Sender: Bilibili_Live_DM\r\n");
            sb.Append("Option: nodescript,notranslate\r\n");
            sb.Append("Charset: UTF-8\r\n");
            sb.Append("Script: " + msg.Replace("\r", "").Replace("\n", "\\n"));
            sb.Append("\r\n\r\n");
            foreach (var hwnd in GetSSPhWnd())
            {




                sendWindowsStringMessage(hwnd, 0, sb.ToString());
            }
        }

        private static int[] GetSSPhWnd()
        {
            try
            {
                MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("Sakura");
                var data = new Dictionary<string, Dictionary<string, string>>();
                byte[] buf;
                //
                //                            Mutex mutex = Mutex.OpenExisting("SakuraFMO");
                //                            mutex.WaitOne();
                //                            bool mutexCreated;
                //                            Mutex mutex = new Mutex(true, "SakuraFMO", out mutexCreated);
                using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                {
                    BinaryReader r = new BinaryReader(stream);

                    int len = r.ReadInt32();
                    if (len < 5)
                    {
                        return new int[0];
                    }
                    buf = new byte[len - 4];
                    r.Read(buf, 0, len - 4);
                    




                }
                var s = Encoding.Default.GetString(buf);
                Regex regex = new Regex("^([^.]+)\\.([^\u0001]+)\u0001([^\r\n]+)\r$", RegexOptions.Multiline);
                foreach (Match match in regex.Matches(s))
                {
                    string id = match.Groups[1].Value;
                    string key = match.Groups[2].Value;
                    string val = match.Groups[3].Value;

                    if (!data.ContainsKey(id))
                        data[id] = new Dictionary<string, string>();

                    data[id][key] = val;
                }
               return data.Where(d => d.Value.ContainsKey("hwnd")).Select(d => int.Parse(d.Value["hwnd"])).ToArray();
            }
            catch (Exception)
            {
                return new int[0];
            }
           
        }
       
        private static int sendWindowsStringMessage(int hWnd, int wParam, string msg)
        {
            int result = 0;
            if (hWnd > 0)
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
                IntPtr ptr = Marshal.AllocHGlobal(data.Length);

                try
                {
                    Marshal.Copy(data, 0, ptr, data.Length);

                    int len = data.Length;
                    COPYDATASTRUCT cds;
                    cds.dwData = (IntPtr)9801;
                    cds.lpData = ptr;
                    cds.cbData = len + 1;
                    result = SendMessage(hWnd, WM_COPYDATA, wParam, ref cds);
                    //deal with ptr

                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }


            }
            return result;
        }
    }
}