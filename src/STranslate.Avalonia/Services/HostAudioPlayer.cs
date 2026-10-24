using NAudio.Wave;
using STranslate.Sdk;

namespace STranslate.Avalonia.Services;

/// <summary>
/// 宿主音频播放（NAudio，Windows）。
/// ponytail: 仅支持 MP3/WAV（Edge TTS 返回 MP3）；PCM 流按需补采样率参数。
/// </summary>
public sealed class HostAudioPlayer
{
    public void Play(AudioData audio)
    {
        using var stream = new MemoryStream(audio.Content);
        using WaveStream reader = audio.Format switch
        {
            AudioFormat.Wav => new WaveFileReader(stream),
            AudioFormat.Mp3 or AudioFormat.Auto => new Mp3FileReader(stream),
            _ => throw new NotSupportedException($"暂不支持播放 {audio.Format} 音频")
        };
        using var output = new WaveOutEvent();
        output.Init(reader);
        output.Play();
        while (output.PlaybackState == PlaybackState.Playing)
            Thread.Sleep(100);
    }
}
