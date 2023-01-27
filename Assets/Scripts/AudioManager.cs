using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Sound
{
    public string name;
    
    public AudioClip clip;
    public AudioMixerGroup MixerGroup; 

    [Range(0f,1f)]
    public float volume;
    [Range(.1f,3f)]
    public float pitch;

    public bool loop;
    public bool playOnAwake;
    
    [HideInInspector]
    public AudioSource source;
}


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer Mixer;

    [Tooltip("For Settings Panel")] public AudioSetting[] AudioSettings;
    private enum AudioGroups {Global, Music, SFX, Environment }
    
    [Tooltip("In-game Audio")] public Sound[] sounds;

    
    private AudioSource _clip;
    private void Awake() {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = s.MixerGroup;  
            
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            
        }
    }

    private void Start()
    {
        for (int i = 0; i < AudioSettings.Length; i++)
        {
            AudioSettings[i].Initialize();
        }
    }

    public Sound GetSound(string name)
    {
        return Array.Find(sounds, sound => sound.name == name);
    }

    public void PlayFromObject(string name, GameObject audioObject)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null)
        {
            s.source = audioObject.AddComponent<AudioSource>();
            s.source.Play();
        }
    }

    public AudioSource PlayWithReturn(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null) {
            s.source.Play();

            return s.source;
        }

        return null;
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null)
        {
            s.source.Play();
        }
    }

    public void GlobalSlider(float value)
    {
        AudioSettings[(int)AudioGroups.Global].SetExposedParam(value); 
    }
    
    public void MusicSlider(float value)
    {
        AudioSettings[(int)AudioGroups.Music].SetExposedParam(value); 
    }
    
    public void SFXSlider(float value)
    {
        AudioSettings[(int)AudioGroups.SFX].SetExposedParam(value); 
    }
    
    public void EnvSlider(float value)
    {
        AudioSettings[(int)AudioGroups.Environment].SetExposedParam(value); 
    }

    public void GlobalToggle()
    {
        AudioSettings[(int)AudioGroups.Global].ToggleAudio();
    }
    
    public void MusicToggle()
    {
        AudioSettings[(int)AudioGroups.Music].ToggleAudio();
    }
    
    public void SFXToggle()
    {
        AudioSettings[(int)AudioGroups.SFX].ToggleAudio();
    }
    
    public void EnvToggle()
    {
        AudioSettings[(int)AudioGroups.Environment].ToggleAudio();
    }
}

[System.Serializable]
public class AudioSetting
{
    [CanBeNull] public Slider slider;
    public string exposedParam;

    [SerializeField][Range(-80f, 0f)] private float audioVolume; 
    public Toggle soundToggle;

    public void Initialize()
    {
        audioVolume = PlayerPrefs.GetFloat(exposedParam);
        if (slider != null) slider.value = audioVolume;
        
    }

    void OnValidate()
    {
        SetExposedParam(audioVolume);
    }
    

    public void ToggleAudio()
    {
        if (soundToggle.isOn == true)
        {
            slider.interactable = true; 
            slider.value = PlayerPrefs.GetFloat(exposedParam);
            SetExposedParamNoWrite(slider.value);
        }
        
        if (soundToggle.isOn == false)
        {
            slider.interactable = false; 
            SetExposedParamNoWrite(-80f);
        }
    }
    
    public void SetExposedParam(float value)
    {
        AudioManager.Instance.Mixer.SetFloat(exposedParam, value);
        PlayerPrefs.SetFloat(exposedParam, value);

        if (value <= slider.minValue) soundToggle.isOn = false; 
    }
    
    public void SetExposedParamNoWrite(float value)
    {
        AudioManager.Instance.Mixer.SetFloat(exposedParam, value);
    }
}
