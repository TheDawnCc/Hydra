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

        private Uri bulletWsUri;

        private ClientWebSocket _clientWebSocket = new ClientWebSocket();

        private CancellationTokenSource _cts = new CancellationTokenSource();

        public BiliBulletWebSock(string roomId)
        {
            this.roomId = roomId;

            #region 获取token

            var request = WebRequest.Create($"{getTokenUrl}{roomId}");
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            var json = JsonConvert.DeserializeObject<GetTokenModel>(reader.ReadToEnd());

            token = json.data.token;

            bulletWsUri = new Uri($"wss://{json.data.host_list[0].host}/sub");

            #endregion

            WebSocket();
        }

        private async void WebSocket()
        {
            try
            {
                await _clientWebSocket.ConnectAsync(bulletWsUri, _cts.Token);


                #region 认证包

                var jsonIdentify = JsonConvert.SerializeObject(new
                {
                    uid = 0,
                    roomid = roomId,
                    protover = 2,
                    platform = "web",
                    clientver = "1.7.3",
                    type = "1",
                    key = token,
                });
                byte[] body = Encoding.UTF8.GetBytes(jsonIdentify);

                WebSocketHeader webSocketHeader = new WebSocketHeader(16 + body.Length, 16, 1, 7, 1);


                byte[] header = webSocketHeader.GetHeaderConcat();

                byte[] stream = header.Concat(body).ToArray();

                await _clientWebSocket.SendAsync(new ArraySegment<byte>(stream), WebSocketMessageType.Binary, true, _cts.Token);
                #endregion

                #region 接收包

                await Task.Factory.StartNew(
                    async () =>
                    {
                        var receiveBytes = new byte[128];
                        var receiveBuffer = new byte[8192 * 1024];

                        WebSocketReceiveResult receiveResult;

                        while (!_cts.IsCancellationRequested)
                        {
                            int receiveOffset = 0;

                            do
                            {
                                receiveResult = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer,receiveOffset,receiveBuffer.Length - receiveOffset), _cts.Token);
                                receiveOffset += receiveResult.Count;

                            } while (!receiveResult.EndOfMessage);

                            BulletParse(receiveBuffer);
                        }
                    }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                #endregion

                #region 心跳包

                while (!_cts.IsCancellationRequested)
                {

                    var message = Console.ReadLine();
                    byte[] sendBytes = Encoding.UTF8.GetBytes(message);
                    var sendBuffer = new ArraySegment<byte>(sendBytes);
                    await _clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true,
                        _cts.Token);
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

        }
    }
}
