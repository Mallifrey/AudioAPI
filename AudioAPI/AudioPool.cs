using System;

using AudioAPI.Playback;

using LabExtended.API;
using LabExtended.Core.Pooling;

using UnityEngine;

namespace AudioAPI;

public class AudioPool : PoolBase<AudioHandler>
{
    private static int _handlerIdClock = 0;

    public static AudioPool Shared { get; } = new AudioPool();
    
    public override string Name { get; } = "Audio Pool";

    public bool PlayAt(string clipName, Vector3 position, Func<ExPlayer, bool> receivePredicate,  float volume = 1f, float maxDistance = 50f, float minDistance = 5f, bool isSpatial = true)
    {
        if (string.IsNullOrWhiteSpace(clipName))
            throw new ArgumentNullException(nameof(clipName));

        if (!AudioManager.TryGetClip(clipName, out var clip))
            return false;
        
        PlayAt(clip, position, receivePredicate, volume, maxDistance, minDistance, isSpatial);
        return true;
    }
    
    public bool PlayAt(string clipName, Transform parent, Func<ExPlayer, bool> receivePredicate, float volume = 1f, float maxDistance = 50f, float minDistance = 5f, bool isSpatial = true)
    {
        if (string.IsNullOrWhiteSpace(clipName))
            throw new ArgumentNullException(nameof(clipName));

        if (parent is null)
            throw new ArgumentNullException(nameof(parent));
        
        if (!AudioManager.TryGetClip(clipName, out var clip))
            return false;
        
        PlayAt(clip, parent, receivePredicate, volume, maxDistance, minDistance, isSpatial);
        return true;
    }

    public void PlayAt(AudioClip clip, Vector3 position, Func<ExPlayer, bool> receivePredicate, float volume = 1f, float maxDistance = 50f, float minDistance = 5f, bool isSpatial = true)
    {
        if (clip is null)
            throw new ArgumentNullException(nameof(clip));
        
        var handler = Rent(null, () => AudioHandler.GetOrAdd($"PooledHandler_{_handlerIdClock++}"));
        
        handler.ForEachSpeaker(x =>
        {
            x.NetworkPosition = position;
            x.NetworkVolume = volume;
            
            x.NetworkMaxDistance = maxDistance;
            x.NetworkMinDistance = minDistance;
            
            x.NetworkIsSpatial = isSpatial;
        });
        
        handler.Player.OnFinished += OnFinished;
        handler.Player.ReceivePredicate = receivePredicate;
        handler.Player.Play(clip);
    }
    
    public void PlayAt(AudioClip clip, Transform parent, Func<ExPlayer, bool> receivePredicate,  float volume = 1f, float maxDistance = 50f, float minDistance = 5f, bool isSpatial = true)
    {
        if (clip is null)
            throw new ArgumentNullException(nameof(clip));
        
        if (parent is null)
            throw new ArgumentNullException(nameof(parent));
        
        var handler = Rent(null, () => AudioHandler.GetOrAdd($"PooledHandler_{_handlerIdClock++}"));
        
        handler.ForEachSpeaker(x =>
        {
            x.NetworkVolume = volume;
            
            x.NetworkMaxDistance = maxDistance;
            x.NetworkMinDistance = minDistance;
            
            x.NetworkIsSpatial = isSpatial;
            x.NetworkIsStatic = false;
        });

        handler.ParentTransform = parent;
        handler.Player.OnFinished += OnFinished;
        handler.Player.ReceivePredicate = receivePredicate;
        handler.Player.Play(clip);
    }
    
    public override void HandleReturn(AudioHandler item)
    {
        base.HandleReturn(item);
        
        if (item.Player.IsPlaying)
            item.Player.Stop(true);
    }

    public override void HandleNewItem(AudioHandler item)
    {
        base.HandleNewItem(item);
        
        item.AddSpeaker("PoolableSpeaker");
    }

    private AudioClip OnFinished(AudioPlayer player, AudioClip _)
    {
        player.OnFinished -= OnFinished;
        
        Return(player.Handler);
        return null;
    }

    internal void OnRoundRestart()
    {
        Clear();
        _handlerIdClock = 0;
    }

    internal void OnRoundWait()
        => Preload(AudioManager.Config.AudioPoolSize, () => AudioHandler.GetOrAdd($"PooledHandler_{_handlerIdClock++}"));
}