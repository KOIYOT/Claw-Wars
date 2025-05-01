using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";
    private const string UI_KEY = "UIVolume";

    void Start()
    {
        // Cargar valores al iniciar
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        uiSlider.onValueChanged.AddListener(SetUIVolume);

        LoadVolumes();
    }

    private void SetMusicVolume(float value)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
    }

    private void SetSFXVolume(float value)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(SFX_KEY, value);
    }

    private void SetUIVolume(float value)
    {
        mixer.SetFloat("UIVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(UI_KEY, value);
    }

    private void LoadVolumes()
    {
        float music = PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);
        float ui = PlayerPrefs.GetFloat(UI_KEY, 0.75f);

        musicSlider.value = music;
        sfxSlider.value = sfx;
        uiSlider.value = ui;

        SetMusicVolume(music);
        SetSFXVolume(sfx);
        SetUIVolume(ui);
    }
}