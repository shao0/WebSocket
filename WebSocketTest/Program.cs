using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Brotli;
using zlib;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Speech.Synthesis;

namespace WebSocketTest
{
    class Program
    {

        static void Main(string[] args)
        {
            Barrage barrage = new BiliBili();
            barrage.Start(3495920);
            while (true)
            {
                Console.ReadKey();
            }
        }
    }
}
