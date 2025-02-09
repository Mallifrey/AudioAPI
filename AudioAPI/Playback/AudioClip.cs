using System;
using System.IO;

using LabExtended.Extensions;
using Mirror;
using NorthwoodLib.Pools;

using NVorbis;

namespace AudioAPI.Playback;

public class AudioClip
{
    public string Name { get; }
    
    public byte[] Data { get; }

    public TimeSpan Duration { get; }
    
    public int Channels { get; }
    public int SampleRate { get; }

    public (VorbisReader reader, MemoryStream stream) GetStream()
    {
        var newStream = new MemoryStream(Data);
        
        newStream.Seek(0, SeekOrigin.Begin);
        return (new VorbisReader(newStream), newStream);
    }

    public AudioClip(string name, byte[] data, MemoryStream stream, VorbisReader reader)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));
        
        if (reader is null)
            throw new ArgumentNullException(nameof(reader));
        
        Name = name;
        Data = data;

        Duration = reader.TotalTime;
        Channels = reader.Channels;
        SampleRate = reader.SampleRate;
        
        reader.Dispose();
        stream.Dispose();
    }

    public override string ToString()
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            x.Append("Audio Clip (Name=");
            x.Append(Name);
            x.Append("; Size=");
            x.Append(Mirror.Utils.PrettyBytes(Data.Length));
            x.Append("; Duration=");
            x.Append(Duration);
            x.Append(")");
        });
    }
}