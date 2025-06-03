using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class BackToMainMenu : MonoBehaviour
{
    public Image FadeOverlay_;
    public float FadeDuration_ = 1f;
    public string MenuSceneName_ = "MainMenu";

    public void ReturnToMainMenu()
    {
        if (FadeOverlay_ != null)
        {
            StartCoroutine(FadeAndLoad_());
        }
        else
        {
            SceneManager.LoadScene(MenuSceneName_);
        }
    }

    IEnumerator FadeAndLoad_()
    {
        FadeOverlay_.gameObject.SetActive(true);

        float startAlpha = FadeOverlay_.color.a;
        float time = 0f;

        while (time < FadeDuration_)
        {
            time += Time.deltaTime;
            float normalized = Mathf.Clamp01(time / FadeDuration_);
            float alpha = Mathf.Lerp(startAlpha, 1f, normalized);

            Color c = FadeOverlay_.color;
            c.a = alpha;
            FadeOverlay_.color = c;

            yield return null;
        }

        SceneManager.LoadScene(MenuSceneName_);
    }
}