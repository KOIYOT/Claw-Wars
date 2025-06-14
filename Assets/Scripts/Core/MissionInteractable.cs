using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionInteractable : MonoBehaviour
{
    public float InteractionRadius_ = 2f;
    public float InteractionTextFadeDuration_ = 0.25f;
    public string InteractionTextString_ = "Interact";
    public string MissionSceneName_ = "Scene";
    public TextMeshProUGUI InteractTextMeshPro_;

    [Header("Scene Fade Settings")]
    public Image SceneFadeOverlayImage_;
    public float SceneFadeDuration_ = 1.0f;

    private bool CanInteract_ = false;
    private Coroutine FadeCoroutine_;
    private Coroutine SceneFadeCoroutine_;
    private Transform PlayerTransform_;
    private AsyncOperation AsyncLoadOperation_;

    private PlayerInput PlayerInput_;
    private InputAction InteractAction_;

    void Start()
    {
        GameObject PlayerObject_ = GameObject.FindGameObjectWithTag("Player");
        if (PlayerObject_ != null)
        {
            PlayerTransform_ = PlayerObject_.transform;
            PlayerInput_ = PlayerObject_.GetComponent<PlayerInput>();
            if (PlayerInput_ != null)
            {
                InteractAction_ = PlayerInput_.currentActionMap.FindAction("Interact");
                if (InteractAction_ == null)
                    Debug.LogWarning("Action 'Interact' not found in PlayerInput's current action map.");
            }
            else
            {
                Debug.LogWarning("PlayerInput component not found on GameObject with tag 'Player'.");
            }
        }
        else
        {
            Debug.LogWarning("GameObject with tag 'Player' not found.");
        }

        if (InteractTextMeshPro_ != null)
        {
            InteractTextMeshPro_.text = InteractionTextString_;
            SetTextAlpha_(InteractTextMeshPro_, 0f);
            InteractTextMeshPro_.gameObject.SetActive(false);
        }

        if (SceneFadeOverlayImage_ != null)
        {
            SetImageAlpha_(SceneFadeOverlayImage_, 0f);
            SceneFadeOverlayImage_.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (PlayerTransform_ == null) return;

        if (CanInteract_)
        {
            if (InteractAction_ != null && InteractAction_.WasPressedThisFrame())
            {
                ForceTriggerInteract();
            }
        }
    }

    public void ForceTriggerInteract()
    {
        if (SceneFadeCoroutine_ == null && AsyncLoadOperation_ == null)
        {
            if (SceneFadeOverlayImage_ != null && !string.IsNullOrEmpty(MissionSceneName_))
            {
                SceneFadeCoroutine_ = StartCoroutine(FadeOutAndLoadScene_(MissionSceneName_, SceneFadeDuration_));
            }
        }
    }

    void OnTriggerEnter(Collider Other_)
    {
        if (Other_.CompareTag("Player"))
        {
            CanInteract_ = true;

            if (InteractTextMeshPro_ != null)
            {
                if (FadeCoroutine_ != null)
                    StopCoroutine(FadeCoroutine_);

                InteractTextMeshPro_.gameObject.SetActive(true);
                FadeCoroutine_ = StartCoroutine(FadeText_(1f, InteractionTextFadeDuration_));
            }
        }
    }

    void OnTriggerExit(Collider Other_)
    {
        if (Other_.CompareTag("Player"))
        {
            CanInteract_ = false;

            if (InteractTextMeshPro_ != null)
            {
                if (FadeCoroutine_ != null)
                    StopCoroutine(FadeCoroutine_);

                FadeCoroutine_ = StartCoroutine(FadeText_(0f, InteractionTextFadeDuration_));
            }

            if (SceneFadeCoroutine_ != null)
            {
                StopCoroutine(SceneFadeCoroutine_);
                SceneFadeCoroutine_ = null;
                if (SceneFadeOverlayImage_ != null)
                {
                    SetImageAlpha_(SceneFadeOverlayImage_, 0f);
                    SceneFadeOverlayImage_.gameObject.SetActive(false);
                }
                AsyncLoadOperation_ = null;
            }
        }
    }

    IEnumerator FadeText_(float TargetAlpha_, float Duration_)
    {
        if (InteractTextMeshPro_ == null) yield break;

        float StartAlpha_ = InteractTextMeshPro_.alpha;
        float Time_ = 0f;

        while (Time_ < Duration_)
        {
            Time_ += Time.deltaTime;
            float NormalizedTime_ = Mathf.Clamp01(Time_ / Duration_);
            float NewAlpha_ = Mathf.Lerp(StartAlpha_, TargetAlpha_, NormalizedTime_);
            SetTextAlpha_(InteractTextMeshPro_, NewAlpha_);
            yield return null;
        }

        SetTextAlpha_(InteractTextMeshPro_, TargetAlpha_);
        if (TargetAlpha_ == 0f)
            InteractTextMeshPro_.gameObject.SetActive(false);

        FadeCoroutine_ = null;
    }

    void SetTextAlpha_(TextMeshProUGUI TmpText_, float Alpha_)
    {
        if (TmpText_ != null)
        {
            Color Color_ = TmpText_.color;
            Color_.a = Alpha_;
            TmpText_.color = Color_;
        }
    }

    IEnumerator FadeOutAndLoadScene_(string SceneName_, float Duration_)
    {
        if (SceneFadeOverlayImage_ == null || string.IsNullOrEmpty(SceneName_))
            yield break;

        SceneFadeOverlayImage_.gameObject.SetActive(true);

        float StartAlpha_ = SceneFadeOverlayImage_.color.a;
        float TargetAlpha_ = 1f;
        float Time_ = 0f;

        while (Time_ < Duration_)
        {
            Time_ += Time.deltaTime;
            float NormalizedTime_ = Mathf.Clamp01(Time_ / Duration_);
            float NewAlpha_ = Mathf.Lerp(StartAlpha_, TargetAlpha_, NormalizedTime_);
            SetImageAlpha_(SceneFadeOverlayImage_, NewAlpha_);
            yield return null;
        }

        SetImageAlpha_(SceneFadeOverlayImage_, TargetAlpha_);

        int lastSlot = PlayerPrefs.GetInt("LastUsedSlot", -1);
        if (lastSlot != -1 && SaveSystem.HasSaveData(lastSlot))
        {
            SaveData current = SaveSystem.LoadFromSlot(lastSlot);
            current.completedMissions += 1;
            current.lastSaveDate = System.DateTime.Now.ToString();
            SaveSystem.SaveToSlot(lastSlot, current);
        }

        AsyncLoadOperation_ = SceneManager.LoadSceneAsync(SceneName_);
        while (!AsyncLoadOperation_.isDone)
            yield return null;

        SceneFadeCoroutine_ = null;
        AsyncLoadOperation_ = null;
    }

    void SetImageAlpha_(Image Img_, float Alpha_)
    {
        if (Img_ != null)
        {
            Color Color_ = Img_.color;
            Color_.a = Alpha_;
            Img_.color = Color_;
        }
    }
}
