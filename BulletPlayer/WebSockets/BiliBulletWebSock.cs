using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BulletPlayer.Units.BulletScreen.Model;
using BulletPlayer.Units.BulletScreen.Model.Bili;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BulletPlayer.Kits
{
    public class BiliBulletWebSock
    {
        //private string bulletDefaultUrl = "https://api.live.bilibili.com/xlive/web-room/v1/dM/gethistory?roomid=";
        private string bulletDefaultUrl = "wss://broadcastlv.chat.bilibili.com/sub";

        private string getTokenUrl = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo?type=0&;id=";

        private string roomId;

        private string token;

        private int heartInterval = 25 * 1000;

        private Uri bulletWsUri;

        private ClientWebSocket _clientWebSocket = new ClientWebSocket();

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private byte[] bufferBytes = new byte[8192 * 1024];

        public BiliBulletWebSock(string roomId)
        {
            this.roomId = roomId;

            #region 获取token

            var request = WebRequest.Create($"{getTokenUrl}{roomId}");
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            var json = JsonConvert.DeserializeObject<GetTokenModel>(reader.ReadToEnd());

            token = json.data.token;

            bulletWsUri = new Uri(bulletDefaultUrl);

            #endregion

            WebSocket();
        }

        private async void WebSocket()
        {
            try
            {
                await _clientWebSocket.ConnectAsync(bulletWsUri, _cts.Token);

                #region 接收包

                await Task.Factory.StartNew(
                    async () =>
                    {
                        var receiveBuffer = new byte[8192 * 1024];

                        WebSocketReceiveResult receiveResult;

                        while (!_cts.IsCancellationRequested)
                        {
                            int receiveOffset = 0;

                            do
                            {
                                receiveResult = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, receiveOffset, receiveBuffer.Length - receiveOffset), _cts.Token);
                                receiveOffset += receiveResult.Count;

                            } while (!receiveResult.EndOfMessage);

                            BulletParse(receiveBuffer);
                        }
                    }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                #endregion

                #region 认证包

                var jsonIdentify = JsonConvert.SerializeObject(new
                {
                    uid = 293793435,
                    roomid = roomId,
                    protover = 2,
                    platform = "web",
                    clientver = "1.10.3",
                    type = "2",
                    key = token,
                });

                byte[] body = Encoding.UTF8.GetBytes(jsonIdentify);

                WebSocketHeader webSocketHeader = new WebSocketHeader(16 + body.Length, 16, 1, 7, 1);

                byte[] header = webSocketHeader.GetHeaderConcat();

                byte[] stream = header.Concat(body).ToArray();

                await _clientWebSocket.SendAsync(new ArraySegment<byte>(stream), WebSocketMessageType.Binary, true, _cts.Token);
                #endregion

                #region 心跳包

                while (!_cts.IsCancellationRequested)
                {
                    byte[] heartBodyBytes = Encoding.UTF8.GetBytes("[object object]");

                    var webSocketHeartHeader = new WebSocketHeader(16 + heartBodyBytes.Length, 16, 1, 2, 1);

                    byte[] heartHeader = webSocketHeartHeader.GetHeaderConcat();
                    byte[] heartStream = heartHeader.Concat(heartBodyBytes).ToArray();

                    await _clientWebSocket.SendAsync(new ArraySegment<byte>(heartStream), WebSocketMessageType.Binary, endOfMessage: true,
                        _cts.Token);

                    await Task.Delay(heartInterval, _cts.Token);
                }

                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void BulletParse(byte[] receiveBuffer)
        {
            byte[] header = new byte[16];

            Array.Copy(receiveBuffer, 0, header, 0, 16);
            var webSocketHeader = new WebSocketHeader(header);

            if (webSocketHeader.packageSize < 16)
            {
                return;
            }

            byte[] bufferBytes = receiveBuffer.Skip(16).Take(webSocketHeader.packageSize - 16).ToArray();

            string bufferString = Encoding.UTF8.GetString(bufferBytes);
            dynamic obj = JsonConvert.DeserializeObject<dynamic>(bufferString);

            switch (webSocketHeader.operation)
            {

                case 3:     //心跳包回复(人气值)
                    var popularity = BitConverter.ToInt32(bufferBytes.Reverse().ToArray(), 0);

                    Console.WriteLine($"人气值:{popularity}");

                    break;
                case 5:     //普通包(命令)

                    break;
                case 8:     //认证包回复
                    if (obj.code == "0")
                    {
                        Console.WriteLine("认证成功");
                    }
                    break;
            }

            

        }
    }
}
