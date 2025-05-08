using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] private Image FadeOverlay_;
    private float FadeDuration_ = 1.0f;

    void Start()
    {
        if (FadeOverlay_ != null)
        {
            FadeOverlay_.color = new Color(FadeOverlay_.color.r, FadeOverlay_.color.g, FadeOverlay_.color.b, 1f);
            StartCoroutine(FadeIn());
        }
    }

    private IEnumerator FadeIn()
    {
        if (FadeOverlay_ == null) yield break;
        Color Color_ = FadeOverlay_.color;
        
        float Time_ = 0f;
        while (Time_ < FadeDuration_)
        {
            Time_ += Time.deltaTime;
            FadeOverlay_.color = new Color(Color_.r, Color_.g, Color_.b, Mathf.Lerp(1f, 0f, Time_ / FadeDuration_));
            yield return null;
        }
        FadeOverlay_.color = new Color(Color_.r, Color_.g, Color_.b, 0f);
    }
}