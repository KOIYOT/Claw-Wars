using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class CustomMenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISubmitHandler
{
    private bool hasPlayedSelectionSound = false;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip selectionSound;
    [SerializeField] private AudioClip clickSound;

    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 2.05f;

    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image underline;

    public Color defaultTextColor = Color.black;
    public Color selectedTextColor = Color.black;
    public Color pressedTextColor = Color.red;
    public Color underlineSelectedColor = Color.red;

    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private float textFadeDuration = 0.08f;

    private bool isHovered = false;
    private bool isSelected = false;
    private bool isPressed = false;

    private Coroutine underlineFadeCoroutine;
    private Coroutine textFadeCoroutine;

    void Start()
    {
        ResetVisual();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        isPressed = true;
        UpdateVisual();
        PlayClickSound();

        StartCoroutine(ResetPressedState());
    }
    private IEnumerator ResetPressedState()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        isPressed = false;
        UpdateVisual();
    }
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        isHovered = false;
        UpdateVisual();
        PlaySelectionSound();
        hasPlayedSelectionSound = true;
    }
    

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        isHovered = false;
        UpdateVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        EventSystem.current.SetSelectedGameObject(gameObject);
        UpdateVisual();
        hasPlayedSelectionSound = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisual();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateVisual();
        PlayClickSound();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (isPressed)
        {
            FadeTextColor(pressedTextColor);
            FadeUnderline(underlineSelectedColor, 1f);
        }
        else if (isSelected || isHovered)
        {
            FadeTextColor(selectedTextColor);
            FadeUnderline(underlineSelectedColor, 1f);
        }
        else
        {
            FadeTextColor(defaultTextColor);
            FadeUnderline(underlineSelectedColor, 0f);
        }
    }

    private void FadeTextColor(Color targetColor)
    {
        if (textFadeCoroutine != null)
            StopCoroutine(textFadeCoroutine);
        textFadeCoroutine = StartCoroutine(AnimateTextColor(targetColor));
    }

    private IEnumerator AnimateTextColor(Color targetColor)
    {
        Color startColor = label.color;
        float elapsed = 0f;

        while (elapsed < textFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / textFadeDuration);
            label.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        label.color = targetColor;
    }

    private void FadeUnderline(Color baseColor, float targetAlpha)
    {
        if (underlineFadeCoroutine != null)
            StopCoroutine(underlineFadeCoroutine);
        underlineFadeCoroutine = StartCoroutine(AnimateUnderlineAlpha(baseColor, targetAlpha));
    }

    private IEnumerator AnimateUnderlineAlpha(Color baseColor, float targetAlpha)
    {
        underline.enabled = true;
        float startAlpha = underline.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            underline.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }

        underline.color = new Color(baseColor.r, baseColor.g, baseColor.b, targetAlpha);
        if (targetAlpha == 0f)
            underline.enabled = false;
    }

    public void ResetVisual()
    {
        isHovered = false;
        isSelected = false;
        isPressed = false;
        label.color = defaultTextColor;
        FadeUnderline(underlineSelectedColor, 0f);
    }

    public void ForceVisualUpdate()
    {
        isSelected = true;
        isHovered = false;
        isPressed = false;
        UpdateVisual();
    }
    
    private void PlaySelectionSound()
    {
        if (audioSource != null && selectionSound != null)
        {
            if (randomizePitch)
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            else
                audioSource.pitch = 1f;

            audioSource.PlayOneShot(selectionSound);
        }
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            if (randomizePitch)
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            else
                audioSource.pitch = 1f;

            audioSource.PlayOneShot(clickSound);
        }
    }
}