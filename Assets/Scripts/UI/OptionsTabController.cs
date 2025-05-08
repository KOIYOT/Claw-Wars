using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine;
using TMPro;

public class OptionsTabController : MonoBehaviour
{
    [Header("Pestañas")]
    [SerializeField] private List<GameObject> tabPanels;
    [SerializeField] private List<TextMeshProUGUI> tabLabels; 
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.red;

    [Header("Selección inicial")]
    [SerializeField] private GameObject firstTabButton;

    private int currentTabIndex = 0;
    private bool insideCategory = false;

    private PlayerInput playerInput;

    void Start()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
        ShowTabs();
    }
    
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
        {
            if (insideCategory)
            {
                ExitCategory();
            }
            else
            {
                FindObjectOfType<MainMenuController>().ReturnToMainMenu();
            }
        }
    }

    void OnEnable()
    {
        if (playerInput != null)
            playerInput.actions["Cancel"].performed += OnCancelPressed;
    }

    void OnDisable()
    {
        if (playerInput != null)
            playerInput.actions["Cancel"].performed += OnCancelPressed;
    }

    private void OnCancelPressed(InputAction.CallbackContext context)
    {
        if (insideCategory)
        {
            ExitCategory();
            FindObjectOfType<MainMenuController>().ReturnToMainMenu();
        }
    }
    
    public void EnterCategory(int index)
    {
        if (index < 0 || index >= tabPanels.Count)
            return;

        insideCategory = true;
        EventSystem.current.sendNavigationEvents = false;

        StartCoroutine(SmoothSwitchCategory(index));
    }

    public void ExitCategory()
    {
        insideCategory = false;
        EventSystem.current.sendNavigationEvents = true;
    }

    private void ShowTabs()
    {
        for (int i = 0; i < tabPanels.Count; i++)
        {
            tabPanels[i].SetActive(false);

            if (tabLabels != null && i < tabLabels.Count)
                tabLabels[i].color = normalColor;
        }

        StartCoroutine(SelectFirstTabNextFrame());
    }

    private IEnumerator SelectFirstTabNextFrame()
    {
        yield return null;

        if (firstTabButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstTabButton);

            CustomMenuButton cmb = firstTabButton.GetComponent<CustomMenuButton>();
            if (cmb != null)
            {
                cmb.ForceVisualUpdate();
            }
        }
    }

    private IEnumerator SmoothSwitchCategory(int newIndex)
    {
        GameObject currentPanel = null;
        if (currentTabIndex >= 0 && currentTabIndex < tabPanels.Count)
            currentPanel = tabPanels[currentTabIndex];

        GameObject newPanel = tabPanels[newIndex];

        CanvasGroup currentGroup = currentPanel?.GetComponent<CanvasGroup>();
        CanvasGroup newGroup = newPanel.GetComponent<CanvasGroup>();

        float fadeDuration = 0.25f;

        if (currentGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                t = Mathf.SmoothStep(0f, 1f, t);
                currentGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            currentGroup.alpha = 0f;
            currentPanel.SetActive(false);
        }
        
        newPanel.SetActive(true);
        if (newGroup != null)
        {
            newGroup.alpha = 0f;
        }

        if (newGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                t = Mathf.SmoothStep(0f, 1f, t);
                newGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            newGroup.alpha = 1f;
        }

        for (int i = 0; i < tabLabels.Count; i++)
        {
            if (tabLabels != null && i < tabLabels.Count)
                tabLabels[i].color = (i == newIndex) ? selectedColor : normalColor;
        }

        currentTabIndex = newIndex;
    }
}