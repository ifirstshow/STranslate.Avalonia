using System.Text;
using System.Text.Json;

namespace STranslate.Worker.Ipc;

/// <summary>
/// worker IPC 帧（ndjson：一行一帧）。params/result 均为原始 JSON 字符串，
/// 两端各自按方法约定解析，协议层不做强类型耦合。
/// </summary>
public sealed class IpcFrame
{
    /// <summary>请求关联 id；worker 主动推送（describe）为 null。</summary>
    public long? Id { get; set; }

    /// <summary>方法名。</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>请求参数 JSON（或 worker 推送的载荷）。</summary>
    public string? Params { get; set; }

    /// <summary>成功结果 JSON。</summary>
    public string? Result { get; set; }

    /// <summary>错误消息（非空表示失败）。</summary>
    public string? Error { get; set; }

    public string Serialize()
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            if (Id is { } id)
                writer.WriteNumber("id", id);
            writer.WriteString("method", Method);
            if (Params is not null)
            {
                writer.WritePropertyName("params");
                WriteRawOrString(writer, Params);
            }
            if (Result is not null)
            {
                writer.WritePropertyName("result");
                WriteRawOrString(writer, Result);
            }
            if (Error is not null)
                writer.WriteString("error", Error);
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static void WriteRawOrString(Utf8JsonWriter writer, string value)
    {
        if (value.StartsWith('{') || value.StartsWith('['))
        {
            using var doc = JsonDocument.Parse(value);
            doc.RootElement.WriteTo(writer);
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }

    public static IpcFrame? Deserialize(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;
        using var doc = JsonDocument.Parse(line);
        var root = doc.RootElement;
        var frame = new IpcFrame
        {
            Method = root.TryGetProperty("method", out var m) ? m.GetString() ?? "" : "",
            Params = root.TryGetProperty("params", out var p) ? p.GetRawText() : null,
            Result = root.TryGetProperty("result", out var r) ? r.GetRawText() : null,
            Error = root.TryGetProperty("error", out var e) ? e.GetString() : null
        };
        if (root.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.Number)
            frame.Id = id.GetInt64();
        return frame;
    }
}

/// <summary>ndjson 帧读写器（包一层 StreamReader/Writer）。</summary>
public sealed class IpcConnection : IDisposable
{
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public IpcConnection(Stream stream)
    {
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };
    }

    /// <summary>读一帧；流关闭返回 null。</summary>
    public async Task<IpcFrame?> ReadAsync(CancellationToken cancellationToken = default)
    {
        var line = await _reader.ReadLineAsync(cancellationToken);
        return line is null ? null : IpcFrame.Deserialize(line);
    }

    public async Task WriteAsync(IpcFrame frame, CancellationToken cancellationToken = default)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            await _writer.WriteLineAsync(frame.Serialize());
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public void Dispose()
    {
        _reader.Dispose();
        _writer.Dispose();
        _writeLock.Dispose();
    }
}
