using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class TutorialController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup tutorialPanelGroup;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private string[] tutorialSteps;
    [SerializeField] private float fadeDuration = 0.4f;

    private int currentStep = 0;
    private bool isShowing = false;
    private PlayerInput playerInput;
    private InputAction continueAction;

    private void Awake()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
        continueAction = playerInput.actions["Submit"];
    }

    public void StartTutorial()
    {
        if (isShowing) return;

        isShowing = true;
        currentStep = 0;
        tutorialText.text = "";
        gameObject.SetActive(true);

        if (continueAction != null)
            continueAction.performed += OnContinue;

        if (MouseLockCenter.Instance != null)
            MouseLockCenter.Instance.UnlockMouse();

        StartCoroutine(FadeInPanel());
        ShowCurrentStep();
    }

    private void OnContinue(InputAction.CallbackContext context)
    {
        if (!isShowing) return;

        currentStep++;
        if (currentStep < tutorialSteps.Length)
        {
            ShowCurrentStep();
        }
        else
        {
            EndTutorial();
        }
    }

    private void ShowCurrentStep()
    {
        tutorialText.text = tutorialSteps[currentStep];
    }

    private void EndTutorial()
    {
        if (continueAction != null)
            continueAction.performed -= OnContinue;

        StartCoroutine(FadeOutPanel(() =>
        {
            isShowing = false;
            gameObject.SetActive(false);

            Time.timeScale = 1f;

            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("Move");

            var pauseMenu = FindObjectOfType<PauseMenuController>();
            if (pauseMenu != null)
            {
                pauseMenu.ResetPauseState();
            }
        }));
    }


    private IEnumerator FadeInPanel()
    {
        tutorialPanelGroup.alpha = 0f;
        tutorialPanelGroup.gameObject.SetActive(true);
        tutorialPanelGroup.interactable = false;
        tutorialPanelGroup.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            tutorialPanelGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        tutorialPanelGroup.alpha = 1f;
        tutorialPanelGroup.interactable = true;
        tutorialPanelGroup.blocksRaycasts = true;
    }

    private IEnumerator FadeOutPanel(System.Action onComplete)
    {
        tutorialPanelGroup.interactable = false;
        tutorialPanelGroup.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            tutorialPanelGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        tutorialPanelGroup.alpha = 0f;
        tutorialPanelGroup.gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}
