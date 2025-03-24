using UnityEngine.UIElements;
using System.Collections;
using UnityEngine;

public class SplashAnimatorController : MonoBehaviour
{
    private VisualElement fadeElement;
    private const float FadeDuration_ = 1.5f;
    private const float WaitTime_ = 3.0f;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        fadeElement = root.Q<VisualElement>("fade");

        fadeElement.style.opacity = 1f;

        StartCoroutine(Fade(1f, 0f, FadeDuration_, () =>
        {
            StartCoroutine(WaitAndFadeOut());
        }));
    }

    IEnumerator WaitAndFadeOut()
    {
        yield return new WaitForSeconds(WaitTime_);

        StartCoroutine(Fade(0f, 1f, FadeDuration_, () =>
        { }));
    }

    IEnumerator Fade(float from, float to, float duration, System.Action onComplete)
    {
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            float value = Mathf.Lerp(from, to, t);
            fadeElement.style.opacity = value;
            time += Time.deltaTime;
            yield return null;
        }

        fadeElement.style.opacity = to;
        onComplete?.Invoke();
    }
}