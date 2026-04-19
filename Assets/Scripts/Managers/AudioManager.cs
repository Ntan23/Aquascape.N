using UnityEngine;
using System;

[Serializable]
public class Audio
{
    public string name;
    public AudioClip clip;
    [Range(0.0f, 1.0f)]
    public float volume;
    [Range(0.1f, 3.0f)]
    public float pitch;
    public bool loop;
    [HideInInspector] public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance {get; private set;}
    void Awake()
    {
        if(instance == null) 
        {
            instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
    
    [SerializeField] private Audio[] audios;

    private void Play(string name)
    {
        Audio s = System.Array.Find(audios, sound => sound.name == name);

        if(s == null) return;

        s.source.Play();
    }

    private void Stop(string name)
    { 
        Audio s = System.Array.Find(audios, sound => sound.name == name);

        if(s == null) return;

        s.source.Stop();
    }


    void Start() 
    {
        foreach (Audio s in audios)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        Play("BGM");
    } 

    public void PlaySFX(string name) => Play(name);
    public void StopSFX(string name) => Stop(name);
} 

