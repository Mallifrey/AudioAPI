using System;
using System.IO;
using System.Collections.Generic;

using LabApi.Loader.Features.Paths;

using LabExtended.Core;
using LabExtended.Attributes;
using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Utilities.Unity;
using NVorbis;
using UnityEngine.PlayerLoop;
using VoiceChat;

namespace AudioAPI;

using Playback;

public static class AudioManager
{
    public struct AudioUpdateLoop { }
    
    public static AudioConfig Config => AudioPlugin.Config;
    public static AudioPlugin Plugin => AudioPlugin.Plugin;
    
    public static Dictionary<string, AudioClip> Clips { get; } = new Dictionary<string, AudioClip>();

    public static event Action Update;

    public static bool TryGetClip(string clipName, out AudioClip clip)
    {
        if (string.IsNullOrWhiteSpace(clipName))
            throw new ArgumentNullException(nameof(clipName));
        
        return Clips.TryGetValue(clipName, out clip);
    }

    public static AudioClip GetClip(string clipName)
    {
        if (string.IsNullOrWhiteSpace(clipName))
            throw new ArgumentNullException(nameof(clipName));
        
        return Clips[clipName];
    }
    
    public static void LoadFiles()
    {
        if (Clips.Count > 0)
            return;

        foreach (var file in Directory.GetFiles(Config.Directory))
        {
            var name = Path.GetFileNameWithoutExtension(file);

            if (Clips.ContainsKey(name))
            {
                ApiLog.Error("Audio Manager", $"File &1{name}&r has already been loaded");
                continue;
            }

            try
            {
                var data = File.ReadAllBytes(file);

                if (!ValidateFile(data, false, name, out var stream, out var reader))
                    continue;
                
                Clips.Add(name, new AudioClip(name, data, stream, reader));
                
                ApiLog.Info("Audio Manager", $"Loaded audio file &1{name}&r (&6{Mirror.Utils.PrettyBytes(data.Length)}&r)");
            }
            catch (Exception ex)
            {
                ApiLog.Error("Audio Manager", $"Failed to load file &1{name}&r:\n{ex.ToColoredString()}");
            }
        }
    }
    
    public static bool ValidateFile(byte[] fileBytes, bool disposeReader, string logFileName, out MemoryStream stream, out VorbisReader reader)
    {
        if (fileBytes is null)
            throw new ArgumentNullException(nameof(fileBytes));
        
        stream = null;
        reader = null;

        if (fileBytes.Length < 20)
        {
            if (!string.IsNullOrWhiteSpace(logFileName))
                ApiLog.Error("Audio Manager", $"Could not load file &1{logFileName}&r: file too small");

            return false;
        }
        
        stream = new MemoryStream(fileBytes);
        stream.Seek(0, SeekOrigin.Begin);
        
        reader = new VorbisReader(stream);

        if (reader.TotalSamples < 1)
        {
            if (!string.IsNullOrWhiteSpace(logFileName))
                ApiLog.Error("Audio Manager", $"Could not load file &1{logFileName}&r: file contains zero audio samples");
            
            if (disposeReader)
            {
                reader.Dispose();
                stream.Dispose();
            }

            return false;
        }

        if (reader.Channels != VoiceChatSettings.Channels)
        {
            if (!string.IsNullOrWhiteSpace(logFileName))
                ApiLog.Error("Audio Manager", $"Could not load file &1{logFileName}&r: audio must be mono");

            if (disposeReader)
            {
                reader.Dispose();
                stream.Dispose();
            }

            return false;
        }

        if (reader.SampleRate != VoiceChatSettings.SampleRate)
        {
            if (!string.IsNullOrWhiteSpace(logFileName))
                ApiLog.Error("Audio Manager", $"Could not load file &1{logFileName}&r: audio must be sampled at {VoiceChatSettings.SampleRate} KHz");

            if (disposeReader)
            {
                reader.Dispose();
                stream.Dispose();
            }

            return false;
        }

        if (disposeReader)
        {
            reader.Dispose();
            stream.Dispose();
        }

        return true;
    }
    
    internal static void Init()
    {
        if (string.IsNullOrWhiteSpace(Config.Directory))
        {
            Config.Directory = Path.Combine(PathManager.SecretLab.FullName, "Audio API");
            Plugin.SaveConfig();
        }
        
        if (!Directory.Exists(Config.Directory))
            Directory.CreateDirectory(Config.Directory);

        var watcher = new FileSystemWatcher(Config.Directory) { NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime };

        watcher.Created += OnFileAdded;
        watcher.EnableRaisingEvents = true;

        RoundEvents.OnRoundRestarted += AudioHandler.OnRoundRestart;
        RoundEvents.OnRoundRestarted += AudioPool.Shared.OnRoundRestart;
        RoundEvents.OnWaitingForPlayers += AudioPool.Shared.OnRoundWait;
        
        PlayerLoopHelper.ModifySystem(x => x.InjectAfter<TimeUpdate.WaitForLastPresentationAndUpdateTime>(OnUpdate, typeof(AudioUpdateLoop)) ? x : null);
        
        LoadFiles();
    }

    private static void OnFileAdded(object _, FileSystemEventArgs ev)
    {
        ApiLog.Info("Audio Manager", $"Loading new file &1{ev.Name}&r");
        
        var name = Path.GetFileNameWithoutExtension(ev.FullPath);

        if (Clips.ContainsKey(name))
        {
            ApiLog.Error("Audio Manager", $"File &1{name}&r has already been loaded");
            return;
        }

        try
        {
            var data = File.ReadAllBytes(ev.FullPath);

            if (!ValidateFile(data, false, name, out var stream, out var reader))
                return;
                
            Clips.Add(name, new AudioClip(name, data, stream, reader));
                
            ApiLog.Info("Audio Manager", $"Loaded audio file &1{name}&r (&6{Mirror.Utils.PrettyBytes(data.Length)}&r)");
        }
        catch (Exception ex)
        {
            ApiLog.Error("Audio Manager", $"Failed to load file &1{name}&r:\n{ex.ToColoredString()}");
        }
    }

    private static void OnUpdate()
        => Update.InvokeSafe();
}