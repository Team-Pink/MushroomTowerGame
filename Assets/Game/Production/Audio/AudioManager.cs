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

        Debug.Log(audioSourceDictionary.Count);
        foreach (KeyValuePair<string, AudioSource> element in audioSourceDictionary)
        {
            Debug.Log(element.Key + ": " + element.Value);
        }
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
    // Global Accessibility
    public static AudioManager main
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
            string error = "Multiple instances of AudioManager exist. " +
                "This will cause fatal errors. The program will now end.";

            Debug.LogError(error, main);
            Debug.LogError(error, this);
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
            #else
            UnityEngine.Diagnostics.Utils.NativeAssert(error);
            UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.FatalError);
            #endif
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

    public void PlayAmbience(string clipName)
    {
        PlayClip(ambience[0], clipName);
    }
    public void PlayMusic(string clipName)
    {
        PlayClip(music[0], clipName);
    }
    /// <param name="groupIndex"> 0 = Enemy Sounds, 1 = Tower Sounds</param>
    public void PlaySoundEffect(string clipName, int groupIndex, float duration = 0)
    {
        if (duration == 0) PlayClip(soundEffects[groupIndex], clipName);
        else PlayClip(soundEffects[groupIndex], clipName, duration);
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