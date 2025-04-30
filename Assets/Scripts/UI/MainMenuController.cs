using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private CanvasGroup menuButtonsGroup;
    [SerializeField] private RectTransform letterboxRect;
    [SerializeField] private GameObject uiOptionsMenu;
    [SerializeField] private GameObject uiPlayMenu;

    [Header("Fade de escena")]
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Transición Options")]
    [SerializeField] private Vector2 letterboxTargetAnchoredPos;
    [SerializeField] private float optionsTransitionDuration = 0.5f;

    [Header("Selección inicial")]
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private GameObject firstPlayMenuButton;


    private GameObject lastValidSelection_;
    private Vector2 letterboxStartAnchoredPos;

    void Start()
    {
        if (letterboxRect != null)
            letterboxStartAnchoredPos = letterboxRect.anchoredPosition;

        if (uiOptionsMenu != null)
            uiOptionsMenu.SetActive(false);

        StartCoroutine(SetInitialSelectionDelayed());
        StartCoroutine(FadeIn());
    }

    void Update()
    {
        TrackAndRestoreSelection();
        if (uiPlayMenu.activeInHierarchy &&
            (Keyboard.current.escapeKey.wasPressedThisFrame ||
             (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)))
        {
            ReturnToMainMenuFromPlay();
        }
    }

    private IEnumerator SetInitialSelectionDelayed()
    {
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        lastValidSelection_ = firstSelectedButton;
    }

    private void TrackAndRestoreSelection()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current != null)
            lastValidSelection_ = current;
        else
            EventSystem.current.SetSelectedGameObject(lastValidSelection_);
    }

    private IEnumerator FadeIn()
    {
        if (fadeOverlay == null) yield break;

        Color c = fadeOverlay.color;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeOverlay.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, t / fadeDuration));
            yield return null;
        }
        fadeOverlay.color = new Color(c.r, c.g, c.b, 0f);
    }

    private IEnumerator FadeOut()
    {
        if (fadeOverlay == null) yield break;

        Color c = fadeOverlay.color;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeOverlay.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, t / fadeDuration));
            yield return null;
        }
        fadeOverlay.color = new Color(c.r, c.g, c.b, 1f);
    }

    public void ExitGame()
    {
        StartCoroutine(FadeOutAndExit());
    }

    private IEnumerator FadeOutAndExit()
    {
        yield return StartCoroutine(FadeOut());
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void StartGame()
    {
        StartCoroutine(FadeOutAndLoadScene("Game"));
    }

    public IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
    }
    
    public void OpenPlayScreen()
    {
        StartCoroutine(OpenPlayCoroutine());
    }

    private IEnumerator OpenPlayCoroutine()
    {
        EventSystem.current.sendNavigationEvents = false;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstPlayMenuButton);
        
        SaveSlotGroupController group = uiPlayMenu.GetComponent<SaveSlotGroupController>();
        if (group != null)
        {
            group.SendMessage("RefreshAll"); 
        }


        float elapsed = 0f;
        float duration = optionsTransitionDuration;
        Vector2 startPos = letterboxRect.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            if (titleGroup != null) titleGroup.alpha = Mathf.Lerp(1f, 0f, t);
            if (menuButtonsGroup != null) menuButtonsGroup.alpha = Mathf.Lerp(1f, 0f, t);
            if (letterboxRect != null)
                letterboxRect.anchoredPosition = Vector2.Lerp(startPos, letterboxTargetAnchoredPos, t);

            yield return null;
        }

        if (titleGroup != null) titleGroup.alpha = 0f;
        if (menuButtonsGroup != null) menuButtonsGroup.alpha = 0f;
        if (letterboxRect != null) letterboxRect.anchoredPosition = letterboxTargetAnchoredPos;

        if (uiPlayMenu != null)
        {
            uiPlayMenu.SetActive(true);
            CanvasGroup cg = uiPlayMenu.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;

                float fadeElapsed = 0f;
                float fadeDuration = optionsTransitionDuration;

                while (fadeElapsed < fadeDuration)
                {
                    fadeElapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(fadeElapsed / fadeDuration);
                    t = Mathf.SmoothStep(0f, 1f, t);

                    cg.alpha = Mathf.Lerp(0f, 1f, t);
                    yield return null;
                }

                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

        EventSystem.current.sendNavigationEvents = true;
    }

    public void OpenOptionsScreen()
    {
        StartCoroutine(OpenOptionsCoroutine());
    }

    private IEnumerator OpenOptionsCoroutine()
    {
        EventSystem.current.sendNavigationEvents = false;

        float elapsed = 0f;
        float duration = optionsTransitionDuration;
        Vector2 startPos = letterboxRect.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            if (titleGroup != null) titleGroup.alpha = Mathf.Lerp(1f, 0f, t);
            if (menuButtonsGroup != null) menuButtonsGroup.alpha = Mathf.Lerp(1f, 0f, t);
            if (letterboxRect != null)
                letterboxRect.anchoredPosition = Vector2.Lerp(startPos, letterboxTargetAnchoredPos, t);

            yield return null;
        }

        if (titleGroup != null) titleGroup.alpha = 0f;
        if (menuButtonsGroup != null) menuButtonsGroup.alpha = 0f;
        if (letterboxRect != null) letterboxRect.anchoredPosition = letterboxTargetAnchoredPos;

        if (uiOptionsMenu != null)
            uiOptionsMenu.SetActive(true);

        EventSystem.current.sendNavigationEvents = true;
    }
    
    public void ReturnToMainMenuFromPlay()
    {
        StartCoroutine(ReturnFromPlayCoroutine());
    }

    private IEnumerator ReturnFromPlayCoroutine()
    {
        EventSystem.current.sendNavigationEvents = false;

        // Fade out del PlayMenu
        CanvasGroup cg = uiPlayMenu?.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            float elapsed = 0f;
            float duration = optionsTransitionDuration;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = Mathf.SmoothStep(0f, 1f, t);
                cg.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }

            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        if (uiPlayMenu != null)
            uiPlayMenu.SetActive(false);

        // Bajar el letterbox
        Vector2 startPos = letterboxRect.anchoredPosition;
        Vector2 endPos = letterboxStartAnchoredPos;

        float elapsedLetterbox = 0f;
        float durationLetterbox = optionsTransitionDuration;

        while (elapsedLetterbox < durationLetterbox)
        {
            elapsedLetterbox += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedLetterbox / durationLetterbox);
            t = Mathf.SmoothStep(0f, 1f, t);

            if (letterboxRect != null)
                letterboxRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        if (letterboxRect != null)
            letterboxRect.anchoredPosition = endPos;

        // Fade in del título y botones
        if (titleGroup != null && menuButtonsGroup != null)
        {
            float elapsed = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = Mathf.SmoothStep(0f, 1f, t);

                titleGroup.alpha = Mathf.Lerp(0f, 1f, t);
                menuButtonsGroup.alpha = Mathf.Lerp(0f, 1f, t);

                yield return null;
            }

            titleGroup.alpha = 1f;
            menuButtonsGroup.alpha = 1f;
        }

        menuButtonsGroup.interactable = true;
        menuButtonsGroup.blocksRaycasts = true;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        EventSystem.current.sendNavigationEvents = true;
    }

    public void ReturnToMainMenu()
    {
        StartCoroutine(ReturnToMainMenuCoroutine());
    }
    
    private IEnumerator ReturnToMainMenuCoroutine()
    {
        EventSystem.current.sendNavigationEvents = false;

        // Fade out de UIOptionsMenu
        CanvasGroup optionsGroup = uiOptionsMenu?.GetComponent<CanvasGroup>();
        if (optionsGroup != null)
        {
            float elapsed = 0f;
            float duration = optionsTransitionDuration;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = Mathf.SmoothStep(0f, 1f, t);

                optionsGroup.alpha = Mathf.Lerp(1f, 0f, t);

                yield return null;
            }

            optionsGroup.alpha = 0f;
            optionsGroup.blocksRaycasts = false;
            optionsGroup.interactable = false;
        }

        // 🔥 Después de hacer fade bonito, simplemente APAGAMOS
        if (uiOptionsMenu != null)
            uiOptionsMenu.SetActive(false);

        // Bajar el letterbox
        Vector2 startPos = letterboxRect.anchoredPosition;
        Vector2 endPos = letterboxStartAnchoredPos;

        float elapsedLetterbox = 0f;
        float durationLetterbox = optionsTransitionDuration;

        while (elapsedLetterbox < durationLetterbox)
        {
            elapsedLetterbox += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedLetterbox / durationLetterbox);
            t = Mathf.SmoothStep(0f, 1f, t);

            if (letterboxRect != null)
                letterboxRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        if (letterboxRect != null)
            letterboxRect.anchoredPosition = endPos;

        // Fade In del título y botones
        if (titleGroup != null) titleGroup.alpha = 0f;
        if (menuButtonsGroup != null) menuButtonsGroup.alpha = 0f;

        if (titleGroup != null && menuButtonsGroup != null)
        {
            float elapsedFadeIn = 0f;
            float durationFadeIn = 0.4f;

            while (elapsedFadeIn < durationFadeIn)
            {
                elapsedFadeIn += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedFadeIn / durationFadeIn);
                t = Mathf.SmoothStep(0f, 1f, t);

                titleGroup.alpha = Mathf.Lerp(0f, 1f, t);
                menuButtonsGroup.alpha = Mathf.Lerp(0f, 1f, t);

                yield return null;
            }

            titleGroup.alpha = 1f;
            menuButtonsGroup.alpha = 1f;
        }

        menuButtonsGroup.interactable = true;
        menuButtonsGroup.blocksRaycasts = true;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        EventSystem.current.sendNavigationEvents = true;
    }
    public IEnumerator FadeOutMusicAndLoadSceneAsync(string sceneName)
    {
        float currentVolume;
        mixer.GetFloat("MusicVolume", out currentVolume);
        currentVolume = Mathf.Pow(10f, currentVolume / 20f);

        float fadeDuration = 1.2f;
        float elapsed = 0f;

        Color originalColor = fadeOverlay.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            t = Mathf.SmoothStep(0f, 1f, t);
            
            if (fadeOverlay != null)
            {
                fadeOverlay.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(0f, 1f, t));
            }

            float volume = Mathf.Lerp(currentVolume, 0.0001f, t);
            mixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);

            yield return null;
        }

        if (fadeOverlay != null)
            fadeOverlay.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        mixer.SetFloat("MusicVolume", Mathf.Log10(0.0001f) * 20);
        
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
        {
            yield return null;
        }
    }
}