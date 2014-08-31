using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BiliDMLib;
namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {


            BiliDMLib.DanmakuLoader L=new DanmakuLoader();
            L.ConnectAsync(1016).Wait();
            
        }
    }
}
