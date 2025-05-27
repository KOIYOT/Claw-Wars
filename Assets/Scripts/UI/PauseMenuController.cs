using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup pauseMenuGroup;
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private Image fadeOverlay;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Tutorial Logic")]
    [SerializeField] private GameObject tutorialController;

    private bool isPaused = false;
    private GameObject lastValidSelection_;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
    }
    private void Start()
    {
        if (playerInput != null)
        {
            playerInput.actions["Pause"].performed -= OnPausePressed; // por si ya está duplicado
            playerInput.actions["Pause"].performed += OnPausePressed;
        }
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    void Update()
    {
        TrackAndRestoreSelection();
    }

    private void TrackAndRestoreSelection()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current != null)
            lastValidSelection_ = current;
        else
            EventSystem.current.SetSelectedGameObject(lastValidSelection_);
    }

    public void PauseGame()
    {
        Debug.Log(">>> Se ejecutó PauseGame()");

        // FORZAR la reactivación total del panel, aunque ya esté activo
        pauseMenuGroup.gameObject.SetActive(false);
        pauseMenuGroup.gameObject.SetActive(true);

        pauseMenuGroup.alpha = 0f;
        pauseMenuGroup.interactable = false;
        pauseMenuGroup.blocksRaycasts = false;

        Time.timeScale = 0f;
        StartCoroutine(FadeInGroup(pauseMenuGroup));
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        isPaused = true;

        if (MouseLockCenter.Instance != null)
            MouseLockCenter.Instance.UnlockMouse();
    }


    public void ResumeGame()
    {
        Debug.Log("ResumeGame() llamado");
        StartCoroutine(FadeOutGroup(pauseMenuGroup, () => {
            Time.timeScale = 1f;
            isPaused = false;

            if (MouseLockCenter.Instance != null)
                MouseLockCenter.Instance.LockMouse();
        }));
    }

    public void OpenTutorial()
    {
        StartCoroutine(FadeOutGroup(pauseMenuGroup, () => {
            Time.timeScale = 1f;
            isPaused = false;

            if (MouseLockCenter.Instance != null)
                MouseLockCenter.Instance.UnlockMouse();

            tutorialController.GetComponent<TutorialController>().StartTutorial();
        }));
    }

    public void BackToMainMenu()
    {
        if (MouseLockCenter.Instance != null)
            MouseLockCenter.Instance.UnlockMouse();

        StartCoroutine(FadeOutAndLoadScene("MainMenu"));
    }

    private IEnumerator FadeInGroup(CanvasGroup group)
    {
        group.gameObject.SetActive(true);
        group.interactable = false;
        group.blocksRaycasts = false;
        group.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private IEnumerator FadeOutGroup(CanvasGroup group, System.Action onComplete)
    {
        group.interactable = false;
        group.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        group.alpha = 0f;
        group.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (fadeOverlay == null)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        float elapsed = 0f;
        Color original = fadeOverlay.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeOverlay.color = new Color(original.r, original.g, original.b, Mathf.Lerp(0f, 1f, elapsed / fadeDuration));
            yield return null;
        }

        fadeOverlay.color = new Color(original.r, original.g, original.b, 1f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
    public void ResetPauseState()
    {
        isPaused = false;

        // Restaurar el estado visual del panel de pausa para que pueda aparecer luego
        pauseMenuGroup.gameObject.SetActive(true);
        pauseMenuGroup.alpha = 0f;
        pauseMenuGroup.interactable = false;
        pauseMenuGroup.blocksRaycasts = false;

        if (MouseLockCenter.Instance != null)
            MouseLockCenter.Instance.LockMouse();
    }
}
