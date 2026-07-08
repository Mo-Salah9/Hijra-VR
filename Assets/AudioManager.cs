using System.Collections.Generic;
using UnityEngine;
using static AudioManager;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {

        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;
        [HideInInspector] public AudioSource source;
    }

    public List<Sound> sounds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }
    }

    public void Play(string name)
    {
        Sound sound = sounds.Find(s => s.name == name);
        if (sound != null)
        {
            sound.source.Play();
        }
        else
        {
            Debug.LogWarning("Sound not found: " + name);
        }
    }


    public void Stop(string name)
    {
        Sound sound = sounds.Find(s => s.name == name);
        if (sound != null)
        {
            sound.source.Stop();
        }
        else
        {
            Debug.LogWarning("Sound not found: " + name);
        }
    }
    public void PlayOrStopClip(string clipName, bool value)
    {
        if (value)
        {
            Play(clipName);

        }                                                   //true to play , false to stop
        else
        {
            Stop(clipName);
        }


    }

   public void PlayAlone(string clipName)
    {
        foreach (Sound sound in sounds)
        {
            if (sound.name == "Wrong" || sound.name == "Correct")
                continue;
            sound.source.Stop();
        }

        Play(clipName);
    }
}