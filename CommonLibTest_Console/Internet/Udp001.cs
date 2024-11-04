using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Common_Util.Module.LayerComponentBaseLong;

namespace CommonLibTest_Console.Internet
{
    internal class Udp001() : TestBase("UDP 通讯测试")
    {
        protected override void RunImpl()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            Task serviceTask = runUdpListener(cts.Token);

            string? read = string.Empty;
            bool end = false;
            while (true)
            {
                read = Console.ReadLine();
                string[] reads = read?.Split(' ') ?? [];
                string r1 = reads.Length > 0 ? reads[0].ToLower().Trim() : string.Empty;
                string r2 = reads.Length > 1 ? reads[1] : string.Empty;
                string r3 = reads.Length > 2 ? reads[2] : string.Empty;
                switch (r1)
                {
                    case "stop":
                        break;
                    case "send":
                        _ = runUdpClient(r2, int.TryParse(r3, out int randomCount) ? randomCount : 1, cts.Token);
                        break;
                }
                if (end) break;
            }
            cts.Cancel();
        }

        byte[] Random(int length)
        {
             return Common_Util.Random.RandomStringHelper.GetRandomEnglishString(length, System.Random.Shared)
                .Select(i => (byte)i)
                .ToArray();
        }

        async Task runUdpClient(string ip, int randomCount, CancellationToken cancellationToken)
        {
            if (randomCount < 1) randomCount = 1;

            UdpClient client = new UdpClient();
            try
            {
                client.Connect(new System.Net.IPEndPoint(IPAddress.Parse(ip), 4324));
                WriteLine("客户端连接完成");
            }
            catch (Exception ex)
            {
                WriteLine("客户端连接异常: " + ex.Message);
                return;
            }

            int sendResult = 0;
            Exception? sendException = null;
            try
            {
                sendResult = await client.SendAsync(Random(randomCount), randomCount);
            }
            catch (Exception ex)
            {
                sendException = ex;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("========================");
            sb.AppendLine($"- 客户端发送随机内容");
            sb.AppendLine($"- 目标: {ip}:4324");
            if (sendException != null)
            {
                sb.AppendLine($"- 发送异常: {sendException.Message}");
            }
            else
            {
                sb.AppendLine($"- 成功发送数量: {sendResult}");
            }
            sb.AppendLine("========================");
            sb.AppendLine();
            WriteLine(sb.ToString());

            UdpReceiveResult result = default;
            Exception? receiveException = null;
            try
            {
                result = await client.ReceiveAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                receiveException = ex;
            }

            sb.Clear();
            sb.AppendLine();
            sb.AppendLine("========================");
            sb.AppendLine($"- 客户端接收到数据");
            if (receiveException != null)
            {
                sb.AppendLine($"- 接收异常: {receiveException.Message}");
            }
            else
            {
                sb.AppendLine($"- 源: {result.RemoteEndPoint}");
                sb.AppendLine($"- 内容: {result.Buffer.ToHexString()}");
            }
            sb.AppendLine("========================");
            sb.AppendLine();
            WriteLine(sb.ToString());

        }
        async Task runUdpListener(CancellationToken cancellationToken)
        {
            UdpClient client = new UdpClient();
            try
            {
                client.Client.Bind(new IPEndPoint(IPAddress.Any, 4324));
                WriteLine("服务端绑定完成");
            }
            catch (Exception ex)
            {
                WriteLine("服务端绑定异常: " + ex.Message);
                return;
            }
            while (!cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult result = default;
                Exception? receiveException = null;
                try
                {
                    result = await client.ReceiveAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    receiveException = ex;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("========================");
                sb.AppendLine($"- 服务端接收到数据");
                if (receiveException != null)
                {
                    sb.AppendLine($"- 接收异常: {receiveException.Message}");
                }
                else
                {
                    sb.AppendLine($"- 源: {result.RemoteEndPoint}");
                    sb.AppendLine($"- 内容: {result.Buffer.ToHexString()}");
                }
                sb.AppendLine("========================");
                sb.AppendLine();
                WriteLine(sb.ToString());

                if (receiveException != null) continue;

                int sendResult = 0;
                Exception? sendException = null;
                try
                {
                    sendResult = await client.SendAsync([(byte)'1'], 1, result.RemoteEndPoint);
                }
                catch (Exception ex)
                {
                    sendException = ex;
                }
                sb.Clear();
                sb.AppendLine();
                sb.AppendLine("========================");
                sb.AppendLine($"- 服务端回应固定内容");
                sb.AppendLine($"- 目标: {result.RemoteEndPoint}");
                if (sendException != null)
                {
                    sb.AppendLine($"- 发送异常: {sendException.Message}");
                }
                else
                {
                    sb.AppendLine($"- 成功发送数量: {sendResult}");
                }
                sb.AppendLine("========================");
                sb.AppendLine();
                WriteLine(sb.ToString());
            }
        }

    }
}
