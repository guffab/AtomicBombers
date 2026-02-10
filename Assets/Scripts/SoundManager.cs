using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sounds are queued up to prevent too many from playing at the same time.<br/>
/// In the event of a burst (many components calling at the same time) the limit is increased to allow for more powerful sounds.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    const int MinSources = 3;
    const int MaxSources = 8;
    const int BurstThreshold = 15;
    const float BurstWindow = 0.1f;

    Queue<AudioSource> sourceQueue = new Queue<AudioSource>(MinSources);
    Queue<AudioSource> sourceQueueLarge = new Queue<AudioSource>(MaxSources);
    Queue<float> requestTimes = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < MaxSources - MinSources; i++)
            sourceQueueLarge.Enqueue(gameObject.AddComponent<AudioSource>());

        for (int i = 0; i < MinSources; i++)
        {
            sourceQueue.Enqueue(gameObject.AddComponent<AudioSource>());
            sourceQueueLarge.Enqueue(gameObject.AddComponent<AudioSource>());
        }
    }

    public static void Play(AudioClip clip, float volume = 1f) => Instance.PlayInternal(clip, volume);

    private void PlayInternal(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
            return;

        RegisterRequest();
        var source = GetSource();

        source.Stop();
        source.clip = clip;
        source.volume = volume;
        source.Play();
    }

    private void RegisterRequest()
    {
        float now = Time.unscaledTime;
        requestTimes.Enqueue(now);

        while (requestTimes.Count > 0 && now - requestTimes.Peek() > BurstWindow)
            requestTimes.Dequeue();
    }

    private AudioSource GetSource()
    {
        if (requestTimes.Count >= BurstThreshold)
        {
            var source = sourceQueueLarge.Dequeue();
            sourceQueueLarge.Enqueue(source);
            return source;
        }
        else
        {
            var source = sourceQueue.Dequeue();
            sourceQueue.Enqueue(source);
            return source;
        }
    }
}
