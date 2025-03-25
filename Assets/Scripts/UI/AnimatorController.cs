using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    public string SceneToLoad_ = "MainMenu";
    
    public Renderer SplashRenderer_;
    private Material SplashMaterial_;
    private VisualElement FadeElement_;
    private AsyncOperation AsyncLoad_;
    
    private const float FadeDuration_ = 1.5f;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        FadeElement_ = root.Q<VisualElement>("fade");
        var title = root.Q<VisualElement>("title");
        
        SplashMaterial_ = SplashRenderer_.material;

        title.style.opacity = 0f;
        FadeElement_.style.opacity = 1f;
        title.transform.scale = new Vector3(0.8f, 0.8f, 1f);

        StartCoroutine(AnimateTitle(title));
        StartCoroutine(Fade(1f, 0f, FadeDuration_, () =>
        {
            StartCoroutine(WaitAndFadeOut());
        }));
        
        AsyncLoad_ = SceneManager.LoadSceneAsync(SceneToLoad_);
        AsyncLoad_.allowSceneActivation = false;
        
        StartCoroutine(AnimateShaderByLoading());
    }

    IEnumerator WaitAndFadeOut()
    {
        while (AsyncLoad_.progress < 0.9f)
            yield return null;
        
        yield return new WaitForSeconds(0.5f);

        yield return Fade(0f, 1f, FadeDuration_, () =>
        {
            AsyncLoad_.allowSceneActivation = true;
        });
    }

    IEnumerator Fade(float from, float to, float duration, System.Action onComplete)
    {
        float time = 0f;
        
        while (time < duration)
        {
            float t = time / duration;
            float value = Mathf.SmoothStep(from, to, t);
            FadeElement_.style.opacity = value;
            time += Time.deltaTime;
            yield return null;
        }

        FadeElement_.style.opacity = to;
        onComplete?.Invoke();
    }
    
    IEnumerator AnimateTitle(VisualElement title)
    {
        float duration = 2f;
        float time = 0f;
        Vector3 startScale = new Vector3(0.95f, 0.95f, 1f);
        Vector3 endScale = Vector3.one;

        while (time < duration)
        {
            float t = time / duration;
            float easedT = Mathf.SmoothStep(0f, 1f, t);
        
            title.style.opacity = easedT;
            Vector3 scale = Vector3.Lerp(startScale, endScale, easedT);
            title.transform.scale = scale;

            time += Time.deltaTime;
            yield return null;
        }

        title.style.opacity = 1f;
        title.transform.scale = Vector3.one;
    }
    IEnumerator AnimateShaderByLoading()
    {
        while (!AsyncLoad_.isDone)
        {
            float progress = Mathf.Clamp01(AsyncLoad_.progress / 0.9f); // 0-1

            float reveal = Mathf.Lerp(0f, 0.5f, progress);
            float glow = Mathf.Lerp(0.5f, 0.85f, progress);

            SplashMaterial_.SetFloat("_RevealProgress", reveal);
            SplashMaterial_.SetFloat("_GlowIntensity", glow);

            yield return null;
        }
    }
}