using System;
using System.Collections.Generic;

using AudioAPI.Playback;

using LabExtended.API;
using LabExtended.Events;
using LabExtended.Extensions;

using AdminToys;

using Mirror;

using UnityEngine;

using Utils.NonAllocLINQ;

using VoiceChat.Networking;

namespace AudioAPI;

public class AudioHandler : IDisposable
{
    private static byte idClock = 0;
    
    public static Dictionary<string, AudioHandler> Handlers { get; } = new Dictionary<string, AudioHandler>();
    public static Queue<byte> IdQueue { get; } = new Queue<byte>();
    public static SpeakerToy Prefab { get; private set; }

    public static byte NextId
    {
        get
        {
            if (IdQueue.TryDequeue(out var nextId))
                return nextId;

            if (idClock + 1 > byte.MaxValue)
                throw new Exception("No more available IDs.");

            return idClock++;
        }
    }

    public static bool TryGetHandler(string name, out AudioHandler handler)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        return Handlers.TryGetValue(name, out handler);
    }

    public static AudioHandler GetOrAdd(string name, Action<AudioHandler> setup = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (Handlers.TryGetValue(name, out var handler))
            return handler;

        handler = new AudioHandler(name, NextId);

        setup.InvokeSafe(handler);
        return handler;
    }

    public string Name { get; private set; }
    public byte? Id { get; private set; }

    public Dictionary<string, SpeakerToy> Speakers { get; private set; } = new Dictionary<string, SpeakerToy>();
    
    public AudioPlayer Player { get; private set; }
    public GameObject GameObject { get; private set; }

    public Vector3 Position
    {
        get => GameObject.transform.position;
        set => GameObject.transform.position = value;
    }

    public Quaternion Rotation
    {
        get => GameObject.transform.rotation;
        set => GameObject.transform.rotation = value;
    }

    public Transform ParentTransform
    {
        get => GameObject.transform.parent;
        set => GameObject.transform.parent = value;
    }
    
    private AudioHandler(string name, byte id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        if (Handlers.ContainsKey(name))
            throw new ArgumentException($"Audio Handler already registered: {name}");
        
        Name = name;
        Id = id;

        Player = new AudioPlayer(this);
        GameObject = new GameObject(Name);

        Handlers.Add(name, this);
        
        RoundEvents.OnRoundRestarted += Dispose;
    }

    public void ForEachSpeaker(Action<SpeakerToy> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        Speakers.ForEachValue(action);
    }

    public void SetVolume(float volume)
        => ForEachSpeaker(x => x.NetworkVolume = volume);

    public void SetPosition(Vector3 position)
    {
        Position = position;
        ForEachSpeaker(x => x.NetworkPosition = position);
    }

    public bool HasSpeaker(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        return Speakers.ContainsKey(name);
    }

    public bool HasSpeaker(string name, out SpeakerToy speaker)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        return Speakers.TryGetValue(name, out speaker);
    }

    public bool AddSpeaker(string name, Action<SpeakerToy> setup = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        if (!Id.HasValue)
            throw new Exception("This audio handler does not have an ID.");
        
        if (Speakers.ContainsKey(name))
            return false;

        var speaker = CreateSpeaker(Id.Value, Vector3.zero);
        
        speaker.transform.parent = GameObject.transform;
        
        setup.InvokeSafe(speaker);
        
        Speakers.Add(name, speaker);
        return true;
    }

    public bool RemoveSpeaker(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (!Speakers.TryGetValue(name, out var speaker))
            return false;
        
        Speakers.Remove(name);
        
        NetworkServer.Destroy(speaker.gameObject);
        return true;
    }

    public void RemoveSpeakers()
    {
        Speakers.ForEachValue(x => NetworkServer.Destroy(x.gameObject));
        Speakers.Clear();
    }

    public void Send(byte[] data, int length, Func<ExPlayer, bool> receivePredicate = null)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        if (length < 0 || length > data.Length) throw new ArgumentOutOfRangeException(nameof(length));
        if (!Id.HasValue) throw new Exception("This audio handler does not have an ID.");
        
        var message = default(AudioMessage?);

        ExPlayer.Players.ForEach(player =>
        {
            if (!player) return;
            if (receivePredicate != null && !receivePredicate(player)) return;

            if (!message.HasValue)
                message = new AudioMessage(Id.Value, data, length);

            player.Connection.Send(message.Value);
        });
    }

    public void Send(ExPlayer player, byte[] data, int length)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        if (length < 0 || length > data.Length) throw new ArgumentOutOfRangeException(nameof(length));
        if (!Id.HasValue) throw new Exception("This audio handler does not have an ID.");
        if (!player) throw new ArgumentNullException(nameof(player));
        
        player.Connection.Send(new AudioMessage(Id.Value, data, length));
    }
    
    public void Dispose()
    {
        RoundEvents.OnRoundRestarted -= Dispose;

        if (Player != null && !Player.IsDisposed)
            Player.Dispose();
        
        Player = null;
        
        if (!string.IsNullOrWhiteSpace(Name))
        {
            Handlers.Remove(Name);
            Name = null;
        }

        if (Id.HasValue)
        {
            IdQueue.Enqueue(Id.Value);
            Id = null;
        }

        if (Speakers != null)
        {
            Speakers.ForEachValue(x => NetworkServer.Destroy(x.gameObject));
            Speakers.Clear();
            Speakers = null;
        }
    }

    public static SpeakerToy CreateSpeaker(byte controllerId, Vector3 position, float volume = 1f, bool isSpatial = true, float minDistance = 1f, float maxDistance = 5f)
    {
        if (Prefab is null)
        {
            foreach (var pref in NetworkClient.prefabs.Values)
            {
                if (!pref.TryGetComponent<SpeakerToy>(out var target))
                    continue;

                Prefab = target;
                break;
            }
        }

        var newInstance = UnityEngine.Object.Instantiate(Prefab, position, Quaternion.identity);

        newInstance.NetworkControllerId = controllerId;
        newInstance.NetworkVolume = volume;
        
        newInstance.NetworkIsSpatial = isSpatial;
        newInstance.NetworkIsStatic = false;
        
        newInstance.NetworkMinDistance = minDistance;
        newInstance.NetworkMaxDistance = maxDistance;

        NetworkServer.Spawn(newInstance.gameObject);
        return newInstance;
    }
    
    internal static void OnRoundRestart()
        => idClock = 0;
}