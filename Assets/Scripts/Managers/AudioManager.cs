using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

public class AudioManagerException : System.Exception
{
    public AudioManagerException(string message) : base(message)
    {

    }
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager SingeltonAudioManager;

    public static void setAudioManager(AudioManager audio_manager)
    {
        if(AudioManager.SingeltonAudioManager != null)
        {
            throw new AudioManagerException("trying to redefine AudioManager singleton");
        }
        AudioManager.SingeltonAudioManager = audio_manager;
    }


    [Header("Sounds")]
    [SerializeField]
    private List<Sound> sounds;
    [SerializeField]
    private Sound DeathSound;
    private int sound_counter = 0;
    public bool is_gameover = false;

    void Awake()
    {
        AudioManager.setAudioManager(this);
        foreach(Sound s in this.sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
            DeathSound.source = gameObject.AddComponent<AudioSource>();
            DeathSound.source.clip = DeathSound.clip;
            DeathSound.source.volume = DeathSound.volume;
            DeathSound.source.pitch = DeathSound.pitch;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.sounds[sound_counter].source.Play();
    }

    public void PlayDeathMusic()
    {
        this.sounds[sound_counter].source.Pause();
        this.DeathSound.source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if(is_gameover && !this.DeathSound.source.isPlaying)
        {
            this.PlayDeathMusic();
        }
        else if(!this.sounds[sound_counter].source.isPlaying && !this.DeathSound.source.isPlaying)
        {
            this.sound_counter = (sound_counter+1) >= this.sounds.Count ? 0 : sound_counter + 1;
            this.sounds[sound_counter].source.Play();
        }

    }
}
