using STranslate.Avalonia.Services;
using Xunit;

namespace STranslate.Tests;

/// <summary>LoadIntoCurrent(path)：磁盘值必须拷贝进当前单例（导入/恢复即时生效的数据层）。</summary>
public class LoadIntoCurrentTests
{
    [Fact]
    public void LoadIntoCurrent_CopiesDiskValuesOntoCurrent()
    {
        // 写入一份"备份状态"
        var path = Path.Combine(Path.GetTempPath(), "itc-test-settings.json");
        File.WriteAllText(path, """
        {
          "TargetLang": "Japanese",
          "CtrlCcEnabled": true,
          "MouseSelectionEnabled": true,
          "Hotkeys": { "OpenWindow": "Ctrl+Alt+F9" }
        }
        """);

        try
        {
            var before = AppSettings.Current;
            var oldTarget = before.TargetLang;
            var oldCc = before.CtrlCcEnabled;
            var oldHk = before.Hotkeys.OpenWindow;

            AppSettings.LoadIntoCurrent(path);

            Assert.Equal("Japanese", before.TargetLang);
            Assert.True(before.CtrlCcEnabled);
            Assert.Equal("Ctrl+Alt+F9", before.Hotkeys.OpenWindow);
            Assert.NotEqual(oldTarget, before.TargetLang);

            // 还原现场
            before.TargetLang = oldTarget;
            before.CtrlCcEnabled = oldCc;
            before.Hotkeys.OpenWindow = oldHk;
        }
        finally
        {
            File.Delete(path);
        }
    }
}
