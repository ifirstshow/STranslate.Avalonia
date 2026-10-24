using System.Net;
using System.Net.Sockets;

namespace STranslate.Avalonia.Services;

/// <summary>
/// 跨平台单实例：绑定 127.0.0.1 固定端口，绑定失败即为第二实例，
/// 通过连接一次通知首实例显示主窗口（无 IPC 数据，仅作信号）。
/// </summary>
public sealed class SingleInstanceService : IDisposable
{
    private const ushort Port = 50890;

    private TcpListener? _listener;
    public event Action? SecondInstanceStarted;

    public SingleInstanceService()
    {
        try
        {
            // 绑定成功才赋值 _listener；Start() 抛异常时保持 null
            var listener = new TcpListener(IPAddress.Loopback, Port);
            listener.Start();
            _listener = listener;
            _ = AcceptLoopAsync();
        }
        catch (SocketException)
        {
            // 端口被占 = 已有实例
            NotifyFirstInstance();
        }
    }

    public bool IsFirstInstance => _listener is not null;

    private static void NotifyFirstInstance()
    {
        try
        {
            using var client = new TcpClient();
            client.Connect(IPAddress.Loopback, Port);
        }
        catch
        {
            // ponytail: 首实例可能恰在退出，忽略；可接受偶尔丢失"唤起"信号
        }
    }

    private async Task AcceptLoopAsync()
    {
        var listener = _listener!;
        while (true)
        {
            using var client = await listener.AcceptTcpClientAsync();
            SecondInstanceStarted?.Invoke();
        }
    }

    public void Dispose() => _listener?.Stop();
}
