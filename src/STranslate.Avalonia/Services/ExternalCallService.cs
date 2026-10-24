using System.Net;
using Avalonia.Threading;
using STranslate.Avalonia.Services.Triggers;
using STranslate.Avalonia.ViewModels;

namespace STranslate.Avalonia.Services;

/// <summary>
/// 外部调用 HTTP 接口（对齐 WPF ExternalCallService，默认端口 50020）。
/// GET/POST http://127.0.0.1:51080/{action}，body 为可选文本。
/// 动作：translate / translate_force / translate_input / translate_crossword /
///       translate_ocr(截图翻译) / ocr_silence(静默 OCR)。
/// </summary>
public sealed class ExternalCallService(
    MainViewModel viewModel,
    Func<WindowsTriggerService?> triggers)
{
    private readonly MainViewModel _viewModel = viewModel;
    private readonly Func<WindowsTriggerService?> _triggers = triggers;

    private HttpListener? _listener;

    public void Start(string prefix)
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            return;
        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _listener.Start();
            _ = Task.Run(() => ListenLoopAsync(_listener));
        }
        catch (Exception e)
        {
            Program.LogError("ExternalCall.Start", e);
        }
    }

    private async Task ListenLoopAsync(HttpListener listener)
    {
        while (listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await listener.GetContextAsync();
            }
            catch (Exception)
            {
                break; // listener 已停止
            }
            _ = Task.Run(() => HandleAsync(context));
        }
    }

    private async Task HandleAsync(HttpListenerContext context)
    {
        try
        {
            var uri = context.Request.Url ?? throw new InvalidOperationException("no url");
            var action = uri.AbsolutePath.TrimStart('/');
            if (action == "")
                action = "translate";

            string content = "";
            if (context.Request.HttpMethod == "POST")
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                content = await reader.ReadToEndAsync();
            }
            else if (context.Request.HttpMethod != "GET")
            {
                throw new InvalidOperationException("Method Not Allowed");
            }

            await Dispatcher.UIThread.InvokeAsync(() => Execute(action, content));
            context.Response.StatusCode = 200;
        }
        catch (Exception e)
        {
            Program.LogError("ExternalCall", e);
            context.Response.StatusCode = 500;
        }
        finally
        {
            context.Response.Close();
        }
    }

    private void Execute(string action, string content)
    {
        switch (action)
        {
            case "translate":
                if (string.IsNullOrWhiteSpace(content))
                    _viewModel.InputClear();
                else
                    _viewModel.ShowAndTranslate(content, forceForeground: false);
                break;
            case "translate_force":
                if (string.IsNullOrWhiteSpace(content))
                    _viewModel.InputClear();
                else
                    _viewModel.ShowAndTranslate(content, forceForeground: true);
                break;
            case "translate_input":
                _viewModel.InputClear();
                break;
            case "translate_crossword":
                _triggers()?.BeginCrosswordCapture(forceForeground: false);
                break;
            case "translate_ocr":
                _triggers()?.BeginScreenshot();
                break;
            case "ocr_silence":
                _triggers()?.BeginSilentOcr();
                break;
            default:
                Program.LogError("ExternalCall", new InvalidOperationException($"unknown action: {action}"));
                break;
        }
    }

    public void Stop()
    {
        try { _listener?.Stop(); } catch { }
    }
}
