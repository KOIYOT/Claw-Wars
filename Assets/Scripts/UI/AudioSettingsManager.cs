
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";
    private const string UI_KEY = "UIVolume";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Opcional

            Canvas.ForceUpdateCanvases(); // Asegurar Sliders inicializados

            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            uiSlider.onValueChanged.AddListener(SetUIVolume);

            InitializeAudioSettings(); // Inicializar y cargar
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSettings()
    {
        // Valores predeterminados
        float defaultMusicVolume = 0.75f;
        float defaultSFXVolume = 0.75f;
        float defaultUIVolume = 0.75f;

        // Cargar valores guardados o usar predeterminados
        float music = PlayerPrefs.GetFloat(MUSIC_KEY, defaultMusicVolume);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, defaultSFXVolume);
        float ui = PlayerPrefs.GetFloat(UI_KEY, defaultUIVolume);

        // Actualizar Sliders
        musicSlider.value = music;
        sfxSlider.value = sfx;
        uiSlider.value = ui;

        // Aplicar volúmenes al AudioMixer
        SetMusicVolume(music);
        SetSFXVolume(sfx);
        SetUIVolume(ui);

        Debug.Log("AudioSettings: Initialized with music=" + music + ", sfx=" + sfx + ", ui=" + ui);
    }

    private void SetMusicVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f); // Clampar el valor
        float dbValue = Mathf.Log10(value) * 20;
        mixer.SetFloat("MusicVolume", dbValue);
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
        PlayerPrefs.Save(); // Guardar inmediatamente
        Debug.Log("SetMusicVolume: slider value = " + value + ", dB value = " + dbValue);
    }

    private void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        float dbValue = Mathf.Log10(value) * 20;
        mixer.SetFloat("SFXVolume", dbValue);
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
        Debug.Log("SetSFXVolume: slider value = " + value + ", dB value = " + dbValue);
    }

    private void SetUIVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        float dbValue = Mathf.Log10(value) * 20;
        mixer.SetFloat("UIVolume", dbValue);
        PlayerPrefs.SetFloat(UI_KEY, value);
        PlayerPrefs.Save();
        Debug.Log("SetUIVolume: slider value = " + value + ", dB value = " + dbValue);
    }
}