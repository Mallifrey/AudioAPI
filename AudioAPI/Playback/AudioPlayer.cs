using System;
using System.Collections.Generic;
using System.IO;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Extensions;

using MEC;

using NVorbis;

using UnityEngine;

using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;

namespace AudioAPI.Playback;

public class AudioPlayer : IDisposable
{
    public const int HeadSamples = 1920;
    
    private bool isEventAssigned = false;
    private bool isDisposed = false;
    
    private bool isStop = false;
    private bool isReady = false;

    private byte[] encodedBuffer = new byte[VoiceChatSettings.MaxEncodedSize];

    private float[] sendBuffer;
    private float[] readBuffer;

    private float allowedSamples;
    private int samplesPerSec;
    
    private CoroutineHandle coroutineHandle;
    
    private OpusEncoder encoder = new OpusEncoder(OpusApplicationType.Voip);
    private PlaybackBuffer buffer = new PlaybackBuffer();
    
    private VorbisReader reader;
    private MemoryStream stream;
    
    private Queue<float> streamQueue = new Queue<float>();
    
    public bool IsDisposed => isDisposed;
    
    public bool IsPlaying { get; private set; }
    
    public bool IsPaused { get; set; }
    public bool IsLooping { get; set; }

    public float Volume { get; set; } = 100f;
    
    public AudioHandler Handler { get; private set; }
    
    public AudioClip CurrentClip { get; private set; }
    public AudioClip NextClip { get; private set; }

    public Queue<AudioClip> ClipQueue { get; private set; } = new Queue<AudioClip>();
    
    public Func<ExPlayer, bool> ReceivePredicate { get; set; }

    public event Action<AudioPlayer> OnDisposed;
    public event Action<AudioPlayer, AudioClip> OnStarted;
    
    public event Func<AudioPlayer, AudioClip, AudioClip> OnFinished;  
    
    public AudioPlayer(AudioHandler handler)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));
        
        Handler = handler;

        AudioManager.Update += OnUpdate;
    }

    public void Dispose()
    {
        ThrowIfDisposed();
        
        AudioManager.Update -= OnUpdate;
        
        OnDisposed.InvokeSafe(this);
        
        Cleanup();

        NextClip = null;
        CurrentClip = null;
        
        ClipQueue?.Clear();
        ClipQueue = null;
        
        encodedBuffer = null;
        sendBuffer = null;
        readBuffer = null;
        streamQueue = null;
        
        encoder?.Dispose();
        encoder = null;
        
        buffer?.Dispose();
        buffer = null;
        
        isDisposed = true;
    }

    public bool Play(string clipName, bool overrideCurrent = false)
    {
        if (string.IsNullOrWhiteSpace(clipName))
            throw new ArgumentNullException(nameof(clipName));

        ThrowIfDisposed();
        
        if (!AudioManager.TryGetClip(clipName, out var audioClip))
            return false;
        
        Play(audioClip, overrideCurrent);
        return true;
    }

    public void Play(AudioClip clip, bool overrideCurrent = false)
    {
        if (clip is null)
            throw new ArgumentNullException(nameof(clip));
        
        ThrowIfDisposed();

        if (IsPlaying && !overrideCurrent)
        {
            ClipQueue.Enqueue(clip);
            return;
        }

        if (IsPlaying)
        {
            if (NextClip != null)
                ClipQueue.Enqueue(NextClip);
            
            Cleanup();
        }

        CurrentClip = clip;
        coroutineHandle = Timing.RunCoroutine(Playback(), Segment.FixedUpdate);
    }

    public void Stop(bool clearQueue = false)
    {
        ThrowIfDisposed();
        Cleanup();
        
        if (clearQueue)
            ClipQueue.Clear();
        else if (ClipQueue.TryDequeue(out var clip))
            Play(clip, true);
    }
    
    public void Pause()
        => IsPaused = true;
    
    public void Resume()
        => IsPaused = false;
    
    public void TogglePause()
        => IsPaused = !IsPaused;
    
    public void Loop()
        => IsLooping = true;
    
    public void StopLoop()
        => IsLooping = false;
    
    public void ToggleLoop()
        => IsLooping = !IsLooping;
    
    private void OnUpdate()
    {
        if (Handler is null || !isReady || streamQueue is null || streamQueue.Count < 1 || isStop)
            return;
        
        allowedSamples += Time.deltaTime * samplesPerSec;
        
        var toCopy = Mathf.Min(Mathf.FloorToInt(allowedSamples), streamQueue.Count);

        for (int i = 0; i < toCopy; i++)
            buffer.Write(streamQueue.Dequeue() * (Volume / 100f));

        allowedSamples -= toCopy;

        while (buffer.Length >= VoiceChatSettings.PacketSizePerChannel)
        {
            buffer.ReadTo(sendBuffer, VoiceChatSettings.PacketSizePerChannel);
            Handler.Send(encodedBuffer, encoder.Encode(sendBuffer, encodedBuffer, VoiceChatSettings.PacketSizePerChannel), ReceivePredicate);
        }
    }

    private IEnumerator<float> Playback()
    {
        isStop = false;
        isReady = false;

        var audio = CurrentClip.GetStream();

        stream = audio.stream;
        reader = audio.reader;

        samplesPerSec = VoiceChatSettings.SampleRate * VoiceChatSettings.Channels;

        sendBuffer = new float[samplesPerSec / 5 + HeadSamples];
        readBuffer = new float[samplesPerSec / 5 + HeadSamples];

        OnStarted.InvokeSafe(this, CurrentClip);

        IsPlaying = true;
        
        ApiLog.Debug("Audio Player", $"Started {CurrentClip}");
        
        while (reader.ReadSamples(readBuffer, 0, readBuffer.Length) > 0)
        {
            if (isStop)
            {
                reader.SeekTo(reader.TotalSamples - 1);
                isStop = false;
            }

            while (IsPaused)
                yield return Timing.WaitForOneFrame;

            while (streamQueue.Count >= readBuffer.Length)
            {
                isReady = true;
                yield return Timing.WaitForOneFrame;
            }

            for (int i = 0; i < readBuffer.Length; i++)
                streamQueue.Enqueue(readBuffer[i]);
        }

        while (streamQueue.Count > 0)
            yield return Timing.WaitForOneFrame;
        
        ApiLog.Debug("Audio Player", $"Finished {CurrentClip}");

        IsPlaying = false;

        NextClip = null;
        
        if (IsLooping)
            NextClip = CurrentClip;
        else if (ClipQueue.TryDequeue(out var clip))
            NextClip = clip;

        if (OnFinished != null)
            NextClip = OnFinished(this, NextClip);

        Cleanup(NextClip is null);

        if (NextClip != null)
        {
            CurrentClip = NextClip;
            NextClip = null;
            
            coroutineHandle = Timing.RunCoroutine(Playback(), Segment.FixedUpdate);
        }
    }

    private void Cleanup(bool killCoroutine = true)
    {
        if (killCoroutine)
            Timing.KillCoroutines(coroutineHandle);

        IsPlaying = false;
        
        isStop = false;
        isReady = false;
        
        reader?.Dispose();
        reader = null;
        
        stream?.Dispose();
        stream = null;
        
        streamQueue?.Clear();
        buffer?.Clear();
    }

    private void ThrowIfDisposed()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(AudioPlayer));
    }
}