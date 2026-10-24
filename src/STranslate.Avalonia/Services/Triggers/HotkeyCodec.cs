using STranslate.Avalonia.Services.Triggers;

namespace STranslate.Avalonia.Services;

/// <summary>
/// 热键字符串与 Win32 组合的互转。格式："Ctrl+Alt+G"、"Alt+Shift+F12"，修饰键 Ctrl/Alt/Shift/Win。
/// </summary>
public static class HotkeyCodec
{
    public static bool TryParse(string? text, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        uint mods = 0;
        string? key = null;
        foreach (var raw in text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            switch (raw.ToLowerInvariant())
            {
                case "ctrl" or "control": mods |= Win32Native.MOD_CONTROL; break;
                case "alt": mods |= Win32Native.MOD_ALT; break;
                case "shift": mods |= Win32Native.MOD_SHIFT; break;
                case "win": mods |= Win32Native.MOD_WIN; break;
                default:
                    if (key is not null)
                        return false; // 出现两个非修饰键
                    key = raw;
                    break;
            }
        }

        if (key is null || mods == 0)
            return false;

        return TryKeyToVk(key, out vk) && (modifiers = mods) != 0;
    }

    public static bool TryKeyToVk(string key, out uint vk)
    {
        vk = 0;
        // A-Z
        if (key.Length == 1 && char.IsAsciiLetter(key[0]))
        {
            vk = char.ToUpperInvariant(key[0]);
            return true;
        }
        // 0-9
        if (key.Length == 1 && char.IsAsciiDigit(key[0]))
        {
            vk = key[0];
            return true;
        }
        // F1-F24
        if (key.Length is 2 or 3 && key[0] is 'F' or 'f' && int.TryParse(key[1..], out var f) && f is >= 1 and <= 24)
        {
            vk = 0x70u + (uint)(f - 1);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 解析裸键（无修饰键，如 F9/A/5），用于增量翻译触发键。
    /// </summary>
    public static bool TryParseBareKey(string? text, out uint vk)
    {
        vk = 0;
        if (string.IsNullOrWhiteSpace(text) || text.Contains('+'))
            return false;
        return TryKeyToVk(text.Trim(), out vk);
    }

    /// <summary>规范化显示（"alt+g" → "Alt+G"）；无法解析时原样返回。</summary>
    public static string Normalize(string text)
    {
        if (!TryParse(text, out var mods, out var vk))
            return text;

        var parts = new List<string>();
        if ((mods & Win32Native.MOD_CONTROL) != 0) parts.Add("Ctrl");
        if ((mods & Win32Native.MOD_ALT) != 0) parts.Add("Alt");
        if ((mods & Win32Native.MOD_SHIFT) != 0) parts.Add("Shift");
        if ((mods & Win32Native.MOD_WIN) != 0) parts.Add("Win");
        parts.Add(VkToKey(vk));
        return string.Join('+', parts);
    }

    private static string VkToKey(uint vk)
    {
        if (vk is >= 0x41 and <= 0x5A or >= 0x30 and <= 0x39)
            return ((char)vk).ToString();
        if (vk is >= 0x70 and <= 0x87)
            return $"F{vk - 0x70 + 1}";
        return $"VK_{vk:X2}";
    }
}
