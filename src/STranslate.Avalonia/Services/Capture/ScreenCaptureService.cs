using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using STranslate.Avalonia.Services.Triggers;

namespace STranslate.Avalonia.Services.Capture;

/// <summary>一台显示器的冻结快照。</summary>
public sealed class MonitorSnapshot
{
    public required PixelPoint ScreenOrigin { get; init; }   // 屏幕原点（虚拟桌面物理坐标）
    public required PixelSize PixelSize { get; init; }       // 物理像素尺寸
    public required Bitmap Bitmap { get; init; }             // 冻结画面
    public required double Scaling { get; init; }            // 该屏 DPI 缩放（窗口 DIP 布局用）
}

/// <summary>
/// 截屏服务（Windows：GDI BitBlt 抓取 + Avalonia Bitmap 承载，AOT 安全）。
/// mac/Linux 按降级矩阵在对应里程碑接入各自实现。
/// </summary>
public static class ScreenCaptureService
{
    /// <summary>冻结全部显示器画面。</summary>
    public static IReadOnlyList<MonitorSnapshot> CaptureAllMonitors(TopLevel topLevel)
    {
        var screens = topLevel.Screens.All;
        var result = new List<MonitorSnapshot>(screens.Count);

        var screenDc = Gdi32.GetDC(IntPtr.Zero);
        if (screenDc == IntPtr.Zero)
            throw new InvalidOperationException("GetDC 失败");

        try
        {
            foreach (var screen in screens)
            {
                // 实测（2026-10-14）：Avalonia Screen.Bounds 已是物理像素（与 GetSystemMetrics 虚拟桌面一致），勿再乘缩放
                var bounds = screen.Bounds;
                var scaling = screen.Scaling;
                DebugTrace.Log($"screen bounds={bounds} scaling={scaling}");
                var origin = new PixelPoint(bounds.X, bounds.Y);
                var size = new PixelSize(bounds.Width, bounds.Height);

                var bitmap = CaptureRegion(screenDc, origin, size);
                result.Add(new MonitorSnapshot
                {
                    ScreenOrigin = origin,
                    PixelSize = size,
                    Bitmap = bitmap,
                    Scaling = scaling
                });
            }
        }
        finally
        {
            _ = Gdi32.ReleaseDC(IntPtr.Zero, screenDc);
        }
        return result;
    }

    /// <summary>从屏幕 DC 抓取指定物理像素区域，输出 BGRA 位图。</summary>
    private static Bitmap CaptureRegion(IntPtr screenDc, PixelPoint origin, PixelSize size)
    {
        var memDc = Gdi32.CreateCompatibleDC(screenDc);
        if (memDc == IntPtr.Zero)
            throw new InvalidOperationException("CreateCompatibleDC 失败");

        var bi = new Gdi32.BITMAPINFO
        {
            biSize = (uint)Marshal.SizeOf<Gdi32.BITMAPINFO>(),
            biWidth = size.Width,
            biHeight = -size.Height, // 自顶向下
            biPlanes = 1,
            biBitCount = 32,
            biCompression = 0 // BI_RGB
        };

        var bits = IntPtr.Zero;
        var hBitmap = Gdi32.CreateDIBSection(screenDc, in bi, 0 /* DIB_RGB_COLORS */, ref bits, IntPtr.Zero, 0);
        if (hBitmap == IntPtr.Zero || bits == IntPtr.Zero)
        {
            _ = Gdi32.DeleteDC(memDc);
            throw new InvalidOperationException("CreateDIBSection 失败");
        }

        try
        {
            var old = Gdi32.SelectObject(memDc, hBitmap);
            _ = Gdi32.BitBlt(memDc, 0, 0, size.Width, size.Height, screenDc, origin.X, origin.Y, Gdi32.SRCCOPY);
            _ = Gdi32.SelectObject(memDc, old);

            // BGRA → Avalonia 位图（BGRA8888 正好对应 Avalonia BGRA8888 像素格式）
            var buffer = Marshal.AllocHGlobal(size.Width * size.Height * 4);
            try
            {
                unsafe
                {
                    Buffer.MemoryCopy((void*)bits, (void*)buffer, (long)size.Width * size.Height * 4, (long)size.Width * size.Height * 4);
                }
                return new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, buffer, size, new Vector(96, 96), size.Width * 4);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            _ = Gdi32.DeleteObject(hBitmap);
            _ = Gdi32.DeleteDC(memDc);
        }
    }
}

internal static partial class Gdi32
{
    [LibraryImport("user32.dll")]
    public static partial int GetSystemMetrics(int nIndex);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AddClipboardFormatListener(IntPtr hwnd);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool RemoveClipboardFormatListener(IntPtr hwnd);

    [LibraryImport("user32.dll")]
    public static partial IntPtr GetDC(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    public static partial int ReleaseDC(IntPtr hWnd, IntPtr hDc);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    public static partial IntPtr CreateCompatibleDC(IntPtr hdc);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    public static partial IntPtr CreateDIBSection(IntPtr hdc, in BITMAPINFO pbmi, uint iUsage, ref IntPtr ppvBits, IntPtr hSection, uint dwOffset);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    public static partial IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool BitBlt(IntPtr hdcDest, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint rop);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DeleteObject(IntPtr hObject);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DeleteDC(IntPtr hdc);

    public const uint SRCCOPY = 0x00CC0020;

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFO
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }
}
