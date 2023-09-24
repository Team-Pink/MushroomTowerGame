using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AudioSourceGroup
{
    [HideInInspector, SerializeField] string name;
    public List<AudioSource> audioSources;
    public Dictionary<string, AudioSource> audioSourceDictionary;
    public AudioSourceGroup(string nameInit)
    {
        name = nameInit;
        audioSources = new();
        audioSourceDictionary = new();
    }

    public bool TryGetClip(string clipName, out AudioSource audioSource)
    {
        if (audioSourceDictionary.TryGetValue(clipName, out audioSource))
        {
            return true;
        }
        Debug.LogError("Audio Source with clip name " + clipName + " not found");
        return false;
    }

    public void Validate()
    {
        // Populate Dictionary for Audio Calls
        audioSourceDictionary.Clear();
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource == null) continue;
            if (audioSource.clip == null)
            {
                Debug.LogWarning("Audio Source " + audioSource + " has no audio clip.", audioSource);
                continue;
            }

            audioSourceDictionary.Add(audioSource.clip.name, audioSource);
        }
    }
}

public class ActiveClip
{
    public AudioSource audioSource;
    public float remainingDuration;

    public ActiveClip(AudioSource audioSourceInit, float durationInit)
    {
        audioSource = audioSourceInit;
        remainingDuration = durationInit;
    }
}

public class AudioManager : MonoBehaviour
{
    // Singleton Reference
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject audioManager = new("Audio Manager");
                instance = audioManager.AddComponent<AudioManager>();
                Debug.LogWarning("No AudioManager exists. An empty AudioManager has been created.", instance);
            }

            return instance;
        }
        
        private set => instance = value;
   }
    private static AudioManager instance = null;

    // Data
    readonly List<ActiveClip> activeClips = new();

    // Ambience
    [Space(), SerializeField] List<AudioSourceGroup> ambience = new() { new("Ambience") };

    // Music
    [SerializeField] List<AudioSourceGroup> music = new() { new("Music") };

    // Sound Effects
    [Space(), SerializeField] List<AudioSourceGroup> soundEffects = new() { new("Enemies"), new("Towers") };

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Multiple instances of AudioManager exist. " +
                "This will cause fatal errors. An instance will be destroyed.", Instance);
            Debug.LogError("An instance of AudioManager attached to this GameObject has been destroyed.", gameObject);

            Destroy(this);
        }
        instance = this;
    }

    private void OnValidate()
    {
        foreach (AudioSourceGroup sourceGroup in ambience) { sourceGroup.Validate(); }
        foreach (AudioSourceGroup sourceGroup in music) { sourceGroup.Validate(); }
        foreach (AudioSourceGroup sourceGroup in soundEffects) { sourceGroup.Validate(); }
    }

    private void Update()
    {
        List<ActiveClip> clipsToRemove = new();
        foreach (ActiveClip clip in activeClips)
        {
            if (clip.remainingDuration < 0.0f)
            {
                clip.audioSource.Stop();
                clipsToRemove.Add(clip);
            }
            else
            {
                clip.remainingDuration -= Time.deltaTime;
            }
        }
        foreach (ActiveClip clip in clipsToRemove)
        {
            activeClips.Remove(clip);
        }
        clipsToRemove.Clear();
    }

    public static void PlayAmbience(string clipName)
    {
        Instance.PlayClip(Instance.ambience[0], clipName);
    }
    public static void PlayMusic(string clipName)
    {
        Instance.PlayClip(Instance.music[0], clipName);
    }
    /// <param name="groupIndex"> 0 = Enemy Sounds, 1 = Tower Sounds</param>
    public static void PlaySoundEffect(string clipName, int groupIndex, float duration = 0)
    {
        if (duration == 0) Instance.PlayClip(Instance.soundEffects[groupIndex], clipName);
        else Instance.PlayClip(Instance.soundEffects[groupIndex], clipName, duration);
    }

    private void PlayClip(AudioSourceGroup sourceGroup, string clipName)
    {
        if (sourceGroup.TryGetClip(clipName, out AudioSource audioSource))
        {
            audioSource.Play();
        }
    }
    private void PlayClip(AudioSourceGroup sourceGroup, string clipName, float duration)
    {
        if (sourceGroup.TryGetClip(clipName, out AudioSource audioSource))
        {
            audioSource.Play();
            activeClips.Add(new(audioSource, duration));
        }
    }
}