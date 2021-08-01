using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketTest
{
    public abstract class Barrage
    {

        protected ClientWebSocket _WebSocket;
        /// <summary>
        /// 心跳包轮询
        /// </summary>
        protected System.Timers.Timer _Timer;
        /// <summary>
        /// 语音播放
        /// </summary>
        protected VoiceBarrage _VoiceBarrage;
        /// <summary>
        /// 房间号
        /// </summary>
        protected int Roomid;
        public Barrage()
        {
            _WebSocket = new ClientWebSocket();
            _VoiceBarrage = new VoiceBarrage();
            _Timer = new System.Timers.Timer(30000);
            _Timer.Elapsed += Timer_Elapsed;
        }
        /// <summary>
        /// 心跳包
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected abstract void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e);
        /// <summary>
        /// 获取发送Bytes
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected abstract byte[] GetSendBytes(byte[] message);
        /// <summary>
        /// 操作弹幕
        /// </summary>
        /// <param name="item"></param>
        protected abstract void ControlBarrage(string item);
        /// <summary>
        /// 获取Url
        /// </summary>
        /// <returns></returns>
        protected abstract string GetUrl();
        /// <summary>
        /// Bytes转换为Analysis
        /// </summary>
        /// <param name="data"></param>
        /// <param name="analysisNumber"></param>
        /// <param name="model"></param>
        protected abstract void BytesToAnalysis(byte[] data, ref int analysisNumber, out Analysis model);
        /// <summary>
        /// Analysis转换为字符串
        /// </summary>
        /// <param name="analysis"></param>
        /// <returns></returns>
        protected abstract Task<List<string>> AnalysisToString(Analysis analysis);
        /// <summary>
        /// 第一连接发送Byte
        /// </summary>
        /// <returns></returns>
        protected abstract byte[] FirstConnect();
        /// <summary>
        /// 开始获取弹幕
        /// </summary>
        /// <param name="roomid"></param>
        public async void Start(int roomid)
        {
            Roomid = roomid;
            var url = GetUrl();
            var firstBytes = FirstConnect();
            var sendBytes = GetSendBytes(firstBytes);
            await _WebSocket.ConnectAsync(new Uri(url), CancellationToken.None);

            await _WebSocket.SendAsync(new ArraySegment<byte>(sendBytes), WebSocketMessageType.Binary, true, CancellationToken.None); //发送数据
            while (true)
            {
                var result = new byte[1024 * 4];
                var receiveResult = await _WebSocket.ReceiveAsync(new ArraySegment<byte>(result), CancellationToken.None);//接受数据
                try
                {
                    var analysisList = await AnalysisPackage(result, receiveResult.Count);
                    var JObjectList = await GetPackageBody(analysisList);
                    foreach (var item in JObjectList)
                    {
                        ControlBarrage(item);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }


        }
        /// <summary>
        /// 解析包
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected async Task<List<Analysis>> AnalysisPackage(byte[] data, int count)
        {
            List<Analysis> result = new List<Analysis>();
            Analysis model;
            var analysisNumber = 0;
            await Task.Run(() =>
            {
                while (analysisNumber < count)
                {
                    BytesToAnalysis(data, ref analysisNumber, out model);
                    result.Add(model);
                }
            });
            return result;
        }
        /// <summary>
        /// 获取包内容
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected async Task<List<string>> GetPackageBody(List<Analysis> list)
        {
            List<string> result = new List<string>();
            foreach (var item in list)
            {
                var stringList = await AnalysisToString(item);
                result.AddRange(stringList);
            }
            return result;
        }
    }
}
