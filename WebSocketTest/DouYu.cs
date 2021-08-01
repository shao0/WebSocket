using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace WebSocketTest
{
    public class DouYu : Barrage
    {
        protected override Task<List<string>> AnalysisToString(Analysis analysis) => Task.Run(() => new List<string> { Encoding.UTF8.GetString(analysis.Body) });

        protected override void BytesToAnalysis(byte[] data, ref int analysisNumber, out Analysis model)
        {
            var packetLengthBytes = data.Skip(analysisNumber).Take(4).ToArray();
            var packetLength = BitConverter.ToInt32(packetLengthBytes, 0);
            var protocolVersionBytes = data.Skip(analysisNumber + 8).Take(2).ToArray();
            var protocolVersion = BitConverter.ToInt16(protocolVersionBytes, 0);
            var body = data.Skip(analysisNumber + 12).Take(packetLength - 10).ToArray();
            model = new Analysis
            {
                PacketLength = packetLength,
                HeaderLength = 12,
                ProtocolVersion = protocolVersion,
                SequenceId = 1,
                Body = body
            };
            analysisNumber += packetLength + 4;
        }

        protected override async void ControlBarrage(string str)
        {
            Dictionary<string, string> item = new Dictionary<string, string>();
            try
            {
                foreach (var array in str.Split('/').Select(s => s.Replace("@S", "/").Replace("@A", "@").Replace("@=", " ").Split(' ')))
                {
                    if (array.Length > 1 && !item.ContainsKey(array[0]))
                    {
                        item.Add(array[0], array[1]);
                    }
                }
                if (!item.ContainsKey("type")) return;
                if (item["type"] == "pingreq")
                {
                    var group = new Dictionary<string, string>();
                    group.Add("type", "joingroup");
                    group.Add("rid", Roomid.ToString());
                    group.Add("gid", "1");
                    var groupBytes = GetSendBytes(DictionaryTOByte(group));
                    Console.WriteLine("发送加入房间包");
                    await _WebSocket.SendAsync(new ArraySegment<byte>(groupBytes), WebSocketMessageType.Binary, true, CancellationToken.None);
                    _Timer.Interval = 45000;
                    _Timer.Start();
                }
                else if (item["type"] == "chatmsg")
                {
                    var msg = $"{item["nn"]}发送弹幕{item["txt"]}";
                    Console.WriteLine(msg);
                    _VoiceBarrage.AddVoiceText(msg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        protected override byte[] FirstConnect()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("type", "loginreq");
            dictionary.Add("roomid", Roomid.ToString());
            dictionary.Add("dfl", "sn@A=107@Sss@A=1/sn@A=108@Sss@A=1/sn@A=105@Sss@A=1/sn@A=undefined@Sss@A=1");
            dictionary.Add("username", "60938040");
            dictionary.Add("uid", "60938040");
            dictionary.Add("ver", "20190610");
            dictionary.Add("aver", "218101901");
            dictionary.Add("ct", "0");
            return DictionaryTOByte(dictionary);
        }
        byte[] DictionaryTOByte(Dictionary<string, string> dictionary) => Encoding.UTF8.GetBytes(dictionary.Aggregate(string.Empty, (first, item) => first += $"{Escaped(item.Key)}@={Escaped(item.Value)}/") + "\0");
        protected override byte[] GetSendBytes(byte[] message)
        {
            byte[] result = new byte[message.Length + 12];
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    var coundBytes = BitConverter.GetBytes(message.Length + 8);
                    bw.Write(coundBytes);
                    bw.Write(coundBytes);
                    ushort type = 689;
                    var typeBytes = BitConverter.GetBytes(type);
                    bw.Write(typeBytes);
                    ushort e = 0;
                    var eBytes = BitConverter.GetBytes(e);
                    bw.Write(eBytes);
                    bw.Write(message);
                    result = ms.ToArray();
                }
            }
            return result;
        }

        static string Escaped(string source) => source.Replace("@", "@A").Replace("/", "@S");
        protected override string GetUrl() => "wss://danmuproxy.douyu.com:8503/";

        protected override void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var heartbeat = new Dictionary<string, string>();
            heartbeat.Add("type", "mrkl");
            var heartbeatBytes = GetSendBytes(DictionaryTOByte(heartbeat));
            _WebSocket.SendAsync(new ArraySegment<byte>(heartbeatBytes), WebSocketMessageType.Binary, true, CancellationToken.None);
            Console.WriteLine("发送心跳包");
        }
    }
}
